using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class RunwayCache : StdPatternCacheBase
	{
		public RunwayCache( gp_Ax3 refCoord, IStdPatternGeomData geomData, CraftData craftData )
			: base( refCoord, craftData )
		{
			if( geomData == null || !( geomData is RunwayGeomData runwayGeomData ) ) {
				throw new ArgumentNullException( "RunwayCache constructing argument error - invalid geomData" );
			}
			m_RunwayGeomData = runwayGeomData;
			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( refCoord, runwayGeomData );
			BuildCAMPointList();
		}

		protected override void BuildCAMPointList()
		{
			ClearCraftDataDirty();
			m_RefPoint = RunwayRefPoint();

			// build initial CAM point list
			m_StartCAMPointList = new List<CAMPoint>();
			for( int i = 0; i < m_StartCADPointList.Count; i++ ) {
				CADPoint cadPoint = m_StartCADPointList[ i ];
				CAMPoint camPoint = new CAMPoint( cadPoint );
				m_StartCAMPointList.Add( camPoint );
			}
			SetStartPoint();
			m_MaxOverCutLength = OverCutHelper.GetMaxOverCutLength( m_RunwayGeomData, m_CraftData.StartPointIndex );

			// set over cut
			List<IOrientationPoint> camPointOverCutList = m_StartCAMPointList.Cast<IOrientationPoint>().ToList();
			OverCutHelper.SetStdPatternOverCut( m_RefCoord, m_RunwayGeomData, camPointOverCutList, m_CraftData.OverCutLength, out List<IOrientationPoint> overCutPointList );
			m_OverCutCAMPointList = overCutPointList.Cast<CAMPoint>().ToList();

			// set lead
			List<IOrientationPoint> mainPointList = m_StartCAMPointList.Cast<IOrientationPoint>().ToList();
			LeadHelper.SetLeadIn( mainPointList, out List<IOrientationPoint> leadInPointList, m_CraftData.LeadData, m_CraftData.IsPathReverse );
			m_LeadInCAMPointList = leadInPointList.Cast<CAMPoint>().ToList();
		}

		CAMPoint RunwayRefPoint()
		{
			// calculate runway parameters
			double length = m_RunwayGeomData.Length;
			double width = m_RunwayGeomData.Width;
			double radius = width / 2.0;
			double straightLength = length - width;

			// left arc center position in local coordinate system
			gp_Pnt leftArcCenter;

			if( straightLength <= 0.001 ) {

				// pure circle case: center is at origin
				leftArcCenter = new gp_Pnt( 0, 0, 0 );
			}
			else {
				// runway shape: left arc center is at (-straightLength/2, 0, 0)
				double halfStraight = straightLength / 2.0;
				leftArcCenter = new gp_Pnt( -halfStraight, 0, 0 );
			}

			// transform local coordinates to world coordinate system
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				m_RefCoord.Location(),
				m_RefCoord.Direction(),
				m_RefCoord.XDirection()
			);
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );
			gp_Pnt worldLeftArcCenter = leftArcCenter.Transformed( transformation );

			return new CAMPoint(
				new CADPoint(
					worldLeftArcCenter,
					m_RefCoord.Direction(),
					m_RefCoord.XDirection(),
					m_RefCoord.YDirection()
				),
				m_RefCoord.Direction()
			);
		}

		RunwayGeomData m_RunwayGeomData;
	}
}