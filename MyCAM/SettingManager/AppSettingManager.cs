using System;
using System.IO;
using System.Xml.Linq;

namespace MyCAM.SettingManager
{
	// manage Setting.xml read/write, owned by MyApp with lifecycle
	internal class AppSettingManager
	{
		// load setting from file in constructor, so the instance is ready to use right after creation
		public AppSettingManager()
		{
			Load();
		}

		// UI language tag (e.g. "zh-TW", "zh-CN")
		public string UILanguage
		{
			get { return m_szUILanguage; }
			set { m_szUILanguage = value; }
		}

		// CNC IP address (IPv4 format, e.g. "192.168.1.200")
		public string CNCIP
		{
			get { return m_szCNCIP; }
			set { m_szCNCIP = value; }
		}

		// validate then persist UILanguage
		public void ApplyUILanguage( string value )
		{
			if( string.IsNullOrWhiteSpace( value ) ) {
				return;
			}
			if( m_szUILanguage == value ) {
				return;
			}
			UILanguage = value;
			Save();
		}

		// validate then persist CNCIP
		public void ApplyCNCIP( string value )
		{
			if( string.IsNullOrWhiteSpace( value ) ) {
				return;
			}
			if( m_szCNCIP == value ) {
				return;
			}
			CNCIP = value;
			Save();
		}

		// file path & xml format
		const string CUSTOM_Folder_Name = "Custom";
		const string SETTING_File_Name = "AppSetting.xml";
		const string XML_ROOT_NAME = "AppSetting";
		const string XML_UILanguage_NODE = "UILanguage";
		const string XML_CNCIP_NODE = "CNCIP";
		const string DEFAULT_UI_Language = "zh-TW";
		const string DEFAULT_CNC_IP = "192.168.1.200";
		static readonly string SETTING_File_Path = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory,
										  CUSTOM_Folder_Name,
										  SETTING_File_Name );

		// in-memory setting data
		string m_szUILanguage = DEFAULT_UI_Language;
		string m_szCNCIP = DEFAULT_CNC_IP;

		// load setting from file, if file not exist or any node is missing,
		// fill defaults and write a complete standard file
		void Load()
		{
			if( File.Exists( SETTING_File_Path ) == false ) {
				ResetToDefault();
				Save();
				return;
			}

			try {
				XDocument xmlDoc = XDocument.Load( SETTING_File_Path );
				XElement root = xmlDoc.Element( XML_ROOT_NAME );

				// root missing, regenerate a standard file
				if( root == null ) {
					ResetToDefault();
					Save();
					return;
				}

				bool bNeedSave = false;

				// UILanguage node
				XElement uiLanguageNode = root.Element( XML_UILanguage_NODE );
				if( uiLanguageNode == null || string.IsNullOrWhiteSpace( uiLanguageNode.Value ) ) {
					m_szUILanguage = DEFAULT_UI_Language;
					bNeedSave = true;
				}
				else {
					m_szUILanguage = uiLanguageNode.Value.Trim();
				}

				// CNCIP node
				XElement cncIPNode = root.Element( XML_CNCIP_NODE );
				if( cncIPNode == null || string.IsNullOrWhiteSpace( cncIPNode.Value ) ) {
					m_szCNCIP = DEFAULT_CNC_IP;
					bNeedSave = true;
				}
				else {
					m_szCNCIP = cncIPNode.Value.Trim();
				}

				// re-save to patch any missing nodes (e.g. upgrading from older Setting.xml)
				if( bNeedSave ) {
					Save();
				}
			}
			catch( Exception ) {

				// load failed, fall back to default in memory only, do not overwrite the file
				ResetToDefault();
			}
		}

		void ResetToDefault()
		{
			m_szUILanguage = DEFAULT_UI_Language;
			m_szCNCIP = DEFAULT_CNC_IP;
		}

		void Save()
		{
			try {

				// ensure directory exists before saving
				Directory.CreateDirectory( Path.GetDirectoryName( SETTING_File_Path ) );

				XDocument xmlDoc = new XDocument(
					new XDeclaration( "1.0", "utf-8", null ),
					new XElement( XML_ROOT_NAME,
						new XElement( XML_UILanguage_NODE, m_szUILanguage ?? DEFAULT_UI_Language ),
						new XElement( XML_CNCIP_NODE, m_szCNCIP ?? DEFAULT_CNC_IP ) ) );
				xmlDoc.Save( SETTING_File_Path );
			}
			catch( Exception ) {

				// ignore save error
			}
		}
	}
}
