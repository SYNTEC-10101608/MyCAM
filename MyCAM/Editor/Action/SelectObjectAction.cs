using MyCAM.Data;
using OCC.AIS;
using OCC.Aspect;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal enum ESelectObjectType
	{
		Part,
		Path,
	}

	internal class SelectObjectAction : KeyMouseActionBase
	{
		public Action TreeSelectionChange;
		public Action RemovePath;
		public Action<bool> PathOrderMove;

		public SelectObjectAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager,
			ESelectObjectType type )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_SelectType = type;

			// viewer
			m_RubberBand = new AIS_RubberBand( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ), Aspect_TypeOfLine.Aspect_TOL_SOLID, 1 );
			m_RubberBand.SetRectangle( 0, 0, 0, 0 ); // need to set initial rectangle

			// sync selection
			m_SelectedIDSet = new HashSet<string>();
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SelectObject;
			}
		}

		public override void Start()
		{
			base.Start();

			// reset activation mode
			if( m_SelectType == ESelectObjectType.Part ) {
				foreach( var partID in m_DataManager.PartIDList ) {
					if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
						continue;
					}
					m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle );
				}
			}
			else if( m_SelectType == ESelectObjectType.Path ) {
				foreach( var partID in m_DataManager.PathIDList ) {
					if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
						continue;
					}
					m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle );
				}
			}
			m_bSuppressTreeViewSync = false;
			//SyncSelectionFromTreeToView();

			// display rubber band
			m_Viewer.GetAISContext().Display( m_RubberBand, true );
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// reset activation mode
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Deactivate( viewObject.AISHandle );
			}
			m_bSuppressTreeViewSync = false;

			// remove rubber band
			m_Viewer.GetAISContext().Remove( m_RubberBand, true );
			base.End();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			// single select
			if( !m_IsDragging && e.Button == MouseButtons.Left ) {
				if( ( Control.ModifierKeys & Keys.Control ) == Keys.Control ) {
					m_Viewer.Select( AIS_SelectionScheme.AIS_SelectionScheme_XOR );
				}
				else {
					m_Viewer.Select();
				}
				SyncSelectionFromViewToTree();
			}
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				int modifiedY = m_Viewer.Height - e.Y;
				m_RubberBandStartX = e.X;
				m_RubberBandStartY = modifiedY;
				m_RubberBand.SetRectangle( m_RubberBandStartX, m_RubberBandStartY, m_RubberBandStartX, m_RubberBandStartY );
				m_Viewer.GetAISContext().Redisplay( m_RubberBand, true ); // UpdateView does not work here
			}
		}

		protected override void ViewerMouseMove( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				int modifiedY = m_Viewer.Height - e.Y;
				int minX = Math.Min( m_RubberBandStartX, e.X );
				int minY = Math.Min( m_RubberBandStartY, modifiedY );
				int maxX = Math.Max( m_RubberBandStartX, e.X );
				int maxY = Math.Max( m_RubberBandStartY, modifiedY );
				m_RubberBand.SetRectangle( minX, minY, maxX, maxY );
				m_Viewer.GetAISContext().Redisplay( m_RubberBand, true ); // UpdateView does not work here

				// update dragging flag
				m_IsDragging = true;
			}
		}

		protected override void ViewerMouseUp( MouseEventArgs e )
		{
			if( m_IsDragging && e.Button == MouseButtons.Left ) {
				int modifiedY = m_Viewer.Height - e.Y;
				m_RubberBand.SetRectangle( e.X, modifiedY, e.X, modifiedY );
				m_Viewer.GetAISContext().Redisplay( m_RubberBand, true ); // UpdateView does not work here

				// select shapes within rubber band
				int minX = Math.Min( m_RubberBandStartX, e.X );
				int minY = Math.Min( m_RubberBandStartY, modifiedY );
				int maxX = Math.Max( m_RubberBandStartX, e.X );
				int maxY = Math.Max( m_RubberBandStartY, modifiedY );
				if( ( Control.ModifierKeys & Keys.Control ) == Keys.Control ) {
					m_Viewer.SelectRectangle( minX, minY, maxX, maxY, AIS_SelectionScheme.AIS_SelectionScheme_XOR );
				}
				else {
					m_Viewer.SelectRectangle( minX, minY, maxX, maxY );
				}
				SyncSelectionFromViewToTree();

				// update dragging flag
				m_IsDragging = false;
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			OnKeyDown( e );
		}

		protected override void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			//if( e.Node == null ) {
			//	return;
			//}

			//// tell CAMEditor tree select is change
			//TreeSelectionChange?.Invoke();
			//SyncSelectionFromTreeToView();
		}

		protected override void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			OnKeyDown( e );
		}

		public void ChangeObjectVisibility()
		{
			TreeNode selectedNode = m_TreeView.SelectedNode;
			if( selectedNode == null || string.IsNullOrEmpty( selectedNode.Text ) ) {
				return;
			}
			string szUID = selectedNode.Text;
			ChangeObjectVisibility( szUID );
		}

		void TreeViewSelectionChanged()
		{
			SyncSelectionFromTreeToView();
		}

		void OnKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Space ) {
				ChangeObjectVisibility();
			}
			if( e.KeyCode == Keys.Delete ) {
				RemovePath?.Invoke();
			}
			if( e.Shift ) {
				if( e.KeyCode == Keys.Up ) {
					PathOrderMove?.Invoke( true );
				}

				if( e.KeyCode == Keys.Down ) {
					PathOrderMove?.Invoke( false );
				}
			}
		}

		void ChangeObjectVisibility( string szUID )
		{
			// toggle the visibility of the selected object
			if( !m_ViewManager.ViewObjectMap.ContainsKey( szUID ) ) {
				return;
			}
			ViewObject viewObject = m_ViewManager.ViewObjectMap[ szUID ];
			if( viewObject == null || viewObject.AISHandle == null ) {
				return;
			}
			if( !m_ViewManager.TreeNodeMap.ContainsKey( szUID ) ) {
				return;
			}
			TreeNode node = m_ViewManager.TreeNodeMap[ szUID ];
			if( node == null ) {
				return;
			}

			// toggle visibility
			viewObject.Visible = !viewObject.Visible;
			if( viewObject.Visible ) {
				node.ForeColor = System.Drawing.Color.Black;
				m_Viewer.GetAISContext().Display( viewObject.AISHandle, true );
			}
			else {
				node.ForeColor = System.Drawing.Color.LightGray;
				m_Viewer.GetAISContext().Erase( viewObject.AISHandle, true );
			}
		}

		void SyncSelectionFromViewToTree()
		{
			m_bSuppressTreeViewSync = true;

			// clear old selection
			(m_TreeView as MultiSelectTreeView).ClearSelection();
			m_SelectedIDSet.Clear();

			// get the selected ID
			m_Viewer.GetAISContext().InitSelected();
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				TopoDS_Shape selectedShape = m_Viewer.GetAISContext().SelectedShape();
				if( selectedShape == null || selectedShape.IsNull() ) {
					continue;
				}

				// find the corresponding UID
				string szUID = m_DataManager.GetUIDByShape( selectedShape );
				if( string.IsNullOrEmpty( szUID ) ) {
					continue;
				}
				m_SelectedIDSet.Add( szUID );
				m_Viewer.GetAISContext().NextSelected();
			}


			// find the node in the tree view
			List<TreeNode> selectedNodes = new List<TreeNode>();
			foreach( string szUID in m_SelectedIDSet ) {
				if( !m_ViewManager.TreeNodeMap.ContainsKey( szUID ) ) {
					continue;
				}
				selectedNodes.Add( m_ViewManager.TreeNodeMap[ szUID ] );
			}
			( m_TreeView as MultiSelectTreeView ).SelectNodes( selectedNodes );
			m_bSuppressTreeViewSync = false;
		}

		void SyncSelectionFromTreeToView()
		{
			if( m_bSuppressTreeViewSync ) {
				return;
			}

			// clear old slection
			m_Viewer.GetAISContext().ClearSelected( false );
			m_SelectedIDSet.Clear();

			// get the selected ID
			foreach( TreeNode node in ( m_TreeView as MultiSelectTreeView ).SelectedNodes ) {
				if( node == null || string.IsNullOrEmpty( node.Text ) ) {
					continue;
				}
				m_SelectedIDSet.Add( node.Text );
			}

			// find the corresponding view object
			foreach( string szUID in m_SelectedIDSet ) {
				if( !m_ViewManager.ViewObjectMap.ContainsKey( szUID ) ) {
					continue;
				}
				ViewObject viewObject = m_ViewManager.ViewObjectMap[ szUID ];
				if( viewObject == null || viewObject.AISHandle == null ) {
					continue;
				}
				m_Viewer.GetAISContext().AddOrRemoveSelected( viewObject.AISHandle, false );
			}
			m_Viewer.UpdateView();
		}

		bool m_bSuppressTreeViewSync = false;
		ESelectObjectType m_SelectType;

		// viewer mouse action
		AIS_RubberBand m_RubberBand;
		int m_RubberBandStartX = 0;
		int m_RubberBandStartY = 0;
		bool m_IsDragging = false;

		// selection sync
		HashSet<string> m_SelectedIDSet;
	}
}
