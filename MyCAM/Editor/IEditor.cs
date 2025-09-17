using MyCAM.Data;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	enum EEditorType
	{
		CAD,
		CAM,
		Post,
	}

	internal interface IEditor
	{
		EEditorType Type
		{
			get;
		}

		void EditStart();

		void EditEnd();
	}

	internal abstract class EditorBase : IEditor
	{
		protected EditorBase( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
		{
			if( viewer == null || treeView == null || cadManager == null || viewManager == null ) {
				throw new ArgumentNullException( "EditorBase consturcting argument null." );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_CADManager = cadManager;
			m_ViewManager = viewManager;
		}

		public abstract EEditorType Type
		{
			get;
		}

		public virtual void EditStart()
		{
		}

		public virtual void EditEnd()
		{
		}

		// editor properties
		protected readonly Viewer m_Viewer;
		protected readonly TreeView m_TreeView;
		protected readonly DataManager m_CADManager;
		protected readonly ViewManager m_ViewManager;

		// action properties
		protected IEditorAction m_DefaultAction;
		protected IEditorAction m_CurrentAction;
	}
}
