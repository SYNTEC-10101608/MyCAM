using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	public class CircleCache : StdPatternCacheBase
	{
		public CircleCache( gp_Ax3 coordinateInfo, IStdPatternGeomData geomData, CraftData craftData )
			: base( coordinateInfo, craftData )
		{
			if( geomData == null || !( geomData is CircleGeomData circleGeomData ) ) {
				throw new ArgumentNullException( "CircleCache constructing argument error - invalid geomData" );
			}
			m_CircleGeomData = circleGeomData;
			BuildCAMPointList();
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Circle;
			}
		}

		protected override void BuildCAMPointList()
		{
			ClearCraftDataDirty();
			m_RefPoint = new CAMPoint( new CADPoint( m_CoordinateInfo.Location(), m_CoordinateInfo.Direction(), m_CoordinateInfo.XDirection(), m_CoordinateInfo.YDirection() ), m_CoordinateInfo.Direction() );
			m_StartPointList = CircleCacheExtensions.GetStartPointList( CoordinateInfo, m_CircleGeomData.Diameter );
		}

		CircleGeomData m_CircleGeomData;
	}

	internal static class CircleCacheExtensions
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
