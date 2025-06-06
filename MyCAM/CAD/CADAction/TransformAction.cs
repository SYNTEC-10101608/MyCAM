using OCCViewer;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	internal class TransformAction : CADACtionBase
	{
		public TransformAction( Viewer viewer, TreeView treeView, CADManager cadManager )
			: base( viewer, treeView, cadManager )
		{
		}

		public override CADActionType ActionType
		{
			get
			{
				return CADActionType.Default;
			}
		}

		public override void Start()
		{
			base.Start();

			// disable tree view
			m_TreeView.Enabled = false;

			// reset activation mode
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Activate( viewObject.AISHandle );
			}
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// enable tree view
			m_TreeView.Enabled = true;

			// reset activation mode
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Deactivate();
			}
			base.End();
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
		}

		public void ApplyTransform( ETransformType type, bool bReverse = false )
		{
		}
	}
}
