using System;
using System.Globalization;

namespace LicenseGenerator
{
	internal class Program
	{
		static void Main( string[] _ )
		{
			Console.Title = "License Generator";
			Console.WriteLine( "=== License Generator ===" );
			Console.WriteLine( "Needs existing ActivationRequest.txt in current directory" );
			Console.WriteLine( "Type 'exit' to quit at any prompt" );
			Console.WriteLine();

			string szCustomer = PromptCustomer();
			if( szCustomer == null ) {
				return;
			}

			DateTime? expireDateUtc = PromptExpireDateUtc();
			if( expireDateUtc == DateTime.MinValue ) {
				return;
			}

			string szMinVer = PromptMinAppVersion();
			if( szMinVer == null ) {
				return;
			}

			string szMaxVer = PromptMaxAppVersion();
			if( szMaxVer == null ) {
				return;
			}

			// Simple check version.
			if( string.IsNullOrWhiteSpace( szMinVer ) == false &&
				string.IsNullOrWhiteSpace( szMaxVer ) == false &&
				string.Compare( szMinVer, szMaxVer, StringComparison.Ordinal ) > 0 ) {
				Console.WriteLine();
				Console.WriteLine( "Error: MinAppVersion cannot be greater than MaxAppVersion." );
				return;
			}

			try {
				var result = GenerateLicense.Generate(
					"ActivationRequest.txt",
					szCustomer,
					expireDateUtc,
					szMinVer,
					szMaxVer
				);

				Console.WriteLine();
				Console.WriteLine( $"License.key generated: {result.LicenseFilePath}" );
				Console.WriteLine( $"Signature: {result.Signature}" );
				Console.WriteLine();
				Console.WriteLine( "License Code (Base64): " );
				Console.WriteLine( result.LicenseCodeBase64 );
			}
			catch( Exception ex ) {
				Console.WriteLine();
				Console.WriteLine( $"Generate license failed: {ex.Message}" );
			}

			Console.WriteLine();
			Console.WriteLine( "Press any key to exit..." );
			Console.ReadKey();
		}

		static string PromptCustomer()
		{
			while( true ) {
				Console.Write( "Customer: " );
				string szInput = Console.ReadLine();
				if( szInput == null ) {
					return null;
				}
				szInput = szInput.Trim();
				if( szInput.Equals( "exit", StringComparison.OrdinalIgnoreCase ) ) {
					return null;
				}
				if( szInput.Length == 0 ) {
					Console.WriteLine( "Customer cannot be empty." );
					continue;
				}
				return szInput;
			}
		}

		static DateTime? PromptExpireDateUtc()
		{
			while( true ) {
				Console.Write( "ExpireDate UTC (Format yyyy-MM-dd or leave empty for no expiration): " );
				string szInput = Console.ReadLine();
				if( szInput == null ) {
					return DateTime.MinValue;
				}
				szInput = szInput.Trim();
				if( szInput.Equals( "exit", StringComparison.OrdinalIgnoreCase ) ) {
					return DateTime.MinValue;
				}
				if( szInput.Length == 0 ) {
					return null;
				}
				if( DateTime.TryParseExact( szInput, "yyyy-MM-dd", CultureInfo.InvariantCulture,
					 DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt ) ) {
					return new DateTime( dt.Year, dt.Month, dt.Day, 23, 59, 59, DateTimeKind.Utc );
				}
				Console.WriteLine( "Format incorrect." );
				continue;
			}
		}

		static string PromptMinAppVersion()
		{
			while( true ) {
				Console.Write( $"MinAppVersion (Format x.x.x.x or leave empty for no limit, but currently should set 0.1.0.0): " );
				string szInput = Console.ReadLine();
				if( szInput == null ) {
					return null;
				}
				szInput = szInput.Trim();
				if( szInput.Equals( "exit", StringComparison.OrdinalIgnoreCase ) ) {
					return null;
				}
				if( szInput.Length == 0 ) {
					return string.Empty;
				}

				// Simple check x.x.x.x format.
				if( CheckVersionFormat( szInput ) == false ) {
					Console.WriteLine( "Version format incorrect." );
					continue;
				}
				return szInput;
			}
		}

		static string PromptMaxAppVersion()
		{
			while( true ) {
				Console.Write( $"MaxAppVersion (Format x.x.x.x or leave empty for no limit, but currently should set 0.99.99.0): " );
				string szInput = Console.ReadLine();
				if( szInput == null ) {
					return null;
				}
				szInput = szInput.Trim();
				if( szInput.Equals( "exit", StringComparison.OrdinalIgnoreCase ) ) {
					return null;
				}
				if( szInput.Length == 0 ) {
					return string.Empty;
				}

				// Simple check x.x.x.x format.
				if( CheckVersionFormat( szInput ) == false ) {
					Console.WriteLine( "Version format incorrect." );
					continue;
				}
				return szInput;
			}
		}

		static bool CheckVersionFormat( string szInput )
		{
			// Simple check x.x.x.x format.
			int nDot1 = szInput.IndexOf( '.' );
			int nDot2 = szInput.IndexOf( '.', nDot1 + 1 );
			int nDot3 = szInput.LastIndexOf( '.' );
			if( nDot1 > 0 && nDot2 > nDot1 && nDot3 > nDot2 && nDot3 < szInput.Length - 1 ) {
				if( int.TryParse( szInput.Substring( 0, nDot1 ), out _ ) &&
					int.TryParse( szInput.Substring( nDot1 + 1, nDot2 - nDot1 - 1 ), out _ ) &&
					int.TryParse( szInput.Substring( nDot2 + 1, nDot3 - nDot2 - 1 ), out _ ) &&
					int.TryParse( szInput.Substring( nDot3 + 1 ), out _ ) ) {
					return true;
				}
			}
			return false;
		}
	}
}
