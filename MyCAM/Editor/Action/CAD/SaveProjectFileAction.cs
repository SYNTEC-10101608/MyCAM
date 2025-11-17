using MyCAM.App;
using MyCAM.Data;
using MyCAM.FileManager;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace MyCAM.Editor
{
	internal class SaveProjectFileAction : EditActionBase
	{
		public SaveProjectFileAction( DataManager dataManager )
			: base( dataManager )
		{
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SaveProjectFile;
			}
		}

		public override void Start()
		{
			if( m_DataManager.PartIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "尚無工件能夠匯出專案檔", MyApp.NoticeType.Warning );
				End();
				return;
			}
			string projectFilePath = GetProjectFileInfo();
			if( projectFilePath != null ) {
				if( string.IsNullOrEmpty( projectFilePath ) ) {
					return;
				}

				
				// avoid writing error
				try {
					// turn data manager to dto
					DataManagerDTO dataManagerDTO = new DataManagerDTO( m_DataManager );

					// serialize to XML
					XmlSerializer serializer = new XmlSerializer( typeof( DataManagerDTO ) );

					// to remove unnecessary comments in XML (xmlns:xsd and xmlns:xsi)
					XmlSerializerNamespaces serializerNameSpace = new XmlSerializerNamespaces();
					serializerNameSpace.Add( "", "" );
					using( FileStream fileStream = new FileStream( projectFilePath, FileMode.Create ) ) {
						serializer.Serialize( fileStream, dataManagerDTO, serializerNameSpace );
					}
				}
				catch( Exception ex ) {
					MyApp.Logger.ShowOnLogPanel( $"儲存檔案時發生錯誤：\n{ex.Message}", MyApp.NoticeType.Error );
				}
				
			}
			End();
		}

		static string GetProjectFileInfo()
		{
			using( SaveFileDialog fileDialog = new SaveFileDialog() ) {
				fileDialog.Filter = "SFA Files (*.sfa)|*.sfa";
				fileDialog.FileName = "";
				fileDialog.Title = "儲存專案";
				fileDialog.OverwritePrompt = true;
				fileDialog.AddExtension = true;
				if( fileDialog.ShowDialog() == DialogResult.OK ) {
					return fileDialog.FileName;
				}

				// cancle 
				else {
					return string.Empty;
				}
			}
		}
	}
}
