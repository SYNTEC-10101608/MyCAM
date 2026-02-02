using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	public abstract class StdPatternCacheBase : IStdPatternCache
	{
		protected StdPatternCacheBase( CraftData craftData )
		{
			if( craftData == null ) {
				throw new ArgumentNullException( "StdPatternCacheBase constructing argument null" );
			}

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
				return m_CAMPointList;
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
				return m_MainPathCADPointList;
			}
		}

		public List<CADPoint> StartCADPointList
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

		public List<CAMPoint> StartPointList
		{
			get
			{
				return m_StartCAMPointList;
			}
		}

		#endregion

		#region Common Methods

		public void DoTransform( gp_Trsf transform )
		{
			foreach( CADPoint cadPoint in m_StartCADPointList ) {
				cadPoint.Transform( transform );
			}
			foreach( CADPoint cadPoint in m_MainPathCADPointList ) {
				cadPoint.Transform( transform );
			}
			m_RefCoord.Transform( transform );
			BuildCAMPointList();
		}

		#endregion

		#region Protected Members

		protected abstract void BuildCADPointList();

		protected abstract void BuildCAMPointList();

		protected abstract List<CADPoint> Discretize();

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

		#region Start Point Management


		protected void SetStartPointList()
		{
			if( m_CraftData.StartPointIndex < 0 || m_CraftData.StartPointIndex >= m_StartCADPointList.Count ) {
				return;
			}

			// rearrange start point list and build CAMPoint list
			m_StartCAMPointList = ResortCAMPointList( m_StartCADPointList, m_CraftData.StartPointIndex );
		}

		protected void SetMainPathCAMPoint()
		{
			if( m_CraftData.StartPointIndex < 0 || m_CraftData.StartPointIndex >= m_StartCADPointList.Count ) {
				return;
			}

			// find the corresponding index in MainPathCADPointList
			int mainPathIndex = GetMainPathStartPointIndex( m_CraftData.StartPointIndex );
			if( mainPathIndex < 0 ) {
				return;
			}

			// rearrange main path and build CAMPoint list
			m_MainPathCADPointList = ResortList( m_MainPathCADPointList, mainPathIndex, m_CraftData.IsPathReverse );
			m_CAMPointList = ConvertToCAMPointList( m_MainPathCADPointList );

			// add closing point to main path (for closed loop)
			if( m_CAMPointList.Count > 0 ) {
				CAMPoint closingPoint = m_CAMPointList[ 0 ].Clone();
				m_CAMPointList.Add( closingPoint );
			}
		}

		#endregion

		#region Helper Methods

		List<CAMPoint> ResortCAMPointList( List<CADPoint> cadPointList, int startIndex )
		{
			if( cadPointList == null || cadPointList.Count == 0 ) {
				return new List<CAMPoint>();
			}

			// calculate effective start index
			int effectiveStartIndex = startIndex % cadPointList.Count;
			if( effectiveStartIndex < 0 ) {
				effectiveStartIndex += cadPointList.Count;
			}

			// build and rearrange in one pass
			List<CAMPoint> camPointList = new List<CAMPoint>();

			for( int i = 0; i < cadPointList.Count; i++ ) {
				int sourceIndex = ( i + effectiveStartIndex ) % cadPointList.Count;
				camPointList.Add( new CAMPoint( cadPointList[ sourceIndex ] ) );
			}
			return camPointList;
		}

		List<CAMPoint> ConvertToCAMPointList( List<CADPoint> cadPointList )
		{
			List<CAMPoint> camPointList = new List<CAMPoint>( cadPointList.Count );
			foreach( CADPoint cadPoint in cadPointList ) {
				camPointList.Add( new CAMPoint( cadPoint ) );
			}
			return camPointList;
		}

		List<T> ResortList<T>( List<T> sourceList, int startIndex, bool isReverse )
		{
			if( sourceList == null || sourceList.Count == 0 ) {
				return new List<T>();
			}

			if( startIndex < 0 || startIndex >= sourceList.Count ) {
				return new List<T>( sourceList );
			}

			List<T> rearrangedList = new List<T>( sourceList.Count );
			int count = sourceList.Count;

			if( isReverse ) {
				// reverse direction: go backwards from start index
				for( int i = 0; i < count; i++ ) {
					int index = ( startIndex - i + count ) % count;
					rearrangedList.Add( sourceList[ index ] );
				}
			}
			else {
				// forward direction: go forwards from start index
				for( int i = 0; i < count; i++ ) {
					int index = ( startIndex + i ) % count;
					rearrangedList.Add( sourceList[ index ] );
				}
			}

			return rearrangedList;
		}

		int GetMainPathStartPointIndex( int startPointIndexInStartList )
		{
			if( startPointIndexInStartList < 0 || startPointIndexInStartList >= m_StartCADPointList.Count ) {
				return -1;
			}

			CADPoint startPoint = m_StartCADPointList[ startPointIndexInStartList ];
			const double TOLERANCE = 0.001;

			// find matching point in MainPathCADPointList
			for( int i = 0; i < m_MainPathCADPointList.Count; i++ ) {
				double distSq = startPoint.Point.SquareDistance( m_MainPathCADPointList[ i ].Point );
				if( distSq < TOLERANCE ) {
					return i;
				}
			}
			return -1;
		}

		#endregion

		#region Protected Fields

		protected gp_Ax3 m_RefCoord;
		protected List<CADPoint> m_MainPathCADPointList = new List<CADPoint>();
		protected List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
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
