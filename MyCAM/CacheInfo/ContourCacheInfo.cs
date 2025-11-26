using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;
using static MyCAM.CacheInfo.ContourCacheInfo;

namespace MyCAM.CacheInfo
{
	internal class ContourCacheInfo : ICacheInfo
	{
		public ContourCacheInfo( string szID, List<ICADSegment> cadSegmentList, CraftData craftData, bool isClose )
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

		void BuildPathCAMSegment2()
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

		// segment be breaked
		internal class BrokenCAMSegment
		{
			public ICAMSegmentElement CAMSegment
			{
				get; set;
			}
			public bool IsStartSegment
			{
				get; set;
			}
			public bool IsControlSegment
			{
				get; set;
			}
			public SegmentPointIndex? OriginalControlPoint
			{
				get; set;
			}

			public BrokenCAMSegment( ICAMSegmentElement camSegment, bool isStart = false, bool isControl = false, SegmentPointIndex? originalCtrlPnt = null )
			{
				CAMSegment = camSegment;
				IsStartSegment = isStart;
				IsControlSegment = isControl;
				OriginalControlPoint = originalCtrlPnt;
			}
		}

		// 代表處理後的點資訊
		internal class CAMPointInfo
		{
			public CAMPoint2 Point
			{
				get; set;
			}

			public bool IsControlPoint
			{
				get; set;
			}

			// if is not control point, ABValues is null
			public Tuple<double, double> ABValues
			{
				get; set;
			}

			public double DistanceToNext
			{
				get; set;
			}

			// if this point is the end point of a segment, mapping to the start point of next segment
			public int? MappingToNextSegmentStart
			{
				get; set;
			}

			public SegmentPointIndex? OriginalIndex
			{
				get; set;
			}

			public CAMPointInfo( CAMPoint2 point )
			{
				Point = point;
				IsControlPoint = false;
				DistanceToNext = 0;
			}
		}

		void BuildPathCAMSegment()
		{
			try {
				m_IsCraftDataDirty = false;

				// Step 1: Collect all break points (start point + ctrl point)
				HashSet<SegmentPointIndex> breakPoints = CollectBreakPoints();

				// Step 2: Split CAD segments by break points
				List<BrokenCAMSegment> brokenSegmentList = SplitCADSegmentsByBreakPoints( breakPoints );

				// Step 3: reorder segments by start point
				List<BrokenCAMSegment> reorderedSegmentList = ReorderSegmentsByStartPoint( brokenSegmentList );

				// Step 4: Flatten CAM points to point bars
				List<CAMPointInfo> camPointInfoList = FlattenCAMPointsToBar( reorderedSegmentList );

				// Step 5: interpolate tool vector
				ApplyToolVectorInterpolation( camPointInfoList );

				// Step 6: 重新組織成最終的 CAM 段列表
				// List<ICAMSegmentElement> finalSegments = ReconstructCAMSegments( camPointInfoList, reorderedSegmentList );
				// Step 5: 直接提取，無需重建
				List<ICAMSegmentElement> finalSegments = ExtractFinalSegments( reorderedSegmentList );

				// Step 7: 設定結果
				SetResults( finalSegments, ExtractControlPointIndices( reorderedSegmentList ) );
			}
			catch( Exception ex ) {
				MyApp.Logger?.ShowOnLogPanel( $"建立CAM段失敗: {ex.Message}", MyApp.NoticeType.Error );
				SetResults( new List<ICAMSegmentElement>(), new Dictionary<int, SegmentPointIndex>() );
			}
		}

		List<ICAMSegmentElement> ExtractFinalSegments( List<BrokenCAMSegment> reorderedSegmentList )
		{
			List<ICAMSegmentElement> finalSegments = new List<ICAMSegmentElement>();

			foreach( var segment in reorderedSegmentList ) {
				// 設定修改標記
				if( segment.IsControlSegment ) {
					segment.CAMSegment.IsModify = true;
				}

				finalSegments.Add( segment.CAMSegment );
			}

			return finalSegments;
		}


