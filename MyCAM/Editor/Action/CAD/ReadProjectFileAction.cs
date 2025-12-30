using MyCAM.App;
using MyCAM.Data;
using MyCAM.FileManager;
using OCC.AIS;
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
		public ReadProjectFileAction( DataManager dataManager, Viewer viewer, ViewManager viewManager )
			: base( dataManager )
		{
			if( viewer == null || viewManager == null ) {
				throw new ArgumentNullException( "ReadProjectFileAction constructing argument null" );
			}
			m_Viewer = viewer;
			m_ViewManager = viewManager;
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
					dataManagerDTO.DataMgrDTO2Data( out Dictionary<string, IObject> ObjectMap, out List<string> partIDList, out List<string> pathIDList, out ShapeIDsStruct shapeIDs, out EntryAndExitData entryAndExitData );

					// set back to data manager
					m_DataManager.ResetDataManger( ObjectMap, partIDList, pathIDList, shapeIDs, entryAndExitData );
					UpdateAllViewData();
				}
				catch( Exception ex ) {
					MyApp.Logger.ShowOnLogPanel( $"讀取專案檔案失敗：\n{ex.Message}", MyApp.NoticeType.Error );
				}
			}
			End();
		}

		void UpdateAllViewData()
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
			foreach( var szNewDataID in m_DataManager.PartIDList ) {
				PartObject data = (PartObject)m_DataManager.ObjectMap[ szNewDataID ];

				// add node to the tree view
				TreeNode node = new TreeNode( data.UID );
				m_ViewManager.PartNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( data.UID, node );

				// add shape to the viewer
				if( !DataGettingHelper.GetShapeObject( szNewDataID, out IShapeObject shapeObject ) ) {
					continue;
				}
				AIS_Shape aisShape = ViewHelper.CreatePartAIS( shapeObject.Shape );
				m_ViewManager.ViewObjectMap.Add( data.UID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			// build path tree
			m_ViewManager.PathNode.Nodes.Clear();
			int nodeStartIndex = 1;
			for( int i = 0; i < m_DataManager.PathIDList.Count; i++ ) {
				string pathID = m_DataManager.PathIDList[ i ];

				// get tree node text
				string szNodeText = PATH_NODE_PREFIX + nodeStartIndex.ToString();
				TreeNode node = new TreeNode( szNodeText );
				m_ViewManager.PathNode.Nodes.Add( node );

				// key is "Path_xxx" wich show on tree text, value is the node
				m_ViewManager.TreeNodeMap.Add( szNodeText, node );

				// add a new shape to the viewer
				if( !DataGettingHelper.GetShapeObject( pathID, out IShapeObject shapeObject ) ) {
					continue;
				}
				AIS_Shape aisShape = ViewHelper.CreatePathAIS( shapeObject.Shape );
				m_ViewManager.ViewObjectMap.Add( pathID, new ViewObject( aisShape ) );
				nodeStartIndex++;
			}

			// update tree view and viewer
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

		Viewer m_Viewer;
		ViewManager m_ViewManager;
		const string PATH_NODE_PREFIX = "Path_";
	}
}
