using Clipper2Lib;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using OCC.Bnd;
using OCC.BRep;
using OCC.BRepAdaptor;
using OCC.BRepAlgoAPI;
using OCC.BRepBndLib;
using OCC.BRepBuilderAPI;
using OCC.BRepOffsetAPI;
using OCC.BRepProj;
using OCC.GCPnts;
using OCC.GeomAbs;
using OCC.gp;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyCAM.Helper
{
	public class WallSupportData
	{
		// index properties
		public int IndexIJ
		{
			get; set;
		}

		public bool IsXZPlane
		{
			get; set;
		}

		public double FixedCoord
		{
			get; set;
		}

		// section properties
		public List<TopoDS_Face> SectionFaces
		{
			get; set;
		}
		public List<List<gp_Pnt>> SectionFacesVertices
		{
			get; set;
		}

		// rib plate properties (wall bottom reinforcement)
		public TopoDS_Face RibPlate
		{
			get; set;
		}

		public List<gp_Pnt> RibPlateVertices
		{
			get; set;
		}

		// cross slot properties (interlocking slots)
		public List<TopoDS_Face> CrossSlotFaces
		{
			get; set;
		}

		public List<List<gp_Pnt>> CrossSlotFacesVertices
		{
			get; set;
		}

		// pin properties (feet that insert into base plate)
		public List<TopoDS_Face> PinFaces
		{
			get; set;
		}

		public List<List<gp_Pnt>> PinFacesVertices
		{
			get; set;
		}

		// the final wall shape
		public TopoDS_Face FinalFace
		{
			get; set;
		}

		public PathD FilletedPath2D
		{
			get; set;
		}

		// for debugging: compound of all section faces, rib plate, cross slots, and pins
		public TopoDS_Shape DebugCompound
		{
			get; set;
		}

		public WallSupportData()
		{
			SectionFaces = new List<TopoDS_Face>();
			SectionFacesVertices = new List<List<gp_Pnt>>();
			RibPlateVertices = new List<gp_Pnt>();
			CrossSlotFaces = new List<TopoDS_Face>();
			CrossSlotFacesVertices = new List<List<gp_Pnt>>();
			PinFaces = new List<TopoDS_Face>();
			PinFacesVertices = new List<List<gp_Pnt>>();
		}
	}

	public class BasePlateData
	{
		// 3D geometry (for visualization, no fillet)
		public List<gp_Pnt> OuterVertices
		{
			get; set;
		}

		public List<List<gp_Pnt>> PinSlotVertices
		{
			get; set;
		}

		public TopoDS_Face FinalFace
		{
			get; set;
		}

		// 2D paths (for DXF export, with fillet)
		public PathD OuterFilletedPath2D
		{
			get; set;
		}

		public List<PathD> PinSlotFilletedPaths2D
		{
			get; set;
		}

		// geometry parameters
		public double FixedZ
		{
			get; set;
		}

		public BasePlateData()
		{
			PinSlotVertices = new List<List<gp_Pnt>>();
			PinSlotFilletedPaths2D = new List<PathD>();
		}
	}

	public class SupportStructureResult
	{
		public List<WallSupportData> XZWalls
		{
			get; set;
		}

		public List<WallSupportData> YZWalls
		{
			get; set;
		}

		public BasePlateData BasePlate
		{
			get; set;
		}

		public TopoDS_Shell OriginalShell
		{
			get; set;
		}

		// all support face compound for view
		public TopoDS_Shape AllSupportCompound
		{
			get; set;
		}

		public SupportStructureResult()
		{
			XZWalls = new List<WallSupportData>();
			YZWalls = new List<WallSupportData>();
			BasePlate = new BasePlateData();
		}
	}

	struct WallPlaneConfig // internal used only for wall construction
	{
		public int AxisU;           // 0 = X, 1 = Y
		public double UMin;         // Min value on U axis
		public double UMax;         // Max value on U axis
		public double UExtension;   // Extension on U axis (xE or yE)
		public double FixedCoord;   // Fixed coordinate (yPos for XZ, xPos for YZ)
		public bool CutTopSide;     // true = cut top (YZ), false = cut bottom (XZ)
	}

	public enum SupportBuildError
	{
		None = 0,
		XZWallsFailed,
		YZWallsFailed,
		BasePlateFailed,
	}

	public static class SupportDesignHelper
	{
		public static bool FillShellHoles( TopoDS_Shell inputShell, out TopoDS_Shape filledShape, double offset = 0.0 )
		{
			filledShape = null;
			try {
				if( inputShell == null || inputShell.IsNull() ) {
					return false;
				}

				// step 1: get all closed free-boundary wires on the shell
				ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( inputShell );
				TopoDS_Shape closedWiresShape = freeBounds.GetClosedWires();
				if( closedWiresShape == null || closedWiresShape.IsNull() ) {
					return false;
				}

				List<TopoDS_Wire> wireList = new List<TopoDS_Wire>();
				TopExp_Explorer wireExp = new TopExp_Explorer( closedWiresShape, TopAbs_ShapeEnum.TopAbs_WIRE );
				for( ; wireExp.More(); wireExp.Next() ) {
					wireList.Add( TopoDS.ToWire( wireExp.Current() ) );
				}

				if( wireList.Count == 0 ) {
					return false;
				}

				// step 2: compute XY projected bounding box area for each wire
				List<double> areaList = new List<double>();
				foreach( TopoDS_Wire wire in wireList ) {
					Bnd_Box bbox = new Bnd_Box();
					BRepBndLib.AddOptimal( wire, ref bbox );
					double xmin = 0, ymin = 0, zmin = 0, xmax = 0, ymax = 0, zmax = 0;
					bbox.Get( ref xmin, ref ymin, ref zmin, ref xmax, ref ymax, ref zmax );
					areaList.Add( ( xmax - xmin ) * ( ymax - ymin ) );
				}

				// step 3: largest XY projected area = outer contour, exclude it; rest are holes
				int outerContourIndex = 0;
				double maxArea = areaList[ 0 ];
				for( int i = 1; i < areaList.Count; i++ ) {
					if( areaList[ i ] > maxArea ) {
						maxArea = areaList[ i ];
						outerContourIndex = i;
					}
				}

				List<TopoDS_Wire> holeWireList = new List<TopoDS_Wire>();
				for( int i = 0; i < wireList.Count; i++ ) {
					if( i != outerContourIndex ) {
						holeWireList.Add( wireList[ i ] );
					}
				}

				// no holes found, return original shell as-is
				if( holeWireList.Count == 0 ) {
					filledShape = inputShell;
					return true;
				}

				// step 4: build patches for each hole wire
				List<TopoDS_Face> patchFaceList = new List<TopoDS_Face>();
				if( !BuildHolePatches( holeWireList, offset, ref patchFaceList ) ) {
					return false;
				}

				// step 5: sew the original shell together with all patch faces
				BRepBuilderAPI_Sewing sewing = new BRepBuilderAPI_Sewing( GEOMETRY_TOLERANCE );
				sewing.Add( inputShell );
				foreach( TopoDS_Face patchFace in patchFaceList ) {
					sewing.Add( patchFace );
				}
				sewing.Perform();
				TopoDS_Shape sewedShape = sewing.SewedShape();
				if( sewedShape == null || sewedShape.IsNull() ) {
					return false;
				}

				filledShape = sewedShape;
				return true;
			}
			catch {
				return false;
			}
		}

		static bool BuildHolePatches( List<TopoDS_Wire> holeWireList, double offset, ref List<TopoDS_Face> patchFaceList )
		{
			try {
				if( Math.Abs( offset ) < GEOMETRY_TOLERANCE ) {

					// offset ≈ 0: use original logic to fill holes directly
					return BuildDirectFillPatches( holeWireList, ref patchFaceList );
				}
				else {
					// offset ≠ 0: build projected plane and side walls
					return BuildProjectedPatches( holeWireList, offset, ref patchFaceList );
				}
			}
			catch {
				return false;
			}
		}

		static bool BuildDirectFillPatches( List<TopoDS_Wire> holeWireList, ref List<TopoDS_Face> patchFaceList )
		{
			try {
				foreach( TopoDS_Wire holeWire in holeWireList ) {
					if( !TryBuildHoleFace( holeWire, out TopoDS_Face patchFace ) ) {
						return false;
					}
					patchFaceList.Add( patchFace );
				}
				return true;
			}
			catch {
				return false;
			}
		}

		static bool BuildProjectedPatches( List<TopoDS_Wire> holeWireList, double offset, ref List<TopoDS_Face> patchFaceList )
		{
			try {
				foreach( TopoDS_Wire holeWire in holeWireList ) {

					// compute hole center Z coordinate
					double zCenter = GetWireCenterZ( holeWire );
					double targetZ = zCenter + offset;

					// project wire to XY plane at target Z
					if( !ProjectWireToXYPlane( holeWire, targetZ, out TopoDS_Wire projectedWire ) ) {
						return false;
					}

					// build projected plane face
					if( !TryBuildPlaneFace( projectedWire, out TopoDS_Face planeFace ) ) {
						return false;
					}
					patchFaceList.Add( planeFace );

					// build side wall between original wire and projected wire
					if( !BuildSideWall( holeWire, projectedWire, out TopoDS_Shape sideWall ) ) {
						return false;
					}

					// extract all faces from side wall and add to patch list
					TopExp_Explorer faceExp = new TopExp_Explorer( sideWall, TopAbs_ShapeEnum.TopAbs_FACE );
					for( ; faceExp.More(); faceExp.Next() ) {
						patchFaceList.Add( TopoDS.ToFace( faceExp.Current() ) );
					}
				}
				return true;
			}
			catch {
				return false;
			}
		}

		static double GetWireCenterZ( TopoDS_Wire wire )
		{
			Bnd_Box bbox = new Bnd_Box();
			BRepBndLib.AddOptimal( wire, ref bbox );
			double xmin = 0, ymin = 0, zmin = 0, xmax = 0, ymax = 0, zmax = 0;
			bbox.Get( ref xmin, ref ymin, ref zmin, ref xmax, ref ymax, ref zmax );
			return ( zmin + zmax ) / 2.0;
		}

		static bool ProjectWireToXYPlane( TopoDS_Wire wire, double targetZ, out TopoDS_Wire projectedWire )
		{
			projectedWire = null;
			try {
				// create target plane at Z = targetZ
				gp_Pnt planeOrigin = new gp_Pnt( 0, 0, targetZ );
				gp_Dir planeNormal = new gp_Dir( 0, 0, 1 );
				gp_Pln plane = new gp_Pln( planeOrigin, planeNormal );

				// create a large plane face for projection target
				const double PLANE_SIZE = 1e6;
				BRepBuilderAPI_MakeFace planeFaceMaker = new BRepBuilderAPI_MakeFace( plane, -PLANE_SIZE, PLANE_SIZE, -PLANE_SIZE, PLANE_SIZE );
				if( !planeFaceMaker.IsDone() ) {
					return false;
				}
				TopoDS_Face planeFace = planeFaceMaker.Face();

				// project wire onto plane along Z direction
				BRepProj_Projection projection = new BRepProj_Projection( wire, planeFace, planeNormal );
				if( !projection.IsDone() || !projection.More() ) {
					return false;
				}

				projectedWire = projection.Current();
				return projectedWire != null && !projectedWire.IsNull();
			}
			catch {
				return false;
			}
		}

		static bool TryBuildPlaneFace( TopoDS_Wire wire, out TopoDS_Face planeFace )
		{
			planeFace = null;
			try {
				BRepBuilderAPI_MakeFace makeFace = new BRepBuilderAPI_MakeFace( wire );
				if( makeFace.IsDone() && makeFace.Error() == BRepBuilderAPI_FaceError.BRepBuilderAPI_FaceDone ) {
					planeFace = makeFace.Face();
					return true;
				}
				return false;
			}
			catch {
				return false;
			}
		}

		static bool BuildSideWall( TopoDS_Wire bottomWire, TopoDS_Wire topWire, out TopoDS_Shape sideWall )
		{
			sideWall = null;
			try {
				// create ruled surface between two wires
				BRepOffsetAPI_ThruSections thruSections = new BRepOffsetAPI_ThruSections( false, true );

				// true by default, but explicitly set for clarity
				thruSections.CheckCompatibility( true );

				// add wires in order: bottom first, then top
				thruSections.AddWire( bottomWire );
				thruSections.AddWire( topWire );

				// build the side wall
				thruSections.Build();
				if( !thruSections.IsDone() ) {
					return false;
				}

				sideWall = thruSections.Shape();
				return sideWall != null && !sideWall.IsNull();
			}
			catch {
				return false;
			}
		}

		static bool TryBuildHoleFace( TopoDS_Wire holeWire, out TopoDS_Face holeFace )
		{
			holeFace = null;

			// stage 4a: attempt planar face
			BRepBuilderAPI_MakeFace makeFace = new BRepBuilderAPI_MakeFace( holeWire );
			if( makeFace.IsDone() && makeFace.Error() == BRepBuilderAPI_FaceError.BRepBuilderAPI_FaceDone ) {
				holeFace = makeFace.Face();
				return true;
			}

			// stage 4b: fallback to free-form surface filling
			BRepOffsetAPI_MakeFilling filling = new BRepOffsetAPI_MakeFilling();
			TopExp_Explorer edgeExp = new TopExp_Explorer( holeWire, TopAbs_ShapeEnum.TopAbs_EDGE );
			for( ; edgeExp.More(); edgeExp.Next() ) {
				TopoDS_Edge edge = TopoDS.ToEdge( edgeExp.Current() );

				// this is inital design by copilot
				filling.Add( edge, GeomAbs_Shape.GeomAbs_C0, true );
			}
			filling.Build();
			if( !filling.IsDone() ) {
				return false;
			}
			TopoDS_Shape fillingShape = filling.Shape();
			if( fillingShape == null || fillingShape.IsNull() ) {
				return false;
			}

			// extract the resulting face from the filling shape
			TopExp_Explorer faceExp = new TopExp_Explorer( fillingShape, TopAbs_ShapeEnum.TopAbs_FACE );
			if( !faceExp.More() ) {
				return false;
			}
			holeFace = TopoDS.ToFace( faceExp.Current() );
			return true;
		}

		public static bool ComputeShellWithSectionWires(
				TopoDS_Shell inputShell,
				double[] xPosList,
				double[] yPosList,
				double zE,
				double xE,
				double yE,
				double wallThick,
				double crossGap,
				double ribHeight,
				double filletRadius,
				double baseThick,
				double pinWidth,
				int xPinCount,
				int yPinCount,
				out SupportStructureResult result,
				out SupportBuildError buildError )
		{
			result = null;
			buildError = SupportBuildError.None;
			try {
				if( inputShell == null || inputShell.IsNull() ) {
					return false;
				}

				result = new SupportStructureResult();
				result.OriginalShell = inputShell;

				// step 1: compute bounding box and bottom plane
				double xmin, xmax, ymin, ymax, zbottom;
				if( !ComputeBoundingBox( inputShell, zE, out xmin, out xmax, out ymin, out ymax, out zbottom ) ) {
					return false;
				}

				// step 2: pre-compute all cross position shell height
				double[,] intersectionMatrix;
				if( !ComputeIntersectionMatrix( inputShell, xPosList, yPosList, zbottom, ribHeight, out intersectionMatrix ) ) {
					return false;
				}

				// step 3: pre-compute all section wires
				List<TopTools_HSequenceOfShape> xzWires = new List<TopTools_HSequenceOfShape>();
				List<TopTools_HSequenceOfShape> yzWires = new List<TopTools_HSequenceOfShape>();
				ComputeAllXZSectionWires( inputShell, yPosList, ref xzWires );
				ComputeAllYZSectionWires( inputShell, xPosList, ref yzWires );

				// step 4: build XZ walls (one for each yPos)
				if( !BuildXZWalls(
					xzWires, xPosList, yPosList,
					xmin, xmax, xE, zbottom, ribHeight,
					wallThick, crossGap,
					baseThick, pinWidth, xPinCount,
					intersectionMatrix,
					result.XZWalls ) ) {
					buildError = SupportBuildError.XZWallsFailed;
					return false;
				}

				// step 5: build YZ walls (one for each xPos)
				if( !BuildYZWalls(
					yzWires, xPosList, yPosList,
					ymin, ymax, yE, zbottom, ribHeight,
					wallThick, crossGap,
					baseThick, pinWidth, yPinCount,
					intersectionMatrix,
					 result.YZWalls ) ) {
					buildError = SupportBuildError.YZWallsFailed;
					return false;
				}

				// step 6: build base plate
				if( !BuildBasePlate(
					xPosList, yPosList,
					xmin, xmax, ymin, ymax, xE, yE,
					zbottom, baseThick, wallThick, pinWidth,
					xPinCount, yPinCount,
					result.XZWalls, result.YZWalls,
					result.BasePlate ) ) {
					buildError = SupportBuildError.BasePlateFailed;
					return false;
				}

				// step 7: compute wall final faces
				foreach( WallSupportData wall in result.XZWalls ) {
					ComputeWallFinalFace( wall, filletRadius );
				}
				foreach( WallSupportData wall in result.YZWalls ) {
					ComputeWallFinalFace( wall, filletRadius );
				}

				// step 8: compute base plate final face
				ComputeBasePlateFinalFace( result.BasePlate, filletRadius );

				// step 9: build debug compound for visualization
				TopoDS_Shape finalViewCompound;
				if( !BuildFinalViewCompound( result, out finalViewCompound ) ) {
					return false;
				}
				result.AllSupportCompound = finalViewCompound;

				return true;
			}
			catch {
				return false;
			}
		}

		static bool ComputeBoundingBox( TopoDS_Shell inputShell, double zE, out double xmin, out double xmax, out double ymin, out double ymax, out double zbottom )
		{
			xmin = 0;
			xmax = 0;
			ymin = 0;
			ymax = 0;
			zbottom = 0;
			try {
				Bnd_Box bbox = new Bnd_Box();
				BRepBndLib.AddOptimal( inputShell, ref bbox );

				double zmin = 0, zmax = 0;
				bbox.Get( ref xmin, ref ymin, ref zmin, ref xmax, ref ymax, ref zmax );

				zbottom = zmin - zE;
				return true;
			}
			catch {
				return false;
			}
		}

		static bool ComputeIntersectionMatrix(
			TopoDS_Shell inputShell,
			double[] xPosList,
			double[] yPosList,
			double zbottom,
			double zBase,
			out double[,] intersectionMatrix )
		{
			int nx = xPosList.Length;
			int ny = yPosList.Length;
			intersectionMatrix = new double[ ny, nx ];

			try {
				for( int i = 0; i < ny; i++ ) {
					for( int j = 0; j < nx; j++ ) {
						double xPos = xPosList[ j ];
						double yPos = yPosList[ i ];

						// build vertical ray from top to bottom
						const double RAY_LENGTH = 1e6; // sufficiently long to intersect the shell
						gp_Pnt rayStart = new gp_Pnt( xPos, yPos, RAY_LENGTH );
						gp_Pnt rayEnd = new gp_Pnt( xPos, yPos, -RAY_LENGTH );
						BRepBuilderAPI_MakeEdge rayMaker = new BRepBuilderAPI_MakeEdge( rayStart, rayEnd );
						if( !rayMaker.IsDone() ) {
							intersectionMatrix[ i, j ] = zbottom + zBase;
							continue;
						}
						TopoDS_Edge rayEdge = rayMaker.Edge();

						// compute intersection with shell
						BRepAlgoAPI_Section section = new BRepAlgoAPI_Section( inputShell, rayEdge );
						section.Build();

						double maxZ = double.MinValue;
						bool found = false;

						if( section.IsDone() ) {
							TopExp_Explorer vExp = new TopExp_Explorer( section.Shape(), TopAbs_ShapeEnum.TopAbs_VERTEX );
							for( ; vExp.More(); vExp.Next() ) {
								TopoDS_Vertex vertex = TopoDS.ToVertex( vExp.Current() );
								gp_Pnt pt = BRep_Tool.Pnt( vertex );
								if( pt.Z() > maxZ ) {
									maxZ = pt.Z();
									found = true;
								}
							}
						}

						intersectionMatrix[ i, j ] = found ? maxZ : ( zbottom + zBase );
					}
				}

				return true;
			}
			catch {
				return false;
			}
		}

		static bool ComputeAllXZSectionWires( TopoDS_Shell inputShell, double[] yPosList, ref List<TopTools_HSequenceOfShape> wireList )
		{
			try {
				if( yPosList == null || yPosList.Length == 0 ) {
					return true;
				}

				foreach( double yPos in yPosList ) {

					// XZ plane at y = yPos, normal = (0, 1, 0)
					gp_Pnt planeOrigin = new gp_Pnt( 0, yPos, 0 );
					gp_Dir planeNormal = new gp_Dir( 0, 1, 0 );

					TopTools_HSequenceOfShape wireSeq;
					if( ComputeSectionWires( inputShell, planeOrigin, planeNormal, out wireSeq ) ) {
						wireList.Add( wireSeq );
					}
					else {
						wireList.Add( null );  // placeholder to maintain index correspondence
					}
				}

				return true;
			}
			catch {
				return false;
			}
		}

		static bool ComputeAllYZSectionWires( TopoDS_Shell inputShell, double[] xPosList, ref List<TopTools_HSequenceOfShape> wireList )
		{
			try {
				if( xPosList == null || xPosList.Length == 0 ) {
					return true;
				}

				foreach( double xPos in xPosList ) {
					// YZ plane at x = xPos, normal = (1, 0, 0)
					gp_Pnt planeOrigin = new gp_Pnt( xPos, 0, 0 );
					gp_Dir planeNormal = new gp_Dir( 1, 0, 0 );

					TopTools_HSequenceOfShape wireSeq;
					if( ComputeSectionWires( inputShell, planeOrigin, planeNormal, out wireSeq ) ) {
						wireList.Add( wireSeq );
					}
					else {
						wireList.Add( null );  // placeholder to maintain index correspondence
					}
				}

				return true;
			}
			catch {
				return false;
			}
		}

		static bool ComputeSectionWires( TopoDS_Shell inputShell, gp_Pnt planeOrigin, gp_Dir planeNormal, out TopTools_HSequenceOfShape wireSeq )
		{
			wireSeq = null;
			try {
				// construct plane
				gp_Pln plane = new gp_Pln( planeOrigin, planeNormal );

				// intersect shell with plane
				BRepAlgoAPI_Section section = new BRepAlgoAPI_Section( inputShell, plane );
				section.Build();
				if( !section.IsDone() ) {
					return false;
				}

				// collect edges from section
				TopTools_ListOfShape edges = section.SectionEdges();
				List<TopoDS_Edge> edgeList = new List<TopoDS_Edge>();
				foreach( TopoDS_Shape edgeShape in edges ) {
					if( edgeShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_EDGE ) {
						edgeList.Add( TopoDS.ToEdge( edgeShape ) );
					}
				}

				if( edgeList.Count == 0 ) {
					return false;
				}

				// connect edges into wires
				TopTools_HSequenceOfShape edgeSeq = new TopTools_HSequenceOfShape();
				foreach( TopoDS_Edge edge in edgeList ) {
					edgeSeq.Append( edge );
				}
				wireSeq = new TopTools_HSequenceOfShape();
				ShapeAnalysis_FreeBounds.ConnectEdgesToWires( edgeSeq, 0.01, false, wireSeq );

				return wireSeq.Length() > 0;
			}
			catch {
				return false;
			}
		}

		static bool BuildXZWalls(
			List<TopTools_HSequenceOfShape> xzWires,
			double[] xPosList,
			double[] yPosList,
			double xmin, double xmax, double xE,
			double zbottom, double ribHeight,
			double wallThick, double crossGap,
			double baseThick, double pinWidth, int xPinCount,
			double[,] intersectionMatrix,
			 List<WallSupportData> xzWalls )
		{
			try {
				for( int i = 0; i < yPosList.Length; i++ ) {
					WallPlaneConfig config = new WallPlaneConfig
					{
						AxisU = 0,              // X axis
						UMin = xmin,
						UMax = xmax,
						UExtension = xE,
						FixedCoord = yPosList[ i ],
						CutTopSide = false      // cut bottom
					};

					WallSupportData wall = new WallSupportData();
					wall.IndexIJ = i;
					wall.IsXZPlane = true;
					wall.FixedCoord = config.FixedCoord;

					if( !BuildWallGeneric( xzWires[ i ], config, xPosList, yPosList, i,
						zbottom, ribHeight, wallThick, crossGap,
						baseThick, pinWidth, xPinCount,
						intersectionMatrix, ref wall ) ) {
						return false;
					}

					BuildWallDebugCompound( ref wall );
					xzWalls.Add( wall );
				}

				return true;
			}
			catch {
				return false;
			}
		}

		static bool BuildYZWalls(
			List<TopTools_HSequenceOfShape> yzWires,
			double[] xPosList,
			double[] yPosList,
			double ymin, double ymax, double yE,
			double zbottom, double ribHeight,
			double wallThick, double crossGap,
			double baseThick, double pinWidth, int yPinCount,
			double[,] intersectionMatrix,
			 List<WallSupportData> yzWalls )
		{
			try {
				for( int j = 0; j < xPosList.Length; j++ ) {
					WallPlaneConfig config = new WallPlaneConfig
					{
						AxisU = 1,              // Y axis
						UMin = ymin,
						UMax = ymax,
						UExtension = yE,
						FixedCoord = xPosList[ j ],
						CutTopSide = true       // cut top
					};

					WallSupportData wall = new WallSupportData();
					wall.IndexIJ = j;
					wall.IsXZPlane = false;
					wall.FixedCoord = config.FixedCoord;

					if( !BuildWallGeneric( yzWires[ j ], config, xPosList, yPosList, j,
						zbottom, ribHeight, wallThick, crossGap,
						baseThick, pinWidth, yPinCount,
						intersectionMatrix, ref wall ) ) {
						return false;
					}

					BuildWallDebugCompound( ref wall );
					yzWalls.Add( wall );
				}

				return true;
			}
			catch {
				return false;
			}
		}

		static bool BuildWallGeneric(
			TopTools_HSequenceOfShape wireSeq,
			WallPlaneConfig config,
			double[] xPosList,
			double[] yPosList,
			int mainIndex,
			double zbottom,
			double ribHeight,
			double wallThick,
			double crossGap,
			double baseThick,
			double pinWidth,
			int pinCount,
			double[,] intersectionMatrix,
			ref WallSupportData wall )
		{
			try {
				// a. build section faces
				if( wireSeq != null ) {
					if( !BuildSectionFacesGeneric( wireSeq, config, zbottom, ref wall ) ) {
						return false;
					}
				}

				// b. build rib plate
				if( !BuildRibPlateGeneric( config, zbottom, ribHeight, ref wall ) ) {
					return false;
				}

				// c. build cross slot faces
				BuildCrossSlotFacesGeneric( config, xPosList, yPosList, mainIndex,
					zbottom, crossGap, wallThick, intersectionMatrix, ref wall );

				// d. build pin faces
				BuildPinFacesGeneric( config, zbottom, baseThick, pinWidth, pinCount, ref wall );

				return true;
			}
			catch {
				return false;
			}
		}

		static bool BuildSectionFacesGeneric(
			TopTools_HSequenceOfShape wireSeq,
			WallPlaneConfig config,
			double zbottom,
			ref WallSupportData wall )
		{
			try {
				for( int i = 1; i <= wireSeq.Length(); i++ ) {
					TopoDS_Wire sectionWire = TopoDS.ToWire( wireSeq.Value( i ) );

					if( BRep_Tool.IsClosed( sectionWire ) ) {
						continue;
					}

					TopoDS_Vertex vFirst = new TopoDS_Vertex();
					TopoDS_Vertex vLast = new TopoDS_Vertex();
					TopExp.Vertices( sectionWire, ref vFirst, ref vLast );

					gp_Pnt pFirst = BRep_Tool.Pnt( vFirst );
					gp_Pnt pLast = BRep_Tool.Pnt( vLast );

					// extract U and Z coordinates
					double uFirst = ( config.AxisU == 0 ) ? pFirst.X() : pFirst.Y();
					double zFirst = pFirst.Z();
					double uLast = ( config.AxisU == 0 ) ? pLast.X() : pLast.Y();
					double zLast = pLast.Z();

					// determine left and right based on U coordinate
					double uL, zL, uR, zR;
					bool needReverse = false;

					if( uFirst < uLast ) {
						uL = uFirst;
						zL = zFirst;
						uR = uLast;
						zR = zLast;
						needReverse = true;
					}
					else {
						uL = uLast;
						zL = zLast;
						uR = uFirst;
						zR = zFirst;
						needReverse = false;
					}

					if( Math.Abs( uR - uL ) < GEOMETRY_TOLERANCE ) {
						continue;
					}

					TopoDS_Wire topWire = needReverse ? TopoDS.ToWire( sectionWire.Reversed() ) : sectionWire;

					// build corner points in 3D
					gp_Pnt pTopRight, pTopLeft, pBottomLeft, pBottomRight;
					if( config.AxisU == 0 ) {  // XZ plane
						pTopRight = new gp_Pnt( uR, config.FixedCoord, zR );
						pTopLeft = new gp_Pnt( uL, config.FixedCoord, zL );
						pBottomLeft = new gp_Pnt( uL, config.FixedCoord, zbottom );
						pBottomRight = new gp_Pnt( uR, config.FixedCoord, zbottom );
					}
					else {  // YZ plane
						pTopRight = new gp_Pnt( config.FixedCoord, uR, zR );
						pTopLeft = new gp_Pnt( config.FixedCoord, uL, zL );
						pBottomLeft = new gp_Pnt( config.FixedCoord, uL, zbottom );
						pBottomRight = new gp_Pnt( config.FixedCoord, uR, zbottom );
					}

					// build edges, this is for debug only
					BRepBuilderAPI_MakeEdge leftEdgeMaker = new BRepBuilderAPI_MakeEdge( pTopLeft, pBottomLeft );
					if( !leftEdgeMaker.IsDone() ) {
						continue;
					}
					TopoDS_Edge leftEdge = leftEdgeMaker.Edge();

					BRepBuilderAPI_MakeEdge bottomEdgeMaker = new BRepBuilderAPI_MakeEdge( pBottomLeft, pBottomRight );
					if( !bottomEdgeMaker.IsDone() ) {
						continue;
					}
					TopoDS_Edge bottomEdge = bottomEdgeMaker.Edge();

					BRepBuilderAPI_MakeEdge rightEdgeMaker = new BRepBuilderAPI_MakeEdge( pBottomRight, pTopRight );
					if( !rightEdgeMaker.IsDone() ) {
						continue;
					}
					TopoDS_Edge rightEdge = rightEdgeMaker.Edge();

					// build closed wire, this is for debug only
					BRepBuilderAPI_MakeWire wireMaker = new BRepBuilderAPI_MakeWire();
					TopExp_Explorer edgeExp = new TopExp_Explorer( topWire, TopAbs_ShapeEnum.TopAbs_EDGE );
					for( ; edgeExp.More(); edgeExp.Next() ) {
						wireMaker.Add( TopoDS.ToEdge( edgeExp.Current() ) );
					}
					wireMaker.Add( leftEdge );
					wireMaker.Add( bottomEdge );
					wireMaker.Add( rightEdge );

					if( !wireMaker.IsDone() || !BRep_Tool.IsClosed( wireMaker.Wire() ) ) {
						continue;
					}

					// build face, this is for debug only
					BRepBuilderAPI_MakeFace faceMaker = new BRepBuilderAPI_MakeFace( wireMaker.Wire() );
					if( !faceMaker.IsDone() ) {
						continue;
					}

					TopoDS_Face face = faceMaker.Face();
					wall.SectionFaces.Add( face );

					// discrete to vertices, this is the data used for final computation
					List<gp_Pnt> vertices;
					if( DiscreteWireToVertices( topWire, CHORD_ERROR_TOLERANCE, out vertices ) ) {
						wall.SectionFacesVertices.Add( vertices );
					}
					vertices.Add( pBottomLeft );
					vertices.Add( pBottomRight );
				}

				return wall.SectionFaces.Count > 0;
			}
			catch {
				return false;
			}
		}

		static bool BuildRibPlateGeneric(
			WallPlaneConfig config,
			double zbottom,
			double ribHeight,
			ref WallSupportData wall )
		{
			try {
				// calculate corner points
				gp_Pnt p1, p2, p3, p4;
				if( config.AxisU == 0 ) {  // XZ plane
					p1 = new gp_Pnt( config.UMax + config.UExtension, config.FixedCoord, zbottom + ribHeight );
					p2 = new gp_Pnt( config.UMin - config.UExtension, config.FixedCoord, zbottom + ribHeight );
					p3 = new gp_Pnt( config.UMin - config.UExtension, config.FixedCoord, zbottom );
					p4 = new gp_Pnt( config.UMax + config.UExtension, config.FixedCoord, zbottom );
				}
				else {  // YZ plane
					p1 = new gp_Pnt( config.FixedCoord, config.UMax + config.UExtension, zbottom + ribHeight );
					p2 = new gp_Pnt( config.FixedCoord, config.UMin - config.UExtension, zbottom + ribHeight );
					p3 = new gp_Pnt( config.FixedCoord, config.UMin - config.UExtension, zbottom );
					p4 = new gp_Pnt( config.FixedCoord, config.UMax + config.UExtension, zbottom );
				}

				// build edges, this is for debug only
				BRepBuilderAPI_MakeEdge e1 = new BRepBuilderAPI_MakeEdge( p1, p2 );
				BRepBuilderAPI_MakeEdge e2 = new BRepBuilderAPI_MakeEdge( p2, p3 );
				BRepBuilderAPI_MakeEdge e3 = new BRepBuilderAPI_MakeEdge( p3, p4 );
				BRepBuilderAPI_MakeEdge e4 = new BRepBuilderAPI_MakeEdge( p4, p1 );

				if( !e1.IsDone() || !e2.IsDone() || !e3.IsDone() || !e4.IsDone() ) {
					return false;
				}

				// build wire, this is for debug only
				BRepBuilderAPI_MakeWire wireMaker = new BRepBuilderAPI_MakeWire();
				wireMaker.Add( e1.Edge() );
				wireMaker.Add( e2.Edge() );
				wireMaker.Add( e3.Edge() );
				wireMaker.Add( e4.Edge() );

				if( !wireMaker.IsDone() || !BRep_Tool.IsClosed( wireMaker.Wire() ) ) {
					return false;
				}

				// build face, this is for debug only
				BRepBuilderAPI_MakeFace faceMaker = new BRepBuilderAPI_MakeFace( wireMaker.Wire() );
				if( !faceMaker.IsDone() ) {
					return false;
				}

				wall.RibPlate = faceMaker.Face();

				// store rib plate vertices, this is the data used for final computation
				wall.RibPlateVertices = new List<gp_Pnt> { p1, p2, p3, p4 };

				return true;
			}
			catch {
				return false;
			}
		}

		static bool BuildCrossSlotFacesGeneric(
			WallPlaneConfig config,
			double[] xPosList,
			double[] yPosList,
			int mainIndex,
			double zbottom,
			double crossGap,
			double wallThick,
			double[,] intersectionMatrix,
			ref WallSupportData wall )
		{
			try {
				// determine which positions to iterate (cross positions)
				double[] crossPosList = config.AxisU == 0 ? xPosList : yPosList;

				for( int crossIdx = 0; crossIdx < crossPosList.Length; crossIdx++ ) {
					double crossPos = crossPosList[ crossIdx ];

					// get intersection Z from matrix
					double zSlot;
					if( config.AxisU == 0 ) {  // XZ plane, mainIndex is yIndex
						zSlot = intersectionMatrix[ mainIndex, crossIdx ];
					}
					else {  // YZ plane, mainIndex is xIndex
						zSlot = intersectionMatrix[ crossIdx, mainIndex ];
					}

					// calculate slot Z range
					double zSlotTop, zSlotBot;
					const double CUTTER_COMPENSATION = 100.0;  // to ensure a big enough cutter size
					if( config.CutTopSide ) {
						// cut top side (YZ walls)
						zSlotTop = zSlot + CUTTER_COMPENSATION;
						zSlotBot = zbottom + ( zSlot - zbottom ) / 2.0 - crossGap;
					}
					else {
						// cut bottom side (XZ walls)
						zSlotTop = zbottom + ( zSlot - zbottom ) / 2.0 + crossGap;
						zSlotBot = zbottom - CUTTER_COMPENSATION;
					}

					// validate slot
					if( Math.Abs( zSlotTop - zSlotBot ) < GEOMETRY_TOLERANCE ) {
						continue;
					}

					// build slot rectangle
					gp_Pnt p1, p2, p3, p4;
					if( config.AxisU == 0 ) {  // XZ plane
						p1 = new gp_Pnt( crossPos + wallThick / 2, config.FixedCoord, zSlotTop );
						p2 = new gp_Pnt( crossPos - wallThick / 2, config.FixedCoord, zSlotTop );
						p3 = new gp_Pnt( crossPos - wallThick / 2, config.FixedCoord, zSlotBot );
						p4 = new gp_Pnt( crossPos + wallThick / 2, config.FixedCoord, zSlotBot );
					}
					else {  // YZ plane
						p1 = new gp_Pnt( config.FixedCoord, crossPos + wallThick / 2, zSlotTop );
						p2 = new gp_Pnt( config.FixedCoord, crossPos - wallThick / 2, zSlotTop );
						p3 = new gp_Pnt( config.FixedCoord, crossPos - wallThick / 2, zSlotBot );
						p4 = new gp_Pnt( config.FixedCoord, crossPos + wallThick / 2, zSlotBot );
					}

					// build edges, this is for debug only
					BRepBuilderAPI_MakeEdge e1 = new BRepBuilderAPI_MakeEdge( p1, p2 );
					BRepBuilderAPI_MakeEdge e2 = new BRepBuilderAPI_MakeEdge( p2, p3 );
					BRepBuilderAPI_MakeEdge e3 = new BRepBuilderAPI_MakeEdge( p3, p4 );
					BRepBuilderAPI_MakeEdge e4 = new BRepBuilderAPI_MakeEdge( p4, p1 );

					if( !e1.IsDone() || !e2.IsDone() || !e3.IsDone() || !e4.IsDone() ) {
						continue;
					}

					// build wire, this is for debug only
					BRepBuilderAPI_MakeWire wireMaker = new BRepBuilderAPI_MakeWire();
					wireMaker.Add( e1.Edge() );
					wireMaker.Add( e2.Edge() );
					wireMaker.Add( e3.Edge() );
					wireMaker.Add( e4.Edge() );

					if( !wireMaker.IsDone() || !BRep_Tool.IsClosed( wireMaker.Wire() ) ) {
						continue;
					}

					// build face, this is for debug only
					BRepBuilderAPI_MakeFace faceMaker = new BRepBuilderAPI_MakeFace( wireMaker.Wire() );
					if( !faceMaker.IsDone() ) {
						continue;
					}

					wall.CrossSlotFaces.Add( faceMaker.Face() );

					// store cross slot face vertices, this is the data used for final computation
					wall.CrossSlotFacesVertices.Add( new List<gp_Pnt> { p1, p2, p3, p4 } );
				}
				return wall.CrossSlotFaces.Count > 0;
			}
			catch {
				return false;
			}
		}

		static bool BuildPinFacesGeneric(
			WallPlaneConfig config,
			double zbottom,
			double baseThick,
			double pinWidth,
			int pinCount,
			ref WallSupportData wall )
		{
			try {
				if( pinCount <= 0 ) {
					return true;  // No pins is not an error
				}

				// Calculate pin positions (evenly distributed)
				double uBase = ( config.UMax + config.UExtension ) - ( config.UMin - config.UExtension );
				double uStep = uBase / ( pinCount + 1 );

				for( int m = 1; m <= pinCount; m++ ) {
					double uPin = ( config.UMin - config.UExtension ) + m * uStep;

					// Build rectangular pin (extending downward)
					gp_Pnt p1, p2, p3, p4;
					double halfWidth = pinWidth / 2.0;

					if( config.AxisU == 0 ) {  // XZ plane
						p1 = new gp_Pnt( uPin + halfWidth, config.FixedCoord, zbottom );
						p2 = new gp_Pnt( uPin - halfWidth, config.FixedCoord, zbottom );
						p3 = new gp_Pnt( uPin - halfWidth, config.FixedCoord, zbottom - baseThick );
						p4 = new gp_Pnt( uPin + halfWidth, config.FixedCoord, zbottom - baseThick );
					}
					else {  // YZ plane
						p1 = new gp_Pnt( config.FixedCoord, uPin + halfWidth, zbottom );
						p2 = new gp_Pnt( config.FixedCoord, uPin - halfWidth, zbottom );
						p3 = new gp_Pnt( config.FixedCoord, uPin - halfWidth, zbottom - baseThick );
						p4 = new gp_Pnt( config.FixedCoord, uPin + halfWidth, zbottom - baseThick );
					}

					// Build edges (for debug only)
					BRepBuilderAPI_MakeEdge e1 = new BRepBuilderAPI_MakeEdge( p1, p2 );
					BRepBuilderAPI_MakeEdge e2 = new BRepBuilderAPI_MakeEdge( p2, p3 );
					BRepBuilderAPI_MakeEdge e3 = new BRepBuilderAPI_MakeEdge( p3, p4 );
					BRepBuilderAPI_MakeEdge e4 = new BRepBuilderAPI_MakeEdge( p4, p1 );

					if( !e1.IsDone() || !e2.IsDone() || !e3.IsDone() || !e4.IsDone() ) {
						continue;
					}

					// Build wire (for debug only)
					BRepBuilderAPI_MakeWire wireMaker = new BRepBuilderAPI_MakeWire();
					wireMaker.Add( e1.Edge() );
					wireMaker.Add( e2.Edge() );
					wireMaker.Add( e3.Edge() );
					wireMaker.Add( e4.Edge() );

					if( !wireMaker.IsDone() || !BRep_Tool.IsClosed( wireMaker.Wire() ) ) {
						continue;
					}

					// Build face (for debug only)
					BRepBuilderAPI_MakeFace faceMaker = new BRepBuilderAPI_MakeFace( wireMaker.Wire() );
					if( !faceMaker.IsDone() ) {
						continue;
					}

					wall.PinFaces.Add( faceMaker.Face() );

					// Store vertices (for computation)
					wall.PinFacesVertices.Add( new List<gp_Pnt> { p1, p2, p3, p4 } );
				}

				return true;
			}
			catch {
				return false;
			}
		}

		static bool DiscreteWireToVertices( TopoDS_Wire wire, double maxDeflection, out List<gp_Pnt> vertices )
		{
			vertices = new List<gp_Pnt>();

			// the wire orientation and edge orientation are both matter
			try {
				// collect all edges from the wire
				List<TopoDS_Edge> edgeList = new List<TopoDS_Edge>();
				TopExp_Explorer edgeExp = new TopExp_Explorer( wire, TopAbs_ShapeEnum.TopAbs_EDGE );
				for( ; edgeExp.More(); edgeExp.Next() ) {
					edgeList.Add( TopoDS.ToEdge( edgeExp.Current() ) );
				}

				if( edgeList.Count == 0 ) {
					return false;
				}

				// reverse edge order if wire is reversed
				if( wire.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					edgeList.Reverse();
				}

				// process each edge
				bool isFirstEdge = true;
				foreach( TopoDS_Edge edge in edgeList ) {
					BRepAdaptor_Curve adC = new BRepAdaptor_Curve( edge );
					double dStartU = adC.FirstParameter();
					double dEndU = adC.LastParameter();

					GCPnts_QuasiUniformDeflection qUD = new GCPnts_QuasiUniformDeflection( adC, maxDeflection, dStartU, dEndU );

					if( qUD.NbPoints() == 0 ) {
						continue;
					}

					// collect points for this edge
					List<gp_Pnt> edgePoints = new List<gp_Pnt>();
					for( int i = 1; i <= qUD.NbPoints(); i++ ) {
						edgePoints.Add( qUD.Value( i ) );
					}

					// reverse if edge orientation is reversed
					if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
						edgePoints.Reverse();
					}

					// add points, skipping first point if not first edge (to avoid duplication)
					int startIdx = isFirstEdge ? 0 : 1;
					for( int i = startIdx; i < edgePoints.Count; i++ ) {
						vertices.Add( edgePoints[ i ] );
					}

					isFirstEdge = false;
				}

				return vertices.Count >= 3;
			}
			catch {
				return false;
			}
		}

		static void BuildWallDebugCompound( ref WallSupportData wall )
		{
			try {
				TopoDS_Compound compound = new TopoDS_Compound();
				TopoDS_Shape compoundShape = compound;
				BRep_Builder builder = new BRep_Builder();
				builder.MakeCompound( ref compound );

				foreach( TopoDS_Face face in wall.SectionFaces ) {
					builder.Add( ref compoundShape, face );
				}

				if( wall.RibPlate != null && !wall.RibPlate.IsNull() ) {
					builder.Add( ref compoundShape, wall.RibPlate );
				}

				foreach( TopoDS_Face face in wall.CrossSlotFaces ) {
					builder.Add( ref compoundShape, face );
				}

				foreach( TopoDS_Face face in wall.PinFaces ) {
					builder.Add( ref compoundShape, face );
				}

				wall.DebugCompound = compound;
			}
			catch {
			}
		}

		static bool BuildFinalViewCompound( SupportStructureResult result, out TopoDS_Shape debugCompound )
		{
			debugCompound = null;

			try {
				TopoDS_Compound compound = new TopoDS_Compound();
				TopoDS_Shape compoundShape = compound;
				BRep_Builder builder = new BRep_Builder();
				builder.MakeCompound( ref compound );

				foreach( WallSupportData wall in result.XZWalls ) {
					if( wall.FinalFace != null && !wall.FinalFace.IsNull() ) {
						builder.Add( ref compoundShape, wall.FinalFace );
					}
					else if( wall.DebugCompound != null && !wall.DebugCompound.IsNull() ) {
						builder.Add( ref compoundShape, wall.DebugCompound );
					}
				}

				foreach( WallSupportData wall in result.YZWalls ) {
					if( wall.FinalFace != null && !wall.FinalFace.IsNull() ) {
						builder.Add( ref compoundShape, wall.FinalFace );
					}
					else if( wall.DebugCompound != null && !wall.DebugCompound.IsNull() ) {
						builder.Add( ref compoundShape, wall.DebugCompound );
					}
				}

				// add base plate
				if( result.BasePlate != null ) {
					if( result.BasePlate.FinalFace != null && !result.BasePlate.FinalFace.IsNull() ) {
						builder.Add( ref compoundShape, result.BasePlate.FinalFace );
					}
				}

				debugCompound = compound;
				return true;
			}
			catch {
				return false;
			}
		}

		static bool ComputeWallFinalFace( WallSupportData wall, double filletRadius )
		{
			try {
				if( wall == null ) {
					return false;
				}

				WallPlaneConfig config = wall.IsXZPlane
					? new WallPlaneConfig { AxisU = 0, FixedCoord = wall.FixedCoord, CutTopSide = false }
					: new WallPlaneConfig { AxisU = 1, FixedCoord = wall.FixedCoord, CutTopSide = true };

				// step 1: convert all 3D polygons to 2D Clipper paths
				PathsD ribPath = Convert3DPolygonTo2DPath( wall.RibPlateVertices, config );
				if( ribPath == null || ribPath.Count == 0 ) {
					return false;
				}


				// section faces
				List<PathsD> sectionPaths = new List<PathsD>();
				foreach( var vertices in wall.SectionFacesVertices ) {
					PathsD path = Convert3DPolygonTo2DPath( vertices, config );
					if( path != null && path.Count > 0 ) {
						sectionPaths.Add( path );
					}
				}

				// cross slot faces
				List<PathsD> crossSlotPaths = new List<PathsD>();
				foreach( var vertices in wall.CrossSlotFacesVertices ) {
					PathsD path = Convert3DPolygonTo2DPath( vertices, config );
					if( path != null && path.Count > 0 ) {
						crossSlotPaths.Add( path );
					}
				}

				// pin faces
				List<PathsD> pinPaths = new List<PathsD>();
				foreach( var vertices in wall.PinFacesVertices ) {
					PathsD path = Convert3DPolygonTo2DPath( vertices, config );
					if( path != null && path.Count > 0 ) {
						pinPaths.Add( path );
					}
				}

				// step 2: perform boolean union starting from rib plate
				PathsD unionResult = ribPath;

				// union with section faces
				foreach( PathsD sectionPath in sectionPaths ) {
					unionResult = Clipper.Union( unionResult, sectionPath, FillRule.NonZero );
					if( unionResult == null || unionResult.Count == 0 ) {
						return false;
					}
				}

				// union with pins
				foreach( PathsD pinPath in pinPaths ) {
					unionResult = Clipper.Union( unionResult, pinPath, FillRule.NonZero );
					if( unionResult == null || unionResult.Count == 0 ) {
						return false;
					}
				}

				// subtract all cross slots
				foreach( PathsD slotPath in crossSlotPaths ) {
					unionResult = Clipper.Difference( unionResult, slotPath, FillRule.NonZero );
					if( unionResult == null || unionResult.Count == 0 ) {
						return false;
					}
				}

				// step 3: keep the largest island
				PathD largestIsland = GetLargestIsland( unionResult );
				if( largestIsland == null || largestIsland.Count < 3 ) {
					return false;
				}

				// step 4: simplify the polygon
				PathsD simplifiedPaths = new PathsD();
				simplifiedPaths.Add( largestIsland );
				simplifiedPaths = Clipper.SimplifyPaths( simplifiedPaths, CHORD_ERROR_TOLERANCE );
				if( simplifiedPaths == null || simplifiedPaths.Count == 0 || simplifiedPaths[ 0 ].Count < 3 ) {
					return false;
				}

				// step 5: apply fillet to the polygon corners
				PathD filletedPath = ApplyFilletToPath( simplifiedPaths[ 0 ], filletRadius );
				if( filletedPath == null || filletedPath.Count < 3 ) {
					return false;
				}

				// step 6: store filleted path for DXF export
				wall.FilletedPath2D = filletedPath;

				// step 7: convert back to 3D and build final face
				List<gp_Pnt> finalVertices = Convert2DPathTo3DPolygon( filletedPath, config );
				if( finalVertices == null || finalVertices.Count < 3 ) {
					return false;
				}

				// step 8: build wire and face
				TopoDS_Wire finalWire;
				if( !BuildWireFromPoints( finalVertices, out finalWire ) ) {
					return false;
				}

				TopoDS_Face finalFace;
				if( !TryBuildPlaneFace( finalWire, out finalFace ) ) {
					return false;
				}

				wall.FinalFace = finalFace;
				return true;
			}
			catch {
				return false;
			}
		}

		static PathD ApplyFilletToPath( PathD inputPath, double filletRadius )
		{
			if( inputPath == null || inputPath.Count < 3 || filletRadius <= GEOMETRY_TOLERANCE ) {
				return inputPath;
			}

			// this is the inital design by copilot
			try {
				const double MIN_HANDLE_ANGLE_RAD = 60.0 * Math.PI / 180.0;  // 60 degrees
				const double MAX_HANDLE_ANGLE_RAD = 120.0 * Math.PI / 180.0; // 120 degrees
				const double MIN_EDGE_LENGTH_FACTOR = 2.0;
				const double MAX_DEFLECTION = CHORD_ERROR_TOLERANCE; // Consistent with DiscreteWireToVertices
				const double MAX_FILLETS_DISCRETE_ANGLE_DEG = 12.0; // Maximum angle step for fillet discretization

				// Calculate arc point angle step based on chord height error
				double angleStep = 2.0 * Math.Acos( 1.0 - MAX_DEFLECTION / filletRadius );

				// Limit to max 12 degrees
				angleStep = Math.Min( angleStep, MAX_FILLETS_DISCRETE_ANGLE_DEG * Math.PI / 180.0 );

				PathD newPath = new PathD();
				int vertexCount = inputPath.Count;

				for( int i = 0; i < vertexCount; i++ ) {
					int prevIdx = ( i - 1 + vertexCount ) % vertexCount;
					int currIdx = i;
					int nextIdx = ( i + 1 ) % vertexCount;

					PointD prevPt = inputPath[ prevIdx ];
					PointD currPt = inputPath[ currIdx ];
					PointD nextPt = inputPath[ nextIdx ];

					// Calculate vectors from current vertex to neighbors
					double v1x = prevPt.x - currPt.x;
					double v1y = prevPt.y - currPt.y;
					double v2x = nextPt.x - currPt.x;
					double v2y = nextPt.y - currPt.y;

					double len1 = Math.Sqrt( v1x * v1x + v1y * v1y );
					double len2 = Math.Sqrt( v2x * v2x + v2y * v2y );

					// Check if edge lengths are sufficient
					if( len1 < filletRadius * MIN_EDGE_LENGTH_FACTOR || len2 < filletRadius * MIN_EDGE_LENGTH_FACTOR ) {
						// Edge too short, keep original vertex
						newPath.Add( currPt );
						continue;
					}

					// Normalize vectors
					v1x /= len1;
					v1y /= len1;
					v2x /= len2;
					v2y /= len2;

					// Calculate angle using dot product
					double dot = v1x * v2x + v1y * v2y;
					dot = Math.Max( -1.0, Math.Min( 1.0, dot ) ); // Clamp to [-1, 1]
					double angle = Math.Acos( dot );

					// Check if angle is within processing range (60-120 degrees)
					if( angle < MIN_HANDLE_ANGLE_RAD || angle > MAX_HANDLE_ANGLE_RAD ) {
						// Angle out of range, keep original vertex
						newPath.Add( currPt );
						continue;
					}

					// Calculate arc start and end positions on edges
					double halfAngle = angle / 2.0;
					double distToTangent = filletRadius / Math.Tan( halfAngle );

					// Arc start point (on prev -> curr edge)
					PointD arcStart = new PointD(
						currPt.x + v1x * distToTangent,
						currPt.y + v1y * distToTangent
					);

					// Arc end point (on curr -> next edge)
					PointD arcEnd = new PointD(
						currPt.x + v2x * distToTangent,
						currPt.y + v2y * distToTangent
					);

					// Add arc start point
					newPath.Add( arcStart );

					// Calculate arc center position
					double bisectorX = v1x + v2x;
					double bisectorY = v1y + v2y;
					double bisectorLen = Math.Sqrt( bisectorX * bisectorX + bisectorY * bisectorY );
					bisectorX /= bisectorLen;
					bisectorY /= bisectorLen;

					double distToCenter = filletRadius / Math.Sin( halfAngle );
					PointD center = new PointD(
						currPt.x + bisectorX * distToCenter,
						currPt.y + bisectorY * distToCenter
					);

					// Calculate start and end angles
					double startAngle = Math.Atan2( arcStart.y - center.y, arcStart.x - center.x );
					double endAngle = Math.Atan2( arcEnd.y - center.y, arcEnd.x - center.x );

					// Calculate sweep angle (take inner arc)
					// Use cross product to determine rotation direction
					double cross = v1x * v2y - v1y * v2x;
					double sweepAngle;
					if( cross > 0 ) {
						// Counter-clockwise rotation, arc should go clockwise (inner side)
						sweepAngle = endAngle - startAngle;
						if( sweepAngle > 0 ) {
							sweepAngle -= 2.0 * Math.PI;
						}
					}
					else {
						// Clockwise rotation, arc should go counter-clockwise (inner side)
						sweepAngle = endAngle - startAngle;
						if( sweepAngle < 0 ) {
							sweepAngle += 2.0 * Math.PI;
						}
					}

					// Generate intermediate points on the arc (excluding start and end)
					int numSegments = (int)Math.Ceiling( Math.Abs( sweepAngle ) / angleStep );
					if( numSegments < 1 ) {
						numSegments = 1;
					}

					for( int j = 1; j < numSegments; j++ ) {
						double t = (double)j / numSegments;
						double currentAngle = startAngle + sweepAngle * t;
						PointD arcPoint = new PointD(
							center.x + filletRadius * Math.Cos( currentAngle ),
							center.y + filletRadius * Math.Sin( currentAngle )
						);
						newPath.Add( arcPoint );
					}

					// Add arc end point
					newPath.Add( arcEnd );
				}

				return newPath.Count >= 3 ? newPath : inputPath;
			}
			catch {
				return inputPath; // Return original path on error
			}
		}

		static PathsD Convert3DPolygonTo2DPath( List<gp_Pnt> vertices, WallPlaneConfig config )
		{
			if( vertices == null || vertices.Count < 3 ) {
				return null;
			}

			try {
				PathD path = new PathD();
				foreach( gp_Pnt pt in vertices ) {
					PointD pt2d = Project3DPointTo2D( pt, config );
					path.Add( pt2d );
				}

				PathsD paths = new PathsD();
				paths.Add( path );
				return paths;
			}
			catch {
				return null;
			}
		}

		static PointD Project3DPointTo2D( gp_Pnt pt3d, WallPlaneConfig config )
		{
			if( config.AxisU == 0 ) {  // XZ plane
				return new PointD( pt3d.X(), pt3d.Z() );
			}
			else {  // YZ plane
				return new PointD( pt3d.Y(), pt3d.Z() );
			}
		}

		static List<gp_Pnt> Convert2DPathTo3DPolygon( PathD path, WallPlaneConfig config )
		{
			if( path == null || path.Count < 3 ) {
				return null;
			}

			try {
				List<gp_Pnt> vertices = new List<gp_Pnt>();
				foreach( PointD pt2d in path ) {
					gp_Pnt pt3d = Unproject2DPointTo3D( pt2d, config );
					vertices.Add( pt3d );
				}
				return vertices;
			}
			catch {
				return null;
			}
		}

		static gp_Pnt Unproject2DPointTo3D( PointD pt2d, WallPlaneConfig config )
		{
			if( config.AxisU == 0 ) {  // XZ plane: U=X, V=Z, fixed Y
				return new gp_Pnt( pt2d.x, config.FixedCoord, pt2d.y );
			}
			else {  // YZ plane: U=Y, V=Z, fixed X
				return new gp_Pnt( config.FixedCoord, pt2d.x, pt2d.y );
			}
		}

		static PathD GetLargestIsland( PathsD paths )
		{
			if( paths == null || paths.Count == 0 ) {
				return null;
			}

			PathD largest = null;
			double largestArea = 0.0;

			foreach( PathD path in paths ) {
				double area = Math.Abs( Clipper.Area( path ) );
				if( area > largestArea ) {
					largestArea = area;
					largest = path;
				}
			}

			return largest;
		}

		static bool BuildBasePlate(
			double[] xPosList,
			double[] yPosList,
			double xmin, double xmax, double ymin, double ymax,
			double xE, double yE,
			double zbottom,
			double baseThick,
			double wallThick,
			double pinWidth,
			int xPinCount,
			int yPinCount,
			List<WallSupportData> xzWalls,
			List<WallSupportData> yzWalls,
			BasePlateData basePlate )
		{
			try {
				double fixedZ = zbottom;
				basePlate.FixedZ = fixedZ;

				// 1. Build outer contour vertices (CCW from +Z)
				gp_Pnt p1 = new gp_Pnt( xmax + xE, ymax + yE, fixedZ );
				gp_Pnt p2 = new gp_Pnt( xmin - xE, ymax + yE, fixedZ );
				gp_Pnt p3 = new gp_Pnt( xmin - xE, ymin - yE, fixedZ );
				gp_Pnt p4 = new gp_Pnt( xmax + xE, ymin - yE, fixedZ );
				basePlate.OuterVertices = new List<gp_Pnt> { p1, p2, p3, p4 };

				// 2. Calculate pin slot vertices (from XZ walls)
				double xBase = ( xmax + xE ) - ( xmin - xE );
				double yBase = ( ymax + yE ) - ( ymin - yE );

				foreach( WallSupportData xzWall in xzWalls ) {
					double yFixed = xzWall.FixedCoord;
					double xStep = xBase / ( xPinCount + 1 );

					for( int m = 1; m <= xPinCount; m++ ) {
						double xPin = ( xmin - xE ) + m * xStep;
						double halfPinWidth = pinWidth / 2.0;
						double halfWallThick = wallThick / 2.0;

						// CW order (for holes)
						gp_Pnt s1 = new gp_Pnt( xPin + halfPinWidth, yFixed + halfWallThick, fixedZ );
						gp_Pnt s2 = new gp_Pnt( xPin + halfPinWidth, yFixed - halfWallThick, fixedZ );
						gp_Pnt s3 = new gp_Pnt( xPin - halfPinWidth, yFixed - halfWallThick, fixedZ );
						gp_Pnt s4 = new gp_Pnt( xPin - halfPinWidth, yFixed + halfWallThick, fixedZ );

						basePlate.PinSlotVertices.Add( new List<gp_Pnt> { s1, s2, s3, s4 } );
					}
				}

				// 3. Calculate pin slot vertices (from YZ walls)
				foreach( WallSupportData yzWall in yzWalls ) {
					double xFixed = yzWall.FixedCoord;
					double yStep = yBase / ( yPinCount + 1 );

					for( int n = 1; n <= yPinCount; n++ ) {
						double yPin = ( ymin - yE ) + n * yStep;
						double halfPinWidth = pinWidth / 2.0;
						double halfWallThick = wallThick / 2.0;

						// CW order (for holes)
						gp_Pnt s1 = new gp_Pnt( xFixed + halfWallThick, yPin + halfPinWidth, fixedZ );
						gp_Pnt s2 = new gp_Pnt( xFixed + halfWallThick, yPin - halfPinWidth, fixedZ );
						gp_Pnt s3 = new gp_Pnt( xFixed - halfWallThick, yPin - halfPinWidth, fixedZ );
						gp_Pnt s4 = new gp_Pnt( xFixed - halfWallThick, yPin + halfPinWidth, fixedZ );

						basePlate.PinSlotVertices.Add( new List<gp_Pnt> { s1, s2, s3, s4 } );
					}
				}

				// 4. Build 3D Face (no fillet, for visualization)
				TopoDS_Wire outerWire;
				if( !BuildWireFromPoints( basePlate.OuterVertices, out outerWire ) ) {
					return false;
				}

				BRepBuilderAPI_MakeFace faceMaker = new BRepBuilderAPI_MakeFace( outerWire );

				foreach( var slotVertices in basePlate.PinSlotVertices ) {
					TopoDS_Wire holeWire;
					if( BuildWireFromPoints( slotVertices, out holeWire ) ) {
						faceMaker.Add( holeWire );  // OCC handles direction automatically
					}
				}

				if( !faceMaker.IsDone() ) {
					return false;
				}

				basePlate.FinalFace = faceMaker.Face();
				return true;
			}
			catch {
				return false;
			}
		}

		static bool ComputeBasePlateFinalFace( BasePlateData basePlate, double filletRadius )
		{
			try {
				if( basePlate == null ) {
					return false;
				}

				double fixedZ = basePlate.FixedZ;

				// ===== Step 1: 3D → 2D projection =====
				PathD outerPath2D = new PathD();
				foreach( gp_Pnt pt in basePlate.OuterVertices ) {
					outerPath2D.Add( new PointD( pt.X(), pt.Y() ) );
				}

				List<PathD> holePaths2D = new List<PathD>();
				foreach( var slotVertices in basePlate.PinSlotVertices ) {
					PathD holePath = new PathD();
					foreach( gp_Pnt pt in slotVertices ) {
						holePath.Add( new PointD( pt.X(), pt.Y() ) );
					}
					holePaths2D.Add( holePath );
				}

				// ===== Step 2: 2D boolean operations =====
				PathsD outerPathsD = new PathsD();
				outerPathsD.Add( outerPath2D );

				PathsD resultPaths = outerPathsD;
				foreach( PathD holePath in holePaths2D ) {
					PathsD holePathsD = new PathsD();
					holePathsD.Add( holePath );
					resultPaths = Clipper.Difference( resultPaths, holePathsD, FillRule.NonZero );

					if( resultPaths == null || resultPaths.Count == 0 ) {
						return false;  // Completely consumed by holes
					}
				}

				// ===== Step 3 & 4: Separate outer contour and inner contours =====
				var sortedPaths = resultPaths.OrderByDescending( p => Math.Abs( Clipper.Area( p ) ) ).ToList();

				PathD outerPath = sortedPaths[ 0 ];
				if( Clipper.Area( outerPath ) < 0 ) {
					outerPath.Reverse();  // Ensure CCW
				}

				List<PathD> innerPaths = new List<PathD>();
				for( int i = 1; i < sortedPaths.Count; i++ ) {
					PathD innerPath = sortedPaths[ i ];
					if( Clipper.Area( innerPath ) > 0 ) {
						innerPath.Reverse();  // Ensure CW
					}
					innerPaths.Add( innerPath );
				}

				// ===== Step 5: Simplify and fillet =====
				// Outer contour
				PathsD outerSimplifiedPaths = Clipper.SimplifyPaths( new PathsD { outerPath }, CHORD_ERROR_TOLERANCE );
				if( outerSimplifiedPaths == null || outerSimplifiedPaths.Count == 0 ) {
					return false;
				}
				PathD outerSimplified = outerSimplifiedPaths[ 0 ];

				PathD outerFilleted = ApplyFilletToPath( outerSimplified, filletRadius );
				if( outerFilleted == null || outerFilleted.Count < 3 ) {
					return false;
				}
				basePlate.OuterFilletedPath2D = outerFilleted;

				// Inner contours
				basePlate.PinSlotFilletedPaths2D.Clear();
				foreach( PathD innerPath in innerPaths ) {
					PathsD innerSimplifiedPaths = Clipper.SimplifyPaths( new PathsD { innerPath }, CHORD_ERROR_TOLERANCE );
					if( innerSimplifiedPaths == null || innerSimplifiedPaths.Count == 0 ) {
						continue;
					}
					PathD innerSimplified = innerSimplifiedPaths[ 0 ];

					// Reverse → Fillet → Reverse (for CW holes)
					innerSimplified.Reverse();
					PathD innerFilletedCCW = ApplyFilletToPath( innerSimplified, filletRadius );
					if( innerFilletedCCW == null || innerFilletedCCW.Count < 3 ) {
						continue;
					}
					innerFilletedCCW.Reverse();

					basePlate.PinSlotFilletedPaths2D.Add( innerFilletedCCW );
				}

				// ===== Step 6: 2D → 3D conversion =====
				List<gp_Pnt> outerVertices3D = new List<gp_Pnt>();
				foreach( PointD pt in outerFilleted ) {
					outerVertices3D.Add( new gp_Pnt( pt.x, pt.y, fixedZ ) );
				}

				List<List<gp_Pnt>> innerVertices3D = new List<List<gp_Pnt>>();
				foreach( PathD innerFilleted in basePlate.PinSlotFilletedPaths2D ) {
					List<gp_Pnt> vertices = new List<gp_Pnt>();
					foreach( PointD pt in innerFilleted ) {
						vertices.Add( new gp_Pnt( pt.x, pt.y, fixedZ ) );
					}
					innerVertices3D.Add( vertices );
				}

				// ===== Step 7: Rebuild 3D Face =====
				TopoDS_Wire outerWire;
				if( !BuildWireFromPoints( outerVertices3D, out outerWire ) ) {
					return false;
				}

				BRepBuilderAPI_MakeFace faceMaker = new BRepBuilderAPI_MakeFace( outerWire );

				foreach( var innerVertices in innerVertices3D ) {
					TopoDS_Wire innerWire;
					if( BuildWireFromPoints( innerVertices, out innerWire ) ) {
						faceMaker.Add( innerWire );
					}
				}

				if( !faceMaker.IsDone() ) {
					return false;
				}

				basePlate.FinalFace = faceMaker.Face();
				return true;
			}
			catch {
				return false;
			}
		}

		static bool BuildWireFromPoints( List<gp_Pnt> points, out TopoDS_Wire wire )
		{
			wire = null;
			try {
				if( points == null || points.Count < 3 ) {
					return false;
				}

				BRepBuilderAPI_MakePolygon polyMaker = new BRepBuilderAPI_MakePolygon();
				foreach( gp_Pnt pt in points ) {
					polyMaker.Add( pt );
				}
				polyMaker.Close();

				if( !polyMaker.IsDone() ) {
					return false;
				}

				wire = polyMaker.Wire();
				return wire != null && !wire.IsNull();
			}
			catch {
				return false;
			}
		}

		public static bool ExportSupportsToDxf( SupportStructureResult result, string outputDirectory )
		{
			if( result == null || string.IsNullOrEmpty( outputDirectory ) ) {
				return false;
			}

			try {
				// Create directory if not exists
				if( !Directory.Exists( outputDirectory ) ) {
					Directory.CreateDirectory( outputDirectory );
				}

				// Export XZ walls
				for( int i = 0; i < result.XZWalls.Count; i++ ) {
					WallSupportData wall = result.XZWalls[ i ];
					if( wall.FilletedPath2D != null && wall.FilletedPath2D.Count >= 3 ) {
						string fileName = Path.Combine( outputDirectory, $"XZ_Wall_{i}.dxf" );
						ExportPathToDxf( wall.FilletedPath2D, fileName );
					}
				}

				// Export YZ walls
				for( int j = 0; j < result.YZWalls.Count; j++ ) {
					WallSupportData wall = result.YZWalls[ j ];
					if( wall.FilletedPath2D != null && wall.FilletedPath2D.Count >= 3 ) {
						string fileName = Path.Combine( outputDirectory, $"YZ_Wall_{j}.dxf" );
						ExportPathToDxf( wall.FilletedPath2D, fileName );
					}
				}

				// Export base plate
				if( result.BasePlate != null ) {
					string fileName = Path.Combine( outputDirectory, "Base.dxf" );
					ExportBasePlateToDxf( result.BasePlate, fileName );
				}

				return true;
			}
			catch {
				return false;
			}
		}

		static bool ExportPathToDxf( PathD path, string fileName )
		{
			if( path == null || path.Count < 3 ) {
				return false;
			}

			try {
				// Create DXF document
				DxfDocument dxfDoc = new DxfDocument();

				// Convert PathD to Vector2 list
				List<Vector2> vertices = new List<Vector2>();
				foreach( PointD pt in path ) {
					vertices.Add( new Vector2( pt.x, pt.y ) );
				}

				// Create closed Polyline2D
				Polyline2D polyline = new Polyline2D( vertices, isClosed: true );

				// Add to document
				dxfDoc.Entities.Add( polyline );

				// Save to file
				return dxfDoc.Save( fileName );
			}
			catch {
				return false;
			}
		}

		static bool ExportBasePlateToDxf( BasePlateData basePlate, string fileName )
		{
			if( basePlate == null ) {
				return false;
			}

			try {
				// Create DXF document
				DxfDocument dxfDoc = new DxfDocument();

				// Create layers
				Layer outerLayer = new Layer( "OUTER" );
				Layer holesLayer = new Layer( "HOLES" );

				// Export outer contour
				if( basePlate.OuterFilletedPath2D != null && basePlate.OuterFilletedPath2D.Count >= 3 ) {
					List<Vector2> outerVertices = new List<Vector2>();
					foreach( PointD pt in basePlate.OuterFilletedPath2D ) {
						outerVertices.Add( new Vector2( pt.x, pt.y ) );
					}
					Polyline2D outerPolyline = new Polyline2D( outerVertices, isClosed: true );
					outerPolyline.Layer = outerLayer;
					dxfDoc.Entities.Add( outerPolyline );
				}

				// Export pin slots (holes)
				foreach( PathD slotPath in basePlate.PinSlotFilletedPaths2D ) {
					if( slotPath != null && slotPath.Count >= 3 ) {
						List<Vector2> slotVertices = new List<Vector2>();
						foreach( PointD pt in slotPath ) {
							slotVertices.Add( new Vector2( pt.x, pt.y ) );
						}
						Polyline2D slotPolyline = new Polyline2D( slotVertices, isClosed: true );
						slotPolyline.Layer = holesLayer;
						dxfDoc.Entities.Add( slotPolyline );
					}
				}

				// Save to file
				return dxfDoc.Save( fileName );
			}
			catch {
				return false;
			}
		}

		const double GEOMETRY_TOLERANCE = 0.001;

		private const double CHORD_ERROR_TOLERANCE = 0.01;

		public static bool ComputePreviewPlanes(
				TopoDS_Shape workpieceShape,
				double[] xPosList,
				double[] yPosList,
				double xE,
				double yE,
				double zE,
				double ribHeight,
				out List<PreviewPlaneData> xzWallPlanes,
				out List<PreviewPlaneData> yzWallPlanes,
				out PreviewPlaneData basePlanePreview )
		{
			xzWallPlanes = new List<PreviewPlaneData>();
			yzWallPlanes = new List<PreviewPlaneData>();
			basePlanePreview = null;

			try {
				if( workpieceShape == null || workpieceShape.IsNull() ) {
					return false;
				}

				Bnd_Box bbox = new Bnd_Box();
				BRepBndLib.AddOptimal( workpieceShape, ref bbox );
				double xmin = 0, ymin = 0, zmin = 0, xmax = 0, ymax = 0, zmax = 0;
				bbox.Get( ref xmin, ref ymin, ref zmin, ref xmax, ref ymax, ref zmax );

				double zbottom = zmin - zE;
				double zRibTop = zbottom + ribHeight;

				// XZ walls: one wall per yPos, spanning X (workpiece range + xE)
				if( yPosList != null ) {
					foreach( double yPos in yPosList ) {
						AddWallSegments( xzWallPlanes, yPos, xmin, xmax, xE, zbottom, zmax, zRibTop );
					}
				}

				// YZ walls: one wall per xPos, spanning Y (workpiece range + yE)
				if( xPosList != null ) {
					foreach( double xPos in xPosList ) {
						AddWallSegments( yzWallPlanes, xPos, ymin, ymax, yE, zbottom, zmax, zRibTop );
					}
				}

				// base plate: rectangular plane at zbottom, spanning X/Y (workpiece range + xE/yE)
				basePlanePreview = new PreviewPlaneData
				{
					FixedCoord = zbottom,
					UMin = xmin - xE,
					UMax = xmax + xE,
					VMin = ymin - yE,
					VMax = ymax + yE,
				};

				return true;
			}
			catch {
				return false;
			}
		}

		// builds up to 3 rectangle segments for a single wall along its U span
		static void AddWallSegments( List<PreviewPlaneData> wallPlanes, double fixedCoord,
			double uMin, double uMax, double uExtension,
			double zbottom, double zTop, double zRibTop )
		{
			// left/bottom extension segment (rib height only)
			if( uExtension > GEOMETRY_TOLERANCE ) {
				wallPlanes.Add( new PreviewPlaneData
				{
					FixedCoord = fixedCoord,
					UMin = uMin - uExtension,
					UMax = uMin,
					VMin = zbottom,
					VMax = zRibTop,
				} );
			}

			// middle segment (within workpiece bounding box range, full height)
			wallPlanes.Add( new PreviewPlaneData
			{
				FixedCoord = fixedCoord,
				UMin = uMin,
				UMax = uMax,
				VMin = zbottom,
				VMax = zTop,
			} );

			// right/top extension segment (rib height only)
			if( uExtension > GEOMETRY_TOLERANCE ) {
				wallPlanes.Add( new PreviewPlaneData
				{
					FixedCoord = fixedCoord,
					UMin = uMax,
					UMax = uMax + uExtension,
					VMin = zbottom,
					VMax = zRibTop,
				} );
			}
		}
	}

	// simple rectangular plane extents for preview purposes only
	public class PreviewPlaneData
	{
		public double FixedCoord
		{
			get; set;
		}

		public double UMin
		{
			get; set;
		}

		public double UMax
		{
			get; set;
		}

		public double VMin
		{
			get; set;
		}

		public double VMax
		{
			get; set;
		}
	}
}
