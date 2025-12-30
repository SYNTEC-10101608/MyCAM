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

		public List<CADPoint> MainPathCADPointList
		{
			get
			{
				return m_StartCADPointList;
			}
		}

		public double MaxOverCutLength
		{
			get
			{
				return m_MaxOverCutLength;
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

		protected void SetStartPoint()
		{
			// rearrange cam points to start from the start index
			if( m_CraftData.StartPointIndex != 0 ) {
				List<CAMPoint> newStartPointList = new List<CAMPoint>();
				for( int i = 0; i < m_StartCAMPointList.Count; i++ ) {
					newStartPointList.Add( m_StartCAMPointList[ ( i + m_CraftData.StartPointIndex ) % m_StartCAMPointList.Count ] );
				}
				m_StartCAMPointList = newStartPointList;
			}
			m_StartCAMPointList.Add( m_StartCAMPointList[ 0 ].Clone() ); // close the polygon
		}

		#region Protected Fields

		protected gp_Ax3 m_RefCoord;
		protected List<CADPoint> m_StartCADPointList = new List<CADPoint>();
		protected List<CAMPoint> m_StartCAMPointList = new List<CAMPoint>();
		protected List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		protected List<CAMPoint> m_OverCutCAMPointList = new List<CAMPoint>();
		protected CraftData m_CraftData;
		protected CAMPoint m_RefPoint;
		protected double m_MaxOverCutLength;
		protected bool m_IsCraftDataDirty = false;

		#endregion
	}
}
