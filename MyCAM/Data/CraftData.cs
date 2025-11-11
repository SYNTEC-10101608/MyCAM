using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public class CraftData
	{
		public CraftData( string szUID )
		{
			UID = szUID;
		}

		// this constructor is used when reading from file
		public CraftData( string szUID, LeadData leadData, int startPoint, double overCutLength, bool isReverse, bool isClosed, TraverseData traverseData, Dictionary<int, Tuple<double, double>> toolVecModifyMap, bool isToolVecReverse )
		{
			UID = szUID;
			m_LeadParam = leadData;
			m_StartPointIndex = startPoint;
			m_OverCutLength = overCutLength;
			m_IsReverse = isReverse;
			m_TraverseData = traverseData;
			m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>( toolVecModifyMap );
			m_IsToolVecReverse = isToolVecReverse;
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

		// To-do：use this function is Dirty
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

		// To-do：use this function is Dirty
		public void RemoveToolVecModify( int index )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap.Remove( index );
			}
			m_IsDirty = true;
		}

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
			m_IsDirty = false;
			return true;
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


}