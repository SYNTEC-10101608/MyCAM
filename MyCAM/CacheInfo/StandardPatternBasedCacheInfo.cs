using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.CacheInfo
{
	/// <summary>
	/// Base class for all standard pattern cache info (Circle, Rectangle, Runway, Polygon)
	/// Provides common functionality and abstracts away repetitive code
	/// </summary>
	public abstract class StandardPatternBasedCacheInfo : IStandardPatternCacheInfo, IProcessPathStartEndCache, IMainPathStartPointCache, ILeadCache, IPathReverseCache, IOverCutCache, IToolVecCache
	{
		/// <summary>
		/// Constructor for standard pattern cache info
		/// </summary>
		protected StandardPatternBasedCacheInfo( gp_Ax3 coordinateInfo, CraftData craftData )
		{
			if( coordinateInfo == null || craftData == null ) {
				throw new ArgumentNullException( "StandardPatternBasedCacheInfo constructing argument null" );
			}

			m_CoordinateInfo = coordinateInfo;
			m_CraftData = craftData;
			m_CraftData.ParameterChanged += SetCraftDataDirty;
			BuildCAMPointList();
		}

		#region Abstract Members - Must be implemented by derived classes

		/// <summary>
		/// Gets the PathType for this cache info
		/// </summary>
		public abstract PathType PathType { get; }

		/// <summary>
		/// Builds the CAM point list - implementation varies by pattern type
		/// </summary>
		protected abstract void BuildCAMPointList();

		#endregion

		#region Common Properties

		/// <summary>
		/// Gets the start point list (lazy evaluation with dirty checking)
		/// </summary>
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

		/// <summary>
		/// Gets the lead-in CAM point list (lazy evaluation with dirty checking)
		/// </summary>
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

		/// <summary>
		/// Gets the lead-out CAM point list
		/// </summary>
		public List<CAMPoint> LeadOutCAMPointList { get; }

		/// <summary>
		/// Gets the overcut CAM point list (lazy evaluation with dirty checking)
		/// </summary>
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

		/// <summary>
		/// Gets whether the path is reversed
		/// </summary>
		public bool IsPathReverse
		{
			get
			{
				return m_CraftData.IsReverse;
			}
		}

		/// <summary>
		/// Gets the lead data
		/// </summary>
		public LeadData LeadData
		{
			get
			{
				return m_CraftData.LeadLineParam;
			}
		}

		/// <summary>
		/// Gets the overcut length
		/// </summary>
		public double OverCutLength
		{
			get
			{
				return m_CraftData.OverCutLength;
			}
		}

		#endregion

		#region Common Methods

		/// <summary>
		/// Gets the process reference point
		/// </summary>
		public CAMPoint GetProcessRefPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			return new CAMPoint( 
				new CADPoint( 
					m_CoordinateInfo.Location(), 
					m_CoordinateInfo.Direction(), 
					m_CoordinateInfo.XDirection(), 
					m_CoordinateInfo.YDirection() ), 
				m_CoordinateInfo.Direction() );
		}

		/// <summary>
		/// Gets the process start point
		/// </summary>
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

		/// <summary>
		/// Gets the process end point
		/// </summary>
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

		/// <summary>
		/// Gets the main path start CAM point
		/// </summary>
		public IProcessPoint GetMainPathStartCAMPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			return m_StartPointList[ m_CraftData.StartPointIndex ].Clone();
		}

		/// <summary>
		/// Gets the tool vector list
		/// </summary>
		public IReadOnlyList<IProcessPoint> GetToolVecList()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			return m_StartPointList.Cast<IProcessPoint>().ToList();
		}

		/// <summary>
		/// Checks if a point is a tool vector modify point
		/// Standard patterns typically don't support this
		/// </summary>
		public virtual bool IsToolVecModifyPoint( ISetToolVecPoint point )
		{
			return false;
		}

		/// <summary>
		/// Applies transformation to the cache info
		/// </summary>
		public void DoTransform( gp_Trsf transform )
		{
			m_CoordinateInfo.Transform( transform );
			BuildCAMPointList();
		}

		#endregion

		#region Protected Members

		/// <summary>
		/// Gets the coordinate information
		/// </summary>
		protected gp_Ax3 CoordinateInfo
		{
			get { return m_CoordinateInfo; }
		}

		/// <summary>
		/// Gets the craft data
		/// </summary>
		protected CraftData CraftData
		{
			get { return m_CraftData; }
		}

		/// <summary>
		/// Sets the craft data dirty flag
		/// </summary>
		protected void SetCraftDataDirty()
		{
			if( !m_IsCraftDataDirty ) {
				m_IsCraftDataDirty = true;
			}
		}

		/// <summary>
		/// Clears the craft data dirty flag
		/// Called by derived classes after rebuilding
		/// </summary>
		protected void ClearCraftDataDirty()
		{
			m_IsCraftDataDirty = false;
		}

		#endregion

		#region Protected Fields

		/// <summary>
		/// Coordinate system information
		/// </summary>
		protected gp_Ax3 m_CoordinateInfo;

		/// <summary>
		/// Start point list
		/// </summary>
		protected List<CAMPoint> m_StartPointList = new List<CAMPoint>();

		/// <summary>
		/// Lead-in CAM point list
		/// </summary>
		protected List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();

		/// <summary>
		/// Overcut point list
		/// </summary>
		protected List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();

		/// <summary>
		/// Craft data (sibling pointer)
		/// </summary>
		protected CraftData m_CraftData;

		/// <summary>
		/// Flag to indicate craft data changed
		/// </summary>
		protected bool m_IsCraftDataDirty = false;

		#endregion
	}
}
