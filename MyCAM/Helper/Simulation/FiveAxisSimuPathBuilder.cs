using MyCAM.Data;
using MyCAM.Post;
using OCC.gp;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	// five-axis simulation path builder; owns five-axis IK / FK and master / slave interpolation
	public class FiveAxisSimuPathBuilder : SimuPathBuilderBase
	{
		// build the full frame transform map for the whole simulation
		internal override bool BuildFrameTransMap(
			SimuData.SimuRequiredData.SimuInputSet calNeedData,
			out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap,
			out List<SimuData.ResultData.PathStartEndIndex> pathStartEndIndexList,
			out int frameCount )
		{
			frameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
			pathStartEndIndexList = new List<SimuData.ResultData.PathStartEndIndex>();
			frameCount = 0;

			// five-axis specific machine data
			FiveAxisMachineData machineData = calNeedData.MachineData as FiveAxisMachineData;
			if( machineData == null ) {
				return false;
			}

			// build full path IK point list ( each path is connected )
			bool bGetWholePathIK = GetWholePathIKPnt( calNeedData.EachPathIKPostDataList, calNeedData.LastPathLastPnt, calNeedData.EntryAndExitData, calNeedData.PostSolver, out List<PathSimuIKData> pathIKPntList );
			if( !bGetWholePathIK ) {
				return false;
			}

			// solve FK for each path
			bool bSolveFKDone = BuildPathFKList( pathIKPntList, calNeedData.PostSolver, machineData.SimulationOffset, out List<PathSimuFKData> pathFKPntList );
			if( !bSolveFKDone ) {
				return false;
			}

			pathStartEndIndexList = SimulationHelper.GetPathStartEndIndex( pathFKPntList );

			// build frame transform map
			bool bFrameOk = SimulationHelper.BuildFrame( pathFKPntList, calNeedData.PostSolver, machineData.WorkPieceChain, machineData, machineData.ChainListMap, out frameTransformMap, out frameCount );
			return bFrameOk;
		}

		// build a single point's frame transform, used by tool vector edit actions
		public override bool BuildSinglePointFrameTransform(
			IMachineDataBase machineData,
			gp_Pnt position,
			PostPointBase orientationPoint,
			out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap )
		{
			frameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();

			// five-axis specific machine data and orientation ( master / slave )
			if( !( machineData is FiveAxisMachineData fiveAxisMachineData ) || !( orientationPoint is PostPoint postPoint ) ) {
				return false;
			}

			// chain structures come from the machine data itself
			HashSet<MachineComponentType> workPieceChain = fiveAxisMachineData.WorkPieceChain;
			Dictionary<MachineComponentType, List<MachineComponentType>> machineChainListMap = fiveAxisMachineData.ChainListMap;

			// validate input
			if( position == null || workPieceChain == null || machineChainListMap == null ) {
				return false;
			}
			if( fiveAxisMachineData.MasterAxis == null || fiveAxisMachineData.SlaveAxis == null
				|| fiveAxisMachineData.MasterAxis.RotateCenter == null || fiveAxisMachineData.SlaveAxis.RotateCenter == null
				|| fiveAxisMachineData.MasterRotateDir == null || fiveAxisMachineData.SlaveRotateDir == null ) {
				return false;
			}

			// set default offset
			gp_Vec simuOffset = fiveAxisMachineData.SimulationOffset ?? new gp_Vec();

			// create IK point ( G54 coordinate )
			PostPoint G54pnt = new PostPoint()
			{
				X = position.X(),
				Y = position.Y(),
				Z = position.Z(),
				Master = postPoint.Master,
				Slave = postPoint.Slave
			};

			// create PostSolver and solve FK
			PostSolver postSolver = new PostSolver( fiveAxisMachineData );
			bool calSuccess = BuildFKPostPntFiveAxis( postSolver, G54pnt, out PostPoint FKpnt, simuOffset );
			if( !calSuccess ) {
				return false;
			}

			// flatten chain map into int[component index][parent indices] for fast lookup
			IPerFrameTransformBuilder builder = PerFrameTransformBuilderFactory.Create( fiveAxisMachineData );
			if( builder == null ) {
				return false;
			}
			int componentCount = builder.ComponentTypes.Count;
			int[][] chainIdx = SimulationHelper.BuildFlattenChainList( machineChainListMap, builder );

			// per-component reusable buffer ( single frame, so length == componentCount )
			gp_Trsf[] transformBuf = new gp_Trsf[ componentCount ];

			// single-frame lists ( capacity 1 to avoid extra growth )
			var lists = new List<gp_Trsf>[ componentCount ];
			for( int i = 0; i < componentCount; i++ ) {
				lists[ i ] = new List<gp_Trsf>( 1 );
			}

			// calculate frame transform via the flattened fast path
			SimulationHelper.FKToFrameTranfResult( FKpnt, fiveAxisMachineData.SimulationOffset, workPieceChain,
				builder, chainIdx, ref transformBuf, ref lists );

			// materialize into the output Dictionary once
			for( int i = 0; i < componentCount; i++ ) {
				frameTransformMap[ builder.ComponentTypes[ i ] ] = lists[ i ];
			}
			return true;
		}

		// build whole-path IK point list with five-axis master / slave interpolation
		protected override bool GetWholePathIKPnt(
			List<PostData> postDataList,
			IProcessPoint lastPathLastPnt,
			EntryAndExitData entryAndExitData,
			PostSolver postSolver,
			out List<PathSimuIKData> pathSimuPntList )
		{
			pathSimuPntList = new List<PathSimuIKData>();
			if( postDataList == null || postDataList.Count == 0 || lastPathLastPnt == null || entryAndExitData == null || postSolver == null ) {
				return false;
			}
			PostPoint prePathLastPnt = new PostPoint();
			foreach( PostData currentPostData in postDataList ) {
				List<PostPointBase> traverseIKPntList;

				// traverse path
				if( currentPostData == postDataList.First() ) {
					traverseIKPntList = GetTraversIKPnt( currentPostData, postSolver, true, null );
				}
				else {
					traverseIKPntList = GetTraversIKPnt( currentPostData, postSolver, false, prePathLastPnt );
				}
				if( currentPostData.SafeBoundaryPostPointList != null && traverseIKPntList.Count == 0 ) {
					return false;
				}

				// main path
				List<PostPointBase> processPathIKPntList = GetPathIKPnt( currentPostData );
				prePathLastPnt = GetLastProcess( currentPostData );

				// exit path
				PathSimuIKData pathSimuIKPnt;
				if( currentPostData == postDataList.Last() ) {
					List<PostPointBase> exitIKPntList = GetExitPathIKPnt( lastPathLastPnt, entryAndExitData, prePathLastPnt, postSolver );
					pathSimuIKPnt = new PathSimuIKData( traverseIKPntList, processPathIKPntList, exitIKPntList );
				}
				else {
					pathSimuIKPnt = new PathSimuIKData( traverseIKPntList, processPathIKPntList, new List<PostPointBase>() );
				}
				pathSimuPntList.Add( pathSimuIKPnt );
			}
			return true;
		}

		// convert whole-path IK list into FK list with five-axis FK solving
		protected override bool BuildPathFKList(
			List<PathSimuIKData> pathIKPntList,
			PostSolver postSolver,
			gp_Vec simulationOffset,
			out List<PathSimuFKData> pathSimuPntList )
		{
			pathSimuPntList = new List<PathSimuFKData>();
			if( pathIKPntList == null || pathIKPntList.Count == 0 || postSolver == null ) {
				return false;
			}
			foreach( PathSimuIKData pathIKList in pathIKPntList ) {
				List<PostPointBase> traverseFKPntList = ConvertIKPntToFKPnt( pathIKList.TraversePntIKList, postSolver, simulationOffset );
				List<PostPointBase> processPathFKPntList = ConvertIKPntToFKPnt( pathIKList.PathPntIKList, postSolver, simulationOffset );
				List<PostPointBase> exitFKPntList = ConvertIKPntToFKPnt( pathIKList.ExitPntIKList, postSolver, simulationOffset );
				pathSimuPntList.Add( new PathSimuFKData( traverseFKPntList, processPathFKPntList, exitFKPntList ) );
			}
			return true;
		}

		// convert a single IK point into an FK point with five-axis FK
		protected override bool BuildFKPostPnt(
			PostSolver postSolver,
			PostPointBase ikPostPoint,
			out PostPointBase fkPoint,
			gp_Vec simuOffset = null )
		{
			fkPoint = null;
			if( !( ikPostPoint is PostPoint postPoint ) ) {
				return false;
			}
			bool bSuccess = BuildFKPostPntFiveAxis( postSolver, postPoint, out PostPoint fkPost, simuOffset );
			fkPoint = fkPost;
			return bSuccess;
		}

		#region Five-axis IK / FK helpers

		// five-axis forward kinematics for a single point
		static bool BuildFKPostPntFiveAxis( PostSolver postSolver, PostPoint IKpostPoint, out PostPoint FKPoint, gp_Vec simuOffset = null )
		{
			FKPoint = null;
			if( postSolver == null || IKpostPoint == null ) {
				return false;
			}
			PostPoint temPostPnt = IKpostPoint.Clone();
			gp_Pnt pointG54 = new gp_Pnt( temPostPnt.X, temPostPnt.Y, temPostPnt.Z );
			gp_Vec tcpOffset = postSolver.SolveFK( temPostPnt.Master, temPostPnt.Slave, pointG54, simuOffset );
			gp_Pnt pointMCS = pointG54.Translated( tcpOffset );
			FKPoint = new PostPoint()
			{
				X = pointMCS.X(),
				Y = pointMCS.Y(),
				Z = pointMCS.Z(),
				Master = temPostPnt.Master,
				Slave = temPostPnt.Slave
			};
			return true;
		}

		// convert IK list to FK list; five-axis FK works on PostPoint
		static List<PostPointBase> ConvertIKPntToFKPnt( IReadOnlyList<PostPointBase> IKpostPntList, PostSolver postSolver, gp_Vec simuOffset = null )
		{
			if( IKpostPntList == null || IKpostPntList.Count == 0 || postSolver == null ) {
				return new List<PostPointBase>();
			}
			List<PostPointBase> FKPntList = new List<PostPointBase>();
			foreach( PostPointBase postPointBase in IKpostPntList ) {
				if( !( postPointBase is PostPoint postPoint ) ) {
					return new List<PostPointBase>();
				}
				bool bSuccess = BuildFKPostPntFiveAxis( postSolver, postPoint.Clone(), out PostPoint FKPoint, simuOffset );
				if( !bSuccess ) {
					return new List<PostPointBase>();
				}
				FKPntList.Add( FKPoint );
			}
			return FKPntList;
		}

		static PostPoint GetLastProcess( PostData postData )
		{
			if( postData.LeadOutPostPointList.Count != 0 ) {
				return postData.LeadOutPostPointList.Last().Clone();
			}
			if( postData.OverCutPostPointList.Count != 0 ) {
				return postData.OverCutPostPointList.Last().Clone();
			}
			return postData.MainPathPostPointList.Last().Clone();
		}

		protected override List<PostPointBase> GetExitPathIKPnt( IProcessPoint lastPathLastPnt, EntryAndExitData entryAndExitData, PostPointBase pathEndPoint, PostSolver postSolver )
		{
			// five-axis exit needs master / slave from the path end point
			if( !( pathEndPoint is PostPoint pathEndPost ) ) {
				return new List<PostPointBase>();
			}

			// calculate exit point
			IProcessPoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( lastPathLastPnt, entryAndExitData.ExitDistance );
			PostPoint exitPostPnt = new PostPoint()
			{
				X = exitPoint.Point.X(),
				Y = exitPoint.Point.Y(),
				Z = exitPoint.Point.Z(),
				Master = pathEndPost.Master,
				Slave = pathEndPost.Slave
			};
			bool bSuccess = BuildLinearTraverseIKPnt( pathEndPost, exitPostPnt, out List<PostPointBase> IKPntList );
			if( bSuccess == false ) {
				return new List<PostPointBase>();
			}
			return IKPntList;
		}

		protected override List<PostPointBase> GetPathIKPnt( PostData postData )
		{
			List<PostPointBase> pathIKPntList = new List<PostPointBase>();
			if( postData.LeadInPostPointList != null ) {
				pathIKPntList.AddRange( postData.LeadInPostPointList );
			}
			if( postData.MainPathPostPointList != null ) {
				pathIKPntList.AddRange( postData.MainPathPostPointList );
			}
			if( postData.OverCutPostPointList != null ) {
				pathIKPntList.AddRange( postData.OverCutPostPointList );
			}
			if( postData.LeadOutPostPointList != null ) {
				pathIKPntList.AddRange( postData.LeadOutPostPointList );
			}
			return pathIKPntList;
		}

		protected override List<PostPointBase> GetTraversIKPnt( PostData postData, PostSolver postSolver, bool isFirstPath, PostPointBase prePathLastPnt )
		{
			if( postData == null || postSolver == null ) {
				return new List<PostPointBase>();
			}
			if( isFirstPath == true ) {
				bool bSuccess = BuildLinearTraverseIKPnt( postData.CutDownPostPoint, postData.ProcessStartPoint, out List<PostPointBase> IKPnList );
				if( !bSuccess ) {
					return new List<PostPointBase>();
				}
				return IKPnList;
			}
			bool needLift = false;

			List<PostPointBase> wholeTraverseIKPntList = new List<PostPointBase>();

			// lift up
			if( postData.LiftUpPostPoint != null ) {
				bool bSuccess = BuildLinearTraverseIKPnt( prePathLastPnt, postData.LiftUpPostPoint, out List<PostPointBase> IKPnList );
				if( !bSuccess ) {
					return new List<PostPointBase>();
				}
				wholeTraverseIKPntList.AddRange( IKPnList );
				needLift = true;
			}

			if( needLift ) {
				List<PostPointBase> IKPnList = MovementAndCutDownIKPnt( postData.LiftUpPostPoint, postData );
				if( IKPnList == null || IKPnList.Count == 0 ) {
					return new List<PostPointBase>();
				}
				wholeTraverseIKPntList.AddRange( IKPnList );
				return wholeTraverseIKPntList;
			}
			else {
				List<PostPointBase> IKPnList = MovementAndCutDownIKPnt( prePathLastPnt, postData );
				if( IKPnList == null || IKPnList.Count == 0 ) {
					return new List<PostPointBase>();
				}
				wholeTraverseIKPntList.AddRange( IKPnList );
				return wholeTraverseIKPntList;
			}
		}

		#endregion

		#region Five-axis master / slave interpolation

		// frog leap + cut down
		protected override List<PostPointBase> MovementAndCutDownIKPnt( PostPointBase traversStartPnt, PostData pathPostData )
		{
			List<PostPointBase> traverseIKPoints = new List<PostPointBase>();
			bool bSuccess;

			if( pathPostData.SafeBoundaryPostPointList != null ) {
				List<PostPoint> boundaryPoints = pathPostData.SafeBoundaryPostPointList;
				if( boundaryPoints.Count < 2 ) {
					return new List<PostPointBase>();
				}
				PostPointBase segmentStart = traversStartPnt;
				for( int index = 1; index < boundaryPoints.Count; index++ ) {
					if( !BuildLinearTraverseIKPnt( segmentStart, boundaryPoints[ index ], out List<PostPointBase> boundarySegmentIKList ) ) {
						return new List<PostPointBase>();
					}
					if( traverseIKPoints.Count > 0 ) {
						boundarySegmentIKList.RemoveAt( 0 );
					}
					traverseIKPoints.AddRange( boundarySegmentIKList );
					segmentStart = boundaryPoints[ index ];
				}
				if( !BuildLinearTraverseIKPnt( segmentStart, pathPostData.ProcessStartPoint, out List<PostPointBase> boundaryCutDownIKList ) ) {
					return new List<PostPointBase>();
				}
				boundaryCutDownIKList.RemoveAt( 0 );
				traverseIKPoints.AddRange( boundaryCutDownIKList );
				return traverseIKPoints;
			}

			// frog leap
			if( pathPostData.FrogLeapMidPostPoint != null ) {

				// frog leap -> cut down -> start
				if( pathPostData.CutDownPostPoint != null ) {
					bSuccess = BuildFrogLeapTraverseIKPnt( traversStartPnt, pathPostData.FrogLeapMidPostPoint, pathPostData.CutDownPostPoint, out List<PostPointBase> frogLeapIKList );
					if( bSuccess ) {
						traverseIKPoints.AddRange( frogLeapIKList );
					}
					bSuccess = BuildLinearTraverseIKPnt( pathPostData.CutDownPostPoint, pathPostData.ProcessStartPoint, out List<PostPointBase> cutDownIKLIst );
					if( bSuccess ) {
						traverseIKPoints.AddRange( cutDownIKLIst );
					}
					return traverseIKPoints;
				}

				// frog leap directly to start
				bSuccess = BuildFrogLeapTraverseIKPnt( traversStartPnt, pathPostData.FrogLeapMidPostPoint, pathPostData.ProcessStartPoint, out List<PostPointBase> justFrogLeapIKList );
				if( bSuccess ) {
					traverseIKPoints.AddRange( justFrogLeapIKList );
				}
				return traverseIKPoints;
			}

			// move to plane -> move on plane -> move to start
			if( pathPostData.LiftUpPostSafePlanePoint != null && pathPostData.CutDownPostSafePlanePoint != null ) {
				bSuccess = BuildLinearTraverseIKPnt( traversStartPnt, pathPostData.LiftUpPostSafePlanePoint, out List<PostPointBase> moveToPlaneIKList );
				if( bSuccess ) {
					traverseIKPoints.AddRange( moveToPlaneIKList );
				}
				bSuccess = BuildLinearTraverseIKPnt( pathPostData.LiftUpPostSafePlanePoint, pathPostData.CutDownPostSafePlanePoint, out List<PostPointBase> moveOnPlaneIKList );
				if( bSuccess ) {
					traverseIKPoints.AddRange( moveOnPlaneIKList );
				}
				if( pathPostData.CutDownPostPoint != null ) {
					bSuccess = BuildLinearTraverseIKPnt( pathPostData.CutDownPostSafePlanePoint, pathPostData.CutDownPostPoint, out List<PostPointBase> moveToCutDownIKList );
					if( bSuccess ) {
						traverseIKPoints.AddRange( moveToCutDownIKList );
					}
					bSuccess = BuildLinearTraverseIKPnt( pathPostData.CutDownPostPoint, pathPostData.ProcessStartPoint, out List<PostPointBase> moveToStartIKList );
					if( bSuccess ) {
						traverseIKPoints.AddRange( moveToStartIKList );
					}
				}
				else {
					bSuccess = BuildLinearTraverseIKPnt( pathPostData.CutDownPostSafePlanePoint, pathPostData.ProcessStartPoint, out List<PostPointBase> moveToCutDownIKList );
					if( bSuccess ) {
						traverseIKPoints.AddRange( moveToCutDownIKList );
					}
				}
				return traverseIKPoints;
			}

			// move directly + cut down
			if( pathPostData.CutDownPostPoint != null ) {
				bSuccess = BuildLinearTraverseIKPnt( traversStartPnt, pathPostData.CutDownPostPoint, out List<PostPointBase> moveDirectlyIKList );
				if( bSuccess ) {
					traverseIKPoints.AddRange( moveDirectlyIKList );
				}
				bSuccess = BuildLinearTraverseIKPnt( pathPostData.CutDownPostPoint, pathPostData.ProcessStartPoint, out List<PostPointBase> cutDownIKList );
				if( bSuccess ) {
					traverseIKPoints.AddRange( cutDownIKList );
				}
				return traverseIKPoints;
			}

			// straight move to start
			bSuccess = BuildLinearTraverseIKPnt( traversStartPnt, pathPostData.ProcessStartPoint, out List<PostPointBase> moveDirectlyToStartIKList );
			if( bSuccess ) {
				traverseIKPoints.AddRange( moveDirectlyToStartIKList );
			}
			return traverseIKPoints;
		}

		// linear traverse with master / slave interpolation
		protected override bool BuildLinearTraverseIKPnt( PostPointBase startPostPnt, PostPointBase endPostPnt, out List<PostPointBase> IKpostPointList )
		{
			IKpostPointList = new List<PostPointBase>();
			if( !( startPostPnt is PostPoint startPost ) || !( endPostPnt is PostPoint endPost ) ) {
				return false;
			}

			gp_Pnt startPoint = new gp_Pnt( startPost.X, startPost.Y, startPost.Z );
			gp_Pnt endPoint = new gp_Pnt( endPost.X, endPost.Y, endPost.Z );
			List<gp_Pnt> discretePoints = SimulationHelper.DiscretizeLine( startPoint, endPoint );
			if( discretePoints == null ) {
				return false;
			}
			if( discretePoints.Count < 2 ) {
				IKpostPointList.Add( startPost.Clone() );
				IKpostPointList.Add( endPost.Clone() );
				return true;
			}
			double totalLength = startPoint.Distance( endPoint );
			for( int index = 0; index < discretePoints.Count; index++ ) {
				double ratio = startPoint.Distance( discretePoints[ index ] ) / totalLength;
				PostPoint point = new PostPoint()
				{
					X = discretePoints[ index ].X(),
					Y = discretePoints[ index ].Y(),
					Z = discretePoints[ index ].Z(),
					Master = startPost.Master + ratio * ( endPost.Master - startPost.Master ),
					Slave = startPost.Slave + ratio * ( endPost.Slave - startPost.Slave )
				};
				IKpostPointList.Add( point );
			}
			IKpostPointList[ 0 ] = startPost.Clone();
			IKpostPointList[ IKpostPointList.Count - 1 ] = endPost.Clone();
			return true;
		}

		// frog leap arc traverse with master / slave interpolation
		protected override bool BuildFrogLeapTraverseIKPnt( PostPointBase startPostPnt, PostPointBase midPostPoint, PostPointBase endPostPnt, out List<PostPointBase> frogLeapIKPostPnt )
		{
			frogLeapIKPostPnt = new List<PostPointBase>();
			if( !( startPostPnt is PostPoint startPost ) || !( midPostPoint is PostPoint midPost ) || !( endPostPnt is PostPoint endPost ) ) {
				return false;
			}
			List<List<gp_Pnt>> arcPntList = SimulationHelper.DiscretizeArcSegments( new gp_Pnt( startPost.X, startPost.Y, startPost.Z ), new gp_Pnt( midPost.X, midPost.Y, midPost.Z ), new gp_Pnt( endPost.X, endPost.Y, endPost.Z ) );
			if( arcPntList == null || arcPntList.Count < 2 || arcPntList[ 0 ] == null || arcPntList[ 1 ] == null ) {
				return false;
			}
			bool bSuccess = AddInterpolatedPoints( arcPntList[ 0 ], startPost, midPost, out List<PostPoint> FrogLeapFirstHalfList );
			if( !bSuccess ) {
				return false;
			}
			frogLeapIKPostPnt.AddRange( FrogLeapFirstHalfList );
			if( frogLeapIKPostPnt.Count() == 0 ) {
				return false;
			}

			// remove duplicated mid point
			frogLeapIKPostPnt.RemoveAt( frogLeapIKPostPnt.Count() - 1 );
			bSuccess = AddInterpolatedPoints( arcPntList[ 1 ], midPost, endPost, out List<PostPoint> FrogLeapSecondHalfList );
			if( !bSuccess ) {
				return false;
			}
			frogLeapIKPostPnt.AddRange( FrogLeapSecondHalfList );
			return true;
		}

		// interpolate master / slave over a set of arc points
		static bool AddInterpolatedPoints( List<gp_Pnt> arcPoints, PostPoint startPost, PostPoint endPost, out List<PostPoint> interpolatedPoints )
		{
			interpolatedPoints = new List<PostPoint>();
			if( arcPoints == null || arcPoints.Count == 0 ) {
				return false;
			}
			if( arcPoints.Count < 2 ) {
				interpolatedPoints.Add( startPost.Clone() );
				interpolatedPoints.Add( endPost.Clone() );
				return true;
			}
			double[] cumulativeLengths = new double[ arcPoints.Count ];
			for( int i = 1; i < arcPoints.Count; i++ ) {
				cumulativeLengths[ i ] = cumulativeLengths[ i - 1 ] + arcPoints[ i - 1 ].Distance( arcPoints[ i ] );
			}
			double totalLength = cumulativeLengths[ arcPoints.Count - 1 ];

			for( int i = 0; i < arcPoints.Count; i++ ) {
				// Preserve angular motion even when all positions coincide.
				double ratio = totalLength > 0 ? cumulativeLengths[ i ] / totalLength : (double)i / ( arcPoints.Count - 1 );
				PostPoint pnt = new PostPoint();
				pnt.X = arcPoints[ i ].X();
				pnt.Y = arcPoints[ i ].Y();
				pnt.Z = arcPoints[ i ].Z();
				pnt.Master = startPost.Master + ratio * ( endPost.Master - startPost.Master );
				pnt.Slave = startPost.Slave + ratio * ( endPost.Slave - startPost.Slave );
				interpolatedPoints.Add( pnt );
			}
			interpolatedPoints[ 0 ] = startPost.Clone();
			interpolatedPoints[ interpolatedPoints.Count - 1 ] = endPost.Clone();
			return true;
		}

		#endregion
	}
}
