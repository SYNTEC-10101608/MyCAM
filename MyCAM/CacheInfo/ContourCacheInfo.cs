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
		public ContourCacheInfo( string szID, List<ICADSegmentElement> cadPointList, CraftData craftData, bool isClose )
		{
			if( string.IsNullOrEmpty( szID ) || cadPointList == null || cadPointList.Count == 0 || craftData == null ) {
				throw new ArgumentNullException( "ContourCacheInfo constructing argument null" );
			}
			UID = szID;
			m_CADSegmentList = cadPointList;
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

		#region result

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

		List<ICAMSegmentElement> CAMSegmentList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_CAMSegmentList;
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

		public bool IsClosed
		{
			get; private set;
		}

		#endregion

		#region Public API
		// when the shape has tranform, need to call this to update the cache info
		public void Transform()
		{
			BuildCAMPointList();
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

		void BuildCAMPointList()
		{
			m_CAMPointList = new List<CAMPoint>();
			SetToolVec();
			SetOrientation();

			// close the loop if is closed
			if( IsClosed && m_CAMPointList.Count > 0 ) {
				m_CAMPointList.Add( m_CAMPointList[ 0 ].Clone() );
			}

			// all CAM point are settled down, start set lead / overcut
			SetOverCut();
			SetLeadIn();
			SetLeadout();
		}

		#region Build CAM Segment

		void BuildPathCAMSegment()
		{
			m_IsCraftDataDirty = false;
			List<ICAMSegmentElement> reorderedSegment = BreakAndReorderByStartPoint();

			// List<SegmentPointIndex> is new tool stick index after modify, Dictionary<SegmentPointIndex> key is new tool stick index vlaue is old tool stick index
			List<SegmentPointIndex> toolVecControlStickList = ModidyToolBarInexMap( reorderedSegment.Count, out Dictionary<SegmentPointIndex, SegmentPointIndex> ControlStickMap );

			// dictionary<int, (int, int)> key is new tool bar segment index (tool bar must at segment last point index) , value is old tool bar (segment index, point index)
			List<ICAMSegmentElement> breakedCADSegment = BreakByToolVecBar( reorderedSegment, toolVecControlStickList, ControlStickMap, out Dictionary<int, SegmentPointIndex> ControlStickMapAsOriIndex );
			List<ICAMSegmentElement> camSegmentList = AdjustSegmentToolVec( this, breakedCADSegment, ControlStickMapAsOriIndex );
			List<int> controlBarIndex = ControlStickMapAsOriIndex.Keys.ToList();
			this.BreakedCAMSegmentList = camSegmentList;
			this.ControlBarIndexList = controlBarIndex;
		}

		List<ICAMSegmentElement> BreakAndReorderByStartPoint()
		{
			SegmentPointIndex startPoint = m_CraftData.StartPointIndex;
			List<ICAMSegmentElement> reorderedCAMSegmentList = new List<ICAMSegmentElement>();

			// reorder segment list
			IReadOnlyList<ICADSegmentElement> cadSegmentList = CADSegmentList;
			for( int i = 0; i < cadSegmentList.Count; i++ ) {
				int index = ( startPoint.SegIdx + i ) % cadSegmentList.Count;
				bool isBuildSuccess = CADCAMSegmentBuilder.BuildCAMSegment( cadSegmentList[ index ], out ICAMSegmentElement camSegment );
				if( isBuildSuccess ) {
					reorderedCAMSegmentList.Add( camSegment );
				}
			}

			// no need to break segment
			if( startPoint.PntIdx == cadSegmentList[ startPoint.SegIdx ].PointList.Count - 1 ) {
				ICAMSegmentElement realLastSegment = reorderedCAMSegmentList.First();
				reorderedCAMSegmentList.RemoveAt( 0 );
				reorderedCAMSegmentList.Add( realLastSegment );
				return reorderedCAMSegmentList;
			}

			// split segment at start point
			bool isSuccess = SplitStartPointSegment( reorderedCAMSegmentList.First(), startPoint.pointIndex, out List<ICAMSegmentElement> breakedStartSegmentList );

			// delete the first segment and insert breaked segment
			if( isSuccess && breakedStartSegmentList.Count == 2 ) {

				// this segment need to break
				reorderedCAMSegmentList.RemoveAt( 0 );

				// insert breaked segment ( [0] is segment after start point, [1] is segment before start point)
				reorderedCAMSegmentList.Insert( 0, breakedStartSegmentList.First() );
				reorderedCAMSegmentList.Add( breakedStartSegmentList.Last() );
				return reorderedCAMSegmentList;
			}
			return reorderedCAMSegmentList;
		}

		List<SegmentPointIndex> ModidyToolBarInexMap( int SegmentCount, out Dictionary<SegmentPointIndex, SegmentPointIndex> ControlStickMap )
		{
			// key is new segment info value is old segment info
			ControlStickMap = new Dictionary<SegmentPointIndex, SegmentPointIndex>();
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
					ControlStickMap.Add( modifyMap[ i ], backup );
				}

				// start point is at the middle of segment, that segment be breaked into two segments
				else {

					// not at start segment, need to modify segment index
					if( modifyMap[ i ].SegIdx != startPoint.SegIdx ) {
						SegmentPointIndex backup = modifyMap[ i ];
						int newSegmentIndex = ( ( modifyMap[ i ].SegIdx - startPoint.SegIdx + oriSegmentCount ) % oriSegmentCount );
						modifyMap[ i ] = new SegmentPointIndex( newSegmentIndex, modifyMap[ i ].PntIdx );
						ControlStickMap.Add( modifyMap[ i ], backup );
					}

					// is at start segment
					else {
						if( modifyMap[ i ].PntIdx > startPoint.PntIdx ) {
							SegmentPointIndex backup = modifyMap[ i ];

							// in the first part
							modifyMap[ i ] = new SegmentPointIndex( 0, modifyMap[ i ].PntIdx - startPoint.PntIdx );
							ControlStickMap.Add( modifyMap[ i ], backup );
						}
						else {

							// in the last 
							SegmentPointIndex backup = modifyMap[ i ];
							int newSegmentIndex = SegmentCount - 1;
							modifyMap[ i ] = new SegmentPointIndex( newSegmentIndex, modifyMap[ i ].PntIdx );
							ControlStickMap.Add( modifyMap[ i ], backup );
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

		List<ICAMSegmentElement> BreakByToolVecBar( List<ICAMSegmentElement> orderedCADSegmentList, List<SegmentPointIndex> toolVecControlStickList, Dictionary<SegmentPointIndex, SegmentPointIndex> ControlStickMap, out Dictionary<int, SegmentPointIndex> ControlStickMapAsOriIndex )
		{
			ControlStickMapAsOriIndex = new Dictionary<int, SegmentPointIndex>();
			List<ICAMSegmentElement> breakedCAMSegmentList = new List<ICAMSegmentElement>();
			if( toolVecControlStickList.Count == 0 ) {
				return orderedCADSegmentList;
			}
			toolVecControlStickList.Sort();
			for( int segmentIndex = 0; segmentIndex < orderedCADSegmentList.Count; segmentIndex++ ) {
				List<int> breakPointIndex = new List<int>();

				// get this segment control stick
				for( int j = 0; j < toolVecControlStickList.Count; j++ ) {
					if( toolVecControlStickList[ j ].SegIdx == segmentIndex ) {
						breakPointIndex.Add( toolVecControlStickList[ j ].PntIdx );
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

						// record this segment index with control stick
						if( k != splitedCAMPointList.Count - 1 ) {
							SegmentPointIndex oriSegmentIndex = ControlStickMap[ new SegmentPointIndex( segmentIndex, breakPointIndex[ k ] ) ];
							ControlStickMapAsOriIndex[ breakedCAMSegmentList.Count - 1 ] = oriSegmentIndex;
						}
						else {

							// last segment with control stick
							if( isLastSegmentModify ) {
								SegmentPointIndex oriSegmentIndex = ControlStickMap[ new SegmentPointIndex( segmentIndex, breakPointIndex[ k ] ) ];
								ControlStickMapAsOriIndex[ breakedCAMSegmentList.Count - 1 ] = oriSegmentIndex;
							}
						}
					}
				}
			}
			return breakedCAMSegmentList;
		}

		List<ICAMSegmentElement> AdjustSegmentToolVec( List<ICAMSegmentElement> breakedCAMSegment, Dictionary<int, SegmentPointIndex> ControlBarMapedAsIndex )
		{
			List<ICAMSegmentElement> camSegmentList = new List<ICAMSegmentElement>();
			camSegmentList = breakedCAMSegment;
			if( ControlBarMapedAsIndex.Count == 0 ) {
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
			if( ControlBarMapedAsIndex.Count == 1 ) {
				PathWith1ControlStick( ref camSegmentList, ControlBarMapedAsIndex );
				return camSegmentList;
			}
			BuildCAMSegmentWithSeveralToolBar( ref camSegmentList, ControlBarMapedAsIndex );
			return camSegmentList;
		}

		void PathWith1ControlStick( ref List<ICAMSegmentElement> breakedCAMSegment, Dictionary<int, SegmentPointIndex> ControlBarMapedAsIndex )
		{
			// calculate tool vec
			int toolStickIndex = ControlBarMapedAsIndex.Keys.First();
			SegmentPointIndex oriToolStickIndex = ControlBarMapedAsIndex.Values.First();
			m_CraftData.ToolVecModifyMap.TryGetValue( oriToolStickIndex, out Tuple<double, double> AB_Value );
			CAMPoint2 targetCAMPoint = breakedCAMSegment[ toolStickIndex ].EndPoint;
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

		void BuildCAMSegmentWithSeveralToolBar( ref List<ICAMSegmentElement> breakedCAMSegment, Dictionary<int, (int, int)> ControlBarMapedAsIndex )
		{
			if( ControlBarMapedAsIndex.Count == 0 || breakedCAMSegment.Count == 0 ) {
				return;
			}
			List<ICAMSegmentElement> camSegmentList = new List<ICAMSegmentElement>();

			// the segment with control stick
			List<int> ControlStickIndexList = ControlBarMapedAsIndex.Keys.ToList();
			ControlStickIndexList.Sort();

			Dictionary<SegmentPointIndex, Tuple<double, double>> ToolVecModifyMap = m_CraftData.ToolVecModifyMap;
			for( int i = 0; i < breakedCAMSegment.Count; i++ ) {
				List<int> controlStickRange = FindBarIndexRange( ControlStickIndexList, i );
				int startBarIndex = controlStickRange[ 0 ];
				int endBarIndex = controlStickRange[ 1 ];
				gp_Vec startToolVec = GetToolVecByBreakedSegmenIndex( camData, breakedCAMSegment, startBarIndex, ControlBarMapedAsIndex, ToolVecModifyMap );
				gp_Vec endToolVec = GetToolVecByBreakedSegmenIndex( camData, breakedCAMSegment, endBarIndex, ControlBarMapedAsIndex, ToolVecModifyMap );

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

		List<int> FindBarIndexRange( List<int> barIndex, int targetIndex )
		{
			if( barIndex == null || barIndex.Count == 0 ) {
				return new List<int>();
			}
			barIndex.Sort();

			// find first index which >= targetIndex
			int nextBarPos = barIndex.FindIndex( x => x >= targetIndex );

			// no found means all bar is smaller than target index
			if( nextBarPos == -1 ) {
				return new List<int> { barIndex.Last(), barIndex.First() };
			}

			// find the largest value less than taget index
			int prevBarPos = ( nextBarPos - 1 + barIndex.Count ) % barIndex.Count;
			return new List<int> { barIndex[ prevBarPos ], barIndex[ nextBarPos ] };
		}

		#endregion

		void SetToolVec()
		{
			for( int i = 0; i < m_CADSegmentList.Count; i++ ) {

				// calculate tool vector
				CADPoint cadPoint = m_CADSegmentList[ i ];
				CAMPoint camPoint;
				if( m_CraftData.IsToolVecReverse ) {
					camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st.Reversed() );
				}
				else {
					camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st );
				}
				m_CAMPointList.Add( camPoint );
			}
			ModifyToolVec();
		}

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

		public HashSet<int> GetToolVecModifyIndex()
		{
			HashSet<int> result = new HashSet<int>();
			foreach( int nIndex in m_CraftData.ToolVecModifyMap.Keys ) {
				result.Add( nIndex );
			}
			return result;
		}

		void ModifyToolVec()
		{
			if( m_CraftData.ToolVecModifyMap.Count == 0 ) {
				return;
			}

			// all tool vector are modified to the same value, no need to do interpolation
			if( m_CraftData.ToolVecModifyMap.Count == 1 ) {
				gp_Vec newVec = GetVecFromAB( m_CAMPointList[ m_CraftData.ToolVecModifyMap.Keys.First() ],
					m_CraftData.ToolVecModifyMap.Values.First().Item1 * Math.PI / 180,
					m_CraftData.ToolVecModifyMap.Values.First().Item2 * Math.PI / 180 );
				foreach( CAMPoint camPoint in m_CAMPointList ) {
					camPoint.ToolVec = new gp_Dir( newVec.XYZ() );
				}
			}

			// get the interpolate interval list
			List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList();

			// modify the tool vector
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {

				// get start and end index
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				InterpolateToolVec( nStartIndex, nEndIndex );
			}
		}

		gp_Vec GetVecFromAB( CAMPoint camPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( camPoint.ToolVec );
			}

			// get the x, y, z direction
			gp_Dir x = camPoint.CADPoint.TangentVec;
			gp_Dir z = camPoint.CADPoint.NormalVec_1st;
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

		List<Tuple<int, int>> GetInterpolateIntervalList()
		{
			// sort the modify data by index
			List<int> indexInOrder = m_CraftData.ToolVecModifyMap.Keys.ToList();
			indexInOrder.Sort();
			List<Tuple<int, int>> intervalList = new List<Tuple<int, int>>();
			if( IsClosed ) {

				// for closed path, the index is wrapped
				for( int i = 0; i < indexInOrder.Count; i++ ) {
					int nextIndex = ( i + 1 ) % indexInOrder.Count;
					intervalList.Add( new Tuple<int, int>( indexInOrder[ i ], indexInOrder[ nextIndex ] ) );
				}
			}
			else {
				for( int i = 0; i < indexInOrder.Count - 1; i++ ) {
					intervalList.Add( new Tuple<int, int>( indexInOrder[ i ], indexInOrder[ i + 1 ] ) );
				}
			}
			return intervalList;
		}

		void InterpolateToolVec( int nStartIndex, int nEndIndex )
		{
			// consider wrapped
			int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + m_CAMPointList.Count : nEndIndex;

			// get the start and end tool vector
			gp_Vec startVec = GetVecFromAB( m_CAMPointList[ nStartIndex ],
				m_CraftData.ToolVecModifyMap[ nStartIndex ].Item1 * Math.PI / 180,
				m_CraftData.ToolVecModifyMap[ nStartIndex ].Item2 * Math.PI / 180 );
			gp_Vec endVec = GetVecFromAB( m_CAMPointList[ nEndIndex ],
				m_CraftData.ToolVecModifyMap[ nEndIndex ].Item1 * Math.PI / 180,
				m_CraftData.ToolVecModifyMap[ nEndIndex ].Item2 * Math.PI / 180 );

			// get the total distance for interpolation parameter
			double totaldistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				totaldistance += m_CAMPointList[ i % m_CAMPointList.Count ].CADPoint.Point.SquareDistance( m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].CADPoint.Point );
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				double t = accumulatedDistance / totaldistance;
				accumulatedDistance += m_CAMPointList[ i % m_CAMPointList.Count ].CADPoint.Point.SquareDistance( m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].CADPoint.Point );
				gp_Quaternion q = new gp_Quaternion();
				slerp.Interpolate( t, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				m_CAMPointList[ i % m_CAMPointList.Count ].ToolVec = new gp_Dir( startVec.Transformed( trsf ) );
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

		#region Over cut

		void SetOverCut()
		{
			m_OverCutPointList.Clear();
			if( m_CAMPointList.Count == 0 || m_CraftData.OverCutLength == 0 || !IsClosed ) {
				return;
			}
			double dTotalOverCutLength = 0;

			// end point is the start of over cut
			m_OverCutPointList.Add( m_CAMPointList.Last().Clone() );
			for( int i = 0; i < m_CAMPointList.Count - 1; i++ ) {

				// get this edge distance
				double dDistance = m_CAMPointList[ i ].CADPoint.Point.Distance( m_CAMPointList[ i + 1 ].CADPoint.Point );
				if( dTotalOverCutLength + dDistance < m_CraftData.OverCutLength ) {

					// still within overcut length → take next point directly
					m_OverCutPointList.Add( m_CAMPointList[ i + 1 ].Clone() );
					dTotalOverCutLength += dDistance;
				}
				else {

					// need to stop inside this segment
					double dRemain = m_CraftData.OverCutLength - dTotalOverCutLength;
					if( dRemain <= MyApp.PRECISION_MIN_ERROR ) {
						return;
					}

					// compute new point along segment
					gp_Pnt overCutEndPoint = GetExactOverCutEndPoint( m_CAMPointList[ i ].CADPoint.Point, m_CAMPointList[ i + 1 ].CADPoint.Point, dRemain );

					// interpolate tool vector
					InterpolateToolAndTangentVecBetween2CAMPoint( m_CAMPointList[ i ], m_CAMPointList[ i + 1 ], overCutEndPoint, out gp_Dir endPointToolVec, out gp_Dir endPointTangentVec );

					// create new cam poiont
					CADPoint cadPoint = new CADPoint( overCutEndPoint, endPointToolVec, endPointToolVec, endPointTangentVec );
					CAMPoint camPoint = new CAMPoint( cadPoint, endPointToolVec );
					m_OverCutPointList.Add( camPoint );
					return;
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

		void InterpolateToolAndTangentVecBetween2CAMPoint( CAMPoint currentCAMPoint, CAMPoint nextCAMPoint, gp_Pnt point, out gp_Dir toolDir, out gp_Dir tangentDir )
		{
			toolDir = currentCAMPoint.ToolVec;
			tangentDir = currentCAMPoint.CADPoint.TangentVec;

			// get current and next tool vector
			gp_Vec currentVec = new gp_Vec( currentCAMPoint.ToolVec );
			gp_Vec nextVec = new gp_Vec( nextCAMPoint.ToolVec );

			// get current and next tangent vector
			gp_Vec currentTangentVec = new gp_Vec( currentCAMPoint.CADPoint.TangentVec );
			gp_Vec nextTangentVec = new gp_Vec( nextCAMPoint.CADPoint.TangentVec );

			// calculate new point percentage
			double dDistanceOfCAMPath2Point = currentCAMPoint.CADPoint.Point.Distance( nextCAMPoint.CADPoint.Point );
			double dDistanceBetweenCurrentPoint2NewPoint = currentCAMPoint.CADPoint.Point.Distance( point );

			// two point overlap
			if( dDistanceOfCAMPath2Point <= MyApp.PRECISION_MIN_ERROR ) {
				return;
			}
			double interpolatePercent = dDistanceBetweenCurrentPoint2NewPoint / dDistanceOfCAMPath2Point;

			// get new point dir
			toolDir = InterpolateVecBetween2Vec( currentVec, nextVec, interpolatePercent );
			tangentDir = InterpolateVecBetween2Vec( currentTangentVec, nextTangentVec, interpolatePercent );
		}

		#endregion

		#region Lead function

		void SetLeadIn()
		{
			m_LeadInCAMPointList.Clear();
			if( m_CAMPointList.Count == 0 ) {
				return;
			}
			switch( m_CraftData.LeadLineParam.LeadIn.Type ) {
				case LeadLineType.Line:
					m_LeadInCAMPointList = LeadHelper.BuildStraightLeadLine( m_CAMPointList.First(), true, m_CraftData.LeadLineParam.LeadIn.Length, m_CraftData.LeadLineParam.LeadIn.Angle, m_CraftData.LeadLineParam.IsChangeLeadDirection, m_CraftData.IsReverse );
					break;
				case LeadLineType.Arc:
					m_LeadInCAMPointList = LeadHelper.BuildArcLeadLine( m_CAMPointList.First(), true, m_CraftData.LeadLineParam.LeadIn.Length, m_CraftData.LeadLineParam.LeadIn.Angle, m_CraftData.LeadLineParam.IsChangeLeadDirection, m_CraftData.IsReverse, MyApp.PRECISION_DEFLECTION, MyApp.PRECISION_MAX_LENGTH );
					break;
				default:
					break;
			}
		}

		void SetLeadout()
		{
			m_LeadOutCAMPointList.Clear();
			if( m_CAMPointList.Count == 0 ) {
				return;
			}

			// with over cut means lead out first point is over cut last point
			CAMPoint leadOutStartPoint;
			if( m_CraftData.OverCutLength > 0 && m_OverCutPointList.Count > 0 ) {
				leadOutStartPoint = m_OverCutPointList.Last();
			}
			else {
				leadOutStartPoint = m_CAMPointList.Last();
			}
			switch( m_CraftData.LeadLineParam.LeadOut.Type ) {
				case LeadLineType.Line:
					m_LeadOutCAMPointList = LeadHelper.BuildStraightLeadLine( leadOutStartPoint, false, m_CraftData.LeadLineParam.LeadOut.Length, m_CraftData.LeadLineParam.LeadOut.Angle, m_CraftData.LeadLineParam.IsChangeLeadDirection, m_CraftData.IsReverse );
					break;
				case LeadLineType.Arc:
					m_LeadOutCAMPointList = LeadHelper.BuildArcLeadLine( leadOutStartPoint, false, m_CraftData.LeadLineParam.LeadOut.Length, m_CraftData.LeadLineParam.LeadOut.Angle, m_CraftData.LeadLineParam.IsChangeLeadDirection, m_CraftData.IsReverse, MyApp.PRECISION_DEFLECTION, MyApp.PRECISION_MAX_LENGTH );
					break;
				default:
					break;
			}
		}

		#endregion

		List<ICAMSegmentElement> m_CAMSegmentList = new List<ICAMSegmentElement>();
		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadOutCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();

		// they are sibling pointer, and change the declare order
		CraftData m_CraftData;
		List<ICADSegmentElement> m_CADSegmentList;
		public IReadOnlyList<ICADSegmentElement> CADSegmentList => m_CADSegmentList;

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
	}
}
