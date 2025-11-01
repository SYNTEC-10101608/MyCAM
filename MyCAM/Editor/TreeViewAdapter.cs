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
		// Maintains the selected nodes
		private readonly List<TreeNode> _selectedNodes = new List<TreeNode>();
		// Anchor node for Shift selection
		private TreeNode _anchorNode = null;

		// Public read-only access to selected nodes
		[Browsable( false )]
		public IReadOnlyList<TreeNode> SelectedNodes => _selectedNodes.AsReadOnly();

		// Event for selection changes
		public event EventHandler SelectionChanged;

		public MultiSelectTreeView()
		{
			// Enable keyboard support
			this.HideSelection = false;
			// Ensure no built-in selected node (we manage highlighting ourselves)
			base.SelectedNode = null;
			// Subscribe to key down
			this.KeyDown += MultiSelectTreeView_KeyDown;
		}

		protected override void OnBeforeSelect( TreeViewCancelEventArgs e )
		{
			// Cancel built-in selection – we manage selection manually
			e.Cancel = true;
			base.OnBeforeSelect( e );
		}

		protected override void OnAfterSelect( TreeViewEventArgs e )
		{
			// Prevent built-in selection highlight
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
				// Normal click: clear selection, select this node
				ClearSelection();
				SelectNode( node );
				_anchorNode = node;
			}
			else if( ctrl ) {
				// Ctrl click: toggle this node
				if( _selectedNodes.Contains( node ) )
					UnselectNode( node );
				else
					SelectNode( node );
				_anchorNode = node;
			}
			else if( shift ) {
				// Shift click: select range from anchor to this node
				if( _anchorNode == null ) {
					// No anchor, behave like normal click
					ClearSelection();
					SelectNode( node );
					_anchorNode = node;
				}
				else {
					SelectRange( _anchorNode, node, ctrlMode: false );
				}
			}

			base.OnMouseDown( e );
		}

		private void MultiSelectTreeView_KeyDown( object sender, KeyEventArgs e )
		{
			// Handle Up / Down navigation
			if( e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ) {
				e.Handled = true;
				TreeNode next = null;
				if( _selectedNodes.Count > 0 ) {
					// pick last selected as focus
					TreeNode current = _selectedNodes.Last();
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
						_anchorNode = next;
					}
					else if( ctrl ) {
						// toggle
						if( _selectedNodes.Contains( next ) )
							UnselectNode( next );
						else
							SelectNode( next );
						_anchorNode = next;
					}
					else if( shift ) {
						if( _anchorNode == null ) {
							SelectNode( next );
							_anchorNode = next;
						}
						else {
							SelectRange( _anchorNode, next, ctrlMode: false );
						}
					}

					// bring node into view
					this.SelectedNode = next;  // temporarily set to scroll into view
					this.SelectedNode = null;
				}
			}
			// Handle Ctrl+A
			else if( e.Control && e.KeyCode == Keys.A ) {
				e.Handled = true;
				SelectAll();
			}
		}

		/// <summary>
		/// Selects the given node (adds to selection).
		/// </summary>
		public void SelectNode( TreeNode node )
		{
			if( node == null )
				return;
			if( _selectedNodes.Contains( node ) )
				return;
			_selectedNodes.Add( node );
			HighlightNode( node, true );
			OnSelectionChanged();
		}

		/// <summary>
		/// Unselects the given node (removes from selection).
		/// </summary>
		public void UnselectNode( TreeNode node )
		{
			if( node == null )
				return;
			if( _selectedNodes.Remove( node ) ) {
				HighlightNode( node, false );
				OnSelectionChanged();
			}
		}

		/// <summary>
		/// Clears all selections.
		/// </summary>
		public void ClearSelection()
		{
			if( _selectedNodes.Count == 0 )
				return;
			foreach( var n in _selectedNodes.ToList() ) {
				HighlightNode( n, false );
			}
			_selectedNodes.Clear();
			OnSelectionChanged();
		}

		/// <summary>
		/// Selects all nodes in the tree (pre-order).
		/// </summary>
		public void SelectAll()
		{
			ClearSelection();
			foreach( TreeNode n in GetAllNodes( this.Nodes ) ) {
				_selectedNodes.Add( n );
				HighlightNode( n, true );
			}
			OnSelectionChanged();
		}

		/// <summary>
		/// Helper: select a contiguous range between two nodes in tree-visible order.
		/// If ctrlMode is true, we don’t clear existing, otherwise we clear first.
		/// </summary>
		private void SelectRange( TreeNode anchor, TreeNode current, bool ctrlMode )
		{
			if( anchor == null || current == null )
				return;

			// Build a list of visible nodes in display order
			List<TreeNode> visible = GetVisibleNodes().ToList();
			int idx1 = visible.IndexOf( anchor );
			int idx2 = visible.IndexOf( current );
			if( idx1 < 0 || idx2 < 0 )
				return;

			int start = Math.Min( idx1, idx2 );
			int end = Math.Max( idx1, idx2 );

			if( !ctrlMode )
				ClearSelection();

			for( int i = start; i <= end; i++ ) {
				TreeNode n = visible[ i ];
				if( !_selectedNodes.Contains( n ) ) {
					_selectedNodes.Add( n );
					HighlightNode( n, true );
				}
			}

			OnSelectionChanged();
		}

		/// <summary>
		/// Raise selection changed event.
		/// </summary>
		protected virtual void OnSelectionChanged()
		{
			SelectionChanged?.Invoke( this, EventArgs.Empty );
		}

		/// <summary>
		/// Highlight or un-highlight a node (change BackColor/ForeColor).
		/// </summary>
		private void HighlightNode( TreeNode node, bool highlight )
		{
			if( highlight ) {
				node.BackColor = SystemColors.Highlight;
				node.ForeColor = SystemColors.HighlightText;
			}
			else {
				node.BackColor = this.BackColor;
				node.ForeColor = this.ForeColor;
			}
		}

		/// <summary>
		/// Get all nodes under a collection (recursive).
		/// </summary>
		private IEnumerable<TreeNode> GetAllNodes( TreeNodeCollection nodes )
		{
			foreach( TreeNode n in nodes ) {
				yield return n;
				foreach( var child in GetAllNodes( n.Nodes ) )
					yield return child;
			}
		}

		/// <summary>
		/// Get a flat list of visible nodes (in expand/collapse visible order).
		/// </summary>
		private IEnumerable<TreeNode> GetVisibleNodes()
		{
			TreeNode n = this.TopNode;
			while( n != null ) {
				yield return n;
				if( n.IsExpanded && n.Nodes.Count > 0 ) {
					n = n.Nodes[ 0 ];
				}
				else {
					// go to next sibling or parent’s next sibling
					while( n != null && n.NextNode == null )
						n = n.Parent;
					if( n != null )
						n = n.NextNode;
				}
			}
		}

		/// <summary>
		/// Gets the next visible node after the given node, or null.
		/// </summary>
		private TreeNode GetNextVisibleNode( TreeNode node )
		{
			if( node == null )
				return null;
			if( node.IsExpanded && node.Nodes.Count > 0 )
				return node.Nodes[ 0 ];
			if( node.NextNode != null )
				return node.NextNode;
			TreeNode p = node.Parent;
			while( p != null ) {
				if( p.NextNode != null )
					return p.NextNode;
				p = p.Parent;
			}
			return null;
		}

		/// <summary>
		/// Gets the previous visible node before the given node, or null.
		/// </summary>
		private TreeNode GetPreviousVisibleNode( TreeNode node )
		{
			if( node == null )
				return null;
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
	}
}
