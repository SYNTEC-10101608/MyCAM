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
		MasterNormalSlaveInterpolation,
		MasterInterpolationSlaveNormal,
	}

	public class StartPntToolVecParam
	{
		public Action PropertyChanged;

		public ToolVecModifyData StartPnt
		{
			get
			{
				if( m_StartPnt == null ) {
					m_StartPnt = new ToolVecModifyData();
				}
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
				if( m_EndPnt == null ) {
					m_EndPnt = new ToolVecModifyData();
				}
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

		// init both start and end point as null ToolVecAngleData, and also with default interpolation type
		public StartPntToolVecParam()
		{
			StartPnt = new ToolVecModifyData();
			EndPnt = new ToolVecModifyData();
		}

		public StartPntToolVecParam( ToolVecModifyData startPntData, ToolVecModifyData endPntData )
		{
			StartPnt = startPntData;
			EndPnt = endPntData;
		}

		ToolVecModifyData m_EndPnt;
		ToolVecModifyData m_StartPnt;
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
			Dictionary<int, ToolVecModifyData> toolVecModifyMap2,
			StartPntToolVecParam startPntToolVecData,
			bool isToolVecReverse,
			TraverseData traverseData,
			Dictionary<int, ContourEditData> contourEditMap,
			Dictionary<int, double> microJointStartIdxMap )
		{
			m_TechLayer = techLayer;
			m_StartPointIndex = startPoint;
			m_IsPathReverse = isPathReverse;
			m_LeadData = leadData;
			m_OverCutLength = overCutLength;
			m_MicroJointStartIdxMap = microJointStartIdxMap;
			if( toolVecModifyMap2 != null ) {
				foreach( var kvp in toolVecModifyMap2 ) {
					m_ToolVecModifyMap.Add( kvp.Key, kvp.Value.Clone() );
				}
			}
			if( startPntToolVecData != null ) {
				m_StartPntToolVecData = startPntToolVecData;
			}
			m_ContourEditMap = new Dictionary<int, ContourEditData>();
			if( contourEditMap != null ) {
				foreach( var kvp in contourEditMap ) {
					m_ContourEditMap.Add( kvp.Key, kvp.Value.Clone() );
				}
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

		public Dictionary<int, double> MicroJointStartIdxMap
		{
			get
			{
				if( m_MicroJointStartIdxMap == null ) {
					m_MicroJointStartIdxMap = new Dictionary<int, double>();
				}
				return m_MicroJointStartIdxMap;
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
					ClearToolVecModify();
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
		public void SetToolVecModify( int index, double dRA_deg, double dRB_deg, double master_deg, double slave_deg, EToolVecInterpolateType interpolateType = EToolVecInterpolateType.Normal )
		{
			ToolVecAngleData angleData = new ToolVecAngleData( dRA_deg, dRB_deg, master_deg, slave_deg );
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap[ index ] = new ToolVecModifyData( angleData, m_ToolVecModifyMap[ index ].InterpolateType );
			}
			else {
				m_ToolVecModifyMap.Add( index, new ToolVecModifyData( angleData, interpolateType ) );
			}
			CAMFactorChanged?.Invoke();
		}

		public void RemoveToolVecModify( int index )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				if( IsPathReverse == false ) {

					// the interpolat type will be set to next modify idx 
					bool isFoundNext = FindNextMapIndex( index, out int nNextIdx );
					if( isFoundNext ) {
						m_ToolVecModifyMap.Remove( index, nNextIdx );
					}
					else {
						EToolVecInterpolateType removedType = m_ToolVecModifyMap[ index ].InterpolateType;
						StartPntToolVecData.EndPnt.InterpolateType = removedType;
						m_ToolVecModifyMap.Remove( index );
					}
				}
				else {

					// this region interpolate type in revese case is record at pre index
					// remove this index means pre region will include this region, so type do not have to change )
					m_ToolVecModifyMap.Remove( index );
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
				if( IsPathReverse ) {

					// first region is record on first pnt (in reverse case this region type is recorded at preidx)
					StartPntToolVecData.StartPnt.InterpolateType = interpolateType;
				}

				// last region is record on end pnt
				else {
					StartPntToolVecData.EndPnt.InterpolateType = interpolateType;
				}
			}
			CAMFactorChanged?.Invoke();

		}

		public void ClearToolVecModify()
		{
			m_ToolVecModifyMap.Clear();
			StartPntToolVecData = new StartPntToolVecParam();
			CAMFactorChanged?.Invoke();
		}

		public bool IsStartPntModified( bool isStartIdx, out ToolVecAngleData toolVecAngleData, out EToolVecInterpolateType type )
		{
			toolVecAngleData = null;
			type = EToolVecInterpolateType.Normal;
			if( isStartIdx ) {
				type = StartPntToolVecData.StartPnt.InterpolateType;
				if( StartPntToolVecData != null && StartPntToolVecData.StartPnt != null && StartPntToolVecData.StartPnt.AngleData != null ) {
					toolVecAngleData = StartPntToolVecData.StartPnt.AngleData;
					return true;
				}
				return false;
			}

			// is end idx
			type = StartPntToolVecData.EndPnt.InterpolateType;
			if( StartPntToolVecData != null && StartPntToolVecData.EndPnt != null && StartPntToolVecData.EndPnt.AngleData != null ) {
				toolVecAngleData = StartPntToolVecData.EndPnt.AngleData;
				return true;
			}
			return false;
		}

		// let path know this region type (region type is record at the end of region )
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

		public void ClearMicroJointStartIdx()
		{
			m_MicroJointStartIdxMap.Clear();
			CAMFactorChanged?.Invoke();
		}

		public void AddMicroJointStartIdx( int index, double length )
		{
			m_MicroJointStartIdxMap[ index ] = length;
			CAMFactorChanged?.Invoke();
		}

		public void RemoveMicroJointStartIdx( int index )
		{
			if( m_MicroJointStartIdxMap.ContainsKey( index ) ) {
				m_MicroJointStartIdxMap.Remove( index );
				CAMFactorChanged?.Invoke();
			}
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

		public double InitMaster_rad
		{
			get
			{
				return m_InitMaster_rad;
			}
			set
			{
				m_InitMaster_rad = value;
			}
		}

		public double InitSlave_rad
		{
			get
			{
				return m_InitSlave_rad;
			}
			set
			{
				m_InitSlave_rad = value;
			}
		}

		const int DEFAULT_TECH_LAYER = 1;
		int m_TechLayer = DEFAULT_TECH_LAYER;
		int m_StartPointIndex = 0;
		Dictionary<int, double> m_MicroJointStartIdxMap = new Dictionary<int, double>();
		bool m_IsPathReverse = false;
		LeadData m_LeadData = new LeadData();
		double m_OverCutLength = 0;
		ToolVecModifyMap m_ToolVecModifyMap = new ToolVecModifyMap();
		StartPntToolVecParam m_StartPntToolVecData = new StartPntToolVecParam();
		bool m_IsToolVecReverse = false;
		TraverseData m_TraverseData = new TraverseData();
		gp_Trsf m_CumulativeTrsfMatrix = new gp_Trsf();
		double m_CompensatedDistance = 0;
		double m_InitMaster_rad = 0;
		double m_InitSlave_rad = 0;
		Dictionary<int, ContourEditData> m_ContourEditMap = new Dictionary<int, ContourEditData>();
	}


}
