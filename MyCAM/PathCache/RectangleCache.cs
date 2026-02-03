using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class RectangleCache : StdPatternCacheBase
	{
		public RectangleCache( IStdPatternGeomData geomData, CraftData craftData )
			: base( craftData )
		{
			if( geomData == null || !( geomData is RectangleGeomData rectangleGeomData ) ) {
				throw new ArgumentNullException( "RectangleCache constructing argument error - invalid geomData" );
			}
			m_RectangleGeomData = rectangleGeomData;
			BuildCADPointList();
			BuildCAMPointList();
		}

		protected override void BuildCADPointList()
		{
			m_StartCADPointList = StdPatternStartPointListFactory.GetStartPointList( m_RectangleGeomData );
			m_MainPathCADPointList = Discretize();
		}

		protected override void BuildCAMPointList()
		{
			ClearCraftDataDirty();

			// set reference point
			m_RefPoint = new CAMPoint(
				new CADPoint(
					m_RectangleGeomData.RefCoord.Location(),
					m_RectangleGeomData.RefCoord.Direction(),
					m_RectangleGeomData.RefCoord.XDirection(),
					m_RectangleGeomData.RefCoord.YDirection()
				),
				m_RectangleGeomData.RefCoord.Direction()
			);

			SetMainPathCAMPoint();
			SetStartPointList();

			// calculate max over cut length
			m_MaxOverCutLength = OverCutHelper.GetMaxOverCutLength( m_RectangleGeomData, m_CraftData.StartPointIndex );

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
			if( m_RectangleGeomData == null || m_RectangleGeomData.RefCoord == null ) {
				return new List<CADPoint>();
			}

			gp_Trsf transformation = DiscreteUtility.CreateCoordTransformation( m_RectangleGeomData.RefCoord );
			List<CADPoint> discretizedPoints = StdPatternDiscreteFactory.DiscretizeRectangle( m_RectangleGeomData.Width, m_RectangleGeomData.Length, m_RectangleGeomData.CornerRadius, transformation );

			// ensure all start points are included in the discretized list
			if( m_StartCADPointList != null && m_StartCADPointList.Count > 0 ) {
				discretizedPoints = StartPointHelper.EnsureStartPointsIncluded( discretizedPoints, m_StartCADPointList, m_RectangleGeomData );
			}

			return discretizedPoints;
		}

		RectangleGeomData m_RectangleGeomData;
	}
}
