using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	public enum CADActionType
	{
		None = 0,
		Default = 1,
		AddPoint = 2,
		Create3PCS = 3,
	}

	internal interface ICADAction
	{
		CADActionType ActionType
		{
			get;
		}

		void Start();

		void End();

		Action<ICADAction> EndAction
		{
			get; set;
		}
	}

	internal class CADACtionBase : ICADAction
	{
		public CADACtionBase( Viewer viewer, TreeView treeView,
			CADManager cadManager, Dictionary<string, ViewObject> viewObjectMap, Dictionary<string, TreeNode> treeNodeMap )
		{
			if( viewer == null || treeView == null
				|| viewObjectMap == null || treeNodeMap == null
				|| cadManager == null ) {
				throw new ArgumentNullException( "Action constructing argument null - Base" );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewObjectMap = viewObjectMap;
			m_TreeNodeMap = treeNodeMap;
			m_CADManager = cadManager;
		}

		public virtual CADActionType ActionType
		{
			get
			{
				return CADActionType.None;
			}
		}

		public virtual void Start()
		{
			// Register events
			m_Viewer.MouseDown += ViewerMouseDown;
			m_Viewer.KeyDown += ViewerKeyDown;
			m_TreeView.AfterSelect += TreeViewAfterSelect;
			m_TreeView.KeyDown += TreeViewKeyDown;
		}

		public virtual void End()
		{
			// Unregister events
			m_Viewer.MouseDown -= ViewerMouseDown;
			m_Viewer.KeyDown -= ViewerKeyDown;
			m_TreeView.AfterSelect -= TreeViewAfterSelect;
			m_TreeView.KeyDown -= TreeViewKeyDown;

			// Invoke end action event
			EndAction?.Invoke( this );
		}

		public Action<ICADAction> EndAction
		{
			get; set;
		}

		protected virtual void ViewerMouseDown( MouseEventArgs e )
		{
			// Default mouse down action
		}

		protected virtual void ViewerKeyDown( KeyEventArgs e )
		{
			// Default key down action
		}

		protected virtual void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			// Default tree view after select action
		}

		protected virtual void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			// Default tree view key down action
		}

		protected Viewer m_Viewer;
		protected TreeView m_TreeView;
		protected Dictionary<string, ViewObject> m_ViewObjectMap;
		protected Dictionary<string, TreeNode> m_TreeNodeMap;
		protected CADManager m_CADManager;
	}
}
