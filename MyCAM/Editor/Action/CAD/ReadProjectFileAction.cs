using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using MyCAM.Data;
using MyCAM.FileManager;
using OCC.AIS;
using OCCViewer;
using static MyCAM.Data.DataManager;

namespace MyCAM.Editor
{
	internal class ReadProjectFileAction : EditActionBase
	{
		public ReadProjectFileAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
			: base( viewer, treeView, cadManager, viewManager )
		{
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
					dataManagerDTO.DataMgrDTO2Data( out Dictionary<string, ShapeData> shapeDataMap, out List<string> partIDList, out List<string> pathIDList, out ShapeIDsStruct shapeIDs, out TraverseData traverseData );

					// set back to data manager
					m_CADManager.ResetDataManger( shapeDataMap, partIDList, pathIDList, shapeIDs, traverseData );
					ChangeViewerManager();
				}
				catch( Exception ex ) {
					MessageBox.Show(
						$"讀取專案檔案失敗：\n{ex.Message}",
						"錯誤",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
					);
				}
			}
			End();
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

		void ChangeViewerManager()
		{
			// clear the tree view and viewer
			m_ViewManager.PartNode.Nodes.Clear();
			m_ViewManager.PathNode.Nodes.Clear();
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			}

			// update view manager data
			m_ViewManager.ViewObjectMap.Clear();
			m_ViewManager.TreeNodeMap.Clear();

			// buill part tree
			foreach( var szNewDataID in m_CADManager.PartIDList ) {
				ShapeData data = m_CADManager.ShapeDataMap[ szNewDataID ];

				// add node to the tree view
				TreeNode node = new TreeNode( data.UID );
				m_ViewManager.PartNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( data.UID, node );

				// add shape to the viewer
				AIS_Shape aisShape = ViewHelper.CreatePartAIS( data.Shape );
				m_ViewManager.ViewObjectMap.Add( data.UID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			//build path tree
			m_ViewManager.PathNode.Nodes.Clear();
			foreach( var szNewPathDataID in m_CADManager.PathIDList ) {
				ShapeData data = m_CADManager.ShapeDataMap[ szNewPathDataID ];

				// add a new node to the tree view
				TreeNode node = new TreeNode( szNewPathDataID );
				m_ViewManager.PathNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( szNewPathDataID, node );

				// add a new shape to the viewer
				AIS_Shape aisShape = ViewHelper.CreatePathAIS( m_CADManager.ShapeDataMap[ szNewPathDataID ].Shape );
				m_ViewManager.ViewObjectMap.Add( szNewPathDataID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			// update tree view and viewer (current is in cad mode so open part tree)
			m_ViewManager.PartNode.ExpandAll();
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
	}
}
