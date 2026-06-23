using OCC.gp;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using System;
using System.Collections.Generic;
using System.Linq;

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
			gp_Pnt referenceCenter )
		{
			// step 1: find outermost face using ray strategies
			TopoDS_Face outerFace = FindOutermostFaceByStrategies( partShapes, rayStrategies, referenceCenter );

			if( outerFace == null || outerFace.IsNull() ) {
				return new WireFindingResult
				{
					IsSuccess = false,
					Error = WireFindingError.NoIntersectedFace
				};
			}

			// step 2: find D1 continuous faces from the outermost face
			List<TopoDS_Face> d1FaceList = FindD1ContinuousFaces( outerFace, partShapes );

			if( d1FaceList.Count == 0 ) {
				return new WireFindingResult
				{
					IsSuccess = false,
					Error = WireFindingError.NoD1ContinuousFaces
				};
			}

			// step 3: sew faces into groups (shells or individual faces)
			List<TopoDS_Shape> sewedGroups = SewFaceGroups( d1FaceList );

			if( sewedGroups.Count == 0 ) {
				return new WireFindingResult
				{
					IsSuccess = false,
					Error = WireFindingError.SewFaceGroupFailed
				};
			}

			// step 4: extract free boundaries from face groups
			return ExtractFreeBoundariesFromFaceGroups( sewedGroups );
		}

		// extract free boundaries from face groups and return as wires
		public static WireFindingResult ExtractFreeBoundariesFromFaceGroups( List<TopoDS_Shape> faceGroupList )
		{
			List<TopoDS_Wire> wires = new List<TopoDS_Wire>();
			TopTools_IndexedDataMapOfShapeListOfShape edgeMap = new TopTools_IndexedDataMapOfShapeListOfShape();

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

		public static bool TubeStrategies( BoundingBox bbox, out gp_Pnt bboxCenter, out List<gp_Ax1> stretchedStrategies )
		{
			stretchedStrategies = new List<gp_Ax1>();
			bboxCenter = null;
			if( bbox == null ) {
				return false;
			}
			bboxCenter = new gp_Pnt( bbox.XCenter, bbox.YCenter, bbox.ZCenter );

			// Build strategies along +X then +Y, interleaved by priority
			// (center along X, center along Y, then corner midpoints for both)
			List<gp_Ax1> strategiesX = GeometryTool.BuildRayAxesFromBBox( new gp_Dir( 1, 0, 0 ), bbox );
			List<gp_Ax1> strategiesY = GeometryTool.BuildRayAxesFromBBox( new gp_Dir( 0, 1, 0 ), bbox );

			stretchedStrategies = new List<gp_Ax1>();
			int maxCount = Math.Max( strategiesX.Count, strategiesY.Count );
			for( int i = 0; i < maxCount; i++ ) {
				if( i < strategiesX.Count ) {
					stretchedStrategies.Add( strategiesX[ i ] );
				}
				if( i < strategiesY.Count ) {
					stretchedStrategies.Add( strategiesY[ i ] );
				}
			}
			return true;
		}

		public static string GetWireFindingErrorMessage( WireFindingError error )
		{
			switch( error ) {
				case WireFindingError.NoIntersectedFace:
					return "[操作提醒]找不到相交的面";
				case WireFindingError.NoD1ContinuousFaces:
					return "[操作提醒]找不到 D1 連續面";
				case WireFindingError.SewFaceGroupFailed:
					return "[操作提醒]面群組建立失敗";
				case WireFindingError.NoClosedBoundaries:
					return "[操作提醒]找不到封閉邊界";
				default:
					return "[操作提醒]未知錯誤";
			}
		}

		// use multiple ray strategies to find the outermost face
		static TopoDS_Face FindOutermostFaceByStrategies(
			List<TopoDS_Shape> partShapes,
			List<gp_Ax1> strategies,
			gp_Pnt referenceCenter )
		{
			foreach( gp_Ax1 strategy in strategies ) {
				List<TopoDS_Face> candidates = GetFaceCandidatesByRay( partShapes, strategy );

				if( candidates.Count == 0 ) {
					continue;
				}

				if( GeometryTool.FindOutermostFaceAlongPrincipalAxis(
					candidates,
					strategy.Location(),
					strategy.Direction(),
					referenceCenter,
					out TopoDS_Face outerFace ) ) {
					return outerFace;
				}
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
			List<TopoDS_Shape> partShapes )
		{
			// build edge-face map for D1 continuity check
			TopTools_IndexedDataMapOfShapeListOfShape edgeFaceMap = BuildEdgeFaceMap( partShapes );

			// use GeometryTool's BFS algorithm to find D1 continuous faces
			return GeometryTool.FindD1ContinuousFaces( new List<TopoDS_Face> { seedFace }, edgeFaceMap );
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
