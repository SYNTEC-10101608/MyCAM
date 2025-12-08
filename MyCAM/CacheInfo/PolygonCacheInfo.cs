using MyCAM.Data;
using MyCAM.Data.GeomDataFolder;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.CacheInfo
{
	public class PolygonCacheInfo : IStartPointCache, ILeadCache, IPathReverseCache, IPathHeadTailCache, IOverCutCache
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

		public CAMPoint GetMainPathStartCAMPoint()
		{
			return m_StartPointList[ m_CraftData.StartPointIndex ];
		}

		public List<CAMPoint> GetToolVecList()
		{
			return m_StartPointList;
		}

		public bool IsClosed
		{
			get
			{
				return true;
			}
		}

		public bool IsPathReverse
		{
			get
			{
				return m_CraftData.IsReverse;
			}
		}

		public LeadData LeadData
		{
			get
			{
				return m_CraftData.LeadLineParam;
			}
		}

		public double OverCutLength
		{
			get
			{
				return m_CraftData.OverCutLength;
			}
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