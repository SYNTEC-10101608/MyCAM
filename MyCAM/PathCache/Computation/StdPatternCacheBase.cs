using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	public abstract class StdPatternCacheBase : IStdPatternCache
	{
		protected StdPatternCacheBase( gp_Ax3 refCoord, CraftData craftData )
		{
			if( refCoord == null || craftData == null ) {
				throw new ArgumentNullException( "StdPatternCacheBase constructing argument null" );
			}

			m_RefCoord = refCoord;
			m_CraftData = craftData;
			m_CraftData.ParameterChanged += SetCraftDataDirty;
		}

		#region Computation Result

		public List<CAMPoint> MainPathPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_StartCAMPointList;
			}
		}

		public List<CAMPoint> LeadInPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_LeadInCAMPointList;
			}
		}

		public List<CAMPoint> LeadOutPointList
		{
			get
			{
				return new List<CAMPoint>();
			}
		}

		public List<CAMPoint> OverCutPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_OverCutCAMPointList;
			}
		}

		public CAMPoint RefPoint
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_RefPoint;
			}
		}

		public List<CADPoint> MainPathPointListForOrder
		{
			get
			{
				return m_StartCADPointList;
			}
		}

		#endregion

		#region Common Methods

		public void DoTransform( gp_Trsf transform )
		{
			foreach( CADPoint cadPoint in m_StartCADPointList ) {
				cadPoint.Transform( transform );
			}
			m_RefCoord.Transform( transform );
			BuildCAMPointList();
		}

		#endregion

		#region Protected Members

		protected abstract void BuildCAMPointList();

		protected void SetCraftDataDirty()
		{
			if( !m_IsCraftDataDirty ) {
				m_IsCraftDataDirty = true;
			}
		}

		protected void ClearCraftDataDirty()
		{
			m_IsCraftDataDirty = false;
		}

		#endregion

		#region Protected Fields

		protected gp_Ax3 m_RefCoord;
		protected List<CADPoint> m_StartCADPointList = new List<CADPoint>();
		protected List<CAMPoint> m_StartCAMPointList = new List<CAMPoint>();
		protected List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		protected List<CAMPoint> m_OverCutCAMPointList = new List<CAMPoint>();
		protected CraftData m_CraftData;
		protected CAMPoint m_RefPoint;
		protected bool m_IsCraftDataDirty = false;

		#endregion
	}
}
