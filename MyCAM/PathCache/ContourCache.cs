using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
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

		public List<int> OriginalIndexMap
		{
			get
			{
				if( m_IsCADFactorDirty ) {
					BuildCADCAMPointList();
				}
				return m_OriginalIndexMap;
			}
		}

		public List<Tuple<int, int, EToolVecInterpolateType>> GetMapedModifyMap()
		{
			return m_interpolateTypeRegion;
		}

		// convert a stored RCorner endpoint to a CAM index
		public bool TryGetRCornerCAMIndex( int nCADIndex, out int camIndex )
		{
			camIndex = -1;
			if( MainPathPointList.Count == 0 ) {
				return false;
			}

			// machining end point: the tail of the CAM point list
			if( nCADIndex == SpecialPointIndex.CLOSED_POINT_INDEX ) {
				camIndex = MainPathPointList.Count - 1;
				return true;
			}

			// real CAD index (including the machining start point)
			return CADToCAMIndexMap.TryGetValue( nCADIndex, out camIndex );
		}

		// convert to a valid ordered CAM interval [startCam, endCam]
		public bool TryGetRCornerCAMRange( RCornerData range, out int startCam, out int endCam )
		{
			startCam = -1;
			endCam = -1;
			if( range == null ) {
				return false;
			}
			if( !TryGetRCornerCAMIndex( range.StartIndex, out int cam1 ) ) {
				return false;
			}
			if( !TryGetRCornerCAMIndex( range.EndIndex, out int cam2 ) ) {
				return false;
			}
			startCam = Math.Min( cam1, cam2 );
			endCam = Math.Max( cam1, cam2 );
			return startCam != endCam;
		}

		public bool ValidateRCornerData()
		{
			List<RCornerData> validatedRanges = new List<RCornerData>();
			foreach( KeyValuePair<int, RCornerData> item in m_CraftData.RCornerMap ) {
				RCornerData range = item.Value;
				if( range == null || item.Key != range.StartIndex
				   || !TryGetRCornerCAMRange( range, out int startCam, out int endCam ) ) {
					return false;
				}

				foreach( RCornerData existingRange in validatedRanges ) {
					TryGetRCornerCAMRange( existingRange, out int existingStartCam, out int existingEndCam );
					if( IsRCornerCAMRangeOverlap( startCam, endCam, existingStartCam, existingEndCam ) ) {
						return false;
					}
				}
				validatedRanges.Add( range );
			}
			return true;
		}

		public bool TryBuildAddRCornerChange( RCornerData range, out List<RCornerData> rangesToRemove, out RCornerData rangeToAdd )
		{
			return TryBuildRCornerChange( range, null, out rangesToRemove, out rangeToAdd );
		}

		// validate whether the range can be added, ignoring an optional range that is about to be replaced
		public bool CanAddRCorner( RCornerData range, RCornerData rangeToIgnore = null )
		{
			if( !TryGetRCornerCAMRange( range, out int newStart, out int newEnd ) ) {
				return false;
			}

			// overlap / containment check against existing ranges, in CAM space
			foreach( RCornerData existingRange in m_CraftData.RCornerMap.Values ) {
				if( existingRange == rangeToIgnore ) {
					continue;
				}
				if( !TryGetRCornerCAMRange( existingRange, out int existStart, out int existEnd ) ) {
					continue;
				}

				if( IsRCornerCAMRangeOverlap( newStart, newEnd, existStart, existEnd ) ) {
					return false;
				}
			}
			return true;
		}

		bool IsRCornerCAMRangeOverlap( int startCam, int endCam, int otherStartCam, int otherEndCam )
		{
			return startCam < otherEndCam && endCam > otherStartCam;
		}

		bool TryBuildRCornerChange( RCornerData range, RCornerData rangeToIgnore,
			 out List<RCornerData> rangesToRemove, out RCornerData rangeToAdd )
		{
			rangesToRemove = new List<RCornerData>();
			rangeToAdd = null;
			if( range == null || !CanAddRCorner( range, rangeToIgnore ) ) {
				return false;
			}
			if( rangeToIgnore != null ) {
				rangesToRemove.Add( rangeToIgnore );
			}
			rangeToAdd = BuildMergedRCornerRange( range, rangeToIgnore, rangesToRemove );
			return true;
		}

		// switch a range touching the path start/end to the other side
		public bool TryBuildSwitchRCornerChange( RCornerData range, int pathStartIndex,
			 out List<RCornerData> rangesToRemove, out RCornerData rangeToAdd )
		{
			rangesToRemove = new List<RCornerData>();
			rangeToAdd = null;
			if( range == null ) {
				return false;
			}

			// whole path cannot be switched
			if( range.StartIndex == pathStartIndex && range.EndIndex == SpecialPointIndex.CLOSED_POINT_INDEX ) {
				return false;
			}

			RCornerData newRange = null;

			// starts from the path start => switch to end at the path end
			if( range.StartIndex == pathStartIndex ) {
				newRange = new RCornerData( range.EndIndex, SpecialPointIndex.CLOSED_POINT_INDEX );
			}

			// ends at the path end => switch to start from the path start
			else if( range.EndIndex == SpecialPointIndex.CLOSED_POINT_INDEX ) {
				newRange = new RCornerData( pathStartIndex, range.StartIndex );
			}

			if( newRange == null ) {
				return false;
			}
			return TryBuildRCornerChange( newRange, range, out rangesToRemove, out rangeToAdd );
		}

		// find the range containing the point, and whether the point is one of its endpoints
		public bool TryGetRCornerSelectionInfo( int nCADIndex, out RCornerData range, out RCornerData endPointRange )
		{
			range = null;
			endPointRange = null;
			if( !TryGetRCornerCAMIndex( nCADIndex, out int nCam ) ) {
				return false;
			}

			foreach( RCornerData currentRange in m_CraftData.RCornerMap.Values ) {
				if( !TryGetRCornerCAMRange( currentRange, out int startCam, out int endCam ) ) {
					continue;
				}
				if( nCam < startCam || nCam > endCam ) {
					continue;
				}

				range = currentRange;
				if( nCam == startCam || nCam == endCam ) {
					endPointRange = currentRange;
				}
				break;
			}
			return range != null;
		}

		// build a merged range without modifying CraftData
		RCornerData BuildMergedRCornerRange( RCornerData start, RCornerData rangeToIgnore, List<RCornerData> rangesToRemove )
		{
			int nMergeCount = 0;
			int nMergeLimit = m_CraftData.RCornerMap.Count + 1;
			RCornerData current = start;
			while( nMergeCount < nMergeLimit ) {
				nMergeCount++;
				if( !TryGetRCornerCAMRange( current, out int curStart, out int curEnd ) ) {
					return current;
				}

				RCornerData mergeTarget = null;
				bool isTargetPreceding = false;

				foreach( RCornerData other in m_CraftData.RCornerMap.Values ) {
					if( other == rangeToIgnore || rangesToRemove.Contains( other ) ) {
						continue;
					}
					if( !TryGetRCornerCAMRange( other, out int otherStart, out int otherEnd ) ) {
						continue;
					}

					// other's end touches current's start
					if( otherEnd == curStart ) {
						mergeTarget = other;
						isTargetPreceding = true;
						break;
					}

					// other's start touches current's end
					if( otherStart == curEnd ) {
						mergeTarget = other;
						isTargetPreceding = false;
						break;
					}
				}

				if( mergeTarget == null ) {
					return current;
				}

				// build the merged range: keep the outer stored endpoints of the two ranges
				int mergedStartStored = GetStoredEndpointByCAM( isTargetPreceding ? mergeTarget : current, true );
				int mergedEndStored = GetStoredEndpointByCAM( isTargetPreceding ? current : mergeTarget, false );
				RCornerData merged = new RCornerData( mergedStartStored, mergedEndStored );

				rangesToRemove.Add( mergeTarget );
				current = merged;
			}
			return current;
		}

		// the stored endpoint whose CAM index is smaller (takeMin=true) or larger (takeMin=false)
		int GetStoredEndpointByCAM( RCornerData range, bool takeMin )
		{
			if( TryGetRCornerCAMIndex( range.StartIndex, out int nCamStart )
				&& TryGetRCornerCAMIndex( range.EndIndex, out int nCamEnd ) ) {
				bool startIsMin = nCamStart <= nCamEnd;
				if( startIsMin == takeMin ) {
					return range.StartIndex;
				}
				return range.EndIndex;
			}
			return takeMin ? range.StartIndex : range.EndIndex;
		}

		#endregion

		#region API

		// when the shape has tranform, need to call this to update the cache info
		public void DoTransform( gp_Trsf trasform )
		{
			BuildCADCAMPointList();
		}

		#endregion

		#region Build pnt

		void BuildCADCAMPointList()
		{
			m_IsCADFactorDirty = false;
			SetCenterDir();

			// Step 1: apply offset on raw geometry (before transform)
			List<CADPoint> offsetResult = ContourOffsetHelper.ApplyOffset(
				m_ContourGeomData.CADPointList,
				m_ContourGeomData.ConnectPointMap,
				m_CraftData.CompensatedDistance,
				m_IsClose,
				out List<int> originalIndexMap );

			// fallback: if offset returns null (degenerate), use raw geometry without offset
			if( offsetResult == null ) {
				offsetResult = m_ContourGeomData.CADPointList.Select( p => p.Clone() ).ToList();
				originalIndexMap = Enumerable.Range( 0, offsetResult.Count ).ToList();
			}
			m_OriginalIndexMap = originalIndexMap;

			// Step 2: apply global transform on offset result
			for( int i = 0; i < offsetResult.Count; i++ ) {
				offsetResult[ i ].Transform( m_CraftData.CumulativeTrsfMatrix );
			}

			// Step 3: remap ContourEditMap keys from ori CAD index to offset index
			Dictionary<int, ContourEditData> remappedEditMap = RemapContourEditMap(
				m_CraftData.ContourEditMap, m_OriginalIndexMap );

			// Step 4: apply local CAD point displacement on top of offset + transform
			m_CADPointList = ContourEditHelper.ApplyContourEdit( offsetResult, remappedEditMap, m_IsClose );

			m_RefCoord = StdPatternHelper.GetPatternRefCoord( m_ComputeRefCenterDir, false );
			BuildCAMPointList();
		}

		void BuildCAMPointList()
		{
			m_IsCAMFactorDirty = false;

			// build initial CAM point list, not closed yet
			m_CAMPointList = new List<CAMPoint>();
			for( int i = 0; i < m_CADPointList.Count; i++ ) {

				// build CAM point
				CADPoint cadPoint = m_CADPointList[ i ];
				CAMPoint camPoint = new CAMPoint( cadPoint, m_CraftData.IsToolVecReverse );
				camPoint.InitPathIndex = ( m_OriginalIndexMap != null && i < m_OriginalIndexMap.Count )
					? m_OriginalIndexMap[ i ]
					: i;
				m_CAMPointList.Add( camPoint );
			}

			// set start point and orientation
			SetStartPoint();
			SetOrientation();

			// create index map consider the start point and orientation
			CreateIndexMap();

			// close the loop if is closed
			if( m_IsClose && m_CAMPointList.Count > 0 ) {
				CAMPoint startPoint = m_CAMPointList[ 0 ];
				CAMPoint closedCAMPoint = startPoint.Clone();
				closedCAMPoint.InitPathIndex = SpecialPointIndex.CLOSED_POINT_INDEX;
				m_CAMPointList.Add( closedCAMPoint );
			}

			ContourEditHelper.ApplyStartPntContourEdit(
				m_CAMPointList,
				m_CraftData.StartPntContourEditData?.StartPnt,
				m_CraftData.StartPntContourEditData?.EndPnt,
				GetCAMContourEditControlPointIndices() );

			// set tool vector
			List<ISetToolVecPoint> toolVecPointList = m_CAMPointList.Cast<ISetToolVecPoint>().ToList();
			CalMSForEachPoint( toolVecPointList );

			// mark micro joint start and end pnt
			SetMicroJoint( ref m_CAMPointList, ref m_CADToCAMIndexMap );

			// set over cut
			IReadOnlyList<CAMPoint> copyedCAMPointList = m_CAMPointList.Select( p => p.Clone() ).ToList();
			OverCutHelper.SetOverCut( copyedCAMPointList, out m_OverCutPointList, m_CraftData.OverCutLength, m_IsClose, m_CraftData.IsToolVecReverse );

			// set lead
			LeadHelper.SetLeadIn( m_CAMPointList.First().Clone(), out m_LeadInCAMPointList, m_CraftData.LeadData, m_CraftData.IsPathReverse, m_CraftData.IsToolVecReverse );

			// get lead out start point (last over cut pnt if exist, otherwise last main path pnt)
			CAMPoint leadOutStartPoint = GetLeadOutStartPnt();
			LeadHelper.SetLeadOut( leadOutStartPoint, out m_LeadOutCAMPointList, m_CraftData.LeadData, m_CraftData.IsPathReverse, m_CraftData.IsToolVecReverse );
			ToolVecHelper.UpdateTiltAngles( m_CAMPointList, m_OverCutPointList, m_LeadInCAMPointList, m_LeadOutCAMPointList );
		}

		#endregion

		#region Point Mapping

		void CreateIndexMap()
		{
			m_CADToCAMIndexMap.Clear();
			for( int i = 0; i < m_CAMPointList.Count; i++ ) {
				m_CADToCAMIndexMap[ m_CAMPointList[ i ].InitPathIndex ] = i;
			}
		}

		Dictionary<int, ContourEditData> RemapContourEditMap(
			IReadOnlyDictionary<int, ContourEditData> oriEditMap,
			List<int> originalIndexMap )
		{
			Dictionary<int, ContourEditData> remapped = new Dictionary<int, ContourEditData>();
			if( oriEditMap == null || oriEditMap.Count == 0 || originalIndexMap == null ) {
				return remapped;
			}

			// build reverse map: ori CAD index -> offset index
			Dictionary<int, int> oriToOffsetMap = new Dictionary<int, int>();
			for( int i = 0; i < originalIndexMap.Count; i++ ) {
				int oriIdx = originalIndexMap[ i ];
				if( oriIdx >= 0 && !oriToOffsetMap.ContainsKey( oriIdx ) ) {
					oriToOffsetMap[ oriIdx ] = i;
				}
			}

			foreach( var kvp in oriEditMap ) {
				if( oriToOffsetMap.ContainsKey( kvp.Key ) ) {
					remapped[ oriToOffsetMap[ kvp.Key ] ] = kvp.Value.Clone();
				}
			}
			return remapped;
		}

		Dictionary<int, ToolVecModifyData> GetCAMToolVecModifyMap()
		{
			Dictionary<int, ToolVecModifyData> toolVecModifyMap = new Dictionary<int, ToolVecModifyData>();
			foreach( int oneIndex in m_CraftData.ToolVecModifyMap.Keys ) {
				if( m_CADToCAMIndexMap.ContainsKey( oneIndex ) ) {
					int camIndex = m_CADToCAMIndexMap[ oneIndex ];
					toolVecModifyMap[ camIndex ] = m_CraftData.ToolVecModifyMap[ oneIndex ].Clone();
				}
			}
			return toolVecModifyMap;
		}

		void SetStartAndEndPntIntoMap( ref List<ISetToolVecPoint> toolVecPointList, ref Dictionary<int, ToolVecModifyData> toolVecModifyMap )
		{
			if( toolVecPointList == null || toolVecPointList.Count == 0 ) {
				return;
			}
			if( m_CraftData.StartPntToolVecData == null ) {
				m_CraftData.StartPntToolVecData = new StartPntToolVecParam();
			}
			// first region head
			toolVecModifyMap[ 0 ] = m_CraftData.StartPntToolVecData.StartPnt.Clone();

			// start pnt is not modified pnt
			if( toolVecModifyMap[ 0 ].AngleData == null ) {

				// set as init IK result
				ToolVecAngleData toolVecAngleData = new ToolVecAngleData( 0, 0, toolVecPointList.First().ModMaster_rad * 180.0 / Math.PI, toolVecPointList.First().ModSlave_rad * 180.0 / Math.PI );
				toolVecModifyMap[ 0 ].AngleData = toolVecAngleData;
			}
			else {
				toolVecPointList[ 0 ].IsToolVecModPoint = true;
			}

			// last region end
			int nLastIndex = m_CAMPointList.Count - 1;
			toolVecModifyMap[ nLastIndex ] = m_CraftData.StartPntToolVecData.EndPnt.Clone();

			// end pnt is not modified pnt
			if( toolVecModifyMap[ nLastIndex ].AngleData == null ) {
				ToolVecAngleData toolVecAngleData = new ToolVecAngleData( 0, 0, toolVecPointList.Last().ModMaster_rad * 180.0 / Math.PI, toolVecPointList.Last().ModSlave_rad * 180.0 / Math.PI );
				toolVecModifyMap[ nLastIndex ].AngleData = toolVecAngleData;
			}
			else {
				toolVecPointList[ nLastIndex ].IsToolVecModPoint = true;
			}
		}


		void SetStartAndEndPntInMapForSpecialCase( ref List<ISetToolVecPoint> toolVecPointList, ref Dictionary<int, ToolVecModifyData> toolVecModifyMap )
		{
			if( m_CraftData.StartPntToolVecData.StartPnt == null || m_CraftData.StartPntToolVecData.StartPnt.AngleData == null ) {
				return;

			}
			if( toolVecPointList == null || toolVecPointList.Count == 0 ) {
				return;
			}
			// set region head
			toolVecModifyMap[ 0 ] = m_CraftData.StartPntToolVecData.StartPnt.Clone();

			// start pnt is not modified pnt (breveling case start pnt must be control point)
			toolVecPointList[ 0 ].IsToolVecModPoint = true;

			// last region end
			int nLastIndex = m_CAMPointList.Count - 1;

			// get end interpolate type
			toolVecModifyMap[ nLastIndex ] = m_CraftData.StartPntToolVecData.EndPnt.Clone();

			//clone start pnt angle data to end pnt
			toolVecModifyMap[ nLastIndex ].AngleData = m_CraftData.StartPntToolVecData.StartPnt.AngleData.Clone();
		}

		// add index of contorl point to CAM ContourEditData 
		List<int> GetCAMContourEditControlPointIndices()
		{
			List<int> camIndexList = new List<int>();
			if( m_CraftData.ContourEditMap == null || m_CraftData.ContourEditMap.Count == 0 ) {
				return camIndexList;
			}

			foreach( int oneOriIndex in m_CraftData.ContourEditMap.Keys ) {

				// the control point may be removed by offset, just skip it
				if( !m_CADToCAMIndexMap.ContainsKey( oneOriIndex ) ) {
					continue;
				}
				camIndexList.Add( m_CADToCAMIndexMap[ oneOriIndex ] );
			}

			camIndexList.Sort();
			return camIndexList;
		}


		#endregion

		#region Tool Vector Calculation

		void CalMSForEachPoint( List<ISetToolVecPoint> toolVecPointList )
		{
			SolveInitIK();
			m_interpolateTypeRegion = new List<Tuple<int, int, EToolVecInterpolateType>>();

			switch( m_CraftData.SpecialToolVecData.Type ) {

				// lock one rotary axis, keep the solved IK result of the other axis
				case ESpecialToolVecType.LockMaster:
				case ESpecialToolVecType.LockSlave:
					ToolVecHelper.SetToolVecByLockedAxis( ref toolVecPointList, m_CraftData.SpecialToolVecData.Type == ESpecialToolVecType.LockMaster );
					break;

				// assign ms set ModMS directly, no need to solve IK
				case ESpecialToolVecType.AssignMS:
					ToolVecHelper.SetToolVecByAssignedMS( ref toolVecPointList, m_CraftData.SpecialToolVecData.MasterAngle_deg, m_CraftData.SpecialToolVecData.SlaveAngle_deg );
					break;

				// FixDir and ZDir need init IK then use the solved start point to set the tool vec for all points
				case ESpecialToolVecType.FixDir:
					SetToolVecByAssignDir( ref toolVecPointList, m_CraftData.IsToolVecReverse ? m_ContourGeomData.RefCenterDir.Direction().Reversed() : m_ContourGeomData.RefCenterDir.Direction() );
					break;
				case ESpecialToolVecType.ZDir:
					if( !DataGettingHelper.GetMachineData( out FiveAxisMachineData machineData ) || machineData == null ) {
						break;
					}
					SetToolVecByAssignDir( ref toolVecPointList, RotaryAxisHelper.GetRotaryAxisDir( machineData.RotaryAxisConfig.RotaryAxisType ) );
					break;

				case ESpecialToolVecType.Normal:
				default:

					// get the info for tool vec interpolation
					InterpolatePreprocessingResult preprocessingResult = InterpolatePreprocessing( ref toolVecPointList );

					// calculate tool vec for each point
					ToolVecHelper.SetToolVec( ref toolVecPointList, preprocessingResult.RegionTypeList, preprocessingResult.CamToolVecModifyMap, preprocessingResult.IsFirstPntControlPnt, preprocessingResult.IsLastPntControlPoint );

					// for tool vec dialog select action to no current index interpolate type
					m_interpolateTypeRegion = preprocessingResult.RegionTypeList;
					break;
			}
		}

		// use the init solved start point to get the ms angle of the target direction, then apply it to all points
		void SetToolVecByAssignDir( ref List<ISetToolVecPoint> toolVecPointList, gp_Dir targetDir )
		{
			// get init solve IK startPnt
			CAMPoint FirstPntCAMPoint = MainPathPointList.First().Clone();

			// get ms angle ( apply tool vec reverse then solve, unified in ToolVecHelper )
			Tuple<double, double> avgMS_deg = ToolVecHelper.GetMSAngleFromToolVec( targetDir, FirstPntCAMPoint );

			// assign machining direction the rotary axis is decided by the start point phase
			if( TryGetAssignedMachiningDirRotaryAngle_deg( out double dAlignedRotary_deg ) ) {
				avgMS_deg = new Tuple<double, double>( avgMS_deg.Item1, dAlignedRotary_deg );
			}
			ToolVecHelper.SetToolVecByAssignedMS( ref toolVecPointList, avgMS_deg.Item1, avgMS_deg.Item2 );
		}

		// turns the start point of this contour onto the assigned world direction
		bool TryGetAssignedMachiningDirRotaryAngle_deg( out double dRotaryAngle_deg )
		{
			dRotaryAngle_deg = 0;
			if( m_CraftData.SpecialToolVecData.Type != ESpecialToolVecType.ZDir
				|| m_CraftData.SpecialToolVecData.MachiningDir == EMachiningDir.None ) {
				return false;
			}
			if( !DataGettingHelper.GetMachineData( out FiveAxisMachineData machineData ) ) {
				return false;
			}

			if( machineData.FiveAxisType != FiveAxisType.Mix && machineData.FiveAxisType != FiveAxisType.Table ) {
				return false;
			}

			// get the start point phase in G54
			double dStartPhase_deg = GetStartPointAngleAtRotaryCenterInG54_deg( machineData );

			// world phase = start point phase + rotary angle, the target world phase is assigned by the user
			double dTargetWorldPhase_deg = RotaryAxisHelper.GetMachiningDirPhase_deg( m_CraftData.SpecialToolVecData.MachiningDir );
			double dTargetRotaryAngle = dTargetWorldPhase_deg - dStartPhase_deg;

			// pre path end rotary deg
			double dPrevRotaryAngle_deg = m_CraftData.InitSlave_rad * 180.0 / Math.PI;

			// get the nearest angle
			dRotaryAngle_deg = RotaryAxisHelper.GetNearestEquivalentAngle_deg( dTargetRotaryAngle, dPrevRotaryAngle_deg );

			return true;
		}

		// the phase of the path start point about the G54 origin
		double GetStartPointAngleAtRotaryCenterInG54_deg( FiveAxisMachineData machineData )
		{
			if( machineData == null || MainPathPointList.Count == 0 ) {
				return 0;
			}
			RotaryAxis rotaryAxis = machineData.RotaryAxisConfig.RotaryAxisType;

			// the start point already carries the placement transform, otherwise it would not sit where it is machined
			gp_Pnt startPnt = MainPathPointList.First().Point;

			// get startPnt angle about the axis of rotation
			return RotaryAxisHelper.GetPhaseAngle_deg( startPnt, null, rotaryAxis );
		}

		InterpolatePreprocessingResult InterpolatePreprocessing( ref List<ISetToolVecPoint> toolVecPointList )
		{
			bool isLastPntControlPoint = m_CraftData.IsStartPntModified( false, out _, out EToolVecInterpolateType endPntInterpolateType );
			bool isFirstPntIsControlPnt = m_CraftData.IsStartPntModified( true, out _, out EToolVecInterpolateType startPntInterpolateType );

			// only start pnt is contrl pnt && no other modify pnt
			bool isSpecialCase = isFirstPntIsControlPnt && !isLastPntControlPoint && m_CraftData.ToolVecModifyMap.Count == 0;
			if( isSpecialCase ) {

				// user only want c+/c- with normal solve IK (end pnt involve)
				// path reverse interpolate type record at region head
				if( m_CraftData.IsPathReverse ) {
					if( startPntInterpolateType == EToolVecInterpolateType.Normal
						|| endPntInterpolateType == EToolVecInterpolateType.MasterNormalSlaveInterpolation
						|| endPntInterpolateType == EToolVecInterpolateType.MasterInterpolationSlaveNormal ) {
						return NormalCasePreprocessing( ref toolVecPointList );
					}
				}
				else {
					if( endPntInterpolateType == EToolVecInterpolateType.Normal
						|| endPntInterpolateType == EToolVecInterpolateType.MasterNormalSlaveInterpolation
						|| endPntInterpolateType == EToolVecInterpolateType.MasterInterpolationSlaveNormal ) {
						return NormalCasePreprocessing( ref toolVecPointList );
					}
				}
				Dictionary<int, ToolVecModifyData> camToolVecModifyMap = new Dictionary<int, ToolVecModifyData>();
				SetStartAndEndPntInMapForSpecialCase( ref toolVecPointList, ref camToolVecModifyMap );
				List<Tuple<int, int, EToolVecInterpolateType>> regionTypeList = GetInterpolateIntervalList( camToolVecModifyMap );
				return new InterpolatePreprocessingResult
				{
					CamToolVecModifyMap = camToolVecModifyMap,
					RegionTypeList = regionTypeList,
					IsLastPntControlPoint = false,
					IsFirstPntControlPnt = true
				};
			}
			return NormalCasePreprocessing( ref toolVecPointList );
		}

		InterpolatePreprocessingResult NormalCasePreprocessing( ref List<ISetToolVecPoint> toolVecPointList )
		{
			// get all control point index ( include start and end point)
			Dictionary<int, ToolVecModifyData> camToolVecModifyMap = GetCAMToolVecModifyMap();
			SetAsModifyPnt( ref toolVecPointList, camToolVecModifyMap );
			SetStartAndEndPntIntoMap( ref toolVecPointList, ref camToolVecModifyMap );
			List<Tuple<int, int, EToolVecInterpolateType>> regionTypeList = GetInterpolateIntervalList( camToolVecModifyMap );
			bool isLastPntControlPoint = m_CraftData.IsStartPntModified( false, out _, out _ );
			bool isFirstPntIsControlPnt = m_CraftData.IsStartPntModified( true, out _, out _ );

			return new InterpolatePreprocessingResult
			{
				CamToolVecModifyMap = camToolVecModifyMap,
				RegionTypeList = regionTypeList,
				IsLastPntControlPoint = isLastPntControlPoint,
				IsFirstPntControlPnt = isFirstPntIsControlPnt
			};
		}

		List<Tuple<int, int, EToolVecInterpolateType>> GetInterpolateIntervalList( IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap )
		{
			List<Tuple<int, int, EToolVecInterpolateType>> intervalList = new List<Tuple<int, int, EToolVecInterpolateType>>();
			if( toolVecModifyMap == null || toolVecModifyMap.Count == 0 ) {
				return intervalList;
			}
			// sort the modify data by index
			List<int> indexInOrder = toolVecModifyMap.Keys.ToList();
			indexInOrder.Sort();
			for( int i = 0; i < indexInOrder.Count - 1; i++ ) {
				if( m_CraftData.IsPathReverse ) {
					intervalList.Add( new Tuple<int, int, EToolVecInterpolateType>( indexInOrder[ i ], indexInOrder[ i + 1 ], toolVecModifyMap.ContainsKey( indexInOrder[ i ] ) ? toolVecModifyMap[ indexInOrder[ i ] ].InterpolateType : EToolVecInterpolateType.Normal ) );
				}
				else {
					intervalList.Add( new Tuple<int, int, EToolVecInterpolateType>( indexInOrder[ i ], indexInOrder[ i + 1 ], toolVecModifyMap.ContainsKey( indexInOrder[ i + 1 ] ) ? toolVecModifyMap[ indexInOrder[ i + 1 ] ].InterpolateType : EToolVecInterpolateType.Normal ) );
				}
			}
			return intervalList;
		}

		void SolveInitIK()
		{
			// arrange solver
			if( !DataGettingHelper.GetMachineData( out FiveAxisMachineData machineData ) ) {
				throw new Exception( "ContourCache SolveInitIK get machine data failed" );
			}
			PostSolver postSolver = new PostSolver( machineData );

			// init master and slave angle, use CraftData reference values if set
			double dLastProcessPathM = m_CraftData.InitMaster_rad;
			double dLastProcessPathS = m_CraftData.InitSlave_rad;

			// solve IK
			for( int i = 0; i < m_CAMPointList.Count; i++ ) {
				SolvIK( postSolver, m_CAMPointList[ i ], dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
			}

			return;
		}

		void SolvIK( PostSolver postSolver, CAMPoint camPoint, double dLastProcessPathM, double dLastProcessPathS, out double dOutLastProcessPathM, out double dOutLastProcessPathS )
		{
			IKSolveResult ikResult = postSolver.SolveIK( camPoint.InitToolVec, dLastProcessPathM, dLastProcessPathS, out dOutLastProcessPathM, out dOutLastProcessPathS );
			if( ikResult == IKSolveResult.InvalidInput || ikResult == IKSolveResult.NoSolution ) {
				camPoint.InitMaster_rad = 0;
				camPoint.InitSlave_rad = 0;
				camPoint.ModMaster_rad = 0;
				camPoint.ModSlave_rad = 0;
				return;
			}
			else if( ikResult == IKSolveResult.OutOfRange ) {
				// temporary do nothing
			}
			camPoint.InitMaster_rad = dOutLastProcessPathM;
			camPoint.InitSlave_rad = dOutLastProcessPathS;
			camPoint.ModMaster_rad = dOutLastProcessPathM;
			camPoint.ModSlave_rad = dOutLastProcessPathS;
			camPoint.ToolVec = postSolver.SolveToolVec( dOutLastProcessPathM, dOutLastProcessPathS );
		}

		#endregion

		CAMPoint GetLeadOutStartPnt()
		{
			CAMPoint leadOutStartPoint;
			if( m_OverCutPointList.Count > 0 ) {
				leadOutStartPoint = m_OverCutPointList.Last();
			}
			else {
				leadOutStartPoint = m_CAMPointList.Last();
			}
			return leadOutStartPoint;
		}

		void SetAsModifyPnt( ref List<ISetToolVecPoint> toolVecPointList, Dictionary<int, ToolVecModifyData> camToolVecModifyMap )
		{
			if( toolVecPointList == null || camToolVecModifyMap == null ) {
				return;
			}
			foreach( var KeyValue in camToolVecModifyMap ) {
				if( KeyValue.Key >= 0 && KeyValue.Key < toolVecPointList.Count ) {
					toolVecPointList[ KeyValue.Key ].IsToolVecModPoint = true;
				}
			}
		}

		void SetStartPoint()
		{
			// convert ori CAD start index to offset index
			int offsetStartIndex = ConvertOriToOffsetIndex( m_CraftData.StartPointIndex );

			// rearrange cam points to start from the offset start index
			if( offsetStartIndex != 0 ) {
				List<CAMPoint> newCAMPointList = new List<CAMPoint>();
				for( int i = 0; i < m_CAMPointList.Count; i++ ) {
					newCAMPointList.Add( m_CAMPointList[ ( i + offsetStartIndex ) % m_CAMPointList.Count ] );
				}
				m_CAMPointList = newCAMPointList;
			}
		}

		int ConvertOriToOffsetIndex( int oriCADIndex )
		{
			if( m_OriginalIndexMap == null || m_OriginalIndexMap.Count == 0 ) {
				return oriCADIndex;
			}

			// try direct lookup in reverse map
			for( int i = 0; i < m_OriginalIndexMap.Count; i++ ) {
				if( m_OriginalIndexMap[ i ] == oriCADIndex ) {
					return i;
				}
			}

			// ori CAD index was removed by offset, search backwards (wrap around) for nearest alive ori index
			int totalOriCount = m_ContourGeomData.CADPointList.Count;
			for( int step = 1; step < totalOriCount; step++ ) {
				int prevOriIdx = ( oriCADIndex - step + totalOriCount ) % totalOriCount;
				for( int i = 0; i < m_OriginalIndexMap.Count; i++ ) {
					if( m_OriginalIndexMap[ i ] == prevOriIdx ) {
						return i;
					}
				}
			}

			// fallback: start from 0
			return 0;
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

		void SetMicroJoint( ref List<CAMPoint> camPointList, ref Dictionary<int, int> CADToCAMIndexMap )
		{
			// set micro joint
			List<Tuple<int, double>> microJointStartCAMIdxList = GetMicroJointStartCAMIdx();
			MicroJointHelper.SetMicroJoint( ref camPointList, ref CADToCAMIndexMap, microJointStartCAMIdxList );
		}

		List<Tuple<int, double>> GetMicroJointStartCAMIdx()
		{
			List<Tuple<int, double>> microJointStartCAMIdxList = new List<Tuple<int, double>>();
			foreach( int oneIndex in m_CraftData.MicroJointStartIdxMap.Keys ) {
				if( m_CADToCAMIndexMap.ContainsKey( oneIndex ) ) {
					double microJointLength = m_CraftData.MicroJointStartIdxMap[ oneIndex ];
					int camIndex = m_CADToCAMIndexMap[ oneIndex ];
					microJointStartCAMIdxList.Add( new Tuple<int, double>( camIndex, microJointLength ) );
				}
			}
			return microJointStartCAMIdxList;
		}

		struct InterpolatePreprocessingResult
		{
			public Dictionary<int, ToolVecModifyData> CamToolVecModifyMap;
			public List<Tuple<int, int, EToolVecInterpolateType>> RegionTypeList;
			public bool IsLastPntControlPoint;
			public bool IsFirstPntControlPnt;
		}

		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		List<Tuple<int, int, EToolVecInterpolateType>> m_interpolateTypeRegion;

		// for CAM point connection
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadOutCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();
		Dictionary<int, int> m_CADToCAMIndexMap = new Dictionary<int, int>();

		// they are sibling pointer, and change the declare order
		CraftData m_CraftData;
		List<CADPoint> m_CADPointList = new List<CADPoint>();     // offset + global transform + local edit, used by CAM pipeline
		List<int> m_OriginalIndexMap = null;                      // offset idx -> ori CAD idx mapping

		// flag to indicate craft data changed
		bool m_IsCAMFactorDirty = false;
		bool m_IsCADFactorDirty = false;
		bool m_IsClose = false;

		gp_Ax3 m_RefCoord = new gp_Ax3();
		gp_Ax1 m_ComputeRefCenterDir = new gp_Ax1();
		ContourGeomData m_ContourGeomData = null;
	}
}
