using OCC.Bnd;
using OCC.BRepAdaptor;
using OCC.BRepBndLib;
using OCC.BRepBuilderAPI;
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
	internal static class RingShapedIdentifyHelper
	{
		public static bool ComputeRevolutionToG54Transform( TopoDS_Shape oneShape, out gp_Trsf accumulatedTrsf )
		{
			accumulatedTrsf = new gp_Trsf();
			bool isSuccess = GetRevolutionDirAndCenter( oneShape, out gp_Dir axisDir, out gp_Pnt centerPnt, out bool isFittingByCircle );

			if( !isSuccess ) {
				return false;
			}

			// Step 1: Align axis to Z and center to XY
			bool isDone = AlignAxisToZAndCenterXYWithTransform( oneShape, axisDir, centerPnt, out TopoDS_Shape shape, out gp_Trsf trsf1 );
			if( !isDone ) {
				return false;
			}
			accumulatedTrsf = trsf1;

			// Step 2: Check if need to flip upside down
			bool isSliceDone = AnalyzeRevolutionShapeTaper( shape, out bool isTopWiderThanBottom );
			if( isSliceDone && isTopWiderThanBottom ) {
				if( ShapeTool.FlipShapeUpsideDown( shape, out TopoDS_Shape flippedShape, out gp_Trsf trsf2, null ) ) {
					if( flippedShape != null ) {
						shape = flippedShape;
						accumulatedTrsf = trsf2.Multiplied( accumulatedTrsf );
					}
				}
			}

			// Step 3: Move bottom to Z=0
			if( ShapeTool.MoveShapeBottomToZ0( shape, out TopoDS_Shape MovedShape, out gp_Trsf trsf3 ) ) {
				shape = MovedShape;
				accumulatedTrsf = trsf3.Multiplied( accumulatedTrsf );
			}

			// Step 4: Align OBB to XY axes if not fitting by circle
			if( isFittingByCircle == false ) {
				if( ShapeTool.AlignOBBToXYAxes( shape, out _, out gp_Trsf trsf4 ) ) {
					accumulatedTrsf = trsf4.Multiplied( accumulatedTrsf );
				}
			}
			return true;
		}

		const double OUT_LinerRatio = 0.8;

		// 5 degree
		const double AXIS_TOLERANCE = 0.087;
		const double ACCURACY_Threshold = 1e-3;

		#region Get Revolution Axis Direction

		static bool GetRevolutionDirAndCenter( TopoDS_Shape partShape,
			out gp_Dir axisDir,
			out gp_Pnt centerPnt,
			out bool isFittingByCircle )
		{
			centerPnt = null;
			List<TopoDS_Wire> wireOrderList = new List<TopoDS_Wire>();

			bool bGetDirSuccess = GetRevolutionDirSuccess( partShape, out axisDir, out TopoDS_Wire majorWire, out wireOrderList, out isFittingByCircle, out gp_Pnt circleCenter );
			if( !bGetDirSuccess ) {
				return false;
			}

			if( isFittingByCircle && circleCenter != null ) {
				centerPnt = circleCenter;
				return true;
			}
			TopoDS_Wire minerWire = new TopoDS_Wire();
			if( wireOrderList != null && wireOrderList.Count > 1 ) {
				minerWire = wireOrderList[ 1 ];
			}
			else {
				minerWire = wireOrderList.First();
			}

			bool isGetCenterSuccess = FindRevolutionCenter( majorWire, minerWire, axisDir, out centerPnt, out isFittingByCircle );
			if( isGetCenterSuccess == false ) {
				return false;
			}
			return true;
		}

		static bool GetRevolutionDirSuccess( TopoDS_Shape partShape,
			out gp_Dir axisDir,
			out TopoDS_Wire majorWire,
			out List<TopoDS_Wire> wireOrderList,
			out bool isFittingByCircle,
			out gp_Pnt circleCenter )
		{
			axisDir = new gp_Dir();
			majorWire = new TopoDS_Wire();
			wireOrderList = new List<TopoDS_Wire>();
			isFittingByCircle = false;
			circleCenter = new gp_Pnt();

			// quick input validation
			if( partShape == null || partShape.IsNull() ) {
				return false;
			}
			bool isGetRingSuccess = GetRingFace( partShape, out List<TopoDS_Shape> closedShellList );
			if( !isGetRingSuccess || closedShellList == null || closedShellList.Count == 0 ) {
				return false;
			}
			bool GetBiggestShellSuccess = GetBiggestShell( closedShellList, out TopoDS_Shape biggestShell );

			if( !GetBiggestShellSuccess || biggestShell == null || biggestShell.IsNull() ) {
				return false;
			}

			// get longest wire
			if( !GetSortedWiresInShell( biggestShell, out wireOrderList ) ) {
				return false;
			}
			if( wireOrderList == null || wireOrderList.Count == 0 ) {
				return false;
			}

			majorWire = wireOrderList.First();
			bool isCircularWire = IsWireCircular( majorWire, out axisDir, out circleCenter );

			// if wire is detected as circular, validate with sample normals
			if( isCircularWire && axisDir != null ) {
				isFittingByCircle = true;
				return true;
			}

			// get sample normals (needed for validation or fallback methods)
			GetSampleNormalAndPoint( majorWire, biggestShell, out List<gp_Pnt> samplePoints, out List<gp_Dir> sampleNormals );

			// if wire almost on plane, use average normal
			if( IsWireAllmostFromPlane( majorWire, biggestShell, out axisDir ) ) {
				isFittingByCircle = false;
				return true;
			}

			bool isSuccess = FitAxisDirectionByRANSAC( samplePoints, sampleNormals, out axisDir, 1 );
			return isSuccess;
		}

		static bool GetRingFace( TopoDS_Shape rawShape, out List<TopoDS_Shape> closedShellList )
		{
			TopoDS_Shape sewedShape = ShapeTool.SewShape( rawShape );

			// get sewedFace
			List<TopoDS_Face> sewedFaceList = GetFaceList( sewedShape );

			// face with XYZ max/min point might be outer face
			List<TopoDS_Face> extremeFaceList = GetCoordExtremeFace( sewedFaceList );

			// get tube outer faces
			if( !GetShellListFromPart( rawShape, extremeFaceList, out closedShellList ) ) {
				return false;
			}
			return true;
		}

		static List<TopoDS_Face> GetFaceList( TopoDS_Shape shape )
		{
			List<TopoDS_Face> faceList = new List<TopoDS_Face>();
			TopExp_Explorer faceExplorer = new TopExp_Explorer();
			faceExplorer.Init( shape, TopAbs_ShapeEnum.TopAbs_FACE );
			while( faceExplorer.More() ) {
				TopoDS_Face face = TopoDS.ToFace( faceExplorer.Current() );
				if( face != null ) {
					faceList.Add( face );
				}
				faceExplorer.Next();
			}
			return faceList;
		}

		static List<TopoDS_Face> GetCoordExtremeFace( List<TopoDS_Face> tubeFaceList )
		{
			double xmin = double.MaxValue, xmax = double.MinValue;
			double ymin = double.MaxValue, ymax = double.MinValue;
			double zmin = double.MaxValue, zmax = double.MinValue;

			// record those faces that touch the extreme values ​​of XYZ
			Dictionary<AxisExtreme, List<TopoDS_Face>> result = new Dictionary<AxisExtreme, List<TopoDS_Face>>();

			// list for return, do not need extreme key because any extreme face may be the outer face
			List<TopoDS_Face> extremeFaceList = new List<TopoDS_Face>();
			foreach( TopoDS_Face face in tubeFaceList ) {
				// get the bounding box of the face
				BoundingBox faceBoundingBox = new BoundingBox( face );

				// check if this face is outside the extreme value
				UpdateExtremeFace( result, AxisExtreme.XMin, faceBoundingBox.Xmin, face, true, ref xmin );
				UpdateExtremeFace( result, AxisExtreme.XMax, faceBoundingBox.Xmax, face, false, ref xmax );
				UpdateExtremeFace( result, AxisExtreme.YMin, faceBoundingBox.Ymin, face, true, ref ymin );
				UpdateExtremeFace( result, AxisExtreme.YMax, faceBoundingBox.Ymax, face, false, ref ymax );
				UpdateExtremeFace( result, AxisExtreme.ZMin, faceBoundingBox.Zmin, face, true, ref zmin );
				UpdateExtremeFace( result, AxisExtreme.ZMax, faceBoundingBox.Zmax, face, false, ref zmax );
			}

			// remove key to return
			foreach( List<TopoDS_Face> faceList in result.Values ) {
				foreach( var face in faceList ) {

					// If this face belongs to many extreme values ​​at the same time , only need to record 1 time
					if( !extremeFaceList.Any( f => f.IsEqual( face ) ) ) {
						extremeFaceList.Add( face );
					}
				}
			}
			return extremeFaceList;
		}

		static void UpdateExtremeFace( Dictionary<AxisExtreme, List<TopoDS_Face>> extremeFaceList, AxisExtreme extremeValueKey, double currentValue, TopoDS_Face face, bool isMin, ref double boundToUpdate )
		{
			// over accuracy means need to update the bound
			bool IsOverAccuracy = currentValue - boundToUpdate > 0.01;

			// havn't with this extreme key
			if( extremeFaceList.ContainsKey( extremeValueKey ) == false ) {
				extremeFaceList[ extremeValueKey ] = new List<TopoDS_Face>();
			}


			if( IsOverAccuracy ) {

				// is over min value or over max value
				if( ( isMin && currentValue < boundToUpdate ) || ( !isMin && currentValue > boundToUpdate ) ) {
					extremeFaceList[ extremeValueKey ].Clear();
					extremeFaceList[ extremeValueKey ].Add( face );
					boundToUpdate = currentValue;
				}
				return;
			}

			// is not over accuracy
			extremeFaceList[ extremeValueKey ].Add( face );
		}

		static bool GetShellListFromPart( TopoDS_Shape sewedShape, List<TopoDS_Face> extremeFaceList, out List<TopoDS_Shape> colsedShellList )
		{
			// map each edge and the corresponding face in the sew shape
			TopTools_IndexedDataMapOfShapeListOfShape indexedDataMapOfShapeListOfShape = new TopTools_IndexedDataMapOfShapeListOfShape();
			TopExp.MapShapesAndAncestors( sewedShape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref indexedDataMapOfShapeListOfShape );

			// list include the outer face candidates
			List<TopoDS_Face> D1ContinueFace = new List<TopoDS_Face>();
			colsedShellList = new List<TopoDS_Shape>();


			// the remaining number which may be the starting of outer face
			int nUnCheckedExtermeFaceCount = extremeFaceList.Count;

			TopTools_IndexedDataMapOfShapeListOfShape m_EdgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();

			// add into map
			TopExp.MapShapesAndAncestors( sewedShape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref m_EdgeFaceMap );

			List<List<TopoDS_Face>> D1ContFaceList = new List<List<TopoDS_Face>>();
			// try to find the outer face by using the extreme face
			while( nUnCheckedExtermeFaceCount > 0 ) {
				TopoDS_Face face = extremeFaceList.First();

				// protection
				if( face.IsNull() || face == null ) {
					continue;
				}

				// try to use this extreme face to get outer face
				bool isGetOuterFaceSuccess = GetFaceByD1ContinueSuccessed( face, m_EdgeFaceMap, ref indexedDataMapOfShapeListOfShape, out D1ContinueFace );

				bool isClosedShell = IsClosedShell( D1ContinueFace, out TopoDS_Shape closedShell );
				if( isGetOuterFaceSuccess && isClosedShell ) {
					colsedShellList.Add( closedShell );
				}
				// because these D1ContinueFace can't get closed outer face, so if extremeface list include some face of this must be removed
				foreach( TopoDS_Face faceInG1List in D1ContinueFace ) {
					TopoDS_Face matchedFace = extremeFaceList.FirstOrDefault( extremeface => extremeface.IsEqual( faceInG1List ) );
					if( matchedFace != null ) {
						extremeFaceList.Remove( matchedFace );
						nUnCheckedExtermeFaceCount--;
					}
				}
			}
			if( colsedShellList.Count > 0 ) {
				return true;
			}
			return false;
		}

		static bool IsWireCircular(
			TopoDS_Wire wire,
			out gp_Dir normal,
			out gp_Pnt circleCenter,
			double tolerance = ACCURACY_Threshold )
		{
			normal = null;
			circleCenter = new gp_Pnt();

			if( wire == null || wire.IsNull() ) {
				return false;
			}

			// Get all edges from wire
			List<TopoDS_Edge> edges = GetEdgesFromWire( wire );

			if( edges.Count == 0 ) {
				return false;
			}

			// Case 1: Single edge - check if it's a complete circle
			if( edges.Count == 1 ) {
				bool isValidCircle = GeometryTool.IsCircularArc( edges.First(), out circleCenter, out _, out normal );
				if( isValidCircle ) {
					return true;
				}
			}

			// Case 2: Multiple edges - check if they form a circle
			if( edges.Count > 1 ) {
				return CheckMultipleArcsFormCircle( edges, out circleCenter, out _, out normal, tolerance );
			}
			return false;
		}

		static bool CheckMultipleArcsFormCircle(
			List<TopoDS_Edge> edges,
			out gp_Pnt circleCenter,
			out double radius,
			out gp_Dir normal,
			double tolerance
		)
		{
			circleCenter = null;
			radius = 0;
			normal = null;

			if( edges == null || edges.Count < 2 ) {
				return false;
			}

			// 60% length threshold 
			const double lengthRatioThreshold = 0.6;

			// Step 1: Calculate total length of all input edges
			double totalLength = 0.0;
			foreach( TopoDS_Edge edge in edges ) {
				totalLength += CalculateEdgeLength( edge );
			}
			if( totalLength < ACCURACY_Threshold ) {
				return false;
			}

			// Step 2: Extract arc info from each edge
			List<(TopoDS_Edge edge, gp_Pnt center, double radius, gp_Dir axis)> arcInfoList = new List<(TopoDS_Edge, gp_Pnt, double, gp_Dir)>();

			foreach( TopoDS_Edge edge in edges ) {
				if( GeometryTool.IsCircularArc( edge, out gp_Pnt edgeCenter, out double edgeRadius, out gp_Dir edgeAxis ) ) {
					arcInfoList.Add( (edge, edgeCenter, edgeRadius, edgeAxis) );
				}
			}
			if( arcInfoList.Count == 0 ) {
				return false;
			}

			// Step 3: Fit centers - try to group centers that are close together
			List<List<(TopoDS_Edge edge, gp_Pnt center, double radius, gp_Dir axis)>> fittedGroups =
				new List<List<(TopoDS_Edge, gp_Pnt, double, gp_Dir)>>();

			foreach( var arcInfo in arcInfoList ) {
				bool foundGroup = false;

				// Try to find an existing group where this arc's center can fit
				foreach( var group in fittedGroups ) {
					// Calculate average center, axis and radius of this group
					gp_Pnt groupAvgCenter = CalculateAverageCenter( group );
					gp_Dir groupAvgAxis = CalculateAverageAxis( group );
					double groupAvgRadius = CalculateAverageRadius( group );

					// Check if this arc's center is close to group's average center
					double centerDistance = arcInfo.center.Distance( groupAvgCenter );
					double axisAngle = CalculateAngleBetweenDirections( arcInfo.axis, groupAvgAxis, allowOpposite: true );
					double radiusDifference = Math.Abs( arcInfo.radius - groupAvgRadius );

					if( centerDistance < 1 && axisAngle < AXIS_TOLERANCE && radiusDifference < 3 ) {
						group.Add( arcInfo );
						foundGroup = true;
						break;
					}
				}

				// If no matching group found, create a new group
				if( !foundGroup ) {
					fittedGroups.Add( new List<(TopoDS_Edge, gp_Pnt, double, gp_Dir)> { arcInfo } );
				}
			}

			// Step 4: Check each fitted group's edge length ratio
			foreach( var group in fittedGroups ) {
				if( group.Count == 0 ) {
					continue;
				}

				// Calculate total length of edges in this group
				double groupLength = 0.0;
				foreach( var arcInfo in group ) {
					groupLength += CalculateEdgeLength( arcInfo.edge );
				}

				// Calculate length ratio
				double lengthRatio = groupLength / totalLength;

				// Check if this group's length ratio exceeds 60%
				if( lengthRatio >= lengthRatioThreshold ) {
					// This group qualifies - calculate fitted center and axis
					gp_Pnt fittedCenter = CalculateAverageCenter( group );
					gp_Dir fittedAxis = CalculateAverageAxis( group );
					double avgRadius = CalculateAverageRadius( group );

					circleCenter = fittedCenter;
					radius = avgRadius;
					normal = fittedAxis;
					return true;
				}
			}

			// No qualifying arc group found
			return false;
		}

		static gp_Pnt CalculateAverageCenter( List<(TopoDS_Edge edge, gp_Pnt center, double radius, gp_Dir axis)> arcInfoList )
		{
			if( arcInfoList == null || arcInfoList.Count == 0 ) {
				return null;
			}

			double sumX = 0, sumY = 0, sumZ = 0;
			foreach( var arcInfo in arcInfoList ) {
				sumX += arcInfo.center.X();
				sumY += arcInfo.center.Y();
				sumZ += arcInfo.center.Z();
			}

			int count = arcInfoList.Count;
			return new gp_Pnt( sumX / count, sumY / count, sumZ / count );
		}

		static gp_Dir CalculateAverageAxis( List<(TopoDS_Edge edge, gp_Pnt center, double radius, gp_Dir axis)> arcInfoList )
		{
			if( arcInfoList == null || arcInfoList.Count == 0 ) {
				return null;
			}

			double sumX = 0, sumY = 0, sumZ = 0;
			foreach( var arcInfo in arcInfoList ) {
				sumX += arcInfo.axis.X();
				sumY += arcInfo.axis.Y();
				sumZ += arcInfo.axis.Z();
			}

			double length = Math.Sqrt( sumX * sumX + sumY * sumY + sumZ * sumZ );
			if( length < ACCURACY_Threshold ) {
				return arcInfoList[ 0 ].axis; // Fallback to first axis
			}

			return new gp_Dir( sumX / length, sumY / length, sumZ / length );
		}

		static double CalculateAverageRadius( List<(TopoDS_Edge edge, gp_Pnt center, double radius, gp_Dir axis)> arcInfoList )
		{
			if( arcInfoList == null || arcInfoList.Count == 0 ) {
				return 0.0;
			}

			double sumRadius = 0;
			foreach( var arcInfo in arcInfoList ) {
				sumRadius += arcInfo.radius;
			}

			return sumRadius / arcInfoList.Count;
		}

		static bool GetBiggestShell( List<TopoDS_Shape> closedShellList, out TopoDS_Shape biggestShell )
		{
			biggestShell = null;
			double maxVolume = 0.0;

			if( closedShellList == null || closedShellList.Count == 0 ) {
				return false;
			}

			// traverse each shell candidate
			foreach( TopoDS_Shape Shell in closedShellList ) {
				if( Shell == null ) {
					continue;
				}

				// calculate obb volume for this shell
				double volume = CalculateOBBVolume( Shell );

				// record shell with max volume
				if( volume > maxVolume ) {
					maxVolume = volume;
					biggestShell = Shell;
				}
			}

			// check if found valid shell
			return biggestShell != null && maxVolume > ACCURACY_Threshold;
		}

		#endregion

		#region Get Revolution Axis Center

		static bool FindRevolutionCenter( TopoDS_Wire majorWire, TopoDS_Wire minerWire, gp_Dir axisDirection, out gp_Pnt centerPnt, out bool isFittingByCircle )
		{
			centerPnt = null;
			isFittingByCircle = false;

			// fit two wires and pick higher confidence
			bool isGetMajorSuccess = TryFitWithCircleAndRant( majorWire, axisDirection, out double dConfidence_major, out gp_Pnt centerPnt_major, out bool isFittingByCircle_major );
			bool isGetMinerSuccess = TryFitWithCircleAndRant( minerWire, axisDirection, out double dConfidence_miner, out gp_Pnt centerPnt_miner, out bool isFittingByCircle_miner );

			if( isGetMajorSuccess && isGetMinerSuccess ) {

				// if both succeed, choose higher confidence
				if( dConfidence_major >= dConfidence_miner ) {
					centerPnt = centerPnt_major;
					isFittingByCircle = isFittingByCircle_major;
				}
				else {
					isFittingByCircle = isFittingByCircle_miner;
					centerPnt = centerPnt_miner;
				}
				return true;
			}

			if( isGetMajorSuccess ) {
				isFittingByCircle = isFittingByCircle_major;
				centerPnt = centerPnt_major;
				return true;
			}
			if( isGetMinerSuccess ) {
				isFittingByCircle = isFittingByCircle_miner;
				centerPnt = centerPnt_miner;
				return true;
			}
			return false;
		}

		static bool TryFitWithCircleAndRant( TopoDS_Wire wire, gp_Dir axisDirection, out double dConfidence, out gp_Pnt centerPnt, out bool isFittingByCircle )
		{
			dConfidence = 0.0;
			gp_Pnt center3D = new gp_Pnt();
			centerPnt = null;
			isFittingByCircle = false;
			bool isFitMajorWireSuccess = TryFitCircleToWireProjection( wire, axisDirection, out center3D, out _, out double dCircleConfidence );

			if( isFitMajorWireSuccess ) {
				dConfidence = dCircleConfidence;
				centerPnt = center3D;
				isFittingByCircle = true;
				return true;
			}

			isFitMajorWireSuccess = TryFitRectangleToWireProjection( wire,
						axisDirection,
						out center3D,
						out _,
						out _,
						out _,
						out double dRectangleConfidence );
			if( isFitMajorWireSuccess ) {
				dConfidence = dRectangleConfidence;
				centerPnt = center3D;
				return true;
			}
			return false;
		}

		static bool TryFitCircleToWireProjection(
		   TopoDS_Wire wire,
		   gp_Dir viewDirection,
		   out gp_Pnt center3D,
		   out double radius,
		   out double confidenceRatio,
		   double toleranceRatio = 0.1,
		   double minOverlapRatio = 0.75
	   )
		{
			center3D = null;
			radius = 0.0;
			confidenceRatio = 0.0;

			if( wire == null || wire.IsNull() || viewDirection == null ) {
				return false;
			}

			// Step 1: create projection plane (with view direction as normal)
			gp_Ax2 projectionPlane = CreateProjectionPlane( wire, viewDirection );

			// Step 2: project wire to 2D plane
			List<gp_Pnt2d> points2D = ProjectWireTo2D( wire, projectionPlane, 300 );
			if( points2D == null || points2D.Count < 10 ) {
				return false;
			}

			// step 3: filter noise and fit circle
			gp_Pnt2d center2D;
			List<int> inlierIndices;
			if( !FilterOutliersAndRefitCircle(
				points2D,
				out center2D,
				out radius,
				out inlierIndices,

				// acceptable points outside circle
				outlierThreshold: 0.1,
				minSegmentLength: 3,
				maxIterations: 3
			) ) {
				return false;
			}

			// step 4: calculate overlap ratio (using filtered points)
			double tolerance = radius * toleranceRatio;
			int finalInlierCount = 0;

			foreach( int idx in inlierIndices ) {
				gp_Pnt2d pt = points2D[ idx ];
				double distance = Math.Abs( pt.Distance( center2D ) - radius );
				if( distance <= tolerance ) {
					finalInlierCount++;
				}
			}

			confidenceRatio = (double)finalInlierCount / points2D.Count;

			// step 5: check if can fit with circle
			if( confidenceRatio < minOverlapRatio ) {
				return false;
			}

			// step 6: convert 2d center back to 3d
			center3D = Convert2DTo3D( center2D, projectionPlane );

			return true;
		}

		static gp_Ax2 CreateProjectionPlane( TopoDS_Wire wire, gp_Dir viewDirection )
		{
			// calculate wire center as plane origin
			gp_Pnt origin = CalculateWireCenter( wire );

			// viewdirection as plane normal (z axis)
			gp_Dir zAxis = viewDirection;

			// choose x axis (perpendicular to z axis)
			gp_Dir xAxis;
			if( Math.Abs( zAxis.Z() ) < 0.9 ) {
				gp_Dir zWorld = new gp_Dir( 0, 0, 1 );
				xAxis = zWorld.Crossed( zAxis );
			}
			else {
				gp_Dir xWorld = new gp_Dir( 1, 0, 0 );
				xAxis = xWorld.Crossed( zAxis );
			}

			return new gp_Ax2( origin, zAxis, xAxis );
		}

		static List<gp_Pnt2d> ProjectWireTo2D( TopoDS_Wire wire, gp_Ax2 plane, int sampleCount )
		{
			List<gp_Pnt2d> points2D = new List<gp_Pnt2d>();

			// uniform sampling on wire
			List<SamplePoint> samples = SamplePointsOnWire( wire, sampleCount );
			if( samples == null || samples.Count == 0 ) {
				return points2D;
			}

			gp_Pnt planeOrigin = plane.Location();
			gp_Dir xDir = plane.XDirection();
			gp_Dir yDir = plane.YDirection();

			foreach( var sample in samples ) {
				gp_Pnt pt3D = sample.Point;

				// project to plane local coord system
				gp_Vec vecFromOrigin = new gp_Vec( planeOrigin, pt3D );
				double u = vecFromOrigin.Dot( new gp_Vec( xDir ) );
				double v = vecFromOrigin.Dot( new gp_Vec( yDir ) );

				points2D.Add( new gp_Pnt2d( u, v ) );
			}

			return points2D;
		}

		static List<SamplePoint> SamplePointsOnWire( TopoDS_Wire wire, int sampleCount )
		{
			List<SamplePoint> samples = new List<SamplePoint>();

			// step 1: get all edges
			List<TopoDS_Edge> edges = new List<TopoDS_Edge>();
			TopExp_Explorer edgeExp = new TopExp_Explorer( wire, TopAbs_ShapeEnum.TopAbs_EDGE );
			while( edgeExp.More() ) {
				TopoDS_Edge edge = TopoDS.ToEdge( edgeExp.Current() );
				if( edge != null && !edge.IsNull() ) {
					edges.Add( edge );
				}
				edgeExp.Next();
			}

			if( edges.Count == 0 ) {
				return samples;
			}

			// step 2: calculate each edge length
			List<double> edgeLengths = new List<double>();
			double totalLength = 0.0;

			foreach( TopoDS_Edge edge in edges ) {
				double length = CalculateEdgeLength( edge );
				edgeLengths.Add( length );
				totalLength += length;
			}

			if( totalLength < ACCURACY_Threshold ) {
				return samples;
			}

			// step 3: uniform sampling
			double sampleInterval = totalLength / sampleCount;

			for( int i = 0; i < sampleCount; i++ ) {
				double targetLength = i * sampleInterval;
				double accumulatedLength = 0.0;

				// find corresponding edge and parameter
				for( int edgeIdx = 0; edgeIdx < edges.Count; edgeIdx++ ) {
					double edgeLength = edgeLengths[ edgeIdx ];
					double nextAccumulatedLength = accumulatedLength + edgeLength;

					if( targetLength <= nextAccumulatedLength || edgeIdx == edges.Count - 1 ) {
						// target point is on this edge
						TopoDS_Edge edge = edges[ edgeIdx ];
						double localLength = targetLength - accumulatedLength;
						double t = edgeLength > ACCURACY_Threshold ? localLength / edgeLength : 0.0;

						if( TryGetEdgeParameterRange( edge, out double first, out double last ) ) {
							double param = first + t * ( last - first );
							gp_Pnt point = GetPntOnEdgeByParam( edge, param );

							if( point != null ) {
								samples.Add( new SamplePoint
								{
									Edge = edge,
									Parameter = param,
									Point = point
								} );
							}
						}

						break;
					}
					accumulatedLength = nextAccumulatedLength;
				}
			}

			return samples;
		}

		static gp_Pnt Convert2DTo3D( gp_Pnt2d point2D, gp_Ax2 plane )
		{
			double u = point2D.X();
			double v = point2D.Y();

			gp_Pnt origin = plane.Location();
			gp_Vec xVec = new gp_Vec( plane.XDirection() );
			gp_Vec yVec = new gp_Vec( plane.YDirection() );

			xVec.Multiply( u );
			yVec.Multiply( v );

			return new gp_Pnt(
					origin.X() + xVec.X() + yVec.X(),
					origin.Y() + xVec.Y() + yVec.Y(),
					origin.Z() + xVec.Z() + yVec.Z()
				);
		}

		#endregion

		#region Analyse Part Up or Down 

		enum ProjectionPlane
		{
			XZ,  // project to xz plane (use x coord)
			YZ   // project to yz plane (use y coord)
		}

		static bool AnalyzeRevolutionShapeTaper(
		   TopoDS_Shape shape,
		   out bool isTopWiderThanBottom )
		{
			isTopWiderThanBottom = false;
			bool isXZDone = AnalyzeRevolutionShapeTaperByExtremePoints(
				shape,
				ProjectionPlane.XZ,
				out bool isTopWiderThanBottomXZ
			);
			bool isYZDone = AnalyzeRevolutionShapeTaperByExtremePoints(
				shape,
				ProjectionPlane.YZ,
				out bool isTopWiderThanBottom_YZ
			);
			if( isXZDone && isYZDone ) {
				// combine xz and yz projection results
				isTopWiderThanBottom = isTopWiderThanBottomXZ || isTopWiderThanBottom_YZ;
				return true;
			}
			if( isXZDone ) {
				isTopWiderThanBottom = isTopWiderThanBottomXZ;
				return true;
			}
			if( isYZDone ) {
				isTopWiderThanBottom = isTopWiderThanBottom_YZ;
				return true;
			}
			return false;
		}

		static bool AnalyzeRevolutionShapeTaperByExtremePoints(
			TopoDS_Shape shape,
			ProjectionPlane plane,
			out bool isTopWiderThanBottom
		)
		{
			isTopWiderThanBottom = false;

			if( shape == null || shape.IsNull() ) {
				return false;
			}

			// step 1: collect all projection points (h, z) by sampling edges
			List<(double h, double z)> points = new List<(double, double)>();

			// sample points from all edges instead of only using vertices
			TopExp_Explorer edgeExp = new TopExp_Explorer( shape, TopAbs_ShapeEnum.TopAbs_EDGE );
			while( edgeExp.More() ) {
				TopoDS_Edge edge = TopoDS.ToEdge( edgeExp.Current() );
				if( edge != null && !edge.IsNull() ) {

					// sample points along this edge (10 points for good curve representation)
					List<gp_Pnt> edgeSamples = SamplePointsOnEdge( edge, 10 );

					foreach( gp_Pnt point in edgeSamples ) {
						double h = ( plane == ProjectionPlane.XZ ) ? point.X() : point.Y();
						double z = point.Z();
						points.Add( (h, z) );
					}
				}
				edgeExp.Next();
			}

			if( points.Count < 4 ) {
				return false;
			}

			// step 2: find z range
			double zMin = points.Min( p => p.z );
			double zMax = points.Max( p => p.z );
			double zRange = zMax - zMin;

			if( zRange < ACCURACY_Threshold ) {
				return false;
			}

			// step 3: get points at zMin and zMax (with small tolerance)
			double tolerance = zRange * 0.01; // 1% tolerance for numerical precision

			var bottomPoints = points.Where( p => Math.Abs( p.z - zMin ) <= tolerance ).ToList();
			var topPoints = points.Where( p => Math.Abs( p.z - zMax ) <= tolerance ).ToList();

			if( bottomPoints.Count == 0 || topPoints.Count == 0 ) {
				return false;
			}

			// step 4: calculate bottom width (max |h| at zMin)
			double bottomWidth = bottomPoints.Max( p => Math.Abs( p.h ) );

			// step 5: calculate top width (max |h| at zMax)
			double topWidth = topPoints.Max( p => Math.Abs( p.h ) );

			// step 6: compare top and bottom widths
			// need significant difference (at least 5%)
			double avgWidth = ( topWidth + bottomWidth ) / 2.0;
			double widthDiff = Math.Abs( topWidth - bottomWidth );

			if( widthDiff < avgWidth * 0.05 ) {
				// difference is too small( default as : top narrower than bottom) 
				isTopWiderThanBottom = false;
			}
			else {
				// bottom narrower than top
				isTopWiderThanBottom = topWidth > bottomWidth;
			}
			return true;
		}

		static bool TryGetEdgeParameterRange( TopoDS_Edge edge, out double first, out double last )
		{
			first = 0;
			last = 0;

			if( edge == null || edge.IsNull() ) {
				return false;
			}

			OCC.BRep.BRep_Tool.Range( edge, ref first, ref last );
			return Math.Abs( last - first ) >= ACCURACY_Threshold;
		}

		static gp_Pnt GetPntOnEdgeByParam( TopoDS_Edge edge, double parameter )
		{
			if( edge == null || edge.IsNull() ) {
				return null;
			}

			BRepAdaptor_Curve curve = new BRepAdaptor_Curve( edge );
			return curve.Value( parameter );
		}

		static List<gp_Pnt> SamplePointsOnEdge( TopoDS_Edge edge, int sampleCount )
		{
			List<gp_Pnt> samples = new List<gp_Pnt>();

			if( edge == null || edge.IsNull() || sampleCount < 2 ) {
				return samples;
			}

			if( !TryGetEdgeParameterRange( edge, out double first, out double last ) ) {
				return samples;
			}

			for( int i = 0; i < sampleCount; i++ ) {
				double t = i / (double)( sampleCount - 1 );
				double param = first + t * ( last - first );
				gp_Pnt point = GetPntOnEdgeByParam( edge, param );
				if( point != null ) {
					samples.Add( point );
				}
			}

			return samples;
		}
		#endregion

		#region  Fitting Axis

		static bool FitAxisDirectionByRANSAC(
				   List<gp_Pnt> points,
				   List<gp_Dir> normals,
				   out gp_Dir axisDir,
				   double sampleRatio = OUT_LinerRatio,
				   int maxIterations = 100,

				   // allowed angle deviation
				   double inlierThreshold = 0.05,

				   // 85% perpent inliers required
				   double minInlierRatio = 0.85
			   )
		{
			axisDir = null;
			if( points == null || normals == null || points.Count != normals.Count || points.Count < 2 ) {
				return false;
			}

			int n = points.Count;
			Random random = new Random();

			// to check wich situation have most inlier count
			int bestInlierCount = 0;

			// track consecutive iterations with same bestInlierCount
			int SameAnswerTimesCount = 0;

			// stop if same count for 5 iterations
			const int maxConsecutiveSame = 5;

			// 95% inliers sufficient
			int earlyStopThreshold = (int)( points.Count() * sampleRatio * minInlierRatio );

			// RANSAC main loop
			for( int iter = 0; iter < maxIterations; iter++ ) {

				// step 1: randomly select sample
				List<int> sampleIndices = new List<int>();
				HashSet<int> usedIndices = new HashSet<int>();
				for( int i = 0; i < points.Count() * sampleRatio; i++ ) {
					int idx;
					do {
						idx = random.Next( n );
					} while( usedIndices.Contains( idx ) );

					usedIndices.Add( idx );
					sampleIndices.Add( idx );
				}

				// step 2: fit candidate axis from sample
				List<gp_Pnt> samplePoints = new List<gp_Pnt>();
				List<gp_Dir> sampleNormals = new List<gp_Dir>();

				foreach( int idx in sampleIndices ) {
					samplePoints.Add( points[ idx ] );
					sampleNormals.Add( normals[ idx ] );
				}

				// fit using method 2 (min eigenvector)
				if( !FitAxisDirectionByPCA( sampleNormals, out axisDir ) ) {
					continue;
				}
				if( axisDir == null ) {
					continue;
				}

				// step 3: count inliers with consistent angle to axis
				List<int> currentInlierIndices = new List<int>();

				// collect all angles between normals and axis
				List<double> angles = new List<double>();
				for( int i = 0; i < n; i++ ) {
					double angle = CalculateAngleBetweenDirections(
						axisDir,
						normals[ i ],
						allowOpposite: false
					);
					angles.Add( angle );
				}

				// calculate average angle as reference
				double medianAngle = angles.Average();

				// angle with the target vector
				double angleToleranceRadians = inlierThreshold; // ~0.05 rad ≈ 2.87°

				for( int i = 0; i < n; i++ ) {
					double deviation = Math.Abs( angles[ i ] - medianAngle );
					if( deviation < angleToleranceRadians ) {
						currentInlierIndices.Add( i );
					}
				}

				// step 4: update best model
				if( currentInlierIndices.Count > bestInlierCount ) {
					bestInlierCount = currentInlierIndices.Count;

					// reset counter
					SameAnswerTimesCount = 0;

					// solution good enough
					if( bestInlierCount >= earlyStopThreshold ) {
						break;
					}
				}
				else {
					// same or worse result, increment consecutive counter
					SameAnswerTimesCount++;

					// stuck with same result for too many iterations
					if( SameAnswerTimesCount >= maxConsecutiveSame ) {
						break;
					}
				}
			}
			return true;
		}

		static bool FitAxisDirectionByPCA( List<gp_Dir> normals, out gp_Dir axisDir )
		{
			axisDir = null;
			bool bOrthogonalSuccess = FitOrthogonalAxis( normals, out gp_Dir axisDir_O, out double confidence_O );
			bool bParallelSuccess = FitParallelAxis( normals, out gp_Dir axisDir_P, out double confidence_P );
			if( bOrthogonalSuccess && bParallelSuccess ) {
				axisDir = confidence_O >= confidence_P ? axisDir_O : axisDir_P;
				Console.WriteLine( $"Orthogonal Confidence: {confidence_O}, Parallel Confidence: {confidence_P}" );
				Console.WriteLine( $"Orthogonal Axis: {axisDir_O}, Parallel Axis: {axisDir_P}" );
				return true;
			}

			if( bOrthogonalSuccess ) {
				axisDir = axisDir_O;
				return true;
			}

			if( bParallelSuccess ) {
				axisDir = axisDir_P;
				return true;
			}
			return false;
		}

		static bool FitOrthogonalAxis( List<gp_Dir> normals, out gp_Dir axisDir, out double confidence )
		{
			// Use core method to find perpendicular direction (min eigenvector)
			bool success = FitAxisByPCA( normals, Orthogonal: true, out axisDir, out confidence );

			if( !success ) {
				return false;
			}
			return true;
		}

		static bool FitParallelAxis( List<gp_Dir> normals, out gp_Dir mainDir, out double confidence )
		{
			bool success = FitAxisByPCA( normals, Orthogonal: false, out mainDir, out confidence );
			if( !success ) {
				return false;
			}
			return true;
		}

		static bool FitAxisByPCA( List<gp_Dir> normals, bool Orthogonal, out gp_Dir direction, out double quality )
		{
			direction = null;
			quality = 0.0;

			if( normals == null || normals.Count < 2 ) {
				return false;
			}

			int n = normals.Count;
			// build covariance matrix
			double cxx = 0, cyy = 0, czz = 0;
			double cxy = 0, cxz = 0, cyz = 0;

			foreach( gp_Dir N in normals ) {
				cxx += N.X() * N.X();
				cyy += N.Y() * N.Y();
				czz += N.Z() * N.Z();
				cxy += N.X() * N.Y();
				cxz += N.X() * N.Z();
				cyz += N.Y() * N.Z();
			}

			// normalize
			cxx /= n;
			cyy /= n;
			czz /= n;
			cxy /= n;
			cxz /= n;
			cyz /= n;

			// compute eigenvector
			if( Orthogonal ) {
				direction = ComputeMinEigenvector( cxx, cyy, czz, cxy, cxz, cyz );
			}

			// try parallel first
			else {
				// Max eigenvector: most aligned with normals
				direction = ComputeMaxEigenvector( cxx, cyy, czz, cxy, cxz, cyz );
			}
			if( direction == null ) {
				return false;
			}

			// Evaluate quality using angle consistency
			quality = EvaluateDirectionAngleConsistency( direction, normals );

			return true;
		}

		static gp_Dir ComputeEigenvectorCore(
				double cxx, double cyy, double czz,
				double cxy, double cxz, double cyz,
				double trace,
				bool computeMin
			)
		{
			// Initialize vector
			double vx = 1.0, vy = 1.0, vz = 1.0;
			double norm = Math.Sqrt( vx * vx + vy * vy + vz * vz );
			vx /= norm;
			vy /= norm;
			vz /= norm;

			const int maxIterations = 100;
			double prevNorm = 0;

			for( int iter = 0; iter < maxIterations; iter++ ) {
				// Matrix-vector multiplication
				double nx = cxx * vx + cxy * vy + cxz * vz;
				double ny = cxy * vx + cyy * vy + cyz * vz;
				double nz = cxz * vx + cyz * vy + czz * vz;

				// Normalize
				norm = Math.Sqrt( nx * nx + ny * ny + nz * nz );

				// Check convergence
				if( Math.Abs( norm - prevNorm ) < ACCURACY_Threshold && iter > 10 ) {
					break;
				}

				if( norm < ACCURACY_Threshold ) {
					// Matrix may be singular, try different initialization
					if( iter < 10 ) {
						vx = (double)( iter + 1 );
						vy = (double)( iter + 2 );
						vz = (double)( iter + 3 );
						norm = Math.Sqrt( vx * vx + vy * vy + vz * vz );
						vx /= norm;
						vy /= norm;
						vz /= norm;
						continue;
					}

					// Use fallback method
					return null;
				}

				vx = nx / norm;
				vy = ny / norm;
				vz = nz / norm;
				prevNorm = norm;
			}

			// Verify result via Rayleigh quotient
			// Note: Always use original matrix C for validation, not shifted matrix
			double rayleighQuotient = vx * vx * cxx + vy * vy * cyy + vz * vz * czz
				+ 2.0 * ( vx * vy * cxy + vx * vz * cxz + vy * vz * cyz );

			// Validate based on whether computing min or max eigenvector
			if( computeMin ) {
				// For min eigenvector, quotient should be small
				if( rayleighQuotient > trace * 0.6 ) {
					return null;
				}
			}
			else {
				// For max eigenvector, quotient should be large
				if( rayleighQuotient < trace * 0.3 ) {
					return null;
				}
			}

			return new gp_Dir( vx, vy, vz );
		}

		static gp_Dir ComputeMinEigenvector(
					double cxx, double cyy, double czz,
					double cxy, double cxz, double cyz
				)
		{
			// Calculate matrix trace
			double trace = cxx + cyy + czz;

			// Apply matrix shifting for inverse power iteration
			// This converts min eigenvector problem to max eigenvector problem
			double shift = trace / 2.0;
			double cxx_shifted = cxx - shift;
			double cyy_shifted = cyy - shift;
			double czz_shifted = czz - shift;

			// Compute using core method with shifted matrix
			gp_Dir result = ComputeEigenvectorCore(
				cxx_shifted, cyy_shifted, czz_shifted,
				cxy, cxz, cyz,
				trace,
				computeMin: true
			);

			// Use fallback if core method failed
			if( result == null ) {
				return ComputeMinEigenvectorFallback( cxx, cyy, czz, cxy, cxz, cyz );
			}

			return result;
		}

		static gp_Dir ComputeMinEigenvectorFallback( double cxx, double cyy, double czz, double cxy, double cxz, double cyz
				)
		{
			// find smallest diagonal element
			double minVariance = Math.Min( cxx, Math.Min( cyy, czz ) );

			if( Math.Abs( minVariance - cxx ) < ACCURACY_Threshold ) {
				return new gp_Dir( 1, 0, 0 );
			}
			else if( Math.Abs( minVariance - cyy ) < ACCURACY_Threshold ) {
				return new gp_Dir( 0, 1, 0 );
			}
			else {
				return new gp_Dir( 0, 0, 1 );
			}
		}

		static gp_Dir ComputeMaxEigenvector(
			double cxx, double cyy, double czz,
			double cxy, double cxz, double cyz
		)
		{
			// Calculate matrix trace
			double trace = cxx + cyy + czz;

			if( trace < ACCURACY_Threshold ) {
				// Matrix is near-zero, use fallback
				return ComputeMaxEigenvectorFallback( cxx, cyy, czz );
			}

			// Apply power iteration (no shifting needed for max eigenvector)
			// Compute using core method with original matrix
			gp_Dir result = ComputeEigenvectorCore(
				cxx, cyy, czz,
				cxy, cxz, cyz,
				trace,
				computeMin: false
			);

			// Use fallback if core method failed
			if( result == null ) {
				return ComputeMaxEigenvectorFallback( cxx, cyy, czz );
			}

			return result;
		}

		static gp_Dir ComputeMaxEigenvectorFallback( double cxx, double cyy, double czz )
		{
			// Find largest diagonal element
			double maxVariance = Math.Max( cxx, Math.Max( cyy, czz ) );

			if( Math.Abs( maxVariance - cxx ) < ACCURACY_Threshold ) {
				return new gp_Dir( 1, 0, 0 );
			}
			else if( Math.Abs( maxVariance - cyy ) < ACCURACY_Threshold ) {
				return new gp_Dir( 0, 1, 0 );
			}
			else {
				return new gp_Dir( 0, 0, 1 );
			}
		}

		#endregion

		static bool AlignAxisToZAndCenterXYWithTransform(
			TopoDS_Shape shape,
			gp_Dir axisDir,
			gp_Pnt axisPoint,
			out TopoDS_Shape transformedShape,
			out gp_Trsf combinedTrsf
			)
		{
			transformedShape = null;
			combinedTrsf = new gp_Trsf();

			if( shape == null || shape.IsNull() || axisDir == null || axisPoint == null ) {
				return false;
			}

			// step 1: create rotation to align axis to z
			gp_Trsf rotationTrsf = new gp_Trsf();
			gp_Dir zAxis = new gp_Dir( 0, 0, 1 );

			double dotProduct = axisDir.Dot( zAxis );

			// check if already aligned
			if( Math.Abs( Math.Abs( dotProduct ) - 1.0 ) < ACCURACY_Threshold ) {
				// already parallel to z axis
				if( dotProduct < 0 ) {
					// opposite direction, rotate 180 degrees
					gp_Ax1 rotAxis = new gp_Ax1( axisPoint, new gp_Dir( 1, 0, 0 ) );
					rotationTrsf.SetRotation( rotAxis, Math.PI );
				}
				// no rotation needed
			}
			else {
				// calculate rotation axis (axisDir x z)
				gp_Vec cross = new gp_Vec( axisDir ).Crossed( new gp_Vec( zAxis ) );
				double angle = axisDir.Angle( zAxis );

				gp_Ax1 rotationAxis = new gp_Ax1( axisPoint, new gp_Dir( cross ) );
				rotationTrsf.SetRotation( rotationAxis, angle );
			}

			// step 2: calculate rotated axis point position
			gp_Pnt rotatedPoint = axisPoint.Transformed( rotationTrsf );

			// step 3: create translation for x=0, y=0
			gp_Trsf translationTrsf = new gp_Trsf();
			translationTrsf.SetTranslation(
					new gp_Vec( -rotatedPoint.X(), -rotatedPoint.Y(), 0 )
			);

			// step 4: combine transforms (rotate then translate)
			combinedTrsf = translationTrsf.Multiplied( rotationTrsf );

			// step 5: apply transformation
			BRepBuilderAPI_Transform transform = new BRepBuilderAPI_Transform( shape, combinedTrsf, true );


			if( !transform.IsDone() ) {
				return false;
			}

			transformedShape = transform.Shape();
			return true;
		}

		static double CalculateOBBVolume( TopoDS_Shape shape )
		{
			if( shape == null || shape.IsNull() ) {
				return 0.0;
			}

			// step 2: create obb
			Bnd_OBB obb = new Bnd_OBB();
			BRepBndLib.AddOBB( shape, ref obb, true, true, true );

			if( obb.IsVoid() ) {
				return 0.0;
			}

			// step 3: calculate obb volume = length x width x height
			double xHSize = obb.XHSize();
			double yHSize = obb.YHSize();
			double zHSize = obb.ZHSize();

			// obb returns half length, multiply by 2
			double length = xHSize * 2.0;
			double width = yHSize * 2.0;
			double height = zHSize * 2.0;

			double volume = length * width * height;
			return volume;
		}

		static bool GetSortedWiresInShell( TopoDS_Shape closedShell, out List<TopoDS_Wire> sortedWires )
		{
			List<(TopoDS_Wire wire, double size)> result = new List<(TopoDS_Wire wire, double size)>();
			sortedWires = new List<TopoDS_Wire>();
			if( closedShell == null || closedShell.IsNull() ) {
				return false;
			}

			// step 1: get all boundary wires
			List<TopoDS_Wire> wires = GetBoundaryWires( closedShell );

			// step 2: calculate wire length and add to result
			foreach( TopoDS_Wire wire in wires ) {
				double length = CalculateWireSize( wire );
				if( length > ACCURACY_Threshold ) {
					result.Add( (wire, length) );
				}
			}

			if( result.Count == 0 ) {
				return false;
			}
			// step 3: sort by size descending
			result = result.OrderByDescending( x => x.size ).ToList();

			// re-save as list format
			sortedWires = result.Select( x => x.wire ).ToList();

			return true;
		}

		static List<TopoDS_Wire> GetBoundaryWires( TopoDS_Shape faceList )
		{
			List<TopoDS_Wire> wires = new List<TopoDS_Wire>();

			// step 2: get free bounds using shapeanalysis
			ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( faceList, false );

			// step 3: get closed wires
			TopoDS_Compound closedWires = freeBounds.GetClosedWires();

			TopExp_Explorer exp = new TopExp_Explorer( closedWires, TopAbs_ShapeEnum.TopAbs_WIRE );
			while( exp.More() ) {
				TopoDS_Shape shape = exp.Current();
				if( shape.ShapeType() == TopAbs_ShapeEnum.TopAbs_WIRE ) {
					TopoDS_Wire wire = TopoDS.ToWire( shape );
					wires.Add( wire );
				}
				exp.Next();
			}

			return wires;
		}

		static bool GetSampleNormalAndPoint( TopoDS_Wire wire, TopoDS_Shape shell, out List<gp_Pnt> points, out List<gp_Dir> normals )
		{
			points = new List<gp_Pnt>();
			normals = new List<gp_Dir>();

			if( wire == null || wire.IsNull() || shell == null || shell.IsNull() ) {
				return false;
			}
			const int totalSampleCount = 50;
			List<SamplePoint> wireSamples = SamplePointsOnWire( wire, totalSampleCount );

			if( wireSamples == null || wireSamples.Count < 10 ) {
				return false;
			}
			Dictionary<TopoDS_Edge, TopoDS_Face> edgeFaceCache = BuildEdgeFaceMap( wire, shell );

			foreach( SamplePoint sample in wireSamples ) {
				// use cache lookup, avoid repeated traversal
				TopoDS_Face correspondingFace = null;
				if( edgeFaceCache != null && edgeFaceCache.ContainsKey( sample.Edge ) ) {
					correspondingFace = edgeFaceCache[ sample.Edge ];
				}
				else {
					// fallback: use original method if cache fails
					correspondingFace = FindFaceForEdge( sample.Edge, shell );
				}

				if( correspondingFace == null || correspondingFace.IsNull() ) {
					continue;
				}

				// calculate normal for this point
				gp_Dir normal = GetSurfaceNormal( sample.Edge, correspondingFace, sample.Parameter );
				if( normal != null ) {
					points.Add( sample.Point );
					normals.Add( normal );
				}
			}

			if( normals.Count < 10 || points.Count < 10 ) {
				return false;
			}
			return true;
		}

		static Dictionary<TopoDS_Edge, TopoDS_Face> BuildEdgeFaceMap( TopoDS_Wire wire, TopoDS_Shape shell )
		{
			Dictionary<TopoDS_Edge, TopoDS_Face> edgeFaceMap = new Dictionary<TopoDS_Edge, TopoDS_Face>();

			if( wire == null || wire.IsNull() || shell == null || shell.IsNull() ) {
				return edgeFaceMap;
			}

			// step 1: collect all edges in wire
			HashSet<TopoDS_Edge> wireEdges = new HashSet<TopoDS_Edge>();
			TopExp_Explorer wireExp = new TopExp_Explorer( wire, TopAbs_ShapeEnum.TopAbs_EDGE );
			while( wireExp.More() ) {
				TopoDS_Edge edge = TopoDS.ToEdge( wireExp.Current() );
				if( edge != null && !edge.IsNull() ) {
					wireEdges.Add( edge );
				}
				wireExp.Next();
			}

			if( wireEdges.Count == 0 ) {
				return edgeFaceMap;
			}

			// step 2: traverse faces in shell, build mapping
			TopExp_Explorer faceExp = new TopExp_Explorer( shell, TopAbs_ShapeEnum.TopAbs_FACE );
			while( faceExp.More() ) {
				TopoDS_Face face = TopoDS.ToFace( faceExp.Current() );
				if( face != null && !face.IsNull() ) {
					// traverse all edges of this face
					TopExp_Explorer edgeExp = new TopExp_Explorer( face, TopAbs_ShapeEnum.TopAbs_EDGE );
					while( edgeExp.More() ) {
						TopoDS_Edge edge = TopoDS.ToEdge( edgeExp.Current() );
						if( edge != null && !edge.IsNull() && wireEdges.Contains( edge ) ) {
							// found matching edge, add to map if not exists
							if( !edgeFaceMap.ContainsKey( edge ) ) {
								edgeFaceMap[ edge ] = face;
							}
						}
						edgeExp.Next();
					}
				}
				faceExp.Next();

				// optimization: early exit if all found
				if( edgeFaceMap.Count >= wireEdges.Count ) {
					break;
				}
			}

			return edgeFaceMap;
		}

		static bool FitCircle2D( List<gp_Pnt2d> points, out gp_Pnt2d center, out double radius )
		{
			center = null;
			radius = 0.0;

			if( points == null || points.Count < 3 ) {
				return false;
			}

			try {
				// copy point set, avoid modifying original
				List<gp_Pnt2d> workingPoints = new List<gp_Pnt2d>( points );

				// can ignore 30% outliers
				int maxOutliers = (int)( points.Count * 0.3 ) > 10 ? (int)( points.Count * 0.3 ) : points.Count();
				int iterationCount = 0;
				int maxIterations = 10;

				while( iterationCount < maxIterations && workingPoints.Count >= 3 ) {
					// step 1: fit circle
					if( !FitCircle2D_LeastSquares( workingPoints, out center, out radius ) ) {
						return false;
					}

					// step 2: calculate deviation for each point
					List<(gp_Pnt2d point, double deviation)> pointDeviations =
					new List<(gp_Pnt2d, double)>();

					foreach( var pt in workingPoints ) {
						double distToCenter = pt.Distance( center );
						double deviation = Math.Abs( distToCenter - radius );
						pointDeviations.Add( (pt, deviation) );
					}

					// step 3: find point with max deviation
					pointDeviations.Sort( ( a, b ) => b.deviation.CompareTo( a.deviation ) );

					// step 4: calculate median radius and threshold
					double medianRadius = CalculateMedianRadius( workingPoints, center );
					double threshold = medianRadius * 0.15;
					double maxDeviation = pointDeviations[ 0 ].deviation;

					// over threshold and still have room for outliers
					if( maxDeviation > threshold &&
					   ( points.Count - workingPoints.Count ) < maxOutliers ) {
						workingPoints.Remove( pointDeviations[ 0 ].point );
						iterationCount++;
					}
					else {
						// converged, stop iteration
						break;
					}
				}

				// final fit with remaining points
				return FitCircle2D_LeastSquares( workingPoints, out center, out radius );
			}
			catch {
				return false;
			}
		}

		static bool FitCircle2D_LeastSquares( List<gp_Pnt2d> points, out gp_Pnt2d center, out double radius )
		{
			center = null;
			radius = 0.0;

			if( points == null || points.Count < 3 ) {
				return false;
			}

			int n = points.Count;

			// calculate centroid
			double sumX = 0, sumY = 0;
			foreach( var pt in points ) {
				sumX += pt.X();
				sumY += pt.Y();
			}
			double cx = sumX / n;
			double cy = sumY / n;

			// algebraic fitting method
			double sumXX = 0, sumYY = 0, sumXY = 0;
			double sumXXX = 0, sumXYY = 0, sumXXY = 0, sumYYY = 0;

			foreach( var pt in points ) {
				double xi = pt.X() - cx;
				double yi = pt.Y() - cy;
				double xi2 = xi * xi;
				double yi2 = yi * yi;

				sumXX += xi2;
				sumYY += yi2;
				sumXY += xi * yi;
				sumXXX += xi2 * xi;
				sumXYY += xi * yi2;
				sumXXY += xi2 * yi;
				sumYYY += yi2 * yi;
			}

			double A = 2.0 * ( sumXX * sumYY - sumXY * sumXY );
			if( Math.Abs( A ) < ACCURACY_Threshold ) {
				return false;
			}

			double B1 = sumXXX + sumXYY;
			double B2 = sumXXY + sumYYY;

			double centerX = ( B1 * sumYY - B2 * sumXY ) / A + cx;
			double centerY = ( sumXX * B2 - sumXY * B1 ) / A + cy;

			center = new gp_Pnt2d( centerX, centerY );

			// calculate radius (average distance)
			double sumR = 0;
			foreach( var pt in points ) {
				sumR += pt.Distance( center );
			}
			radius = sumR / n;

			return radius > ACCURACY_Threshold;
		}

		static double CalculateMedianRadius( List<gp_Pnt2d> points, gp_Pnt2d center )
		{
			if( points == null || points.Count == 0 || center == null ) {
				return 0.0;
			}

			List<double> radii = new List<double>();
			foreach( var pt in points ) {
				radii.Add( pt.Distance( center ) );
			}

			radii.Sort();

			int mid = radii.Count / 2;
			if( radii.Count % 2 == 0 ) {
				return ( radii[ mid - 1 ] + radii[ mid ] ) / 2.0;
			}
			else {
				return radii[ mid ];
			}
		}

		static bool FilterOutliersAndRefitCircle(
			List<gp_Pnt2d> points,
			out gp_Pnt2d center,
			out double radius,
			out List<int> inlierIndices,
			double outlierThreshold = 0.15,
			int minSegmentLength = 5,
			int maxIterations = 3
		)
		{
			center = null;
			radius = 0.0;
			inlierIndices = new List<int>();

			if( points == null || points.Count < 10 ) {
				return false;
			}

			// initialize: all points are inliers
			for( int i = 0; i < points.Count; i++ ) {
				inlierIndices.Add( i );
			}

			// iterative filtering
			for( int iter = 0; iter < maxIterations; iter++ ) {
				// step 1: fit circle with current inliers
				List<gp_Pnt2d> currentPoints = new List<gp_Pnt2d>();
				foreach( int idx in inlierIndices ) {
					currentPoints.Add( points[ idx ] );
				}

				if( !FitCircle2D( currentPoints, out center, out radius ) ) {
					return false;
				}

				// step 2: detect outlier segments
				double threshold = radius * outlierThreshold;
				List<(int startIdx, int endIdx)> outlierSegments = DetectOutlierSegments(
					inlierIndices,
					points,
					center,
					radius,
						threshold
					);

				// step 3: filter qualified outlier segments
				bool hasFiltered = false;
				List<int> toRemove = new List<int>();

				foreach( var (startIdx, endIdx) in outlierSegments ) {
					int segmentLength = endIdx >= startIdx ? endIdx - startIdx + 1 : ( inlierIndices.Count - startIdx ) + endIdx + 1;

					// check segment length
					if( segmentLength < minSegmentLength ) {
						continue;
					}

					// extract segment points
					List<gp_Pnt2d> segmentPoints = new List<gp_Pnt2d>();
					if( endIdx >= startIdx ) {
						for( int i = startIdx; i <= endIdx; i++ ) {
							segmentPoints.Add( points[ inlierIndices[ i ] ] );
						}
					}
					else {
						// ring connection
						for( int i = startIdx; i < inlierIndices.Count; i++ ) {
							segmentPoints.Add( points[ inlierIndices[ i ] ] );
						}
						for( int i = 0; i <= endIdx; i++ ) {
							segmentPoints.Add( points[ inlierIndices[ i ] ] );
						}
					}

					// check if protrusion/depression pattern
					if( IsProtrusionPattern( segmentPoints, center, radius ) ) {
						// mark indices for deletion
						if( endIdx >= startIdx ) {
							for( int i = startIdx; i <= endIdx; i++ ) {
								toRemove.Add( i );
							}
						}
						else {
							for( int i = startIdx; i < inlierIndices.Count; i++ ) {
								toRemove.Add( i );
							}
							for( int i = 0; i <= endIdx; i++ ) {
								toRemove.Add( i );
							}
						}
						hasFiltered = true;
					}
				}

				// step 4: remove outlier points
				if( hasFiltered && toRemove.Count > 0 ) {
					// remove from back to avoid index shift
					toRemove.Sort();
					toRemove.Reverse();
					foreach( int idx in toRemove ) {
						inlierIndices.RemoveAt( idx );
					}
				}
				else {
					// no new outlier segments, converged
					break;
					;
				}

				// check if enough points remain
				if( inlierIndices.Count < 10 ) {
					return false;
				}
			}

			// final fit
			List<gp_Pnt2d> finalPoints = new List<gp_Pnt2d>();
			foreach( int idx in inlierIndices ) {
				finalPoints.Add( points[ idx ] );
			}

			return FitCircle2D( finalPoints, out center, out radius );
		}

		static List<(int startIdx, int endIdx)> DetectOutlierSegments(
			List<int> inlierIndices,
			List<gp_Pnt2d> allPoints,
			gp_Pnt2d center,
			double radius,
			double threshold
		)
		{
			List<(int, int)> segments = new List<(int, int)>();

			if( inlierIndices == null || inlierIndices.Count == 0 ) {
				return segments;
			}

			// mark outlier points
			bool[] isOutlier = new bool[ inlierIndices.Count ];
			for( int i = 0; i < inlierIndices.Count; i++ ) {
				int pointIdx = inlierIndices[ i ];
				gp_Pnt2d pt = allPoints[ pointIdx ];
				double dist = pt.Distance( center );
				double deviation = Math.Abs( dist - radius );
				isOutlier[ i ] = deviation > threshold;
			}

			// find continuous segments
			int startIdx = -1;
			for( int i = 0; i < inlierIndices.Count; i++ ) {
				if( isOutlier[ i ] ) {
					if( startIdx == -1 ) {
						startIdx = i;
					}
				}
				else {
					if( startIdx != -1 ) {
						segments.Add( (startIdx, i - 1) );
						startIdx = -1;
					}
				}
			}

			// handle ring connection
			if( startIdx != -1 ) {
				int endIdx = 0;
				while( endIdx < inlierIndices.Count && isOutlier[ endIdx ] ) {
					endIdx++;
				}

				if( endIdx > 0 ) {
					// merge head and tail segments
					segments.Add( (startIdx, endIdx - 1) );
				}
				else {
					segments.Add( (startIdx, inlierIndices.Count - 1) );
				}
			}

			return segments;
		}

		static bool IsProtrusionPattern(
			List<gp_Pnt2d> segmentPoints,
			gp_Pnt2d center,
			double radius
		)
		{
			if( segmentPoints == null || segmentPoints.Count < 3 || center == null ) {
				return false;
			}

			// calculate deviation to circle (with sign)
			List<double> deviations = new List<double>();
			foreach( var pt in segmentPoints ) {
				double dist = pt.Distance( center );
				double deviation = dist - radius;
				deviations.Add( deviation );
			}

			// check pattern:
			// 1. head/tail deviation near 0 (close to circle)tion near 0 (close to circle)
			// 2. obvious peak or valley in middle

			double firstDev = Math.Abs( deviations[ 0 ] );
			double lastDev = Math.Abs( deviations[ deviations.Count - 1 ] );
			double maxAbsDev = deviations.Max( d => Math.Abs( d ) );

			// ends near circle (deviation < threshold)
			bool endsNearCircle = firstDev < radius * 0.1 && lastDev < radius * 0.1;

			// middle has significant deviation
			bool hasSignificantPeak = maxAbsDev > radius * 0.15;

			return endsNearCircle && hasSignificantPeak;
		}

		static bool TryFitRectangleToWireProjection(
			   TopoDS_Wire wire,
			   gp_Dir viewDirection,
			   out gp_Pnt center3D,
			   out double width,
			   out double height,
			   out double rotation,
			   out double confidenceRatio,
			   double toleranceRatio = 0.1,
			   double minOverlapRatio = 0.75 )
		{
			center3D = null;
			width = 0.0;
			height = 0.0;
			rotation = 0.0;
			confidenceRatio = 0.0;

			if( wire == null || wire.IsNull() || viewDirection == null ) {
				return false;
			}

			gp_Ax2 projectionPlane = CreateProjectionPlane( wire, viewDirection );

			// prjotect wire to 2d plane, get 2d points
			List<gp_Pnt2d> points2D = ProjectWireTo2D( wire, projectionPlane, 300 );
			if( points2D == null || points2D.Count < 4 ) {
				return false;
			}

			//  fit 2d rectangle using obb
			gp_Pnt2d center2D;
			if( !FitRectangle2D_OBB( points2D, out center2D, out width, out height, out rotation ) ) {
				return false;
			}

			// calculate overlap ratio
			confidenceRatio = CalculateRectangleOverlapRatio(
				points2D, center2D, width, height, rotation, toleranceRatio
			);

			// check if can fit with rectangle
			if( confidenceRatio < minOverlapRatio ) {
				return false;
			}

			// convert 2d center back to 3d
			center3D = Convert2DTo3D( center2D, projectionPlane );

			return true;
		}

		static bool FitRectangle2D_OBB(
			List<gp_Pnt2d> points,
			out gp_Pnt2d center,
			out double width,
			out double height,
			out double rotation
		)
		{
			center = null;
			width = 0.0;
			height = 0.0;
			rotation = 0.0;

			if( points == null || points.Count < 4 ) {
				return false;
			}

			// step 1: convert 2d points to 3d (z=0 plane)
			BRepBuilderAPI_MakePolygon polygonMaker = new BRepBuilderAPI_MakePolygon();
			foreach( var pt2d in points ) {
				gp_Pnt pt3d = new gp_Pnt( pt2d.X(), pt2d.Y(), 0.0 );
				polygonMaker.Add( pt3d );
			}

			if( !polygonMaker.IsDone() ) {
				return false;
			}

			TopoDS_Wire wire = polygonMaker.Wire();

			// step 2: calculate minimum obb
			Bnd_OBB obb = new Bnd_OBB();
			BRepBndLib.AddOBB( wire, ref obb, true, true, true );

			if( obb.IsVoid() ) {
				return false;
			}

			// step 3: extract center, size and direction from obb
			gp_XYZ obbCenterXYZ = obb.Center();
			gp_Pnt obbCenter = new gp_Pnt( obbCenterXYZ );
			center = new gp_Pnt2d( obbCenter.X(), obbCenter.Y() );

			// obb returns half length (multiply by 2)
			double xHalfSize = obb.XHSize();
			double yHalfSize = obb.YHSize();

			// choose larger and smaller as width and height
			if( xHalfSize >= yHalfSize ) {
				width = xHalfSize * 2.0;
				height = yHalfSize * 2.0;
			}
			else {
				width = yHalfSize * 2.0;
				height = xHalfSize * 2.0;
			}

			// step 4: get obb x direction as rotation
			gp_XYZ obbXDirXYZ = obb.XDirection();
			gp_Dir obbXDir = new gp_Dir( obbXDirXYZ );

			// ensure using direction corresponding to width
			if( xHalfSize >= yHalfSize ) {
				// x direction corresponds to width
				rotation = Math.Atan2( obbXDir.Y(), obbXDir.X() );
			}
			else {
				// y direction corresponds to width, rotate 90 degrees
				gp_XYZ obbYDirXYZ = obb.YDirection();
				gp_Dir obbYDir = new gp_Dir( obbYDirXYZ );
				rotation = Math.Atan2( obbYDir.Y(), obbYDir.X() );
			}

			return width > ACCURACY_Threshold && height > ACCURACY_Threshold;
		}

		static double CalculateRectangleOverlapRatio(
			List<gp_Pnt2d> points,
			gp_Pnt2d center,
			double width,
			double height,
			double rotation,
			double toleranceRatio
		)
		{
			if( points == null || points.Count == 0 ) {
				return 0.0;
			}

			// tolerance (based on rectangle diagonal)
			double diagonal = Math.Sqrt( width * width + height * height );
			double tolerance = diagonal * toleranceRatio;

			int inlierCount = 0;
			double cosR = Math.Cos( rotation );
			double sinR = Math.Sin( rotation );

			foreach( var pt in points ) {
				// convert point to rectangle local coord system
				double dx = pt.X() - center.X();
				double dy = pt.Y() - center.Y();

				double localX = dx * cosR + dy * sinR;
				double localY = -dx * sinR + dy * cosR;

				// calculate distance to rectangle boundary
				double distX = Math.Max( 0, Math.Abs( localX ) - width / 2.0 );
				double distY = Math.Max( 0, Math.Abs( localY ) - height / 2.0 );
				double distToRect = Math.Sqrt( distX * distX + distY * distY );

				if( distToRect <= tolerance ) {
					inlierCount++;
				}
			}

			return (double)inlierCount / points.Count;
		}

		class SamplePoint
		{
			public TopoDS_Edge Edge { get; set; }
			public double Parameter { get; set; }
			public gp_Pnt Point { get; set; }
		}

		static TopoDS_Face FindFaceForEdge( TopoDS_Edge edge, TopoDS_Shape shell )
		{
			if( edge == null || edge.IsNull() || shell == null || shell.IsNull() ) {
				return null;
			}

			// traverse all faces in shell
			TopExp_Explorer expFace = new TopExp_Explorer( shell, TopAbs_ShapeEnum.TopAbs_FACE );
			while( expFace.More() ) {
				TopoDS_Face face = TopoDS.ToFace( expFace.Current() );

				// check if this face contains the edge
				TopExp_Explorer expEdge = new TopExp_Explorer( face, TopAbs_ShapeEnum.TopAbs_EDGE );
				while( expEdge.More() ) {
					TopoDS_Edge faceEdge = TopoDS.ToEdge( expEdge.Current() );
					if( faceEdge.IsSame( edge ) ) {
						return face;
					}
					expEdge.Next();
				}

				expFace.Next();
			}

			return null;
		}

		static double CalculateEdgeLength( TopoDS_Edge edge )
		{
			OCC.GProp.GProp_GProps props = new OCC.GProp.GProp_GProps();
			OCC.BRepGProp.BRepGProp.LinearProperties( edge, ref props );
			return props.Mass();
		}

		static double GetFaceArea( TopoDS_Face face )
		{
			if( face == null || face.IsNull() ) {
				return 0.0;
			}

			OCC.GProp.GProp_GProps props = new OCC.GProp.GProp_GProps();
			OCC.BRepGProp.BRepGProp.SurfaceProperties( face, ref props );
			return props.Mass();
		}

		static gp_Dir GetSurfaceNormal( TopoDS_Edge edge, TopoDS_Face face, double param )
		{
			return GeometryTool.GetSurfaceNormal( edge, face, param );
		}

		static double CalculateAngleBetweenDirections( gp_Dir dir1, gp_Dir dir2, bool allowOpposite = false )
		{
			// calculate dot product
			double dotProduct = dir1.X() * dir2.X()
							  + dir1.Y() * dir2.Y()
							  + dir1.Z() * dir2.Z();

			// opposite directions are allowed
			if( allowOpposite ) {
				dotProduct = Math.Abs( dotProduct );
			}

			// Clamp to [-1, 1]
			dotProduct = Math.Max( -1.0, Math.Min( 1.0, dotProduct ) );
			return Math.Acos( dotProduct );
		}

		static double EvaluateDirectionAngleConsistency( gp_Dir direction, List<gp_Dir> normals )
		{
			if( direction == null || normals == null || normals.Count == 0 ) {
				return 0.0;
			}

			int n = normals.Count;

			// Calculate angle between each normal vector and evaluation vector
			double[] angles = new double[ n ];
			for( int i = 0; i < n; i++ ) {
				angles[ i ] = CalculateAngleBetweenDirections( direction, normals[ i ], allowOpposite: true );
			}

			// Calculate average angle
			double meanAngle = 0;
			foreach( double angle in angles ) {
				meanAngle += angle;
			}
			meanAngle /= n;

			// Calculate standard deviation of angles
			double variance = 0;
			foreach( double angle in angles ) {
				double diff = angle - meanAngle;
				variance += diff * diff;
			}
			variance /= n;
			double stdDev = Math.Sqrt( variance );

			// Quality metric: smaller standard deviation indicates higher quality
			// Maximum possible standard deviation is approximately π/2 (90 degrees), used for normalization
			const double MaxExpectedStdDev = Math.PI / 2.0;
			double quality = Math.Max( 0.0, 1.0 - ( stdDev / MaxExpectedStdDev ) );

			return quality;
		}

		static TopoDS_Shape SewFaces( List<TopoDS_Face> faces )
		{
			if( faces == null || faces.Count == 0 ) {
				return null;
			}

			BRepBuilderAPI_Sewing sewing = new BRepBuilderAPI_Sewing();
			foreach( TopoDS_Face face in faces ) {
				sewing.Add( face );
			}
			sewing.Perform();
			return sewing.SewedShape();
		}

		static bool IsClosedShell( List<TopoDS_Face> faceList, out TopoDS_Shape colsedShell )
		{
			colsedShell = new TopoDS_Shape();
			if( faceList == null || faceList.Count == 0 ) {
				return false;
			}

			// Step 1: Sew all faces into one shape
			TopoDS_Shape sewedShape = SewFaces( faceList );
			if( sewedShape == null || sewedShape.IsNull() ) {
				return false;
			}

			// Step 2: Check free boundaries using ShapeAnalysis_FreeBounds
			ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( sewedShape, false );

			// Step 3: Check if there are open wires (free edges)
			TopoDS_Compound openWires = freeBounds.GetOpenWires();

			// If there are no open boundaries, it's a closed shell
			TopExp_Explorer explorer = new TopExp_Explorer( openWires, TopAbs_ShapeEnum.TopAbs_WIRE );
			bool hasOpenWires = explorer.More();

			// No open edges = closed
			if( !hasOpenWires ) {
				colsedShell = sewedShape;
				return true;
			}
			return false;
		}

		enum AxisExtreme
		{
			XMin,
			XMax,
			YMin,
			YMax,
			ZMin,
			ZMax
		}

		static bool GetFaceByD1ContinueSuccessed( TopoDS_Face outerStartFace, TopTools_IndexedDataMapOfShapeListOfShape edgeFaceMap, ref TopTools_IndexedDataMapOfShapeListOfShape indexedDataMapOfShapeListOfShape, out List<TopoDS_Face> outerFaceList )
		{
			List<TopoDS_Face> targetFaceList = new List<TopoDS_Face> { outerStartFace };
			outerFaceList = GeometryTool.FindD1ContinuousFaces( targetFaceList, edgeFaceMap );
			if( outerFaceList == null || outerFaceList.Count == 0 ) {
				return false;
			}
			return true;
		}

		static gp_Pnt CalculateWireCenter( TopoDS_Wire wire )
		{
			if( wire == null || wire.IsNull() ) {
				return null;
			}

			// Calculate center point using Bounding Box
			BoundingBox bbox = new BoundingBox( wire );
			gp_Pnt center = new gp_Pnt( bbox.XCenter, bbox.YCenter, bbox.ZCenter );
			return center;
		}

		static bool IsWireAllmostFromPlane(
			TopoDS_Wire wire,
			TopoDS_Shape shell,
			out gp_Dir largestPlaneNormal,
			double threshold = 0.5,
			double minFaceAreaRatio = 0.01
		)
		{
			largestPlaneNormal = null;

			try {
				// Step 1: Validate input
				if( wire == null || wire.IsNull() || shell == null || shell.IsNull() ) {
					return false;
				}

				// Step 2: Get all faces that compose the wire
				List<TopoDS_Face> faces = GetFacesForWire( wire, shell );
				if( faces == null || faces.Count == 0 ) {
					return false;
				}

				// Step 3: Calculate total area
				double totalArea = 0.0;
				List<(TopoDS_Face face, double area)> faceAreas = new List<(TopoDS_Face, double)>();

				foreach( TopoDS_Face face in faces ) {
					double area = GetFaceArea( face );
					if( area > ACCURACY_Threshold ) {
						faceAreas.Add( (face, area) );
						totalArea += area;
					}
				}

				if( totalArea < ACCURACY_Threshold ) {
					return false;
				}

				// Step 4: Filter out faces with too small area
				double minAreaThreshold = totalArea * minFaceAreaRatio;
				List<(TopoDS_Face face, double area)> filteredFaces = faceAreas
					.Where( fa => fa.area >= minAreaThreshold )
					.ToList();

				if( filteredFaces.Count == 0 ) {
					return false;
				}

				// Step 5: Find the face with the largest area (not limited to planes)
				var largestFace = filteredFaces.OrderByDescending( fa => fa.area ).First();
				double largestFaceRatio = largestFace.area / totalArea;

				// Step 6: Check if the largest face area exceeds threshold
				if( largestFaceRatio < threshold ) {
					return false;
				}

				// Step 7: Check if the largest face is a plane
				if( GeometryTool.IsPlane( largestFace.face, out gp_Pnt center, out gp_Dir normal ) ) {
					largestPlaneNormal = normal;
					return true;
				}

				// Largest face is not a plane
				return false;
			}
			catch {
				return false;
			}
		}

		static double CalculateWireSize( TopoDS_Wire wire )
		{
			const double DEFAULT_LENGTH = 0;

			if( wire == null || wire.IsNull() ) {
				return DEFAULT_LENGTH;
			}

			// Calculate the Oriented Bounding Box (OBB) for the wire
			Bnd_OBB obb = new Bnd_OBB();
			BRepBndLib.AddOBB( wire, ref obb, true, true, true );

			if( obb.IsVoid() ) {
				return DEFAULT_LENGTH;
			}

			// Get the half-sizes (half-length, half-width, half-height) of the OBB
			double halfX = obb.XHSize();
			double halfY = obb.YHSize();
			double halfZ = obb.ZHSize();

			// Calculate full dimensions
			double length = halfX * 2.0;
			double width = halfY * 2.0;
			double height = halfZ * 2.0;

			// Return the sum of length, width, and height
			return length + width + height;
		}

		static List<TopoDS_Edge> GetEdgesFromWire( TopoDS_Wire wire )
		{
			List<TopoDS_Edge> edges = new List<TopoDS_Edge>();

			if( wire == null || wire.IsNull() ) {
				return edges;
			}

			TopExp_Explorer edgeExp = new TopExp_Explorer( wire, TopAbs_ShapeEnum.TopAbs_EDGE );
			while( edgeExp.More() ) {
				TopoDS_Edge edge = TopoDS.ToEdge( edgeExp.Current() );
				if( edge != null && !edge.IsNull() ) {
					edges.Add( edge );
				}
				edgeExp.Next();
			}

			return edges;
		}

		static List<TopoDS_Face> GetFacesForWire( TopoDS_Wire wire, TopoDS_Shape shell )
		{
			List<TopoDS_Face> faceList = new List<TopoDS_Face>();

			if( wire == null || wire.IsNull() || shell == null || shell.IsNull() ) {
				return faceList;
			}

			// get all edges from wire
			TopExp_Explorer edgeExp = new TopExp_Explorer( wire, TopAbs_ShapeEnum.TopAbs_EDGE );
			while( edgeExp.More() ) {
				TopoDS_Edge edge = TopoDS.ToEdge( edgeExp.Current() );

				if( edge != null && !edge.IsNull() ) {

					// find the face(s) in the shell that contain this edge
					TopoDS_Face face = FindFaceForEdge( edge, shell );

					if( face != null && !face.IsNull() ) {

						// avoid duplicates
						bool alreadyAdded = false;
						foreach( TopoDS_Face existingFace in faceList ) {
							if( existingFace.IsSame( face ) ) {
								alreadyAdded = true;
								break;
							}
						}
						if( !alreadyAdded ) {
							faceList.Add( face );
						}
					}
				}
				edgeExp.Next();
			}
			return faceList;
		}

		#region Debug Helpers

		static TopoDS_Edge CreateFittedCircleEdge( gp_Pnt2d center2D, double radius, gp_Ax2 projectionPlane )
		{
			if( center2D == null || radius <= 0 || projectionPlane == null ) {
				return null;
			}

			// step 1: convert 2d center to 3d
			gp_Pnt center3D = Convert2DTo3D( center2D, projectionPlane );
			if( center3D == null ) {
				return null;
			}

			// step 2: create circle in 3d space
			gp_Ax2 circleAxis = new gp_Ax2(
			center3D,
			projectionPlane.Direction(),  // normal vector
			projectionPlane.XDirection()  // x direction
		);

			gp_Circ circle = new gp_Circ( circleAxis, radius );

			// step 3: create full circle edge
			BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( circle );

			if( !edgeMaker.IsDone() ) {
				return null;
			}

			return edgeMaker.Edge();
		}

		static List<TopoDS_Edge> GetPntNormal( List<gp_Pnt> points, List<gp_Dir> normals )
		{
			if( points == null || normals == null || points.Count != normals.Count ) {
				return new List<TopoDS_Edge>();
			}
			const double normalLength = 10.0;
			List<TopoDS_Edge> debugNormalEdges = new List<TopoDS_Edge>();

			for( int i = 0; i < points.Count; i++ ) {
				gp_Pnt startPnt = points[ i ];
				gp_Vec normalVec = new gp_Vec( normals[ i ] );
				normalVec.Multiply( normalLength );
				gp_Pnt endPnt = new gp_Pnt(
					startPnt.X() + normalVec.X(),
					startPnt.Y() + normalVec.Y(),
					startPnt.Z() + normalVec.Z()
				);

				BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( startPnt, endPnt );
				if( edgeMaker.IsDone() ) {
					debugNormalEdges.Add( edgeMaker.Edge() );
				}
			}
			return debugNormalEdges;
		}

		static gp_Pnt CalculatePointAlongDirection( gp_Pnt startPoint, gp_Dir direction, double length )
		{
			// step 1: validate input
			if( startPoint == null || direction == null || length <= 0 ) {
				return null;
			}

			// step 2: calculate end point
			gp_Vec offset = new gp_Vec( direction );
			offset.Multiply( length );

			return new gp_Pnt(
				startPoint.X() + offset.X(),
				startPoint.Y() + offset.Y(),
				startPoint.Z() + offset.Z()
			);
		}

		#endregion
	}
}
