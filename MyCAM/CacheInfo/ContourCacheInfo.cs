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

		public List<ICAMSegment> CAMSegmentList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildPathCAMSegment();
					BuildCAMFeatureSegment();
				}
				return m_CAMSegmentList;
			}
		}

		public List<int> CtrlToolSegIdxList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildPathCAMSegment();
					BuildCAMFeatureSegment();
				}
				return m_CtrlToolSegIdxList;
			}
		}

		public List<ICAMSegment> LeadInSegment
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

		public List<ICAMSegment> LeadOutSegment
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

		public List<ICAMSegment> OverCutSegment
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
			ICAMSegment camSegmentConnectWithStartPnt = null;
			if( m_CraftData.LeadLineParam.LeadIn.Type != LeadLineType.None ) {
				camSegmentConnectWithStartPnt = m_LeadInSegmentList.First();
			}
			else {
				if( m_CAMSegmentList == null || m_CAMSegmentList.Count == 0 ) {
					return null;
				}
				camSegmentConnectWithStartPnt = m_CAMSegmentList.First();
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
			ICAMSegment camSegmentConnectWithEndPnt = null;
			if( m_CraftData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
				camSegmentConnectWithEndPnt = m_LeadOutSegmentList.Last();
			}
			else {
				// with overcut
				if( m_CraftData.OverCutLength > 0 && m_OverCutSegmentList.Count > 0 ) {
					camSegmentConnectWithEndPnt = m_OverCutSegmentList.Last();
				}
				else {
					if( m_CAMSegmentList == null || m_CAMSegmentList.Count == 0 ) {
						return null;
					}
					camSegmentConnectWithEndPnt = m_CAMSegmentList.Last();
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

			// Step 1: Collect all cad point
			List<CAMPointInfo> pathCAMInfo = FlattenCADSegmentsToCAMPointInfo();

			// Step 2: Do interpolation
			ApplyToolVectorInterpolation( pathCAMInfo, m_CraftData.IsReverse );

			// Step 3: use caminfo to build cam segment
			bool isBuildDone = ReBuildCAMSegment( pathCAMInfo, out List<ICAMSegment> PathCAMSegList, out List<int> CtrlSegIdx );
			if( isBuildDone == false ) {
				return;
			}
			m_CAMSegmentList = PathCAMSegList;
			m_CtrlToolSegIdxList = CtrlSegIdx;
		}

		bool ReBuildCAMSegment( List<CAMPointInfo> pathCAMInfo, out List<ICAMSegment> PathCAMSegList, out List<int> CtrlSegIdx )
		{
			CtrlSegIdx = new List<int>();
			int currentSegmentIdx = 0;
			PathCAMSegList = new List<ICAMSegment>();
			bool isBuildDone = ReBuildCAMSegmentFromStartPnt( pathCAMInfo, m_CADSegmentList, ref CtrlSegIdx, ref currentSegmentIdx, out List<ICAMSegment> camSegmentList );
			if( isBuildDone && camSegmentList != null ) {
				PathCAMSegList.AddRange( camSegmentList );
			}
			else {
				return false;
			}
			bool isBuildPreDone = ReBuildCAMSegBeforStartPnt( pathCAMInfo, m_CADSegmentList, ref CtrlSegIdx, ref currentSegmentIdx, out List<ICAMSegment> preCamSegmentList );
			if( isBuildPreDone && preCamSegmentList != null ) {
				PathCAMSegList.AddRange( preCamSegmentList );
			}
			else {
				return false;
			}
			return true;
		}

		void InterpolateToolVec( int nStartIndex, int nEndIndex, List<CAMPointInfo> pathCAMInfo, bool isReverse )
		{
			// consider wrapped
			int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + pathCAMInfo.Count : nEndIndex;
			if( pathCAMInfo[ nStartIndex ].Point2 == null || pathCAMInfo[ nEndIndex ].Point2 == null ) {
				return;
			}

			// to keep use same point to interpolate
			gp_Dir startPntTanVec = isReverse ? pathCAMInfo[ nStartIndex ].Point2.TangentVec : pathCAMInfo[ nStartIndex ].Point.TangentVec;
			gp_Dir endPntTanVec = isReverse ? pathCAMInfo[ nEndIndex ].Point2.TangentVec : pathCAMInfo[ nEndIndex ].Point.TangentVec;

			// get the start and end tool vector
			gp_Vec startVec = GetVecFromAB( pathCAMInfo[ nStartIndex ].Point.NormalVec_1st,
				startPntTanVec,
				pathCAMInfo[ nStartIndex ].ABValues.Item1 * Math.PI / 180,
				pathCAMInfo[ nStartIndex ].ABValues.Item2 * Math.PI / 180 );
			gp_Vec endVec = GetVecFromAB( pathCAMInfo[ nEndIndex ].Point.NormalVec_1st,
				endPntTanVec,
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

		// double cam point case 
		// 1. frist segment first pnt
		// 2. start pnt
		// 3. overlap area pnt
		// 4. ctrl pnt
		List<CAMPointInfo> FlattenCADSegmentsToCAMPointInfo()
		{
			List<CAMPointInfo> result = new List<CAMPointInfo>();
			if( m_CADSegmentList == null || m_CADSegmentList.Count == 0 ) {
				return result;
			}
			SegmentPointIndex startPointIndex = m_CraftData.StartPointIndex;
			Dictionary<SegmentPointIndex, Tuple<double, double>> toolVecModifyMap = m_CraftData.ToolVecModifyMap;

			for( int segIdx = 0; segIdx < m_CADSegmentList.Count; segIdx++ ) {
				ICADSegment cadSegment = m_CADSegmentList[ segIdx ];
				List<CADPoint> pointList = cadSegment.PointList;
				for( int pntIdx = 0; pntIdx < pointList.Count; pntIdx++ ) {
					CADPoint cadPoint = pointList[ pntIdx ];
					SegmentPointIndex currentPointIndex = new SegmentPointIndex( segIdx, pntIdx );

					// conver this cad point
					CAMPoint2 camPoint = ConvertCADPointToCAMPoint2( cadPoint, m_CraftData.IsToolVecReverse );
					bool isFistSegFirstPnt = ( pntIdx == 0 && segIdx == 0 );
					bool isOverlapPnt = ( pntIdx == 0 && result.Count > 0 );

					// case A : fist seg first pnt
					if( isFistSegFirstPnt ) {

						// Step 1: create new CAMPointInfo
						CAMPointInfo currentInfo = new CAMPointInfo( camPoint );

						// first seg first pnt will be break pnt (this point2 is for next segment start point)
						currentInfo.Point2 = camPoint.Clone();

						// Step 2: check property
						bool isStartPoint = startPointIndex.Equals( currentPointIndex );
						if( isStartPoint ) {
							currentInfo.IsStartPnt = true;
						}
						bool isControlPoint = toolVecModifyMap.ContainsKey( currentPointIndex );
						if( isControlPoint ) {
							currentInfo.IsCtrlPnt = true;
							currentInfo.ABValues = toolVecModifyMap[ currentPointIndex ];
						}
						currentInfo.DistanceToNext = cadSegment.SubChordLength;
						result.Add( currentInfo );
					}

					// case B : overlap point
					else if( isOverlapPnt ) {

						// Step 1: get and set at last pointinfo , do not need to create new one
						CAMPointInfo lastPointInfo = result[ result.Count - 1 ];
						lastPointInfo.Point2 = camPoint;

						// Step 2: check property and set back to last point info
						bool isStartPoint = startPointIndex.Equals( currentPointIndex );
						bool isControlPoint = toolVecModifyMap.ContainsKey( currentPointIndex );
						if( isStartPoint ) {
							lastPointInfo.IsStartPnt = true;
						}
						if( isControlPoint ) {
							lastPointInfo.IsCtrlPnt = true;
							lastPointInfo.ABValues = toolVecModifyMap[ currentPointIndex ];
						}
						// Step 3: calculate distance to next point
						lastPointInfo.DistanceToNext = cadSegment.SubChordLength;
					}

					// Case C : not overlap point
					else {

						// Step 1: create new CAMPointInfo
						CAMPointInfo currentInfo = new CAMPointInfo( camPoint );

						// Step 2: check property
						bool isStartPoint = startPointIndex.Equals( currentPointIndex );
						if( isStartPoint ) {
							currentInfo.IsStartPnt = true;

							// start pnt will be break pnt (this point2 is for next segment start point)
							currentInfo.Point2 = camPoint.Clone();
						}
						bool isControlPoint = toolVecModifyMap.ContainsKey( currentPointIndex );
						if( isControlPoint ) {
							currentInfo.IsCtrlPnt = true;
							currentInfo.ABValues = toolVecModifyMap[ currentPointIndex ];

							// ctrl pnt will be break pnt (this point2 is for next segment start point)
							currentInfo.Point2 = camPoint.Clone();
						}

						// Step 3: calculate distance to next point
						if( pntIdx == pointList.Count - 1 ) {

							// it is a segment last pnt
							currentInfo.DistanceToNext = 0;
						}
						else {
							currentInfo.DistanceToNext = cadSegment.SubChordLength;
						}
						result.Add( currentInfo );
					}
				}
			}

			// closed path last point merge to first point
			if( IsClosed && result.Count > 1 ) {
				CAMPointInfo lastCAMInfo = result[ result.Count - 1 ];
				CAMPoint2 lastCAMPoint = lastCAMInfo.Point.Clone();

				// remove last point
				result.RemoveAt( result.Count - 1 );

				// put last point info to first point - point
				CAMPointInfo firstPointInfo = result[ 0 ];
				firstPointInfo.Point = lastCAMPoint;

				// if last point with property set it to first point
				if( lastCAMInfo.IsStartPnt ) {
					firstPointInfo.IsStartPnt = true;
				}
				if( lastCAMInfo.IsCtrlPnt ) {
					firstPointInfo.IsCtrlPnt = true;
					firstPointInfo.ABValues = lastCAMInfo.ABValues;
				}
			}
			if( m_CraftData.IsReverse ) {
				ReverseCAMInfo( ref result );
			}
			return result;
		}

		void ReverseCAMInfo( ref List<CAMPointInfo> camInfoList )
		{
			camInfoList.Reverse();
			foreach( CAMPointInfo camInfo in camInfoList ) {

				if( camInfo.Point2 == null ) {
					continue;
				}
				// swap point and point2
				CAMPoint2 tempPnt = camInfo.Point;
				camInfo.Point = camInfo.Point2;
				camInfo.Point2 = tempPnt;
			}
		}

		CAMPoint2 ConvertCADPointToCAMPoint2( CADPoint cadPoint, bool isToolVecReverse )
		{
			gp_Dir initNorVec = isToolVecReverse ? cadPoint.NormalVec_1st.Reversed() : cadPoint.NormalVec_1st;
			return new CAMPoint2(
				cadPoint.Point,
				initNorVec,
				initNorVec,
				cadPoint.TangentVec,
				initNorVec
			);
		}

		bool ReBuildCAMSegBeforStartPnt( List<CAMPointInfo> camPntList, IReadOnlyList<ICADSegment> cadSegmentList, ref List<int> CtrlSegIdx, ref int currentSegmentIdx, out List<ICAMSegment> camSegmentList )
		{
			camSegmentList = new List<ICAMSegment>();
			if( camPntList == null || camPntList.Count == 0 ) {
				return false;
			}
			int nStartPntIndx = 0;
			for( int i = 0; i < camPntList.Count; i++ ) {
				if( camPntList[ i ].IsStartPnt ) {
					nStartPntIndx = i;
					break;
				}
			}

			// start point is at [0][0], no point before it
			if( nStartPntIndx == 0 ) {
				return true;
			}
			List<CAMPoint2> currentSegmentPoints = new List<CAMPoint2>();
			for( int i = 0; i <= nStartPntIndx; i++ ) {
				CAMPointInfo currentPointInfo = camPntList[ i ];
				currentSegmentPoints.Add( currentPointInfo.Point );

				// check need to build CAMSegment
				bool isSplitPoint = currentPointInfo.Point2 != null && i != 0;
				bool reachStartPoint = i == nStartPntIndx;

				if( isSplitPoint || reachStartPoint ) {
					if( currentSegmentPoints.Count >= 2 ) {

						// check last pnt is at with segment
						bool isGetDone = GetSegmentType( i, cadSegmentList, out ESegmentType segmentType );
						if( isGetDone == false ) {
							return false;
						}
						ICAMSegment camSegment = BuildCAMSegmentFromCAMPointInfo( currentSegmentPoints, segmentType );
						if( camSegment == null ) {
							return false;
						}
						// for record on CtrlSegIdx
						currentSegmentIdx++;
						if( camPntList[ i ].IsCtrlPnt ) {
							CtrlSegIdx.Add( currentSegmentIdx );
						}
						camSegmentList.Add( camSegment );
					}

					// is new segment start
					if( !reachStartPoint ) {
						currentSegmentPoints = new List<CAMPoint2>();
						currentSegmentPoints.Add( currentPointInfo.Point2 );
					}
				}
			}
			return true;
		}

		bool ReBuildCAMSegmentFromStartPnt( List<CAMPointInfo> camPntList, IReadOnlyList<ICADSegment> cadSegmentList, ref List<int> CtrlSegIdx, ref int currentSegmentIdx, out List<ICAMSegment> camSegmentList )
		{
			camSegmentList = new List<ICAMSegment>();
			if( camPntList == null || camPntList.Count == 0 ) {
				return false;
			}

			// find start point at with index
			int nStartPntIndx = 0;
			for( int i = 0; i < camPntList.Count; i++ ) {
				if( camPntList[ i ].IsStartPnt ) {
					nStartPntIndx = i;
					break;
				}
			}
			List<CAMPoint2> currentSegmentPoints = new List<CAMPoint2>();

			for( int i = nStartPntIndx; i < camPntList.Count; i++ ) {
				CAMPointInfo currentPointInfo = camPntList[ i ];

				// start point is special case
				if( i == nStartPntIndx ) {

					// any segment to build start with point2
					if( currentPointInfo.Point2 != null ) {
						currentSegmentPoints.Add( currentPointInfo.Point2 );
					}

					// start point caminfo need have two pnt
					else {
						return false;
					}
				}
				else {
					currentSegmentPoints.Add( currentPointInfo.Point );
				}

				// check special case (break pnt)
				bool isSplitPoint = currentPointInfo.Point2 != null && i != nStartPntIndx;
				bool isLastPoint = i == camPntList.Count - 1;
				if( isSplitPoint || isLastPoint ) {

					// it is close path last pnt
					if( isLastPoint && IsClosed ) {

						// add point back : real end pnt is first segment index[0]
						currentSegmentPoints.Add( camPntList[ 0 ].Point );
					}

					// build cam segment
					if( currentSegmentPoints.Count >= 2 ) {
						// check last pnt is at with segment
						bool isGetDone = GetSegmentType( i, cadSegmentList, out ESegmentType segmentType );
						if( isGetDone == false ) {
							return false;
						}
						ICAMSegment camSegment = BuildCAMSegmentFromCAMPointInfo( currentSegmentPoints, segmentType );
						if( camSegment == null ) {
							return false;
						}

						// for CtrlSegIdx to record
						currentSegmentIdx = camSegmentList.Count;

						// segment end not with isCtrlPnt means it is normal overlap
						if( camPntList[ i ].IsCtrlPnt ) {
							CtrlSegIdx.Add( currentSegmentIdx );
						}
						camSegmentList.Add( camSegment );
					}

					// is not last pnt, prepare new pnt for next segment (every segment start with point2)
					if( !isLastPoint ) {

						// reset
						currentSegmentPoints = new List<CAMPoint2>();

						// next seg start at point2
						currentSegmentPoints.Add( currentPointInfo.Point2 );
					}
				}
			}
			return true;
		}

		ICAMSegment BuildCAMSegmentFromCAMPointInfo( List<CAMPoint2> camPointList, ESegmentType segmentType )
		{
			if( camPointList == null || camPointList.Count < 2 ) {
				return null;
			}
			double dChordLength = camPointList[ 0 ].Point.Distance( camPointList[ 1 ].Point );
			double dEdgeLength = dChordLength * ( camPointList.Count - 1 );
			ICAMSegment camSegment = null;
			if( segmentType == ESegmentType.Line ) {

				camSegment = new LineCAMSegment( camPointList, dEdgeLength, dChordLength, dChordLength );
			}
			else if( segmentType == ESegmentType.Arc ) {
				if( camPointList.Count < 3 ) {
					camSegment = new LineCAMSegment( camPointList, dEdgeLength, dChordLength, dChordLength );
				}
				else {
					camSegment = new ArcCAMSegment( camPointList, dEdgeLength, dChordLength, dChordLength );
				}
			}
			return camSegment;
		}

		bool GetSegmentType( int nPntIndex, IReadOnlyList<ICADSegment> cadsegment, out ESegmentType segmentType )
		{
			int nPntSum = 0;
			segmentType = ESegmentType.Line;
			if( cadsegment == null || cadsegment.Count == 0 ) {
				return false;
			}

			for( int i = 0; i < cadsegment.Count; i++ ) {

				// removew over pnt
				if( i != 0 ) {
					nPntSum -= 1;
				}

				// cal current seg range
				int segmentStartIndex = nPntSum;
				int segmentEndIndex = nPntSum + cadsegment[ i ].PointList.Count - 1;

				// check nPntIndex is in this range
				if( nPntIndex >= segmentStartIndex && nPntIndex <= segmentEndIndex ) {
					segmentType = cadsegment[ i ].SegmentType;
					return true;
				}
				nPntSum += cadsegment[ i ].PointList.Count;
			}
			return false;
		}

		void ApplyToolVectorInterpolation( List<CAMPointInfo> camPointInfoList, bool isReverse )
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
				return;
			}

			// only one ctrl pnt, apply to all point
			if( ctrlPntIdx.Count == 1 ) {
				ApplySpecifiedVec( camPointInfoList, ctrlPntIdx[ 0 ] );
				return;
			}
			List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList( ctrlPntIdx );

			// modify the tool vector
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {

				// get start and end index
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				InterpolateToolVec( nStartIndex, nEndIndex, camPointInfoList, isReverse );
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

		void BuildCAMFeatureSegment()
		{
			// Topo:overcut
			// SetOverCut();
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
				List<ICAMSegment> overCutSement = OverCutSegmentBuilder.BuildOverCutSegment( m_CAMSegmentList, m_CraftData.OverCutLength, m_CraftData.IsReverse );
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
				if( m_CAMSegmentList == null || m_CAMSegmentList.Count == 0 ) {
					return;
				}
				ICAMSegment camSegmentConnectWithStartPnt = m_CAMSegmentList.FirstOrDefault();
				List<ICAMSegment> leadCAMSegment = LeadHelper.BuildLeadCAMSegment( m_CraftData, camSegmentConnectWithStartPnt, true );
				m_LeadInSegmentList = leadCAMSegment;
			}
			if( m_CraftData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
				ICAMSegment camSegmentConnectWithEndPnt;
				// with overcut
				if( m_CraftData.OverCutLength > 0 && m_OverCutSegmentList.Count > 0 ) {
					camSegmentConnectWithEndPnt = m_OverCutSegmentList.Last();
				}
				else {
					if( m_CAMSegmentList == null || m_CAMSegmentList.Count == 0 ) {
						return;
					}
					camSegmentConnectWithEndPnt = m_CAMSegmentList.Last();
				}
				List<ICAMSegment> leadCAMSegment = LeadHelper.BuildLeadCAMSegment( m_CraftData, camSegmentConnectWithEndPnt, false );
				m_LeadOutSegmentList = leadCAMSegment;
			}
		}

		List<ICAMSegment> m_CAMSegmentList = new List<ICAMSegment>();
		List<int> m_CtrlToolSegIdxList = new List<int>();
		List<ICAMSegment> m_OverCutSegmentList = new List<ICAMSegment>();
		List<ICAMSegment> m_LeadInSegmentList = new List<ICAMSegment>();
		List<ICAMSegment> m_LeadOutSegmentList = new List<ICAMSegment>();

		// they are sibling pointer, and change the declare order
		CraftData m_CraftData;
		List<ICADSegment> m_CADSegmentList;
		public IReadOnlyList<ICADSegment> CADSegmentList => m_CADSegmentList;

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;


	}

}
