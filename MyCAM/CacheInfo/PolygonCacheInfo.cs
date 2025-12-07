using MyCAM.Data;
using MyCAM.Data.GeomDataFolder;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.CacheInfo
{
	public class PolygonCacheInfo : IStartPnt
	{
		public PolygonCacheInfo( string szID, gp_Ax3 coordinateInfo, PolygonGeomData polygonGeomData, CraftData craftData )
		{
			if( string.IsNullOrEmpty( szID ) || polygonGeomData == null || craftData == null ) {
				throw new ArgumentNullException( "PolygonCacheInfo constructing argument null" );
			}
			UID = szID;
			m_CoordinateInfo = coordinateInfo;
			m_PolygonGeomData = polygonGeomData;
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
				return m_PolygonGeomData.PathType;
			}
		}

		#region Computation Result

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

		#endregion

		public bool IsClosed
		{
			get
			{
				return true;
			}
		}

		public CAMPoint GetProcessRefPoint()
		{
			return new CAMPoint( new CADPoint( m_CoordinateInfo.Location(), m_CoordinateInfo.Direction(), m_CoordinateInfo.XDirection(), m_CoordinateInfo.YDirection() ), m_CoordinateInfo.Direction() );
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
			m_StartPointList = PolygonCacheInfoExtensions.GetStartPointList( m_CoordinateInfo, m_PolygonGeomData.Sides, m_PolygonGeomData.SideLength );

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
			List<CAMPoint> detailedPathPoints = GenerateDetailedPolygonPath();
			if( detailedPathPoints.Count == 0 ) {
				return;
			}

			double polygonPerimeter = GetPolygonPerimeter();

			// if overcut length is greater than or equal to full perimeter, limit to one full revolution
			double actualOverCutLength = m_CraftData.OverCutLength;
			if( actualOverCutLength >= polygonPerimeter ) {
				actualOverCutLength = polygonPerimeter - 0.001; // just under one full revolution
			}

			// find the starting point in detailed path that corresponds to our start point index
			int detailedStartIndex = FindCorrespondingDetailedPathIndex( detailedPathPoints );

			// overcut starts from the start point (after completing full contour)
			m_OverCutPointList.Add( detailedPathPoints[ detailedStartIndex ].Clone() );

			double dTotalOverCutLength = 0;

			// traverse along the polygon path in clockwise direction from start point
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

		List<CAMPoint> GenerateDetailedPolygonPath()
		{
			List<CAMPoint> detailedPoints = new List<CAMPoint>();

			// get polygon parameters
			int sides = m_PolygonGeomData.Sides;
			double sideLength = m_PolygonGeomData.SideLength;
			double cornerRadius = m_PolygonGeomData.CornerRadius;

			// calculate circumradius (distance from center to vertex)
			double circumradius = sideLength / ( 2.0 * Math.Sin( Math.PI / sides ) );

			// limit corner radius to avoid overlap
			double maxCornerRadius = GetMaxCornerRadius( sides, sideLength );
			if( cornerRadius > maxCornerRadius ) {
				cornerRadius = maxCornerRadius;
			}

			// create coordinate system transformation
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				m_CoordinateInfo.Location(),
				m_CoordinateInfo.Direction(),
				m_CoordinateInfo.XDirection()
			);
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );

			// calculate angle offset based on polygon type
			double angleOffset = GetAngleOffset( sides );

			if( cornerRadius <= 0.001 ) {
				// sharp corners
				GenerateSharpPolygonPath( detailedPoints, sides, circumradius, angleOffset, transformation );
			}
			else {
				// rounded corners
				GenerateRoundedPolygonPath( detailedPoints, sides, sideLength, cornerRadius, angleOffset, transformation );
			}

			return detailedPoints;
		}

		void GenerateSharpPolygonPath( List<CAMPoint> detailedPoints, int sides, double circumradius, double angleOffset, gp_Trsf transformation )
		{
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double angleStep = 2.0 * Math.PI / sides;

			// generate polygon vertices in clockwise order
			List<gp_Pnt> vertices = new List<gp_Pnt>();

			for( int i = 0; i < sides; i++ ) {
				// clockwise: reverse the angle direction
				double angle = angleOffset - i * angleStep; // negative for clockwise
				gp_Pnt vertex = new gp_Pnt(
					circumradius * Math.Cos( angle ),
					circumradius * Math.Sin( angle ),
					0
				);
				vertices.Add( vertex );
			}

			// close the polygon
			vertices.Add( vertices[ 0 ] );

			// generate points along each edge
			for( int i = 0; i < vertices.Count - 1; i++ ) {
				gp_Pnt startPnt = vertices[ i ];
				gp_Pnt endPnt = vertices[ i + 1 ];

				// calculate edge direction and normal
				gp_Vec edgeVec = new gp_Vec( startPnt, endPnt );
				if( edgeVec.Magnitude() <= 0.001 ) {
					continue; // skip zero-length edges
				}

				edgeVec.Normalize();
				gp_Dir tangent = new gp_Dir( edgeVec );

				// normal pointing inward (rotate tangent 90 degrees clockwise in XY plane)
				gp_Dir normal = new gp_Dir( edgeVec.Y(), -edgeVec.X(), 0 );

				// add points along this edge
				double edgeLength = startPnt.Distance( endPnt );
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
		}

		void GenerateRoundedPolygonPath( List<CAMPoint> detailedPoints, int sides, double sideLength, double cornerRadius, double angleOffset, gp_Trsf transformation )
		{
			gp_Dir toolVec = new gp_Dir( 0, 0, 1 );
			double angleStep = 2.0 * Math.PI / sides;
			double circumradius = sideLength / ( 2.0 * Math.Sin( Math.PI / sides ) );

			// calculate interior angle and tangent distance (same logic as PatternFactory)
			double interiorAngle = ( sides - 2 ) * Math.PI / sides;
			double halfAngle = interiorAngle / 2.0;
			double tanHalfAngle = Math.Tan( halfAngle );
			double sinHalfAngle = Math.Sin( halfAngle );

			if( tanHalfAngle < 0.001 || sinHalfAngle < 0.001 ) {
				// fallback to sharp polygon
				GenerateSharpPolygonPath( detailedPoints, sides, circumradius, angleOffset, transformation );
				return;
			}

			// calculate tangent point distance and center distance (same for all vertices)
			double tangentDistance = cornerRadius / tanHalfAngle;
			double centerDistance = cornerRadius / sinHalfAngle;

			// generate vertices in clockwise order
			List<gp_Pnt> vertices = new List<gp_Pnt>();

			for( int i = 0; i < sides; i++ ) {
				// clockwise: reverse the angle direction
				double angle = angleOffset - i * angleStep;
				gp_Pnt vertex = new gp_Pnt(
					circumradius * Math.Cos( angle ),
					circumradius * Math.Sin( angle ),
					0
				);
				vertices.Add( vertex );
			}

			// check if edge length is sufficient for the corner radius
			double edgeLength = vertices[ 0 ].Distance( vertices[ 1 ] );
			if( tangentDistance * 2 > edgeLength ) {
				// fallback to sharp polygon if corner radius is too large
				GenerateSharpPolygonPath( detailedPoints, sides, circumradius, angleOffset, transformation );
				return;
			}

			// calculate start and end points of the edges after being shortened by the fillet
			List<gp_Pnt> edgeStarts = new List<gp_Pnt>();
			List<gp_Pnt> edgeEnds = new List<gp_Pnt>();

			for( int i = 0; i < sides; i++ ) {
				gp_Pnt currentVertex = vertices[ i ];
				gp_Pnt nextVertex = vertices[ ( i + 1 ) % sides ];

				// current edge vector
				gp_Vec edgeVec = new gp_Vec( currentVertex, nextVertex );
				double currentEdgeLength = edgeVec.Magnitude();

				if( currentEdgeLength <= 0.001 ) {
					// fallback to sharp polygon
					GenerateSharpPolygonPath( detailedPoints, sides, circumradius, angleOffset, transformation );
					return;
				}

				edgeVec.Normalize();

				// edge start point: shortened by the fillet of the current vertex
				gp_Pnt edgeStart = new gp_Pnt(
					currentVertex.X() + edgeVec.X() * tangentDistance,
					currentVertex.Y() + edgeVec.Y() * tangentDistance,
					currentVertex.Z()
				);
				edgeStarts.Add( edgeStart );

				// edge end point: shortened by the fillet of the next vertex
				gp_Pnt edgeEnd = new gp_Pnt(
					nextVertex.X() - edgeVec.X() * tangentDistance,
					nextVertex.Y() - edgeVec.Y() * tangentDistance,
					nextVertex.Z()
				);
				edgeEnds.Add( edgeEnd );

				// check if the shortened edge still has a valid length
				if( edgeStart.Distance( edgeEnd ) <= 0.001 ) {
					// fallback to sharp polygon
					GenerateSharpPolygonPath( detailedPoints, sides, circumradius, angleOffset, transformation );
					return;
				}
			}

			// generate path segments alternating between straight lines and arcs
			for( int i = 0; i < sides; i++ ) {
				// 1. Add shortened straight line segment
				AddStraightSegmentPoints( detailedPoints, edgeStarts[ i ], edgeEnds[ i ], toolVec, transformation );

				// 2. Add inscribed corner arc at the next vertex
				int nextVertexIndex = ( i + 1 ) % sides;
				AddInscribedCornerArc( detailedPoints,
					edgeEnds[ i ],
					edgeStarts[ nextVertexIndex ],
					vertices[ nextVertexIndex ],
					cornerRadius,
					centerDistance,
					toolVec,
					transformation );
			}
		}

		void AddInscribedCornerArc( List<CAMPoint> detailedPoints, gp_Pnt tangentPoint1, gp_Pnt tangentPoint2,
			gp_Pnt vertex, double radius, double centerDistance, gp_Dir toolVec, gp_Trsf transformation )
		{
			// calculate vectors from vertex to the two tangent points
			gp_Vec vec1 = new gp_Vec( vertex, tangentPoint1 );
			gp_Vec vec2 = new gp_Vec( vertex, tangentPoint2 );

			if( vec1.Magnitude() < 0.001 || vec2.Magnitude() < 0.001 ) {
				return;
			}

			vec1.Normalize();
			vec2.Normalize();

			// calculate bisector direction (pointing inside the polygon)
			gp_Vec bisector = vec1.Added( vec2 );
			if( bisector.Magnitude() < 0.001 ) {
				return;
			}
			bisector.Normalize();

			// inscribed circle center: move inward along the bisector
			gp_Pnt arcCenter = new gp_Pnt(
				vertex.X() + bisector.X() * centerDistance,
				vertex.Y() + bisector.Y() * centerDistance,
				vertex.Z()
			);

			// verify if the distances from the center to the two tangent points are equal to the radius
			double dist1 = arcCenter.Distance( tangentPoint1 );
			double dist2 = arcCenter.Distance( tangentPoint2 );

			if( Math.Abs( dist1 - radius ) > 1e-3 || Math.Abs( dist2 - radius ) > 1e-3 ) {
				return;
			}

			// calculate start and end angles for the arc
			double startAngle = CalculateAngle( arcCenter, tangentPoint1 );
			double endAngle = CalculateAngle( arcCenter, tangentPoint2 );

			// ensure clockwise direction for the arc
			double angleDiff = endAngle - startAngle;

			// normalize angle difference for clockwise motion
			if( angleDiff > Math.PI ) {
				angleDiff -= 2 * Math.PI;
			}
			else if( angleDiff < -Math.PI ) {
				angleDiff += 2 * Math.PI;
			}

			// for inscribed arcs at polygon corners, we want the shorter arc (interior)
			// if angle difference is positive (counter-clockwise), make it negative (clockwise)
			if( angleDiff > 0 ) {
				angleDiff -= 2 * Math.PI;
			}

			double arcLength = Math.Abs( angleDiff ) * radius;
			int segments = Math.Max( 1, (int)Math.Ceiling( arcLength / 1 ) );

			for( int j = 0; j < segments; j++ ) {
				double t = (double)j / segments;
				double angle = startAngle + t * angleDiff;

				gp_Pnt arcPoint = new gp_Pnt(
					arcCenter.X() + radius * Math.Cos( angle ),
					arcCenter.Y() + radius * Math.Sin( angle ),
					0
				);

				// tangent direction along arc (clockwise)
				gp_Dir arcTangent = new gp_Dir( -Math.Sin( angle ), Math.Cos( angle ), 0 );
				// normal direction (radially inward toward arc center)
				gp_Dir arcNormal = new gp_Dir(
					( arcCenter.X() - arcPoint.X() ) / radius,
					( arcCenter.Y() - arcPoint.Y() ) / radius,
					0
				);

				CADPoint cadPoint = new CADPoint(
					arcPoint.Transformed( transformation ),
					toolVec.Transformed( transformation ),
					arcNormal.Transformed( transformation ),
					arcTangent.Transformed( transformation )
				);

				detailedPoints.Add( new CAMPoint( cadPoint, toolVec.Transformed( transformation ) ) );
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

		double CalculateAngle( gp_Pnt center, gp_Pnt point )
		{
			return Math.Atan2( point.Y() - center.Y(), point.X() - center.X() );
		}

		double GetMaxCornerRadius( int sides, double sideLength )
		{
			// using the same logic as PatternFactory - based on interior angle
			double interiorAngle = ( sides - 2 ) * Math.PI / sides;
			double halfAngle = interiorAngle / 2.0;
			double tanHalfAngle = Math.Tan( halfAngle );

			if( tanHalfAngle < 0.001 ) {
				return 0;
			}

			// maximum radius is when tangent distance equals half edge length
			double maxTangentDistance = sideLength / 2.0;
			double maxRadius = maxTangentDistance * tanHalfAngle;

			return maxRadius * 0.9; // use 90% to be safe
		}

		double GetAngleOffset( int sides )
		{
			switch( sides ) {
				case 3:
					return Math.PI / 2.0;
				case 4:
					return Math.PI / 4.0;
				case 5:
					return Math.PI / 2.0;
				case 6:
					return 0.0;
				default:
					return 0.0;
			}
		}

		double GetPolygonPerimeter()
		{
			int sides = m_PolygonGeomData.Sides;
			double sideLength = m_PolygonGeomData.SideLength;
			double cornerRadius = m_PolygonGeomData.CornerRadius;

			// limit corner radius
			double maxCornerRadius = GetMaxCornerRadius( sides, sideLength );
			if( cornerRadius > maxCornerRadius ) {
				cornerRadius = maxCornerRadius;
			}

			if( cornerRadius <= 0.001 ) {
				// sharp corners
				return sides * sideLength;
			}
			else {
				// rounded corners: use the same calculation as PatternFactory
				double interiorAngle = ( sides - 2 ) * Math.PI / sides;
				double halfAngle = interiorAngle / 2.0;
				double tanHalfAngle = Math.Tan( halfAngle );

				if( tanHalfAngle < 0.001 ) {
					return sides * sideLength; // fallback
				}

				// calculate tangent distance for each corner
				double tangentDistance = cornerRadius / tanHalfAngle;

				// reduced edge length after corner radius
				double reducedSideLength = sideLength - 2.0 * tangentDistance;
				double straightLength = sides * reducedSideLength;

				// corner arc length: each corner arc spans (π - interiorAngle)
				double cornerArcAngle = Math.PI - interiorAngle;
				double arcLength = sides * cornerArcAngle * cornerRadius;

				return Math.Max( 0, straightLength + arcLength );
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

		PolygonGeomData m_PolygonGeomData;
		gp_Ax3 m_CoordinateInfo;
		List<CAMPoint> m_StartPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();
		CraftData m_CraftData;

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
	}

	internal static class PolygonCacheInfoExtensions
	{
		internal static List<CAMPoint> GetStartPointList( gp_Ax3 coordinateInfo, int sides, double sideLength )
		{
			gp_Pnt centerPoint = coordinateInfo.Location();

			// calculate circumradius R = sideLength / (2 * sin(π/n))
			double angleStep = 2.0 * Math.PI / sides;
			double radius = sideLength / ( 2.0 * Math.Sin( Math.PI / sides ) );

			// determine initial angle offset based on number of sides
			double angleOffset = GetAngleOffset( sides );
			gp_Dir local_Z_pos = gp.DZ();

			// create coordinate system transformation
			gp_Ax3 targetCoordSystem = new gp_Ax3( centerPoint, coordinateInfo.Direction(), coordinateInfo.XDirection() );
			gp_Trsf coordTransformation = new gp_Trsf();
			coordTransformation.SetTransformation( targetCoordSystem, new gp_Ax3() );

			// find the edge that intersects negative Y-axis and determine start index
			int startEdgeIndex = FindNegativeYAxisIntersectingEdge( sides, angleOffset, angleStep );

			List<CAMPoint> resultList = new List<CAMPoint>();

			// generate edge midpoints in clockwise order starting from the edge intersecting negative Y-axis
			for( int i = 0; i < sides; i++ ) {
				int currentEdgeIndex = ( startEdgeIndex + i ) % sides;

				// calculate angles for two vertices of current edge
				double vertex1Angle = angleOffset + currentEdgeIndex * angleStep;
				double vertex2Angle = angleOffset + ( ( currentEdgeIndex + 1 ) % sides ) * angleStep;

				// calculate vertex positions on circumcircle
				gp_Pnt vertex1 = new gp_Pnt(
					radius * Math.Cos( vertex1Angle ),
					radius * Math.Sin( vertex1Angle ),
					0
				);

				gp_Pnt vertex2 = new gp_Pnt(
					radius * Math.Cos( vertex2Angle ),
					radius * Math.Sin( vertex2Angle ),
					0
				);

				// calculate edge midpoint
				gp_Pnt edgeMidpoint = new gp_Pnt(
					( vertex1.X() + vertex2.X() ) / 2.0,
					( vertex1.Y() + vertex2.Y() ) / 2.0,
					0
				);

				// calculate edge direction vector for clockwise tangent
				gp_Vec edgeVector = new gp_Vec( vertex1, vertex2 ).Reversed();
				edgeVector.Normalize();
				gp_Dir tangent = new gp_Dir( edgeVector );

				// normal vector pointing toward center
				gp_Vec normalVector = new gp_Vec( edgeMidpoint, new gp_Pnt( 0, 0, 0 ) );
				normalVector.Normalize();
				gp_Dir normal = new gp_Dir( normalVector );

				// transform to final coordinate system
				CADPoint cadPoint = new CADPoint(
					edgeMidpoint.Transformed( coordTransformation ),
					local_Z_pos.Transformed( coordTransformation ),
					normal.Transformed( coordTransformation ),
					tangent.Transformed( coordTransformation )
				);

				CAMPoint camPoint = new CAMPoint( cadPoint, local_Z_pos.Transformed( coordTransformation ) );
				resultList.Add( camPoint );
			}

			return resultList;
		}

		static int FindNegativeYAxisIntersectingEdge( int sides, double angleOffset, double angleStep )
		{
			for( int i = 0; i < sides; i++ ) {
				double vertex1Angle = angleOffset + i * angleStep;
				double vertex2Angle = angleOffset + ( ( i + 1 ) % sides ) * angleStep;

				// normalize angles to [0, 2π] range
				vertex1Angle = NormalizeAngle( vertex1Angle );
				vertex2Angle = NormalizeAngle( vertex2Angle );

				// check if this edge crosses 270° (3π/2, which is negative Y-axis)
				double targetAngle = 3.0 * Math.PI / 2.0; // 270°

				if( IsAngleBetween( targetAngle, vertex1Angle, vertex2Angle ) ) {
					return i;
				}
			}

			// if not found, return 0 as default
			return 0;
		}

		static double NormalizeAngle( double angle )
		{
			while( angle < 0 )
				angle += 2 * Math.PI;
			while( angle >= 2 * Math.PI )
				angle -= 2 * Math.PI;
			return angle;
		}

		static bool IsAngleBetween( double targetAngle, double startAngle, double endAngle )
		{
			// normalize all angles
			targetAngle = NormalizeAngle( targetAngle );
			startAngle = NormalizeAngle( startAngle );
			endAngle = NormalizeAngle( endAngle );

			if( startAngle <= endAngle ) {

				// no crossing of 0°
				return targetAngle >= startAngle && targetAngle <= endAngle;
			}
			else {

				// crossing 0°
				return targetAngle >= startAngle || targetAngle <= endAngle;
			}
		}

		static double GetAngleOffset( int sides )
		{
			switch( sides ) {
				case 3:
					return Math.PI / 2.0;
				case 4:
					return Math.PI / 4.0;
				case 5:
					return Math.PI / 2.0;
				case 6:
					return 0.0;
				default:
					return 0.0;
			}
		}
	}
}