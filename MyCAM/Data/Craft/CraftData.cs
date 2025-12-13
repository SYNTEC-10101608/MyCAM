using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public class CraftData
	{
		public CraftData()
		{
			SubscribeSubParamChanged();
		}

		// this constructor is used when reading from file
		public CraftData( int startPoint,
			bool isPathReverse,
			LeadData leadData,
			double overCutLength,
			Dictionary<int, Tuple<double, double>> toolVecModifyMap,
			bool isToolVecReverse,
			TraverseData traverseData )
		{
			m_StartPointIndex = startPoint;
			m_IsPathReverse = isPathReverse;
			m_LeadData = leadData;
			m_OverCutLength = overCutLength;
			m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>( toolVecModifyMap );
			m_IsToolVecReverse = isToolVecReverse;
			m_TraverseData = traverseData;
			SubscribeSubParamChanged();
		}

		public Action ParameterChanged;

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

		public bool IsPathReverse
		{
			get
			{
				return m_IsPathReverse;
			}
			set
			{
				if( m_IsPathReverse != value ) {
					m_IsPathReverse = value;
					ParameterChanged?.Invoke();
				}
			}
		}

		public LeadData LeadData
		{
			get
			{
				// to prevent null value
				if( m_LeadData == null ) {
					m_LeadData = new LeadData();
				}
				return m_LeadData;
			}
			set
			{
				// to prevent null value
				if( value == null ) {
					value = new LeadData();
				}
				if( m_LeadData != value ) {
					m_LeadData = value;
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
				return m_ToolVecModifyMap;
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

		void SubscribeSubParamChanged()
		{
			m_LeadData.PropertyChanged += SubParamChanged;
			m_TraverseData.PropertyChanged += SubParamChanged;
		}

		void SubParamChanged()
		{
			ParameterChanged?.Invoke();
		}

		int m_StartPointIndex = 0;
		bool m_IsPathReverse = false;
		LeadData m_LeadData = new LeadData();
		double m_OverCutLength = 0;
		Dictionary<int, Tuple<double, double>> m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>();
		bool m_IsToolVecReverse = false;
		TraverseData m_TraverseData = new TraverseData();
	}
}