using DataStructure;
using OCC.gp;
using System;
using System.Windows.Forms;

namespace ProcessEdit
{
	public partial class ObjectForm : Form
	{
		public Action PropertyChanged;
		public Action<int> ItemPick;

		public ObjectForm()
		{
			InitializeComponent();

			// set tree view properties
			m_tvBrowser.Dock = DockStyle.Fill;
			m_tvBrowser.AfterSelect += ( sender, e ) =>
			{
				TreeNode node = e.Node;
				if( node == null ) {
					return;
				}
				ItemPick?.Invoke( node.Index );
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
			}
		}

		// order
		void m_tsmiSiftUp_Click( object sender, EventArgs e )
		{
			DoSift( -1 );
		}

		void m_tsmiSiftDown_Click( object sender, EventArgs e )
		{
			DoSift( 1 );
		}

		void DoSift( int nDirection )
		{
			// get selected index
			TreeNode selectedNode = m_tvBrowser.SelectedNode;
			if( selectedNode == null ) {
				return;
			}
			int nIndex = m_tvBrowser.SelectedNode.Index;

			// not found
			if( nIndex == -1

				// first element can't sift up
				|| nDirection == -1 && nIndex == 0

				// last element can't sift down
				|| nDirection == 1 && nIndex == m_Model.ProcessDataList.Count - 1 ) {
				return;
			}

			// swap element in the process data list
			IProcessData temp = m_Model.ProcessDataList[ nIndex ];
			m_Model.ProcessDataList.RemoveAt( nIndex );
			m_Model.ProcessDataList.Insert( nIndex + nDirection, temp );

			// invoke sift done event
			PropertyChanged?.Invoke();

			// swap node in the tree view
			m_tvBrowser.Nodes.RemoveAt( nIndex );
			m_tvBrowser.Nodes.Insert( nIndex + nDirection, selectedNode );
			m_tvBrowser.SelectedNode = selectedNode;
		}

		// add traverse
		void m_tsmiAddTraverseBefore_Click( object sender, EventArgs e )
		{
			DoAddTraverse( 0 );
		}

		void m_tsmiAddTraverseAfter_Click( object sender, EventArgs e )
		{
			DoAddTraverse( 1 );
		}

		void DoAddTraverse( int nDirection )
		{
			// get selected index
			TreeNode selectedNode = m_tvBrowser.SelectedNode;
			if( selectedNode == null ) {
				return;
			}
			int nIndex = m_tvBrowser.SelectedNode.Index;
			if( nIndex == -1 ) {
				return;
			}

			// add traverse process data
			TraverseForm form = new TraverseForm( new TraverseProcessData( new gp_Pnt() ) );
			if( form.ShowDialog() != DialogResult.OK ) {
				return;
			}
			TraverseProcessData traverseData = new TraverseProcessData( new gp_Pnt( form.X, form.Y, form.Z ) );
			m_Model.ProcessDataList.Insert( nIndex + nDirection, traverseData );

			// invoke add traverse done event
			PropertyChanged?.Invoke();

			// add node in the tree view
			TreeNode node = new TreeNode( "Traverse" );
			m_tvBrowser.Nodes.Insert( nIndex + nDirection, node );
			m_tvBrowser.SelectedNode = node;
		}

		// cancel
		void OrderForm_FormClosing( object sender, FormClosingEventArgs e )
		{
			// TODO: temp solution
			e.Cancel = true;
		}
	}
}
