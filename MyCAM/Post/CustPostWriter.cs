using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MyCAM.Post
{
	internal class CustPostWriter
	{
		public static void WriteCustomizedSection( StreamWriter writer, string content, Dictionary<string, string> variableDict )
		{
			if( writer == null ) {
				return;
			}
			// replace variables in content (even if empty)
			string replaced = ReplaceVariables( content, variableDict );
			if( replaced == string.Empty ) {
				return;
			}
			writer.WriteLine( replaced );
		}

		static string ReplaceVariables( string rawInfo, Dictionary<string, string> variables )
		{
			// protection
			if( string.IsNullOrEmpty( rawInfo ) ) {
				return rawInfo;
			}

			if( variables == null ) {
				variables = new Dictionary<string, string>();
			}

			// replace {var} with variables[ var ]
			return Regex.Replace( rawInfo, @"\{(?<varName>.*?)\}", match =>
			{
				var key = match.Groups[ "varName" ].Value;
				return variables.TryGetValue( key, out var value ) ? value : match.Value;
			} );
		}
	}
}
