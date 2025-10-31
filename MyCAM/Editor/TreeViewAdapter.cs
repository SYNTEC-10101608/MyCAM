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

		/// <summary>
		/// Occurs when the collection of selected nodes changes (added, removed, cleared).
		/// </summary>
		public event EventHandler SelectedNodesChanged;

		/// <summary>
		/// Gets the currently selected nodes (read-only).
		/// </summary>
		[Browsable( false )]
		public IReadOnlyCollection<TreeNode> SelectedNodes => _selectedNodes;

		/// <summary>
		/// If true and CheckBoxes = true, then when a node is checked/unchecked we cascade to its children.
		/// </summary>
		[DefaultValue( false )]
		public bool CascadeCheck
		{
			get => _cascadeCheck;
			set => _cascadeCheck = value;
		}

		public MultiSelectTreeView()
		{
			// Enable owner‐draw so we can highlight multiple nodes.
			this.DrawMode = TreeViewDrawMode.OwnerDrawText;
			this.HideSelection = false;
			this.CheckBoxes = false;  // default no checkboxes; user may enable manually
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

			// intercept base selection behavior
			e.Cancel = true;

			TreeNode node = e.Node;
			bool ctrl = ( ModifierKeys & Keys.Control ) == Keys.Control;
			bool shift = ( ModifierKeys & Keys.Shift ) == Keys.Shift;

			if( !ctrl && !shift ) {
				// single click without modifiers → clear and select this only
				ClearSelection();
				AddNodeToSelection( node );
			}
			else if( ctrl && !shift ) {
				// Ctrl + click: toggle
				if( _selectedNodes.Contains( node ) )
					RemoveNodeFromSelection( node );
				else
					AddNodeToSelection( node );
				_lastNode = node;
			}
			else if( shift ) {
				// Shift + click: select range from last to this (within same parent or root)
				if( _lastNode == null ) {
					AddNodeToSelection( node );
				}
				else {
					TreeNode start = _lastNode;
					TreeNode end = node;

					TreeNodeCollection siblings;
					if( start.Parent == end.Parent && start.Parent != null )
						siblings = start.Parent.Nodes;
					else if( start.Parent == null && end.Parent == null )
						siblings = this.Nodes;
					else
						siblings = null;

					if( siblings != null ) {
						int startIndex = siblings.IndexOf( start );
						int endIndex = siblings.IndexOf( end );
						if( startIndex > endIndex ) {
							int tmp = startIndex;
							startIndex = endIndex;
							endIndex = tmp;
						}
						ClearSelection();
						for( int i = startIndex; i <= endIndex; i++ ) {
							AddNodeToSelection( siblings[ i ] );
						}
					}
					else {
						// fallback to single
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
			// we intentionally do not use base.SelectedNode
			// Raise event for changed selected nodes
			OnSelectedNodesChanged( EventArgs.Empty );
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );
			// If checkboxes are enabled and CascadeCheck is true, handle checking logic
			if( this.CheckBoxes && _cascadeCheck ) {
				TreeNode node = this.GetNodeAt( e.Location );
				if( node != null ) {
					// let the normal check/uncheck happen then cascade in AfterCheck
				}
			}
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
				bool allSiblingsChecked = node.Parent.Nodes.Cast<TreeNode>().All( n => n.Checked );
				node.Parent.Checked = allSiblingsChecked;
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
			if( node != null && node.Bounds != Rectangle.Empty ) {
				this.Invalidate( node.Bounds );
			}
		}

		/// <summary>
		/// Programmatically select a single node (clearing previous selection).
		/// </summary>
		public void SelectNode( TreeNode node )
		{
			ClearSelection();
			AddNodeToSelection( node );
		}

		/// <summary>
		/// Programmatically select multiple nodes (clearing previous selection).
		/// </summary>
		public void SelectNodes( IEnumerable<TreeNode> nodes )
		{
			ClearSelection();
			foreach( var n in nodes ) {
				AddNodeToSelection( n );
			}
		}

		protected virtual void OnSelectedNodesChanged( EventArgs e )
		{
			SelectedNodesChanged?.Invoke( this, e );
		}
	}
}
