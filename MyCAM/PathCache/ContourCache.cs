using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Post;
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
			m_ContourGeomData = geomData;
			m_CraftData = craftData;
			m_IsClose = geomData.IsClosed;
			m_CraftData.CAMFactorChanged += SetCAMDataDirty;
			m_CraftData.CADFactorChanged += SetCADDataDirty;
			BuildCADCAMPointList();

			// build default start/end point tool vec data after tool vec is set
			// BuildDefaultStartPntToolVecData();
		}

		#region computation result

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
				if( m_IsCAMFactorDirty ) {
					BuildCAMPointList();
				}
				return m_LeadInCAMPointList;
			}
		}

		public List<CAMPoint> LeadOutPointList
		{
			get
			{
				if( m_IsCAMFactorDirty ) {
					BuildCAMPointList();
				}
				return m_LeadOutCAMPointList;
			}
		}

		public List<CAMPoint> OverCutPointList
		{
			get
			{
				if( m_IsCAMFactorDirty ) {
					BuildCAMPointList();
				}
				return m_OverCutPointList;
			}
		}

		public Dictionary<int, int> CADToCAMIndexMap
		{
			get
			{
				if( m_IsCAMFactorDirty ) {
					BuildCAMPointList();
				}
				return m_CADToCAMIndexMap;
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

		public List<CADPoint> TrsfCADPointList
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				return m_CADPointList;
			}
		}

		#endregion

		#region API
		// when the shape has tranform, need to call this to update the cache info
		public void DoTransform( gp_Trsf trasform )
		{
			BuildCADCAMPointList();
		}
		#endregion

		void BuildCADCAMPointList()
		{
			m_IsCADFactorDirty = false;
			SetCenterDir();
			m_CADPointList = m_ContourGeomData.CADPointList.Select( p => p.Clone() ).ToList();
			for( int i = 0; i < m_CADPointList.Count; i++ ) {
				m_CADPointList[ i ].Transform( m_CraftData.CumulativeTrsfMatrix );
			}

			m_ConnectCADPointMap.Clear();
			foreach( var kvp in m_ContourGeomData.ConnectPointMap ) {
				CADPoint transformedKey = kvp.Key.Clone();
				transformedKey.Transform( m_CraftData.CumulativeTrsfMatrix );
				CADPoint transformedValue = kvp.Value.Clone();
				transformedValue.Transform( m_CraftData.CumulativeTrsfMatrix );
				m_ConnectCADPointMap.Add( transformedKey, transformedValue );
			}

			m_RefCoord = StdPatternHelper.GetPatternRefCoord( m_ComputeRefCenterDir, false );
			BuildCAMPointList();
		}

		void BuildCAMPointList()
		{
			m_IsCAMFactorDirty = false;

			// build initial CAM point list, not closed yet
			m_CAMPointList = new List<CAMPoint>();
			m_ConnectCAMPointMap.Clear();
			for( int i = 0; i < m_CADPointList.Count; i++ ) {

				// build CAM point
				CADPoint cadPoint = m_CADPointList[ i ];
				CAMPoint camPoint = new CAMPoint( cadPoint, m_CraftData.IsToolVecReverse );
				camPoint.InitPathIndex = i;
				m_CAMPointList.Add( camPoint );

				// build connection CAM point
				if( m_ConnectCADPointMap.ContainsKey( cadPoint ) ) {
					CAMPoint connectedCAMPoint = new CAMPoint( m_ConnectCADPointMap[ cadPoint ], m_CraftData.IsToolVecReverse );
					m_ConnectCAMPointMap.Add( camPoint, connectedCAMPoint );
				}
			}

			// set start point and orientation
			SetStartPoint();
			SetOrientation();

			// create index map consider the start point and orientation
			CreateIndexMap();

			// close the loop if is closed
			if( m_IsClose && m_CAMPointList.Count > 0 ) {
				CAMPoint startPoint = m_CAMPointList[ 0 ];
				CAMPoint closedCAMPoint = m_ConnectCAMPointMap.ContainsKey( startPoint )
												? m_ConnectCAMPointMap[ startPoint ] // use connected point
												: startPoint.Clone(); // or just clone the start point
				closedCAMPoint.InitPathIndex = CLOSED_POINT_INDEX;
				m_CAMPointList.Add( closedCAMPoint );
			}

			// solve initial IK
			SolveInitIK();

			// use origin IK result as default start/end point tool vec data
			SetDefaultStartEndToolVecParam();

			// set tool vector
			List<ISetToolVecPoint> toolVecPointList = m_CAMPointList.Cast<ISetToolVecPoint>().ToList();

			// get all control point index ( include start and end point);
			Dictionary<int, ToolVecModifyData> toolVecModifyMap = GetToolVecModifyMap();
			ToolVecHelper.SetToolVec( ref toolVecPointList, toolVecModifyMap, m_IsClose, out List<Tuple<int, int, EToolVecInterpolateType>> interpolateRegionList, m_CraftData.IsPathReverse );

			// for tool vec dialog select action to no current index interpolate type
			m_interpolateTypeRegion = interpolateRegionList;

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

		void CreateIndexMap()
		{
			m_CADToCAMIndexMap.Clear();
			for( int i = 0; i < m_CAMPointList.Count; i++ ) {
				m_CADToCAMIndexMap[ m_CAMPointList[ i ].InitPathIndex ] = i;
			}
		}

		Dictionary<int, ToolVecModifyData> GetToolVecModifyMap()
		{
			Dictionary<int, ToolVecModifyData> toolVecModifyMap = new Dictionary<int, ToolVecModifyData>();
			foreach( int oneIndex in m_CraftData.ToolVecModifyMap.Keys ) {
				if( m_CADToCAMIndexMap.ContainsKey( oneIndex ) ) {
					int camIndex = m_CADToCAMIndexMap[ oneIndex ];
					toolVecModifyMap[ camIndex ] = m_CraftData.ToolVecModifyMap[ oneIndex ].Clone();
				}
			}
			if( m_CraftData.IsPathReverse ) {
				toolVecModifyMap[ 0 ] = m_CraftData.StartPntToolVecData.EndPnt.Clone();
			}
			else {
				toolVecModifyMap[ 0 ] = m_CraftData.StartPntToolVecData.StartPnt.Clone();
			}

			if( m_IsClose ) {
				if( m_CraftData.IsPathReverse ) {
					toolVecModifyMap[ CLOSED_POINT_INDEX ] = m_CraftData.StartPntToolVecData.StartPnt.Clone();
				}
				else {
					toolVecModifyMap[ CLOSED_POINT_INDEX ] = m_CraftData.StartPntToolVecData.EndPnt.Clone();
				}
			}
			else {
				int nLastIndex = m_CADPointList.Count - 1;
				if( m_CraftData.IsPathReverse ) {
					toolVecModifyMap[ nLastIndex ] = m_CraftData.StartPntToolVecData.StartPnt.Clone();
				}
				else {
					toolVecModifyMap[ nLastIndex ] = m_CraftData.StartPntToolVecData.EndPnt.Clone();
				}
			}
			return toolVecModifyMap;
		}

		List<Tuple<int, int, EToolVecInterpolateType>> m_interpolateTypeRegion;
		public List<Tuple<int, int, EToolVecInterpolateType>> GetMapedModifyMap()
		{
			return m_interpolateTypeRegion;
		}

		void SolveInitIK()
		{
			// arrange solver
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				throw new Exception( "ContourCache SolveInitIK get machine data failed" );
			}
			PostSolver postSolver = new PostSolver( machineData );

			// init master and slave angle
			double dLastProcessPathM = 0;
			double dLastProcessPathS = 0;

			// solve IK
			// solve IK
			for( int i = 0; i < m_CAMPointList.Count; i++ ) {
				IKSolveResult ikResult = postSolver.SolveIK( m_CAMPointList[ i ].InitToolVec, dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
				if( ikResult == IKSolveResult.InvalidInput || ikResult == IKSolveResult.NoSolution ) {
					m_CAMPointList[ i ].InitMaster_rad = 0;
					m_CAMPointList[ i ].InitSlave_rad = 0;
					m_CAMPointList[ i ].ModMaster_rad = 0;
					m_CAMPointList[ i ].ModSlave_rad = 0;
					continue;
				}
				else if( ikResult == IKSolveResult.OutOfRange ) {
					// temporary do nothing
				}
				m_CAMPointList[ i ].InitMaster_rad = dLastProcessPathM;
				m_CAMPointList[ i ].InitSlave_rad = dLastProcessPathS;
				m_CAMPointList[ i ].ModMaster_rad = dLastProcessPathM;
				m_CAMPointList[ i ].ModSlave_rad = dLastProcessPathS;
			}
			return;
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

		void SetCAMDataDirty()
		{
			if( !m_IsCAMFactorDirty ) {
				m_IsCAMFactorDirty = true;
			}
		}

		void SetCADDataDirty()
		{
			if( !m_IsCADFactorDirty ) {
				m_IsCADFactorDirty = true;
			}
		}

		void SetCenterDir()
		{
			m_ComputeRefCenterDir = m_ContourGeomData.RefCenterDir.Transformed( m_CraftData.CumulativeTrsfMatrix );
		}

		void SetDefaultStartEndToolVecParam()
		{
			if( m_CraftData.StartPntToolVecData == null || m_CraftData.StartPntToolVecData.StartPnt == null || m_CraftData.StartPntToolVecData.EndPnt == null ) {
				ToolVecModifyData startPnt = BuildToolVecModifyData( m_CAMPointList.First().Clone() );
				ToolVecModifyData endPnt = BuildToolVecModifyData( m_CAMPointList.Last().Clone() );
				m_CraftData.StartPntToolVecData = new StartPntToolVecParam( startPnt, endPnt );
			}
		}

		ToolVecModifyData BuildToolVecModifyData( CAMPoint camPoint )
		{
			double master_deg = camPoint.ModMaster_rad * 180.0 / Math.PI;
			double slave_deg = camPoint.ModSlave_rad * 180.0 / Math.PI;
			Tuple<double, double> abAngles = ToolVecHelper.GetABAngleFromMSAngle( master_deg, slave_deg, camPoint );
			return new ToolVecModifyData( abAngles.Item1, abAngles.Item2, master_deg, slave_deg, EToolVecInterpolateType.Normal );
		}

		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();

		// for CAM point connection
		Dictionary<CAMPoint, CAMPoint> m_ConnectCAMPointMap = new Dictionary<CAMPoint, CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadOutCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();
		Dictionary<int, int> m_CADToCAMIndexMap = new Dictionary<int, int>();

		// they are sibling pointer, and change the declare order
		CraftData m_CraftData;
		List<CADPoint> m_CADPointList = new List<CADPoint>();

		// for CAD point connection
		Dictionary<CADPoint, CADPoint> m_ConnectCADPointMap = new Dictionary<CADPoint, CADPoint>();

		// flag to indicate craft data changed
		bool m_IsCAMFactorDirty = false;
		bool m_IsCADFactorDirty = false;
		bool m_IsClose = false;

		gp_Ax3 m_RefCoord = new gp_Ax3();
		gp_Ax1 m_ComputeRefCenterDir = new gp_Ax1();
		ContourGeomData m_ContourGeomData = null;
		const int CLOSED_POINT_INDEX = -1;
	}
}