		// use hashset to avoid duplicate break points
		HashSet<SegmentPointIndex> CollectBreakPoints()
		{
			HashSet<SegmentPointIndex> breakPoints = new HashSet<SegmentPointIndex>();

			// start point if need wiil be been adjusted to segment end
			SegmentPointIndex startPoint = AdjustPointToSegmentEnd( m_CraftData.StartPointIndex );
			if( startPoint.SegIdx >= 0 && startPoint.SegIdx < CADSegmentList.Count ) {
				breakPoints.Add( startPoint );
			}

			// add ctrl pnt
			foreach( var controlPoint in m_CraftData.ToolVecModifyMap.Keys ) {
				SegmentPointIndex adjustedPoint = AdjustPointToSegmentEnd( controlPoint );
				if( adjustedPoint.SegIdx >= 0 && adjustedPoint.SegIdx < CADSegmentList.Count ) {
					breakPoints.Add( adjustedPoint );
				}
			}
			return breakPoints;
		}

		SegmentPointIndex AdjustPointToSegmentEnd( SegmentPointIndex originalPoint )
		{
			if( originalPoint.SegIdx < 0 || originalPoint.SegIdx >= CADSegmentList.Count ) {
				return originalPoint;
			}

			ICADSegment segment = CADSegmentList[ originalPoint.SegIdx ];

			// if point is at the start of segment, move to previous segment's end
			if( originalPoint.PntIdx == 0 ) {
				int prevSegIdx = ( originalPoint.SegIdx - 1 + CADSegmentList.Count ) % CADSegmentList.Count;
				ICADSegment prevSegment = CADSegmentList[ prevSegIdx ];
				return new SegmentPointIndex( prevSegIdx, prevSegment.PointList.Count - 1 );
			}

			// if point is at the end of segment, return it
			if( originalPoint.PntIdx == segment.PointList.Count - 1 ) {
				return originalPoint;
			}

			// if point is in the middle of segment, keep it unchanged (this will become a valid break point)
			return originalPoint;
		}

		bool IsValidBreakPoint( SegmentPointIndex point )
		{
			if( point.SegIdx < 0 || point.SegIdx >= CADSegmentList.Count )
				return false;

			var segment = CADSegmentList[ point.SegIdx ];
			if( point.PntIdx < 0 || point.PntIdx >= segment.PointList.Count )
				return false;

			return true;
		}

		List<BrokenCAMSegment> SplitCADSegmentsByBreakPoints( HashSet<SegmentPointIndex> breakPoints )
		{
			List<BrokenCAMSegment> result = new List<BrokenCAMSegment>();

			// protection
			if (breakPoints == null ) {
				breakPoints = new HashSet<SegmentPointIndex>();
			}
			for( int segIdx = 0; segIdx < CADSegmentList.Count; segIdx++ ) {
				ICADSegment cadSegment = CADSegmentList[ segIdx ];

				// find break points for this segment
				List<int> segmentBreakPoints = breakPoints
					.Where( segment => segment.SegIdx == segIdx )
					.Select( breakPnt => breakPnt.PntIdx )
					.OrderBy( idx => idx )
					.ToList();

				if( segmentBreakPoints.Count == 0 ) {

					// no break points, build directly
					if( CAMSegmentBuilder.BuildCAMSegment( cadSegment, out ICAMSegmentElement camSegment ) ) {
						result.Add( new BrokenCAMSegment( camSegment ) );
					}
				}
				else {
					// with break points, need to split
					List<BrokenCAMSegment> splitSegments = SplitSingleCADSegment( cadSegment, segmentBreakPoints, segIdx, breakPoints );
					result.AddRange( splitSegments );
				}
			}
			return result;
		}

