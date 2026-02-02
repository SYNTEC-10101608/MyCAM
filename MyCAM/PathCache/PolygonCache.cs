using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class PolygonCache : StdPatternCacheBase
	{
		public PolygonCache( IStdPatternGeomData geomData, CraftData craftData )
			: base( craftData )
		{
			if( geomData == null || !( geomData is PolygonGeomData polygonGeomData ) ) {
				throw new ArgumentNullException( "PolygonCache constructing argument error - invalid geomData" );
			}
			m_PolygonGeomData = polygonGeomData;
			BuildCADPointList();
			BuildCAMPointList();
		}

		protected override void BuildCADPointList()
		{
			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( m_PolygonGeomData );
			m_MainPathCADPointList = Discretize();
		}

		protected override void BuildCAMPointList()
		{
			ClearCraftDataDirty();

			// set reference point
			m_RefPoint = new CAMPoint(
				new CADPoint(
					m_PolygonGeomData.RefCoord.Location(),
					m_PolygonGeomData.RefCoord.Direction(),
					m_PolygonGeomData.RefCoord.XDirection(),
					m_PolygonGeomData.RefCoord.YDirection()
				),
				m_PolygonGeomData.RefCoord.Direction()
			);

			SetMainPathCAMPoint();
			SetStartPointList();

			// calculate max over cut length
			m_MaxOverCutLength = OverCutHelper.GetMaxOverCutLength( m_PolygonGeomData, m_CraftData.StartPointIndex );

			// set over cut using unified method
			List<IOrientationPoint> mainPathOriPointList = m_CAMPointList.Cast<IOrientationPoint>().ToList();
			OverCutHelper.SetOverCut( mainPathOriPointList, out List<IOrientationPoint> overCutPointList, m_CraftData.OverCutLength, isClosed: true );
			m_OverCutCAMPointList = overCutPointList.Cast<CAMPoint>().ToList();

			// set lead
			List<IOrientationPoint> mainPointList = m_StartCAMPointList.Cast<IOrientationPoint>().ToList();
			LeadHelper.SetLeadIn( mainPointList, out List<IOrientationPoint> leadInPointList, m_CraftData.LeadData, m_CraftData.IsPathReverse );
			m_LeadInCAMPointList = leadInPointList.Cast<CAMPoint>().ToList();
		}

		protected override List<CADPoint> Discretize()
		{
			if( m_PolygonGeomData == null || m_PolygonGeomData.RefCoord == null ) {
				return new List<CADPoint>();
			}

			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( m_PolygonGeomData.RefCoord );
			List<CADPoint> discretizedPoints = StdPatternDiscreteFactory.DiscretizePolygon( m_PolygonGeomData.Sides, m_PolygonGeomData.SideLength, m_PolygonGeomData.CornerRadius, transformation );

			// ensure all start points are included in the discretized list
			if( m_StartCADPointList != null && m_StartCADPointList.Count > 0 ) {
				discretizedPoints = StartPointHelper.EnsureStartPointsIncluded( discretizedPoints, m_StartCADPointList, m_PolygonGeomData );
			}

			return discretizedPoints;
		}

		PolygonGeomData m_PolygonGeomData;
	}
}