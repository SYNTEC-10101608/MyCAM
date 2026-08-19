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
		static readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ReadMachineMeshHelper" );

		public static bool LoadMachineMeshes( string szFolderPath, IMachineDataBase machineData, out MachineMeshes machineMeshes )
		{
			machineMeshes = new MachineMeshes();

			// protection
			if( string.IsNullOrEmpty( szFolderPath ) || machineData == null ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return false;
			}

			// check folder is exit
			if( !Directory.Exists( szFolderPath ) ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return false;
			}
			// get mesh from stl
			bool bLoadSuccess = LoadMahineMesh( szFolderPath, machineData, ref machineMeshes );
			if( bLoadSuccess == false ) {
				return false;
			}
			return true;
		}

		static bool LoadMahineMesh( string szFolderName, IMachineDataBase machineData, ref MachineMeshes machineMeshes )
		{
			if( machineMeshes == null || machineData == null ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return false;
			}

			// get component file name by machine product
			IReadOnlyDictionary<MachineComponentType, string> meshFileNameMap = machineData.MeshFileNameMap;
			if( meshFileNameMap == null ) {
				AnnounceWrning( ReadStlResult.ArgumentNull );
				return false;
			}

			bool bAnyMeshLoaded = false;
			foreach( KeyValuePair<MachineComponentType, string> nameEntry in meshFileNameMap ) {
				MachineComponentType componentType = nameEntry.Key;
				string filePath = Path.Combine( szFolderName, nameEntry.Value + ".stl" );
				if( !File.Exists( filePath ) ) {
					continue;
				}
				machineMeshes.Meshes[ componentType ] = LoadStlAsMesh( filePath );
				bAnyMeshLoaded = true;
			}

			// fail when no mesh file could be loaded at all
			if( !bAnyMeshLoaded ) {
				AnnounceWrning( ReadStlResult.FileNotFound );
				return false;
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
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "MeshFolderPathError", MESH_FOLDER_PATH_ERROR ), MyApp.NoticeType.Warning );
						break;
					case ReadStlResult.FileNotFound:
						MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "MeshFileMissing", MESH_FILE_MISSING ), fileName ), MyApp.NoticeType.Warning );
						break;
					case ReadStlResult.InvalidMesh:
						MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "MeshFileReadFailed", MESH_FILE_READ_FAILED ), fileName ), MyApp.NoticeType.Warning );
						break;
					case ReadStlResult.UnknownError:
						MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "MeshFileUnknownError", MESH_FILE_UNKNOWN_ERROR ), fileName ), MyApp.NoticeType.Warning );
						break;
				}
				return;
			}
		}

		const string MESH_FOLDER_PATH_ERROR = "機構資料夾路徑錯誤";
		const string MESH_FILE_MISSING = "{0} 圖檔有缺漏";
		const string MESH_FILE_READ_FAILED = "{0} 檔案讀取失敗";
		const string MESH_FILE_UNKNOWN_ERROR = "{0} STL讀取失敗，發生未知錯誤";
	}
}
