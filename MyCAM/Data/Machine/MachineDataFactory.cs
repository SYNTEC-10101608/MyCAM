using OCC.gp;

namespace MyCAM.Data
{
	internal static class MachineDataFactory
	{
		// default five-axis spindle-type machine data
		public static SpindleTypeMachineData CreateDefaultSpindle()
		{
			SpindleTypeMachineData spindleData = new SpindleTypeMachineData()
			{
				ToolDirection = ToolDirection.Z,
				MasterRotaryAxis = RotaryAxis.Z,
				SlaveRotaryAxis = RotaryAxis.X,
				MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 ),
				SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 ),
				ToolToSlaveVec = new gp_Vec( -101.2, -0.19, 169.43 ),
				SlaveToMasterVec = new gp_Vec( -151.5, 0.190, 193.550 ),
			};

			// cal rotate center by itself
			spindleData.InitRotateAxes(
				RotaryDirection.RightHand, 0, 0,
				RotaryDirection.LeftHand, 0, 0 );
			return spindleData;
		}

		// brand-new default articulated robot machine data
		public static ArticRobotMachineData CreateDefaultArticRobot()
		{
			return new ArticRobotMachineData();
		}
	}
}
