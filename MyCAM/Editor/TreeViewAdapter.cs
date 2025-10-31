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
		private bool _cascadeCheck = false;

		public event EventHandler SelectedNodesChanged;

		[Browsable( false )]
		public IReadOnlyCollection<TreeNode> SelectedNodes => _selectedNodes;

		[DefaultValue( false )]
		public bool CascadeCheck
		{
			get => _cascadeCheck;
			set => _cascadeCheck = value;
		}

		public MultiSelectTreeView()
		{
			this.DrawMode = TreeViewDrawMode.OwnerDrawText;
			this.HideSelection = false;
			// Allow checkboxes if wanted, user must set CheckBoxes = true in designer/code
			this.CheckBoxes = false;
		}

		protected override void OnDrawNode( DrawTreeNodeEventArgs e )
		{
			if( _selectedNodes.Contains( e.Node ) ) {
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

			e.Cancel = true;  // we will handle selection ourselves

			TreeNode node = e.Node;
			bool ctrl = ( ModifierKeys & Keys.Control ) == Keys.Control;
			bool shift = ( ModifierKeys & Keys.Shift ) == Keys.Shift;

			if( !ctrl && !shift ) {
				ClearSelection();
				AddNodeToSelection( node );
			}
			else if( ctrl && !shift ) {
				if( _selectedNodes.Contains( node ) )
					RemoveNodeFromSelection( node );
				else
					AddNodeToSelection( node );
				_lastNode = node;
			}
			else if( shift ) {
				if( _lastNode == null ) {
					AddNodeToSelection( node );
				}
				else {
					// attempt range select across same parent or root
					TreeNode start = _lastNode;
					TreeNode end = node;
					TreeNodeCollection siblings = null;

					if( start.Parent == end.Parent && start.Parent != null )
						siblings = start.Parent.Nodes;
					else if( start.Parent == null && end.Parent == null )
						siblings = this.Nodes;

					if( siblings != null ) {
						int startIndex = siblings.IndexOf( start );
						int endIndex = siblings.IndexOf( end );
						if( startIndex > endIndex ) {
							var tmp = startIndex;
							startIndex = endIndex;
							endIndex = tmp;
						}
						ClearSelection();
						for( int i = startIndex; i <= endIndex; i++ ) {
							AddNodeToSelection( siblings[ i ] );
						}
					}
					else {
						ClearSelection();
						AddNodeToSelection( node );
					}
				}
			}

			base.OnBeforeSelect( e );
		}

		protected override void OnAfterSelect( TreeViewEventArgs e )
		{
			if( !_suppressSelectEvent ) {
				_suppressSelectEvent = true;
				base.OnAfterSelect( e );
				_suppressSelectEvent = false;
			}
			OnSelectedNodesChanged( EventArgs.Empty );
		}

		protected override bool IsInputKey( Keys keyData )
		{
			// Let Up/Down/Home/End/PageUp/PageDown be input keys so OnKeyDown will receive them.
			if( ( keyData & Keys.KeyCode ) == Keys.Up ||
				( keyData & Keys.KeyCode ) == Keys.Down ||
				( keyData & Keys.KeyCode ) == Keys.Home ||
				( keyData & Keys.KeyCode ) == Keys.End ||
				( keyData & Keys.KeyCode ) == Keys.PageUp ||
				( keyData & Keys.KeyCode ) == Keys.PageDown ||
				( keyData & Keys.KeyCode ) == Keys.A && ( ModifierKeys & Keys.Control ) == Keys.Control ) {
				return true;
			}
			return base.IsInputKey( keyData );
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			base.OnKeyDown( e );

			if( e.Control && e.KeyCode == Keys.A ) {
				// Ctrl+A = select all nodes
				SelectAllNodes();
				e.Handled = true;
				return;
			}

			if( !_selectedNodes.Any() )
				return;

			TreeNode currentFocus = _lastNode ?? _selectedNodes.First();

			TreeNode next = null;
			switch( e.KeyCode ) {
				case Keys.Up:
					next = GetPreviousNode( currentFocus );
					break;
				case Keys.Down:
					next = GetNextNode( currentFocus );
					break;
				case Keys.Home:
					next = GetFirstVisibleNode();
					break;
				case Keys.End:
					next = GetLastVisibleNode();
					break;
				case Keys.PageUp:
					next = GetPageUpNode( currentFocus );
					break;
				case Keys.PageDown:
					next = GetPageDownNode( currentFocus );
					break;
			}

			if( next != null ) {
				ClearSelection();
				AddNodeToSelection( next );
				this.SelectedNode = next;  // ensures focus is set
				e.Handled = true;
			}
		}

		private void SelectAllNodes()
		{
			ClearSelection();
			foreach( TreeNode node in GetAllNodesRecursive( this.Nodes ) ) {
				AddNodeToSelection( node );
			}
		}

		private IEnumerable<TreeNode> GetAllNodesRecursive( TreeNodeCollection nodes )
		{
			foreach( TreeNode n in nodes ) {
				yield return n;
				foreach( var child in GetAllNodesRecursive( n.Nodes ) )
					yield return child;
			}
		}

		private TreeNode GetFirstVisibleNode()
		{
			return this.Nodes.Cast<TreeNode>().FirstOrDefault( n => n != null );
		}

		private TreeNode GetLastVisibleNode()
		{
			var all = GetAllNodesRecursive( this.Nodes ).ToList();
			return all.LastOrDefault();
		}

		private TreeNode GetNextNode( TreeNode node )
		{
			// In‑order traversal of visible nodes (all expanded).
			if( node == null )
				return null;

			// children first
			if( node.IsExpanded && node.Nodes.Count > 0 )
				return node.Nodes[ 0 ];

			// next sibling
			var parent = node.Parent;
			TreeNodeCollection siblings = parent != null ? parent.Nodes : this.Nodes;
			int idx = siblings.IndexOf( node );
			if( idx < siblings.Count - 1 )
				return siblings[ idx + 1 ];

			// climb up until you find a next sibling
			TreeNode ancestor = parent;
			while( ancestor != null ) {
				TreeNodeCollection ancSib = ancestor.Parent != null ? ancestor.Parent.Nodes : this.Nodes;
				int ancIdx = ancSib.IndexOf( ancestor );
				if( ancIdx < ancSib.Count - 1 )
					return ancSib[ ancIdx + 1 ];
				ancestor = ancestor.Parent;
			}

			return null;
		}

		private TreeNode GetPreviousNode( TreeNode node )
		{
			if( node == null )
				return null;

			var parent = node.Parent;
			TreeNodeCollection siblings = parent != null ? parent.Nodes : this.Nodes;
			int idx = siblings.IndexOf( node );

			if( idx > 0 ) {
				// previous sibling, then go deep into its last visible child
				TreeNode prev = siblings[ idx - 1 ];
				// go to deepest visible child
				while( prev.IsExpanded && prev.Nodes.Count > 0 )
					prev = prev.Nodes[ prev.Nodes.Count - 1 ];
				return prev;
			}

			// no previous sibling so parent is previous
			if( parent != null )
				return parent;

			return null;
		}

		private TreeNode GetPageUpNode( TreeNode node )
		{
			// simple implementation: move up by e.g. 10 nodes
			const int step = 10;
			TreeNode current = node;
			for( int i = 0; i < step; i++ ) {
				var prev = GetPreviousNode( current );
				if( prev == null )
					break;
				current = prev;
			}
			return current;
		}

		private TreeNode GetPageDownNode( TreeNode node )
		{
			const int step = 10;
			TreeNode current = node;
			for( int i = 0; i < step; i++ ) {
				var nxt = GetNextNode( current );
				if( nxt == null )
					break;
				current = nxt;
			}
			return current;
		}

		protected override void OnAfterCheck( TreeViewEventArgs e )
		{
			base.OnAfterCheck( e );
			if( this.CheckBoxes && _cascadeCheck ) {
				TreeNode node = e.Node;
				bool isChecked = node.Checked;
				CascadeCheckChildren( node, isChecked );
				CascadeCheckParent( node );
			}
		}

		private void CascadeCheckChildren( TreeNode node, bool isChecked )
		{
			foreach( TreeNode child in node.Nodes ) {
				child.Checked = isChecked;
				CascadeCheckChildren( child, isChecked );
			}
		}

		private void CascadeCheckParent( TreeNode node )
		{
			if( node.Parent != null ) {
				bool allSibsChecked = node.Parent.Nodes.Cast<TreeNode>().All( n => n.Checked );
				node.Parent.Checked = allSibsChecked;
				CascadeCheckParent( node.Parent );
			}
		}

		private void AddNodeToSelection( TreeNode node )
		{
			if( node == null )
				return;
			if( !_selectedNodes.Contains( node ) ) {
				_selectedNodes.Add( node );
				_lastNode = node;
				node.BackColor = SystemColors.Highlight;
				node.ForeColor = SystemColors.HighlightText;
				InvalidateNode( node );
				OnSelectedNodesChanged( EventArgs.Empty );
			}
		}

		private void RemoveNodeFromSelection( TreeNode node )
		{
			if( node == null )
				return;
			if( _selectedNodes.Contains( node ) ) {
				_selectedNodes.Remove( node );
				node.BackColor = this.BackColor;
				node.ForeColor = this.ForeColor;
				InvalidateNode( node );
				OnSelectedNodesChanged( EventArgs.Empty );
			}
		}

		public void ClearSelection()
		{
			if( _selectedNodes.Count == 0 )
				return;
			foreach( var n in _selectedNodes.ToList() ) {
				n.BackColor = this.BackColor;
				n.ForeColor = this.ForeColor;
				InvalidateNode( n );
			}
			_selectedNodes.Clear();
			_lastNode = null;
			OnSelectedNodesChanged( EventArgs.Empty );
		}

		private void InvalidateNode( TreeNode node )
		{
			if( node != null && node.Bounds != Rectangle.Empty )
				this.Invalidate( node.Bounds );
		}

		public void SelectNode( TreeNode node )
		{
			ClearSelection();
			AddNodeToSelection( node );
		}

		public void SelectNodes( IEnumerable<TreeNode> nodes )
		{
			ClearSelection();
			foreach( var n in nodes )
				AddNodeToSelection( n );
		}

		protected virtual void OnSelectedNodesChanged( EventArgs e )
		{
			SelectedNodesChanged?.Invoke( this, e );
		}
	}
}
