namespace MyCAM.Data
{
	public enum FiveAxisType
	{
		None = 0,
		Spindle = 1,
		Table = 2,
		Mix = 3,

		// single rotary axis machine, file format code only, runtime uses MixTypeMachineData with MixTypeVariant
		SingleSpindle = 4,
		SingleTable = 5,
	}

	// mix type specialization: MasterOnly locks slave axis at 0, SlaveOnly locks master axis at 0
	public enum MixTypeVariant
	{
		Mix = 0,
		MasterOnly = 1,
		SlaveOnly = 2,
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


	//   it's idx value will be used directly as an array index in the simulation hot path
	public enum MachineComponentType
	{
		UnKnow = -1,
		XAxis = 0,
		YAxis = 1,
		ZAxis = 2,
		Master = 3,
		Slave = 4,
		Laser = 5,
		WorkPiece = 6,
		Base = 7,

		// articulated robot joints
		Joint1 = 10,
		Joint2 = 11,
		Joint3 = 12,
		Joint4 = 13,
		Joint5 = 14,
		Joint6 = 15,

		// T0: flange face frame (ax3)
		T0_Flange = 16,

		// T1: tool coordinate frame (ax3)
		T1_ToolFrame = 17,
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
		public const string SIMULATION_OFFSET_X = "Simulation_Offset_X";
		public const string SIMULATION_OFFSET_Y = "Simulation_Offset_Y";
		public const string SIMULATION_OFFSET_Z = "Simulation_Offset_Z";
		public const string FULL_MACHINE_SIMULATION = "Full_Machine_Simulation";
		public const string TUBE_DIR = "Tube_Dir";
	}
}
