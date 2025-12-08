using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.CacheInfo
{
	public class CircleCacheInfo : IPathHeadTailCache, IStartPointCache, ILeadCache, IPathReverseCache, IToolVecCache, IOverCutCache
	{
		public CircleCacheInfo( string szID, gp_Ax3 coordinateInfo, CircleGeomData circleGeomData, CraftData craftData )
		{
			// Q:Should this class know about DataManager? If so, we can validate the UID against existing paths.
			if( string.IsNullOrEmpty( szID ) || circleGeomData == null || craftData == null ) {
				throw new ArgumentNullException( "CircleCacheInfo constructing argument null" );
			}

			UID = szID;
			m_CoordinateInfo = coordinateInfo;
			m_CircleGeomData = circleGeomData;
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
				return PathType.Circle;
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

		public gp_Pnt GetMainPathStartPoint()
		{
			return m_StartPointList[ m_CraftData.StartPointIndex ].Point;
		}

		public CAMPoint GetFirstCAMPoint()
		{
			return m_StartPointList[ m_CraftData.StartPointIndex ];
		}

		public List<CAMPoint> GetToolVecList()
		{
			return m_StartPointList;
		}

		public bool IsToolVecModifyPoint( ISetToolVecPoint point )
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			//if( m_CAMPointIndexMap.ContainsKey( point as CAMPoint ) ) {
			//	int index = m_CAMPointIndexMap[ point as CAMPoint ];
			//	return m_CraftData.ToolVecModifyMap.ContainsKey( index );
			//}
			return false;
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
			m_StartPointList = CircleCacheInfoExtensions.GetStartPointList( m_CoordinateInfo, m_CircleGeomData.Diameter );
		}

		void SetCraftDataDirty()
		{
			if( !m_IsCraftDataDirty ) {
				m_IsCraftDataDirty = true;
			}
		}

		CircleGeomData m_CircleGeomData;
		gp_Ax3 m_CoordinateInfo;
		List<CAMPoint> m_StartPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();

		// they are sibling pointer, and change the declare order
		CraftData m_CraftData;

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
	}

	internal static class CircleCacheInfoExtensions
	{
		internal static List<CAMPoint> GetStartPointList( gp_Ax3 coordinateInfo, double diameter )
		{
			gp_Pnt centerPoint = coordinateInfo.Location();
			double radius = diameter / 2;

			// directly calculate intersection point along coordinateInfo X-axis direction
			// intersection point = center + X-axis direction * radius
			gp_Dir xDirection = coordinateInfo.XDirection();
			gp_Vec radiusVector = new gp_Vec( xDirection.XYZ() * radius );
			gp_Pnt intersectionPoint = centerPoint.Translated( radiusVector );

			// define various vectors (in target coordinate system)
			gp_Dir normal = coordinateInfo.Direction();
			gp_Dir radialIn = xDirection.Reversed();
			gp_Dir tangent = coordinateInfo.YDirection().Reversed();
			gp_Dir toolVec = normal;

			// create CADPoint and CAMPoint
			CADPoint cadPoint = new CADPoint( intersectionPoint, normal, radialIn, tangent );
			CAMPoint camPoint = new CAMPoint( cadPoint, toolVec );

			// return list containing single point
			List<CAMPoint> resultList = new List<CAMPoint>
			{
				camPoint
			};
			return resultList;
		}
	}
}
