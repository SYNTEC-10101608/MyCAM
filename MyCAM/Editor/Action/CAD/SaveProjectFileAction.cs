using MyCAM.Data;
using MyCAM.FileManager;
using OCCViewer;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace MyCAM.Editor
{
	internal class SaveProjectFileAction : EditActionBase
	{
		public SaveProjectFileAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
			: base( viewer, treeView, cadManager, viewManager )
		{
		}

		public override void Start()
		{
			if( m_CADManager.PartIDList.Count == 0 ) {
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
					DataManagerDTO dataManagerDTO = new DataManagerDTO( m_CADManager );

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
					MessageBox.Show( "儲存檔案時發生錯誤：\n" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error );
				}
			}
			End();
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SaveProjectFile;
			}
		}

		protected override void ViewerMouseMove( MouseEventArgs e )
		{
			// do nothing
		}

		protected override void ViewerMouseUp( MouseEventArgs e )
		{
			// do nothing
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			// do nothing
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			// do nothing
		}

		protected override void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			// do nothing
		}

		protected override void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			// do nothing
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
