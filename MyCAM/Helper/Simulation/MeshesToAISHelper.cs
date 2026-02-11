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

	public enum MeshesToAISResult
	{
		Success,
		ArgumentNull,
		InvalidMesh,
		UnknownError
	}

	internal class MeshesToAISHelper
	{
		public static bool ConvertMeshesListToAIS( MachineMeshes machineMeshes, out MachineAIS machineAppearance )
		{
			machineAppearance = new MachineAIS();
			if( machineMeshes == null ) {
				AnnounceWrning( MeshesToAISResult.ArgumentNull );
				return false;
			}
			foreach( KeyValuePair<MachineComponentType, Poly_Triangulation> keyValue in machineMeshes.Meshes ) {

				// do not have mash data
				if( keyValue.Value == null ) {
					AnnounceWrning( MeshesToAISResult.InvalidMesh, keyValue.Key );
					continue;
				}
				if( machineAppearance.AISList.ContainsKey( keyValue.Key ) == false ) {
					AnnounceWrning( MeshesToAISResult.InvalidMesh, keyValue.Key );
					continue;
				}
				machineAppearance.AISList[ keyValue.Key ] = ConvertMeshToAIS( keyValue.Value );
			}
			return true;
		}

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

		static void AnnounceWrning( MeshesToAISResult result, MachineComponentType componentType = MachineComponentType.UnKnow )
		{
			if( result != MeshesToAISResult.Success ) {
				switch( result ) {
					case MeshesToAISResult.ArgumentNull:
						MyApp.Logger.ShowOnLogPanel( $"機構網格資訊錯誤", MyApp.NoticeType.Warning );
						break;
					case MeshesToAISResult.InvalidMesh:
						//MyApp.Logger.ShowOnLogPanel( $"{componentType} 原始網格資訊讀取失敗", MyApp.NoticeType.Warning );
						break;
					case MeshesToAISResult.UnknownError:
					default:
						//MyApp.Logger.ShowOnLogPanel( $"{componentType} STL讀取失敗，發生未知錯誤", MyApp.NoticeType.Warning );
						break;
				}
				return;
			}
		}
	}
}
