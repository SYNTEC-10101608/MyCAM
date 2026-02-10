using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.PathCache
{
	public abstract class StdPatternCacheBase : IStdPatternCache
	{
		protected StdPatternCacheBase( IStdPatternGeomData geomData, CraftData craftData )
		{
			if( craftData == null ) {
				throw new ArgumentNullException( "StdPatternCacheBase constructing argument null" );
			}
			m_CraftData = craftData;
			m_CraftData.CAMFactorChanged += SetCAMFactorDirty;
			m_CraftData.CADFactorChanged += SetCADFactorDirty;
			m_ComputeRefCenterDir = geomData.RefCenterDir.MakeCopy();
			geomData.CADFactorChanged += SetCADFactorDirty;
		}

		#region Computation Result

		public List<CAMPoint> MainPathPointList
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				else if( m_IsCAMFactorDirty ) {
					BuildCAMPointList();
				}
				return m_CAMPointList;
			}
		}

		public List<CAMPoint> LeadInPointList
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				else if( m_IsCAMFactorDirty ) {
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
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				else if( m_IsCAMFactorDirty ) {
					BuildCAMPointList();
				}
				return m_OverCutCAMPointList;
			}
		}

		public CAMPoint RefPoint
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				else if( m_IsCAMFactorDirty ) {
					BuildCAMPointList();
				}
				return m_RefPoint;
			}
		}

		public List<CADPoint> MainPathCADPointList
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				return m_MainPathCADPointList;
			}
		}

		public List<CADPoint> KeyCADPointList
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				return m_StartCADPointList;
			}
		}

		public double MaxOverCutLength
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				return m_MaxOverCutLength;
			}
		}

		public List<CAMPoint> KeyCAMPointList
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				return m_StartCAMPointList;
			}
		}

		public gp_Ax3 RefCoord
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				return m_RefCoord;
			}
		}

		public gp_Ax1 ComputeRefCenterDir
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				return m_ComputeRefCenterDir;
			}
		}

		public IStdPatternGeomData ComputeGeomData
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				return m_ComputeGeomData;
			}
		}

		#endregion

		#region Common Methods

		public void DoTransform( gp_Trsf transform )
		{
			BuildCADCAMPointList();
		}

		#endregion

		#region Protected Members

		protected abstract void BuildCADCAMPointList();

		protected abstract void BuildCAMPointList();

		protected void SetCAMFactorDirty()
		{
			if( !m_IsCAMFactorDirty ) {
				m_IsCAMFactorDirty = true;
			}
		}

		protected void ClearCAMFactorDirty()
		{
			m_IsCAMFactorDirty = false;
		}

		protected void SetCADFactorDirty()
		{
			if( !m_IsCADFactorDirty ) {
				m_IsCADFactorDirty = true;
			}
		}

		protected void ClearCADFactorDirty()
		{
			m_IsCADFactorDirty = false;
		}

		#endregion

		#region Start Point Management

		protected void SetStartPoint()
		{
			if( m_CraftData.StartPointIndex < 0 || m_CraftData.StartPointIndex >= m_StartCADPointList.Count ) {
				return;
			}

			List<CADPoint> resortedList = ResortCADPointList( m_StartCADPointList, m_CraftData.StartPointIndex );
			m_StartCAMPointList = ConvertToCAMPoints( resortedList );
		}

		protected void SetPathCAMPoint()
		{
			if( m_CraftData.StartPointIndex < 0 || m_CraftData.StartPointIndex >= m_StartCADPointList.Count ) {
				return;
			}

			// find the corresponding index in MainPathCADPointList
			int mainPathIndex = GetMainPathStartPointIndex( m_CraftData.StartPointIndex );
			if( mainPathIndex < 0 ) {
				return;
			}

			List<CADPoint> resortedList = ResortCADPointList( m_MainPathCADPointList, mainPathIndex );
			m_CAMPointList = ConvertToCAMPoints( resortedList );

			// add closing point to main path (for closed loop)
			if( m_CAMPointList.Count > 0 ) {
				CAMPoint closingPoint = m_CAMPointList[ 0 ].Clone();
				m_CAMPointList.Add( closingPoint );
			}
		}

		protected void SetRefCoordSelfRotated( double rotationAngle_deg )
		{
			const double ANGLE_TOLERANCE_DEG = 0.0001;
			if( Math.Abs( rotationAngle_deg ) > ANGLE_TOLERANCE_DEG ) {
				double rotationAngleInRadians = rotationAngle_deg * Math.PI / 180.0;
				gp_Ax1 rotationAxis = new gp_Ax1( m_RefCoord.Location(), m_RefCoord.Direction() );
				m_RefCoord.Rotate( rotationAxis, rotationAngleInRadians );
			}
		}

		protected abstract void SetCenterDir();

		#endregion

		#region Helper Methods

		List<CADPoint> ResortCADPointList( List<CADPoint> cadPointList, int startIndex )
		{
			if( cadPointList == null || cadPointList.Count == 0 ) {
				return new List<CADPoint>();
			}

			int effectiveStartIndex = startIndex % cadPointList.Count;
			if( effectiveStartIndex < 0 ) {
				effectiveStartIndex += cadPointList.Count;
			}

			List<CADPoint> resortedList = new List<CADPoint>( cadPointList.Count );
			for( int i = 0; i < cadPointList.Count; i++ ) {
				int sourceIndex = ( i + effectiveStartIndex ) % cadPointList.Count;
				resortedList.Add( cadPointList[ sourceIndex ].Clone() );
			}
			return resortedList;
		}

		List<CAMPoint> ConvertToCAMPoints( List<CADPoint> cadPointList )
		{
			if( cadPointList == null || cadPointList.Count == 0 ) {
				return new List<CAMPoint>();
			}

			List<CAMPoint> camPointList = new List<CAMPoint>( cadPointList.Count );
			foreach( CADPoint cadPoint in cadPointList ) {
				CADPoint point = cadPoint.Clone();
				camPointList.Add( new CAMPoint( point ) );
			}
			return camPointList;
		}

		int GetMainPathStartPointIndex( int startPointIndexInStartList )
		{
			if( startPointIndexInStartList < 0 || startPointIndexInStartList >= m_StartCADPointList.Count ) {
				return -1;
			}

			CADPoint startPoint = m_StartCADPointList[ startPointIndexInStartList ];
			const double TOLERANCE = 0.001;

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
		protected gp_Ax1 m_ComputeRefCenterDir;
		protected List<CADPoint> m_MainPathCADPointList = new List<CADPoint>();
		protected List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		protected List<CADPoint> m_StartCADPointList = new List<CADPoint>();
		protected List<CAMPoint> m_StartCAMPointList = new List<CAMPoint>();
		protected List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		protected List<CAMPoint> m_OverCutCAMPointList = new List<CAMPoint>();
		protected CraftData m_CraftData;
		protected CAMPoint m_RefPoint;
		protected double m_MaxOverCutLength;
		protected bool m_IsCAMFactorDirty = false;
		protected bool m_IsCADFactorDirty = false;
		protected IStdPatternGeomData m_ComputeGeomData;

		#endregion
	}
}
