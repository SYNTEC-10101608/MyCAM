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
		IReadOnlyList<PostPointBase> TraversePoints
		{
			get;
		}
		IReadOnlyList<PostPointBase> PathPoints
		{
			get;
		}
		IReadOnlyList<PostPointBase> ExitPoints
		{
			get;
		}
	}

	public class PathSimuIKData : IPathSimuData
	{
		public readonly List<PostPointBase> TraversePntIKList;
		public readonly List<PostPointBase> PathPntIKList;
		public readonly List<PostPointBase> ExitPntIKList;

		// IPathSimuData mapping
		public IReadOnlyList<PostPointBase> TraversePoints => TraversePntIKList;
		public IReadOnlyList<PostPointBase> PathPoints => PathPntIKList;
		public IReadOnlyList<PostPointBase> ExitPoints => ExitPntIKList;

		public PathSimuIKData()
		{
			TraversePntIKList = new List<PostPointBase>();
			PathPntIKList = new List<PostPointBase>();
			ExitPntIKList = new List<PostPointBase>();
		}

		public PathSimuIKData( List<PostPointBase> traverPntList, List<PostPointBase> pathPntList, List<PostPointBase> exitPntList )
		{
			TraversePntIKList = traverPntList;
			PathPntIKList = pathPntList;
			ExitPntIKList = exitPntList;
		}
	}

	public class PathSimuFKData : IPathSimuData
	{
		public readonly List<PostPointBase> TraversePntFKList;
		public readonly List<PostPointBase> PathPntFKList;
		public readonly List<PostPointBase> ExitPntFKList;

		// IPathSimuData mapping
		public IReadOnlyList<PostPointBase> TraversePoints => TraversePntFKList;
		public IReadOnlyList<PostPointBase> PathPoints => PathPntFKList;
		public IReadOnlyList<PostPointBase> ExitPoints => ExitPntFKList;

		public PathSimuFKData()
		{
			TraversePntFKList = new List<PostPointBase>();
			PathPntFKList = new List<PostPointBase>();
			ExitPntFKList = new List<PostPointBase>();
		}

		public PathSimuFKData( List<PostPointBase> traverPntList, List<PostPointBase> pathPntList, List<PostPointBase> exitPntList )
		{
			TraversePntFKList = traverPntList;
			PathPntFKList = pathPntList;
			ExitPntFKList = exitPntList;
		}
	}

	public static class SimulationHelper
	{
		// shared cached LanguageManager to avoid per-frame allocations in hot paths.
		static readonly LanguageManager.LanguageManager m_LangMgr = new LanguageManager.LanguageManager( "SimulationHelper" );

		const double DISCRETE_MAX_DEFLECTION = 0.01;
		const double DISCRETE_MAX_EDGE_LENGTH = 1.0;
		const double TOLERANCE = 1e-3;

		internal static List<SimuData.ResultData.PathStartEndIndex> GetPathStartEndIndex( IEnumerable<IPathSimuData> pathFKPntList )
		{
			int nCurrentIndex = 0;
			List<SimuData.ResultData.PathStartEndIndex> pathIndexList = new List<SimuData.ResultData.PathStartEndIndex>();
			foreach( IPathSimuData pathData in pathFKPntList ) {
				int tempStartIndex = nCurrentIndex;
				if( pathData.TraversePoints != null ) {
					nCurrentIndex += pathData.TraversePoints.Count;
				}
				if( pathData.PathPoints != null ) {
					nCurrentIndex += pathData.PathPoints.Count;
				}
				if( pathData.ExitPoints != null ) {
					nCurrentIndex += pathData.ExitPoints.Count;
				}
				SimuData.ResultData.PathStartEndIndex pathStartEndIndex = new SimuData.ResultData.PathStartEndIndex( tempStartIndex, nCurrentIndex - 1 );
				pathIndexList.Add( pathStartEndIndex );
			}
			return pathIndexList;
		}

		#region Build Frame Transform Map

		internal static bool BuildFrame( List<PathSimuFKData> pathFKPntList, PostSolver postSolver, HashSet<MachineComponentType> workPiecesChaintSet, FiveAxisMachineData machineData, Dictionary<MachineComponentType, List<MachineComponentType>> chainListMap, out Dictionary<MachineComponentType, List<gp_Trsf>> m_FrameTransformMap, out int frameCount )
		{
			frameCount = 0;
			m_FrameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
			if( pathFKPntList == null || postSolver == null || machineData == null || chainListMap == null || workPiecesChaintSet == null ) {
				return false;
			}

			// build frame by frame (with guard against invalid machine data)
			if( machineData.MasterAxis == null || machineData.SlaveAxis == null
				|| machineData.MasterAxis.RotateCenter == null || machineData.SlaveAxis.RotateCenter == null ) {
				MyApp.Logger.ShowOnLogPanel( m_LangMgr.TryGetString( "MissingRotationCenter", MISSING_ROTATION_CENTER ), MyApp.NoticeType.Warning );
				return false;
			}
			if( machineData.MasterRotateDir == null || machineData.SlaveRotateDir == null ) {
				MyApp.Logger.ShowOnLogPanel( m_LangMgr.TryGetString( "MissingRotationDirection", MISSING_ROTATION_DIRECTION ), MyApp.NoticeType.Warning );
				return false;
			}

			// [component][List<gp_Trsf>]
			IPerFrameTransformBuilder builder = PerFrameTransformBuilderFactory.Create( machineData );
			if( builder == null ) {
				return false;
			}
			int componentCount = builder.ComponentTypes.Count;
			List<gp_Trsf>[] arrayWithTrsftList = CreateFrameTransformLists( pathFKPntList, componentCount );

			// int[component count][parent indices]
			int[][] chainIdx = BuildFlattenChainList( chainListMap, builder );

			// buffer of component transforms (avoid repeated allocation)
			gp_Trsf[] trsfBuffer = new gp_Trsf[ componentCount ];
			gp_Vec G54Offset = machineData.SimulationOffset;

			// each path
			foreach( var simuFKData in pathFKPntList ) {

				// each point
				if( simuFKData.TraversePntFKList != null && simuFKData.TraversePntFKList.Count > 0 ) {
					foreach( var postpoint in simuFKData.TraversePntFKList ) {
						FKToFrameTranfResult( postpoint, G54Offset, workPiecesChaintSet,
							builder, chainIdx, ref trsfBuffer, ref arrayWithTrsftList );
						frameCount++;
					}
				}
				if( simuFKData.PathPntFKList != null && simuFKData.PathPntFKList.Count > 0 ) {
					foreach( var postpoint in simuFKData.PathPntFKList ) {
						FKToFrameTranfResult( postpoint, G54Offset, workPiecesChaintSet,
							builder, chainIdx, ref trsfBuffer, ref arrayWithTrsftList );
						frameCount++;
					}
				}
				if( simuFKData.ExitPntFKList != null && simuFKData.ExitPntFKList.Count > 0 ) {
					foreach( var postpoint in simuFKData.ExitPntFKList ) {
						FKToFrameTranfResult( postpoint, G54Offset, workPiecesChaintSet,
							builder, chainIdx, ref trsfBuffer, ref arrayWithTrsftList );
						frameCount++;
					}
				}
			}

			// transform as output Dictionary once
			for( int i = 0; i < componentCount; i++ ) {
				m_FrameTransformMap[ builder.ComponentTypes[ i ] ] = arrayWithTrsftList[ i ];
			}
			return true;
		}

		// capacity the trsf for each frame
		static List<gp_Trsf>[] CreateFrameTransformLists( List<PathSimuFKData> pathFKPntList, int componentCount )
		{
			int estimatedFrames = 0;
			foreach( var simuFKData in pathFKPntList ) {
				if( simuFKData.TraversePntFKList != null ) {
					estimatedFrames += simuFKData.TraversePntFKList.Count;
				}
				if( simuFKData.PathPntFKList != null ) {
					estimatedFrames += simuFKData.PathPntFKList.Count;
				}
				if( simuFKData.ExitPntFKList != null ) {
					estimatedFrames += simuFKData.ExitPntFKList.Count;
				}
			}
			var componentTrsfList = new List<gp_Trsf>[ componentCount ];
			for( int i = 0; i < componentCount; i++ ) {
				componentTrsfList[ i ] = new List<gp_Trsf>( estimatedFrames );
			}
			return componentTrsfList;
		}

		// flatten chainListMap into int[component index][parent indices]
		internal static int[][] BuildFlattenChainList( Dictionary<MachineComponentType, List<MachineComponentType>> chainListMap, IPerFrameTransformBuilder builder )
		{
			int componentCount = builder.ComponentTypes.Count;
			int[][] chainIdx = new int[ componentCount ][];
			for( int i = 0; i < componentCount; i++ ) {
				MachineComponentType type = builder.ComponentTypes[ i ];
				if( !chainListMap.ContainsKey( type ) ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LangMgr.TryGetString( "MissingChainMapEntry", MISSING_CHAIN_MAP_ENTRY ), type ), MyApp.NoticeType.Warning );
					chainIdx[ i ] = new int[ 0 ];
					continue;
				}
				var parents = chainListMap[ type ];
				var tmp = new List<int>( parents.Count );
				foreach( var parent in parents ) {
					int pIdx = builder.IndexOf( parent );

					// skip components not in the simulated list (e.g. Base)
					if( pIdx < 0 ) {
						continue;
					}
					tmp.Add( pIdx );
				}
				chainIdx[ i ] = tmp.ToArray();
			}
			return chainIdx;
		}

		internal static void FKToFrameTranfResult( PostPointBase FKPnt, gp_Vec G54Offset,
			HashSet<MachineComponentType> workPiecesChaintSet, IPerFrameTransformBuilder builder,
			int[][] chainIdx, ref gp_Trsf[] trsfBuffer, ref List<gp_Trsf>[] arrayWithTrsfList )
		{
			// per-frame component transforms into trsfBuffer
			builder.BuildFrameTransforms( FKPnt, G54Offset, workPiecesChaintSet, trsfBuffer );

			// calculate each component transform and append to its list
			int componentCount = builder.ComponentTypes.Count;
			for( int i = 0; i < componentCount; i++ ) {
				gp_Trsf trsf = new gp_Trsf();
				int[] parents = chainIdx[ i ];
				for( int p = 0; p < parents.Length; p++ ) {
					trsf.Multiply( trsfBuffer[ parents[ p ] ] );
				}
				trsf.Multiply( trsfBuffer[ i ] );
				arrayWithTrsfList[ i ].Add( trsf );
			}
		}

		#endregion

		#region Discretize Geometry

		internal static List<gp_Pnt> DiscretizeLine( gp_Pnt startPnt, gp_Pnt endPnt, double dStep = DISCRETE_MAX_EDGE_LENGTH )
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
		internal static List<List<gp_Pnt>> DiscretizeArcSegments( gp_Pnt arcStartPnt, gp_Pnt arcMidPnt, gp_Pnt arcEndPnt, double chordHeight = DISCRETE_MAX_DEFLECTION, double maxStep = DISCRETE_MAX_EDGE_LENGTH )
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

		const string MISSING_ROTATION_CENTER = "缺少旋轉中心設定";
		const string MISSING_ROTATION_DIRECTION = "缺少旋轉方向設定";
		const string MISSING_CHAIN_MAP_ENTRY = "鏈列表中缺少 {0} 的映射項";
	}
}
