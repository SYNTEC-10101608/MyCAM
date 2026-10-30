using System;

namespace MyCAM.Data
{
	public enum ESpecialToolVecType
	{
		Normal,
		FixDir,
		AssignMS,
		ZDir,

		// keep the solved IK result and lock one rotary axis to the value of the first point
		LockMaster,
		LockSlave,
	}

	// Z rotary axis is measured on ( x as ref, y as cross ), X axis on ( y, z ) and Y axis on ( z, x )
	public enum EMachiningDir
	{
		None = 0,
		RefAxisPositive = 1, // is the phase zero axis of that plane
		CrossAxisPositive = 2, // is the RefAxis rotated by 90 degrees
		RefAxisNegative = 3,
		CrossAxisNegative = 4
	}

	public enum ToolVecMode
	{
		Interpolate,
		MachiningDirection,
		AssignMS,
		LockMasterWithoutAssign,
		LockSlaveWithoutAssign,
		LockMasterWithAssign,
		LockSlaveWithAssign,
		LockMasterWithoutAssignButAssignSlave,
		LockSlaveWithoutAssignButAssignMaster,
		LockDueAxisWithoutAssign,
	}

	public class SpecialToolVecData
	{
		public event Action PropertyChanged;

		public SpecialToolVecData(
			double? masterAngle_deg = null,
			double? slaveAngle_deg = null,
			EMachiningDir machiningDir = EMachiningDir.None,
			bool lockMaster = false,
			bool lockSlave = false )
		{
			MasterAngle_deg = masterAngle_deg;
			SlaveAngle_deg = slaveAngle_deg;
			MachiningDir = machiningDir;
			LockMaster = lockMaster;
			LockSlave = lockSlave;
		}

		public ToolVecMode Mode
		{
			get
			{
				if( MachiningDir != EMachiningDir.None ) {
					return ToolVecMode.MachiningDirection;
				}
				bool hasMasterAngle = MasterAngle_deg.HasValue;
				bool hasSlaveAngle = SlaveAngle_deg.HasValue;
				if( LockMaster && LockSlave && hasMasterAngle && hasSlaveAngle ) {
					return ToolVecMode.AssignMS;
				}
				if( LockMaster && LockSlave && !hasMasterAngle && !hasSlaveAngle ) {
					return ToolVecMode.LockDueAxisWithoutAssign;
				}
				if( LockMaster && !LockSlave && !hasMasterAngle ) {
					return ToolVecMode.LockMasterWithoutAssign;
				}
				if( LockSlave && !LockMaster && !hasSlaveAngle ) {
					return ToolVecMode.LockSlaveWithoutAssign;
				}
				if( LockMaster && !LockSlave && hasMasterAngle && !hasSlaveAngle ) {
					return ToolVecMode.LockMasterWithAssign;
				}
				if( LockSlave && !LockMaster && hasSlaveAngle && !hasMasterAngle ) {
					return ToolVecMode.LockSlaveWithAssign;
				}
				if( LockMaster && LockSlave && !hasMasterAngle && hasSlaveAngle ) {
					return ToolVecMode.LockMasterWithoutAssignButAssignSlave;
				}
				if( LockMaster && LockSlave && hasMasterAngle && !hasSlaveAngle ) {
					return ToolVecMode.LockSlaveWithoutAssignButAssignMaster;
				}
				return ToolVecMode.Interpolate;
			}
		}

		public double? MasterAngle_deg
		{
			get { return m_MasterAngle_deg; }
			set
			{
				if( !Nullable.Equals( m_MasterAngle_deg, value ) ) {
					m_MasterAngle_deg = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public double? SlaveAngle_deg
		{
			get { return m_SlaveAngle_deg; }
			set
			{
				if( !Nullable.Equals( m_SlaveAngle_deg, value ) ) {
					m_SlaveAngle_deg = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public bool LockMaster
		{
			get { return m_LockMaster; }
			set
			{
				if( m_LockMaster != value ) {
					m_LockMaster = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public bool LockSlave
		{
			get { return m_LockSlave; }
			set
			{
				if( m_LockSlave != value ) {
					m_LockSlave = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public EMachiningDir MachiningDir
		{
			get { return m_MachiningDir; }
			set
			{
				if( m_MachiningDir != value ) {
					m_MachiningDir = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public SpecialToolVecData Clone()
		{
			return new SpecialToolVecData(
				MasterAngle_deg,
				SlaveAngle_deg,
				MachiningDir,
				LockMaster,
				LockSlave );
		}

		EMachiningDir m_MachiningDir = EMachiningDir.None;
		double? m_MasterAngle_deg;
		double? m_SlaveAngle_deg;
		bool m_LockMaster;
		bool m_LockSlave;
	}
}