		List<BrokenCAMSegment> SplitSingleCADSegment( ICADSegment cadSegment, List<int> breadPntIdx, int thisSegIdx, HashSet<SegmentPointIndex> allBreakPoints )
		{
			List<BrokenCAMSegment> result = new List<BrokenCAMSegment>();
			List<CADPoint> pointList = cadSegment.PointList;

			int startIdx = 0;

			foreach( int breakIdx in breadPntIdx ) {
				if( breakIdx <= startIdx ) {
					continue;
				}

				// build sub segment: [startIdx, breakIdx]
				List<CADPoint> subPoints = pointList.GetRange( startIdx, breakIdx - startIdx + 1 );
				if( subPoints.Count >= 2 ) {

					// calculate sub segment length
					double subLength = CalculateSubSegmentLength( cadSegment, startIdx, breakIdx );
					double perLength = subLength / ( subPoints.Count - 1 );

					if( CAMSegmentBuilder.BuildCAMSegmentByCADPoint( subPoints, cadSegment.SegmentType, subLength, perLength, cadSegment.PerChordLength, out ICAMSegmentElement subCAMSegment ) ) {

						// check if this break point is start point or control point
						SegmentPointIndex breakPoint = new SegmentPointIndex( thisSegIdx, breakIdx );
						bool isStart = m_CraftData.StartPointIndex.Equals( breakPoint );
						bool isControl = m_CraftData.ToolVecModifyMap.ContainsKey( breakPoint );

						result.Add( new BrokenCAMSegment( subCAMSegment, isStart, isControl, breakPoint ) );
					}
				}
				startIdx = breakIdx;
			}

			// last segment[startIdx, end]
			if( startIdx < pointList.Count - 1 ) {
				List<CADPoint> lastSubPoints = pointList.GetRange( startIdx, pointList.Count - startIdx );
				if( lastSubPoints.Count >= 2 ) {
					double subLength = CalculateSubSegmentLength( cadSegment, startIdx, pointList.Count - 1 );
					double perLength = subLength / ( lastSubPoints.Count - 1 );

					if( CAMSegmentBuilder.BuildCAMSegmentByCADPoint( lastSubPoints, cadSegment.SegmentType, subLength, perLength, cadSegment.PerChordLength, out ICAMSegmentElement lastCAMSegment ) ) {
						result.Add( new BrokenCAMSegment( lastCAMSegment ) );
					}
				}
			}

			return result;
		}

		double CalculateSubSegmentLength( ICADSegment originalSegment, int startIdx, int endIdx )
		{
			if( originalSegment == null || startIdx < 0 || endIdx >= originalSegment.PointList.Count ||
				startIdx >= endIdx || originalSegment.PointList.Count < 1 ) {
				return 0;
			}
			// calculate length ratio
			double dRatio = (double)( endIdx - startIdx ) / ( originalSegment.PointList.Count - 1 );
			return originalSegment.SegmentLength * dRatio;
		}

		List<BrokenCAMSegment> ReorderSegmentsByStartPoint( List<BrokenCAMSegment> brokenSegmentList )
		{
			if( brokenSegmentList == null || brokenSegmentList.Count == 0 )
				return new List<BrokenCAMSegment>();

			// find start segment index
			int startSegmentIndex = -1;
			for( int i = 0; i < brokenSegmentList.Count; i++ ) {
				if( brokenSegmentList[ i ].IsStartSegment ) {
					startSegmentIndex = i;
					break;
				}
			}

			// did not find start segment, keep original order
			if( startSegmentIndex == -1 ) {
				return new List<BrokenCAMSegment>( brokenSegmentList );
			}

			// reorder: segments after start segment + segments before start segment + start segment
			List<BrokenCAMSegment> reorderedList = new List<BrokenCAMSegment>();
			
			// add segments after start segment
			reorderedList.AddRange( brokenSegmentList.Skip( startSegmentIndex + 1 ) );

			// add segments before start segment 
			reorderedList.AddRange( brokenSegmentList.Take( startSegmentIndex ) );

			// add start segment at last
			reorderedList.Add( brokenSegmentList[ startSegmentIndex ] );
			return reorderedList;
		}

		List<CAMPointInfo> FlattenCAMPointsToBar( List<BrokenCAMSegment> brokenSegmentList )
		{
			List<CAMPointInfo> camInfoList = new List<CAMPointInfo>();

			if( brokenSegmentList == null || brokenSegmentList.Count == 0 ) {
				return camInfoList;
			}
			for( int segIdx = 0; segIdx < brokenSegmentList.Count; segIdx++ ) {
				BrokenCAMSegment segment = brokenSegmentList[ segIdx ];
				List<CAMPoint2> camPoints = segment.CAMSegment.CAMPointList;

				for( int pntIdx = 0; pntIdx < camPoints.Count; pntIdx++ ) {
					CAMPointInfo camPointInfo = new CAMPointInfo( camPoints[ pntIdx ]);

					// this is ctrl pnt (this segment is contrl segment + this point is last point + this ctrl pnt can map back to ori pnt)
					if( segment.IsControlSegment && pntIdx == camPoints.Count - 1 && segment.OriginalControlPoint.HasValue ) {
						camPointInfo.IsControlPoint = true;
						camPointInfo.OriginalIndex = segment.OriginalControlPoint;

						if( m_CraftData.ToolVecModifyMap.TryGetValue( segment.OriginalControlPoint.Value, out var abValues ) ) {
							camPointInfo.ABValues = abValues;
						}
					}

					// this pnt is not this segmnt last pnt
					if( pntIdx < camPoints.Count - 1 ) {
						camPointInfo.DistanceToNext = camPoints[ pntIdx ].Point.Distance( camPoints[ pntIdx + 1 ].Point );
					}

					// this pnt is the last pnt of this segment
					else if( segIdx < brokenSegmentList.Count - 1 ) {
						
						// cal this segment last pnt to next segment start pnt
						var nextSegmentFirstPoint = brokenSegmentList[ segIdx + 1 ].CAMSegment.CAMPointList.First();
						camPointInfo.DistanceToNext = camPoints[ pntIdx ].Point.Distance( nextSegmentFirstPoint.Point );
						camPointInfo.MappingToNextSegmentStart = 0; // mapping 到下一段的第一個點
					}

					camInfoList.Add( camPointInfo );
				}
			}
			return camInfoList;
		}

