using MyCAM.Data;
using MyCAM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.CacheInfo
{
	public class ContourCacheInfo : ICacheInfo
	{
		public ContourCacheInfo( string szID, ContourGeomData geomData, CraftData craftData, bool isClose )
		{
			if( string.IsNullOrEmpty( szID ) || geomData == null || craftData == null ) {
				throw new ArgumentNullException( "ContourCacheInfo constructing argument null" );
			}
			if( geomData.CADPointList.Count == 0 ) {
				throw new ArgumentException( "ContourCacheInfo constructing argument empty cadPointList" );
			}
			UID = szID;
			m_CADPointList = geomData.CADPointList;
			m_ConnectCADPointMap = geomData.ConnectPointMap;
			m_CraftData = craftData;
			IsClosed = isClose;
			m_CraftData.ParameterChanged += SetCraftDataDirty;
			BuildCAMPointList();
		}

		public string UID
		{
			get; private set;
		}

		public PathType PathType
		{
			get
			{
				return PathType.Contour;
			}
		}

		#region computation result

		internal List<CAMPoint> CAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_CAMPointList;
			}
		}

		internal List<CAMPoint> LeadInCAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_LeadInCAMPointList;
			}
		}

		internal List<CAMPoint> LeadOutCAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_LeadOutCAMPointList;
			}
		}

		internal List<CAMPoint> OverCutCAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_OverCutPointList;
			}
		}

		public CAMPoint GetProcessStartPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			CAMPoint camPoint = null;
			if( m_LeadInCAMPointList.Count > 0 && m_CraftData.LeadLineParam.LeadIn.Length > 0 ) {
				camPoint = m_LeadInCAMPointList.First().Clone();
			}
			else if( m_CAMPointList.Count > 0 ) {
				camPoint = m_CAMPointList.First().Clone();
			}
			return camPoint;
		}

		public CAMPoint GetProcessEndPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			CAMPoint camPoint = null;
			if( m_LeadOutCAMPointList.Count > 0 && m_CraftData.LeadLineParam.LeadOut.Length > 0 ) {
				camPoint = m_LeadOutCAMPointList.Last().Clone();
			}
			else if( m_OverCutPointList.Count > 0 && m_CraftData.OverCutLength > 0 ) {
				camPoint = m_OverCutPointList.Last().Clone();
			}
			else if( m_CAMPointList.Count > 0 ) {
				camPoint = m_CAMPointList.Last().Clone();
			}
			return camPoint;
		}
		#endregion

		#region API
		// when the shape has tranform, need to call this to update the cache info
		public void DoTransform()
		{
			BuildCAMPointList();
		}
		#endregion

		#region craft data
		public int StartPointIndex
		{
			get
			{
				return m_CraftData.StartPointIndex;
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

		public bool GetToolVecModify( int index, out double dRA_deg, out double dRB_deg )
		{
			if( m_CraftData.ToolVecModifyMap.ContainsKey( index ) ) {
				dRA_deg = m_CraftData.ToolVecModifyMap[ index ].Item1;
				dRB_deg = m_CraftData.ToolVecModifyMap[ index ].Item2;
				return true;
			}
			else {
				dRA_deg = 0;
				dRB_deg = 0;
				return false;
			}
		}

		// TODO: fix the stupid way to get the modified index
		public HashSet<int> GetToolVecModifyIndex()
		{
			// simulate the index modification
			List<int> modifiedIndices = new List<int>();
			for( int i = 0; i < m_CADPointList.Count; i++ ) {
				modifiedIndices.Add( i );
			}

			// index change due to start point
			if( m_CraftData.StartPointIndex != 0 ) {

				// need to map back to original index
				List<int> rearrangedIndices = new List<int>();
				for( int i = 0; i < modifiedIndices.Count; i++ ) {
					rearrangedIndices.Add( modifiedIndices[ ( i + m_CraftData.StartPointIndex ) % modifiedIndices.Count ] );
				}
				modifiedIndices = rearrangedIndices;
			}

			// index change due to reverse
			if( m_CraftData.IsReverse ) {

				// modify start point index for closed path
				modifiedIndices.Reverse();
				if( IsClosed ) {
					int lastIndex = modifiedIndices.Last();
					modifiedIndices.RemoveAt( modifiedIndices.Count - 1 );
					modifiedIndices.Insert( 0, lastIndex );
				}
			}

			// the final mapping
			HashSet<int> result = new HashSet<int>();
			for( int i = 0; i < modifiedIndices.Count; i++ ) {
				if( m_CraftData.ToolVecModifyMap.ContainsKey( modifiedIndices[ i ] ) ) {
					result.Add( i );
				}
			}
			return result;
		}
		#endregion

		public bool IsClosed
		{
			get; private set;
		}

		void BuildCAMPointList()
		{
			m_IsCraftDataDirty = false;

			// bild inital CAM point list
			m_CAMPointList = new List<CAMPoint>();
			foreach( CADPoint cadPoint in m_CADPointList ) {
				CAMPoint camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st );
				m_CAMPointList.Add( camPoint );
				if( m_ConnectCADPointMap.ContainsKey( cadPoint ) ) {
					CADPoint connectedCADPoint = m_ConnectCADPointMap[ cadPoint ];
					CAMPoint connectedCAMPoint = new CAMPoint( connectedCADPoint, connectedCADPoint.NormalVec_1st );
					m_ConnectCAMPointMap.Add( camPoint, connectedCAMPoint );
				}
			}

			// set tool vector
			List<IToolVecPoint> toolVecPointList = m_CAMPointList.Cast<IToolVecPoint>().ToList();
			ToolVecHelper.SetToolVec( ref toolVecPointList, m_CraftData.ToolVecModifyMap, IsClosed, m_CraftData.IsToolVecReverse );
			foreach( var oneConnect in m_ConnectCAMPointMap ) {
				oneConnect.Value.ToolVec = oneConnect.Key.ToolVec;
			}

			// set start point and orientation
			SetStartPoint();
			SetOrientation();

			// close the loop if is closed
			if( IsClosed && m_CAMPointList.Count > 0 ) {
				CAMPoint startPoint = m_CAMPointList[ 0 ];
				CAMPoint connectedCAMPoint = m_ConnectCAMPointMap.ContainsKey( startPoint )
												? m_ConnectCAMPointMap[ startPoint ]
												: startPoint.Clone();
				m_CAMPointList.Add( connectedCAMPoint );
			}

			// set over cut
			List<IOverCutPoint> camPointOverCutList = m_CAMPointList.Cast<IOverCutPoint>().ToList();
			OverCutHelper.SetOverCut( camPointOverCutList, out List<IOverCutPoint> overCutPointList, m_CraftData.OverCutLength, IsClosed );
			m_OverCutPointList = overCutPointList.Cast<CAMPoint>().ToList();

			// set lead
			List<ILeadLinePoint> mainPointList = m_CAMPointList.Cast<ILeadLinePoint>().ToList();
			List<ILeadLinePoint> overCutPointList2 = m_OverCutPointList.Cast<ILeadLinePoint>().ToList();
			LeadHelper.SetLeadIn( mainPointList, out List<ILeadLinePoint> leadInPointList, m_CraftData.LeadLineParam, m_CraftData.IsReverse );
			m_LeadInCAMPointList = leadInPointList.Cast<CAMPoint>().ToList();
			LeadHelper.SetLeadOut( mainPointList, overCutPointList2, out List<ILeadLinePoint> leadOutPointList, m_CraftData.LeadLineParam, m_CraftData.IsReverse );
			m_LeadOutCAMPointList = leadOutPointList.Cast<CAMPoint>().ToList();
		}

		void SetStartPoint()
		{
			// rearrange cam points to start from the strt index
			if( m_CraftData.StartPointIndex != 0 ) {
				List<CAMPoint> newCAMPointList = new List<CAMPoint>();
				for( int i = 0; i < m_CAMPointList.Count; i++ ) {
					newCAMPointList.Add( m_CAMPointList[ ( i + m_CraftData.StartPointIndex ) % m_CAMPointList.Count ] );
				}
				m_CAMPointList = newCAMPointList;
			}
		}

		void SetOrientation()
		{
			// reverse the cad points if is reverse
			if( m_CraftData.IsReverse ) {
				m_CAMPointList.Reverse();

				// modify start point index for closed path
				if( IsClosed ) {
					CAMPoint lastPoint = m_CAMPointList.Last();
					m_CAMPointList.Remove( lastPoint );
					m_CAMPointList.Insert( 0, lastPoint );
				}
			}
		}

		void SetCraftDataDirty()
		{
			if( !m_IsCraftDataDirty ) {
				m_IsCraftDataDirty = true;
			}
		}

		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		Dictionary<CAMPoint, CAMPoint> m_ConnectCAMPointMap = new Dictionary<CAMPoint, CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadOutCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();

		// they are sibling pointer, and change the declare order
		CraftData m_CraftData;
		List<CADPoint> m_CADPointList = new List<CADPoint>();
		Dictionary<CADPoint, CADPoint> m_ConnectCADPointMap = new Dictionary<CADPoint, CADPoint>();

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
	}
}
