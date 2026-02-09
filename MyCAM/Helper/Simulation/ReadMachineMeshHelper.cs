using MyCAM.App;
using MyCAM.Data;
using OCC.Poly;
using OCC.RWStl;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyCAM.Helper
{
	public sealed class MachineMeshes
	{
		// per component mesh
		public readonly Dictionary<MachineComponentType, Poly_Triangulation> Meshes = new Dictionary<MachineComponentType, Poly_Triangulation>();

		public MachineMeshes()
		{
			foreach( MachineComponentType componentType in Enum.GetValues( typeof( MachineComponentType ) ) ) {
				Meshes[ componentType ] = null;
			}
		}
	}

	public enum ReadStlResult
	{
		Success,
		ArgumentNull,
		FileNotFound,
		InvalidMesh,
		UnknownError
	}

	public class ReadMachineMeshHelper
	{
		public static bool LoadMachineMeshes( string szFolderPath, out MachineMeshes machineMeshes )
		{
			machineMeshes = new MachineMeshes();

			// protection
			if( string.IsNullOrEmpty( szFolderPath ) ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return false;
			}

			// check folder is exit
			if( !Directory.Exists( szFolderPath ) ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return false;
			}
			// get mesh from stl
			bool bLoadSuccess = LoadMahineMesh( szFolderPath, ref machineMeshes );
			if( bLoadSuccess == false ) {
				return false;
			}
			return true;
		}

		static bool LoadMahineMesh( string szFolderName, ref MachineMeshes machineMeshes )
		{
			if( machineMeshes == null ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return false;
			}
			// read stl files
			string[] stlNames = { "Base", "X", "Y", "Z", "Master", "Slave" };

			foreach( string name in stlNames ) {
				string filePath = Path.Combine( szFolderName, name + ".stl" );
				if( !File.Exists( filePath ) ) {
					if( name == "Master" || name == "Slave") {
						AnnounceWrning( ReadStlResult.FileNotFound, name );
						return false;
					}
					continue;
				}
				Poly_Triangulation mesh = LoadStlAsMesh( filePath );
				switch( name ) {
					case "Base":
						machineMeshes.Meshes[ MachineComponentType.Base ] = mesh;
						break;
					case "X":
						machineMeshes.Meshes[ MachineComponentType.XAxis ] = mesh;
						break;
					case "Y":
						machineMeshes.Meshes[ MachineComponentType.YAxis ] = mesh;
						break;
					case "Z":
						machineMeshes.Meshes[ MachineComponentType.ZAxis ] = mesh;
						break;
					case "Master":
						machineMeshes.Meshes[ MachineComponentType.Master ] = mesh;
						break;
					case "Slave":
						machineMeshes.Meshes[ MachineComponentType.Slave ] = mesh;
						break;
					default:
						break;
				}
			}
			return true;
		}

		static Poly_Triangulation LoadStlAsMesh( string fileName )
		{
			ReadStlResult result = ReadStlFile( fileName, out Poly_Triangulation triangulation );
			if( result != ReadStlResult.Success ) {
				AnnounceWrning( result, fileName );
			}
			return triangulation;
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
