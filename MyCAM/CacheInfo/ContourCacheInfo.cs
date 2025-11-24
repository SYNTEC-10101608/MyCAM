using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;

namespace MyCAM.CacheInfo
{
	internal class ContourCacheInfo : ICacheInfo
	{
		public ContourCacheInfo( string szID, List<ICADSegmentElement> cadSegmentList, CraftData craftData, bool isClose )
		{
			if( string.IsNullOrEmpty( szID ) || cadSegmentList == null || cadSegmentList.Count == 0 || craftData == null ) {
				throw new ArgumentNullException( "ContourCacheInfo constructing argument null" );
			}
			UID = szID;
			m_CADSegmentList = cadSegmentList;
			m_CraftData = craftData;
			IsClosed = isClose;
			m_CraftData.ParameterChanged += SetCraftDataDirty;
			BuildPathCAMSegment();
			BuildCAMFeatureSegment();
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

		#region result

		public List<ICAMSegmentElement> CAMSegmentList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildPathCAMSegment();
					BuildCAMFeatureSegment();
				}
				return m_CAMSegmentList;
			}
			set
			{
				if( value != null ) {
					m_CAMSegmentList = value;
				}
			}
		}

		public List<int> CtrlToolSegIdxList
		{
			get => m_CtrlToolSegIdxList;
			private set
			{
				if( value != null ) {
					m_CtrlToolSegIdxList = value;
				}
			}
		}

		public List<ICAMSegmentElement> LeadInSegment
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildPathCAMSegment();
					BuildCAMFeatureSegment();
				}
				return m_LeadInSegmentList;
			}
		} 

		public List<ICAMSegmentElement> LeadOutSegment
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildPathCAMSegment();
					BuildCAMFeatureSegment();
				}
				return m_LeadOutSegmentList;
			}
		}

		public List<ICAMSegmentElement> OverCutSegment
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildPathCAMSegment();
					BuildCAMFeatureSegment();
				}
				return m_OverCutSegmentList;
			}
		}

		public bool IsClosed
		{
			get; private set;
		}

		#endregion

		#region Public API

		// when the shape has tranform, need to call this to update the cache info
		public void Transform()
		{
			BuildPathCAMSegment();
			BuildCAMFeatureSegment();
		}

		public CAMPoint GetProcessStartPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildPathCAMSegment();
				BuildCAMFeatureSegment();
			}
			CAMPoint camPoint = null;
			ICAMSegmentElement camSegmentConnectWithStartPnt = null;
			if( m_CraftData.LeadLineParam.LeadIn.Type != LeadLineType.None ) {
				camSegmentConnectWithStartPnt = LeadInSegment.First();
			}
			else {
				camSegmentConnectWithStartPnt = CAMSegmentList.First(); ;
			}
			CAMPoint2 camPoint2 = camSegmentConnectWithStartPnt.StartPoint;
			CADPoint cadPOint2 = new CADPoint( camPoint2.Point, camPoint2.NormalVec_1st, camPoint2.NormalVec_2nd, camPoint2.TangentVec );
			camPoint = new CAMPoint( cadPOint2, camPoint2.ToolVec );
			return camPoint;
		}

		public CAMPoint GetProcessEndPoint()
		 {
			if( m_IsCraftDataDirty ) {
				BuildPathCAMSegment();
				BuildCAMFeatureSegment();
			}
			CAMPoint camPoint = null;
			ICAMSegmentElement camSegmentConnectWithEndPnt = null;
			if( m_CraftData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
				camSegmentConnectWithEndPnt = LeadOutSegment.Last();
			}
			else {
				// with overcut
				if( m_CraftData.OverCutLength > 0 && m_OverCutSegmentList.Count > 0 ) {
					camSegmentConnectWithEndPnt = m_OverCutSegmentList.Last();
				}
				else {
					camSegmentConnectWithEndPnt = CAMSegmentList.Last();
				}
			}
			CAMPoint2 camEndPoint = camSegmentConnectWithEndPnt.EndPoint;
			CADPoint cadEndPnt = new CADPoint( camEndPoint.Point, camEndPoint.NormalVec_1st, camEndPoint.NormalVec_2nd, camEndPoint.TangentVec );
			camPoint = new CAMPoint( cadEndPnt, camEndPoint.ToolVec );
			return camPoint;
		}

		public bool GetPathIsReverse()
		{
			return m_CraftData.IsReverse;
		}

		public SegmentPointIndex GetPathStartPointIndex()
		{
			return m_CraftData.StartPointIndex;
		}

		public LeadData GetPathLeadData()
		{
			return m_CraftData.LeadLineParam;
		}

		public double GetPathOverCutLength()
		{
			return m_CraftData.OverCutLength;
		}

		#endregion

		#region Build CAM Segment

		void BuildPathCAMSegment()
		{
			m_IsCraftDataDirty = false;

			// Step 1: Reorder segments based on start point
			List<ICAMSegmentElement> reorderedSegment = BreakAndReorderByStartPoint();

			// Step 2: Map tool vector control indices to new segment order
			// List<SegmentPointIndex>: Tool vector control points after reordering (adjusted for new segment sequence)
			// Dictionary<SegmentPointIndex, SegmentPointIndex>: 
			// Key = Tool vector control point location in reordered segments
			// Value = Original tool vector control point location before reordering
			List<SegmentPointIndex> toolVecCtrlPntList = ModidyCtrlPntIdxMap( reorderedSegment.Count, out Dictionary<SegmentPointIndex, SegmentPointIndex> CtrlPntMap );

			// Step 3: Break segments at tool vector control points
			// Dictionary<int, SegmentPointIndex>
			// Key: Final segment index (after reordering and breaking) where tool control point is at segment end
			// Value: Original control point location (segment index and point index before any processing)
			List<ICAMSegmentElement> breakedCAMSegment = BreakByToolVecBar( reorderedSegment, toolVecCtrlPntList, CtrlPntMap, out Dictionary<int, SegmentPointIndex> CtrPntMapWithOriIndex );
			
			// Step 4: Adjust tool vectors based on control settings
			List<ICAMSegmentElement> finalCAMSegments = AdjustSegmentToolVec( breakedCAMSegment, CtrPntMapWithOriIndex );

			// Step 5: Set final results
			SetResults( finalCAMSegments, CtrPntMapWithOriIndex );
		}

		void BuildCAMFeatureSegment()
		{
			SetOverCut();
			SetLead();
		}

		List<ICAMSegmentElement> BreakAndReorderByStartPoint()
		{
			const int EXPECTED_SPLIT_COUNT = 2;
			SegmentPointIndex startPoint = m_CraftData.StartPointIndex;
			List<ICAMSegmentElement> reorderedCAMSegmentList = new List<ICAMSegmentElement>();

			// Step 1: Reorder segments starting from startPoint.SegIdx
			IReadOnlyList<ICADSegmentElement> cadSegmentList = CADSegmentList;
			for( int i = 0; i < cadSegmentList.Count; i++ ) {
				int index = ( startPoint.SegIdx + i ) % cadSegmentList.Count;
				bool isBuildSuccess = CADCAMSegmentBuilder.BuildCAMSegment( cadSegmentList[ index ], out ICAMSegmentElement camSegment );
				if( isBuildSuccess ) {
					reorderedCAMSegmentList.Add( camSegment );
				}
			}

			// Stpe2: Check if startPoint.PntIdx is at the last point, start from next segmentno need to break segment
			if( startPoint.PntIdx == cadSegmentList[ startPoint.SegIdx ].PointList.Count - 1 ) {
				ICAMSegmentElement realLastSegment = reorderedCAMSegmentList.First();
				reorderedCAMSegmentList.RemoveAt( 0 );
				reorderedCAMSegmentList.Add( realLastSegment );
				return reorderedCAMSegmentList;
			}

			// Step 3: Split the first segment at startPoint.PntIdx
			bool isSuccess = SplitStartPointSegment( reorderedCAMSegmentList.First(), startPoint.PntIdx, out List<ICAMSegmentElement> splitSegments );

			// delete the first segment and insert breaked segment
			if( isSuccess && splitSegments.Count == EXPECTED_SPLIT_COUNT ) {

				// remove original first segment
				reorderedCAMSegmentList.RemoveAt( 0 );

				// add split segments: first part is the begining of path, second part is the end of path
				reorderedCAMSegmentList.Insert( 0, splitSegments.First() );
				reorderedCAMSegmentList.Add( splitSegments.Last() );
				return reorderedCAMSegmentList;
			}
			return reorderedCAMSegmentList;
		}

		List<SegmentPointIndex> ModidyCtrlPntIdxMap( int SegmentCount, out Dictionary<SegmentPointIndex, SegmentPointIndex> ctrlPntMap )
		{
			// key is new segment info value is old segment info
			ctrlPntMap = new Dictionary<SegmentPointIndex, SegmentPointIndex>();
			List<SegmentPointIndex> modifyMap = m_CraftData.ToolVecModifyMap.Keys.ToList();
			if( modifyMap.Count == 0 ) {
				return modifyMap;
			}

			// call CADSegmentList will waste time
			int oriSegmentCount = CADSegmentList.Count;
			bool isStartPointDoNotSplitSegment = oriSegmentCount == SegmentCount;

			SegmentPointIndex startPoint = m_CraftData.StartPointIndex;
			for( int i = 0; i < modifyMap.Count; i++ ) {

				// start point is at the end of segment, that segment no change
				if( isStartPointDoNotSplitSegment ) {
					SegmentPointIndex backup = modifyMap[ i ];
					int newSegmentIndex = ( modifyMap[ i ].SegIdx - startPoint.SegIdx - 1 + oriSegmentCount ) % oriSegmentCount;
					modifyMap[ i ] = new SegmentPointIndex( newSegmentIndex, modifyMap[ i ].PntIdx );
					ctrlPntMap.Add( modifyMap[ i ], backup );
				}

				// start point is at the middle of segment, that segment be breaked into two segments
				else {

					// not at start segment, need to modify segment index
					if( modifyMap[ i ].SegIdx != startPoint.SegIdx ) {
						SegmentPointIndex backup = modifyMap[ i ];
						int newSegmentIndex = ( ( modifyMap[ i ].SegIdx - startPoint.SegIdx + oriSegmentCount ) % oriSegmentCount );
						modifyMap[ i ] = new SegmentPointIndex( newSegmentIndex, modifyMap[ i ].PntIdx );
						ctrlPntMap.Add( modifyMap[ i ], backup );
					}

					// is at start segment
					else {
						if( modifyMap[ i ].PntIdx > startPoint.PntIdx ) {
							SegmentPointIndex backup = modifyMap[ i ];

							// in the first part
							modifyMap[ i ] = new SegmentPointIndex( 0, modifyMap[ i ].PntIdx - startPoint.PntIdx );
							ctrlPntMap.Add( modifyMap[ i ], backup );
						}
						else {

							// in the last 
							SegmentPointIndex backup = modifyMap[ i ];
							int newSegmentIndex = SegmentCount - 1;
							modifyMap[ i ] = new SegmentPointIndex( newSegmentIndex, modifyMap[ i ].PntIdx );
							ctrlPntMap.Add( modifyMap[ i ], backup );
						}
					}
				}
			}
			return modifyMap;
		}

		bool SplitStartPointSegment( ICAMSegmentElement camSegment, int targetIndex, out List<ICAMSegmentElement> breakedCADSegmentList )
		{
			breakedCADSegmentList = new List<ICAMSegmentElement>();
			if( camSegment == null || targetIndex == 0 || targetIndex == camSegment.CAMPointList.Count - 1 ) {
				return false;
			}
			List<int> startPntIndex = new List<int> { targetIndex };
			List<List<CAMPoint2>> splitedPointList = SplitCAMPointList( camSegment.CAMPointList, startPntIndex, out _ );
			if( splitedPointList.Count != 2 ) {
				return false;
			}
			List<CAMPoint2> pntListAfterTargetIndex = splitedPointList.Last();
			List<CAMPoint2> pntListBeforeTargetIndex = splitedPointList.First();

			bool isFirstBuildSuccess = CADCAMSegmentBuilder.BuildCAMSegmentByCAMPoint( pntListAfterTargetIndex, camSegment.ContourType, camSegment.PerArcLength * ( pntListAfterTargetIndex.Count - 1 ), camSegment.PerArcLength, camSegment.PerChordLength, out ICAMSegmentElement segmentAfterStartPoint );
			bool isLastBuildSuccess = CADCAMSegmentBuilder.BuildCAMSegmentByCAMPoint( pntListBeforeTargetIndex, camSegment.ContourType, camSegment.PerArcLength * ( pntListBeforeTargetIndex.Count - 1 ), camSegment.PerArcLength, camSegment.PerChordLength, out ICAMSegmentElement segmentBeforeStartPoint );
			if( !isFirstBuildSuccess || !isLastBuildSuccess ) {
				return false;
			}
			breakedCADSegmentList.Add( segmentAfterStartPoint );
			breakedCADSegmentList.Add( segmentBeforeStartPoint );
			return true;
		}

		public List<List<CAMPoint2>> SplitCAMPointList( List<CAMPoint2> segmentCAMPointList, List<int> separateLocation, out bool isLastSegmentModify )
		{
			List<List<CAMPoint2>> resultCAMPointList = new List<List<CAMPoint2>>();
			isLastSegmentModify = true;
			if( segmentCAMPointList == null || segmentCAMPointList.Count == 0 ) {
				return resultCAMPointList;
			}
			separateLocation = separateLocation.OrderBy( index => index ).ToList();
			int nStartIndex = 0;
			foreach( int nIndex in separateLocation ) {

				// nuclose path may have tool bar and segment index 0
				if( nIndex == 0 ) {
					continue;
				}

				// avoid out of range
				if( nIndex > segmentCAMPointList.Count - 1 ) {
					break;
				}

				// clone the point to prevent pointer issue
				List<CAMPoint2> partCAMPointList = segmentCAMPointList
					.GetRange( nStartIndex, nIndex - nStartIndex + 1 )
					.Select( point => point.Clone() )
					.ToList();
				resultCAMPointList.Add( partCAMPointList );
				nStartIndex = nIndex;
			}

			// last part
			if( nStartIndex < segmentCAMPointList.Count - 1 ) {
				resultCAMPointList.Add( segmentCAMPointList.GetRange( nStartIndex, segmentCAMPointList.Count - nStartIndex ) );
				isLastSegmentModify = false;
			}
			return resultCAMPointList;
		}

		List<ICAMSegmentElement> BreakByToolVecBar( List<ICAMSegmentElement> orderedCADSegmentList, List<SegmentPointIndex> toolVecCtrlPntList, Dictionary<SegmentPointIndex, SegmentPointIndex> CtrlPntMap, out Dictionary<int, SegmentPointIndex> CtrlPntMapWithOriIdx )
		{
			CtrlPntMapWithOriIdx = new Dictionary<int, SegmentPointIndex>();
			List<ICAMSegmentElement> breakedCAMSegmentList = new List<ICAMSegmentElement>();
			if( toolVecCtrlPntList.Count == 0 ) {
				return orderedCADSegmentList;
			}
			toolVecCtrlPntList.Sort();
			for( int segmentIndex = 0; segmentIndex < orderedCADSegmentList.Count; segmentIndex++ ) {
				List<int> breakPointIndex = new List<int>();

				// get this segment control point
				for( int j = 0; j < toolVecCtrlPntList.Count; j++ ) {
					if( toolVecCtrlPntList[ j ].SegIdx == segmentIndex ) {
						breakPointIndex.Add( toolVecCtrlPntList[ j ].PntIdx );
					}
				}
				// no need to break
				if( breakPointIndex.Count == 0 ) {
					breakedCAMSegmentList.Add( orderedCADSegmentList[ segmentIndex ] );
					continue;
				}

				List<List<CAMPoint2>> splitedCAMPointList = SplitCAMPointList( orderedCADSegmentList[ segmentIndex ].CAMPointList, breakPointIndex, out bool isLastSegmentModify );
				for( int k = 0; k < splitedCAMPointList.Count; k++ ) {
					bool isBuildSuccess = CADCAMSegmentBuilder.BuildCAMSegmentByCAMPoint( splitedCAMPointList[ k ], orderedCADSegmentList[ segmentIndex ].ContourType, orderedCADSegmentList[ segmentIndex ].PerArcLength * splitedCAMPointList[ k ].Count - 1, orderedCADSegmentList[ segmentIndex ].PerArcLength, orderedCADSegmentList[ segmentIndex ].PerChordLength, out ICAMSegmentElement newCAMSegment );
					if( isBuildSuccess ) {
						breakedCAMSegmentList.Add( newCAMSegment );

						// record this segment index with control point
						if( k != splitedCAMPointList.Count - 1 ) {
							SegmentPointIndex oriSegmentIndex = CtrlPntMap[ new SegmentPointIndex( segmentIndex, breakPointIndex[ k ] ) ];
							breakedCAMSegmentList[ breakedCAMSegmentList.Count - 1 ].IsModify = true;
							CtrlPntMapWithOriIdx[ breakedCAMSegmentList.Count - 1 ] = oriSegmentIndex;
						}
						else {

							// last segment with control point
							if( isLastSegmentModify ) {
								SegmentPointIndex oriSegmentIndex = CtrlPntMap[ new SegmentPointIndex( segmentIndex, breakPointIndex[ k ] ) ];
								CtrlPntMapWithOriIdx[ breakedCAMSegmentList.Count - 1 ] = oriSegmentIndex;
								breakedCAMSegmentList[ breakedCAMSegmentList.Count - 1 ].IsModify = true;
							}
						}
					}
				}
			}
			return breakedCAMSegmentList;
		}

		List<ICAMSegmentElement> AdjustSegmentToolVec( List<ICAMSegmentElement> breakedCAMSegment, Dictionary<int, SegmentPointIndex> CtrlPntMapWithIndex )
		{
			List<ICAMSegmentElement> camSegmentList = new List<ICAMSegmentElement>();
			camSegmentList = breakedCAMSegment;
			if( CtrlPntMapWithIndex.Count == 0 ) {
				if( m_CraftData.IsToolVecReverse == false ) {
					return camSegmentList;
				}
				else {
					foreach( ICAMSegmentElement camSegment in camSegmentList ) {
						camSegment.SetStartPointToolVec( camSegment.StartPoint.ToolVec.Reversed() );
						camSegment.SetEndPointToolVec( camSegment.EndPoint.ToolVec.Reversed() );
					}
					return camSegmentList;
				}

			}
			if( CtrlPntMapWithIndex.Count == 1 ) {
				PathWith1CtrlPnt( ref camSegmentList, CtrlPntMapWithIndex );
				return camSegmentList;
			}
			BuildCAMSegmentWithSeveralCtrlPnt( ref camSegmentList, CtrlPntMapWithIndex );
			return camSegmentList;
		}

		void SetResults( List<ICAMSegmentElement> camSegments, Dictionary<int, SegmentPointIndex> CtrlPntMapWithIdx )
		{
			List<int> controlIndices = CtrlPntMapWithIdx.Keys.ToList();
			CAMSegmentList = camSegments ?? new List<ICAMSegmentElement>();
			CtrlToolSegIdxList = controlIndices ?? new List<int>();
		}

		void PathWith1CtrlPnt( ref List<ICAMSegmentElement> breakedCAMSegment, Dictionary<int, SegmentPointIndex> CtrlPntMapWithIdx )
		{
			// calculate tool vec
			int CtrlPntIdx = CtrlPntMapWithIdx.Keys.First();
			SegmentPointIndex oriCtrlPntIndex = CtrlPntMapWithIdx.Values.First();
			m_CraftData.ToolVecModifyMap.TryGetValue( oriCtrlPntIndex, out Tuple<double, double> AB_Value );
			CAMPoint2 targetCAMPoint = breakedCAMSegment[ CtrlPntIdx ].EndPoint;
			gp_Dir oriToolVec = m_CraftData.IsToolVecReverse ? targetCAMPoint.NormalVec_1st.Reversed() : targetCAMPoint.NormalVec_1st;
			gp_Vec ToolVec = GetVecFromAB( oriToolVec, targetCAMPoint.TangentVec, AB_Value.Item1 * Math.PI / 180, AB_Value.Item2 * Math.PI / 180 );

			for( int i = 0; i < breakedCAMSegment.Count; i++ ) {
				breakedCAMSegment[ i ].SetStartPointToolVec( new gp_Dir( ToolVec.XYZ() ) );
				breakedCAMSegment[ i ].SetEndPointToolVec( new gp_Dir( ToolVec.XYZ() ) );
			}
		}

		gp_Vec GetVecFromAB( gp_Dir normal_1st, gp_Dir tangentVec, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( normal_1st );
			}

			// get the x, y, z direction
			gp_Dir x = tangentVec;
			gp_Dir z = normal_1st;
			gp_Dir y = z.Crossed( x );

			// X:Y:Z = tanA:tanB:1
			double X = 0;
			double Y = 0;
			double Z = 0;
			if( dRA_rad == 0 ) {
				X = 0;
				Z = 1;
			}
			else {
				X = dRA_rad < 0 ? -1 : 1;
				Z = X / Math.Tan( dRA_rad );
			}
			Y = Z * Math.Tan( dRB_rad );
			gp_Dir dir1 = new gp_Dir( x.XYZ() * X + y.XYZ() * Y + z.XYZ() * Z );
			return new gp_Vec( dir1.XYZ() );
		}

		void BuildCAMSegmentWithSeveralCtrlPnt( ref List<ICAMSegmentElement> breakedCAMSegment, Dictionary<int, SegmentPointIndex> CtrlPntMapWithIdx )
		{
			if( CtrlPntMapWithIdx.Count == 0 || breakedCAMSegment.Count == 0 ) {
				return;
			}

			// the segment with control point
			List<int> CtrlPntIdxList = CtrlPntMapWithIdx.Keys.ToList();
			CtrlPntIdxList.Sort();

			Dictionary<SegmentPointIndex, Tuple<double, double>> ToolVecModifyMap = m_CraftData.ToolVecModifyMap;
			for( int i = 0; i < breakedCAMSegment.Count; i++ ) {
				List<int> ctrlPntRange = FindPntIndexRange( CtrlPntIdxList, i );
				int startBarIndex = ctrlPntRange[ 0 ];
				int endBarIndex = ctrlPntRange[ 1 ];
				gp_Vec startToolVec = GetToolVecByBreakedSegmenIndex( breakedCAMSegment, startBarIndex, CtrlPntMapWithIdx, ToolVecModifyMap );
				gp_Vec endToolVec = GetToolVecByBreakedSegmenIndex( breakedCAMSegment, endBarIndex, CtrlPntMapWithIdx, ToolVecModifyMap );

				if( startToolVec == null || endToolVec == null ) {
					continue;
				}

				// calculate total length from start bar to end bar
				double dTotalLength = SumSegmentLength( breakedCAMSegment, startBarIndex, endBarIndex );
				double dLengthFromStartBar = SumSegmentLength( breakedCAMSegment, startBarIndex, i );
				gp_Dir camSegmentStartToolVec = GetInterpolateToolVecByLength( startToolVec, endToolVec, dLengthFromStartBar - breakedCAMSegment[ i ].TotalLength, dTotalLength );
				gp_Dir camSegmentEndToolVec = GetInterpolateToolVecByLength( startToolVec, endToolVec, dLengthFromStartBar, dTotalLength );
				breakedCAMSegment[ i ].SetStartPointToolVec( camSegmentStartToolVec );
				breakedCAMSegment[ i ].SetEndPointToolVec( camSegmentEndToolVec );
			}
		}

		gp_Vec GetToolVecByBreakedSegmenIndex( List<ICAMSegmentElement> breakedCAMSegment, int targetIndex, Dictionary<int, SegmentPointIndex> CtrlPntMapWithIdx, Dictionary<SegmentPointIndex, Tuple<double, double>> ToolVecModifyMap )
		{
			// real control point index in original CAD segment
			if( CtrlPntMapWithIdx.TryGetValue( targetIndex, out SegmentPointIndex oriSegmentIndex ) ) {

				// get AB value
				if( ToolVecModifyMap.TryGetValue( oriSegmentIndex, out Tuple<double, double> AB_Value ) ) {

					CAMPoint2 camPoint = breakedCAMSegment[ targetIndex ].EndPoint;
					gp_Dir oriNormal_1st = m_CraftData.IsToolVecReverse ? camPoint.NormalVec_1st.Reversed() : camPoint.NormalVec_1st;
					gp_Vec ToolVec = GetVecFromAB( oriNormal_1st, camPoint.TangentVec, AB_Value.Item1 * Math.PI / 180, AB_Value.Item2 * Math.PI / 180 );
					return ToolVec;
				}
			}
			return null;
		}

		double SumSegmentLength( List<ICAMSegmentElement> segmentList, int startIndex, int endIndex )
		{
			int nSegmentCount = segmentList.Count;
			if( segmentList == null || segmentList.Count == 0 ) {
				return 0.0;
			}
			if( startIndex < 0 || startIndex >= nSegmentCount || endIndex < 0 || endIndex >= nSegmentCount ) {
				return 0.0;
			}

			double dLength = 0.0;
			int nCurrent = ( startIndex + 1 ) % nSegmentCount;
			while( true ) {
				dLength += segmentList[ nCurrent ].TotalLength;
				if( nCurrent == endIndex ) {
					break;
				}
				nCurrent = ( nCurrent + 1 ) % nSegmentCount;
			}
			return dLength;
		}

		gp_Dir GetInterpolateToolVecByLength( gp_Vec startToolVec, gp_Vec endToolVec, double dDeltaLength, double dTotalLength )
		{
			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startToolVec, endToolVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			gp_Quaternion q = new gp_Quaternion();
			double dWeight = Math.Abs( 1 - ( dDeltaLength / dTotalLength ) ) < 1e-3 ? 1 : dDeltaLength / dTotalLength;
			slerp.Interpolate( dWeight, ref q );
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetRotation( q );
			gp_Dir toolVecDir = new gp_Dir( startToolVec.Transformed( trsf ) );
			return toolVecDir;
		}

		List<int> FindPntIndexRange( List<int> pointIndex, int targetIndex )
		{
			if( pointIndex == null || pointIndex.Count == 0 ) {
				return new List<int>();
			}
			pointIndex.Sort();

			// find first index which >= targetIndex
			int nextCtrlPntPos = pointIndex.FindIndex( x => x >= targetIndex );

			// no found means all bar is smaller than target index
			if( nextCtrlPntPos == -1 ) {
				return new List<int> { pointIndex.Last(), pointIndex.First() };
			}

			// find the largest value less than taget index
			int preCtrlPntPos = ( nextCtrlPntPos - 1 + pointIndex.Count ) % pointIndex.Count;
			return new List<int> { pointIndex[ preCtrlPntPos ], pointIndex[ nextCtrlPntPos ] };
		}

		#endregion

		public bool GetToolVecModify( SegmentPointIndex index, out double dRA_deg, out double dRB_deg )
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

		public HashSet<SegmentPointIndex> GetToolVecModifyIndex()
		{
			HashSet<SegmentPointIndex> result = new HashSet<SegmentPointIndex>();
			foreach( SegmentPointIndex nIndex in m_CraftData.ToolVecModifyMap.Keys ) {
				result.Add( nIndex );
			}
			return result;
		}

		void SetCraftDataDirty()
		{
			if( !m_IsCraftDataDirty ) {
				m_IsCraftDataDirty = true;
			}
		}

		#region Over cut

		void SetOverCut()
		{
			if ( m_CraftData.OverCutLength > 0 ) {
				List<ICAMSegmentElement> overCutSement = OverCutSegmentBuilder.BuildOverCutSegment( m_CAMSegmentList, m_CraftData.OverCutLength ,m_CraftData.IsReverse);
				if( overCutSement.Count > 0 ) {
					m_OverCutSegmentList = overCutSement;
				}
			}
		}

		gp_Pnt GetExactOverCutEndPoint( gp_Pnt currentPoint, gp_Pnt nextPoint, double dDistanceMoveFromOverPoint )
		{
			// from currentPoint → nextOverLengthPoint
			gp_Vec movingVec = new gp_Vec( currentPoint, nextPoint );

			// normalize to unit vector
			movingVec.Normalize();

			gp_Vec moveVec = movingVec.Multiplied( dDistanceMoveFromOverPoint );

			// shifted along the vector
			return new gp_Pnt( currentPoint.XYZ() + moveVec.XYZ() );
		}

		gp_Dir InterpolateVecBetween2Vec( gp_Vec currentVec, gp_Vec nextVec, double interpolatePercent )
		{
			// this case is unsolcvable, so just return current vec
			if( currentVec.IsOpposite( nextVec, MyApp.PRECISION_MIN_ERROR ) ) {
				return new gp_Dir( currentVec.XYZ() );
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( currentVec, nextVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			// calculate new point attitude
			gp_Quaternion q = new gp_Quaternion();
			slerp.Interpolate( interpolatePercent, ref q );
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetRotation( q );
			gp_Dir resultDir = new gp_Dir( currentVec.Transformed( trsf ) );
			return resultDir;
		}

		#endregion

		void SetLead()
		{
			if( m_CraftData.LeadLineParam.LeadIn.Type != LeadLineType.None ) {
				ICAMSegmentElement camSegmentConnectWithStartPnt = CAMSegmentList.FirstOrDefault();
				List<ICAMSegmentElement> leadCAMSegment = LeadHelper.BuildLeadCAMSegment( m_CraftData, camSegmentConnectWithStartPnt, true );
				m_LeadInSegmentList = leadCAMSegment;
			}
			if (m_CraftData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
				ICAMSegmentElement camSegmentConnectWithEndPnt;
				// with overcut
				if( m_CraftData.OverCutLength > 0 && m_OverCutSegmentList.Count > 0 ) {
					camSegmentConnectWithEndPnt = m_OverCutSegmentList.Last();
				}
				else {
					camSegmentConnectWithEndPnt = CAMSegmentList.Last();
				}
				List<ICAMSegmentElement> leadCAMSegment = LeadHelper.BuildLeadCAMSegment( m_CraftData, camSegmentConnectWithEndPnt, false );
				m_LeadOutSegmentList = leadCAMSegment;
			}
		}

		List<ICAMSegmentElement> m_CAMSegmentList = new List<ICAMSegmentElement>();
		List<int> m_CtrlToolSegIdxList = new List<int>();
		List<ICAMSegmentElement> m_OverCutSegmentList = new List<ICAMSegmentElement>();
		List<ICAMSegmentElement> m_LeadInSegmentList = new List<ICAMSegmentElement>();
		List<ICAMSegmentElement> m_LeadOutSegmentList = new List<ICAMSegmentElement>();

		// they are sibling pointer, and change the declare order
		CraftData m_CraftData;
		List<ICADSegmentElement> m_CADSegmentList;
		public IReadOnlyList<ICADSegmentElement> CADSegmentList => m_CADSegmentList;

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
	}
}
