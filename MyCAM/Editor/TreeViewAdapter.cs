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
		private HashSet<TreeNode> _selectedNodes = new HashSet<TreeNode>();
		private TreeNode _lastNode = null;
		private bool _suppressSelectEvent = false;

		[Browsable( false )]
		public IReadOnlyCollection<TreeNode> SelectedNodes => _selectedNodes;

		public MultiSelectTreeView()
		{
			// Enable owner draw so we can highlight multiple nodes.
			this.DrawMode = TreeViewDrawMode.OwnerDrawText;
			this.HideSelection = false;
		}

		protected override void OnDrawNode( DrawTreeNodeEventArgs e )
		{
			if( _selectedNodes.Contains( e.Node ) ) {
				// draw selected background
				e.Graphics.FillRectangle( SystemBrushes.Highlight, e.Bounds );
				TextRenderer.DrawText( e.Graphics,
									  e.Node.Text,
									  this.Font,
									  e.Bounds,
									  SystemColors.HighlightText,
									  TextFormatFlags.GlyphOverhangPadding );
			}
			else {
				e.DrawDefault = true;
				base.OnDrawNode( e );
			}
		}

		protected override void OnBeforeSelect( TreeViewCancelEventArgs e )
		{
			if( _suppressSelectEvent ) {
				base.OnBeforeSelect( e );
				return;
			}

			// We intercept selection and handle our multi-select logic.
			e.Cancel = true;

			TreeNode node = e.Node;
			bool ctrl = ( ModifierKeys & Keys.Control ) == Keys.Control;
			bool shift = ( ModifierKeys & Keys.Shift ) == Keys.Shift;

			if( !ctrl && !shift ) {
				// Single click without modifier: clear previous and select only this
				ClearSelection();
				AddNodeToSelection( node );
			}
			else if( ctrl && !shift ) {
				// Ctrl + click toggles selection of the node
				if( _selectedNodes.Contains( node ) )
					RemoveNodeFromSelection( node );
				else
					AddNodeToSelection( node );

				_lastNode = node;
			}
			else if( shift ) {
				// Shift + click: select range from last node to this node in same parent collection
				if( _lastNode == null ) {
					AddNodeToSelection( node );
				}
				else {
					TreeNode start = _lastNode;
					TreeNode end = node;

					// Find common parent
					TreeNode parent = start.Parent == end.Parent ? start.Parent : null;
					TreeNodeCollection siblings;
					if( parent != null )
						siblings = parent.Nodes;
					else
						siblings = this.Nodes;

					int startIndex = siblings.IndexOf( start );
					int endIndex = siblings.IndexOf( end );
					if( startIndex > endIndex ) {
						int tmp = startIndex;
						startIndex = endIndex;
						endIndex = tmp;
					}

					ClearSelection();
					for( int i = startIndex; i <= endIndex; i++ )
						AddNodeToSelection( siblings[ i ] );
				}
			}

			base.OnBeforeSelect( e );
		}

		protected override void OnAfterSelect( TreeViewEventArgs e )
		{
			// Prevent the base TreeView from doing its default single‐select logic
			if( !_suppressSelectEvent ) {
				_suppressSelectEvent = true;
				base.OnAfterSelect( e );
				_suppressSelectEvent = false;
			}

			// Raise our own event if needed (you could add a SelectedNodesChanged event)
		}

		private void AddNodeToSelection( TreeNode node )
		{
			_selectedNodes.Add( node );
			_lastNode = node;
			node.BackColor = SystemColors.Highlight;
			node.ForeColor = SystemColors.HighlightText;
			this.InvalidateNode( node );
		}

		private void RemoveNodeFromSelection( TreeNode node )
		{
			_selectedNodes.Remove( node );
			node.BackColor = this.BackColor;
			node.ForeColor = this.ForeColor;
			this.InvalidateNode( node );
		}

		public void ClearSelection()
		{
			foreach( var n in _selectedNodes.ToList() ) {
				n.BackColor = this.BackColor;
				n.ForeColor = this.ForeColor;
				this.InvalidateNode( n );
			}

			_selectedNodes.Clear();
		}

		private void InvalidateNode( TreeNode node )
		{
			if( node != null ) {
				Rectangle r = node.Bounds;
				if( !r.IsEmpty )
					this.Invalidate( r );
			}
		}

		/// <summary>
		/// Utility: programmatically select the given node (clearing others) 
		/// </summary>
		/// <param name="node"></param>
		public void SelectNode( TreeNode node )
		{
			ClearSelection();
			AddNodeToSelection( node );
		}

		/// <summary>
		/// Utility: programmatically select multiple nodes (clearing existing selection) 
		/// </summary>
		/// <param name="nodes"></param>
		public void SelectNodes( IEnumerable<TreeNode> nodes )
		{
			ClearSelection();
			foreach( var n in nodes )
				AddNodeToSelection( n );
		}
	}
}
