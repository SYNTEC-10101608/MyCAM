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

	internal abstract class EditActionBase : IEditorAction
	{
		protected EditActionBase( DataManager cadManager )
		{
			if( cadManager == null ) {
				throw new ArgumentNullException( "EditActionBase constructing argument null" );
			}
			m_CADManager = cadManager;
		}

		public abstract EditActionType ActionType
		{
			get;
		}

		public Action<IEditorAction> EndAction
		{
			get; set;
		}

		public Action<IEditorAction> StartAction
		{
			get; set;
		}

		public virtual void Start()
		{
			StartAction?.Invoke( this );
		}

		public virtual void End()
		{
			EndAction?.Invoke( this );
		}

		protected DataManager m_CADManager;
	}

	internal abstract class KeyMouseActionBase : EditActionBase
	{
		protected KeyMouseActionBase( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
			: base( cadManager )
		{
			if( viewer == null || treeView == null || viewManager == null ) {
				throw new ArgumentNullException( "KeyMouseActionBase constructing argument null" );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;
		}

		public override abstract EditActionType ActionType
		{
			get;
		}

		public override void Start()
		{
			base.Start();

			// Register events
			m_Viewer.MouseDown += ViewerMouseDown;
			m_Viewer.KeyDown += ViewerKeyDown;
			m_TreeView.AfterSelect += TreeViewAfterSelect;
			m_TreeView.KeyDown += TreeViewKeyDown;
		}

		public override void End()
		{
			// Unregister events
			m_Viewer.MouseDown -= ViewerMouseDown;
			m_Viewer.KeyDown -= ViewerKeyDown;
			m_TreeView.AfterSelect -= TreeViewAfterSelect;
			m_TreeView.KeyDown -= TreeViewKeyDown;
			base.End();
		}

		protected abstract void ViewerMouseDown( MouseEventArgs e );

		protected abstract void ViewerKeyDown( KeyEventArgs e );

		protected abstract void TreeViewAfterSelect( object sender, TreeViewEventArgs e );

		protected abstract void TreeViewKeyDown( object sender, KeyEventArgs e );

		protected Viewer m_Viewer;
		protected TreeView m_TreeView;
		protected ViewManager m_ViewManager;
	}
}
