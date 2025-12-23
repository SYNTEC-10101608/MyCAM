namespace MyCAM.Data
{
	public enum FiveAxisType
	{
		None = 0,
		Spindle = 1,
		Table = 2,
		Mix = 3,
	}

	public enum ToolDirection
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 3,
	}

	public enum RotaryAxis
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 3,
	}

	public enum RotaryDirection
	{
		None = 0,
		RightHand = 1,
		LeftHand = 2,
	}

	public enum MachineComponentType
	{
		Base = 0,
		XAxis = 1,
		YAxis = 2,
		ZAxis = 3,
		Master = 4,
		Slave = 5,
		WorkPiece = 6,
		Tool = 7,
	}

	public enum MachinePrValue
	{
		FiveAxisType = 3001,
		ToolDirection = 3002,
		MasterRotaryAxis = 3005,
		SlaveRotaryAxis = 3006,
		MasterRotaryDirection = 3007,
		SlaveRotaryDirection = 3008,
		ToolLength = 3013,
		MasterTiltedVec_X = 3015,
		MasterTiltedVec_Y = 3016,
		MasterTiltedVec_Z = 3017,
		SlaveTiltedVec_X = 3018,
		SlaveTiltedVec_Y = 3019,
		SlaveTiltedVec_Z = 3020,

		// spindle type
		ToolToSlaveVec_X = 3021,
		ToolToSlaveVec_Y = 3022,
		ToolToSlaveVec_Z = 3023,
		SlaveToMasterVec_X = 3024,
		SlaveToMasterVec_Y = 3025,
		SlaveToMasterVec_Z = 3026,

		// table type
		MasterToSlaveVec_X = 3031,
		MasterToSlaveVec_Y = 3032,
		MasterToSlaveVec_Z = 3033,
		MCSToMasterVec_X = 3034,
		MCSToMasterVec_Y = 3035,
		MCSToMasterVec_Z = 3036,

		// mix type
		ToolToMasterVec_X = 3041,
		ToolToMasterVec_Y = 3042,
		ToolToMasterVec_Z = 3043,
		MCSToSlaveVec_X = 3044,
		MCSToSlaveVec_Y = 3045,
		MCSToSlaveVec_Z = 3046,

		// limit
		AxialLimit = 2401,
	}

	public class MachineParamName
	{
		public const string MASTER_LIMIMT_START_PARAM_NAME = "Master_Start";
		public const string MASTER_LIMIMT_END_PARAM_NAME = "Master_End";
		public const string SLAVE_LIMIMT_START_PARAM_NAME = "Slave_Start";
		public const string SLAVE_LIMIMT_END_PARAM_NAME = "Slave_End";
	}
}
