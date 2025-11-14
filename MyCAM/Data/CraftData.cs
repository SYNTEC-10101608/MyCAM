using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public class CraftData
	{
		public CraftData( string szUID )
		{
			UID = szUID;
			m_LeadParam.LeadPropertyChanged += MultiLevelParameterChanged;
			m_TraverseData.TraverseParameterChanged += MultiLevelParameterChanged;
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
			m_LeadParam.LeadPropertyChanged += MultiLevelParameterChanged;
			m_TraverseData.TraverseParameterChanged += MultiLevelParameterChanged;
		}

		public Action ParameterChanged;

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
					ParameterChanged?.Invoke();
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
					ParameterChanged?.Invoke();
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
					ParameterChanged?.Invoke();
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
					ParameterChanged?.Invoke();
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
					ParameterChanged?.Invoke();
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
					ParameterChanged?.Invoke();
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
		public void SetToolVecModify( int index, double dRA_deg, double dRB_deg )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap[ index ] = new Tuple<double, double>( dRA_deg, dRB_deg );
			}
			else {
				m_ToolVecModifyMap.Add( index, new Tuple<double, double>( dRA_deg, dRB_deg ) );
			}
			ParameterChanged?.Invoke();
		}

		public void RemoveToolVecModify( int index )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap.Remove( index );
			}
			ParameterChanged?.Invoke();
		}

		void MultiLevelParameterChanged()
		{
			ParameterChanged?.Invoke();
		}

		LeadData m_LeadParam = new LeadData();
		TraverseData m_TraverseData = new TraverseData();
		Dictionary<int, Tuple<double, double>> m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>();

		int m_StartPointIndex = 0;
		double m_OverCutLength = 0;

		bool m_IsToolVecReverse = false;
		bool m_IsReverse = false;
	}
}