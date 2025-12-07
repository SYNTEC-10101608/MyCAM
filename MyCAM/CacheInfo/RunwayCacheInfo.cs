using MyCAM.Data;
using MyCAM.Data.GeomDataFolder;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.CacheInfo
{
	public class RunwayCacheInfo : IStartPnt
	{
		public RunwayCacheInfo( string szID, gp_Ax3 coordinateInfo, RunwayGeomData runwayGeomData, CraftData craftData )
		{
			if( string.IsNullOrEmpty( szID ) || runwayGeomData == null || craftData == null ) {
				throw new ArgumentNullException( "RunwayCacheInfo constructing argument null" );
			}
			UID = szID;
			m_CoordinateInfo = coordinateInfo;
			m_RunwayGeomData = runwayGeomData;
			m_CraftData = craftData;
			m_CraftData.ParameterChanged += SetCraftDataDirty;
			BuildCAMPointList();
		}
		public string UID
		{
			get; private set;
		}

		public PathType PathType
		{
			get
			{
				return PathType.Runway;
			}
		}

		public List<CAMPoint> StartPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_StartPointList;
			}
		}

		public List<CAMPoint> LeadInCAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_LeadInCAMPointList;
			}
		}

		public List<CAMPoint> LeadOutCAMPointList
		{
			get;
		}

		public List<CAMPoint> OverCutCAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_OverCutPointList;
			}
		}

		public bool IsClosed
		{
			get
			{
				return true;
			}
		}

		public CAMPoint GetProcessRefPoint()
		{
			// Calculate runway parameters
			double length = m_RunwayGeomData.Length;
			double width = m_RunwayGeomData.Width;
			double radius = width / 2.0;
			double straightLength = length - width;

			// Left arc center position in local coordinate system
			gp_Pnt leftArcCenter;

			if( straightLength <= 0.001 ) {

				// Pure circle case: center is at origin
				leftArcCenter = new gp_Pnt( 0, 0, 0 );
			}
			else {

				// Runway shape: left arc center is at (-straightLength/2, 0, 0)
				double halfStraight = straightLength / 2.0;
				leftArcCenter = new gp_Pnt( -halfStraight, 0, 0 );
			}

			// Transform local coordinates to world coordinate system
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				m_CoordinateInfo.Location(),
				m_CoordinateInfo.Direction(),
				m_CoordinateInfo.XDirection()
			);
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );
			gp_Pnt worldLeftArcCenter = leftArcCenter.Transformed( transformation );

			return new CAMPoint(
				new CADPoint(
					worldLeftArcCenter,
					m_CoordinateInfo.Direction(),
					m_CoordinateInfo.XDirection(),
					m_CoordinateInfo.YDirection()
				),
				m_CoordinateInfo.Direction()
			);
		}

		public CAMPoint GetProcessStartPoint()
		{
			if( LeadInCAMPointList.Count != 0 ) {
				return LeadInCAMPointList[ 0 ].Clone();
			}

			return m_StartPointList[ m_CraftData.StartPointIndex ].Clone();
		}

		public CAMPoint GetProcessEndPoint()
		{
			if( OverCutCAMPointList.Count != 0 ) {
				return OverCutCAMPointList[ OverCutCAMPointList.Count - 1 ].Clone();
			}

			return m_StartPointList[ m_CraftData.StartPointIndex ].Clone();
		}

		public gp_Pnt GetMainPathStartPoint()
		{
			return m_StartPointList[ m_CraftData.StartPointIndex ].Point;
		}

		public void DoTransform( gp_Trsf transform )
		{
			m_CoordinateInfo.Transform( transform );
			BuildCAMPointList();
		}

		void BuildCAMPointList()
		{
			m_IsCraftDataDirty = false;
			m_StartPointList = RunwayCacheInfoExtensions.GetStartPointList( m_CoordinateInfo, m_RunwayGeomData.Length, m_RunwayGeomData.Width );

			// set tool vector
			List<ISetToolVecPoint> startPointList = m_StartPointList.Cast<ISetToolVecPoint>().ToList();
			ToolVecHelper.SetToolVec( ref startPointList, m_CraftData.ToolVecModifyMap, IsClosed, m_CraftData.IsToolVecReverse );

			// set lead in
			IOrientationPoint startPoint = m_StartPointList[ m_CraftData.StartPointIndex ];
			List<IOrientationPoint> startPntList = new List<IOrientationPoint> { startPoint };
			LeadHelper.SetLeadIn( startPntList, out List<IOrientationPoint> leadInPointList, m_CraftData.LeadLineParam, m_CraftData.IsReverse );

			m_LeadInCAMPointList = leadInPointList.Cast<CAMPoint>().ToList();
			SetOverCut();
		}

		void SetOverCut()
		{
			m_OverCutPointList.Clear();
			if( m_StartPointList.Count == 0 || m_CraftData.OverCutLength <= 0 || !IsClosed ) {
				return;
			}

			// generate detailed path points for overcut calculation
			List<CAMPoint> detailedPathPoints = GenerateDetailedRunwayPath();
			if( detailedPathPoints.Count == 0 ) {
				return;
			}

			double runwayPerimeter = GetRunwayPerimeter();

			// if overcut length is greater than or equal to full perimeter, limit to one full revolution
			double actualOverCutLength = m_CraftData.OverCutLength;
			if( actualOverCutLength >= runwayPerimeter ) {
				actualOverCutLength = runwayPerimeter - 0.001; // just under one full revolution
			}

			// find the starting point in detailed path that corresponds to our start point index
			int detailedStartIndex = FindCorrespondingDetailedPathIndex( detailedPathPoints );

			// overcut starts from the start point (after completing full contour)
			m_OverCutPointList.Add( detailedPathPoints[ detailedStartIndex ].Clone() );

			double dTotalOverCutLength = 0;

			// traverse along the runway path in clockwise direction from start point
			for( int i = 0; i < detailedPathPoints.Count; i++ ) {
				int currentIndex = ( detailedStartIndex + i ) % detailedPathPoints.Count;
				int nextIndex = ( detailedStartIndex + i + 1 ) % detailedPathPoints.Count;

				// get this edge distance
				double dDistance = detailedPathPoints[ currentIndex ].Point.Distance( detailedPathPoints[ nextIndex ].Point );

				if( dTotalOverCutLength + dDistance < actualOverCutLength ) {
					// still within overcut length → take next point directly
					m_OverCutPointList.Add( detailedPathPoints[ nextIndex ].Clone() );
					dTotalOverCutLength += dDistance;
				}
				else {
					// need to stop inside this segment
					double dRemain = actualOverCutLength - dTotalOverCutLength;
					if( dRemain <= 0.001 ) {
						return;
					}

					// compute new point along segment
					gp_Pnt overCutEndPoint = GetExactOverCutEndPoint(
						detailedPathPoints[ currentIndex ].Point,
						detailedPathPoints[ nextIndex ].Point,
						dRemain );

					// interpolate tool and tangent vectors
					InterpolateToolAndTangentVecBetween2CAMPoint(
						detailedPathPoints[ currentIndex ],
						detailedPathPoints[ nextIndex ],
						overCutEndPoint,
						out gp_Dir endPointToolVec,
						out gp_Dir endPointTangentVec );

					// create new cam point
					CADPoint cadPoint = new CADPoint( overCutEndPoint, endPointToolVec, endPointToolVec, endPointTangentVec );
					CAMPoint camPoint = new CAMPoint( cadPoint, endPointToolVec );
					m_OverCutPointList.Add( camPoint );
					return;
				}
			}
		}

		int FindCorrespondingDetailedPathIndex( List<CAMPoint> detailedPathPoints )
		{
			// find the point in detailed path that is closest to our start point
			if( m_CraftData.StartPointIndex >= m_StartPointList.Count ) {
				return 0;
			}

			CAMPoint startPoint = m_StartPointList[ m_CraftData.StartPointIndex ];
			double minDistance = double.MaxValue;
			int bestIndex = 0;

			for( int i = 0; i < detailedPathPoints.Count; i++ ) {
				double distance = startPoint.Point.Distance( detailedPathPoints[ i ].Point );
				if( distance < minDistance ) {
					minDistance = distance;
					bestIndex = i;
				}
			}

			return bestIndex;
		}

		List<CAMPoint> GenerateDetailedRunwayPath()
		{
			List<CAMPoint> detailedPoints = new List<CAMPoint>();

			// get runway parameters
			double length = m_RunwayGeomData.Length;
			double width = m_RunwayGeomData.Width;
			double radius = width / 2.0;
			double straightLength = length - width;

			// create coordinate system transformation
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				m_CoordinateInfo.Location(),
				m_CoordinateInfo.Direction(),
				m_CoordinateInfo.XDirection()
			);
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );

			// handle special case: pure circle (straight segment is 0)
			if( straightLength <= 0.001 ) {
				GenerateCirclePath( detailedPoints, radius, transformation );
			}
			else {
				GenerateRunwayPath( detailedPoints, straightLength, radius, transformation );
			}

			return detailedPoints;
		}

		void GenerateCirclePath( List<CAMPoint> detailedPoints, double radius, gp_Trsf transformation )
		{
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );

			// generate full circle path starting from (radius, 0) in clockwise direction
			double circumference = 2.0 * Math.PI * radius;
			int segments = Math.Max( 1, (int)Math.Ceiling( circumference / 1 ) );

			for( int i = 0; i < segments; i++ ) {
				double t = (double)i / segments;
				double angle = -t * 2 * Math.PI; // negative for clockwise: 0 to -2π

				gp_Pnt circlePoint = new gp_Pnt(
					radius * Math.Cos( angle ),
					radius * Math.Sin( angle ),
					0
				);

				// tangent direction (clockwise)
				gp_Dir tangent = new gp_Dir( -Math.Sin( angle ), Math.Cos( angle ), 0 );
				// normal direction (inward)
				gp_Dir normal = new gp_Dir( -Math.Cos( angle ), -Math.Sin( angle ), 0 );

				CADPoint cadPoint = new CADPoint(
					circlePoint.Transformed( transformation ),
					toolVec.Transformed( transformation ),
					normal.Transformed( transformation ),
					tangent.Transformed( transformation )
				);

				detailedPoints.Add( new CAMPoint( cadPoint, toolVec.Transformed( transformation ) ) );
			}
		}

		void GenerateRunwayPath( List<CAMPoint> detailedPoints, double straightLength, double radius, gp_Trsf transformation )
		{
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double halfStraight = straightLength / 2.0;

			// define runway vertices in clockwise order starting from right arc middle
			List<gp_Pnt> vertices = new List<gp_Pnt>
			{
				new gp_Pnt( halfStraight + radius, 0, 0 ),          // right arc middle (start)
				new gp_Pnt( halfStraight, -radius, 0 ),             // right arc bottom
				new gp_Pnt( -halfStraight, -radius, 0 ),            // left arc bottom
				new gp_Pnt( -halfStraight - radius, 0, 0 ),         // left arc middle
				new gp_Pnt( -halfStraight, radius, 0 ),             // left arc top
				new gp_Pnt( halfStraight, radius, 0 ),              // right arc top
				new gp_Pnt( halfStraight + radius, 0, 0 )           // back to start
			};

			// path segments: alternating between arcs and straight lines
			for( int i = 0; i < vertices.Count - 1; i++ ) {
				gp_Pnt startPnt = vertices[ i ];
				gp_Pnt endPnt = vertices[ i + 1 ];

				if( i % 2 == 1 ) {
					// odd indices: straight line segments
					AddStraightSegmentPoints( detailedPoints, startPnt, endPnt, toolVec, transformation );
				}
				else {
					// even indices: arc segments
					gp_Pnt centerPnt;
					double startAngle, endAngle;

					if( i == 0 ) {
						// right arc: from middle to bottom
						centerPnt = new gp_Pnt( halfStraight, 0, 0 );
						startAngle = 0;          // 0° (right)
						endAngle = -Math.PI / 2; // -90° (bottom)
					}
					else if( i == 2 ) {
						// left arc: from bottom to middle
						centerPnt = new gp_Pnt( -halfStraight, 0, 0 );
						startAngle = -Math.PI / 2; // -90° (bottom)
						endAngle = -Math.PI;       // -180° (left)
					}
					else if( i == 4 ) {
						// left arc: from middle to top
						centerPnt = new gp_Pnt( -halfStraight, 0, 0 );
						startAngle = Math.PI;    // 180° (left)
						endAngle = Math.PI / 2;  // 90° (top)
					}
					else { // i == 6
						   // right arc: from top to middle
						centerPnt = new gp_Pnt( halfStraight, 0, 0 );
						startAngle = Math.PI / 2; // 90° (top)
						endAngle = 0;             // 0° (right)
					}

					AddArcSegmentPoints( detailedPoints, centerPnt, radius, startAngle, endAngle, toolVec, transformation );
				}
			}
		}

		void AddStraightSegmentPoints( List<CAMPoint> detailedPoints, gp_Pnt startPnt, gp_Pnt endPnt, gp_Dir toolVec, gp_Trsf transformation )
		{
			// calculate edge direction and normal
			gp_Vec edgeVec = new gp_Vec( startPnt, endPnt );
			double edgeLength = edgeVec.Magnitude();
			if( edgeLength <= 0.001 ) {
				return; // skip zero-length edges
			}

			edgeVec.Normalize();
			gp_Dir tangent = new gp_Dir( edgeVec );

			// normal pointing inward (rotate tangent 90 degrees clockwise in XY plane)
			gp_Dir normal = new gp_Dir( edgeVec.Y(), -edgeVec.X(), 0 );

			int segments = Math.Max( 1, (int)Math.Ceiling( edgeLength / 1 ) );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				gp_Pnt edgePoint = new gp_Pnt(
					startPnt.X() + t * ( endPnt.X() - startPnt.X() ),
					startPnt.Y() + t * ( endPnt.Y() - startPnt.Y() ),
					0
				);

				CADPoint cadPoint = new CADPoint(
					edgePoint.Transformed( transformation ),
					toolVec.Transformed( transformation ),
					normal.Transformed( transformation ),
					tangent.Transformed( transformation )
				);

				detailedPoints.Add( new CAMPoint( cadPoint, toolVec.Transformed( transformation ) ) );
			}
		}

		void AddArcSegmentPoints( List<CAMPoint> detailedPoints, gp_Pnt centerPnt, double radius, double startAngle, double endAngle, gp_Dir toolVec, gp_Trsf transformation )
		{
			// for clockwise arcs, handle the direction correctly
			double angleDiff = endAngle - startAngle;

			// normalize to ensure we go clockwise
			if( angleDiff > Math.PI ) {
				angleDiff -= 2 * Math.PI; // make it negative for clockwise
			}
			else if( angleDiff < -Math.PI ) {
				angleDiff += 2 * Math.PI; // handle wrap-around
			}

			double arcLength = Math.Abs( angleDiff ) * radius;
			int segments = Math.Max( 1, (int)Math.Ceiling( arcLength / 1 ) );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle = startAngle + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					centerPnt.X() + radius * Math.Cos( angle ),
					centerPnt.Y() + radius * Math.Sin( angle ),
					0
				);

				// tangent direction along arc (clockwise)
				gp_Dir arcTangent = new gp_Dir( -Math.Sin( angle ), Math.Cos( angle ), 0 );
				// normal direction (radially inward)
				gp_Dir arcNormal = new gp_Dir( -Math.Cos( angle ), -Math.Sin( angle ), 0 );

				CADPoint cadPoint = new CADPoint(
					arcPoint.Transformed( transformation ),
					toolVec.Transformed( transformation ),
					arcNormal.Transformed( transformation ),
					arcTangent.Transformed( transformation )
				);

				detailedPoints.Add( new CAMPoint( cadPoint, toolVec.Transformed( transformation ) ) );
			}
		}

		double GetRunwayPerimeter()
		{
			double length = m_RunwayGeomData.Length;
			double width = m_RunwayGeomData.Width;
			double radius = width / 2.0;
			double straightLength = length - width;

			if( straightLength <= 0.001 ) {
				// pure circle
				return 2.0 * Math.PI * radius;
			}
			else {
				// runway: two straight edges + two semicircles
				return 2.0 * straightLength + 2.0 * Math.PI * radius;
			}
		}

		gp_Pnt GetExactOverCutEndPoint( gp_Pnt currentPoint, gp_Pnt nextPoint, double dDistanceMoveFromOverPoint )
		{
			// from currentPoint → nextOverLengthPoint
			gp_Vec movingVec = new gp_Vec( currentPoint, nextPoint );

			// normalize to unit vector
			movingVec.Normalize();

			gp_Vec moveVec = movingVec.Multiplied( dDistanceMoveFromOverPoint );

			// shifted along the vector
			return new gp_Pnt( currentPoint.XYZ() + moveVec.XYZ() );
		}

		gp_Dir InterpolateVecBetween2Vec( gp_Vec currentVec, gp_Vec nextVec, double interpolatePercent )
		{
			// this case is unsolvable, so just return current vec
			if( currentVec.IsOpposite( nextVec, 0.001 ) ) {
				return new gp_Dir( currentVec.XYZ() );
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( currentVec, nextVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			// calculate new point attitude
			gp_Quaternion q = new gp_Quaternion();
			slerp.Interpolate( interpolatePercent, ref q );
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetRotation( q );
			gp_Dir resultDir = new gp_Dir( currentVec.Transformed( trsf ) );
			return resultDir;
		}

		void InterpolateToolAndTangentVecBetween2CAMPoint( CAMPoint currentCAMPoint, CAMPoint nextCAMPoint, gp_Pnt point, out gp_Dir toolDir, out gp_Dir tangentDir )
		{
			toolDir = currentCAMPoint.ToolVec;
			tangentDir = currentCAMPoint.TangentVec;

			// get current and next tool vector
			gp_Vec currentVec = new gp_Vec( currentCAMPoint.ToolVec );
			gp_Vec nextVec = new gp_Vec( nextCAMPoint.ToolVec );

			// get current and next tangent vector
			gp_Vec currentTangentVec = new gp_Vec( currentCAMPoint.TangentVec );
			gp_Vec nextTangentVec = new gp_Vec( nextCAMPoint.TangentVec );

			// calculate new point percentage
			double dDistanceOfCAMPath2Point = currentCAMPoint.Point.Distance( nextCAMPoint.Point );
			double dDistanceBetweenCurrentPoint2NewPoint = currentCAMPoint.Point.Distance( point );

			// two point overlap
			if( dDistanceOfCAMPath2Point <= 0.001 ) {
				return;
			}
			double interpolatePercent = dDistanceBetweenCurrentPoint2NewPoint / dDistanceOfCAMPath2Point;

			// get new point dir
			toolDir = InterpolateVecBetween2Vec( currentVec, nextVec, interpolatePercent );
			tangentDir = InterpolateVecBetween2Vec( currentTangentVec, nextTangentVec, interpolatePercent );
		}

		void SetCraftDataDirty()
		{
			if( !m_IsCraftDataDirty ) {
				m_IsCraftDataDirty = true;
			}
		}

		RunwayGeomData m_RunwayGeomData;
		gp_Ax3 m_CoordinateInfo;
		string m_szPathID;
		List<CAMPoint> m_StartPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();
		CraftData m_CraftData;

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
	}

	internal static class RunwayCacheInfoExtensions
	{
		internal static List<CAMPoint> GetStartPointList( gp_Ax3 coordinateInfo, double length, double width )
		{
			gp_Pnt centerPoint = coordinateInfo.Location();
			gp_Pln plane = new gp_Pln( coordinateInfo );
			double radius = width / 2.0;
			double straightLength = length - width;

			// runway shape start points:
			// 1. arc midpoint intersecting with positive X-axis
			// 2. long edge midpoint intersecting with negative Y-axis
			double halfStraight = straightLength / 2.0;
			gp_Dir local_X_pos = gp.DX();
			gp_Dir local_X_neg = gp.DX().Reversed();
			gp_Dir local_Y_pos = gp.DY();
			gp_Dir local_Y_neg = gp.DY().Reversed();
			gp_Dir local_Z_pos = gp.DZ();

			// 1. arc midpoint intersecting with positive X-axis (right arc point on X-axis)
			gp_Pnt local_Pnt_RightArc = new gp_Pnt( halfStraight + radius, 0, 0 );
			gp_Dir local_N1_RightArc = local_Z_pos;
			gp_Dir local_N2_RightArc = local_X_neg;
			gp_Dir local_Tan_RightArc = local_Y_neg;
			gp_Dir local_Tool_RightArc = local_Z_pos;

			// 2. long edge midpoint intersecting with negative Y-axis (bottom edge midpoint)
			gp_Pnt local_Pnt_BottomEdge = new gp_Pnt( 0, -radius, 0 );
			gp_Dir local_N1_BottomEdge = local_Z_pos;
			gp_Dir local_N2_BottomEdge = local_Y_pos;
			gp_Dir local_Tan_BottomEdge = local_X_neg;
			gp_Dir local_Tool_BottomEdge = local_Z_pos;

			// create coordinate transformation
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				centerPoint,
				plane.Axis().Direction(),
				plane.XAxis().Direction()
			);

			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );

			// transform right arc point
			CADPoint cad_RightArc = new CADPoint(
				local_Pnt_RightArc.Transformed( transformation ),
				local_N1_RightArc.Transformed( transformation ),
				local_N2_RightArc.Transformed( transformation ),
				local_Tan_RightArc.Transformed( transformation )
			);
			CAMPoint cam_RightArc = new CAMPoint( cad_RightArc, local_Tool_RightArc.Transformed( transformation ) );

			// transform bottom edge midpoint
			CADPoint cad_BottomEdge = new CADPoint(
				local_Pnt_BottomEdge.Transformed( transformation ),
				local_N1_BottomEdge.Transformed( transformation ),
				local_N2_BottomEdge.Transformed( transformation ),
				local_Tan_BottomEdge.Transformed( transformation )
			);
			CAMPoint cam_BottomEdge = new CAMPoint( cad_BottomEdge, local_Tool_BottomEdge.Transformed( transformation ) );

			List<CAMPoint> resultList = new List<CAMPoint>
			{
				cam_RightArc,
				cam_BottomEdge,
			};

			return resultList;
		}
	}
}