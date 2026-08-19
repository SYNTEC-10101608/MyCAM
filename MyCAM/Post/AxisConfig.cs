using MyCAM.Data;
using MyCAM.Helper;

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

		public AxisConfig( FiveAxisMachineData machineData )
		{
			MasterRotaryAxis = machineData.MasterRotaryAxis;
			SlaveRotaryAxis = machineData.SlaveRotaryAxis;
			MasterAxisName = RotaryAxisHelper.ConvertRotaryAxisName( MasterRotaryAxis );
			SlaveAxisName = RotaryAxisHelper.ConvertRotaryAxisName( SlaveRotaryAxis );
		}
	}
}
