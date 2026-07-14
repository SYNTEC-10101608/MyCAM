using MyCAM.Data;

namespace MyCAM.Post
{
	internal class AxisConfig
	{
		public string MasterAxisName
		{
			get;
		}

		public string SlaveAxisName
		{
			get;
		}

		public RotaryAxis MasterRotaryAxis
		{
			get;
		}
		public RotaryAxis SlaveRotaryAxis
		{
			get;
		}

		public AxisConfig( MachineData machineData )
		{
			MasterRotaryAxis = machineData.MasterRotaryAxis;
			SlaveRotaryAxis = machineData.SlaveRotaryAxis;
			MasterAxisName = ConvertRotaryAxisName( MasterRotaryAxis );
			SlaveAxisName = ConvertRotaryAxisName( SlaveRotaryAxis );
		}

		string ConvertRotaryAxisName( RotaryAxis axis )
		{
			switch( axis ) {
				case RotaryAxis.X:
					return "A";
				case RotaryAxis.Y:
					return "B";
				case RotaryAxis.Z:
					return "C";
				default:
					return "";
			}
		}
	}
}
