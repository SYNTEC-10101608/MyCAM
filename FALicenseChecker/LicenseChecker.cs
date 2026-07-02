using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FALicenseChecker
{
	public static partial class LicenseChecker
	{
		public static bool IsLicenseActivated()
		{
			try {
				// If license file exists, check if it is valid.
				if( IsLicenseFileExists() ) {
					return ValidateLicenseFile();
				}

				return ActivateLicense();
			}
			catch( System.Exception ex ) {
				MessageBox.Show(
					string.Format( m_LanguageManager.TryGetString( "Error_IsLicenseActivatedException", ERROR_IS_LICENSE_ACTIVATED_EXCEPTION ), ex.Message ),
					m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
					MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}
		}
	}

	public static partial class LicenseChecker
	{
		const string REQUEST_FILE_NAME = "ActivationRequest.txt";
		const string LICENSE_FILE_NAME = "License.key";

		const string CUSTOM_root_Folder_Name = "Custom";

		static string LICENSE_FILE_PATH => Path.Combine(
			System.AppDomain.CurrentDomain.BaseDirectory,
			CUSTOM_root_Folder_Name,
			LICENSE_FILE_NAME );

		const string PUBLIC_KEY_XML = "<RSAKeyValue><Modulus>zNT894WmvUZ/8pTV+8DK6giAjNhyMsBRuWpxetMPpOy6K9L3VqyZz5PbqzekKqOj0UUzZFmfQetut83wjiH3LOwBmmKXJZDP+P//4bJ5sgupLwklFlYtBfH6LpfZzgxL67jFY7nJGwhtUbw8iZdyVZls0FY03YKAwJKpl4PBq8jEGiRdrcK08SHMSsxjuts6Rp3YkZg1Qtk/z2xi6CLsoEHXuGW/aiZlFK0DLvcO6Eh4BQ5Ubw0KHZruRsSs2LNElyQLXN1OjRP4R8Mk/zpdosSrFjVF9OxhXaikxTCt4SVjl6MEEx8FgE759q8kqahxkwJuvPJ4ArAiZKSu4wuDIQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

		const string ERROR_LICENSE_CODE_INVALID_BASE64 = "授權碼格式錯誤（非有效 Base64）";
		const string ERROR_TITLE = "錯誤";
		const string ERROR_INSTALL_LICENSE_FAILED = "安裝授權失敗：\n{0}";
		const string ERROR_ENTRY_ASSEMBLY_NULL = "無法取得執行組件資訊（產生請求）";
		const string ERROR_PRODUCT_EMPTY = "產品名稱為空（產生請求）";
		const string ERROR_VERSION_EMPTY = "版本資訊為空（產生請求）";
		const string INFO_ACTIVATION_REQUEST_GENERATED = "授權請求已產生：\n{0}";
		const string INFO_ACTIVATION_REQUEST_TITLE = "授權請求";
		const string ERROR_IS_LICENSE_ACTIVATED_EXCEPTION = "授權檢查發生例外：\n{0}";
		const string ERROR_LICENSE_FILE_EMPTY = "授權檔內容為空";
		const string ERROR_LICENSE_FILE_SIGNATURE_FAILED = "授權檔簽章驗證失敗";
		const string ERROR_READ_LICENSE_FILE_FAILED = "讀取授權檔失敗：\n{0}";
		const string ERROR_LICENSE_FILE_NO_SIGNATURE = "授權檔缺少簽章欄位";
		const string ERROR_LICENSE_MISSING_FIELDS = "授權檔缺少必要欄位";
		const string ERROR_ENTRY_ASSEMBLY_NULL_VERSION = "無法取得執行組件資訊（版本檢查）";
		const string ERROR_PRODUCT_MISMATCH = "授權產品不符：授權為 {0}，目前為 {1}";
		const string ERROR_MIN_VERSION_NOT_MET = "目前版本低於授權最小版本：最小 {0}，目前 {1}";
		const string ERROR_MAX_VERSION_EXCEEDED = "目前版本超過授權最大版本：最大 {0}，目前 {1}";
		const string ERROR_LICENSE_CODE_EMPTY = "授權碼為空";
		const string ERROR_LICENSE_CODE_NO_SIGNATURE = "授權碼缺少簽章";

		static readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "LicenseChecker" );

		static bool ActivateLicense()
		{
			// License file does not exist.
			GenerateActivationRequest();
			FrmLicenseActivation frmLicenseActivation = new FrmLicenseActivation();
			if( frmLicenseActivation.ShowDialog() != DialogResult.OK ) {
				return false;
			}

			// Check if input code can be installed.
			string szLicenseCode = frmLicenseActivation.LicenseCode;
			if( TryInstallLicenseFromCode( szLicenseCode ) == false ) {
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.Retry, FrmLicenseValidationFailed.EHintID.LicenseCodeInvalid );
			}
			return ValidateLicenseFile();
		}

		static bool IsLicenseFileExists()
		{
			return File.Exists( LICENSE_FILE_PATH );
		}

		static bool ValidateLicenseFile()
		{
			// Read license file lines.
			List<string> lines = ReadLicenseFile();
			if( lines == null || lines.Count == 0 ) {
				MessageBox.Show(
						m_LanguageManager.TryGetString( "Error_LicenseFileEmpty", ERROR_LICENSE_FILE_EMPTY ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
				}

			// Parse license file into key-value pairs and extract signature.
			if( !TryParseLicenseFile( lines, out Dictionary<string, string> keyValue, out List<string> orderedDataLines, out string szSignature ) ) {
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}

			// Verify signature.
			if( !VerifySignature( orderedDataLines, szSignature ) ) {
				MessageBox.Show(
						m_LanguageManager.TryGetString( "Error_LicenseFileSignatureFailed", ERROR_LICENSE_FILE_SIGNATURE_FAILED ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}

			// Validate license content.
			if( !ValidateLicenseContent( keyValue ) ) {
				return false;
			}
			return true;
		}

		static List<string> ReadLicenseFile()
		{
			try {
				return File.ReadAllLines( LICENSE_FILE_PATH, Encoding.UTF8 )
							.Select( l => l.Trim() )
							.Where( l => !string.IsNullOrWhiteSpace( l ) && !l.StartsWith( "#" ) )
							.ToList();
			}
			catch( System.Exception ex ) {
				MessageBox.Show(
					string.Format( m_LanguageManager.TryGetString( "Error_ReadLicenseFileFailed", ERROR_READ_LICENSE_FILE_FAILED ), ex.Message ),
					m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
					MessageBoxButtons.OK, MessageBoxIcon.Error );
				return null;
			}
		}

		static bool TryParseLicenseFile( List<string> lines, out Dictionary<string, string> keyValue, out List<string> orderedDataLines, out string szSignature )
		{
			keyValue = new Dictionary<string, string>( System.StringComparer.OrdinalIgnoreCase );
			orderedDataLines = new List<string>();
			szSignature = null;

			foreach( string line in lines ) {
				int nIdx = line.IndexOf( '=' );
				if( nIdx <= 0 ) {
					continue;
				}

				string szKey = line.Substring( 0, nIdx ).Trim();
				string szValue = line.Substring( nIdx + 1 ).Trim();

				if( szKey.Equals( "Signature", System.StringComparison.OrdinalIgnoreCase ) ) {
					szSignature = szValue;
				}
				else {
					keyValue[ szKey ] = szValue;
					orderedDataLines.Add( $"{szKey}={szValue}" );
				}
			}

			if( string.IsNullOrWhiteSpace( szSignature ) ) {
				MessageBox.Show(
						m_LanguageManager.TryGetString( "Error_LicenseFileNoSignature", ERROR_LICENSE_FILE_NO_SIGNATURE ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
					return false;
				}

			return true;
		}

		static bool ValidateLicenseContent( Dictionary<string, string> keyValue )
		{
			if( !keyValue.TryGetValue( "Customer", out string szCustomer ) ||
				!keyValue.TryGetValue( "HWID", out string szHwid ) || string.IsNullOrWhiteSpace( szHwid ) ) {
				MessageBox.Show(
						m_LanguageManager.TryGetString( "Error_LicenseMissingFields", ERROR_LICENSE_MISSING_FIELDS ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
				}

			if( szCustomer == "SyntecRD" ) {
				return true;
			}

			// Validate HWID.
			if( !IsHwidMatchCurrentMachineHash( szHwid ) ) {
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.MachineMismatch );
			}

			// Validate product and version constraints.
			return ValidateProductAndVersionConstraints( keyValue );
		}

		static bool ValidateProductAndVersionConstraints( Dictionary<string, string> keyValue )
		{
			Assembly entry = Assembly.GetEntryAssembly();
			if( entry == null ) {
				MessageBox.Show(
						m_LanguageManager.TryGetString( "Error_EntryAssemblyNull_Version", ERROR_ENTRY_ASSEMBLY_NULL_VERSION ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}
			AssemblyName asmName = entry.GetName();

			if( !keyValue.TryGetValue( "Product", out string szProduct ) ||
				!keyValue.TryGetValue( "MinAppVersion", out string szMinVersion ) ||
				!keyValue.TryGetValue( "MaxAppVersion", out string szMaxVersion ) ) {
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}

			if( !string.Equals( szProduct, asmName.Name, System.StringComparison.OrdinalIgnoreCase ) ) {
				MessageBox.Show(
						string.Format( m_LanguageManager.TryGetString( "Error_ProductMismatch", ERROR_PRODUCT_MISMATCH ), szProduct, asmName.Name ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}
			if( System.Version.TryParse( szMinVersion, out System.Version minVersion ) && asmName.Version < minVersion ) {
				MessageBox.Show(
						string.Format( m_LanguageManager.TryGetString( "Error_MinVersionNotMet", ERROR_MIN_VERSION_NOT_MET ), minVersion, asmName.Version ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.VersionMismatch );
			}
			if( System.Version.TryParse( szMaxVersion, out System.Version maxVersion ) && asmName.Version > maxVersion ) {
				MessageBox.Show(
						string.Format( m_LanguageManager.TryGetString( "Error_MaxVersionExceeded", ERROR_MAX_VERSION_EXCEEDED ), maxVersion, asmName.Version ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.VersionMismatch );
			}
			return true;
		}

		static bool HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode mode, FrmLicenseValidationFailed.EHintID hint )
		{
			FrmLicenseValidationFailed frmLicenseValidationFailed = new FrmLicenseValidationFailed();
			frmLicenseValidationFailed.ShowDialog( mode, hint );
			if( frmLicenseValidationFailed.DialogResult == DialogResult.Retry ) {
				if( mode == FrmLicenseValidationFailed.EButtonShowMode.RestartActivation ) {
					DeleteLicenseFile();
				}
				return IsLicenseActivated();
			}
			return false;
		}

		static void DeleteLicenseFile()
		{
			if( File.Exists( LICENSE_FILE_PATH ) ) {
				File.Delete( LICENSE_FILE_PATH );
			}
		}

		static bool TryInstallLicenseFromCode( string szLicenseCode )
		{
			if( string.IsNullOrWhiteSpace( szLicenseCode ) ) {
				MessageBox.Show(
						m_LanguageManager.TryGetString( "Error_LicenseCodeEmpty", ERROR_LICENSE_CODE_EMPTY ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
				}

			try {
				byte[] bytes = System.Convert.FromBase64String( szLicenseCode.Trim() );
				string szText = Encoding.UTF8.GetString( bytes );
				if( szText.Contains( "\r\n" ) ) {
					szText = szText.Replace( "\r\n", "\n" );
				}

				// Simple check if there is a signature line.
				if( !szText.Contains( "Signature=" ) ) {
					MessageBox.Show(
							m_LanguageManager.TryGetString( "Error_LicenseCodeNoSignature", ERROR_LICENSE_CODE_NO_SIGNATURE ),
							m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
							MessageBoxButtons.OK, MessageBoxIcon.Error );
						return false;
				}

				// ensure directory exists before writing license file
				Directory.CreateDirectory( Path.GetDirectoryName( LICENSE_FILE_PATH ) );
				File.WriteAllText( LICENSE_FILE_PATH, szText.Replace( "\n", System.Environment.NewLine ), Encoding.UTF8 );
				return true;
			}
			catch( System.FormatException ) {
				MessageBox.Show(
					m_LanguageManager.TryGetString( "Error_LicenseCodeInvalidBase64", ERROR_LICENSE_CODE_INVALID_BASE64 ),
					m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
					MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}
			catch( System.Exception ex ) {
				MessageBox.Show(
					string.Format( m_LanguageManager.TryGetString( "Error_InstallLicenseFailed", ERROR_INSTALL_LICENSE_FAILED ), ex.Message ),
					m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
					MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}
		}

		static bool VerifySignature( List<string> orderedDataLines, string szSignature )
		{
			string szDataBlock = string.Join( "\n", orderedDataLines );
			byte[] dataBytes = Encoding.UTF8.GetBytes( szDataBlock );
			byte[] signature = System.Convert.FromBase64String( szSignature );

			using( var rsa = new RSACryptoServiceProvider( 2048 ) ) {
				rsa.FromXmlString( PUBLIC_KEY_XML );
				using( var sha = SHA256.Create() ) {
					byte[] hash = sha.ComputeHash( dataBytes );
					string szOid = CryptoConfig.MapNameToOID( "SHA256" );
					return rsa.VerifyHash( hash, szOid, signature );
				}
			}
		}

		static bool IsHwidMatchCurrentMachineHash( string szHwid )
		{
			string szCurrentHash = ComputeMachineFingerprintHash( out string _ );
			return string.Equals( szHwid, szCurrentHash, System.StringComparison.OrdinalIgnoreCase );
		}

		static void GenerateActivationRequest()
		{
			Assembly entry = Assembly.GetEntryAssembly();
			if( entry == null ) {
				MessageBox.Show(
					m_LanguageManager.TryGetString( "Error_EntryAssemblyNull_Request", ERROR_ENTRY_ASSEMBLY_NULL ),
					m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
					MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			AssemblyName asmName = entry.GetName();
			string szProduct = asmName.Name;
			string szVersion = asmName.Version.ToString();

			if( string.IsNullOrWhiteSpace( szProduct ) ) {
				MessageBox.Show(
					m_LanguageManager.TryGetString( "Error_ProductEmpty_Request", ERROR_PRODUCT_EMPTY ),
					m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
					MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}
			if( string.IsNullOrWhiteSpace( szVersion ) ) {
				MessageBox.Show(
					m_LanguageManager.TryGetString( "Error_VersionEmpty_Request", ERROR_VERSION_EMPTY ),
					m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
					MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			string szBaseDir = System.AppDomain.CurrentDomain.BaseDirectory;
			string szReqPath = Path.Combine( szBaseDir, REQUEST_FILE_NAME );
			string szMachineHash = ComputeMachineFingerprintHash( out string szRawDetail );

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine( $"RequestId={System.Guid.NewGuid():N}" );
			stringBuilder.AppendLine( $"Product={szProduct}" );
			stringBuilder.AppendLine( $"Version={szVersion}" );
			stringBuilder.AppendLine( $"FingerprintRaw={szRawDetail}" );
			stringBuilder.AppendLine( $"MachineHash={szMachineHash}" );
			stringBuilder.AppendLine( $"RequestTimeUTC={System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}" );
			File.WriteAllText( szReqPath, stringBuilder.ToString(), Encoding.UTF8 );

			MessageBox.Show(
				string.Format( m_LanguageManager.TryGetString( "Info_ActivationRequestGenerated", INFO_ACTIVATION_REQUEST_GENERATED ), szReqPath ),
				m_LanguageManager.TryGetString( "Info_ActivationRequestGeneratedTitle", INFO_ACTIVATION_REQUEST_TITLE ),
				MessageBoxButtons.OK, MessageBoxIcon.Information );
		}

		#region Get Machine Fingerprint and Hash

		// Collect fingerprint and return its hash, also output the raw detail (rawDetail).
		static string ComputeMachineFingerprintHash( out string szRawDetail )
		{
			var parts = new List<string>();

			string uuid = GetWmiValue( "Win32_ComputerSystemProduct", "UUID" );
			if( string.IsNullOrWhiteSpace( uuid ) == false ) {
				parts.Add( "BoardUUID=" + uuid );
			}

			string baseBoard = GetWmiValue( "Win32_BaseBoard", "SerialNumber" );
			if( string.IsNullOrWhiteSpace( baseBoard ) == false ) {
				parts.Add( "BaseBoard=" + baseBoard );
			}

			string cpuId = GetWmiValue( "Win32_Processor", "ProcessorId" );
			if( string.IsNullOrWhiteSpace( cpuId ) == false ) {
				parts.Add( "CPU=" + cpuId );
			}

			string diskSerial = GetSystemDiskSerial();
			if( string.IsNullOrWhiteSpace( diskSerial ) == false ) {
				parts.Add( "Disk=" + diskSerial );
			}

			szRawDetail = string.Join( "|", parts );
			const int MACHINE_FINGERPRINT_PARTS_REQUIRED = 4;
			if( parts.Count != MACHINE_FINGERPRINT_PARTS_REQUIRED ) {
				return string.Empty;
			}

			using( var sha = SHA256.Create() ) {
				var hash = sha.ComputeHash( Encoding.UTF8.GetBytes( szRawDetail ) );
				var fullHex = System.BitConverter.ToString( hash ).Replace( "-", "" );
				return fullHex.Substring( 0, 32 );
			}
		}

		static string GetSystemDiskSerial()
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher( "SELECT VolumeSerialNumber, Name FROM Win32_LogicalDisk WHERE DriveType=3" );
			using( ManagementObjectCollection results = searcher.Get() ) {
				foreach( ManagementObject managementObj in results.Cast<ManagementObject>() ) {
					string szName = ( managementObj[ "Name" ] as string ) ?? string.Empty;
					if( szName.StartsWith( "C", System.StringComparison.OrdinalIgnoreCase ) ) {
						var volumeSerialNumber = managementObj[ "VolumeSerialNumber" ] as string;
						if( string.IsNullOrWhiteSpace( volumeSerialNumber ) == false ) {
							return volumeSerialNumber.Trim();
						}
					}
				}
			}
			return string.Empty;
		}

		static string GetWmiValue( string szWmiClass, string szProp )
		{
			var searcher = new ManagementObjectSearcher( $"SELECT {szProp} FROM {szWmiClass}" );
			using( var results = searcher.Get() ) {
				var obj = results.Cast<ManagementObject>().FirstOrDefault();
				if( obj != null ) {
					var v = obj[ szProp ] as string;
					if( string.IsNullOrWhiteSpace( v ) == false )
						return v.Trim();
				}
			}
			return string.Empty;
		}

		#endregion
	}
}
