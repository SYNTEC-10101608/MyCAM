namespace MyCAM.Data
{
	public enum ESpecialToolVecType
	{
		Normal,
		FixDir,
		AssignMS,
		ZDir
	}

	public class SpecialToolVecData
	{
		public SpecialToolVecData(
			ESpecialToolVecType type = ESpecialToolVecType.Normal,
			double masterAngle_deg = 0,
			double slaveAngle_deg = 0 )
		{
			Type = type;
			MasterAngle_deg = masterAngle_deg;
			SlaveAngle_deg = slaveAngle_deg;
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


		public SpecialToolVecData Clone()
		{
			return new SpecialToolVecData(
				Type,
				MasterAngle_deg,
				SlaveAngle_deg );
		}
	}
}
