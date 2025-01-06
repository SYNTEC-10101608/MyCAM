using DataStructure;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ProcessEdit
{
	public partial class OrderForm : Form
	{
		public Action OrderDone;
		public Action OrderCancel;
		public Action<IProcessData> TreeNodePick;

		public OrderForm()
		{
			InitializeComponent();

			// set tree view properties
			m_tvBrowser.Dock = DockStyle.Fill;
			m_tvBrowser.AfterSelect += ( sender, e ) =>
			{
				if( m_ProcessDataMap.ContainsKey( e.Node ) ) {
					TreeNodePick?.Invoke( m_ProcessDataMap[ e.Node ] );
				}
			};
		}

		public bool Init( ProcessEditModel model )
		{
			if( model == null ) {
				return false;
			}
			m_Model = model;
			ShowProcess();
			return true;
		}

		// model
		ProcessEditModel m_Model;

		// process data map
		Dictionary<TreeNode, IProcessData> m_ProcessDataMap = new Dictionary<TreeNode, IProcessData>();

		void ShowProcess()
		{
			// TEST
			int count = 0;

			// add process data
			foreach( IProcessData processData in m_Model.ProcessDataList ) {
				TreeNode node = null;
				if( processData.ProcessType == EProcessType.ProcessType_Cutting ) {
					count++;
					node = new TreeNode( "Cutting" + count.ToString() );
				}
				else if( processData.ProcessType == EProcessType.ProcessType_Traverse ) {
					node = new TreeNode( "Traverse" );
				}
				m_tvBrowser.Nodes.Add( node );
				m_ProcessDataMap.Add( node, processData );
			}
		}

		void m_tsmiSiftUp_Click( object sender, EventArgs e )
		{
			// get selected node
			TreeNode selectedNode = m_tvBrowser.SelectedNode;
			if( selectedNode == null ) {
				return;
			}

			// get selected process data
			if( m_ProcessDataMap.ContainsKey( selectedNode ) == false ) {
				return;
			}
			IProcessData processData = m_ProcessDataMap[ selectedNode ];

			// swap with previous element in the process data list
			int index = m_Model.ProcessDataList.IndexOf( processData );
			if( index == 0 || index == -1 ) {
				return;
			}
			m_Model.ProcessDataList.RemoveAt( index );
			m_Model.ProcessDataList.Insert( index - 1, processData );

			// swap with previous node in the tree view
			TreeNode prevNode = selectedNode.PrevNode;
			if( prevNode == null ) {
				return;
			}
			m_tvBrowser.Nodes.Remove( selectedNode );
			m_tvBrowser.Nodes.Insert( prevNode.Index, selectedNode );
			m_tvBrowser.SelectedNode = selectedNode;

			// invoke sift done event
			OrderDone?.Invoke();
		}

		void m_tsmiSiftDown_Click( object sender, EventArgs e )
		{
			// get selected node
			TreeNode selectedNode = m_tvBrowser.SelectedNode;
			if( selectedNode == null ) {
				return;
			}

			// get selected process data
			if( m_ProcessDataMap.ContainsKey( selectedNode ) == false ) {
				return;
			}
			IProcessData processData = m_ProcessDataMap[ selectedNode ];

			// swap with next element in the process data list
			int index = m_Model.ProcessDataList.IndexOf( processData );
			if( index == m_Model.ProcessDataList.Count - 1 || index == -1 ) {
				return;
			}
			m_Model.ProcessDataList.RemoveAt( index );
			m_Model.ProcessDataList.Insert( index + 1, processData );

			// swap with next node in the tree view
			TreeNode nextNode = selectedNode.NextNode;
			if( nextNode == null ) {
				return;
			}
			m_tvBrowser.Nodes.Remove( selectedNode );
			m_tvBrowser.Nodes.Insert( nextNode.Index + 1, selectedNode );
			m_tvBrowser.SelectedNode = selectedNode;

			// invoke sift done event
			OrderDone?.Invoke();
		}

		void OrderForm_FormClosing( object sender, FormClosingEventArgs e )
		{
			// just hide the form
			e.Cancel = true;
			Hide();
			OrderCancel?.Invoke();
		}
	}
}
