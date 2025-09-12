using System;
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
			catch( Exception ex ) {
				MessageBox.Show( "IsLicenseActivated exception: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}
		}
	}

	public static partial class LicenseChecker
	{
		const string REQUEST_FILE_NAME = "ActivationRequest.txt";
		const string LICENSE_FILE_NAME = "License.key";

		static string LICENSE_FILE_PATH => Path.Combine( AppDomain.CurrentDomain.BaseDirectory, LICENSE_FILE_NAME );

		const string PUBLIC_KEY_XML = "<RSAKeyValue><Modulus>zNT894WmvUZ/8pTV+8DK6giAjNhyMsBRuWpxetMPpOy6K9L3VqyZz5PbqzekKqOj0UUzZFmfQetut83wjiH3LOwBmmKXJZDP+P//4bJ5sgupLwklFlYtBfH6LpfZzgxL67jFY7nJGwhtUbw8iZdyVZls0FY03YKAwJKpl4PBq8jEGiRdrcK08SHMSsxjuts6Rp3YkZg1Qtk/z2xi6CLsoEHXuGW/aiZlFK0DLvcO6Eh4BQ5Ubw0KHZruRsSs2LNElyQLXN1OjRP4R8Mk/zpdosSrFjVF9OxhXaikxTCt4SVjl6MEEx8FgE759q8kqahxkwJuvPJ4ArAiZKSu4wuDIQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

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
				MessageBox.Show( "License file is empty or unreadable.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}

			// Parse license file into key-value pairs and extract signature.
			if( !TryParseLicenseFile( lines, out Dictionary<string, string> keyValue, out List<string> orderedDataLines, out string szSignature ) ) {
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}

			// Verify signature.
			if( !VerifySignature( orderedDataLines, szSignature ) ) {
				MessageBox.Show( "License file signature verification failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
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
			catch( Exception ex ) {
				MessageBox.Show( $"Failed to read license file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return null;
			}
		}

		static bool TryParseLicenseFile( List<string> lines, out Dictionary<string, string> keyValue, out List<string> orderedDataLines, out string szSignature )
		{
			keyValue = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
			orderedDataLines = new List<string>();
			szSignature = null;

			foreach( string line in lines ) {
				int nIdx = line.IndexOf( '=' );
				if( nIdx <= 0 ) {
					continue;
				}

				string szKey = line.Substring( 0, nIdx ).Trim();
				string szValue = line.Substring( nIdx + 1 ).Trim();

				if( szKey.Equals( "Signature", StringComparison.OrdinalIgnoreCase ) ) {
					szSignature = szValue;
				}
				else {
					keyValue[ szKey ] = szValue;
					orderedDataLines.Add( $"{szKey}={szValue}" );
				}
			}

			if( string.IsNullOrWhiteSpace( szSignature ) ) {
				MessageBox.Show( "License file does not contain a signature.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}

			return true;
		}

		static bool ValidateLicenseContent( Dictionary<string, string> keyValue )
		{
			if( !keyValue.TryGetValue( "Customer", out string szCustomer ) ||
				!keyValue.TryGetValue( "HWID", out string szHwid ) || string.IsNullOrWhiteSpace( szHwid ) ) {
				MessageBox.Show( "License file missing required fields (Cutsomer/HWID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
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
				MessageBox.Show( "Entry assembly is null, cannot validate version.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}
			AssemblyName asmName = entry.GetName();

			if( !keyValue.TryGetValue( "Product", out string szProduct ) ||
				!keyValue.TryGetValue( "MinAppVersion", out string szMinVersion ) ||
				!keyValue.TryGetValue( "MaxAppVersion", out string szMaxVersion ) ) {
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}

			if( !string.Equals( szProduct, asmName.Name, StringComparison.OrdinalIgnoreCase ) ) {
				MessageBox.Show( $"License product does not match current application. Expected: {szProduct}, Current: {asmName.Name}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.LicenseFileInvalid );
			}
			if( Version.TryParse( szMinVersion, out Version minVersion ) && asmName.Version < minVersion ) {
				MessageBox.Show( $"License minimum version requirement not met. Required: {minVersion}, Current: {asmName.Version}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return HandleLicenseValidationFailure( FrmLicenseValidationFailed.EButtonShowMode.RestartActivation, FrmLicenseValidationFailed.EHintID.VersionMismatch );
			}
			if( Version.TryParse( szMaxVersion, out Version maxVersion ) && asmName.Version > maxVersion ) {
				MessageBox.Show( $"License maximum version exceeded. Allowed: {maxVersion}, Current: {asmName.Version}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
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
				MessageBox.Show( "License code cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}

			try {
				byte[] bytes = Convert.FromBase64String( szLicenseCode.Trim() );
				string szText = Encoding.UTF8.GetString( bytes );
				if( szText.Contains( "\r\n" ) ) {
					szText = szText.Replace( "\r\n", "\n" );
				}

				// Simple check if there is a signature line.
				if( !szText.Contains( "Signature=" ) ) {
					MessageBox.Show( "Input license code does not contain a signature.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
					return false;
				}

				File.WriteAllText( LICENSE_FILE_PATH, szText.Replace( "\n", Environment.NewLine ), Encoding.UTF8 );
				return true;
			}
			catch( FormatException ex ) {
				MessageBox.Show( "License code is not in valid Base64 format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}
			catch( Exception ex ) {
				MessageBox.Show( $"Failed to install license: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}
		}

		static bool VerifySignature( List<string> orderedDataLines, string szSignature )
		{
			string szDataBlock = string.Join( "\n", orderedDataLines );
			byte[] dataBytes = Encoding.UTF8.GetBytes( szDataBlock );
			byte[] signature = Convert.FromBase64String( szSignature );

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
			return string.Equals( szHwid, szCurrentHash, StringComparison.OrdinalIgnoreCase );
		}

		static void GenerateActivationRequest()
		{
			Assembly entry = Assembly.GetEntryAssembly();
			if( entry == null ) {
				MessageBox.Show( "Entry assembly is null, cannot generate activation request.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			AssemblyName asmName = entry.GetName();
			string szProduct = asmName.Name;
			string szVersion = asmName.Version.ToString();

			if( string.IsNullOrWhiteSpace( szProduct ) ) {
				MessageBox.Show( "Product can not be null or empty, cannot generate activation request.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}
			if( string.IsNullOrWhiteSpace( szVersion ) ) {
				MessageBox.Show( "Version can not be null or empty, cannot generate activation request.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			string szBaseDir = AppDomain.CurrentDomain.BaseDirectory;
			string szReqPath = Path.Combine( szBaseDir, REQUEST_FILE_NAME );
			string szMachineHash = ComputeMachineFingerprintHash( out string szRawDetail );

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine( $"RequestId={Guid.NewGuid():N}" );
			stringBuilder.AppendLine( $"Product={szProduct}" );
			stringBuilder.AppendLine( $"Version={szVersion}" );
			stringBuilder.AppendLine( $"FingerprintRaw={szRawDetail}" );
			stringBuilder.AppendLine( $"MachineHash={szMachineHash}" );
			stringBuilder.AppendLine( $"RequestTimeUTC={DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}" );
			File.WriteAllText( szReqPath, stringBuilder.ToString(), Encoding.UTF8 );
			MessageBox.Show( $"授權啟動請求檔案產生於:\n{szReqPath}\n\n 請將此檔案傳送給供應商以取得授權代碼。", "Activation Request Generated", MessageBoxButtons.OK, MessageBoxIcon.Information );
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
				var fullHex = BitConverter.ToString( hash ).Replace( "-", "" );

				// Get first 32 hex.
				return fullHex.Substring( 0, 32 );
			}
		}

		static string GetSystemDiskSerial()
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher( "SELECT VolumeSerialNumber, Name FROM Win32_LogicalDisk WHERE DriveType=3" );
			using( ManagementObjectCollection results = searcher.Get() ) {
				foreach( ManagementObject managementObj in results.Cast<ManagementObject>() ) {
					string szName = ( managementObj[ "Name" ] as string ) ?? string.Empty;
					if( szName.StartsWith( "C", StringComparison.OrdinalIgnoreCase ) ) {
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
