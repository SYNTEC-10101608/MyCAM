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
		LockSlave
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

	public class SpecialToolVecData
	{
		public SpecialToolVecData(
			ESpecialToolVecType type = ESpecialToolVecType.Normal,
			double masterAngle_deg = 0,
			double slaveAngle_deg = 0,
			EMachiningDir machiningDir = EMachiningDir.None )
		{
			Type = type;
			MasterAngle_deg = masterAngle_deg;
			SlaveAngle_deg = slaveAngle_deg;
			MachiningDir = machiningDir;
		}

		public ESpecialToolVecType Type
		{
			get; private set;
		}

		public double MasterAngle_deg
		{
			get; private set;
		}

		public double SlaveAngle_deg
		{
			get; private set;
		}

		public EMachiningDir MachiningDir
		{
			get; private set;
		}

		public SpecialToolVecData Clone()
		{
			return new SpecialToolVecData(
				Type,
				MasterAngle_deg,
				SlaveAngle_deg,
				MachiningDir );
		}
	}
}
