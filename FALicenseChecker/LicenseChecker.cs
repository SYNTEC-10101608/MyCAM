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
		const string LICENSE_CHECKER_VERSION = "1";
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
		const string ERROR_MACHINE_INFO_NOT_ENOUGH = "機器資訊不足（產生請求）";
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
		const string ERROR_LICENSE_EXPIRED = "授權已過期：到期日 {0}（UTC），目前 {1}（UTC）";
		const string ERROR_EXPIRE_DATE_INVALID = "授權到期日格式錯誤：{0}";
		const string ERROR_LICENSE_CODE_EMPTY = "授權碼為空";
		const string ERROR_LICENSE_CODE_NO_SIGNATURE = "授權碼缺少簽章";

		static readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "LicenseChecker" );

		static readonly HashSet<string> s_InvalidHardwareValues = new System.Collections.Generic.HashSet<string>( System.StringComparer.OrdinalIgnoreCase )
		{
			"Default string",
			"To Be Filled By O.E.M.",
			"N/A",
			"None",
			"Unknown"
		};

		static bool ActivateLicense( OldLicenseInfo? oldLicenseInfo = null )
		{
			// License file does not exist.
			GenerateActivationRequest( oldLicenseInfo );
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

			// check version
			if( !keyValue.TryGetValue( "LicenseCheckerVersion", out string szOldChckerVersion ) || !szOldChckerVersion.Equals( LICENSE_CHECKER_VERSION, System.StringComparison.OrdinalIgnoreCase ) ) {
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.CheckerVersionMismatch );
			}
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
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.CADCAMVersionMismatch );
			}
			if( System.Version.TryParse( szMaxVersion, out System.Version maxVersion ) && asmName.Version > maxVersion ) {
				MessageBox.Show(
						string.Format( m_LanguageManager.TryGetString( "Error_MaxVersionExceeded", ERROR_MAX_VERSION_EXCEEDED ), maxVersion, asmName.Version ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.CADCAMVersionMismatch );
			}
			if( !ValidateExpireDateUtc( keyValue ) ) {
				return false;
			}
			return true;
		}

		static bool ValidateExpireDateUtc( Dictionary<string, string> keyValue )
		{
			// get node
			if( !keyValue.TryGetValue( "ExpireDateUtc", out string szExpireDateUtc ) ) {
				MessageBox.Show(
						m_LanguageManager.TryGetString( "Error_LicenseMissingFields", ERROR_LICENSE_MISSING_FIELDS ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}

			// no expire date means no expiration
			if( string.IsNullOrWhiteSpace( szExpireDateUtc ) ) {
				return true;
			}

			// check time format
			string[] expireDateFormats = new[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" };
			if( !System.DateTime.TryParseExact( szExpireDateUtc.Trim(), expireDateFormats,
					System.Globalization.CultureInfo.InvariantCulture,
					System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
					out System.DateTime expireDateUtc ) ) {
				// The field exists but the value cannot be interpreted as a valid date/time.
				MessageBox.Show(
						string.Format( m_LanguageManager.TryGetString( "Error_ExpireDateInvalid", ERROR_EXPIRE_DATE_INVALID ), szExpireDateUtc ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}

			// compare against the current UTC time to avoid time-zone dependent behavior.
			System.DateTime nowUtc = System.DateTime.UtcNow;
			if( nowUtc > expireDateUtc ) {

				//license has expired
				MessageBox.Show(
						string.Format( m_LanguageManager.TryGetString( "Error_LicenseExpired", ERROR_LICENSE_EXPIRED ), expireDateUtc.ToString( "yyyy-MM-dd HH:mm:ss" ), nowUtc.ToString( "yyyy-MM-dd HH:mm:ss" ) ),
						m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
						MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseExpired );
			}

			// time its valid
			return true;
		}

		static bool HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode mode, FrmLicenseValidationFailed.EHintID hint )
		{
			FrmLicenseValidationFailed frmLicenseValidationFailed = new FrmLicenseValidationFailed();
			frmLicenseValidationFailed.ShowDialog( mode, hint );
			if( frmLicenseValidationFailed.DialogResult == DialogResult.Retry ) {
				if( mode == FrmLicenseValidationFailed.EButtonShowMode.RestartActivation ) {

					//  get old license info before deleting
					OldLicenseInfo? oldLicenseInfo = null;
					if( IsLicenseFileExists() ) {
						List<string> lines = ReadLicenseFile();
						if( lines != null && TryParseLicenseFile( lines, out Dictionary<string, string> keyValue, out List<string> _, out string _ ) ) {
							if( TryGetOldLicenseInfo( keyValue, out OldLicenseInfo info ) ) {
								info.RenewalReason = hint.ToString();
								oldLicenseInfo = info;
							}
						}
					}
					DeleteLicenseFile();
					return ActivateLicense( oldLicenseInfo );
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
			bool bSuccess = ComputeMachineFingerprintHash( out string szRawDetail, out string MachineHash, out _ );
			if( bSuccess == false ) {
				return false;
			}
			return string.Equals( szHwid, MachineHash, System.StringComparison.OrdinalIgnoreCase );
		}

		static void GenerateActivationRequest( OldLicenseInfo? oldLicenseInfo = null )
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
			bool isGetHashDone = ComputeMachineFingerprintHash( out string szRawDetail, out string szMachineHash, out string szScore );

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine( $"RequestId={System.Guid.NewGuid():N}" );
			stringBuilder.AppendLine( $"Product={szProduct}" );
			stringBuilder.AppendLine( $"Version={szVersion}" );
			stringBuilder.AppendLine( $"LicenseCheckerVersion={LICENSE_CHECKER_VERSION}" );
			stringBuilder.AppendLine( $"FingerprintRaw={szRawDetail}" );
			stringBuilder.AppendLine( $"MachineHash={szMachineHash}" );
			stringBuilder.AppendLine( $"Score={szScore}" );
			stringBuilder.AppendLine( $"RequestTimeUTC={System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}" );

			if( oldLicenseInfo.HasValue ) {
				OldLicenseInfo old = oldLicenseInfo.Value;
				stringBuilder.AppendLine( $"OldLicenseCheckerVersion={old.LicenseCheckerVersion ?? string.Empty}" );
				stringBuilder.AppendLine( $"OldCustomer={old.Customer ?? string.Empty}" );
				stringBuilder.AppendLine( $"OldRequestTime={old.RequestTimeUTC ?? string.Empty}" );
				stringBuilder.AppendLine( $"OldIssuedTime={old.IssueDateUTC ?? string.Empty}" );
				stringBuilder.AppendLine( $"RenewalReason={old.RenewalReason ?? string.Empty}" );
			}

			File.WriteAllText( szReqPath, stringBuilder.ToString(), Encoding.UTF8 );

			MessageBox.Show(
				string.Format( m_LanguageManager.TryGetString( "Info_ActivationRequestGenerated", INFO_ACTIVATION_REQUEST_GENERATED ), szReqPath ),
				m_LanguageManager.TryGetString( "Info_ActivationRequestGeneratedTitle", INFO_ACTIVATION_REQUEST_TITLE ),
				MessageBoxButtons.OK, MessageBoxIcon.Information );
		}

		#region Get Machine Fingerprint and Hash

		// Collect fingerprint and return its hash, also output the raw detail (rawDetail).
		static bool ComputeMachineFingerprintHash( out string szRawDetail, out string MachineHash, out string szScore )
		{
			MachineHash = string.Empty;
			szScore = string.Empty;
			const int MACHINE_FINGERPRINT_WEIGHT_BOARD_UUID = 35;
			const int MACHINE_FINGERPRINT_WEIGHT_CPU = 25;
			const int MACHINE_FINGERPRINT_WEIGHT_BASE_BOARD_SERIAL = 15;
			const int MACHINE_FINGERPRINT_WEIGHT_BASE_BOARD = 10;
			const int MACHINE_FINGERPRINT_WEIGHT_BIOS_SERIAL = 10;
			const int MACHINE_FINGERPRINT_WEIGHT_DISK = 5;
			const int MACHINE_FINGERPRINT_WEIGHT_THRESHOLD = 60;

			var parts = new List<string>();
			int weight = 0;

			string uuid = GetWmiValue( "Win32_ComputerSystemProduct", "UUID" );
			AppendHardwarePart( parts, ref weight, "BoardUUID", uuid, MACHINE_FINGERPRINT_WEIGHT_BOARD_UUID );

			string baseBoardSerial = GetWmiValue( "Win32_BaseBoard", "SerialNumber" );
			AppendHardwarePart( parts, ref weight, "BaseBoardSerial", baseBoardSerial, MACHINE_FINGERPRINT_WEIGHT_BASE_BOARD_SERIAL );

			string baseBoard = GetWmiValue( "Win32_BaseBoard", "Product" );
			AppendHardwarePart( parts, ref weight, "BaseBoard", baseBoard, MACHINE_FINGERPRINT_WEIGHT_BASE_BOARD );

			string cpuId = GetWmiValue( "Win32_Processor", "ProcessorId" );
			AppendHardwarePart( parts, ref weight, "CPU", cpuId, MACHINE_FINGERPRINT_WEIGHT_CPU );

			string biosSerial = GetWmiValue( "Win32_BIOS", "SerialNumber" );
			AppendHardwarePart( parts, ref weight, "BIOSSerial", biosSerial, MACHINE_FINGERPRINT_WEIGHT_BIOS_SERIAL );

			string diskSerial = GetSystemDiskSerial();
			AppendHardwarePart( parts, ref weight, "Disk", diskSerial, MACHINE_FINGERPRINT_WEIGHT_DISK );
			szScore = weight.ToString();

			szRawDetail = string.Join( "|", parts );
			if( weight < MACHINE_FINGERPRINT_WEIGHT_THRESHOLD ) {
				MessageBox.Show(
					m_LanguageManager.TryGetString( "Error_Machine_Info_Lack", ERROR_MACHINE_INFO_NOT_ENOUGH ),
					m_LanguageManager.TryGetString( "Error_Title", ERROR_TITLE ),
					MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}

			using( var sha = SHA256.Create() ) {
				var hash = sha.ComputeHash( Encoding.UTF8.GetBytes( szRawDetail ) );
				var fullHex = System.BitConverter.ToString( hash ).Replace( "-", "" );
				MachineHash = fullHex.Substring( 0, 32 );
				return true;
			}
		}

		static bool IsValidHardwareValue( string value )
		{
			return string.IsNullOrWhiteSpace( value ) == false
				&& !s_InvalidHardwareValues.Contains( value );
		}

		static void AppendHardwarePart( List<string> parts, ref int weight, string key, string value, int partWeight )
		{
			if( string.IsNullOrWhiteSpace( value ) ) {
				return;
			}
			if( IsValidHardwareValue( value ) ) {
				weight += partWeight;
				parts.Add( key + "=" + value );
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
					if( string.IsNullOrWhiteSpace( v ) == false ) {
						return v.Trim();
					}
				}
			}
			return string.Empty;
		}

		#endregion

		#region License Info

		public struct OldLicenseInfo
		{
			public string IssueDateUTC;
			public string RequestTimeUTC;
			public string Customer;
			public string LicenseCheckerVersion; // nullable
			public string RenewalReason;         // nullable
		}

		public static bool TryGetOldLicenseInfo( Dictionary<string, string> keyValue, out OldLicenseInfo licenseInfo )
		{
			licenseInfo = new OldLicenseInfo();

			if( !keyValue.TryGetValue( "IssueDateUTC", out string issueDateUTC ) || string.IsNullOrWhiteSpace( issueDateUTC ) ) {
				return false;
			}
			if( !keyValue.TryGetValue( "RequestTimeUTC", out string requestTimeUTC ) || string.IsNullOrWhiteSpace( requestTimeUTC ) ) {
				return false;
			}
			if( !keyValue.TryGetValue( "Customer", out string customer ) || string.IsNullOrWhiteSpace( customer ) ) {
				return false;
			}

			keyValue.TryGetValue( "LicenseCheckerVersion", out string licenseCheckerVersion );

			licenseInfo = new OldLicenseInfo
			{
				IssueDateUTC = issueDateUTC,
				RequestTimeUTC = requestTimeUTC,
				Customer = customer,
				LicenseCheckerVersion = licenseCheckerVersion
			};
			return true;
		}

		#endregion
	}
}
