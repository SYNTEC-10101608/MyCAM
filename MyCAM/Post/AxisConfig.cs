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

		// single rotary axis machine does not output the locked axis
		public bool WriteMaster
		{
			get;
		} = true;

		public bool WriteSlave
		{
			get;
		} = true;

		public AxisConfig( FiveAxisMachineData machineData )
		{
			MasterRotaryAxis = machineData.MasterRotaryAxis;
			SlaveRotaryAxis = machineData.SlaveRotaryAxis;
			MasterAxisName = RotaryAxisHelper.ConvertRotaryAxisName( MasterRotaryAxis );
			SlaveAxisName = RotaryAxisHelper.ConvertRotaryAxisName( SlaveRotaryAxis );

			// mix type specialization: the locked axis is always 0 and not written to NC file
			if( machineData is MixTypeMachineData mixTypeMachineData ) {
				if( mixTypeMachineData.MixTypeVariant == MixTypeVariant.MasterOnly ) {
					WriteSlave = false;
				}
				else if( mixTypeMachineData.MixTypeVariant == MixTypeVariant.SlaveOnly ) {
					WriteMaster = false;
				}
			}
		}
	}
}
