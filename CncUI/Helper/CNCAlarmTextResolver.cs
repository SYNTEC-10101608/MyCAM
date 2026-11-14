using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using System.Xml;
using CncManager.Definition;

namespace CncUI
{
	internal sealed class CNCAlarmTextResolver
	{
		public CNCAlarmTextResolver( string directory )
		{
			foreach( string folder in new[] { "CHT", "CHS", "COM" } ) {
				Dictionary<string, string> messages = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
				m_Messages[ folder ] = messages;
				string languageDirectory = Path.Combine( directory, folder );
				try {
					string[] files = Directory.GetFiles( languageDirectory, "*.xml" );
					Array.Sort( files, StringComparer.OrdinalIgnoreCase );
					foreach( string file in files ) {
						LoadFile( file, messages );
					}
				}
				catch( Exception ex ) when( IsResourceError( ex ) ) {
					Trace.TraceWarning( "CNC alarm resource directory could not be read: {0}: {1}", languageDirectory, ex.Message );
				}
			}
		}

		public static CNCAlarmTextResolver Default => m_Default.Value;

		public string Resolve( CncAlarm alarm )
		{
			string folder;
			switch( LanguageManager.LanguageManager.CurrentUILanguage ) {
				case LanguageManager.LanguageManager.CHINESE_Traditional:
					folder = "CHT";
					break;
				case LanguageManager.LanguageManager.CHINESE_Simplified:
					folder = "CHS";
					break;
				default:
					folder = "COM";
					break;
			}
			Dictionary<string, string> messages = m_Messages[ folder ];
			string category = GetCategory( alarm.Category );
			string text;
			bool found = messages.TryGetValue( GetKey( category, alarm.AlarmID, alarm.SubAlarmID ), out text );
			if( !found ) {
				messages.TryGetValue( GetKey( category, alarm.AlarmID, -1 ), out text );
			}
			if( string.IsNullOrWhiteSpace( text ) ) {
				return alarm.Text;
			}
			if( string.IsNullOrWhiteSpace( alarm.Text ) || string.Equals( text, alarm.Text.Trim(), StringComparison.Ordinal ) ) {
				return text;
			}
			return text + Environment.NewLine + alarm.Text;
		}

		internal static string GetCategory( string category )
		{
			category = category ?? string.Empty;
			return m_CategoryAliases.TryGetValue( category, out string alias ) ? alias : category;
		}

		void LoadFile( string file, Dictionary<string, string> messages )
		{
			try {
				XmlReaderSettings settings = new XmlReaderSettings
				{
					DtdProcessing = DtdProcessing.Prohibit,
					XmlResolver = null,
				};
				XmlDocument document = new XmlDocument { XmlResolver = null };
				using( XmlReader reader = XmlReader.Create( file, settings ) ) {
					document.Load( reader );
				}
				foreach( XmlElement message in document.SelectNodes( "/ResMap/Message" ) ) {
					string text = message.GetAttribute( "Content" ).Trim();
					if( text.Length == 0 || !TryGetKey( message.GetAttribute( "ID" ), out string key ) ) {
						continue;
					}
					if( messages.TryGetValue( key, out string existing ) && !string.Equals( existing, text, StringComparison.Ordinal ) ) {
						// Conflicting definitions must not select an arbitrary alarm description.
						messages[ key ] = null;
						Trace.TraceWarning( "Conflicting CNC alarm resource: {0} ({1})", key, file );
					}
					else {
						messages[ key ] = text;
					}
				}
			}
			catch( Exception ex ) when( IsResourceError( ex ) ) {
				Trace.TraceWarning( "CNC alarm resource could not be loaded: {0}: {1}", file, ex.Message );
			}
		}

