using MyCAM.Data;
using MyCAM.PathCache;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyCAM.Post
{
	internal static class ExtendedGCodeWriter
	{
		public static void WriteExtendedGCode( StreamWriter writer, CraftData craftData, ExtendedGCodeType gCodeType )
		{
			if( writer == null || craftData == null || craftData.ExtendedGCodeMap == null ) {
				return;
			}

			if( gCodeType == ExtendedGCodeType.ControlPoint ) {
				return;
			}

			if( !craftData.ExtendedGCodeMap.TryGetValue( Tuple.Create( gCodeType, (int?)null ), out string szContent ) ) {
				return;
			}

			WriteContent( writer, szContent );
		}

		public static void WriteContent( StreamWriter writer, string szContent )
		{
			if( writer == null || string.IsNullOrWhiteSpace( szContent ) ) {
				return;
			}

			// normalize the string content and write line
			string[] lineList = szContent.Split( new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries );
			foreach( string szLine in lineList ) {
				string szTrimmed = szLine.Trim();
				if( szTrimmed.Length == 0 ) {
					continue;
				}
				writer.WriteLine( szTrimmed );
			}
		}

		// relies on PostPointList being one-to-one with CAMPointList
		public static Dictionary<int, string> BuildControlPointGCodeMap( string szPathID, CraftData craftData, int nMainPathPointCount )
		{
			Dictionary<int, string> resultMap = new Dictionary<int, string>();

			if( craftData == null || craftData.ExtendedGCodeMap == null || nMainPathPointCount <= 0 ) {
				return resultMap;
			}

			foreach( KeyValuePair<Tuple<ExtendedGCodeType, int?>, string> kvp in craftData.ExtendedGCodeMap ) {
				if( kvp.Key.Item1 != ExtendedGCodeType.ControlPoint || kvp.Key.Item2 == null ) {
					continue;
				}
				int camIndex = kvp.Key.Item2.Value;

				// should not happen, CAM index should be remapped after CAMPoint changed
				if( camIndex < 0 || camIndex >= nMainPathPointCount ) {
					continue;
				}

				resultMap[ camIndex ] = kvp.Value;
			}

			return resultMap;
		}
	}
}
