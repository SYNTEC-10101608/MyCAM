using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LicenseGenerator
{
	internal static partial class GenerateLicense
	{
		internal static GenerateResult Generate( string szActivationRequestPath,
													string szCustomer,
													DateTime? expireDateUtc,
													string szMinAppVersion,
													string szMaxAppVersion )
		{
			if( string.IsNullOrWhiteSpace( szActivationRequestPath ) ) {
				throw new ArgumentException( "szActivationRequestPath is empty." );
			}
			if( string.IsNullOrWhiteSpace( szCustomer ) ) {
				throw new ArgumentException( "szCustomer is empty." );
			}

			if( File.Exists( szActivationRequestPath ) == false ) {
				throw new FileNotFoundException( "ActivationRequest.txt not found.", szActivationRequestPath );
			}

			var reqDict = ParseKeyValueFile( szActivationRequestPath );

			string szRequestId = GetRequired( reqDict, "RequestId" );
			string szProduct = GetRequired( reqDict, "Product" );
			string szProductVersion = GetRequired( reqDict, "Version" );
			string szFingerprintRaw = GetRequired( reqDict, "FingerprintRaw" );
			string szMachineHash = GetRequired( reqDict, "MachineHash" );
			string szRequestTimeUtc = GetRequired( reqDict, "RequestTimeUTC" );

			// Will throw if invalid
			IsActivationRequestValid( szProduct, szProductVersion, szRequestTimeUtc );

			var sb = new StringBuilder();

			sb.AppendLine( $"LicenseId={Guid.NewGuid():N}" );
			sb.AppendLine( $"Customer={szCustomer}" );
			sb.AppendLine( $"HWID={szMachineHash}" );
			sb.AppendLine( $"FingerprintRaw={szFingerprintRaw}" );
			sb.AppendLine( $"IssueDateUTC={DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}" );
			sb.AppendLine( $"ExpireDateUtc={( expireDateUtc.HasValue ? expireDateUtc.Value.ToString( "yyyy-MM-dd HH:mm:ss" ) : "" )}" );
			sb.AppendLine( $"Product={szProduct}" );
			sb.AppendLine( $"Version={szProductVersion}" );
			sb.AppendLine( $"MinAppVersion={szMinAppVersion ?? ""}" );
			sb.AppendLine( $"MaxAppVersion={szMaxAppVersion ?? ""}" );
			sb.AppendLine( $"RequestId={szRequestId}" );
			sb.AppendLine( $"RequestTimeUTC={szRequestTimeUtc}" );

			string szBodyToSign = sb.ToString().TrimEnd();
			string szSignature = SignBody( szBodyToSign );

			sb.AppendLine( $"Signature={szSignature}" );

			string szLicenseContent = sb.ToString();
			string szBaseDir = AppDomain.CurrentDomain.BaseDirectory;
			string szOutPath = Path.Combine( szBaseDir, LICENSE_FILE_NAME );
			File.WriteAllText( szOutPath, szLicenseContent, Encoding.UTF8 );

			return new GenerateResult
			{
				LicenseFilePath = szOutPath,
				Signature = szSignature,
				LicenseCodeBase64 = Convert.ToBase64String( Encoding.UTF8.GetBytes( szLicenseContent ) )
			};
		}

		internal sealed class GenerateResult
		{
			public string LicenseFilePath
			{
				get; set;
			}
			public string Signature
			{
				get; set;
			}
			public string LicenseCodeBase64
			{
				get; set;
			}
		}
	}

	internal static partial class GenerateLicense
	{
		const string PRIVATE_KEY_XML = "<RSAKeyValue><Modulus>zNT894WmvUZ/8pTV+8DK6giAjNhyMsBRuWpxetMPpOy6K9L3VqyZz5PbqzekKqOj0UUzZFmfQetut83wjiH3LOwBmmKXJZDP+P//4bJ5sgupLwklFlYtBfH6LpfZzgxL67jFY7nJGwhtUbw8iZdyVZls0FY03YKAwJKpl4PBq8jEGiRdrcK08SHMSsxjuts6Rp3YkZg1Qtk/z2xi6CLsoEHXuGW/aiZlFK0DLvcO6Eh4BQ5Ubw0KHZruRsSs2LNElyQLXN1OjRP4R8Mk/zpdosSrFjVF9OxhXaikxTCt4SVjl6MEEx8FgE759q8kqahxkwJuvPJ4ArAiZKSu4wuDIQ==</Modulus><Exponent>AQAB</Exponent><P>4qGM6KMqCRxEgnRv6q9atxAHiRO5rgfZ906E/uJ5xA0/b43jhSzTcl25IcjSoZb1qsRJUhkYu05DPHKi6DgTBcvi9J4gfJG/UHGmN/C1Gu+dAaXNs4OusZ1yqzf4Aw6ZCdNBHw2S04AxF0S95QIrVLX/botDytDHSDppEtX18Q8=</P><Q>52BByMcGpvJV2agmvfC2yDpMUv9u5cKFY57g1MsZb0cUxiLJ6LAoSNgbYyoo3XDTkj0zXEoPfZkvNB/uSfVvyWH5v9rkI2Nq94I15SSy1aPTsXq+C9QMpVqRrHGKdoCOS6XtQDwFhXZZpGS8dLqVGj6P/PHH2JzVgpO7VqXI6M8=</Q><DP>023t/JdEPy2dCKvCQYsIsaKFtS+u2Zo/kgYx82ysNug5F0SjT3orZhXkAo3NW61vUbo/wRTiytPPzFQSgVzW+mBlQttG6ZA7kLwGW0f4XcHPk1rw1hny34dJFE1p1shGpDDvHx5VrXV51wBtvHnSnPwR6jqIq20tjcfYgRcVlKc=</DP><DQ>iCfmzmYAjiGxT4JaOg/7H9zeqUDTW7WalUMbf119PDSSDv7N3nY7XpJ9m19l6NnKRxZosPiMIRcR5hScPz2FaPaU58fB1OleOwSO/qcX7gyYOKrIR2Gjh3LCor22A4InOnEMKm6nuSAqJEl7dR2PPIBwbyNAvUGdi1ewOI8aqZU=</DQ><InverseQ>VWNW4QO1nbzJOf23E/XDUK1apa74+hKkAq9804y+pSx0HnWKLBa3iOKR1lCUAyyCKAacTQNRkXn21YVUlRMJ6DC3QAjTRVkVU5gkGevOlijWEQ7mOO0iw6mupVC0N61eXpCIHYlOorxg62epU2gMfz57ZR4RSwIYIGTdGq5zgCI=</InverseQ><D>s5GtS5oBuUFK/fF5PsGfolhdyhwe0VtEyhYE8jaCKoXWGIFUkMmz9gSbuZiH9VGMVfQiv8DKG1I4uC50y0X/mYQ1S5FPDIHNsW9MdiKvCWWrpi6QMurNgb7HI33AViD1letgMLM5GNfkbpllQQJiG6TrfiF1KxQzp59rygaaDOvdaOQCpyIl4in2MKyuISflIzQBhR58IlBKVdSGUEKboXLsb3QpGmbikRieYhuB4L/pjpFrTlYFC0OoIAhZbCaJaAhApnLZbRhTmtqZV2VAcwWcBePevYO2p7DIYJHyEEd3PT/pDztfm09NhwXXOKWVVi9iE+1bRG22hZxiO9M7OQ==</D></RSAKeyValue>";

		const string LICENSE_FILE_NAME = "license.key";

		static Dictionary<string, string> ParseKeyValueFile( string szPath )
		{
			var dict = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
			foreach( var line in File.ReadAllLines( szPath, Encoding.UTF8 ) ) {
				if( string.IsNullOrWhiteSpace( line ) ) {
					continue;
				}
				int nIdx = line.IndexOf( '=' );
				if( nIdx <= 0 ) {
					continue;
				}
				string szKey = line.Substring( 0, nIdx ).Trim();
				string szValue = line.Substring( nIdx + 1 ).Trim();
				if( szKey.Length == 0 ) {
					continue;
				}
				dict[ szKey ] = szValue;
			}
			return dict;
		}

		static string GetRequired( Dictionary<string, string> dict, string szKey )
		{
			if( dict.TryGetValue( szKey, out string szValue ) && string.IsNullOrWhiteSpace( szValue ) == false ) {
				return szValue;
			}
			throw new InvalidDataException( $"ActivationRequest file missing required key: {szKey}" );
		}

		static string SignBody( string szBody )
		{
			byte[] bytes = Encoding.UTF8.GetBytes( szBody.Replace( "\r\n", "\n" ) );
			using( var rsa = new RSACryptoServiceProvider( 2048 ) ) {
				rsa.FromXmlString( PRIVATE_KEY_XML );
				using( var sha = SHA256.Create() ) {
					byte[] hash = sha.ComputeHash( bytes );
					string szOid = CryptoConfig.MapNameToOID( "SHA256" );
					byte[] sig = rsa.SignHash( hash, szOid );
					return Convert.ToBase64String( sig );
				}
			}
		}

		static void IsActivationRequestValid( string szProduct, string szVersion, string szRequestTimeUTC )
		{
			// The product must be either "SocoStartUp" or "StandardStartUp".
			if( !string.Equals( szProduct, "MyCAM", StringComparison.OrdinalIgnoreCase ) ) {
				throw new InvalidDataException( "ActivationRequest.txt has invalid Product, must be 'MyCAM'." );
			}

			// The version must be at least 0.1.0.0.
			Version standardVersion = new Version( 0, 1, 0, 0 );
			if( !Version.TryParse( szVersion, out Version version )
				|| version < standardVersion ) {
				throw new InvalidDataException( "ActivationRequest.txt has invalid Version, must be at least 0.1.0.0." );
			}

			// RequestTimeUTC must be a valid UTC date time in the format "yyyy-MM-dd HH:mm:ss".
			if( !DateTime.TryParseExact( szRequestTimeUTC, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime requestTime ) ) {
				throw new InvalidDataException( "ActivationRequest.txt has invalid RequestTimeUTC, must be in the format 'yyyy-MM-dd HH:mm:ss'." );
			}


			// ============== This part can be coserved for now, as it may be useful in the future.===============
			//// RequestTimeUTC must be within 3 days of the current UTC date time.
			//if( requestTime < DateTime.UtcNow.AddDays( -3 ) || requestTime > DateTime.UtcNow.AddDays( 3 ) ) {
			//	throw new InvalidDataException( "ActivationRequest.txt has invalid RequestTimeUTC, must be within 3 days of the current UTC date time." );
			//}
			// ===================================================================================================
		}
	}
}
