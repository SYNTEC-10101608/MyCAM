using System;
using System.Security.Cryptography;

namespace RsaKeyGenerator
{
	internal class Program
	{
		static void Main( string[] _ )
		{
			// 2048 is enough for most cases.
			// 3072 or 4096 is more secure, but slower.
			using( var rsa = new RSACryptoServiceProvider( 2048 ) ) {

				// Set PersistKeyInCsp to false to avoid writing key to container.
				// It is recommended to save the key manually after generation.
				rsa.PersistKeyInCsp = false;

				// Export public and private key as XML string.
				// Set false to get the public key. It contains only Modulus and Exponent.
				// Set true to get the private key. It contains all parameters. Keep it secret.
				string szPublicXml = rsa.ToXmlString( false );
				string szPrivateXml = rsa.ToXmlString( true );

				Console.WriteLine( "=== PUBLIC KEY ===" );
				Console.WriteLine( szPublicXml );
				Console.WriteLine();
				Console.WriteLine( "=== PRIVATE KEY ===" );
				Console.WriteLine( szPrivateXml );
				Console.WriteLine();
				Console.WriteLine( "=== PLEASE SAVE THE PRIVATE KEY IN A SAFE PLACE ===" );
			}

			Console.WriteLine();
			Console.WriteLine( "Press any key to exit..." );
			Console.ReadKey();
		}
	}
}
