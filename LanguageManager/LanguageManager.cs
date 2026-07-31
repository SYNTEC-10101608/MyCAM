using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace LanguageManager
{
	// public
	public partial class LanguageManager
	{
		public static event Action<string> RDLogHandler;

		// supported languages
		public const string CHINESE_Traditional = "zh-TW";
		public const string CHINESE_Simplified = "zh-CN";

		public static string CurrentUILanguage
		{
			get
			{
				return m_szCurrentUILanguage;
			}
			set
			{
				if( string.IsNullOrEmpty( value ) ) {
					return;
				}
				try {
					string szNewLanguage = value.ToLower();
					foreach( string szLanguage in CurrentSupportedLanguageList ) {
						if( szNewLanguage == szLanguage.ToLower() ) {
							m_szCurrentUILanguage = szLanguage;
						}
					}
				}
				catch( Exception ex ) {
					RDLogHandler?.Invoke( "[LanguageManager] CurrentUILanguage set failed: " + ex.ToString() );
				}
			}
		}

		public static List<string> CurrentSupportedLanguageList
		{
			get
			{
				if( m_CurrentSupportedLanguageList.Count == 0 ) {
					return m_SupportedLanguageDataMap.Select( data => data.Key ).ToList();
				}
				return new List<string>( m_CurrentSupportedLanguageList );
			}
			set
			{
				m_CurrentSupportedLanguageList.Clear();
				foreach( string szLanguage in value ) {
					if( m_SupportedLanguageDataMap.ContainsKey( szLanguage ) == true ) {
						m_CurrentSupportedLanguageList.Add( szLanguage );
					}
				}

				// protection
				if( m_CurrentSupportedLanguageList.Count == 0 ) {
					m_CurrentSupportedLanguageList = m_SupportedLanguageDataMap.Select( data => data.Key ).ToList();
				}
			}
		}

		public static bool CompareLanguageTag( string szLanguage1, string szLanguage2 )
		{
			if( string.IsNullOrEmpty( szLanguage1 ) || string.IsNullOrEmpty( szLanguage2 ) ) {
				return false;
			}
			try {
				return szLanguage1.ToLower() == szLanguage2.ToLower();
			}
			catch( Exception ex ) {
				RDLogHandler?.Invoke( "[LanguageManager] CompareLanguageTag failed: " + ex.ToString() );
				return false;
			}
		}

		public static int GetCNCParamValueByLanguage( string szLanguage )
		{
			if( string.IsNullOrEmpty( szLanguage ) ) {
				return 0;
			}
			try {
				// protection
				if( m_CurrentSupportedLanguageList.Count == 0 ) {
					return 0;
				}

				// the target language is not in supported list
				if( m_CurrentSupportedLanguageList.Contains( szLanguage ) == false ) {
					return 0;
				}

				// deep clone the LanguageData object
				LanguageData languageData = m_SupportedLanguageDataMap[ szLanguage ];
				LanguageData clonedLanguageData = new LanguageData()
				{
					CNCParamValue = languageData.CNCParamValue,
					NativeName = languageData.NativeName,
					DisplayName = new Dictionary<string, string>( languageData.DisplayName )
				};

				return clonedLanguageData.CNCParamValue;
			}
			catch( Exception ex ) {
				RDLogHandler?.Invoke( "[LanguageManager] GetCNCParamValueByLanguage failed: " + ex.ToString() );
				return 0;
			}
		}

		public static string GetNativeNameByLanguage( string szLanguage )
		{
			if( string.IsNullOrEmpty( szLanguage ) ) {
				return string.Empty;
			}
			try {
				if( m_SupportedLanguageDataMap.ContainsKey( szLanguage ) == false ) {
					return string.Empty;
				}
				return m_SupportedLanguageDataMap[ szLanguage ].NativeName;
			}
			catch( Exception ex ) {
				RDLogHandler?.Invoke( "[LanguageManager] GetNativeNameByLanguage failed: " + ex.ToString() );
				return string.Empty;
			}
		}

		public static string GetDisplayNameByLanguage( string szLanguage )
		{
			if( string.IsNullOrEmpty( szLanguage ) ) {
				return string.Empty;
			}
			try {
				// protection
				if( m_CurrentSupportedLanguageList.Count == 0 ) {
					return string.Empty;
				}

				// the display name not found
				if( m_CurrentSupportedLanguageList.Contains( CurrentUILanguage ) == false || m_SupportedLanguageDataMap[ CurrentUILanguage ].DisplayName == null ) {
					return string.Empty;
				}

				// the target languageData is not in supported list
				if( m_SupportedLanguageDataMap[ CurrentUILanguage ].DisplayName.ContainsKey( szLanguage ) == false ) {
					return string.Empty;
				}
				return m_SupportedLanguageDataMap[ CurrentUILanguage ].DisplayName[ szLanguage ];
			}
			catch( Exception ex ) {
				RDLogHandler?.Invoke( "[LanguageManager] GetDisplayNameByLanguage failed: " + ex.ToString() );
				return string.Empty;
			}
		}

		public LanguageManager( string szMapConfigFileName )
		{
			m_szMapConfigFileNameList = new List<string>() { szMapConfigFileName };
			InitResourceMap();
		}

		// with several config files
		public LanguageManager( List<string> szConfigFileNameList )
		{
			m_szMapConfigFileNameList = szConfigFileNameList;
			InitResourceMap();
		}

		public string GetString( string szID )
		{
			return GetString( szID, CurrentUILanguage );
		}

		public string GetString( string szID, string szLanguage )
		{
			if( string.IsNullOrEmpty( szLanguage ) ) {
				return string.Empty;
			}
			try {
				// the target language is not in supported list
				if( m_ResourceMap.ContainsKey( szLanguage ) == false || m_ResourceMap[ szLanguage ] == null ) {
					return string.Empty;
				}

				// invalid argument
				if( string.IsNullOrEmpty( szID ) ) {
					return string.Empty;
				}

				// design intent (dual audience):
				if( m_ResourceMap[ szLanguage ].ContainsKey( szID ) == false ) {
					LogMissingID( szID, szLanguage, m_szMapConfigFileNameList );
					return string.Empty;
				}
				return m_ResourceMap[ szLanguage ][ szID ];
			}
			catch( Exception ex ) {
				RDLogHandler?.Invoke( "[LanguageManager] GetString failed: " + ex.ToString() );
				return string.Empty;
			}
		}

		// look up a localized string by ID; fallback to the caller-provided default text.
		public string TryGetString( string szID, string szDefault )
		{
			if( string.IsNullOrEmpty( szID ) ) {
				return szDefault ?? string.Empty;
			}
			try {
				string szResult = GetString( szID );
				return string.IsNullOrEmpty( szResult ) ? ( szDefault ?? string.Empty ) : szResult;
			}
			catch( Exception ) {

				// GetString throws on invalid language, treat as not found and use fallback
				return szDefault ?? string.Empty;
			}
		}

		public void SetString( string szID, string szNewString )
		{
			SetString( szID, CurrentUILanguage, szNewString );
		}

		public void SetString( string szID, string szLanguage, string szNewString )
		{
			if( string.IsNullOrEmpty( szLanguage ) ) {
				return;
			}
			try {
				// error handling but should not happend
				if( m_ResourceMap.ContainsKey( szLanguage ) == false ) {
					m_ResourceMap.Add( szLanguage, new Dictionary<string, string>() );
				}
				else if( m_ResourceMap.ContainsKey( szLanguage ) && m_ResourceMap[ szLanguage ] == null ) {
					m_ResourceMap[ szLanguage ] = new Dictionary<string, string>();
				}

				// invalid argument
				if( string.IsNullOrEmpty( szID ) ) {
					return;
				}
				m_ResourceMap[ szLanguage ][ szID ] = szNewString;
				WriteResourceMapToXml( szLanguage );
			}
			catch( Exception ex ) {
				RDLogHandler?.Invoke( "[LanguageManager] SetString failed: " + ex.ToString() );
			}
		}
	}

	// protected
	public partial class LanguageManager
	{
	}

	// private
	public partial class LanguageManager
	{
		// config file
		List<string> m_szMapConfigFileNameList;
		const string DEFAULT_DIR_NAME = "Resource";
		const string CUSTOM_DIR_NAME = "Custom";
		const string LANGUAGE_DIR_NAME = "Language";
		const string DISPLAYNAME_FILE_NAME = "DisplayName";
		static readonly string XML_DIR = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, DEFAULT_DIR_NAME, LANGUAGE_DIR_NAME );
		static readonly string CUSTOM_XML_DIR = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, CUSTOM_DIR_NAME, LANGUAGE_DIR_NAME );

		// config format
		const string XML_EXTENSTION = ".xml";
		const string XML_ROOT_NAME = "ResMap";
		const string XML_NODE_NAME = "Message";
		const string XML_NODE_ATTRIBUTE_ID = "ID";
		const string XML_NODE_ATTRIBUTE_CONTENT = "Content";

		// support language
		struct LanguageData
		{
			// pr3209
			public int CNCParamValue;
			public string NativeName;

			// key = language, value = display name
			public Dictionary<string, string> DisplayName;
		}

		static List<string> m_CurrentSupportedLanguageList = new List<string>();

		static readonly Dictionary<string, LanguageData> m_SupportedLanguageDataMap = new Dictionary<string, LanguageData>(){
			{ CHINESE_Traditional, new LanguageData(){ CNCParamValue = 1, NativeName = "繁體中文", DisplayName = InitDisplayName( CHINESE_Traditional ) } },
			{ CHINESE_Simplified, new LanguageData(){ CNCParamValue = 119, NativeName = "简体中文", DisplayName = InitDisplayName( CHINESE_Simplified ) } },
		};

		static string m_szCurrentUILanguage = CHINESE_Traditional;

		// resource map
		Dictionary<string, Dictionary<string, string>> m_ResourceMap;

		void InitResourceMap()
		{
			m_ResourceMap = new Dictionary<string, Dictionary<string, string>>();
			foreach( string szOneLanguage in CurrentSupportedLanguageList ) {
				Dictionary<string, string> oneLanguageMap = LoadResourceMapFromXml( szOneLanguage );
				if( oneLanguageMap == null ) {
					continue;
				}
				m_ResourceMap.Add( szOneLanguage, oneLanguageMap );
			}
		}

		Dictionary<string, string> LoadResourceMapFromXml( string szLanguage )
		{
			if( m_szMapConfigFileNameList == null || m_szMapConfigFileNameList.Count == 0 ) {
				return new Dictionary<string, string>();
			}

			Dictionary<string, string> resourceMap = new Dictionary<string, string>();
			foreach( string szMapConfigFileName in m_szMapConfigFileNameList ) {
				string szFileName = szMapConfigFileName + "_" + szLanguage + XML_EXTENSTION;

				// load from custom directory first (customer override)
				bool bCustomLoaded = LoadXmlIntoMap( Path.Combine( CUSTOM_XML_DIR, szFileName ), szMapConfigFileName, szLanguage, resourceMap );

				// load from config directory, IDs already loaded from custom will be skipped
				bool bResourceLoaded = LoadXmlIntoMap( Path.Combine( XML_DIR, szFileName ), szMapConfigFileName, szLanguage, resourceMap );

				// both files missing, log it for diagnostics
				if( bCustomLoaded == false && bResourceLoaded == false ) {
					LogMissingFile( szFileName );
				}
			}
			return resourceMap;
		}

		// load xml file into map, IDs already in map will be silently skipped (custom override behavior)
		bool LoadXmlIntoMap( string szFilePath, string szMapConfigFileName, string szLanguage, Dictionary<string, string> resourceMap )
		{
			if( File.Exists( szFilePath ) == false ) {
				return false;
			}

			XDocument xmlDoc = null;
			try {

				// load the xml file
				xmlDoc = XDocument.Load( szFilePath );
			}
			catch( Exception ex ) {
				LogXmlLoadError( szFilePath, ex );

				return false;
			}

			if( xmlDoc == null ) {
				return false;
			}

			// create the map
			var allNode = xmlDoc.Descendants( XML_NODE_NAME );

			foreach( var node in allNode ) {

				// check if required attributes exist
				XAttribute idAttr = node.Attribute( XML_NODE_ATTRIBUTE_ID );
				XAttribute contentAttr = node.Attribute( XML_NODE_ATTRIBUTE_CONTENT );

				if( idAttr == null ) {
					string errorMsg = $"[LanguageManager Warning] Missing '{XML_NODE_ATTRIBUTE_ID}' attribute in {szMapConfigFileName}_{szLanguage}.xml. Skipping this node.";
					RDLogHandler?.Invoke( errorMsg );
					System.Diagnostics.Debug.WriteLine( errorMsg );
					continue;
				}

				if( contentAttr == null ) {
					string errorMsg = $"[LanguageManager Warning] Missing '{XML_NODE_ATTRIBUTE_CONTENT}' attribute for ID '{idAttr.Value}' in {szMapConfigFileName}_{szLanguage}.xml. Skipping this node.";
					RDLogHandler?.Invoke( errorMsg );
					System.Diagnostics.Debug.WriteLine( errorMsg );
					continue;
				}

				string id = idAttr.Value;
				string content = contentAttr.Value;

				// check for empty or null values
				if( string.IsNullOrWhiteSpace( id ) ) {
					string errorMsg = $"[LanguageManager Warning] Empty '{XML_NODE_ATTRIBUTE_ID}' in {szMapConfigFileName}_{szLanguage}.xml. Skipping this node.";
					RDLogHandler?.Invoke( errorMsg );
					System.Diagnostics.Debug.WriteLine( errorMsg );
					continue;
				}

				// skip IDs already loaded (duplicate within file, or already loaded from higher priority source)
				if( resourceMap.ContainsKey( id ) ) {
					continue;
				}

				resourceMap.Add( id, content ?? string.Empty );
			}
			return true;
		}

		void WriteResourceMapToXml( string szLanguage )
		{
			// the functions of writing xml are used in SOCO
			const int HEHE_FILE_INDEX = 0;

			// protection
			if( m_szMapConfigFileNameList == null || m_szMapConfigFileNameList.Count == 0 ) {
				return;
			}
			string szFilePath = Path.Combine( XML_DIR, m_szMapConfigFileNameList[ HEHE_FILE_INDEX ] + "_" + szLanguage + XML_EXTENSTION );

			// protection
			if( File.Exists( szFilePath ) == false ) {
				return;
			}

			XmlDocument xmlDoc = new XmlDocument();
			string szTempFilePath = Path.Combine( XML_DIR, m_szMapConfigFileNameList[ HEHE_FILE_INDEX ] + "_" + szLanguage + "_tmp" + XML_EXTENSTION );

			// root node
			XmlElement root = xmlDoc.CreateElement( XML_ROOT_NAME );

			// each data node
			foreach( KeyValuePair<string, string> data in m_ResourceMap[ szLanguage ] ) {
				XmlElement oneNode = xmlDoc.CreateElement( XML_NODE_NAME );
				oneNode.SetAttribute( XML_NODE_ATTRIBUTE_ID, data.Key );
				oneNode.SetAttribute( XML_NODE_ATTRIBUTE_CONTENT, data.Value );
				root.AppendChild( oneNode );
			}

			// save document
			xmlDoc.AppendChild( root );

			try {
				xmlDoc.Save( szTempFilePath );
				XDocument.Load( szTempFilePath );
			}
			catch( Exception ex ) {
				RDLogHandler?.Invoke( ex.ToString() );
				try {

					// delete the temp file
					if( File.Exists( szTempFilePath ) ) {
						File.Delete( szTempFilePath );
					}
				}
				catch( Exception deletEx ) {
					RDLogHandler?.Invoke( deletEx.ToString() );
				}
				return;
			}

			// replace the original file, the API Replace will auto-delete the temp file after replace
			try {
				File.Replace( szTempFilePath, szFilePath, null );
			}
			catch( Exception ex ) {
				RDLogHandler?.Invoke( ex.ToString() );
			}
		}

		static Dictionary<string, string> InitDisplayName( string szLanguage )
		{
			Dictionary<string, string> displayNameMap = new Dictionary<string, string>();

			// check the xml file exist
			string szFilePath = Path.Combine( XML_DIR, DISPLAYNAME_FILE_NAME + "_" + szLanguage + XML_EXTENSTION );
			if( File.Exists( szFilePath ) == false ) {
				return null;
			}

			XDocument xmlDoc = null;
			try {

				// load the xml file
				xmlDoc = XDocument.Load( szFilePath );
			}
			catch( Exception ex ) {
				if( ex is XmlException ) {
					MessageBox.Show( DISPLAYNAME_FILE_NAME + "_" + szLanguage + ".xml" + " file format error.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				}
				else {
					MessageBox.Show( ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				}
				RDLogHandler?.Invoke( ex.ToString() );

				// use throw to stop the program
				throw;
			}

			if( xmlDoc == null ) {
				return null;
			}

			// create the map
			var allNode = xmlDoc.Descendants( XML_NODE_NAME );

			foreach( var node in allNode ) {

				// prevent from repeat
				if( displayNameMap.ContainsKey( node.Attribute( XML_NODE_ATTRIBUTE_ID ).Value ) ) {
					continue;
				}
				displayNameMap.Add( node.Attribute( XML_NODE_ATTRIBUTE_ID ).Value, node.Attribute( XML_NODE_ATTRIBUTE_CONTENT ).Value );
			}
			return displayNameMap;
		}

		// diagnostic log: Resource\Language\LanguageManagerDiagnostic.log
		const string DIAGNOSTIC_LOG_FILE_NAME = "LanguageManagerDiagnostic.log";
		static readonly string DIAGNOSTIC_LOG_FILE_PATH = Path.Combine( XML_DIR, DIAGNOSTIC_LOG_FILE_NAME );

		// dedup sets, scope = one process run
		static readonly HashSet<string> m_LoggedMissingFiles = new HashSet<string>();
		static readonly HashSet<string> m_LoggedMissingIDs = new HashSet<string>();
		static readonly object m_DiagnosticLogLock = new object();

		// whether the run-start separator has been written this process
		static bool m_bRunHeaderWritten = false;

		static void LogMissingFile( string szFileName )
		{
			string errorMsg = "[LanguageManager] Missing language file: " + szFileName;
			lock( m_DiagnosticLogLock ) {
				if( m_LoggedMissingFiles.Add( szFileName ) == false ) {
					return;
				}
				EnsureRunHeader();
				File.AppendAllText( DIAGNOSTIC_LOG_FILE_PATH, "[FileMissing] " + szFileName + Environment.NewLine );
			}
			RDLogHandler?.Invoke( errorMsg );
		}

		static void LogXmlLoadError( string szFilePath, Exception ex )
		{
			string errorMsg = "[LanguageManager] Failed to load xml file: " + szFilePath + ", " + ex.ToString();
			lock( m_DiagnosticLogLock ) {
				EnsureRunHeader();
				File.AppendAllText( DIAGNOSTIC_LOG_FILE_PATH, "[XmlLoadError] " + szFilePath + " - " + ex.ToString() + Environment.NewLine );
			}
			RDLogHandler?.Invoke( errorMsg );
		}

		static void LogMissingID( string szID, string szLanguage, List<string> szMapConfigFileNameList )
		{
			string szKey = szID + "@" + szLanguage;
			string errorMsg;
			lock( m_DiagnosticLogLock ) {
				if( m_LoggedMissingIDs.Add( szKey ) == false ) {
					return;
				}
				EnsureRunHeader();

				// build xml file name hint so reader knows which file to edit
				string szXmlFileHint = string.Empty;
				if( szMapConfigFileNameList != null && szMapConfigFileNameList.Count > 0 ) {
					string szFileNames = string.Join( ", ", szMapConfigFileNameList.Select( name => name + "_" + szLanguage + XML_EXTENSTION ) );
					szXmlFileHint = " in [" + szFileNames + "]";
				}

				File.AppendAllText( DIAGNOSTIC_LOG_FILE_PATH, "[IDMissing] " + szID + " (" + szLanguage + ")" + szXmlFileHint + Environment.NewLine );
				errorMsg = "[LanguageManager] Missing language ID: " + szID + " (" + szLanguage + ")" + szXmlFileHint;
			}
			RDLogHandler?.Invoke( errorMsg );
		}

		// caller must hold m_DiagnosticLogLock
		// writes a run-start separator once per process, always appends so previous runs are preserved
		static void EnsureRunHeader()
		{
			if( m_bRunHeaderWritten ) {
				return;
			}
			try {
				Directory.CreateDirectory( Path.GetDirectoryName( DIAGNOSTIC_LOG_FILE_PATH ) );
				File.AppendAllText(
					DIAGNOSTIC_LOG_FILE_PATH,
					Environment.NewLine + "=== Run: " + DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" ) + " ===" + Environment.NewLine );
				m_bRunHeaderWritten = true;
			}
			catch( Exception ) {

				// diagnostic log is best-effort, ignore IO errors
			}
		}
	}

}
