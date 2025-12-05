using MyCAM.Data;
using OCCViewer;
using System;
using System.Collections.Generic;
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
		// select object
		SelectObject,

		// CAD
		AddPoint_CircArcCenter,
		AddPoint_EdgeMidPoint,
		AddPoint_TwoVertexMidPoint,
		AddLine,
		ThreePtTransform,
		ManualTransform,
		AxisTransform,
		SewPart,

		// CAM
		SelectFace,
		SelectPath,
		StartPoint,
		ToolVec,
		SetLead,
		OverCut,
		SetTraverseParam,
		SetEntryAndExitParam,
		SetPattern,

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

	internal abstract class EditCAMActionBase : EditActionBase
	{
		protected EditCAMActionBase( DataManager dataManager, List<string> pathIDList ) : base( dataManager )
		{
			if( pathIDList == null || pathIDList.Count == 0 ) {
				throw new ArgumentNullException( "LeadAction constructing argument pathIDList null or empty" );
			}
			m_PathIDList = pathIDList;

			foreach( string szID in m_PathIDList ) {
				if( !m_DataManager.ObjectMap.ContainsKey( szID ) ) {
					throw new ArgumentException( "LeadAction constructing argument pathIDList contains invalid path ID" );
				}
				m_CraftDataList.Add( ( dataManager.ObjectMap[ szID ] as PathObject ).CraftData );
			}
		}

		public Action<List<string>> PropertyChanged;
		protected List<CraftData> m_CraftDataList = new List<CraftData>();
		protected List<string> m_PathIDList;
	}

	internal abstract class EditActionBase : IEditorAction
	{
		protected EditActionBase( DataManager dataManager )
		{
			if( dataManager == null ) {
				throw new ArgumentNullException( "EditActionBase constructing argument null" );
			}
			m_DataManager = dataManager;
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

		protected DataManager m_DataManager;
	}

	internal abstract class KeyMouseActionBase : EditActionBase
	{
		protected KeyMouseActionBase( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager )
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
			m_Viewer.MouseMove += ViewerMouseMove;
			m_Viewer.MouseUp += ViewerMouseUp;
			m_Viewer.MouseClick += ViewerMouseClick;
			m_Viewer.MouseDoubleClick += ViewerMouseDoubleClick;
			m_Viewer.KeyDown += ViewerKeyDown;
			m_TreeView.AfterSelect += TreeViewAfterSelect;
			m_TreeView.KeyDown += TreeViewKeyDown;
		}

		public override void End()
		{
			// Unregister events
			m_Viewer.MouseDown -= ViewerMouseDown;
			m_Viewer.MouseMove -= ViewerMouseMove;
			m_Viewer.MouseUp -= ViewerMouseUp;
			m_Viewer.MouseClick -= ViewerMouseClick;
			m_Viewer.MouseDoubleClick -= ViewerMouseDoubleClick;
			m_Viewer.KeyDown -= ViewerKeyDown;
			m_TreeView.AfterSelect -= TreeViewAfterSelect;
			m_TreeView.KeyDown -= TreeViewKeyDown;
			base.End();
		}

		protected virtual void ViewerMouseDown( MouseEventArgs e )
		{
			// do nothing and wait for hook
		}

		protected virtual void ViewerMouseMove( MouseEventArgs e )
		{
			// do nothing and wait for hook
		}

		protected virtual void ViewerMouseUp( MouseEventArgs e )
		{
			// do nothing and wait for hook
		}

		protected virtual void ViewerMouseClick( MouseEventArgs e )
		{
			// do nothing and wait for hook
		}

		protected virtual void ViewerMouseDoubleClick( MouseEventArgs e )
		{
			// do nothing and wait for hook
		}

		protected virtual void ViewerKeyDown( KeyEventArgs e )
		{
			// do nothing and wait for hook
		}

		protected virtual void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			// do nothing and wait for hook
		}

		protected virtual void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			// do nothing and wait for hook
		}

		protected Viewer m_Viewer;
		protected TreeView m_TreeView;
		protected ViewManager m_ViewManager;
	}
}
