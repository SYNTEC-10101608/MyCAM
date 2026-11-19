using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Controls
{
	/// <summary>
	/// Tree view with multi selection (Ctrl / Shift click, Ctrl+A, Ctrl+R) and selection highlight in the theme color.
	/// </summary>
	public class MultiSelectTreeView : TreeView
	{
		/// <summary>
		/// Selected nodes in the order they were selected.
		/// </summary>
		[Browsable( false )]
		public IReadOnlyList<TreeNode> SelectedNodes => m_SelectedNodes.AsReadOnly();

		/// <summary>
		/// Occurs when the selected nodes change.
		/// </summary>
		public event EventHandler SelectionChanged;

		/// <summary>
		/// Occurs when a node is double clicked.
		/// </summary>
		public event TreeNodeMouseClickEventHandler NodeDoubleClicked;

		/// <summary>
		/// True to allow selecting more than one node; false makes every click a single selection.
		/// </summary>
		public bool isAllowMultiSelect
		{
			get; set;
		} = true;

		/// <summary>
		/// Create the tree view; nodes never collapse.
		/// </summary>
		public MultiSelectTreeView()
		{
			// appearance of the theme
			BorderStyle = BorderStyle.FixedSingle;
			ForeColor = ThemeColors.TEXT;

			// enable keyboard support
			this.HideSelection = false;

			// ensure no built-in selected node (we manage highlighting ourselves)
			base.SelectedNode = null;

			// subscribe to key down
			this.KeyDown += MultiSelectTreeView_KeyDown;

			// avoid collapse
			this.BeforeCollapse += MultiSelectTreeView_BeforeCollapse;

			// subscribe to double click
			this.NodeMouseDoubleClick += MultiSelectTreeView_NodeMouseDoubleClick;
		}

		/// <summary>
		/// Select the unselected nodes and unselect the selected ones (multi selection only).
		/// </summary>
		public void ReverseSelection()
		{
			// is not allow at single select mode
			if( isAllowMultiSelect == false ) {
				return;
			}
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

		/// <summary>
		/// Add the node to the selection; it becomes the anchor of Shift selection unless Shift is pressed.
		/// </summary>
		/// <param name="node">Node to select; null is ignored.</param>
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

		/// <summary>
		/// Remove the node from the selection.
		/// </summary>
		/// <param name="node">Node to unselect; null is ignored.</param>
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

		/// <summary>
		/// Unselect all nodes.
		/// </summary>
		public void ClearSelection()
		{
			if( m_SelectedNodes.Count == 0 ) {
				return;
			}
			ClearSelectionSilent();
			OnSelectionChanged();
		}

		void ClearSelectionSilent()
		{
			foreach( var n in m_SelectedNodes.ToList() ) {
				HighlightNode( n, false );
			}
			m_SelectedNodes.Clear();
		}

		/// <summary>
		/// Select all nodes (multi selection only).
		/// </summary>
		public void SelectAll()
		{
			if( isAllowMultiSelect == false ) {
				return;
			}
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

			if( !ctrl && !shift || !isAllowMultiSelect ) {

				// normal click: clear selection, select this node
				ClearSelectionSilent();
				SelectNode( node );
				m_AnchorNode = node;
			}
			else if( ctrl ) {

				// ctrl click: toggle this node
				if( m_SelectedNodes.Contains( node ) ) {
					UnselectNode( node );
				}
				else {
					SelectNode( node );
				}
				m_AnchorNode = node;
			}
			else if( shift ) {

				// shift click: select range from anchor to this node, don't change anchor
				if( m_AnchorNode == null ) {

					// no anchor, behave like normal click but set anchor
					ClearSelectionSilent();
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

		/// <summary>
		/// Raise <see cref="SelectionChanged"/>.
		/// </summary>
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

					// if no modifier or multi-select not allowed: clear selection and select next
					if( !ctrl && !shift || !isAllowMultiSelect ) {
						ClearSelection();
						SelectNode( next );
						m_AnchorNode = next;
					}
					else if( ctrl ) {

						// toggle
						if( m_SelectedNodes.Contains( next ) ) {
							UnselectNode( next );
						}
						else {
							SelectNode( next );
						}

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
			if( idx1 < 0 || idx2 < 0 ) {
				return;
			}

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
				node.BackColor = ThemeColors.ACCENT_TINT_STRONG;
			}
			else {
				node.BackColor = this.BackColor;
			}
		}

		/// <summary>
		/// Mark or unmark a node with a flag: emphasized error-colored text with a flag sign.
		/// </summary>
		/// <param name="node">Node to mark; null is ignored.</param>
		/// <param name="setFlag">True to add the flag, false to remove it.</param>
		public void SetFlagNode( TreeNode node, bool setFlag )
		{
			if( node == null ) {
				return;
			}

			// original text without flag
			string originalText = node.Text;
			if( originalText.EndsWith( " 🚩" ) ) {
				originalText = originalText.Substring( 0, originalText.Length - 3 );
			}

			if( setFlag ) {

				// change font style
				node.NodeFont = ThemeFonts.Emphasis;
				node.Text = originalText + " 🚩";
				node.ForeColor = ThemeColors.ERROR;

			}
			else {
				// reset font style
				node.NodeFont = this.Font;
				node.Text = originalText;
				node.ForeColor = this.ForeColor;
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
				if( p.NextNode != null ) {
					return p.NextNode;
				}

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

		void MultiSelectTreeView_NodeMouseDoubleClick( object sender, TreeNodeMouseClickEventArgs e )
		{
			NodeDoubleClicked?.Invoke( this, e );
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
