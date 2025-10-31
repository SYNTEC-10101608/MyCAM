using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace MyCAM.Editor
{
	public interface IMultiSelectTreeView
	{
		TreeView UnderlyingTreeView
		{
			get;
		}
		IReadOnlyCollection<TreeNode> SelectedNodes
		{
			get;
		}
		void SelectNode( TreeNode node );
		void DeselectNode( TreeNode node );
		void ToggleNodeSelection( TreeNode node );
		void ClearSelection();
	}

	public class MultiSelectTreeViewDecorator : IMultiSelectTreeView
	{
		private readonly TreeView _treeView;
		private readonly HashSet<TreeNode> _selectedNodes = new HashSet<TreeNode>();
		private TreeNode _lastNodeClicked;
		// store the starting node for Shift‑range
		private TreeNode _rangeStartNode;

		public Action SelectionChanged;

		public MultiSelectTreeViewDecorator( TreeView treeView )
		{
			_treeView = treeView ?? throw new ArgumentNullException( nameof( treeView ) );
			AttachEvents();
		}

		public TreeView UnderlyingTreeView => _treeView;

		public IReadOnlyCollection<TreeNode> SelectedNodes => _selectedNodes.ToList().AsReadOnly();

		private void AttachEvents()
		{
			_treeView.BeforeSelect += TreeView_BeforeSelect;
			_treeView.NodeMouseClick += TreeView_NodeMouseClick;
			_treeView.KeyDown += TreeView_KeyDown;
			_treeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
			_treeView.DrawNode += TreeView_DrawNode;
		}

		private void TreeView_BeforeSelect( object sender, TreeViewCancelEventArgs e )
		{
			// Prevent default selection change 
			e.Cancel = true;
		}

		private void TreeView_NodeMouseClick( object sender, TreeNodeMouseClickEventArgs e )
		{
			_treeView.Focus();

			bool ctrl = ( Control.ModifierKeys & Keys.Control ) == Keys.Control;
			bool shift = ( Control.ModifierKeys & Keys.Shift ) == Keys.Shift;

			if( ctrl ) {
				// Ctrl key toggles selection
				ToggleNodeSelection( e.Node );
				_rangeStartNode = e.Node;
			}
			else if( shift && _rangeStartNode != null ) {
				// SHIFT pressed: select range between _rangeStartNode and clicked node
				SelectRange( _rangeStartNode, e.Node );
			}
			else {
				// No modifier: clear previous selection and start new
				ClearSelection();
				SelectNode( e.Node );
				_rangeStartNode = e.Node;
			}

			_lastNodeClicked = e.Node;
		}

		private void TreeView_KeyDown( object sender, KeyEventArgs e )
		{
			// Example: Ctrl+A to select all
			if( e.Control && e.KeyCode == Keys.A ) {
				SelectAllNodes();
				e.Handled = true;
			}
			// You can add more keyboard logic here (e.g., Shift+Arrow)
		}

		private void TreeView_DrawNode( object sender, DrawTreeNodeEventArgs e )
		{
			if( _selectedNodes.Contains( e.Node ) ) {
				e.Graphics.FillRectangle( SystemBrushes.Highlight, e.Node.Bounds );
				TextRenderer.DrawText( e.Graphics, e.Node.Text, _treeView.Font,
					e.Node.Bounds, SystemColors.HighlightText, TextFormatFlags.GlyphOverhangPadding );
			}
			else {
				e.DrawDefault = true;
			}
		}

		private void SelectRange( TreeNode startNode, TreeNode endNode )
		{
			if( startNode == null || endNode == null ) {
				SelectNode( endNode );
				return;
			}

			// Clear previous selection
			ClearSelection();

			// If same parent, simple sibling-range
			if( startNode.Parent == endNode.Parent ) {
				TreeNodeCollection nodes = startNode.Parent == null
					? _treeView.Nodes
					: startNode.Parent.Nodes;

				int idx1 = nodes.IndexOf( startNode );
				int idx2 = nodes.IndexOf( endNode );
				if( idx1 > idx2 ) {
					var t = idx1;
					idx1 = idx2;
					idx2 = t;
				}
				for( int i = idx1; i <= idx2; i++ ) {
					SelectNode( nodes[ i ] );
				}
			}
			else {
				// More complex scenario: tree nodes may be in different branches.
				// Here we handle a “flat list” of all nodes in traversal order, then pick between indices.
				List<TreeNode> all = GetAllVisibleNodes( _treeView ).ToList();
				int i1 = all.IndexOf( startNode );
				int i2 = all.IndexOf( endNode );
				if( i1 < 0 || i2 < 0 ) {
					// fallback to selecting just endNode
					SelectNode( endNode );
				}
				else {
					if( i1 > i2 ) {
						var t = i1;
						i1 = i2;
						i2 = t;
					}
					for( int i = i1; i <= i2; i++ ) {
						SelectNode( all[ i ] );
					}
				}
			}
		}

		private IEnumerable<TreeNode> GetAllVisibleNodes( TreeView tree )
		{
			foreach( TreeNode root in tree.Nodes ) {
				foreach( var node in Traverse( root ) ) {
					yield return node;
				}
			}
		}

		private IEnumerable<TreeNode> Traverse( TreeNode node )
		{
			yield return node;
			foreach( TreeNode child in node.Nodes ) {
				foreach( var desc in Traverse( child ) )
					yield return desc;
			}
		}

		public void SelectNode( TreeNode node )
		{
			if( node == null )
				return;
			if( _selectedNodes.Add( node ) ) {
				OnSelectionChanged();
				_treeView.Invalidate( node.Bounds );
				_treeView.Update();
			}
		}

		public void DeselectNode( TreeNode node )
		{
			if( node == null )
				return;
			if( _selectedNodes.Remove( node ) ) {
				OnSelectionChanged();
				_treeView.Invalidate( node.Bounds );
			}
		}

		public void ToggleNodeSelection( TreeNode node )
		{
			if( node == null )
				return;
			if( _selectedNodes.Contains( node ) )
				DeselectNode( node );
			else
				SelectNode( node );
		}

		public void ClearSelection()
		{
			if( _selectedNodes.Count == 0 )
				return;
			foreach( var node in _selectedNodes.ToList() ) {
				_treeView.Invalidate( node.Bounds );
			}
			_selectedNodes.Clear();
			OnSelectionChanged();
		}

		public void SelectAllNodes()
		{
			ClearSelection();
			foreach( TreeNode node in _treeView.Nodes ) {
				SelectNodeRecursive( node );
			}
		}

		private void SelectNodeRecursive( TreeNode node )
		{
			SelectNode( node );
			foreach( TreeNode child in node.Nodes ) {
				SelectNodeRecursive( child );
			}
		}

		private void OnSelectionChanged()
		{
			SelectionChanged?.Invoke();
		}
	}
}
