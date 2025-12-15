using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	public class CircleCache : StdPatternCacheBase
	{
		public CircleCache( gp_Ax3 refCoord, IStdPatternGeomData geomData, CraftData craftData )
			: base( refCoord, craftData )
		{
			if( geomData == null || !( geomData is CircleGeomData circleGeomData ) ) {
				throw new ArgumentNullException( "CircleCache constructing argument error - invalid geomData" );
			}
			m_CircleGeomData = circleGeomData;
			BuildCAMPointList();
		}

		protected override void BuildCAMPointList()
		{
			ClearCraftDataDirty();
			m_RefPoint = new CAMPoint( new CADPoint( m_RefCoord.Location(), m_RefCoord.Direction(), m_RefCoord.XDirection(), m_RefCoord.YDirection() ), m_RefCoord.Direction() );
			m_StartPointList = CircleCacheExtensions.GetStartPointList( m_RefCoord, m_CircleGeomData.Diameter );

			// close the loop
			m_StartPointList.Add( m_StartPointList[ 0 ].Clone() );
		}

		CircleGeomData m_CircleGeomData;
	}

	internal static class CircleCacheExtensions
	{
		internal static List<CAMPoint> GetStartPointList( gp_Ax3 refCoord, double diameter )
		{
			gp_Pnt centerPoint = refCoord.Location();
			double radius = diameter / 2;

			// directly calculate intersection point along ref coord X-axis direction
			// intersection point = center + X-axis direction * radius
			gp_Dir xDirection = refCoord.XDirection();
			gp_Vec radiusVector = new gp_Vec( xDirection.XYZ() * radius );
			gp_Pnt intersectionPoint = centerPoint.Translated( radiusVector );

			// define various vectors (in target coordinate system)
			gp_Dir normal = refCoord.Direction();
			gp_Dir radialIn = xDirection.Reversed();
			gp_Dir tangent = refCoord.YDirection().Reversed();
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
