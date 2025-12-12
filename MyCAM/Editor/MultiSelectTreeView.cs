using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	public class MultiSelectTreeView : TreeView
	{
		// public read-only access to selected nodes
		[Browsable( false )]
		public IReadOnlyList<TreeNode> SelectedNodes => m_SelectedNodes.AsReadOnly();

		// event for selection changes
		public event EventHandler SelectionChanged;

		public MultiSelectTreeView()
		{
			// enable keyboard support
			this.HideSelection = false;

			// ensure no built-in selected node (we manage highlighting ourselves)
			base.SelectedNode = null;

			// subscribe to key down
			this.KeyDown += MultiSelectTreeView_KeyDown;

			// avoid collapse
			this.BeforeCollapse += MultiSelectTreeView_BeforeCollapse;
		}

		public void ReverseSelection()
		{
			foreach( TreeNode node in GetAllNodes( this.Nodes ) ) {
				if( m_SelectedNodes.Contains( node ) ) {
					m_SelectedNodes.Remove( node );
					HighlightNode( node, false );
				}
				else {
					m_SelectedNodes.Add( node );
					HighlightNode( node, true );
				}
			}
			OnSelectionChanged();
		}

		public void SelectNode( TreeNode node )
		{
			if( node == null ) {
				return;
			}
			if( m_SelectedNodes.Contains( node ) ) {
				return;
			}
			m_SelectedNodes.Add( node );
			HighlightNode( node, true );

			// only set anchor when shift is not pressed
			if( ( ModifierKeys & Keys.Shift ) != Keys.Shift ) {
				m_AnchorNode = node;
			}
			OnSelectionChanged();
		}

		public void UnselectNode( TreeNode node )
		{
			if( node == null ) {
				return;
			}
			if( m_SelectedNodes.Remove( node ) ) {
				HighlightNode( node, false );
				OnSelectionChanged();
			}
		}

		public void ClearSelection()
		{
			if( m_SelectedNodes.Count == 0 ) {
				return;
			}
			foreach( var n in m_SelectedNodes.ToList() ) {
				HighlightNode( n, false );
			}
			m_SelectedNodes.Clear();
			OnSelectionChanged();
		}

		public void SelectAll()
		{
			ClearSelection();
			foreach( TreeNode n in GetAllNodes( this.Nodes ) ) {
				m_SelectedNodes.Add( n );
				HighlightNode( n, true );
			}
			OnSelectionChanged();
		}

		protected override void OnBeforeSelect( TreeViewCancelEventArgs e )
		{
			// cancel built-in selection – we manage selection manually
			e.Cancel = true;
			base.OnBeforeSelect( e );
		}

		protected override void OnAfterSelect( TreeViewEventArgs e )
		{
			// prevent built-in selection highlight
			base.OnAfterSelect( e );
			if( base.SelectedNode != null )
				base.SelectedNode = null;
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			TreeNode node = this.GetNodeAt( e.Location );
			if( node == null ) {
				base.OnMouseDown( e );
				return;
			}

			bool ctrl = ( ModifierKeys & Keys.Control ) == Keys.Control;
			bool shift = ( ModifierKeys & Keys.Shift ) == Keys.Shift;

			if( !ctrl && !shift ) {

				// normal click: clear selection, select this node
				ClearSelection();
				SelectNode( node );
				m_AnchorNode = node;
			}
			else if( ctrl ) {

				// ctrl click: toggle this node
				if( m_SelectedNodes.Contains( node ) )
					UnselectNode( node );
				else
					SelectNode( node );
				m_AnchorNode = node;
			}
			else if( shift ) {

				// shift click: select range from anchor to this node, don't change anchor
				if( m_AnchorNode == null ) {

					// no anchor, behave like normal click but set anchor
					ClearSelection();
					SelectNodeWithoutAnchor( node );
					m_AnchorNode = node;
				}
				else {
					SelectRange( m_AnchorNode, node, ctrlMode: false );
					// don't change anchor here
				}
			}
			base.OnMouseDown( e );
		}

		protected virtual void OnSelectionChanged()
		{
			SelectionChanged?.Invoke( this, EventArgs.Empty );
		}

		void MultiSelectTreeView_KeyDown( object sender, KeyEventArgs e )
		{
			// handle up / down navigation
			if( e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ) {
				e.Handled = true;
				TreeNode next = null;
				if( m_SelectedNodes.Count > 0 ) {

					// pick last selected as focus
					TreeNode current = m_SelectedNodes.Last();
					next = ( e.KeyCode == Keys.Up ) ? GetPreviousVisibleNode( current ) : GetNextVisibleNode( current );
				}
				else {

					// pick first visible node
					next = this.TopNode;
				}

				if( next != null ) {
					bool ctrl = ( ModifierKeys & Keys.Control ) == Keys.Control;
					bool shift = ( ModifierKeys & Keys.Shift ) == Keys.Shift;

					if( !ctrl && !shift ) {
						ClearSelection();
						SelectNode( next );
						m_AnchorNode = next;
					}
					else if( ctrl ) {

						// toggle
						if( m_SelectedNodes.Contains( next ) )
							UnselectNode( next );
						else
							SelectNode( next );
						m_AnchorNode = next;
					}
					else if( shift ) {

						// shift navigation: don't change anchor
						if( m_AnchorNode == null ) {
							SelectNodeWithoutAnchor( next );
							m_AnchorNode = next;
						}
						else {
							SelectRange( m_AnchorNode, next, ctrlMode: false );
						}
					}

					// bring node into view
					this.SelectedNode = next;
					this.SelectedNode = null;
				}
			}

			// handle ctrl+a
			else if( e.Control && e.KeyCode == Keys.A ) {
				e.Handled = true;
				SelectAll();
			}

			// handle ctrl+r
			else if( e.Control && e.KeyCode == Keys.R ) {
				e.Handled = true;
				ReverseSelection();
			}
		}

		void SelectRange( TreeNode anchor, TreeNode current, bool ctrlMode )
		{
			if( anchor == null || current == null ) {
				return;
			}

			// build a list of visible nodes in display order
			List<TreeNode> visible = GetAllNodes( this.Nodes ).ToList();
			int idx1 = visible.IndexOf( anchor );
			int idx2 = visible.IndexOf( current );
			if( idx1 < 0 || idx2 < 0 )
				return;

			int start = Math.Min( idx1, idx2 );
			int end = Math.Max( idx1, idx2 );

			if( !ctrlMode ) {
				ClearSelection();
			}

			for( int i = start; i <= end; i++ ) {
				TreeNode n = visible[ i ];
				if( !m_SelectedNodes.Contains( n ) ) {
					m_SelectedNodes.Add( n );
					HighlightNode( n, true );
				}
			}

			OnSelectionChanged();
		}

		void HighlightNode( TreeNode node, bool highlight )
		{
			if( highlight ) {
				node.BackColor = SystemColors.Highlight;
			}
			else {
				node.BackColor = this.BackColor;
			}
		}

		IEnumerable<TreeNode> GetAllNodes( TreeNodeCollection nodes )
		{
			foreach( TreeNode n in nodes ) {
				yield return n;
				foreach( var child in GetAllNodes( n.Nodes ) ) {
					yield return child;
				}
			}
		}

		TreeNode GetNextVisibleNode( TreeNode node )
		{
			if( node == null ) {
				return null;
			}
			if( node.IsExpanded && node.Nodes.Count > 0 ) {
				return node.Nodes[ 0 ];
			}
			if( node.NextNode != null ) {
				return node.NextNode;
			}
			TreeNode p = node.Parent;
			while( p != null ) {
				if( p.NextNode != null )
					return p.NextNode;
				p = p.Parent;
			}
			return null;
		}

		TreeNode GetPreviousVisibleNode( TreeNode node )
		{
			if( node == null ) {
				return null;
			}
			if( node.PrevNode != null ) {

				// move to last visible child of previous sibling
				TreeNode n = node.PrevNode;
				while( n.IsExpanded && n.Nodes.Count > 0 ) {
					n = n.Nodes[ n.Nodes.Count - 1 ];
				}
				return n;
			}
			return node.Parent;
		}

		void MultiSelectTreeView_BeforeCollapse( object sender, TreeViewCancelEventArgs e )
		{
			e.Cancel = true;
		}

		void SelectNodeWithoutAnchor( TreeNode node )
		{
			if( node == null ) {
				return;
			}
			if( m_SelectedNodes.Contains( node ) ) {
				return;
			}
			m_SelectedNodes.Add( node );
			HighlightNode( node, true );
			OnSelectionChanged();
		}

		// maintains the selected nodes
		readonly List<TreeNode> m_SelectedNodes = new List<TreeNode>();

		// anchor node for shift selection
		TreeNode m_AnchorNode = null;
	}
}
