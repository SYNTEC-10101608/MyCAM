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
			List<CAMPointInfo> pathCAMInfoList = CAMPrestageHelper.FlattenCADSegmentsToCAMPointInfo( m_CADSegmentList, m_CraftData, IsClosed );
			if( m_CraftData.IsReverse ) {
				ReverseCAMInfo( ref pathCAMInfoList );
			}

			// Step 2: Do interpolation
			List<IToolVecCAMPointInfo> toolVecInfoList = pathCAMInfoList.Cast<IToolVecCAMPointInfo>().ToList();
			ToolVectorHelper.CalculateToolVector( ref toolVecInfoList, m_CraftData.IsToolVecReverse, m_CraftData.IsReverse, IsClosed );

			// Step 3: use caminfo to build cam segment
			bool isBuildDone = ReBuildCAMSegment( pathCAMInfoList, out List<ICAMSegment> PathCAMSegList, out List<int> CtrlSegIdx );
			if( isBuildDone == false ) {
				return;
			}
			m_CAMSegmentList = PathCAMSegList;
			m_CtrlToolSegIdxList = CtrlSegIdx;
		}

		void ReverseCAMInfo( ref List<CAMPointInfo> camInfoList )
		{
			camInfoList.Reverse();
			foreach( CAMPointInfo camInfo in camInfoList ) {
				if( camInfo.SharingPoint == null ) {
					continue;
				}
				CAMPoint2 tempPnt = camInfo.Point;
				camInfo.Point = camInfo.SharingPoint;
				camInfo.SharingPoint = tempPnt;
			}
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
				bool isSplitPoint = currentPointInfo.SharingPoint != null && i != 0;
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
						if( camPntList[ i ].IsToolVecdPnt ) {
							CtrlSegIdx.Add( currentSegmentIdx );
						}
						camSegmentList.Add( camSegment );
					}

					// is new segment start
					if( !reachStartPoint ) {
						currentSegmentPoints = new List<CAMPoint2>();
						currentSegmentPoints.Add( currentPointInfo.SharingPoint );
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
					if( currentPointInfo.SharingPoint != null ) {
						currentSegmentPoints.Add( currentPointInfo.SharingPoint );
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
				bool isSplitPoint = currentPointInfo.SharingPoint != null && i != nStartPntIndx;
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
						if( camPntList[ i ].IsToolVecdPnt ) {
							CtrlSegIdx.Add( currentSegmentIdx );
						}
						camSegmentList.Add( camSegment );
					}

					// is not last pnt, prepare new pnt for next segment (every segment start with point2)
					if( !isLastPoint ) {

						// reset
						currentSegmentPoints = new List<CAMPoint2>();

						// next seg start at point2
						currentSegmentPoints.Add( currentPointInfo.SharingPoint );
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

		void SetCraftDataDirty()
		{
			if( !m_IsCraftDataDirty ) {
				m_IsCraftDataDirty = true;
			}
		}

		#endregion

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

		void BuildCAMFeatureSegment()
		{
			// Topo:overcut
			// SetOverCut();
			SetLead();
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
