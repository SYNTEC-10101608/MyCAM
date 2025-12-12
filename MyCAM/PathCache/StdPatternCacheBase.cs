using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public abstract class StdPatternCacheBase : IStdPatternCache, IProcessPathStartEndCache, IMainPathStartPointCache, ILeadCache, IPathReverseCache, IOverCutCache, IToolVecCache, IStdPatternRefPointCache
	{
		protected StdPatternCacheBase( gp_Ax3 coordinateInfo, CraftData craftData )
		{
			if( coordinateInfo == null || craftData == null ) {
				throw new ArgumentNullException( "StandardPatternBasedCacheInfo constructing argument null" );
			}

			m_CoordinateInfo = coordinateInfo;
			m_CraftData = craftData;
			m_CraftData.ParameterChanged += SetCraftDataDirty;
		}

		#region Abstract Members

		public abstract PathType PathType
		{
			get;
		}

		protected abstract void BuildCAMPointList();

		#endregion

		#region Common Properties

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

		#endregion

		#region Common Methods

		public virtual IProcessPoint GetProcessRefPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			return m_RefPoint;
		}

		public IProcessPoint GetProcessStartPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			if( LeadInCAMPointList.Count != 0 ) {
				return LeadInCAMPointList[ 0 ].Clone();
			}
			return m_StartPointList[ m_CraftData.StartPointIndex ].Clone();
		}

		public IProcessPoint GetProcessEndPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			if( OverCutCAMPointList.Count != 0 ) {
				return OverCutCAMPointList[ OverCutCAMPointList.Count - 1 ].Clone();
			}
			return m_StartPointList[ m_CraftData.StartPointIndex ].Clone();
		}

		public CAMPoint GetMainPathStartCAMPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			return m_StartPointList[ m_CraftData.StartPointIndex ].Clone();
		}

		public IReadOnlyList<IProcessPoint> GetToolVecList()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			return m_StartPointList.Cast<IProcessPoint>().ToList();
		}

		public virtual bool IsToolVecModifyPoint( ISetToolVecPoint point )
		{
			return false;
		}

		public void DoTransform( gp_Trsf transform )
		{
			m_CoordinateInfo.Transform( transform );
			BuildCAMPointList();
		}

		#endregion

		#region Protected Members

		protected gp_Ax3 CoordinateInfo
		{
			get
			{
				return m_CoordinateInfo;
			}
		}

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

		protected gp_Ax3 m_CoordinateInfo;

		protected List<CAMPoint> m_StartPointList = new List<CAMPoint>();

		protected List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();

		protected List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();

		protected CraftData m_CraftData;

		protected CAMPoint m_RefPoint;

		protected bool m_IsCraftDataDirty = false;

		#endregion
	}
}
