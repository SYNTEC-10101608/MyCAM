using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCC.Graphic3d;
using OCC.Poly;
using OCC.RWStl;
using System.Collections.Generic;
using System.IO;


namespace MyCAM.Helper
{
	public sealed class MachineAppearance
	{
		// per component mesh
		public readonly Dictionary<MachineComponentType, Poly_Triangulation> Meshes
			= new Dictionary<MachineComponentType, Poly_Triangulation>();

		//per component AIS 
		public readonly Dictionary<MachineComponentType, AIS_InteractiveObject> AisObjects
			= new Dictionary<MachineComponentType, AIS_InteractiveObject>();

		// component list
		static readonly MachineComponentType[] ManagedTypes = new[]
		{
		MachineComponentType.Base,
		MachineComponentType.XAxis,
		MachineComponentType.YAxis,
		MachineComponentType.ZAxis,
		MachineComponentType.Master,
		MachineComponentType.Slave,
		MachineComponentType.Tool,
		MachineComponentType.WorkPiece,
		};

		public MachineAppearance()
		{
			// init dictionary
			foreach( var type in ManagedTypes ) {
				Meshes[ type ] = null;
				AisObjects[ type ] = null;
			}
		}
	}

	public static class MachineMeshToAISHelper
	{
		public static bool LoadMachineAppearance( string szFolderName , out MachineAppearance machineAppearance )
		{
			 machineAppearance = new MachineAppearance();

			// protection
			if( string.IsNullOrEmpty( szFolderName ) ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return false;
			}

			// check folder is exit
			if( !Directory.Exists( szFolderName ) ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return false;
			}

			// get mesh from stl
			bool bLoadSuccess = LoadMahineMesh( szFolderName, ref machineAppearance );
			if (bLoadSuccess == false ) {
				return false;
			}

			// convert mesh to AIS
			ConvertToAIS( ref machineAppearance );
			return true;
		}

		static bool LoadMahineMesh( string szFolderName, ref MachineAppearance machineAppearance )
		{
			if( machineAppearance == null ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return false;
			}
			// read stl files
			string[] stlNames = { "Base", "X", "Y", "Z", "Master", "Slave", "Tool" };

			foreach( string name in stlNames ) {
				string filePath = Path.Combine( szFolderName, name + ".stl" );
				if( !File.Exists( filePath ) ) {
					AnnounceWrning( ReadStlResult.FileNotFound, name );
					continue;
				}
				Poly_Triangulation mesh = LoadStlToMesh( filePath );
				switch( name ) {
					case "Base":
						machineAppearance.Meshes[ MachineComponentType.Base ] = mesh;
						break;
					case "X":
						machineAppearance.Meshes[ MachineComponentType.XAxis ] = mesh;
						break;
					case "Y":
						machineAppearance.Meshes[ MachineComponentType.YAxis ] = mesh;
						break;
					case "Z":
						machineAppearance.Meshes[ MachineComponentType.ZAxis ] = mesh;
						break;
					case "Master":
						machineAppearance.Meshes[ MachineComponentType.Master ] = mesh;
						break;
					case "Slave":
						machineAppearance.Meshes[ MachineComponentType.Slave ] = mesh;
						break;
					case "Tool":
						machineAppearance.Meshes[ MachineComponentType.Tool ] = mesh;
						break;
					default:
						break;
				}
			}
			return true;
		}

		static void ConvertToAIS( ref MachineAppearance machineAppearance )
		{
			if( machineAppearance == null ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return;
			}
			machineAppearance.AisObjects[ MachineComponentType.Base ] = ConvertMeshToAIS( machineAppearance.Meshes[ MachineComponentType.Base ] );
			machineAppearance.AisObjects[ MachineComponentType.XAxis ] = ConvertMeshToAIS( machineAppearance.Meshes[ MachineComponentType.XAxis ] );
			machineAppearance.AisObjects[ MachineComponentType.YAxis ] = ConvertMeshToAIS( machineAppearance.Meshes[ MachineComponentType.YAxis ] );
			machineAppearance.AisObjects[ MachineComponentType.ZAxis ] = ConvertMeshToAIS( machineAppearance.Meshes[ MachineComponentType.ZAxis ] );
			machineAppearance.AisObjects[ MachineComponentType.Master ] = ConvertMeshToAIS( machineAppearance.Meshes[ MachineComponentType.Master ] );
			machineAppearance.AisObjects[ MachineComponentType.Slave ] = ConvertMeshToAIS( machineAppearance.Meshes[ MachineComponentType.Slave ] );
			machineAppearance.AisObjects[ MachineComponentType.Tool ] = ConvertMeshToAIS( machineAppearance.Meshes[ MachineComponentType.Tool ] );
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

		static Poly_Triangulation LoadStlToMesh( string fileName )
		{
			ReadStlResult result = ReadStlFile( fileName, out Poly_Triangulation triangulation );
			if( result != ReadStlResult.Success ) {
				AnnounceWrning( result, fileName );
			}
			return triangulation;
		}

		enum ReadStlResult
		{
			Success,
			ArgumentNull,
			FileNotFound,
			InvalidMesh,
			UnknownError
		}

		static ReadStlResult ReadStlFile( string filePath, out Poly_Triangulation shapes )
		{
			shapes = new Poly_Triangulation();
			try {
				if( !File.Exists( filePath ) ) {
					return ReadStlResult.FileNotFound;
				}
				shapes = RWStl.ReadFile( filePath );
				if( shapes == null ) {
					return ReadStlResult.InvalidMesh;
				}
				if( shapes.NbNodes() <= 0 || shapes.NbTriangles() <= 0 ) {
					return ReadStlResult.InvalidMesh;
				}
			}
			catch {
				return ReadStlResult.UnknownError;
			}
			return ReadStlResult.Success;
		}

		static void AnnounceWrning( ReadStlResult result, string fileName = null )
		{
			if( result != ReadStlResult.Success ) {
				switch( result ) {
					case ReadStlResult.ArgumentNull:
						MyApp.Logger.ShowOnLogPanel( $"機構資料夾路徑錯誤", MyApp.NoticeType.Warning );
						break;
					case ReadStlResult.FileNotFound:
						MyApp.Logger.ShowOnLogPanel( $"{fileName} 圖檔有缺漏", MyApp.NoticeType.Warning );
						break;
					case ReadStlResult.InvalidMesh:
						MyApp.Logger.ShowOnLogPanel( $"{fileName} 檔案讀取失敗", MyApp.NoticeType.Warning );
						break;
					case ReadStlResult.UnknownError:
						MyApp.Logger.ShowOnLogPanel( $"{fileName} STL讀取失敗，發生未知錯誤", MyApp.NoticeType.Warning );
						break;
				}
				return;
			}
		}
	}
}
