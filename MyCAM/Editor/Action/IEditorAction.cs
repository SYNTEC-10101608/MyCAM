using MyCAM.Data;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal enum EActionStatus
	{
		Start,
		End,
	}

	internal enum EditActionType
	{
		// default
		Default,

		// CAD
		AddPoint,
		AddLine,
		ThreePtTransform,
		ManualTransform,
		AxisTransform,

		// CAM
		SelectFace,
		SelectPath,
		StartPoint,
		ToolVec,
		SetLead,
		OverCut,
		SetTraverseParam,

		// File
		ImportProjectFile,
		SaveProjectFile,
	}

	internal interface IEditorAction
	{
		EditActionType ActionType
		{
			get;
		}

		void Start();

		void End();

		Action<IEditorAction> EndAction
		{
			get; set;
		}

		Action<IEditorAction> StartAction
		{
			get; set;
		}
	}

	internal abstract class KeyMouseActionBase : IEditorAction
	{
		protected KeyMouseActionBase( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
		{
			if( viewer == null || treeView == null || cadManager == null || viewManager == null ) {
				throw new ArgumentNullException( "EditActionBase constructing argument null" );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;
			m_CADManager = cadManager;
		}

		public abstract EditActionType ActionType
		{
			get;
		}

		public virtual void Start()
		{
			// Register events
			m_Viewer.MouseDown += ViewerMouseDown;
			m_Viewer.KeyDown += ViewerKeyDown;
			m_TreeView.AfterSelect += TreeViewAfterSelect;
			m_TreeView.KeyDown += TreeViewKeyDown;

			// Invoke start action event
			StartAction?.Invoke( this );
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

		public Action<IEditorAction> EndAction
		{
			get; set;
		}

		public Action<IEditorAction> StartAction
		{
			get; set;
		}

		protected abstract void ViewerMouseDown( MouseEventArgs e );

		protected abstract void ViewerKeyDown( KeyEventArgs e );

		protected abstract void TreeViewAfterSelect( object sender, TreeViewEventArgs e );

		protected abstract void TreeViewKeyDown( object sender, KeyEventArgs e );

		protected Viewer m_Viewer;
		protected TreeView m_TreeView;
		protected ViewManager m_ViewManager;
		protected DataManager m_CADManager;
	}

	internal abstract class DialogActionBase : IEditorAction
	{
		public EditActionType ActionType => throw new NotImplementedException();

		public Action<IEditorAction> EndAction
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public Action<IEditorAction> StartAction
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public void End()
		{
			throw new NotImplementedException();
		}

		public void Start()
		{
			throw new NotImplementedException();
		}
	}
}
