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
			m_CADPointList = geomData.CADPointList;
			m_ConnectCADPointMap = geomData.ConnectPointMap;
			m_CraftData = craftData;
			m_IsClose = geomData.IsClosed;
			m_RefCenterDir = geomData.RefCenterDir;
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

		public List<CADPoint> MainPathCADPointList
		{
			get
			{
				return m_CADPointList;
			}
		}

		public List<Tuple<double, double>> InitIKResult
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_InitIKResult;
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

			// set initial IK result before start point and orientation changes
			SetInitIKResult();

			// create index tracking to map ToolVecModifyMap indices after transformations
			List<int> indexMapping = new List<int>();

			// initialize with original indices
			for( int i = 0; i < m_CAMPointList.Count; i++ ) {
				indexMapping.Add( i );
			}

			// set start point and orientation first
			SetStartPoint();
			SetOrientation();

			// apply start point transformation to index mapping
			if( m_CraftData.StartPointIndex != 0 ) {
				List<int> newIndexMapping = new List<int>();
				for( int i = 0; i < indexMapping.Count; i++ ) {
					int sourceIndex = ( i + m_CraftData.StartPointIndex ) % indexMapping.Count;
					newIndexMapping.Add( indexMapping[ sourceIndex ] );
				}
				indexMapping = newIndexMapping;
			}

			// apply orientation transformation (reverse) to index mapping
			if( m_CraftData.IsPathReverse ) {
				indexMapping.Reverse();

				// modify start point index for closed path
				if( m_IsClose ) {
					int lastIndex = indexMapping.Last();
					indexMapping.RemoveAt( indexMapping.Count - 1 );
					indexMapping.Insert( 0, lastIndex );
				}
			}

			// create transformed ToolVecModifyMap: map from transformed index to original modifications
			Dictionary<int, Tuple<double, double>> transformedToolVecModifyMap = new Dictionary<int, Tuple<double, double>>();
			foreach( var kvp in m_CraftData.ToolVecModifyMap ) {
				int originalIndex = kvp.Key;
				Tuple<double, double> modification = kvp.Value;

				// find which transformed index corresponds to this original index
				for( int transformedIndex = 0; transformedIndex < indexMapping.Count; transformedIndex++ ) {
					if( indexMapping[ transformedIndex ] == originalIndex ) {
						transformedToolVecModifyMap[ transformedIndex ] = modification;
						break;
					}
				}
			}

			int mod = -1;

			// set tool vector with transformed map
			List<ISetToolVecPoint> toolVecPointList = m_CAMPointList.Cast<ISetToolVecPoint>().ToList();
			ToolVecHelper.SetToolVec( ref toolVecPointList, transformedToolVecModifyMap, m_IsClose, m_CraftData.IsToolVecReverse, m_CraftData.InterpolateType, m_RefCenterDir, mod );
			foreach( var oneConnect in m_ConnectCAMPointMap ) {
				oneConnect.Value.ToolVec = oneConnect.Key.ToolVec;
			}

			// close the loop if is closed
			if( m_IsClose && m_CAMPointList.Count > 0 ) {
				CAMPoint startPoint = m_CAMPointList[ 0 ];
				CAMPoint connectedCAMPoint = /*m_ConnectCAMPointMap.ContainsKey( startPoint )
												? m_ConnectCAMPointMap[ startPoint ]
												: */startPoint.Clone();
				connectedCAMPoint.Master += mod * Math.PI;
				// if mod is odd, negate slave
				if( mod % 2 != 0 ) {
					connectedCAMPoint.Slave = -connectedCAMPoint.Slave;
				}
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

		void SetInitIKResult()
		{
			// reset initial IK result
			m_InitIKResult.Clear();

			// get machine data
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return;
			}

			// create index tracking: each element stores the original CAD point index
			List<int> indexMapping = new List<int>();
			List<CAMPoint> tempCAMPointList = new List<CAMPoint>();

			// initialize with original indices
			for( int i = 0; i < m_CAMPointList.Count; i++ ) {
				tempCAMPointList.Add( m_CAMPointList[ i ].Clone() );
				indexMapping.Add( i );
			}

			// apply start point transformation
			if( m_CraftData.StartPointIndex != 0 ) {
				List<CAMPoint> newTempList = new List<CAMPoint>();
				List<int> newIndexMapping = new List<int>();
				for( int i = 0; i < tempCAMPointList.Count; i++ ) {
					int sourceIndex = ( i + m_CraftData.StartPointIndex ) % tempCAMPointList.Count;
					newTempList.Add( tempCAMPointList[ sourceIndex ] );
					newIndexMapping.Add( indexMapping[ sourceIndex ] );
				}
				tempCAMPointList = newTempList;
				indexMapping = newIndexMapping;
			}

			// apply orientation transformation (reverse)
			if( m_CraftData.IsPathReverse ) {
				tempCAMPointList.Reverse();
				indexMapping.Reverse();

				// modify start point index for closed path
				if( m_IsClose ) {
					CAMPoint lastPoint = tempCAMPointList.Last();
					int lastIndex = indexMapping.Last();

					tempCAMPointList.Remove( lastPoint );
					tempCAMPointList.Insert( 0, lastPoint );

					indexMapping.RemoveAt( indexMapping.Count - 1 );
					indexMapping.Insert( 0, lastIndex );
				}
			}

			// solve IK for transformed points
			PostSolver postSolver = new PostSolver( machineData );
			List<IProcessPoint> processPointList = tempCAMPointList.Cast<IProcessPoint>().ToList();
			double dLastM = 0.0;
			double dLastS = 0.0;
			List<Tuple<double, double>> ikResult;
			if( !SolveProcessPath( postSolver, processPointList, out ikResult, ref dLastM, ref dLastS ) ) {
				return;
			}

			// create result list aligned with raw CAD point indices
			List<Tuple<double, double>> alignedResult = new List<Tuple<double, double>>( new Tuple<double, double>[ m_CADPointList.Count ] );

			// map IK results back to original indices using the tracking map
			for( int transformedIndex = 0; transformedIndex < ikResult.Count; transformedIndex++ ) {
				int originalIndex = indexMapping[ transformedIndex ];
				if( originalIndex >= 0 && originalIndex < alignedResult.Count ) {
					alignedResult[ originalIndex ] = ikResult[ transformedIndex ];
				}
			}

			// assign to member variable
			m_InitIKResult = alignedResult;
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

		static bool SolveProcessPath( PostSolver postSolver, IReadOnlyList<IProcessPoint> pointList,
			out List<Tuple<double, double>> resultG54, ref double dLastProcessPathM, ref double dLastProcessPathS )
		{
			resultG54 = new List<Tuple<double, double>>();
			if( pointList == null || pointList.Count == 0 ) {
				return false;
			}

			// solve IK
			List<bool> singularTagList = new List<bool>();
			foreach( IProcessPoint point in pointList ) {
				IKSolveResult ikResult = postSolver.SolveIK( point, dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
				if( ikResult == IKSolveResult.InvalidInput || ikResult == IKSolveResult.NoSolution || ikResult == IKSolveResult.OutOfRange ) {
					return false;
				}
				resultG54.Add( new Tuple<double, double>( dLastProcessPathM, dLastProcessPathS ) );
				if( ikResult == IKSolveResult.NoError ) {
					singularTagList.Add( false );
				}
				else if( ikResult == IKSolveResult.MasterInfinityOfSolution || ikResult == IKSolveResult.SlaveInfinityOfSolution ) {
					singularTagList.Add( true );
				}
			}
			return true;
		}

		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();

		// for CAM point connection
		Dictionary<CAMPoint, CAMPoint> m_ConnectCAMPointMap = new Dictionary<CAMPoint, CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadOutCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();
		List<Tuple<double, double>> m_InitIKResult = new List<Tuple<double, double>>();

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
		gp_Ax1 m_RefCenterDir;
	}
}
