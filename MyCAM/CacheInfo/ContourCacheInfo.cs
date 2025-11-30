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
			List<CAMPointInfo> pathCAMInfoList = CAMPreStageHelper.FlattenCADSegmentsToCAMPointInfo( m_CADSegmentList, m_CraftData, IsClosed );

			// Step 2: Do interpolation
			List<IToolVecCAMPointInfo> toolVecInfoList = pathCAMInfoList.Cast<IToolVecCAMPointInfo>().ToList();
			ToolVectorHelper.CalculateToolVector( ref toolVecInfoList, m_CraftData.IsToolVecReverse, IsClosed );
			if( m_CraftData.IsReverse ) {
				ReverseCAMInfo( ref pathCAMInfoList );
			}

			// Step 3: use caminfo to build cam segment
			List<ICAMSegInfo> camSegElementInfoList = pathCAMInfoList.Cast<ICAMSegInfo>().ToList();
			bool isBuildDone = CAMPostStageHelper.ReBuildCAMSegment( camSegElementInfoList, IsClosed, out List<ICAMSegment> PathCAMSegList, out List<int> CtrlSegIdx );
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
				CAMPoint2 tempPnt = camInfo.MainPoint;
				camInfo.MainPoint = camInfo.SharingPoint;
				camInfo.SharingPoint = tempPnt;
			}
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
