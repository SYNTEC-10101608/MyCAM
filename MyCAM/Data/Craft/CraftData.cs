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

	public class StartPntToolVecParam
	{
		public Action PropertyChanged;

		public ToolVecModifyData StartPnt
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


		public ToolVecModifyData EndPnt
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

		public StartPntToolVecParam( ToolVecModifyData startPntData, ToolVecModifyData endPntData )
		{
			StartPnt = startPntData;
			EndPnt = endPntData;
		}

		ToolVecModifyData m_EndPnt;
		ToolVecModifyData m_StartPnt;
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
			Dictionary<int, ToolVecModifyData> toolVecModifyMap2,
			StartPntToolVecParam startPntToolVecData,
		bool isToolVecReverse,
			TraverseData traverseData )
		{
			m_TechLayer = techLayer;
			m_StartPointIndex = startPoint;
			m_IsPathReverse = isPathReverse;
			m_LeadData = leadData;
			m_OverCutLength = overCutLength;
			if( toolVecModifyMap2 != null ) {
				foreach( var kvp in toolVecModifyMap2 ) {
					m_ToolVecModifyMap2.Add( kvp.Key, kvp.Value.Clone() );
				}
			}
			if( startPntToolVecData != null ) {
				m_StartPntToolVecData = startPntToolVecData;
			}
			m_IsToolVecReverse = isToolVecReverse;
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

		public ToolVecModifyMap ToolVecModifyMap
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
				m_ToolVecModifyMap2[ index ] = new ToolVecModifyData( dRA_deg, dRB_deg, master_deg, slave_deg, m_ToolVecModifyMap2[ index ].InterpolateType );
			}
			else {
				m_ToolVecModifyMap2.Add( index, new ToolVecModifyData( dRA_deg, dRB_deg, master_deg, slave_deg, interpolateType ) );
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
				ToolVecModifyMap[ nNextIdx ].InterpolateType = interpolateType;
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

			if( currentIdx > StartPntIdx ) {


				// find the smallest key that is greater than currentIdx till the end
				foreach( int k in ToolVecModifyMap.Keys ) {
					if( k > currentIdx ) {
						nextIdx = k;
						found = true;
						break;
					}
				}

				// cant find, then find the smallest key that is smaller than start point index
				if( found == false ) {
					foreach( int k in ToolVecModifyMap.Keys ) {
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

			else {

				foreach( int k in ToolVecModifyMap.Keys ) {
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
			preIdx = -1;
			bool found = false;

			if( currentIdx > StartPntIdx ) {

				//find the biggest key that is smaller than currentIdx and bigger than start point index
				foreach( int k in ToolVecModifyMap.Keys ) {
					if( k > currentIdx ) {
						break;
					}
					if( k > StartPntIdx && k < currentIdx ) {
						preIdx = k;
						found = true;
					}
				}
			}
			else {
				// find the biggest key that is smaller than currentIdx
				foreach( int k in ToolVecModifyMap.Keys ) {
					if( k < currentIdx ) {
						preIdx = k;
						found = true;
					}
				}

				// find th biggest key that is bigger than start point index
				if( found == false ) {
					foreach( int k in ToolVecModifyMap.Keys ) {
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
		ToolVecModifyMap m_ToolVecModifyMap2 = new ToolVecModifyMap();
		StartPntToolVecParam m_StartPntToolVecData = new StartPntToolVecParam();
		bool m_IsToolVecReverse = false;
		TraverseData m_TraverseData = new TraverseData();
		gp_Trsf m_CumulativeTrsfMatrix = new gp_Trsf();
		double m_CompensatedDistance = 0;
	}


}