		void ApplyToolVectorInterpolation( List<CAMPointInfo> camPointInfoList )
		{
			if( camPointInfoList == null || camPointInfoList.Count == 0 )
				return;

			// find all ctrl pnt
			List<int> ctrlPntIdx = new List<int>();
			for( int i = 0; i < camPointInfoList.Count; i++ ) {
				if( camPointInfoList[ i ].IsControlPoint ) {
					ctrlPntIdx.Add( i );
				}
			}

			// do not have ctrl pnt
			if( ctrlPntIdx.Count == 0 ) {
				ApplyGlobalToolVector( camPointInfoList );
				return;
			}

			// handle every region
			for( int i = 0; i < ctrlPntIdx.Count; i++ ) {
				int startCtrlIdx = ctrlPntIdx[ i ];
				int endCtrlIdx = ctrlPntIdx[ ( i + 1 ) % ctrlPntIdx.Count ];
				ApplyInterpolationInRegion( camPointInfoList, startCtrlIdx, endCtrlIdx );
			}
		}

		void ApplyInterpolationInRegion( List<CAMPointInfo> pointInfoList, int startCtrlIdx, int endCtrlIdx )
		{
			// cal start/end tool vec	
			gp_Vec startToolVec = CalCtrlPntToolVec( pointInfoList[ startCtrlIdx ] );
			gp_Vec endToolVec = CalCtrlPntToolVec( pointInfoList[ endCtrlIdx ] );

			if( startToolVec == null || endToolVec == null )
				return;

			// call length of this region
			double totalLength = CalRegionLength( pointInfoList, startCtrlIdx, endCtrlIdx );

			if( totalLength <= 0 ) {
				return;
			}

			// do interpolation
			double currentLength = 0;
			int currentIdx = startCtrlIdx;

			while( true ) {

				// 執行四元數插值
				gp_Dir interpolatedToolVec = GetInterpolateToolVecByLength( startToolVec, endToolVec, currentLength, totalLength );

				// 設定工具向量
				pointInfoList[ currentIdx ].Point.ToolVec = interpolatedToolVec;

				// 如果到達終點，跳出
				if( currentIdx == endCtrlIdx ) {
					break;
				}

				// move to next point
				currentLength += pointInfoList[ currentIdx ].DistanceToNext;
				currentIdx = ( currentIdx + 1 ) % pointInfoList.Count;
			}
		}

		gp_Vec CalCtrlPntToolVec( CAMPointInfo controlBar )
		{
			if( !controlBar.IsControlPoint || controlBar.ABValues == null )
				return null;

			var abValues = controlBar.ABValues;
			gp_Dir oriNormal = m_CraftData.IsToolVecReverse ?
				controlBar.Point.NormalVec_1st.Reversed() :
				controlBar.Point.NormalVec_1st;

			return GetVecFromAB( oriNormal, controlBar.Point.TangentVec,
				abValues.Item1 * Math.PI / 180, abValues.Item2 * Math.PI / 180 );
		}

		double CalRegionLength( List<CAMPointInfo> pointBars, int startIdx, int endIdx )
		{
			if(pointBars == null || pointBars.Count == 0 ) {
				return 0;
			}
			double length = 0;
			int currentIdx = startIdx;

			while( currentIdx != endIdx ) {
				length += pointBars[ currentIdx ].DistanceToNext;
				currentIdx = ( currentIdx + 1 ) % pointBars.Count;
			}
			return length;
		}

