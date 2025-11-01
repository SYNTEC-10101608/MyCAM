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
		Simulation,
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
		protected EditorBase( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
		{
			if( viewer == null || treeView == null || dataManager == null || viewManager == null ) {
				throw new ArgumentNullException( "EditorBase consturcting argument null." );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_DataManager = dataManager;
			m_ViewManager = viewManager;
		}

		public abstract EEditorType Type
		{
			get;
		}

		public virtual void EditStart()
		{
			// start default action
			m_CurrentAction = m_DefaultAction;
			m_CurrentAction.Start();
		}

		public virtual void EditEnd()
		{
			// end all action
			if( m_CurrentAction.ActionType == EditActionType.SelectObject ) {
				m_CurrentAction.End();
			}
			else {
				m_CurrentAction.End();
				m_DefaultAction.End();
			}
		}

		protected virtual void StartEditAction( IEditorAction action )
		{
			// to prevent from non-necessary default action start
			m_IsNextAction = true;

			// end the current action
			m_CurrentAction.End();
			m_IsNextAction = false;

			// start the action
			m_CurrentAction = action;
			m_CurrentAction.StartAction += OnEditActionStart;
			m_CurrentAction.EndAction += OnEditActionEnd;
			m_CurrentAction.Start();
		}

		protected virtual void EndActionIfNotDefault()
		{
			if( m_CurrentAction.ActionType != EditActionType.SelectObject ) {
				m_CurrentAction?.End();
			}
		}

		protected virtual void OnEditActionStart( IEditorAction action )
		{
			// currently for notifying main form
		}

		protected virtual void OnEditActionEnd( IEditorAction action )
		{
			// start default action if all edit actions are done
			if( !m_IsNextAction ) {
				m_CurrentAction = m_DefaultAction;
				m_CurrentAction.Start();
			}
		}

		// editor properties
		protected readonly Viewer m_Viewer;
		protected readonly TreeView m_TreeView;
		protected readonly DataManager m_DataManager;
		protected readonly ViewManager m_ViewManager;

		// action properties
		protected SelectObjectAction m_DefaultAction;
		protected IEditorAction m_CurrentAction;
		protected bool m_IsNextAction = false;
	}
}
