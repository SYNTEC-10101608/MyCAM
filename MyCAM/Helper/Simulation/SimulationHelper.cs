using MyCAM.App;
using MyCAM.Data;
using MyCAM.Post;
using OCC.AIS;
using OCC.BRep;
using OCC.BRepMesh;
using OCC.GC;
using OCC.GCPnts;
using OCC.Geom;
using OCC.GeomAdaptor;
using OCC.GeomAPI;
using OCC.gp;
using OCC.Poly;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopLoc;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public interface IPathSimuData
	{
		IReadOnlyList<PostPoint> TraversePoints
		{
			get;
		}
		IReadOnlyList<PostPoint> PathPoints
		{
			get;
		}
		IReadOnlyList<PostPoint> ExitPoints
		{
			get;
		}
	}

	public class PathSimuIKData : IPathSimuData
	{
		public readonly List<PostPoint> TraversePntIKList;
		public readonly List<PostPoint> PathPntIKList;
		public readonly List<PostPoint> ExitPntIKList;

		// IPathSimuData mapping
		public IReadOnlyList<PostPoint> TraversePoints => TraversePntIKList;
		public IReadOnlyList<PostPoint> PathPoints => PathPntIKList;
		public IReadOnlyList<PostPoint> ExitPoints => ExitPntIKList;

		public PathSimuIKData()
		{
			TraversePntIKList = new List<PostPoint>();
			PathPntIKList = new List<PostPoint>();
			ExitPntIKList = new List<PostPoint>();
		}

		public PathSimuIKData( List<PostPoint> traverPntList, List<PostPoint> pathPntList, List<PostPoint> exitPntList )
		{
			TraversePntIKList = traverPntList;
			PathPntIKList = pathPntList;
			ExitPntIKList = exitPntList;
		}
	}

	public class PathSimuFKData : IPathSimuData
	{
		public readonly List<PostPoint> TraversePntFKList;
		public readonly List<PostPoint> PathPntFKList;
		public readonly List<PostPoint> ExitPntFKList;

		// IPathSimuData mapping
		public IReadOnlyList<PostPoint> TraversePoints => TraversePntFKList;
		public IReadOnlyList<PostPoint> PathPoints => PathPntFKList;
		public IReadOnlyList<PostPoint> ExitPoints => ExitPntFKList;

		public PathSimuFKData()
		{
			TraversePntFKList = new List<PostPoint>();
			PathPntFKList = new List<PostPoint>();
			ExitPntFKList = new List<PostPoint>();
		}

		public PathSimuFKData( List<PostPoint> traverPntList, List<PostPoint> pathPntList, List<PostPoint> exitPntList )
		{
			TraversePntFKList = traverPntList;
			PathPntFKList = pathPntList;
			ExitPntFKList = exitPntList;
		}
	}

	public static class SimulationHelper
	{
		internal static bool BuildFrameTransMap( SimuData.RequiredData.SimuInputSet calNeedData, out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap, out List<SimuData.ResultData.PathStartEndIndex> pathStartEndIndexList, out int frameCount )
		{
			frameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
			pathStartEndIndexList = new List<SimuData.ResultData.PathStartEndIndex>();
			frameCount = 0;

			// build full path FK point list (each path is connected)
			bool bGetWholePathIK = GetWholePathIKPnt( calNeedData.EachPathIKPostDataList, calNeedData.LastPathLastPnt, calNeedData.EntryAndExitData, calNeedData.PostSolver, out List<PathSimuIKData> pathIKPntList );
			if( !bGetWholePathIK ) {
				return false;
			}
			bool bSolveFKDone = BuildPathFKList( pathIKPntList, calNeedData.PostSolver, out List<PathSimuFKData> pathFKPntList );
			if( !bSolveFKDone ) {
				return false;
			}

			pathStartEndIndexList = GetPathStartEndIndex( pathFKPntList );

			// build frame transform map
			bool bFrameOk = BuildFrame( pathFKPntList, calNeedData.PostSolver, calNeedData.WorkPiecesChaintSet, calNeedData.MachineData, calNeedData.ChainListMap, out frameTransformMap, out frameCount );
			return bFrameOk;
		}

		internal static bool BuildFKPostPnt( PostSolver PostSolver, PostPoint IKpostPoint, out PostPoint FKPoint )
		{
			FKPoint = null;
			if( PostSolver == null || IKpostPoint == null ) {
				return false;
			}
			PostPoint temPostPnt = IKpostPoint.Clone();
			gp_Pnt pointG54 = new gp_Pnt( temPostPnt.X, temPostPnt.Y, temPostPnt.Z );
			gp_Vec tcpOffset = PostSolver.SolveFK( temPostPnt.Master, temPostPnt.Slave, pointG54 );
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

		const double DISCRETE_MAX_DEFLECTION = 0.01;
		const double DISCRETE_MAX_EDGE_LENGTH = 1.0;
		const double TOLERANCE = 1e-3;

		static List<SimuData.ResultData.PathStartEndIndex> GetPathStartEndIndex( IEnumerable<IPathSimuData> pathFKPntList )
		{
			int nCurrentIndex = 0;
			List<SimuData.ResultData.PathStartEndIndex> pathIndexList = new List<SimuData.ResultData.PathStartEndIndex>();
			foreach( IPathSimuData pathData in pathFKPntList ) {
				int tempStartIndex = nCurrentIndex;
				if( pathData.TraversePoints != null && pathData.TraversePoints.Count > 0 ) {
					if( nCurrentIndex == 0 ) {
						nCurrentIndex += pathData.TraversePoints.Count - 1;
					}
					else {
						nCurrentIndex += pathData.TraversePoints.Count;
					}
				}
				if( pathData.PathPoints != null && pathData.PathPoints.Count > 0 ) {

					if( nCurrentIndex == 0 ) {
						nCurrentIndex += pathData.PathPoints.Count - 1;
					}
					else {
						nCurrentIndex += pathData.PathPoints.Count;
					}
				}
				if( pathData.ExitPoints != null && pathData.ExitPoints.Count > 0 ) {
					nCurrentIndex += pathData.ExitPoints.Count;
				}
				SimuData.ResultData.PathStartEndIndex pathStartEndIndex = new SimuData.ResultData.PathStartEndIndex( tempStartIndex, nCurrentIndex );
				pathIndexList.Add( pathStartEndIndex );
				nCurrentIndex++;
			}
			return pathIndexList;
		}

		#region Solve Path FK Point List

		static bool BuildPathFKList( List<PathSimuIKData> pathIKPntList, PostSolver PostSolver, out List<PathSimuFKData> pathSimuPntList )
		{
			pathSimuPntList = new List<PathSimuFKData>();
			if( pathIKPntList == null || pathIKPntList.Count == 0 || PostSolver == null ) {
				return false;
			}
			PostPoint prePathLastPnt = new PostPoint();
			foreach( PathSimuIKData pathIKList in pathIKPntList ) {
				List<PostPoint> traverseFKPntList = ConvertIKPntToFKPnt( pathIKList.TraversePntIKList, PostSolver );
				List<PostPoint> processPathFKPntList = ConvertIKPntToFKPnt( pathIKList.PathPntIKList, PostSolver );
				List<PostPoint> exitFKPntList = ConvertIKPntToFKPnt( pathIKList.ExitPntIKList, PostSolver );
				pathSimuPntList.Add( new PathSimuFKData( traverseFKPntList, processPathFKPntList, exitFKPntList ) );
			}
			return true;
		}

		static bool GetWholePathIKPnt( List<PostData> PostDataList, IProcessPoint lastPathLastPnt, EntryAndExitData entryAndExitData, PostSolver PostSolver, out List<PathSimuIKData> pathSimuPntList )
		{
			pathSimuPntList = new List<PathSimuIKData>();
			if( PostDataList == null || PostDataList.Count == 0 || lastPathLastPnt == null || entryAndExitData == null || PostSolver == null ) {
				return false;
			}
			PostPoint prePathLastPnt = new PostPoint();
			foreach( PostData currentPostData in PostDataList ) {
				List<PostPoint> traverseIKPntList = new List<PostPoint>();

				// traverse path
				if( currentPostData == PostDataList.First() ) {
					traverseIKPntList = GetTraversIKPnt( currentPostData, PostSolver, true, null );
				}
				else {
					traverseIKPntList = GetTraversIKPnt( currentPostData, PostSolver, false, prePathLastPnt );
				}

				// main path
				List<PostPoint> processPathIKPntList = GetPathIKPnt( currentPostData, PostSolver );
				prePathLastPnt = GetLastProcess( currentPostData );

				// exit path
				PathSimuIKData pathSimuIKPnt = new PathSimuIKData();
				List<PostPoint> exitIKPntList = new List<PostPoint>();
				if( currentPostData == PostDataList.Last() ) {
					exitIKPntList = GetExitPathIKPnt( lastPathLastPnt, entryAndExitData, prePathLastPnt, PostSolver );
					pathSimuIKPnt = new PathSimuIKData( traverseIKPntList, processPathIKPntList, exitIKPntList );
				}
				else {
					pathSimuIKPnt = new PathSimuIKData( traverseIKPntList, processPathIKPntList, new List<PostPoint>() );
				}
				pathSimuPntList.Add( pathSimuIKPnt );
			}
			return true;
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

		static List<PostPoint> GetExitPathIKPnt( IProcessPoint lastPathLastPnt, EntryAndExitData entryAndExitData, PostPoint pathEndPoint, PostSolver postSolver )
		{
			// calculate exit point
			IProcessPoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( lastPathLastPnt, entryAndExitData.ExitDistance );
			PostPoint exitPostPnt = new PostPoint()
			{
				X = exitPoint.Point.X(),
				Y = exitPoint.Point.Y(),
				Z = exitPoint.Point.Z(),
				Master = pathEndPoint.Master,
				Slave = pathEndPoint.Slave
			};
			bool bSuccess = BuildLinearTraverseIKPnt( pathEndPoint, exitPostPnt, out List<PostPoint> IKPntList );
			if( bSuccess == false ) {
				return new List<PostPoint>();
			}
			return IKPntList;
		}

		static List<PostPoint> GetPathIKPnt( PostData postData, PostSolver postSolver )
		{
			List<PostPoint> pathIKPntList = new List<PostPoint>();
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

		#endregion

		#region Build Frame Transform Map

		static bool BuildFrame( List<PathSimuFKData> pathFKPntList, PostSolver postSolver, HashSet<MachineComponentType> workPiecesChaintSet, MachineData machineData, Dictionary<MachineComponentType, List<MachineComponentType>> chainListMap, out Dictionary<MachineComponentType, List<gp_Trsf>> m_FrameTransformMap, out int frameCount )
		{
			frameCount = 0;
			m_FrameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
			if( pathFKPntList == null || postSolver == null || machineData == null || chainListMap == null || workPiecesChaintSet == null ) {
				return false;
			}

			// init frame transform map
			m_FrameTransformMap[ MachineComponentType.XAxis ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.YAxis ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.ZAxis ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.Master ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.Slave ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.Laser ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.WorkPiece ] = new List<gp_Trsf>();

			gp_Vec G54Offset = postSolver.G54Offset;

			// build frame by frame (with guard against invalid machine data)
			if( machineData.PtOnMaster == null || machineData.PtOnSlave == null ) {
				MyApp.Logger.ShowOnLogPanel( "缺少旋轉中心點的定義", MyApp.NoticeType.Warning );
				return false;
			}
			if( machineData.MasterRotateDir == null || machineData.SlaveRotateDir == null ) {
				MyApp.Logger.ShowOnLogPanel( "缺少軸向旋轉方向", MyApp.NoticeType.Warning );
				return false;
			}

			// each path
			foreach( var simuFKData in pathFKPntList ) {

				// each point
				if( simuFKData.TraversePntFKList != null && simuFKData.TraversePntFKList.Count > 0 ) {
					foreach( var postpoint in simuFKData.TraversePntFKList ) {
						FKToFrameTranfResult( postpoint, G54Offset, workPiecesChaintSet,
						machineData, chainListMap, ref m_FrameTransformMap );
						frameCount++;
					}
				}
				if( simuFKData.PathPntFKList != null && simuFKData.PathPntFKList.Count > 0 ) {
					foreach( var postpoint in simuFKData.PathPntFKList ) {
						FKToFrameTranfResult( postpoint, G54Offset, workPiecesChaintSet,
						machineData, chainListMap, ref m_FrameTransformMap );
						frameCount++;
					}
				}
				if( simuFKData.ExitPntFKList != null && simuFKData.ExitPntFKList.Count > 0 ) {
					foreach( var postpoint in simuFKData.ExitPntFKList ) {
						FKToFrameTranfResult( postpoint, G54Offset, workPiecesChaintSet,
						machineData, chainListMap, ref m_FrameTransformMap );
						frameCount++;
					}
				}
			}
			return true;
		}

		public static void FKToFrameTranfResult( PostPoint FKPnt, gp_Vec G54Offset, HashSet<MachineComponentType> workPiecesChaintSet,
			MachineData machineData, Dictionary<MachineComponentType, List<MachineComponentType>> chainListMap,
			ref Dictionary<MachineComponentType, List<gp_Trsf>> m_FrameTransformMap )
		{
			try {
				// set XYZ transform
				Dictionary<MachineComponentType, gp_Trsf> transformMap = new Dictionary<MachineComponentType, gp_Trsf>();
				transformMap = BuildTransformMapPerFrame( FKPnt, G54Offset, workPiecesChaintSet, machineData );

				// set chain
				gp_Trsf trsfX = GetComponentTrsf( transformMap, MachineComponentType.XAxis, chainListMap );
				gp_Trsf trsfY = GetComponentTrsf( transformMap, MachineComponentType.YAxis, chainListMap );
				gp_Trsf trsfZ = GetComponentTrsf( transformMap, MachineComponentType.ZAxis, chainListMap );
				gp_Trsf trsfMaster = GetComponentTrsf( transformMap, MachineComponentType.Master, chainListMap );
				gp_Trsf trsfSlave = GetComponentTrsf( transformMap, MachineComponentType.Slave, chainListMap );
				gp_Trsf trsLaser = GetComponentTrsf( transformMap, MachineComponentType.Laser, chainListMap );
				gp_Trsf trsfAllWorkPiece = GetComponentTrsf( transformMap, MachineComponentType.WorkPiece, chainListMap );
				m_FrameTransformMap[ MachineComponentType.XAxis ].Add( trsfX );
				m_FrameTransformMap[ MachineComponentType.YAxis ].Add( trsfY );
				m_FrameTransformMap[ MachineComponentType.ZAxis ].Add( trsfZ );
				m_FrameTransformMap[ MachineComponentType.Master ].Add( trsfMaster );
				m_FrameTransformMap[ MachineComponentType.Slave ].Add( trsfSlave );
				m_FrameTransformMap[ MachineComponentType.Laser ].Add( trsLaser );
				m_FrameTransformMap[ MachineComponentType.WorkPiece ].Add( trsfAllWorkPiece );
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"特定畫面計算失敗: {ex.Message}", MyApp.NoticeType.Warning );
			}
		}

		static Dictionary<MachineComponentType, gp_Trsf> BuildTransformMapPerFrame(
		   PostPoint thisFramePostPnt,
		   gp_Vec g54,
		   HashSet<MachineComponentType> workPiecesChainSet,
		   MachineData machineData )
		{
			// record each component transform at this frame
			Dictionary<MachineComponentType, gp_Trsf> componentTrsfMap = new Dictionary<MachineComponentType, gp_Trsf>();

			// XYZ axis
			componentTrsfMap[ MachineComponentType.XAxis ] = CreateAxisTranslation( thisFramePostPnt.X + g54.X(), 0, 0, workPiecesChainSet.Contains( MachineComponentType.XAxis ) );
			componentTrsfMap[ MachineComponentType.YAxis ] = CreateAxisTranslation( 0, thisFramePostPnt.Y + g54.Y(), 0, workPiecesChainSet.Contains( MachineComponentType.YAxis ) );
			componentTrsfMap[ MachineComponentType.ZAxis ] = CreateAxisTranslation( 0, 0, thisFramePostPnt.Z + g54.Z(), workPiecesChainSet.Contains( MachineComponentType.ZAxis ) );

			// rotational axis
			componentTrsfMap[ MachineComponentType.Master ] = CreateAxisRotation( machineData.PtOnMaster, machineData.MasterRotateDir, thisFramePostPnt.Master, workPiecesChainSet.Contains( MachineComponentType.Master ) );
			componentTrsfMap[ MachineComponentType.Slave ] = CreateAxisRotation( machineData.PtOnSlave, machineData.SlaveRotateDir, thisFramePostPnt.Slave, workPiecesChainSet.Contains( MachineComponentType.Slave ) );

			// laser / workpiece
			componentTrsfMap[ MachineComponentType.Laser ] = new gp_Trsf();
			gp_Trsf trsfWorkPiece = new gp_Trsf();
			trsfWorkPiece.SetTranslation( g54 );
			componentTrsfMap[ MachineComponentType.WorkPiece ] = trsfWorkPiece;
			return componentTrsfMap;
		}

		static gp_Trsf CreateAxisTranslation( double x, double y, double z, bool bNeedInvert )
		{
			gp_Trsf transform = new gp_Trsf();
			transform.SetTranslation( new gp_Vec( x, y, z ) );
			if( bNeedInvert ) {
				transform.Invert();
			}
			return transform;
		}

		static gp_Trsf CreateAxisRotation( gp_Pnt RotateCenterPnt, gp_Dir dir, double dAngle, bool bNeedInvert )
		{
			gp_Trsf transform = new gp_Trsf();
			gp_Ax1 rotateAxis = new gp_Ax1( RotateCenterPnt, dir );
			transform.SetRotation( rotateAxis, dAngle );
			if( bNeedInvert ) {
				transform.Invert();
			}
			return transform;
		}

		static gp_Trsf GetComponentTrsf( Dictionary<MachineComponentType, gp_Trsf> transformMap, MachineComponentType type, Dictionary<MachineComponentType, List<MachineComponentType>> ChainListMap )
		{
			gp_Trsf trsf = new gp_Trsf();
			try {
				if( transformMap == null || ChainListMap == null ) {
					return trsf;
				}
				if( !ChainListMap.ContainsKey( type ) ) {
					MyApp.Logger.ShowOnLogPanel( $"ChainListMap 缺少機構: {type}", MyApp.NoticeType.Warning );
					return trsf;
				}
				foreach( MachineComponentType parent in ChainListMap[ type ] ) {

					// base is not transformed
					if( parent == MachineComponentType.Base ) {
						continue;
					}
					if( transformMap.ContainsKey( parent ) ) {
						trsf.Multiply( transformMap[ parent ] );
					}
				}
				if( transformMap.ContainsKey( type ) ) {
					trsf.Multiply( transformMap[ type ] );
				}
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"GetComponentTrsf 計算失敗: {ex.Message}", MyApp.NoticeType.Warning );
			}
			return trsf;
		}

		#endregion

		#region Build Traverse IK Point List

		//  frog leap + cut down
		static List<PostPoint> MovementAndCutDownIKPnt( PostPoint traversStartPnt, PostData pathPostData )
		{
			List<PostPoint> traverseIKPoints = new List<PostPoint>();
			if( traversStartPnt == null || pathPostData == null ) {
			}
			bool bSuccess = true;

			// frog leap
			if( pathPostData.FrogLeapMidPostPoint != null ) {

				// frog leap ->  cut down -> start
				if( pathPostData.CutDownPostPoint != null ) {
					bSuccess = BuildForgLeapTraverseIKPnt( traversStartPnt, pathPostData.FrogLeapMidPostPoint, pathPostData.CutDownPostPoint, out List<PostPoint> frogLeapIKList );
					if( bSuccess ) {
						traverseIKPoints.AddRange( frogLeapIKList );
					}
					bSuccess = BuildLinearTraverseIKPnt( pathPostData.CutDownPostPoint, pathPostData.ProcessStartPoint, out List<PostPoint> cutDownIKLIst );
					if( bSuccess ) {
						traverseIKPoints.AddRange( cutDownIKLIst );
					}
					return traverseIKPoints;
				}

				// frog leap directly to start
				bSuccess = BuildForgLeapTraverseIKPnt( traversStartPnt, pathPostData.FrogLeapMidPostPoint, pathPostData.ProcessStartPoint, out List<PostPoint> justFrogLeapIKList );
				if( bSuccess ) {
					traverseIKPoints.AddRange( justFrogLeapIKList );
				}
				return traverseIKPoints;
			}

			// move to plane -> move on plane -> move to start
			if( pathPostData.LiftUpPostSafePlanePoint != null && pathPostData.CutDownPostSafePlanePoint != null ) {
				bSuccess = BuildLinearTraverseIKPnt( traversStartPnt, pathPostData.LiftUpPostSafePlanePoint, out List<PostPoint> moveToPlaneIKList );
				if( bSuccess ) {
					traverseIKPoints.AddRange( moveToPlaneIKList );
				}
				bSuccess = BuildLinearTraverseIKPnt( pathPostData.LiftUpPostSafePlanePoint, pathPostData.CutDownPostSafePlanePoint, out List<PostPoint> moveOnPlaneIKList );
				if( bSuccess ) {
					traverseIKPoints.AddRange( moveOnPlaneIKList );
				}

				if( pathPostData.CutDownPostPoint != null ) {
					bSuccess = BuildLinearTraverseIKPnt( pathPostData.CutDownPostSafePlanePoint, pathPostData.CutDownPostPoint, out List<PostPoint> moveToCutDownIKList );
					if( bSuccess ) {
						traverseIKPoints.AddRange( moveToCutDownIKList );
					}

					bSuccess = BuildLinearTraverseIKPnt( pathPostData.CutDownPostPoint, pathPostData.ProcessStartPoint, out List<PostPoint> moveToStartIKList );
					if( bSuccess ) {
						traverseIKPoints.AddRange( moveToStartIKList );
					}
				}
				else {
					bSuccess = BuildLinearTraverseIKPnt( pathPostData.CutDownPostSafePlanePoint, pathPostData.ProcessStartPoint, out List<PostPoint> moveToCutDownIKList );
					if( bSuccess ) {
						traverseIKPoints.AddRange( moveToCutDownIKList );
					}
				}
				return traverseIKPoints;
			}

			//  move directly + cut down
			if( pathPostData.CutDownPostPoint != null ) {
				bSuccess = BuildLinearTraverseIKPnt( traversStartPnt, pathPostData.CutDownPostPoint, out List<PostPoint> moveDirectlyIKList );
				if( bSuccess ) {
					traverseIKPoints.AddRange( moveDirectlyIKList );
				}
				bSuccess = BuildLinearTraverseIKPnt( pathPostData.CutDownPostPoint, pathPostData.ProcessStartPoint, out List<PostPoint> cutDownIKList );
				if( bSuccess ) {
					traverseIKPoints.AddRange( cutDownIKList );
				}
				return traverseIKPoints;
			}

			// straight move to start
			bSuccess = BuildLinearTraverseIKPnt( traversStartPnt, pathPostData.ProcessStartPoint, out List<PostPoint> moveDirectlyToStartIKList );
			if( bSuccess ) {
				traverseIKPoints.AddRange( moveDirectlyToStartIKList );
			}
			return traverseIKPoints;
		}

		static bool BuildLinearTraverseIKPnt( PostPoint startPostPnt, PostPoint endPostPnt, out List<PostPoint> IKpostPointList )
		{
			IKpostPointList = new List<PostPoint>();
			if( startPostPnt == null || endPostPnt == null ) {
				return false;
			}

			List<gp_Pnt> DiscretizePntList = DiscretizeLine( new gp_Pnt( startPostPnt.X, startPostPnt.Y, startPostPnt.Z ), new gp_Pnt( endPostPnt.X, endPostPnt.Y, endPostPnt.Z ) );
			if( DiscretizePntList == null || DiscretizePntList.Count == 0 ) {
				return false;
			}
			double dMasterStep = ( endPostPnt.Master - startPostPnt.Master ) / DiscretizePntList.Count();
			double dSlaveStep = ( endPostPnt.Slave - startPostPnt.Slave ) / DiscretizePntList.Count();
			for( int i = 0; i < DiscretizePntList.Count; i++ ) {
				PostPoint pnt = new PostPoint();
				pnt.X = DiscretizePntList[ i ].X();
				pnt.Y = DiscretizePntList[ i ].Y();
				pnt.Z = DiscretizePntList[ i ].Z();

				// interpolate master and slave values
				pnt.Master = startPostPnt.Master + dMasterStep * i;
				pnt.Slave = startPostPnt.Slave + dSlaveStep * i;
				IKpostPointList.Add( pnt );
			}
			return true;
		}

		static bool BuildForgLeapTraverseIKPnt( PostPoint startPostPnt, PostPoint MidPostPoint, PostPoint endPostPnt, out List<PostPoint> FrogLeapIKPostPnt )
		{
			FrogLeapIKPostPnt = new List<PostPoint>();
			if( startPostPnt == null || MidPostPoint == null || endPostPnt == null ) {
				return false;
			}
			List<List<gp_Pnt>> arcPntList = DiscretizeArcSegments( new gp_Pnt( startPostPnt.X, startPostPnt.Y, startPostPnt.Z ), new gp_Pnt( MidPostPoint.X, MidPostPoint.Y, MidPostPoint.Z ), new gp_Pnt( endPostPnt.X, endPostPnt.Y, endPostPnt.Z ) );
			bool bSuccess = AddInterpolatedPoints( arcPntList[ 0 ], startPostPnt.Master, startPostPnt.Slave, MidPostPoint.Master, MidPostPoint.Slave, out List<PostPoint> FrogLeapFirstHalfList );
			if( !bSuccess ) {
				return false;
			}
			FrogLeapIKPostPnt.AddRange( FrogLeapFirstHalfList );

			// remove duplicated mid point
			FrogLeapIKPostPnt.RemoveAt( FrogLeapIKPostPnt.Count() - 1 );
			bSuccess = AddInterpolatedPoints( arcPntList[ 1 ], MidPostPoint.Master, MidPostPoint.Slave, endPostPnt.Master, endPostPnt.Slave, out List<PostPoint> FrogLeapSecondHalfList );
			if( !bSuccess ) {
				return false;
			}
			FrogLeapIKPostPnt.AddRange( FrogLeapSecondHalfList );
			return true;
		}

		static bool AddInterpolatedPoints( List<gp_Pnt> arcPoints, double startMaster, double startSlave, double endMaster, double endSlave, out List<PostPoint> interpolatedPoints )
		{
			interpolatedPoints = new List<PostPoint>();
			if( arcPoints == null || arcPoints.Count == 0 ) {
				return false;
			}
			double dMasterStep = ( endMaster - startMaster ) / arcPoints.Count;
			double dSlaveStep = ( endSlave - startSlave ) / arcPoints.Count;

			for( int i = 0; i < arcPoints.Count; i++ ) {
				PostPoint pnt = new PostPoint();
				pnt.X = arcPoints[ i ].X();
				pnt.Y = arcPoints[ i ].Y();
				pnt.Z = arcPoints[ i ].Z();
				pnt.Master = startMaster + dMasterStep * i;
				pnt.Slave = startSlave + dSlaveStep * i;
				interpolatedPoints.Add( pnt );
			}
			return true;
		}

		static List<gp_Pnt> DiscretizeLine( gp_Pnt startPnt, gp_Pnt endPnt, double dStep = DISCRETE_MAX_EDGE_LENGTH )
		{
			if( startPnt == null || endPnt == null || dStep <= 0 || startPnt.IsEqual( endPnt, TOLERANCE ) ) {
				return new List<gp_Pnt>();
			}
			List<gp_Pnt> discretePntList = new List<gp_Pnt>();
			gp_Vec vectorStartToEnd = new gp_Vec( startPnt, endPnt );
			double dLength = vectorStartToEnd.Magnitude();
			int nSegmentCount = (int)( dLength / dStep );
			vectorStartToEnd.Normalize();

			// add discrete points
			for( int i = 0; i <= nSegmentCount; i++ ) {
				gp_Vec offset = new gp_Vec( vectorStartToEnd.X(), vectorStartToEnd.Y(), vectorStartToEnd.Z() );
				offset.Multiply( i * dStep );
				gp_Pnt offSetPnt = new gp_Pnt( startPnt.XYZ() + offset.XYZ() );
				discretePntList.Add( offSetPnt );
			}

			// add the end point if not added
			if( discretePntList.Count == 0 || !discretePntList[ discretePntList.Count - 1 ].IsEqual( endPnt, 1e-3 ) ) {
				discretePntList.Add( new gp_Pnt( endPnt.X(), endPnt.Y(), endPnt.Z() ) );
			}
			return discretePntList;
		}

		// out put first index is arc from start to mid, second index is arc from mid to end
		static List<List<gp_Pnt>> DiscretizeArcSegments( gp_Pnt arcStartPnt, gp_Pnt arcMidPnt, gp_Pnt arcEndPnt, double chordHeight = DISCRETE_MAX_DEFLECTION, double maxStep = DISCRETE_MAX_EDGE_LENGTH )
		{
			List<List<gp_Pnt>> arcPntList = new List<List<gp_Pnt>>();
			if( arcStartPnt == null || arcMidPnt == null || arcEndPnt == null ) {
				return arcPntList;
			}
			GC_MakeArcOfCircle arcMaker = new GC_MakeArcOfCircle( arcStartPnt, arcMidPnt, arcEndPnt );
			if( !arcMaker.IsDone() ) {
				return arcPntList;
			}
			Geom_TrimmedCurve arc = arcMaker.Value();
			double dFirstU = arc.FirstParameter();
			double dLastU = arc.LastParameter();

			bool isFound = FindParameterOfPoint( arc, arcMidPnt, out double dMidU );
			if( !isFound ) {
				return arcPntList;
			}

			// arc from a to midpnt
			List<gp_Pnt> startPntToMidPntList = DiscretizeArc( arc, dFirstU, dMidU, chordHeight, maxStep );
			arcPntList.Add( startPntToMidPntList );

			// arc from midpnt to endpnt
			List<gp_Pnt> midPntToEndPnt = DiscretizeArc( arc, dMidU, dLastU, chordHeight, maxStep );
			arcPntList.Add( midPntToEndPnt );
			return arcPntList;
		}

		static bool FindParameterOfPoint( Geom_TrimmedCurve arc, gp_Pnt targetPnt, out double dTargetU, double tol = TOLERANCE )
		{
			dTargetU = 0;
			GeomAPI_ProjectPointOnCurve projector = new GeomAPI_ProjectPointOnCurve( targetPnt, arc );
			if( projector.NbPoints() > 0 ) {
				dTargetU = projector.LowerDistanceParameter();
				return true;
			}
			else {
				return false;
			}
		}

		static List<gp_Pnt> DiscretizeArc( Geom_TrimmedCurve arc, double startParameter, double endParameter, double chordHeight, double maxStep )
		{
			const int FIRST_IDX = 1;
			const int SECOND_IDX = 2;
			List<gp_Pnt> discretizedPntList = new List<gp_Pnt>();

			// discretize with chord height
			GeomAdaptor_Curve adaptorCurve = new GeomAdaptor_Curve( arc );
			GCPnts_QuasiUniformDeflection deflection = new GCPnts_QuasiUniformDeflection( adaptorCurve, chordHeight, startParameter, endParameter );
			if( deflection.IsDone() && deflection.NbPoints() > 1 ) {
				gp_Pnt firstPnt = deflection.Value( FIRST_IDX );
				gp_Pnt secondPnt = deflection.Value( SECOND_IDX );

				// make sure the step is not too large
				if( firstPnt.Distance( secondPnt ) > maxStep ) {

					// each segment exceed max step, re-discretize by max step
					discretizedPntList.Clear();

					// calculate arc length
					double arcLength = GCPnts_AbscissaPoint.Length( adaptorCurve, startParameter, endParameter );

					// calculate segment count
					int segmentCount = (int)Math.Ceiling( arcLength / 1.0 ) < 1 ? 1 : (int)Math.Ceiling( arcLength / 1.0 );

					for( int i = 0; i <= segmentCount; i++ ) {
						double dUParam = startParameter + ( endParameter - startParameter ) * i / segmentCount;
						discretizedPntList.Add( arc.Value( dUParam ) );
					}
				}
				else {
					// chord height discretization is acceptable
					for( int i = FIRST_IDX; i <= deflection.NbPoints(); i++ ) {
						discretizedPntList.Add( deflection.Value( i ) );
					}
				}
			}
			else {
				// arc too short, just add start and end point
				discretizedPntList.Clear();
				discretizedPntList.Add( arc.Value( startParameter ) );
				discretizedPntList.Add( arc.Value( endParameter ) );
			}

			// macke sure the end point is added
			if( discretizedPntList.Count == 0 || !discretizedPntList[ discretizedPntList.Count - 1 ].IsEqual( arc.Value( endParameter ), 1e-6 ) ) {
				discretizedPntList.Add( arc.Value( endParameter ) );
			}
			return discretizedPntList;
		}

		#endregion

		#region Build Traverse FK Point List

		static List<PostPoint> GetTraversIKPnt( PostData postData, PostSolver postSolver, bool isFirstPath = false, PostPoint prePathLastPnt = null )
		{
			if( postData == null || postSolver == null ) {
				return new List<PostPoint>();
			}
			if( isFirstPath == true ) {
				bool bSuccess = BuildLinearTraverseIKPnt( postData.CutDownPostPoint, postData.ProcessStartPoint, out List<PostPoint> IKPnList );
				if( !bSuccess ) {
					return new List<PostPoint>();
				}
				return IKPnList;
			}
			bool needLift = false;

			List<PostPoint> wholeTraverseIKPntList = new List<PostPoint>();
			// lift up
			if( postData.LiftUpPostPoint != null ) {
				bool bSuccess = BuildLinearTraverseIKPnt( prePathLastPnt, postData.LiftUpPostPoint, out List<PostPoint> IKPnList );
				if( !bSuccess ) {
					return new List<PostPoint>();
				}

				wholeTraverseIKPntList.AddRange( IKPnList );
				needLift = true;
			}

			if( needLift ) {
				List<PostPoint> IKPnList = MovementAndCutDownIKPnt( postData.LiftUpPostPoint, postData );
				if( IKPnList == null || IKPnList.Count == 0 ) {
					return new List<PostPoint>();
				}
				wholeTraverseIKPntList.AddRange( IKPnList );
				return wholeTraverseIKPntList;
			}
			else {
				List<PostPoint> IKPnList = MovementAndCutDownIKPnt( prePathLastPnt, postData );
				if( IKPnList == null || IKPnList.Count == 0 ) {
					return new List<PostPoint>();
				}
				wholeTraverseIKPntList.AddRange( IKPnList );
				return wholeTraverseIKPntList;
			}
		}

		static List<PostPoint> ConvertIKPntToFKPnt( List<PostPoint> IKpostPntList, PostSolver PostSolver )
		{
			if( IKpostPntList == null || IKpostPntList.Count == 0 || PostSolver == null ) {
				return new List<PostPoint>();
			}
			List<PostPoint> FKPntList = new List<PostPoint>();
			foreach( PostPoint postPoint in IKpostPntList ) {
				bool bSuccess = BuildFKPostPnt( PostSolver, postPoint.Clone(), out PostPoint FKPoint );
				if( !bSuccess ) {
					return new List<PostPoint>();
				}
				FKPntList.Add( FKPoint );
			}
			return FKPntList;
		}

		#endregion

		#region Build Work Pieces 

		public static bool BuildMergedTriangulation( List<AIS_Shape> aisShapes, out Poly_Triangulation merged, double deflection = 0.1 )
		{
			merged = new Poly_Triangulation();
			if( aisShapes == null ) {
				return false;
			}
			if( aisShapes.Count == 0 ) {
				return false;
			}

			List<gp_Pnt> globalVertices = new List<gp_Pnt>();
			List<Poly_Triangle> globalTriangles = new List<Poly_Triangle>();
			int vertexOffset = 0;

			foreach( AIS_Shape ais in aisShapes ) {
				if( ais == null )
					continue;

				TopoDS_Shape shape = ais.Shape();
				if( shape == null )
					continue;

				// Ensure triangulation
				new BRepMesh_IncrementalMesh( shape, deflection );

				// Traverse faces
				for( TopExp_Explorer exp = new TopExp_Explorer( shape, TopAbs_ShapeEnum.TopAbs_FACE );
					 exp.More();
					 exp.Next() ) {
					TopoDS_Face face = TopoDS.ToFace( exp.Current() );

					TopLoc_Location loc = new TopLoc_Location();
					Poly_Triangulation tri = BRep_Tool.Triangulation( face, ref loc );

					if( tri == null || tri.IsNull() )
						continue;

					gp_Trsf trsf = loc.Transformation();

					// -----------------------
					// Collect vertices (world coordinates)
					// -----------------------
					int nbNodes = tri.NbNodes();
					for( int i = 1; i <= nbNodes; i++ ) {
						gp_Pnt p = tri.Node( i ).Transformed( trsf );
						globalVertices.Add( p );
					}

					// -----------------------
					// Collect triangles (with global index offset)
					// -----------------------
					int nbTriangles = tri.NbTriangles();
					for( int i = 1; i <= nbTriangles; i++ ) {
						Poly_Triangle t = tri.Triangle( i );
						int a = 0;
						int b = 0;
						int c = 0;
						t.Get( ref a, ref b, ref c );

						globalTriangles.Add( new Poly_Triangle(
							a + vertexOffset,
							b + vertexOffset,
							c + vertexOffset ) );
					}

					vertexOffset += nbNodes;
				}
			}

			if( globalVertices.Count == 0 || globalTriangles.Count == 0 )
				throw new InvalidOperationException( "No triangulation generated from AIS_Shapes." );

			// -----------------------
			// Build merged Poly_Triangulation
			// -----------------------
			merged = new Poly_Triangulation(
						globalVertices.Count,
						globalTriangles.Count,
						false // no UVs
					);

			// Set vertices
			for( int i = 0; i < globalVertices.Count; i++ ) {
				merged.SetNode( i + 1, globalVertices[ i ] ); // Node 1-based
			}

			// Set triangles
			for( int i = 0; i < globalTriangles.Count; i++ ) {
				merged.SetTriangle( i + 1, globalTriangles[ i ] ); // Triangle 1-based
			}

			// Optional: compute normals
			try {
				merged.ComputeNormals();
			}
			catch {
				// ignore if not supported
				return false;
			}

			return true;
		}

		#endregion
	}
}