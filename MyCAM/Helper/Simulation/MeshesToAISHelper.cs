using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCC.Graphic3d;
using OCC.Poly;
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


	internal class MeshesToAISHelper
	{
		static AIS_Triangulation ConvertMeshToAIS( Poly_Triangulation mesh )
		{
			if( mesh == null || mesh.NbNodes() <= 0 || mesh.NbTriangles() <= 0 ) {
				return null;
			}
			AIS_Triangulation resultAIS = new AIS_Triangulation( mesh );

			// set material aspect, this matter since the default material gives wrong color effect
			Graphic3d_MaterialAspect baseAspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NameOfMaterial_UserDefined );
			resultAIS.SetMaterial( baseAspect );
			return resultAIS;
		}
	}
}
