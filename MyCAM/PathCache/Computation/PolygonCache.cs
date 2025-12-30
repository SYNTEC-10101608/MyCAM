using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class PolygonCache : StdPatternCacheBase
	{
		public PolygonCache( gp_Ax3 refCoord, IStdPatternGeomData geomData, CraftData craftData )
			: base( refCoord, craftData )
		{
			if( geomData == null || !( geomData is PolygonGeomData polygonGeomData ) ) {
				throw new ArgumentNullException( "PolygonCache constructing argument error - invalid geomData" );
			}
			m_PolygonGeomData = polygonGeomData;
			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( refCoord, polygonGeomData );
			BuildCAMPointList();
		}

		protected override void BuildCAMPointList()
		{
			ClearCraftDataDirty();
			m_RefPoint = new CAMPoint( new CADPoint( m_RefCoord.Location(), m_RefCoord.Direction(), m_RefCoord.XDirection(), m_RefCoord.YDirection() ), m_RefCoord.Direction() );

			// build initial CAM point list
			m_StartCAMPointList = new List<CAMPoint>();
			for( int i = 0; i < m_StartCADPointList.Count; i++ ) {
				CADPoint cadPoint = m_StartCADPointList[ i ];
				CAMPoint camPoint = new CAMPoint( cadPoint );
				m_StartCAMPointList.Add( camPoint );
			}
			SetStartPoint();
			m_MaxOverCutLength = OverCutHelper.GetMaxOverCutLength( m_PolygonGeomData, m_CraftData.StartPointIndex );

			// set over cut
			List<IOrientationPoint> camPointOverCutList = m_StartCAMPointList.Cast<IOrientationPoint>().ToList();
			OverCutHelper.SetStdPatternOverCut( m_RefCoord, m_PolygonGeomData, camPointOverCutList, m_CraftData.OverCutLength, out List<IOrientationPoint> overCutPointList );
			m_OverCutCAMPointList = overCutPointList.Cast<CAMPoint>().ToList();

			// set lead
			List<IOrientationPoint> mainPointList = m_StartCAMPointList.Cast<IOrientationPoint>().ToList();
			LeadHelper.SetLeadIn( mainPointList, out List<IOrientationPoint> leadInPointList, m_CraftData.LeadData, m_CraftData.IsPathReverse );
			m_LeadInCAMPointList = leadInPointList.Cast<CAMPoint>().ToList();
		}

		PolygonGeomData m_PolygonGeomData;
	}
}