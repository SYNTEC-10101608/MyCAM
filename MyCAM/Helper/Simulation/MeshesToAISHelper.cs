using MyCAM.Data;
using OCC.AIS;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper.Simulation
{
	public sealed class MachineAIS
	{
		//per component AIS 
		public readonly Dictionary<MachineComponentType, AIS_InteractiveObject> AISList = new Dictionary<MachineComponentType, AIS_InteractiveObject>();

		public MachineAIS()
		{
			foreach( MachineComponentType componentType in Enum.GetValues( typeof( MachineComponentType ) ) ) {
				AISList[ componentType ] = null;
			}
		}
	}
}
