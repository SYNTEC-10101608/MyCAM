using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public enum EToolVecInterpolateType
	{
		Normal = 0,
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

	public class ToolVecModifyData2
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

		public EToolVecInterpolateType InterpolateType
		{
			get; set;
		} = EToolVecInterpolateType.Normal;

		public ToolVecModifyData2()
		{
			RA_deg = 0;
			RB_deg = 0;
			Master_deg = 0;
			Slave_deg = 0;
		}

		public ToolVecModifyData2( double ra_deg, double rb_deg, double master_deg, double slave_deg, EToolVecInterpolateType interpolateType )
		{
			RA_deg = ra_deg;
			RB_deg = rb_deg;
			Master_deg = master_deg;
			Slave_deg = slave_deg;
			InterpolateType = interpolateType;
		}

		public ToolVecModifyData2 Clone()
		{
			return new ToolVecModifyData2( RA_deg, RB_deg, Master_deg, Slave_deg, InterpolateType );
		}
	}

	public class StartPntToolVecParam
	{
		public Action PropertyChanged;

		public ToolVecModifyData2 StartPnt
		{
			get
			{
				return m_StartPnt;
			}
			set
			{
				if( m_StartPnt != value ) {
					m_StartPnt = value;
					PropertyChanged?.Invoke();
				}
			}
		}


		public ToolVecModifyData2 EndPnt
		{
			get
			{
				return m_EndPnt;
			}
			set
			{
				if( m_EndPnt != value ) {
					m_EndPnt = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public StartPntToolVecParam()
		{
		}

		public StartPntToolVecParam( ToolVecModifyData2 startPntData, ToolVecModifyData2 endPntData )
		{
			StartPnt = startPntData;
			EndPnt = endPntData;
		}

		ToolVecModifyData2 m_EndPnt;
		ToolVecModifyData2 m_StartPnt;
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
			TraverseData traverseData )
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
					ClearToolVecModify();
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

		public StartPntToolVecParam StartPntToolVecData
		{
			get
			{
				return m_StartPntToolVecData;
			}
			set
			{
				if( m_StartPntToolVecData != value ) {
					if( m_StartPntToolVecData != null ) {
						m_StartPntToolVecData.PropertyChanged -= SubParamChanged;
					}
					m_StartPntToolVecData = value;
					if( m_StartPntToolVecData != null ) {
						m_StartPntToolVecData.PropertyChanged += SubParamChanged;
					}
					CAMFactorChanged?.Invoke();
				}
			}
		}

		public ToolVecModifyMap ToolVecModifyMap2
		{
			get
			{
				return m_ToolVecModifyMap2;
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
		public void SetToolVecModify( int index, double dRA_deg, double dRB_deg, double master_deg, double slave_deg, EToolVecInterpolateType interpolateType = EToolVecInterpolateType.Normal )
		{
			if( m_ToolVecModifyMap2.ContainsKey( index ) ) {
				m_ToolVecModifyMap2[ index ] = new ToolVecModifyData2( dRA_deg, dRB_deg, master_deg, slave_deg, m_ToolVecModifyMap2[ index ].InterpolateType );
			}
			else {
				m_ToolVecModifyMap2.Add( index, new ToolVecModifyData2( dRA_deg, dRB_deg, master_deg, slave_deg, interpolateType ) );
			}
			CAMFactorChanged?.Invoke();
		}

		public void RemoveToolVecModify( int index )
		{
			if( m_ToolVecModifyMap2.ContainsKey( index ) ) {
				if( IsPathReverse == false ) {
					bool isFoundNext = FindNextMapIndex( index, out int nNextIdx );
					if( isFoundNext ) {
						m_ToolVecModifyMap2.Remove( index, nNextIdx );
					}
					else {
						EToolVecInterpolateType removedType = m_ToolVecModifyMap2[ index ].InterpolateType;
						StartPntToolVecData.EndPnt.InterpolateType = removedType;
						m_ToolVecModifyMap2.Remove( index );
					}
				}

				// 反向
				else {
					{
						bool isFoundPre = FindPreMapIndex( index, out int nPreIdx );
						if( isFoundPre ) {
							m_ToolVecModifyMap2.Remove( index, nPreIdx );
						}
						else {
							m_ToolVecModifyMap2.Remove( index );
						}

					}
				}

			}
			CAMFactorChanged?.Invoke();
		}

		public void SetInterpolationMode( int nCurrentIdx, EToolVecInterpolateType interpolateType )
		{
			bool isGetNextModfiyIndex = FindNextMapIndex( nCurrentIdx, out int nNextIdx );
			if( isGetNextModfiyIndex ) {
				ToolVecModifyMap2[ nNextIdx ].InterpolateType = interpolateType;
			}
			else {
				StartPntToolVecData.EndPnt.InterpolateType = interpolateType;
			}
			CAMFactorChanged?.Invoke();

		}

		public void ClearToolVecModify()
		{
			m_ToolVecModifyMap2.Clear();
			StartPntToolVecData = null;
			CAMFactorChanged?.Invoke();
		}

		void SubscribeSubParamChanged()
		{
			m_LeadData.PropertyChanged += SubParamChanged;
			m_TraverseData.PropertyChanged += SubParamChanged;
			m_StartPntToolVecData.PropertyChanged += SubParamChanged;
		}

		void SubParamChanged()
		{
			CAMFactorChanged?.Invoke();
		}


		public bool FindNextMapIndex( int currentIdx, out int nextIdx )
		{
			int StartPntIdx = m_StartPointIndex;
			// find the smallest key that is greater than the removed key
			nextIdx = -1;
			bool found = false;

			// 現在index比起點大
			if( currentIdx > StartPntIdx ) {


				// 從現在位置找到路徑尾中最小的
				foreach( int k in ToolVecModifyMap2.Keys ) {
					if( k > currentIdx ) {
						nextIdx = k;
						found = true;
						break;
					}
				}

				// 從目前起點到最後都沒有,找0~起點前最小的
				if( found == false ) {
					foreach( int k in ToolVecModifyMap2.Keys ) {
						if( k > StartPntIdx ) {
							break;
						}
						if( k < currentIdx ) {
							nextIdx = k;
							found = true;
							break;
						}
					}
				}
			}

			// 現在位置在起點之前
			else {

				foreach( int k in ToolVecModifyMap2.Keys ) {
					if( k > StartPntIdx ) {
						break;
					}
					if( k > currentIdx ) {
						nextIdx = k;
						found = true;
						break;
					}
				}
			}

			return found;
		}

		public bool FindPreMapIndex( int currentIdx, out int preIdx )
		{
			int StartPntIdx = m_StartPointIndex;
			bool isPathReverse = m_IsPathReverse;
			preIdx = -1;
			bool found = false;

			// 現在index比起點大
			if( currentIdx > StartPntIdx ) {

				//從起點找到現在位置中最大的
				foreach( int k in ToolVecModifyMap2.Keys ) {
					if( k > currentIdx ) {
						break;
					}
					if( k > StartPntIdx && k < currentIdx ) {
						preIdx = k;
						found = true;
					}
				}
			}

			// 現在位置在起點之前
			else {
				// 從0找到這個點中最大的
				foreach( int k in ToolVecModifyMap2.Keys ) {
					if( k < currentIdx ) {
						preIdx = k;
						found = true;
					}
				}

				// 從起點到路徑尾中最大的
				if( found == false ) {
					foreach( int k in ToolVecModifyMap2.Keys ) {
						if( k > StartPntIdx ) {
							preIdx = k;
							found = true;
						}
					}
				}
			}
			return found;
		}

		const int DEFAULT_TECH_LAYER = 1;
		int m_TechLayer = DEFAULT_TECH_LAYER;
		int m_StartPointIndex = 0;
		bool m_IsPathReverse = false;
		LeadData m_LeadData = new LeadData();
		double m_OverCutLength = 0;
		EToolVecInterpolateType m_InterpolateType = EToolVecInterpolateType.Normal;
		Dictionary<int, ToolVecModifyData> m_ToolVecModifyMap = new Dictionary<int, ToolVecModifyData>();
		ToolVecModifyMap m_ToolVecModifyMap2 = new ToolVecModifyMap();
		StartPntToolVecParam m_StartPntToolVecData = new StartPntToolVecParam();
		bool m_IsToolVecReverse = false;
		TraverseData m_TraverseData = new TraverseData();
		gp_Trsf m_CumulativeTrsfMatrix = new gp_Trsf();
		double m_CompensatedDistance = 0;
	}


}