		static bool TryGetKey( string id, out string key )
		{
			key = null;
			const string PREFIX = "AlarmMsg::";
			const string SEPARATOR = "::ID=";
			if( !id.StartsWith( PREFIX, StringComparison.Ordinal ) ) {
				return false;
			}
			int separator = id.IndexOf( SEPARATOR, PREFIX.Length, StringComparison.Ordinal );
			if( separator <= PREFIX.Length ) {
				return false;
			}
			string category = id.Substring( PREFIX.Length, separator - PREFIX.Length );
			string number = id.Substring( separator + SEPARATOR.Length );
			bool isHex = number.EndsWith( "h", StringComparison.OrdinalIgnoreCase );
			if( isHex ) {
				number = number.Substring( 0, number.Length - 1 );
			}
			string[] parts = number.Split( '-' );
			NumberStyles style = isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None;
			if( parts.Length > 2 || !int.TryParse( parts[ 0 ], style, CultureInfo.InvariantCulture, out int alarmID ) || alarmID < 0 ) {
				return false;
			}
			int subAlarmID = -1;
			if( parts.Length == 2 && ( !int.TryParse( parts[ 1 ], style, CultureInfo.InvariantCulture, out subAlarmID ) || subAlarmID < 0 ) ) {
				return false;
			}
			key = GetKey( category, alarmID, subAlarmID );
			return true;
		}

		static string GetKey( string category, int alarmID, int subAlarmID )
		{
			return string.Format( CultureInfo.InvariantCulture, "{0}::{1}::{2}", category, alarmID, subAlarmID );
		}

		static bool IsResourceError( Exception ex )
		{
			return ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException || ex is XmlException;
		}

		readonly Dictionary<string, Dictionary<string, string>> m_Messages = new Dictionary<string, Dictionary<string, string>>( StringComparer.OrdinalIgnoreCase );

		static readonly Lazy<CNCAlarmTextResolver> m_Default = new Lazy<CNCAlarmTextResolver>( () =>
			new CNCAlarmTextResolver( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Resource", "Language", "CNCAlarm" ) ) );

		static readonly Dictionary<string, string> m_CategoryAliases = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
		{
			{ "MOTOR", "Motion" },
			{ "MOT", "Motion" },
			{ "OP", "Operation" },
			{ "COM", "Compiler" },
			{ "COR", "Coordinate" },
			{ "SPD", "Spindle" },
			{ "PLCALARM", "MLC" },
			{ "PLC", "MLC" },
			{ "PLCHINT", "MLCHint" },
			{ "PLCH", "MLCHint" },
			{ "MAR", "Macro" },
			{ "COMPILIER", "Compiler" },
			{ "BACKGNDEXECUTER", "BackgndExe" },
			{ "BGND", "BackgndExe" },
			{ "LASER", "LaserCtrl" },
			{ "EXT_STROKELIMIT", "EXTSTROKELIMIT" },
			{ "M3Syn", "Drv_M3_SYNTEC" },
			{ "ECAT_D", "Drv_ECAT_DELTA" },
			{ "DRV_ECAT_PANA_ALM", "Drv_ECAT_PSONIC_ALM" },
			{ "DRV_ECAT_PANA_WARNING", "Drv_ECAT_PSONIC_WARN" },
			{ "DRV_RTEX_PANA_ALM", "Drv_RTEX_PSONIC_ALM" },
			{ "DRV_RTEX_PANA_WARNING", "Drv_RTEX_PSONIC_WARN" },
			{ "DRV_ECAT_SVTRONIX_WARNING", "Drv_ECAT_SVTRONIX_WRN" },
			{ "DRV_ECAT_MITSU_WARNING", "Drv_ECAT_MITSU_WRN" },
			{ "DRV_ECAT_INVENTOR_WARNING", "Drv_ECAT_INVENTOR_WRN" },
			{ "DRV_ECAT_INOVANCE_WARNING", "Drv_ECAT_INOVANCE_WRN" },
			{ "DRV_ECAT_SYNTEC_WARNING", "Drv_ECAT_SYNTEC_WRN" },
		};
	}
}
