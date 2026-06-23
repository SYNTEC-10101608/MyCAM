using MyCAM.App;
using MyCAM.Data;
using MyCAM.FileManager;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace MyCAM.Editor
{
	internal class ReadProjectFileAction : EditActionBase
	{
		public event Action ActionCompleted;

		public bool IsImportSuccess
		{
			get; private set;
		}

		public string ImportedFileName
		{
			get; private set;
		}

		public ReadProjectFileAction( DataManager dataManager, Viewer viewer, ViewManager viewManager )
			: base( dataManager )
		{
			if( viewer == null || viewManager == null ) {
				throw new ArgumentNullException( "ReadProjectFileAction constructing argument null" );
			}
			m_Viewer = viewer;
			m_ViewManager = viewManager;
			IsImportSuccess = false;
			ImportedFileName = string.Empty;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ImportProjectFile;
			}
		}

		public override void Start()
		{
			// get file path
			string filePath = OpenProjectFile();
			if( filePath != null ) {
				if( string.IsNullOrEmpty( filePath ) ) {
					End();
					return;
				}

				// IO protection
				try {
					XmlSerializer serializer = new XmlSerializer( typeof( DataManagerDTO ) );
					DataManagerDTO dataManagerDTO;

					// get DTO from xml file
					using( FileStream fileStream = new FileStream( filePath, FileMode.Open ) ) {
						dataManagerDTO = (DataManagerDTO)serializer.Deserialize( fileStream );
					}

					// turn DTO to data
					dataManagerDTO.DataMgrDTO2Data( out Dictionary<string, IObject> ObjectMap, out List<string> partIDList, out List<string> pathIDList, out ShapeIDsStruct shapeIDs, out EntryAndExitData entryAndExitData, out CalibrationData calibrationData );

					// set back to data manager
					m_DataManager.ResetDataManger( ObjectMap, partIDList, pathIDList, shapeIDs, entryAndExitData, calibrationData );
					UpdateAllViewData();

					// set import success and file name
					IsImportSuccess = true;
					ImportedFileName = Path.GetFileName( filePath );
				}
				catch( Exception ex ) {
					MyApp.Logger.ShowOnLogPanel( $"讀取專案檔案失敗：\n{ex.Message}", MyApp.NoticeType.Error );
					IsImportSuccess = false;
					ImportedFileName = string.Empty;
				}
			}
			End();
		}

		public override void End()
		{
			ActionCompleted?.Invoke();
			base.End();
		}

		void UpdateAllViewData()
		{
			m_ViewManager.RebuildAllViews( m_DataManager, true );
			m_Viewer.UpdateView();
		}

		string OpenProjectFile()
		{
			using( OpenFileDialog fileDialog = new OpenFileDialog() ) {

				// nly allow .saf files
				fileDialog.Filter = "SFA Files (*.sfa)|*.sfa";
				fileDialog.Title = "開啟專案";

				// only can choose one file
				fileDialog.Multiselect = false;

				// double click file or click ok
				if( fileDialog.ShowDialog() == DialogResult.OK ) {
					return fileDialog.FileName;
				}

				// cancle
				else {
					return string.Empty;
				}
			}
		}

		Viewer m_Viewer;
		ViewManager m_ViewManager;
	}
}
