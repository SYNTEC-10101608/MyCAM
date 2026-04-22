using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public enum EToolVecInterpolateType
	{
		Normal,
		VectorInterpolation,
		TiltAngleInterpolation,
	}

	public class ToolVecModifyData
	{
		public double RA_deg
		{
			get; set;
		}

		public double RB_deg
		{
			get; set;
		}

		public double Master_deg
		{
			get; set;
		}

		public double Slave_deg
		{
			get; set;
		}

		public ToolVecModifyData()
		{
			RA_deg = 0;
			RB_deg = 0;
			Master_deg = 0;
			Slave_deg = 0;
		}

		public ToolVecModifyData( double ra_deg, double rb_deg, double master_deg, double slave_deg )
		{
			RA_deg = ra_deg;
			RB_deg = rb_deg;
			Master_deg = master_deg;
			Slave_deg = slave_deg;
		}

		public ToolVecModifyData Clone()
		{
			return new ToolVecModifyData( RA_deg, RB_deg, Master_deg, Slave_deg );
		}
	}

	public class ContourEditData
	{
		public double DX
		{
			get; set;
		}

		public double DY
		{
			get; set;
		}

		public double DZ
		{
			get; set;
		}

		public ContourEditData()
		{
			DX = 0;
			DY = 0;
			DZ = 0;
		}

		public ContourEditData( double dx, double dy, double dz )
		{
			DX = dx;
			DY = dy;
			DZ = dz;
		}

		public ContourEditData Clone()
		{
			return new ContourEditData( DX, DY, DZ );
		}
	}

	public class CraftData
	{
		public CraftData()
		{
			SubscribeSubParamChanged();
		}

		// this constructor is used when reading from file
		public CraftData(
			int techLayer,
			int startPoint,
			bool isPathReverse,
			LeadData leadData,
			double overCutLength,
			Dictionary<int, ToolVecModifyData> toolVecModifyMap,
			bool isToolVecReverse,
			EToolVecInterpolateType interpolateType,
			TraverseData traverseData,
			Dictionary<int, ContourEditData> contourEditMap )
		{
			m_TechLayer = techLayer;
			m_StartPointIndex = startPoint;
			m_IsPathReverse = isPathReverse;
			m_LeadData = leadData;
			m_OverCutLength = overCutLength;
			m_ToolVecModifyMap = new Dictionary<int, ToolVecModifyData>();
			if( toolVecModifyMap != null ) {
				foreach( var kvp in toolVecModifyMap ) {
					m_ToolVecModifyMap.Add( kvp.Key, kvp.Value.Clone() );
				}
			}
			m_ContourEditMap = new Dictionary<int, ContourEditData>();
			if( contourEditMap != null ) {
				foreach( var kvp in contourEditMap ) {
					m_ContourEditMap.Add( kvp.Key, kvp.Value.Clone() );
				}
			}
			m_IsToolVecReverse = isToolVecReverse;
			m_InterpolateType = interpolateType;
			m_TraverseData = traverseData;
			SubscribeSubParamChanged();
		}

		public Action CAMFactorChanged;
		public Action CADFactorChanged;

		public int TechLayer
		{
			get
			{
				return m_TechLayer;
			}
			set
			{
				if( m_TechLayer != value ) {
					if( value < DEFAULT_TECH_LAYER ) {
						m_TechLayer = DEFAULT_TECH_LAYER;
						return;
					}
					m_TechLayer = value;
				}
			}
		}

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
					CAMFactorChanged?.Invoke();
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
					CAMFactorChanged?.Invoke();
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
					CAMFactorChanged?.Invoke();
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
					CAMFactorChanged?.Invoke();
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
					CAMFactorChanged?.Invoke();
				}
			}
		}

		public gp_Trsf CumulativeTrsfMatrix
		{
			get
			{
				// to prevent null value
				if( m_CumulativeTrsfMatrix == null ) {
					m_CumulativeTrsfMatrix = new gp_Trsf();
				}
				return m_CumulativeTrsfMatrix;
			}
			set
			{
				if( value == null ) {
					value = new gp_Trsf();
				}
				if( m_CumulativeTrsfMatrix != value ) {
					m_CumulativeTrsfMatrix = value;
					CADFactorChanged?.Invoke();
				}
			}
		}

		public double CompensatedDistance
		{
			get
			{
				return m_CompensatedDistance;
			}
			set
			{
				if( m_CompensatedDistance != value ) {
					m_CompensatedDistance = value;
					CADFactorChanged?.Invoke();
				}
			}
		}

		public EToolVecInterpolateType InterpolateType
		{
			get
			{
				return m_InterpolateType;
			}
			set
			{
				if( m_InterpolateType != value ) {
					m_InterpolateType = value;
					CAMFactorChanged?.Invoke();
				}
			}
		}

		public Dictionary<int, ToolVecModifyData> ToolVecModifyMap
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
					CAMFactorChanged?.Invoke();
				}
			}
		}

		// API for outside modification
		public void SetToolVecModify( int index, double dRA_deg, double dRB_deg, double master_deg, double slave_deg )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap[ index ] = new ToolVecModifyData( dRA_deg, dRB_deg, master_deg, slave_deg );
			}
			else {
				m_ToolVecModifyMap.Add( index, new ToolVecModifyData( dRA_deg, dRB_deg, master_deg, slave_deg ) );
			}
			CAMFactorChanged?.Invoke();
		}

		public void RemoveToolVecModify( int index )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap.Remove( index );
			}
			CAMFactorChanged?.Invoke();
		}

		public void ClearToolVecModify()
		{
			m_ToolVecModifyMap.Clear();
			CAMFactorChanged?.Invoke();
		}

		public Dictionary<int, ContourEditData> ContourEditMap
		{
			get
			{
				return m_ContourEditMap;
			}
		}

		public void SetContourEditPoint( int index, double dx, double dy, double dz )
		{
			if( m_ContourEditMap.ContainsKey( index ) ) {
				m_ContourEditMap[ index ] = new ContourEditData( dx, dy, dz );
			}
			else {
				m_ContourEditMap.Add( index, new ContourEditData( dx, dy, dz ) );
			}
			CADFactorChanged?.Invoke();
		}

		public void RemoveContourEditPoint( int index )
		{
			if( m_ContourEditMap.ContainsKey( index ) ) {
				m_ContourEditMap.Remove( index );
			}
			CADFactorChanged?.Invoke();
		}

		public void ClearContourEditPoint()
		{
			m_ContourEditMap.Clear();
			CADFactorChanged?.Invoke();
		}

		void SubscribeSubParamChanged()
		{
			m_LeadData.PropertyChanged += SubParamChanged;
			m_TraverseData.PropertyChanged += SubParamChanged;
		}

		void SubParamChanged()
		{
			CAMFactorChanged?.Invoke();
		}

		const int DEFAULT_TECH_LAYER = 1;
		int m_TechLayer = DEFAULT_TECH_LAYER;
		int m_StartPointIndex = 0;
		bool m_IsPathReverse = false;
		LeadData m_LeadData = new LeadData();
		double m_OverCutLength = 0;
		EToolVecInterpolateType m_InterpolateType = EToolVecInterpolateType.Normal;
		Dictionary<int, ToolVecModifyData> m_ToolVecModifyMap = new Dictionary<int, ToolVecModifyData>();
		bool m_IsToolVecReverse = false;
		TraverseData m_TraverseData = new TraverseData();
		gp_Trsf m_CumulativeTrsfMatrix = new gp_Trsf();
		double m_CompensatedDistance = 0;
		Dictionary<int, ContourEditData> m_ContourEditMap = new Dictionary<int, ContourEditData>();
	}
}
