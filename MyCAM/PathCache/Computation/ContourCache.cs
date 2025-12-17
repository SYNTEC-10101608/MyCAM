using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class ContourCache : IContourCache
	{
		public ContourCache( ContourGeomData geomData, CraftData craftData )
		{
			if( geomData == null || craftData == null ) {
				throw new ArgumentNullException( "ContourCache constructing argument null" );
			}
			if( geomData.CADPointList.Count == 0 ) {
				throw new ArgumentException( "ContourCache constructing argument empty cadPointList" );
			}
			m_CADPointList = geomData.CADPointList;
			m_ConnectCADPointMap = geomData.ConnectPointMap;
			m_CraftData = craftData;
			m_IsClose = geomData.IsClosed;
			m_CraftData.ParameterChanged += SetCraftDataDirty;
			BuildCAMPointList();
		}

		#region computation result

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
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_LeadOutCAMPointList;
			}
		}

		public List<CAMPoint> OverCutPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_OverCutPointList;
			}
		}

		public List<CADPoint> MainPathPointListForOrder
		{
			get
			{
				return m_CADPointList;
			}
		}

		#endregion

		#region API
		// when the shape has tranform, need to call this to update the cache info
		public void DoTransform( gp_Trsf trasform )
		{
			BuildCAMPointList();
		}
		#endregion

		void BuildCAMPointList()
		{
			m_IsCraftDataDirty = false;

			// build initial CAM point list
			m_CAMPointList = new List<CAMPoint>();
			m_CAMPointIndexMap.Clear();
			m_ConnectCAMPointMap.Clear();
			for( int i = 0; i < m_CADPointList.Count; i++ ) {
				CADPoint cadPoint = m_CADPointList[ i ];
				CAMPoint camPoint = new CAMPoint( cadPoint );
				m_CAMPointIndexMap.Add( camPoint, i );
				m_CAMPointList.Add( camPoint );
				if( m_ConnectCADPointMap.ContainsKey( cadPoint ) ) {
					CADPoint connectedCADPoint = m_ConnectCADPointMap[ cadPoint ];
					CAMPoint connectedCAMPoint = new CAMPoint( connectedCADPoint );
					m_ConnectCAMPointMap.Add( camPoint, connectedCAMPoint );
				}
			}

			// set tool vector
			List<ISetToolVecPoint> toolVecPointList = m_CAMPointList.Cast<ISetToolVecPoint>().ToList();
			ToolVecHelper.SetToolVec( ref toolVecPointList, m_CraftData.ToolVecModifyMap, m_IsClose, m_CraftData.IsToolVecReverse );
			foreach( var oneConnect in m_ConnectCAMPointMap ) {
				oneConnect.Value.ToolVec = oneConnect.Key.ToolVec;
			}

			// set start point and orientation
			SetStartPoint();
			SetOrientation();

			// close the loop if is closed
			if( m_IsClose && m_CAMPointList.Count > 0 ) {
				CAMPoint startPoint = m_CAMPointList[ 0 ];
				CAMPoint connectedCAMPoint = m_ConnectCAMPointMap.ContainsKey( startPoint )
												? m_ConnectCAMPointMap[ startPoint ]
												: startPoint.Clone();
				m_CAMPointList.Add( connectedCAMPoint );
			}

			// set over cut
			List<IOrientationPoint> camPointOverCutList = m_CAMPointList.Cast<IOrientationPoint>().ToList();
			OverCutHelper.SetOverCut( camPointOverCutList, out List<IOrientationPoint> overCutPointList, m_CraftData.OverCutLength, m_IsClose );
			m_OverCutPointList = overCutPointList.Cast<CAMPoint>().ToList();

			// set lead
			List<IOrientationPoint> mainPointList = m_CAMPointList.Cast<IOrientationPoint>().ToList();
			List<IOrientationPoint> overCutPointList2 = m_OverCutPointList.Cast<IOrientationPoint>().ToList();
			LeadHelper.SetLeadIn( mainPointList, out List<IOrientationPoint> leadInPointList, m_CraftData.LeadData, m_CraftData.IsPathReverse );
			m_LeadInCAMPointList = leadInPointList.Cast<CAMPoint>().ToList();
			LeadHelper.SetLeadOut( mainPointList, overCutPointList2, out List<IOrientationPoint> leadOutPointList, m_CraftData.LeadData, m_CraftData.IsPathReverse );
			m_LeadOutCAMPointList = leadOutPointList.Cast<CAMPoint>().ToList();
		}

		void SetStartPoint()
		{
			// rearrange cam points to start from the start index
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
			if( m_CraftData.IsPathReverse ) {
				m_CAMPointList.Reverse();

				// modify start point index for closed path
				if( m_IsClose ) {
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

		// for CAM point connection
		Dictionary<CAMPoint, CAMPoint> m_ConnectCAMPointMap = new Dictionary<CAMPoint, CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadOutCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();

		// they are sibling pointer, and change the declare order
		CraftData m_CraftData;
		List<CADPoint> m_CADPointList = new List<CADPoint>();

		// for CAD point connection
		Dictionary<CADPoint, CADPoint> m_ConnectCADPointMap = new Dictionary<CADPoint, CADPoint>();

		// for index mapping
		Dictionary<CAMPoint, int> m_CAMPointIndexMap = new Dictionary<CAMPoint, int>();

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
		bool m_IsClose = false;
	}
}
