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

		public StartPntToolVecParam Clone()
		{
			return new StartPntToolVecParam( StartPnt.Clone(), EndPnt.Clone() );
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

	public class StartPntContourEditParam
	{
		public ContourEditData StartPnt
		{
			get;
			set;
		}

		public ContourEditData EndPnt
		{
			get;
			set;
		}

		public StartPntContourEditParam()
		{
			StartPnt = new ContourEditData();
			EndPnt = new ContourEditData();
		}

		public StartPntContourEditParam( ContourEditData startPntData, ContourEditData endPntData )
		{
			StartPnt = startPntData;
			EndPnt = endPntData;
		}
	}

	public enum ExtendedGCodeType
	{
		TraverseHead,
		TraverseTail,
		PathHead,
		PathTail,
		ControlPoint,
	}

	public class RCornerData
	{
		public int StartIndex
		{
			get; set;
		}

		public int EndIndex
		{
			get; set;
		}

		public RCornerData( int startIndex, int endIndex )
		{
			StartIndex = startIndex;
			EndIndex = endIndex;
		}

		public RCornerData Clone()
		{
			return new RCornerData( StartIndex, EndIndex );
		}
	}

	// Assigned external rotary positions, in degrees; copied by value between paths and points.
	public struct ExternalAxisPos : IEquatable<ExternalAxisPos>
	{
		public ExternalAxisPos( double ext1, double ext2 )
		{
			if( double.IsNaN( ext1 ) || double.IsInfinity( ext1 ) ) {
				throw new ArgumentOutOfRangeException( nameof( ext1 ), ext1, "External axis angle must be finite." );
			}
			if( double.IsNaN( ext2 ) || double.IsInfinity( ext2 ) ) {
				throw new ArgumentOutOfRangeException( nameof( ext2 ), ext2, "External axis angle must be finite." );
			}
			Ext1 = ext1;
			Ext2 = ext2;
		}

		public double Ext1
		{
			get;
		}

		public double Ext2
		{
			get;
		}

		public bool Equals( ExternalAxisPos other )
		{
			return Ext1.Equals( other.Ext1 ) && Ext2.Equals( other.Ext2 );
		}

		public override bool Equals( object obj )
		{
			return obj is ExternalAxisPos other && Equals( other );
		}

		public override int GetHashCode()
		{
			return Ext1.GetHashCode() ^ Ext2.GetHashCode();
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
			bool processPath,
			int startPoint,
			bool isPathReverse,
			LeadData leadData,
			double overCutLength,
			Dictionary<int, ToolVecModifyData> toolVecModifyMap2,
			StartPntToolVecParam startPntToolVecData,
			bool isToolVecReverse,
			TraverseData traverseData,
			StartPntContourEditParam startPntContourEditData,
			Dictionary<int, ContourEditData> contourEditMap,
			Dictionary<int, double> microJointStartIdxMap,
			Dictionary<Tuple<ExtendedGCodeType, int?>, string> extendedGCodeMap,
			SpecialToolVecData specialToolVecData = null,
			Dictionary<int, RCornerData> rCornerMap = null )
		{
			m_TechLayer = techLayer;
			m_ProcessPath = processPath;
			m_StartPointIndex = startPoint;
			m_IsPathReverse = isPathReverse;
			m_LeadData = leadData;
			m_OverCutLength = overCutLength;
			m_MicroJointStartIdxMap = microJointStartIdxMap;
			m_extendedGCodeMap = extendedGCodeMap;
			if( toolVecModifyMap2 != null ) {
				foreach( var kvp in toolVecModifyMap2 ) {
					m_ToolVecModifyMap.Add( kvp.Key, kvp.Value.Clone() );
				}
			}
			m_StartPntContourEditData = startPntContourEditData;
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
			if( specialToolVecData != null ) {
				m_SpecialToolVecData = specialToolVecData;
			}
			else {
				m_SpecialToolVecData = new SpecialToolVecData();
			}
			if( rCornerMap != null ) {
				foreach( var kvp in rCornerMap ) {
					m_RCornerMap.Add( kvp.Key, kvp.Value.Clone() );
				}
			}
			SubscribeSubParamChanged();
		}

		public Action CAMFactorChanged;
		public Action CADFactorChanged;

		public ExternalAxisPos ExternalAxisPos
		{
			get
			{
				return m_ExternalAxisPos;
			}
			set
			{
				if( !m_ExternalAxisPos.Equals( value ) ) {
					m_ExternalAxisPos = value;
					CAMFactorChanged?.Invoke();
				}
			}
		}

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

		public bool ProcessPath
		{
			get
			{
				return m_ProcessPath;
			}
			 set
			{
				if( m_ProcessPath != value ) {
					m_ProcessPath = value;
				}
			}
		}

		public int StartPointIndex
		{
			get
			{
				return m_StartPointIndex;
			}
			private set
			{
				if( m_StartPointIndex != value ) {
					m_StartPointIndex = value;
					CAMFactorChanged?.Invoke();
				}
			}
		}

		public void SetStartPntIndex( int index )
		{
			if( StartPointIndex != index ) {
				ClearStartPntContourEditParam();
				StartPointIndex = index;
				ClearToolVecModify();
				ClearSpecialToolVecData();
				ClearExtendedGCode();
				ClearRCorner();
			}
		}
		public void SetToolVecReverse( bool isPathReverse )
		{
			if( IsToolVecReverse != isPathReverse ) {
				IsToolVecReverse = isPathReverse;
				ClearToolVecModify();
				ClearSpecialToolVecData();
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

		// R corner ranges, keyed by StartIndex; endpoints are real CAD indices, or PATH_END for the machining end point
		public IReadOnlyDictionary<int, RCornerData> RCornerMap
		{
			get
			{
				if( m_RCornerMap == null ) {
					m_RCornerMap = new Dictionary<int, RCornerData>();
				}
				return m_RCornerMap;
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
					ClearStartPntContourEditParam();
					m_IsPathReverse = value;
					ClearToolVecModify();
					ClearExtendedGCode();
					ClearRCorner();
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
			private set
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
			set
			{
				if( m_ToolVecModifyMap != value ) {
					if( m_ToolVecModifyMap != null ) {
						m_ToolVecModifyMap.PropertyChanged -= SubParamChanged;
					}
					m_ToolVecModifyMap = value;
					if( m_ToolVecModifyMap != null ) {
						m_ToolVecModifyMap.PropertyChanged += SubParamChanged;
					}
					CAMFactorChanged?.Invoke();
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
					CAMFactorChanged?.Invoke();
				}
			}
		}

		public SpecialToolVecData SpecialToolVecData
		{
			get
			{
				// to prevent null value
				if( m_SpecialToolVecData == null ) {
					m_SpecialToolVecData = new SpecialToolVecData();
					m_IsSpecialToolVecMode = false;
					m_SpecialToolVecData.PropertyChanged += SpecialToolVecChanged;
				}
				return m_SpecialToolVecData;
			}
		}

		public void SetSpecialToolVecData( SpecialToolVecData specialToolVecData )
		{
			if( specialToolVecData == null ) {
				return;
			}
			if( specialToolVecData.MasterAngle_deg == m_SpecialToolVecData.MasterAngle_deg &&
				 specialToolVecData.SlaveAngle_deg == m_SpecialToolVecData.SlaveAngle_deg &&
				 specialToolVecData.LockMaster == m_SpecialToolVecData.LockMaster &&
				 specialToolVecData.LockSlave == m_SpecialToolVecData.LockSlave &&
				 specialToolVecData.MachiningDir == m_SpecialToolVecData.MachiningDir ) {
				return;
			}
			m_SpecialToolVecData.PropertyChanged -= SpecialToolVecChanged;
			m_SpecialToolVecData = specialToolVecData;
			m_SpecialToolVecData.PropertyChanged += SpecialToolVecChanged;
			m_IsSpecialToolVecMode = m_SpecialToolVecData.Mode != ToolVecMode.Interpolate;
			if( m_IsSpecialToolVecMode ) {
				ClearToolVecModify();
			}
			CAMFactorChanged?.Invoke();
		}

		// API for outside modification
		public void ChangeToolVecModify( int index, double dRA_deg, double dRB_deg, double master_deg, double slave_deg )
		{
			if( m_ToolVecModifyMap == null || !m_ToolVecModifyMap.ContainsKey( index ) ) {
				return;
			}
			ToolVecAngleData angleData = new ToolVecAngleData( dRA_deg, dRB_deg, master_deg, slave_deg );
			m_ToolVecModifyMap[ index ] = new ToolVecModifyData( angleData, m_ToolVecModifyMap[ index ].InterpolateType );
		}

		public void AddToolVecModify( int index, double dRA_deg, double dRB_deg, double master_deg, double slave_deg, EToolVecInterpolateType interpolateType = EToolVecInterpolateType.Normal )
		{
			if( m_ToolVecModifyMap == null ) {
				return;
			}
			ToolVecAngleData angleData = new ToolVecAngleData( dRA_deg, dRB_deg, master_deg, slave_deg );
			m_ToolVecModifyMap.Add( index, new ToolVecModifyData( angleData, interpolateType ) );
		}

		public void RemoveCtrlPntAndTransInterpolation( int nPntIdx, int nNextIdx )
		{
			if( m_ToolVecModifyMap == null || !m_ToolVecModifyMap.ContainsKey( nPntIdx ) ) {
				return;
			}
			m_ToolVecModifyMap.RemoveAndTransferInterpolation( nPntIdx, nNextIdx );
		}

		public void RemoveCtrlPnt( int nPntIdx )
		{
			if( m_ToolVecModifyMap == null || !m_ToolVecModifyMap.ContainsKey( nPntIdx ) ) {
				return;
			}
			m_ToolVecModifyMap.Remove( nPntIdx );
		}

		public void SetInterpolationTypeAtPnt( int nCtrlPntIdx, EToolVecInterpolateType interpolateType )
		{
			if( m_ToolVecModifyMap == null || !m_ToolVecModifyMap.ContainsKey( nCtrlPntIdx ) ) {
				return;
			}
			if( m_ToolVecModifyMap[ nCtrlPntIdx ].InterpolateType == interpolateType ) {
				return;
			}
			m_ToolVecModifyMap[ nCtrlPntIdx ].InterpolateType = interpolateType;
			CAMFactorChanged?.Invoke();
		}

		// change start end pnt type should trigger CAMFactorChanged
		public void SetInterpolationTypeAtStartPnt( bool isStartIdx, EToolVecInterpolateType interpolateType )
		{
			if( StartPntToolVecData == null ) {
				StartPntToolVecData = new StartPntToolVecParam();
			}
			if( isStartIdx ) {
				if( StartPntToolVecData.StartPnt.InterpolateType == interpolateType ) {
					return;
				}
				StartPntToolVecData.StartPnt.InterpolateType = interpolateType;
			}
			else {
				if( StartPntToolVecData.EndPnt.InterpolateType == interpolateType ) {
					return;
				}
				StartPntToolVecData.EndPnt.InterpolateType = interpolateType;
			}
			CAMFactorChanged?.Invoke();
		}

		public void ClearToolVecModify()
		{
			m_ToolVecModifyMap.Clear();
			StartPntToolVecData = new StartPntToolVecParam();
			CAMFactorChanged?.Invoke();
		}

		public void ClearSpecialToolVecData()
		{
			if( m_SpecialToolVecData != null ) {
				m_SpecialToolVecData.PropertyChanged -= SpecialToolVecChanged;
			}
			m_SpecialToolVecData = new SpecialToolVecData();
			m_IsSpecialToolVecMode = false;
			m_SpecialToolVecData.PropertyChanged += SpecialToolVecChanged;
			CAMFactorChanged?.Invoke();
		}

		public void ClearExtendedGCode()
		{
			Dictionary<Tuple<ExtendedGCodeType, int?>, string> keepMap = new Dictionary<Tuple<ExtendedGCodeType, int?>, string>();

			foreach( KeyValuePair<Tuple<ExtendedGCodeType, int?>, string> pair in m_extendedGCodeMap ) {
				if( pair.Key.Item1 != ExtendedGCodeType.ControlPoint ) {
					keepMap.Add( pair.Key, pair.Value );
				}
			}

			m_extendedGCodeMap.Clear();
			foreach( KeyValuePair<Tuple<ExtendedGCodeType, int?>, string> pair in keepMap ) {
				m_extendedGCodeMap.Add( pair.Key, pair.Value );
			}

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

		#region ContourEdit

		public Dictionary<int, ContourEditData> ContourEditMap
		{
			get
			{
				return m_ContourEditMap;
			}
		}
		Dictionary<int, ContourEditData> m_ContourEditMap = new Dictionary<int, ContourEditData>();

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

		public StartPntContourEditParam StartPntContourEditData
		{
			get
			{
				if( m_StartPntContourEditData == null ) {
					m_StartPntContourEditData = new StartPntContourEditParam();
				}

				return m_StartPntContourEditData;
			}
		}
		StartPntContourEditParam m_StartPntContourEditData;

		public void SetStartPntContourEdit( bool isStartPnt, double dx, double dy, double dz )
		{
			if( m_StartPntContourEditData == null ) {
				m_StartPntContourEditData = new StartPntContourEditParam();
			}

			if( isStartPnt ) {
				m_StartPntContourEditData.StartPnt = new ContourEditData( dx, dy, dz );
			}
			else {
				m_StartPntContourEditData.EndPnt = new ContourEditData( dx, dy, dz );
			}

			CADFactorChanged?.Invoke();
		}

		public void ClearStartPntContourEditParam()
		{
			if( m_StartPntContourEditData == null ) {
				return;
			}
			m_StartPntContourEditData = null;
			CADFactorChanged?.Invoke();
		}

		#endregion

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

		#region ExtendedGCode

		public Dictionary<Tuple<ExtendedGCodeType, int?>, string> ExtendedGCodeMap
		{
			get
			{
				if( m_extendedGCodeMap == null ) {
					m_extendedGCodeMap = new Dictionary<Tuple<ExtendedGCodeType, int?>, string>();
				}
				return m_extendedGCodeMap;
			}
		}

		// the int? is the CADIndex, is null when gCodeType is not ControlPoint
		Dictionary<Tuple<ExtendedGCodeType, int?>, string> m_extendedGCodeMap = new Dictionary<Tuple<ExtendedGCodeType, int?>, string>();

		public void SetExtendedGCode( ExtendedGCodeType gCodeType, int? pointIdx, string content )
		{
			if( gCodeType != ExtendedGCodeType.ControlPoint ) {
				pointIdx = null;
			}

			if( m_extendedGCodeMap.ContainsKey( Tuple.Create( gCodeType, pointIdx ) ) ) {
				m_extendedGCodeMap[ Tuple.Create( gCodeType, pointIdx ) ] = content;
			}
			else {
				m_extendedGCodeMap.Add( Tuple.Create( gCodeType, pointIdx ), content );
			}

			CAMFactorChanged?.Invoke();
		}

		public void RemoveExtendedGCode( ExtendedGCodeType gCodeType, int? pointIdx )
		{
			if( gCodeType != ExtendedGCodeType.ControlPoint ) {
				pointIdx = null;
			}

			if( m_extendedGCodeMap.ContainsKey( Tuple.Create( gCodeType, pointIdx ) ) ) {
				m_extendedGCodeMap.Remove( Tuple.Create( gCodeType, pointIdx ) );
				CAMFactorChanged?.Invoke();
			}
		}

		#endregion

		#region RCorner

		public bool ApplyRCornerChange( IEnumerable<RCornerData> rangesToRemove, RCornerData rangeToAdd )
		{
			if( rangeToAdd == null ) {
				return false;
			}

			HashSet<int> keysToRemove = new HashSet<int>();
			if( rangesToRemove != null ) {
				foreach( RCornerData range in rangesToRemove ) {
					if( range != null ) {
						keysToRemove.Add( range.StartIndex );
					}
				}
			}

			if( m_RCornerMap.ContainsKey( rangeToAdd.StartIndex ) && !keysToRemove.Contains( rangeToAdd.StartIndex ) ) {
				return false;
			}

			foreach( int key in keysToRemove ) {
				m_RCornerMap.Remove( key );
			}
			m_RCornerMap[ rangeToAdd.StartIndex ] = rangeToAdd;
			CAMFactorChanged?.Invoke();
			return true;
		}

		public bool RemoveRCorner( RCornerData range )
		{
			if( range != null && m_RCornerMap.Remove( range.StartIndex ) ) {
				CAMFactorChanged?.Invoke();
				return true;
			}
			return false;
		}

		public void ClearRCorner()
		{
			m_RCornerMap.Clear();
			CAMFactorChanged?.Invoke();
		}

		#endregion

		void SubscribeSubParamChanged()
		{
			m_LeadData.PropertyChanged += SubParamChanged;
			m_TraverseData.PropertyChanged += SubParamChanged;
			m_StartPntToolVecData.PropertyChanged += SubParamChanged;
			m_ToolVecModifyMap.PropertyChanged += SubParamChanged;
			m_IsSpecialToolVecMode = m_SpecialToolVecData.Mode != ToolVecMode.Interpolate;
			m_SpecialToolVecData.PropertyChanged += SpecialToolVecChanged;
		}

		void SpecialToolVecChanged()
		{
			// current is special
			bool isSpecial = m_SpecialToolVecData.Mode != ToolVecMode.Interpolate;

			//  m_IsSpecialToolVecMode is the previous state
			bool enteredSpecialMode = isSpecial && !m_IsSpecialToolVecMode;
			m_IsSpecialToolVecMode = isSpecial;

			// change from normal to special mode, clear tool vec modify
			if( enteredSpecialMode ) {
				ClearToolVecModify();
			}
			CAMFactorChanged?.Invoke();
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
		bool m_ProcessPath = true;
		int m_StartPointIndex = 0;
		Dictionary<int, double> m_MicroJointStartIdxMap = new Dictionary<int, double>();
		Dictionary<int, RCornerData> m_RCornerMap = new Dictionary<int, RCornerData>();
		bool m_IsPathReverse = false;
		LeadData m_LeadData = new LeadData();
		double m_OverCutLength = 0;
		ToolVecModifyMap m_ToolVecModifyMap = new ToolVecModifyMap();
		StartPntToolVecParam m_StartPntToolVecData = new StartPntToolVecParam();
		bool m_IsToolVecReverse = false;
		TraverseData m_TraverseData = new TraverseData();
		SpecialToolVecData m_SpecialToolVecData = new SpecialToolVecData();
		bool m_IsSpecialToolVecMode;
		gp_Trsf m_CumulativeTrsfMatrix = new gp_Trsf();
		double m_CompensatedDistance = 0;
		double m_InitMaster_rad = 0;
		double m_InitSlave_rad = 0;
		ExternalAxisPos m_ExternalAxisPos;
	}
}
