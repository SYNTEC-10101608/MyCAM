using MyCAM.Data;
using MyCAM.Editor;
using System;

namespace MyCAM.Helper
{
	public static class RotaryAxisHelper
	{
		public static RotaryAxisConfig CreateRotaryAxisConfig()
		{
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				throw new InvalidOperationException( "Cannot get machine data" );
			}

			RotaryAxisConfig config = new RotaryAxisConfig();
			config.MasterName = ConvertRotaryAxisName( machineData.MasterRotaryAxis );
			config.SlaveName = ConvertRotaryAxisName( machineData.SlaveRotaryAxis );

			if( machineData.FiveAxisType == FiveAxisType.Table || machineData.FiveAxisType == FiveAxisType.Mix ) {
				config.RotaryAxisName = config.SlaveName;
			}
			else {
				config.RotaryAxisName = config.MasterName;
			}

			return config;
		}

		public static string ConvertRotaryAxisName( RotaryAxis axis )
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
