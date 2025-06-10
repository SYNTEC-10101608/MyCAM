using OCCViewer;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	internal class SelectFaceAction : CADACtionBase
	{
		public SelectFaceAction( Viewer viewer, TreeView treeView, CADManager cadManager )
			: base( viewer, treeView, cadManager )
		{
		}

		public override CADActionType ActionType
		{
			get
			{
				return CADActionType.SelectFace;
			}
		}

		public override void Start()
		{
			base.Start();
		}

		public override void End()
		{
			base.End();
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
		}

		protected override void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
		}

		protected override void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
		}
	}
}