		void ApplyGlobalToolVector( List<CAMPointInfo> camPointList )
		{
			if( !m_CraftData.IsToolVecReverse )
				return;

			foreach( var point in camPointList ) {
				point.Point.ToolVec = point.Point.ToolVec.Reversed();
			}
		}

		List<ICAMSegmentElement> ReconstructCAMSegments( List<CAMPointInfo> pointBars, List<BrokenCAMSegment> originalWrappers )
		{
			List<ICAMSegmentElement> result = new List<ICAMSegmentElement>();

			if( pointBars == null || originalWrappers == null )
				return result;

			int pointIndex = 0;

			foreach( var wrapper in originalWrappers ) {
				int segmentPointCount = wrapper.CAMSegment.CAMPointList.Count;

				if( pointIndex + segmentPointCount > pointBars.Count )
					break;

				// 提取這個段的點
				List<CAMPoint2> segmentPoints = new List<CAMPoint2>();
				for( int i = 0; i < segmentPointCount; i++ ) {
					segmentPoints.Add( pointBars[ pointIndex + i ].Point );
				}

				// 重建段
				if( CAMSegmentBuilder.BuildCAMSegmentByCAMPoint(
					segmentPoints,
					wrapper.CAMSegment.ContourType,
					wrapper.CAMSegment.TotalLength,
					wrapper.CAMSegment.PerArcLength,
					wrapper.CAMSegment.PerChordLength,
					out ICAMSegmentElement reconstructedSegment ) ) {
					// 設定修改標記
					if( wrapper.IsControlSegment ) {
						reconstructedSegment.IsModify = true;
					}

					result.Add( reconstructedSegment );
				}

				pointIndex += segmentPointCount;
			}

			return result;
		}

		Dictionary<int, SegmentPointIndex> ExtractControlPointIndices( List<BrokenCAMSegment> segmentWrappers )
		{
			Dictionary<int, SegmentPointIndex> result = new Dictionary<int, SegmentPointIndex>();

			for( int i = 0; i < segmentWrappers.Count; i++ ) {
				BrokenCAMSegment segment = segmentWrappers[ i ];
				if( segment.IsControlSegment && segment.OriginalControlPoint.HasValue ) {
					result[ i ] = segment.OriginalControlPoint.Value;
				}
			}

			return result;
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
			IReadOnlyList<ICADSegment> cadSegmentList = CADSegmentList;
			for( int i = 0; i < cadSegmentList.Count; i++ ) {
				int index = ( startPoint.SegIdx + i ) % cadSegmentList.Count;
				bool isBuildSuccess = CAMSegmentBuilder.BuildCAMSegment( cadSegmentList[ index ], out ICAMSegmentElement camSegment );
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

			bool isFirstBuildSuccess = CAMSegmentBuilder.BuildCAMSegmentByCAMPoint( pntListAfterTargetIndex, camSegment.ContourType, camSegment.PerArcLength * ( pntListAfterTargetIndex.Count - 1 ), camSegment.PerArcLength, camSegment.PerChordLength, out ICAMSegmentElement segmentAfterStartPoint );
			bool isLastBuildSuccess = CAMSegmentBuilder.BuildCAMSegmentByCAMPoint( pntListBeforeTargetIndex, camSegment.ContourType, camSegment.PerArcLength * ( pntListBeforeTargetIndex.Count - 1 ), camSegment.PerArcLength, camSegment.PerChordLength, out ICAMSegmentElement segmentBeforeStartPoint );
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
					bool isBuildSuccess = CAMSegmentBuilder.BuildCAMSegmentByCAMPoint( splitedCAMPointList[ k ], orderedCADSegmentList[ segmentIndex ].ContourType, orderedCADSegmentList[ segmentIndex ].PerArcLength * splitedCAMPointList[ k ].Count - 1, orderedCADSegmentList[ segmentIndex ].PerArcLength, orderedCADSegmentList[ segmentIndex ].PerChordLength, out ICAMSegmentElement newCAMSegment );
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
			if( m_CraftData.OverCutLength > 0 ) {
				List<ICAMSegmentElement> overCutSement = OverCutSegmentBuilder.BuildOverCutSegment( m_CAMSegmentList, m_CraftData.OverCutLength, m_CraftData.IsReverse );
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
			if( m_CraftData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
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
		List<ICADSegment> m_CADSegmentList;
		public IReadOnlyList<ICADSegment> CADSegmentList => m_CADSegmentList;

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
	}
}
