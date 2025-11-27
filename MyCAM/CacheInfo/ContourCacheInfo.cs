using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

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

			// unclose path start point may as also a control point
			public bool IsFirstPntIsStartPnt
			{
				get; set;
			}

			public bool IsFirstPntIsCtrlPnt

			{
				get; set;
			}

			public SegmentPointIndex? OriginalControlPoint
			{
				get; set;
			}

			public BrokenCAMSegment( ICAMSegmentElement camSegment, bool isStart = false, bool isControl = false, SegmentPointIndex? originalCtrlPnt = null, bool isFirstPntIsStartPnt = false, bool isFirstPntIsCtrlPnt = false )
			{
				CAMSegment = camSegment;
				IsStartSegment = isStart;
				IsControlSegment = isControl;
				OriginalControlPoint = originalCtrlPnt;
				IsFirstPntIsStartPnt = isFirstPntIsStartPnt;
				IsFirstPntIsCtrlPnt = isFirstPntIsCtrlPnt;
			}
		}

		// 代表處理後的點資訊
		internal class CAMPointInfo
		{
			public CAMPoint2 Point
			{
				get; set;
			}

			public CAMPoint2 Point2
			{
				get; set;
			}

			public gp_Dir ToolVec
			{
				get
				{
					return m_ToolVec;
				}
				set
				{
					m_ToolVec = value;
					setToolVec( value );
				}

			}

			public bool IsCtrlPnt
			{
				get; set;
			} = false;

			public bool IsStartPnt
			{
				get; set;
			} = false;

			// if is not control point, ABValues is null
			public Tuple<double, double> ABValues
			{
				get; set;
			}

			public double DistanceToNext
			{
				get; set;
			}

			public CAMPointInfo( CAMPoint2 point )
			{
				Point = point;
				IsCtrlPnt = false;
				IsStartPnt = false;
				DistanceToNext = 0;
			}

			void setToolVec( gp_Dir dir )
			{
				if( dir == null ) {
					return;
				}
				if( Point != null ) {
					Point.ToolVec = dir;
				}
				if( Point2 != null ) {
					Point2.ToolVec = dir;
				}
			}
			gp_Dir m_ToolVec;
		}


		// use hashset to avoid duplicate break points



		void BuildPathCAMSegment()
		{
			List<CAMPointInfo> pathCAMInfo = FlattenCADSegmentsToCAMPointInfo();
			ApplyToolVectorInterpolation( pathCAMInfo );


		}

		void InterpolateToolVec( int nStartIndex, int nEndIndex, List<CAMPointInfo> pathCAMInfo )
		{
			// consider wrapped
			int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + pathCAMInfo.Count : nEndIndex;

			// get the start and end tool vector
			gp_Vec startVec = GetVecFromAB( pathCAMInfo[ nStartIndex ].Point.NormalVec_1st,
				pathCAMInfo[ nStartIndex ].Point.TangentVec,
				pathCAMInfo[ nStartIndex ].ABValues.Item1 * Math.PI / 180,
				pathCAMInfo[ nStartIndex ].ABValues.Item2 * Math.PI / 180 );
			gp_Vec endVec = GetVecFromAB( pathCAMInfo[ nEndIndex ].Point.NormalVec_1st,
				pathCAMInfo[ nEndIndex ].Point.TangentVec,
				pathCAMInfo[ nEndIndex ].ABValues.Item1 * Math.PI / 180,
				pathCAMInfo[ nEndIndex ].ABValues.Item2 * Math.PI / 180 );

			// get the total distance for interpolation parameter
			double totaldistance = 0;

			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				totaldistance += pathCAMInfo[ i % pathCAMInfo.Count ].DistanceToNext;
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				double t = accumulatedDistance / totaldistance;
				accumulatedDistance += pathCAMInfo[ i % pathCAMInfo.Count ].DistanceToNext;
				gp_Quaternion q = new gp_Quaternion();
				slerp.Interpolate( t, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				pathCAMInfo[ i % pathCAMInfo.Count ].ToolVec = new gp_Dir( startVec.Transformed( trsf ) );
			}
		}

		List<Tuple<int, int>> GetInterpolateIntervalList( List<int> ctrlIndex )
		{

			List<Tuple<int, int>> intervalList = new List<Tuple<int, int>>();
			if( ctrlIndex.Count < 2 ) {
				return intervalList;
			}
			if( IsClosed ) {

				// for closed path, the index is wrapped
				for( int i = 0; i < ctrlIndex.Count; i++ ) {
					int nextIndex = ( i + 1 ) % ctrlIndex.Count;
					intervalList.Add( new Tuple<int, int>( ctrlIndex[ i ], ctrlIndex[ nextIndex ] ) );
				}
			}
			else {
				for( int i = 0; i < ctrlIndex.Count - 1; i++ ) {
					intervalList.Add( new Tuple<int, int>( ctrlIndex[ i ], ctrlIndex[ i + 1 ] ) );
				}
			}
			return intervalList;
		}


		public List<CAMPointInfo> FlattenCADSegmentsToCAMPointInfo()
		{
			List<CAMPointInfo> result = new List<CAMPointInfo>();

			if( CADSegmentList == null || CADSegmentList.Count == 0 ) {
				return result;
			}

			SegmentPointIndex startPointIndex = m_CraftData.StartPointIndex;
			Dictionary<SegmentPointIndex, Tuple<double, double>> toolVecModifyMap = m_CraftData.ToolVecModifyMap;

			for( int segIdx = 0; segIdx < CADSegmentList.Count; segIdx++ ) {
				ICADSegment cadSegment = CADSegmentList[ segIdx ];
				List<CADPoint> pointList = cadSegment.PointList;

				for( int pntIdx = 0; pntIdx < pointList.Count; pntIdx++ ) {
					CADPoint cadPoint = pointList[ pntIdx ];
					SegmentPointIndex currentPointIndex = new SegmentPointIndex( segIdx, pntIdx );

					// 轉換為CAMPoint2
					CAMPoint2 camPoint = ConvertCADPointToCAMPoint2( cadPoint );

					// 創建CAMPointInfo
					CAMPointInfo CurrentInfo = new CAMPointInfo( camPoint );

					// 1. 檢查是否為segment的第一個點且前面還有segment
					if( pntIdx == 0 && result.Count > 0 ) {
						// 取得上一段segment的最後一個點，設置為Point2
						CAMPointInfo lastPointInfo = result[ result.Count - 1 ];
						lastPointInfo.Point2 = camPoint;
						CurrentInfo = lastPointInfo;
					}

					// 2. 檢查是否為起點
					bool isStartPoint = startPointIndex.Equals( currentPointIndex );
					if( isStartPoint ) {
						CurrentInfo.IsStartPnt = true;
					}

					// 3. 檢查是否為控制點
					bool isControlPoint = toolVecModifyMap.ContainsKey( currentPointIndex );
					if( isControlPoint ) {
						CurrentInfo.IsCtrlPnt = true;
						CurrentInfo.ABValues = toolVecModifyMap[ currentPointIndex ];
					}

					// 4. 計算到下一個點的距離
					CurrentInfo.DistanceToNext = CalDistanceToNext( segIdx, pntIdx, CADSegmentList );

					result.Add( CurrentInfo );
				}
			}

			// 5. 封閉路徑處理：移除最後一個點（因為它與第一個點重複）
			if( IsClosed && result.Count > 1 ) {

				// 移除最後一個重複點
				result.RemoveAt( result.Count - 1 );
			}

			return result;
		}

		double CalDistanceToNext( int segIdx, int pntIdx, IReadOnlyList<ICADSegment> cadSegment )
		{
			CADPoint currentPnt = cadSegment[ segIdx ].PointList[ pntIdx ];

			// check is last point in segment
			if( pntIdx == cadSegment[ segIdx ].PointList.Count - 1 ) {

				// is last point in segment
				if( segIdx < cadSegment.Count - 1 ) {
					// has next segment, calculate to next segment first point
					ICADSegment nextSegment = cadSegment[ segIdx + 1 ];
					if( nextSegment.PointList != null && nextSegment.PointList.Count > 0 ) {
						CADPoint nextPnt = nextSegment.StartPoint;
						double distance = currentPnt.Point.Distance( nextPnt.Point );

						// due to segment point overlap, this distance should be close to 0
						return distance;
					}
				}

				else if( IsClosed ) {
					// closed path and is last segment last point, calculate to first segment first point
					if( cadSegment.Count > 0 && cadSegment[ 0 ].PointList.Count > 0 ) {
						CADPoint firstPnt = cadSegment[ 0 ].StartPoint;
						double distance = currentPnt.Point.Distance( firstPnt.Point );
						return distance;
					}
				}
				// unclose path last point, distance is 0
				return 0.0;
			}
			else {
				// point in segment, calculate to next point in same segment
				CADPoint nextPnt = cadSegment[ segIdx ].PointList[ pntIdx + 1 ];
				return currentPnt.Point.Distance( nextPnt.Point );
			}
		}


		private CAMPoint2 ConvertCADPointToCAMPoint2( CADPoint cadPoint )
		{
			// 初始工具向量設為法向量，後續會根據插值計算調整
			gp_Dir initialToolVec = cadPoint.NormalVec_1st;

			return new CAMPoint2(
				cadPoint.Point,
				cadPoint.NormalVec_1st,
				cadPoint.NormalVec_2nd,
				cadPoint.TangentVec,
				initialToolVec
			);
		}



		void ApplyToolVectorInterpolation( List<CAMPointInfo> camPointInfoList )
		{
			if( camPointInfoList == null || camPointInfoList.Count == 0 )
				return;

			// find all ctrl pnt
			List<int> ctrlPntIdx = new List<int>();
			for( int i = 0; i < camPointInfoList.Count; i++ ) {
				if( camPointInfoList[ i ].IsCtrlPnt ) {
					ctrlPntIdx.Add( i );
				}
			}

			// do not have ctrl pnt
			if( ctrlPntIdx.Count == 0 ) {
				ApplyGlobalToolVector( camPointInfoList );
				return;
			}
			if( ctrlPntIdx.Count == 1 ) {
				ApplySpecifiedVec( camPointInfoList, ctrlPntIdx[ 0 ] );
				return;
			}

			List<Tuple<int, int>>  interpolateIntervalList = GetInterpolateIntervalList( ctrlPntIdx );

			// modify the tool vector
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {

				// get start and end index
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				InterpolateToolVec( nStartIndex, nEndIndex, camPointInfoList );
			}
		}

		void ApplySpecifiedVec( List<CAMPointInfo> pointInfoList, int nSpecifiedIdx )
		{
			gp_Vec SpecifiedVec = CalCtrlPntToolVec( pointInfoList[ nSpecifiedIdx ] );

			if( SpecifiedVec == null ) {
				return;
			}
			foreach( var pointInfo in pointInfoList ) {
				pointInfo.ToolVec = new gp_Dir( SpecifiedVec );
			}
		}

		

		gp_Vec CalCtrlPntToolVec( CAMPointInfo controlBar )
		{
			if( !controlBar.IsCtrlPnt || controlBar.ABValues == null )
				return null;

			var abValues = controlBar.ABValues;
			gp_Dir oriNormal = m_CraftData.IsToolVecReverse ?
				controlBar.Point.NormalVec_1st.Reversed() :
				controlBar.Point.NormalVec_1st;

			return GetVecFromAB( oriNormal, controlBar.Point.TangentVec,
				abValues.Item1 * Math.PI / 180, abValues.Item2 * Math.PI / 180 );
		}

		void ApplyGlobalToolVector( List<CAMPointInfo> camPointList )
		{
			if( !m_CraftData.IsToolVecReverse )
				return;

			foreach( var point in camPointList ) {
				point.ToolVec = point.ToolVec.Reversed();
			}
		}

		


		void BuildCAMFeatureSegment()
		{
			SetOverCut();
			SetLead();
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
