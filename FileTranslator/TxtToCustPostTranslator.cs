using CommonData;
using System.Collections.Generic;

namespace FileTranslator
{
	public class TxtToCustPostTranslator
	{
		public bool Translate( string rawTxt, out CustPost custPost )
		{
			custPost = new CustPost();
			if( !ParseSections( rawTxt, out Dictionary<string, string> CustPostContentDict ) ) {
				return false;
			}

			// default values for each section
			Dictionary<string, string> defaultContentDict = new Dictionary<string, string>()
			{
				{ CustPost.EPostSection.Header.ToString(), CustPost.DEFAULT_Header },
				{ CustPost.EPostSection.Tail.ToString(), CustPost.DEFAULT_Tail },
				{ CustPost.EPostSection.PathHeader.ToString(), CustPost.DEFAULT_PathHeader },
				{ CustPost.EPostSection.PathTail.ToString(), CustPost.DEFAULT_PathTail },
				{ CustPost.EPostSection.StdPathHeader.ToString(), CustPost.DEFAULT_StdPathHeader },
				{ CustPost.EPostSection.StdPathTail.ToString(), CustPost.DEFAULT_StdPathTail },
				{ CustPost.EPostSection.TraverseHeader.ToString(), CustPost.DEFAULT_TraverseHeader },
				{ CustPost.EPostSection.TraverseTail.ToString(), CustPost.DEFAULT_TraverseTail }
			};

			// flag to indicate if all sections exist in txt
			bool isAllExist = true;

			// get each section content
			var keys = new List<string>( defaultContentDict.Keys );
			foreach( string key in keys ) {

				// if the section exist in txt, use it; otherwise, use default value and mark allExist as false
				if( GetCustPostValue( CustPostContentDict, key, out var value ) ) {
					defaultContentDict[ key ] = value;
				}
				else {
					isAllExist = false;
				}
			}
			custPost = new CustPost( defaultContentDict );
			return isAllExist;
		}

		bool GetCustPostValue( Dictionary<string, string> dict, string key, out string value )
		{
			return dict.TryGetValue( key, out value );
		}

		bool ParseSections( string text, out Dictionary<string, string> dict )
		{
			dict = new Dictionary<string, string>();
			if( string.IsNullOrEmpty( text ) ) {
				return false;
			}

			// key is property name, value is content
			string currentKey = null;
			var buffer = new System.Text.StringBuilder();
			for( int i = 0; i < text.Length; ) {
				if( text[ i ] == '<' ) {

					// store previous section
					if( currentKey != null ) {
						dict[ currentKey ] = RemoveEmptyLines( buffer.ToString().Trim() );
						buffer.Clear();
					}
					// get new key
					int end = text.IndexOf( '>', i + 1 );

					// this key can't find '>', stop parsing and store the rest as content of current key
					if( end == -1 ) {
						break;
					}
					currentKey = text.Substring( i + 1, end - i - 1 ).Trim();
					i = end + 1;
				}
				else {
					buffer.Append( text[ i ] );
					i++;
				}
			}

			// store last section
			if( currentKey != null ) {
				dict[ currentKey ] = RemoveEmptyLines( buffer.ToString().Trim() );
			}
			return true;
		}

		static string RemoveEmptyLines( string text )
		{
			if( string.IsNullOrEmpty( text ) ) {
				return string.Empty;
			}

			// separate lines and remove empty/whitespace lines
			string[] stringLines = text.Split( new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.None );
			List<string> nonEmptyLines = new List<string>();
			foreach( var line in stringLines ) {

				// only add lines that are not empty or whitespace
				if( !string.IsNullOrWhiteSpace( line ) ) {
					nonEmptyLines.Add( line );
				}
			}

			// stick each line
			return string.Join( System.Environment.NewLine, nonEmptyLines );
		}
	}
}
