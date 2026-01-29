using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class CircleCache : StdPatternCacheBase
	{
		public CircleCache( IStdPatternGeomData geomData, CraftData craftData )
			: base( craftData )
		{
			if( geomData == null || !( geomData is CircleGeomData circleGeomData ) ) {
				throw new ArgumentNullException( "CircleCache constructing argument error - invalid geomData" );
			}
			m_CircleGeomData = circleGeomData;
			BuildCADPointList();
			BuildCAMPointList();
		}

		protected override void BuildCADPointList()
		{
			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( m_CircleGeomData );
			m_MainPathCADPointList = Discretize();
		}

		protected override void BuildCAMPointList()
		{
			ClearCraftDataDirty();

			// set reference point
			m_RefPoint = new CAMPoint(
				new CADPoint(
					m_CircleGeomData.RefCoord.Location(),
					m_CircleGeomData.RefCoord.Direction(),
					m_CircleGeomData.RefCoord.XDirection(),
					m_CircleGeomData.RefCoord.YDirection()
				),
				m_CircleGeomData.RefCoord.Direction()
			);

			SetMainPathCAMPoint();
			SetStartPointList();

			// calculate max over cut length
			m_MaxOverCutLength = OverCutHelper.GetMaxOverCutLength( m_CircleGeomData, m_CraftData.StartPointIndex );

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
			if( m_CircleGeomData == null || m_CircleGeomData.RefCoord == null ) {
				return new List<CADPoint>();
			}

			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( m_CircleGeomData.RefCoord );
			List<CADPoint> discretizedPoints = StdPatternDiscreteFactory.DiscretizeCircle( m_CircleGeomData.Diameter, transformation );

			// ensure all start points are included in the discretized list
			if( m_StartCADPointList != null && m_StartCADPointList.Count > 0 ) {
				discretizedPoints = StartPointHelper.EnsureStartPointsIncluded( discretizedPoints, m_StartCADPointList, m_CircleGeomData );
			}

			return discretizedPoints;
		}

		CircleGeomData m_CircleGeomData;
	}
}
