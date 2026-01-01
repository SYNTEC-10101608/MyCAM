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

			// set tool vector
			List<ISetToolVecPoint> toolVecPointList = m_CAMPointList.Cast<ISetToolVecPoint>().ToList();
			ToolVecHelper.SetToolVec( ref toolVecPointList, m_CraftData.ToolVecModifyMap, m_IsClose, m_CraftData.IsToolVecReverse, m_CraftData.InterpolateType, m_RefCenterDir );
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

		void SetInitIKResult()
		{
			// reset initial IK result
			m_InitIKResult.Clear();

			// get machine data
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return;
			}

			// create a copy of CAM points considering start point and orientation
			List<CAMPoint> tempCAMPointList = new List<CAMPoint>();
			for( int i = 0; i < m_CAMPointList.Count; i++ ) {
				tempCAMPointList.Add( m_CAMPointList[ i ].Clone() );
			}

			// apply start point transformation
			if( m_CraftData.StartPointIndex != 0 ) {
				List<CAMPoint> newTempList = new List<CAMPoint>();
				for( int i = 0; i < tempCAMPointList.Count; i++ ) {
					newTempList.Add( tempCAMPointList[ ( i + m_CraftData.StartPointIndex ) % tempCAMPointList.Count ] );
				}
				tempCAMPointList = newTempList;
			}

			// apply orientation transformation (reverse)
			if( m_CraftData.IsPathReverse ) {
				tempCAMPointList.Reverse();

				// modify start point index for closed path
				if( m_IsClose ) {
					CAMPoint lastPoint = tempCAMPointList.Last();
					tempCAMPointList.Remove( lastPoint );
					tempCAMPointList.Insert( 0, lastPoint );
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

			// now we need to reverse the transformations to align with raw CAD point indices
			// create a list with same size as CAD points
			List<Tuple<double, double>> alignedResult = new List<Tuple<double, double>>( new Tuple<double, double>[ m_CADPointList.Count ] );

			// map transformed indices back to original indices
			for( int transformedIndex = 0; transformedIndex < ikResult.Count; transformedIndex++ ) {
				// reverse the transformations to find original index

				// step 1: reverse orientation transformation
				int afterOrientationIndex = transformedIndex;
				if( m_CraftData.IsPathReverse ) {
					if( m_IsClose && transformedIndex == 0 ) {
						// the first point after reverse is originally the last
						afterOrientationIndex = m_CADPointList.Count - 1;
					}
					else if( m_IsClose ) {
						// shift by one because of the rotation, then reverse
						afterOrientationIndex = m_CADPointList.Count - transformedIndex;
					}
					else {
						// simple reverse
						afterOrientationIndex = m_CADPointList.Count - 1 - transformedIndex;
					}
				}

				// step 2: reverse start point transformation
				int originalIndex = afterOrientationIndex;
				if( m_CraftData.StartPointIndex != 0 ) {
					originalIndex = ( afterOrientationIndex - m_CraftData.StartPointIndex + m_CADPointList.Count ) % m_CADPointList.Count;
				}

				// store the IK result at the original index
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
			out List<Tuple<double,double>> resultG54, ref double dLastProcessPathM, ref double dLastProcessPathS )
		{
			resultG54 = new List<Tuple<double,double>>();
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
