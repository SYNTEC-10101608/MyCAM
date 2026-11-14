using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.gp;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;

namespace MyCAM.Helper
{
	internal enum WireFindingError
	{
		None,
		NoIntersectedFace,
		NoD1ContinuousFaces,
		SewFaceGroupFailed,
		NoClosedBoundaries
	}

	internal static class FindWireOnOuterShellHelper
	{
		public class WireFindingResult
		{
			// wire is to build path
			public List<TopoDS_Wire> Wires { get; set; }

			public TopTools_IndexedDataMapOfShapeListOfShape EdgeFaceMap { get; set; }

			public bool IsSuccess { get; set; }

			public WireFindingError Error { get; set; }

			public WireFindingResult()
			{
				Wires = new List<TopoDS_Wire>();
				EdgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();
				IsSuccess = false;
				Error = WireFindingError.None;
			}
		}

		// find the outermost face using ray strategies, then find D1 continuous faces and extract wires from free boundaries
		public static WireFindingResult FindWiresOnOuterShell(
			List<TopoDS_Shape> partShapes,
			List<gp_Ax1> rayStrategies,
			gp_Pnt referenceCenter,
			double d1Tolerance = GeometryTool.DEFAULT_D1_CONTINUOUS_TOLERANCE,
			Action<LoadingBarInfo> reportProgress = null )
		{
			// step 1: find outermost face using ray strategies
			TopoDS_Face outerFace = FindOutermostFaceByStrategies( partShapes, rayStrategies, referenceCenter, reportProgress );

			if( outerFace == null || outerFace.IsNull() ) {
				return new WireFindingResult
				{
					IsSuccess = false,
					Error = WireFindingError.NoIntersectedFace
				};
			}

			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.FindConnectedFaces ) );

			// step 2: find D1 continuous faces from the outermost face
			List<TopoDS_Face> d1FaceList = FindD1ContinuousFaces( outerFace, partShapes, d1Tolerance );

			if( d1FaceList.Count == 0 ) {
				return new WireFindingResult
				{
					IsSuccess = false,
					Error = WireFindingError.NoD1ContinuousFaces
				};
			}

