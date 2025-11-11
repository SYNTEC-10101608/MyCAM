using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public class CraftData
	{
		// re: comment what is this for
		public CraftData( string szUID, LeadData leadData, int startPoint, double overCutLength, bool isReverse, bool isClosed, TraverseData traverseData, Dictionary<int, Tuple<double, double>> toolVecModifyMap, bool isToolVecReverse )
		{
			UID = szUID;
			m_LeadParam = leadData;
			m_StartPointIndex = startPoint;
			m_OverCutLength = overCutLength;
			m_IsReverse = isReverse;
			IsClosed = isClosed;
			m_TraverseData = traverseData;
			m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>( toolVecModifyMap );
			m_IsToolVecReverse = isToolVecReverse;
		}

		public CraftData( string szUID, bool isClosed )
		{
			UID = szUID;
			IsClosed = isClosed;
		}

		public string UID
		{
			get; private set;
		}

		#region Lead

		public LeadData LeadLineParam
		{
			get
			{
				// to prevent null value
				if( m_LeadParam == null ) {
					m_LeadParam = new LeadData();
				}
				return m_LeadParam;
			}
			set
			{
				// to prevent null value
				if( value == null ) {
					value = new LeadData();
				}
				if( m_LeadParam != value ) {
					m_LeadParam = value;
					m_IsDirty = true;
				}
			}
		}

		//re: why no ref here? and this shoul be cache
		public bool IsHasLead
		{
			get
			{
				if( m_LeadParam == null ) {
					return false;
				}
				return ( m_LeadParam.LeadIn.Type != LeadLineType.None ) || ( m_LeadParam.LeadOut.Type != LeadLineType.None );
			}
		}
		#endregion

		public int StartPointIndex
		{
			get
			{
				return m_StartPointIndex;
			}
			set
			{
				if( m_StartPointIndex != value ) {
					m_StartPointIndex = value;
					m_IsDirty = true;
				}
			}
		}

		public double OverCutLength
		{
			get
			{
				return m_OverCutLength;
			}
			set
			{
				if( m_OverCutLength != value ) {
					m_OverCutLength = value;
					m_IsDirty = true;
				}
			}
		}

		public bool IsReverse
		{
			get
			{
				return m_IsReverse;
			}
			set
			{
				if( m_IsReverse != value ) {
					m_IsReverse = value;
					m_IsDirty = true;
				}
			}
		}

		public bool IsClosed
		{
			get; private set;
		}

		public TraverseData TraverseData
		{
			get
			{
				// to prevent null value
				if( m_TraverseData == null ) {
					m_TraverseData = new TraverseData();
				}
				return m_TraverseData;
			}
			set
			{
				// to prevent null value
				if( value == null ) {
					value = new TraverseData();
				}
				if( m_TraverseData != value ) {
					m_TraverseData = value;
					m_IsDirty = true;
				}
			}
		}

		public bool IsToolVecReverse
		{
			get
			{
				return m_IsToolVecReverse;
			}
			set
			{
				if( m_IsToolVecReverse != value ) {
					m_IsToolVecReverse = value;
					m_IsDirty = true;
				}
			}
		}

		public Dictionary<int, Tuple<double, double>> ToolVecModifyMap
		{
			get
			{
				return new Dictionary<int, Tuple<double, double>>( m_ToolVecModifyMap );
			}
		}

		// API for outside modification
		// re: this is better to stay in cache
		public bool GetToolVecModify( int index, out double dRA_deg, out double dRB_deg )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				dRA_deg = m_ToolVecModifyMap[ index ].Item1;
				dRB_deg = m_ToolVecModifyMap[ index ].Item2;
				return true;
			}
			else {
				dRA_deg = 0;
				dRB_deg = 0;
				return false;
			}
		}

		// re: To-do：use this function is Dirty
		public void SetToolVecModify( int index, double dRA_deg, double dRB_deg )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap[ index ] = new Tuple<double, double>( dRA_deg, dRB_deg );
			}
			else {
				m_ToolVecModifyMap.Add( index, new Tuple<double, double>( dRA_deg, dRB_deg ) );
			}
			m_IsDirty = true;
		}

		// re: To-do：use this function is Dirty
		public void RemoveToolVecModify( int index )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap.Remove( index );
			}
			m_IsDirty = true;
		}

		// re: this is better to stay in cache
		public HashSet<int> GetToolVecModifyIndex()
		{
			HashSet<int> result = new HashSet<int>();
			foreach( int nIndex in m_ToolVecModifyMap.Keys ) {
				result.Add( nIndex );
			}
			return result;
		}

		public bool GetCraftDataIsDirty()
		{
			if( m_IsDirty == false ) {
				return false;
			}
			// re: logic bomb
			bool isDirty = m_IsDirty;
			m_IsDirty = false;
			return isDirty;
		}

		LeadData m_LeadParam = new LeadData();
		TraverseData m_TraverseData = new TraverseData();
		Dictionary<int, Tuple<double, double>> m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>();

		int m_StartPointIndex = 0;
		double m_OverCutLength = 0;

		bool m_IsToolVecReverse = false;
		bool m_IsReverse = false;
		bool m_IsDirty = false;
	}

	// re: should not be here
	public class CADPoint
	{
		public CADPoint( gp_Pnt point, gp_Dir normalVec_1st, gp_Dir normalVec_2nd, gp_Dir tangentVec )
		{
			m_Point = new gp_Pnt( point.X(), point.Y(), point.Z() );
			m_NormalVec_1st = new gp_Dir( normalVec_1st.XYZ() );
			m_NormalVec_2nd = new gp_Dir( normalVec_2nd.XYZ() );
			m_TangentVec = new gp_Dir( tangentVec.XYZ() );
		}

		public gp_Pnt Point
		{
			get
			{
				return new gp_Pnt( m_Point.X(), m_Point.Y(), m_Point.Z() );
			}
		}

		public gp_Dir NormalVec_1st
		{
			get
			{
				return new gp_Dir( m_NormalVec_1st.XYZ() );
			}
		}

		// normal vector on co-face
		public gp_Dir NormalVec_2nd
		{
			get
			{
				return new gp_Dir( m_NormalVec_2nd.XYZ() );
			}
		}

		// tangent vector on path
		public gp_Dir TangentVec
		{
			get
			{
				return new gp_Dir( m_TangentVec.XYZ() );
			}
		}

		public void Transform( gp_Trsf transform )
		{
			m_Point.Transform( transform );
			m_NormalVec_1st.Transform( transform );
			m_NormalVec_2nd.Transform( transform );
			m_TangentVec.Transform( transform );
		}

		public CADPoint Clone()
		{
			return new CADPoint( Point, NormalVec_1st, NormalVec_2nd, TangentVec );
		}

		// using backing fields to prevent modified outside
		gp_Pnt m_Point;
		gp_Dir m_NormalVec_1st;
		gp_Dir m_NormalVec_2nd;
		gp_Dir m_TangentVec;
	}

	// currently assuming CAM = CAD + ToolVec
	public class CAMPoint
	{
		public CAMPoint( CADPoint cadPoint, gp_Dir toolVec )
		{
			CADPoint = cadPoint;
			m_ToolVec = new gp_Dir( toolVec.XYZ() );
		}

		public CADPoint CADPoint
		{
			get; private set;
		}

		public gp_Dir ToolVec
		{
			get
			{
				return new gp_Dir( m_ToolVec.XYZ() );
			}
			set
			{
				m_ToolVec = new gp_Dir( value.XYZ() );
			}
		}

		public CAMPoint Clone()
		{
			return new CAMPoint( CADPoint.Clone(), ToolVec );
		}

		// using backing field to prevent modified outside
		gp_Dir m_ToolVec;
	}
}