			// step 3: sew faces into groups (shells or individual faces)
			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.SewOuterFaces ) );
			List<TopoDS_Shape> sewedGroups = SewFaceGroups( d1FaceList );

			if( sewedGroups.Count == 0 ) {
				return new WireFindingResult
				{
					IsSuccess = false,
					Error = WireFindingError.SewFaceGroupFailed
				};
			}

			// step 4: extract free boundaries from face groups
			return ExtractFreeBoundariesFromFaceGroups( sewedGroups, reportProgress );
		}

		// extract free boundaries from face groups and return as wires
		public static WireFindingResult ExtractFreeBoundariesFromFaceGroups( List<TopoDS_Shape> faceGroupList,
			Action<LoadingBarInfo> reportProgress = null )
		{
			List<TopoDS_Wire> wires = new List<TopoDS_Wire>();
			TopTools_IndexedDataMapOfShapeListOfShape edgeMap = new TopTools_IndexedDataMapOfShapeListOfShape();

			int nCompleted = 0;

			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ExtractContours, 0, faceGroupList.Count ) );
			foreach( TopoDS_Shape faceGroup in faceGroupList ) {
				ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( faceGroup );

				// build edge-face map for this face group
				TopExp.MapShapesAndAncestors(
					faceGroup,
					TopAbs_ShapeEnum.TopAbs_EDGE,
					TopAbs_ShapeEnum.TopAbs_FACE,
					ref edgeMap );

				// collect all closed wires from free boundaries
				TopExp_Explorer wireExp = new TopExp_Explorer(
					freeBounds.GetClosedWires(),
					TopAbs_ShapeEnum.TopAbs_WIRE );

				while( wireExp.More() ) {
					wires.Add( TopoDS.ToWire( wireExp.Current() ) );
					wireExp.Next();
				}

				// Report progress to the loading bar.
				nCompleted++;
				reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ExtractContours, nCompleted, faceGroupList.Count ) );
			}

			if( wires.Count == 0 ) {
				return new WireFindingResult
				{
					IsSuccess = false,
					Error = WireFindingError.NoClosedBoundaries
				};
			}

			return new WireFindingResult
			{
				IsSuccess = true,
				Wires = wires,
				EdgeFaceMap = edgeMap,
				Error = WireFindingError.None
			};
		}

		public static bool TubeStrategies( BoundingBox bbox, EAlignAxis axis, out gp_Pnt bboxCenter, out List<gp_Ax1> stretchedStrategies )
		{
			stretchedStrategies = new List<gp_Ax1>();
			bboxCenter = null;
			if( bbox == null ) {
				return false;
			}
			bboxCenter = new gp_Pnt( bbox.XCenter, bbox.YCenter, bbox.ZCenter );

			// Build strategies along the two radial directions of the revolution axis, interleaved by priority
			// (center along the first radial dir, center along the second one, then corner midpoints for both)
			ShapeTool.GetRadialDirs( axis, out gp_Dir radialDir1, out gp_Dir radialDir2 );
			List<gp_Ax1> strategiesRadial1 = GeometryTool.BuildRayAxesFromBBox( radialDir1, bbox );
			List<gp_Ax1> strategiesRadial2 = GeometryTool.BuildRayAxesFromBBox( radialDir2, bbox );
			if( strategiesRadial1.Count == 0 && strategiesRadial2.Count == 0 ) {
				return false;
			}

			int maxCount = Math.Max( strategiesRadial1.Count, strategiesRadial2.Count );
			for( int i = 0; i < maxCount; i++ ) {
				if( i < strategiesRadial1.Count ) {
					stretchedStrategies.Add( strategiesRadial1[ i ] );
				}
				if( i < strategiesRadial2.Count ) {
					stretchedStrategies.Add( strategiesRadial2[ i ] );
				}
			}
			return true;
		}

		public static string GetWireFindingErrorMessage( WireFindingError error )
		{
			var langMgr = new LanguageManager.LanguageManager( "FindWireOnOuterShellHelper" );
			switch( error ) {
				case WireFindingError.NoIntersectedFace:
					return langMgr.TryGetString( "NoIntersectedFace", NO_INTERSECTED_FACE );
				case WireFindingError.NoD1ContinuousFaces:
					return langMgr.TryGetString( "NoD1ContinuousFaces", NO_D1_CONTINUOUS_FACES );
				case WireFindingError.SewFaceGroupFailed:
					return langMgr.TryGetString( "SewFaceGroupFailed", SEW_FACE_GROUP_FAILED );
				case WireFindingError.NoClosedBoundaries:
					return langMgr.TryGetString( "NoClosedBoundaries", NO_CLOSED_BOUNDARIES );
				default:
					return langMgr.TryGetString( "UnknownError", UNKNOWN_ERROR );
			}
		}

		const string NO_INTERSECTED_FACE = "[操作提醒]找不到相交的面";
		const string NO_D1_CONTINUOUS_FACES = "[操作提醒]找不到 D1 連續面";
		const string SEW_FACE_GROUP_FAILED = "[操作提醒]面群組建立失敗";
		const string NO_CLOSED_BOUNDARIES = "[操作提醒]找不到封閉邊界";
		const string UNKNOWN_ERROR = "[操作提醒]未知錯誤";

		// use multiple ray strategies to find the outermost face
		static TopoDS_Face FindOutermostFaceByStrategies(
			List<TopoDS_Shape> partShapes,
			List<gp_Ax1> strategies,
			gp_Pnt referenceCenter,
			Action<LoadingBarInfo> reportProgress )
		{
			int nCompleted = 0;
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.FindOuterFace, 0, strategies.Count ) );
			foreach( gp_Ax1 strategy in strategies ) {
				List<TopoDS_Face> candidates = GetFaceCandidatesByRay( partShapes, strategy );

				if( candidates.Count == 0 ) {
					// Report progress to the loading bar.
					nCompleted++;
					reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.FindOuterFace, nCompleted, strategies.Count ) );
					continue;
				}

				if( GeometryTool.FindOutermostFaceAlongPrincipalAxis(
					candidates,
					strategy.Location(),
					strategy.Direction(),
					referenceCenter,
					out TopoDS_Face outerFace ) ) {
					// Report progress to the loading bar.
					nCompleted++;
					reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.FindOuterFace, nCompleted, strategies.Count ) );
					return outerFace;
				}

				// Report progress to the loading bar.
				nCompleted++;
				reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.FindOuterFace, nCompleted, strategies.Count ) );
			}
			return null;
		}

		// filter face candidates by ray intersection with bounding box
		static List<TopoDS_Face> GetFaceCandidatesByRay(
			List<TopoDS_Shape> partShapes,
			gp_Ax1 ray )
		{
			List<TopoDS_Face> faceList = new List<TopoDS_Face>();

			foreach( TopoDS_Shape shape in partShapes ) {

				// pre-filter: skip entire part if ray cannot intersect
				BoundingBox bbox = new BoundingBox( shape );
				if( !GeometryTool.RayIntersectsBBox( ray.Location(), ray.Direction(), bbox ) ) {
					continue;
				}

				// collect all faces from this part
				TopExp_Explorer exp = new TopExp_Explorer( shape, TopAbs_ShapeEnum.TopAbs_FACE );
				for( ; exp.More(); exp.Next() ) {
					faceList.Add( TopoDS.ToFace( exp.Current() ) );
				}
			}
			return faceList;
		}

		// find D1 continuous faces starting from a seed face
		static List<TopoDS_Face> FindD1ContinuousFaces(
			TopoDS_Face seedFace,
			List<TopoDS_Shape> partShapes,
			double d1Tolerance )
		{
			// build edge-face map for D1 continuity check
			TopTools_IndexedDataMapOfShapeListOfShape edgeFaceMap = BuildEdgeFaceMap( partShapes );

			// use GeometryTool's BFS algorithm to find D1 continuous faces
			return GeometryTool.FindD1ContinuousFaces( new List<TopoDS_Face> { seedFace }, edgeFaceMap, d1Tolerance );
		}

		// sew face list into shells or keep as individual faces
		static List<TopoDS_Shape> SewFaceGroups( List<TopoDS_Face> faceList )
		{
			List<TopoDS_Shape> result = new List<TopoDS_Shape>();

			TopoDS_Shape sewResult = ShapeTool.SewShape( faceList.Cast<TopoDS_Shape>().ToList() );

			// single shell or single face
			if( sewResult.shapeType == TopAbs_ShapeEnum.TopAbs_SHELL ||
				sewResult.shapeType == TopAbs_ShapeEnum.TopAbs_FACE ) {
				result.Add( sewResult );
			}
			// multiple shells and/or free faces
			else {
				foreach( TopoDS_Shape shape in sewResult.elementsAsList ) {
					result.Add( shape );
				}
			}
			return result;
		}

		// build edge-to-face mapping for all input shapes
		static TopTools_IndexedDataMapOfShapeListOfShape BuildEdgeFaceMap( List<TopoDS_Shape> shapes )
		{
			TopTools_IndexedDataMapOfShapeListOfShape map = new TopTools_IndexedDataMapOfShapeListOfShape();

			foreach( TopoDS_Shape shape in shapes ) {
				TopExp.MapShapesAndAncestors(
					shape,
					TopAbs_ShapeEnum.TopAbs_EDGE,
					TopAbs_ShapeEnum.TopAbs_FACE,
					ref map );
			}
			return map;
		}
	}
}
