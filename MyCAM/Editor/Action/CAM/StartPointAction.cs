using System;
using System.Windows.Forms;
using MyCAM.Data;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class StartPointAction : IndexSelectAction
	{
		public StartPointAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager,
			CAMData camData )
			: base( dataManager, viewer, treeView, viewManager, camData )
		{
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.StartPoint;
			}
		}

		public Action PropertyChanged;

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}
			(int, int) nIndex = GetSelectIndex( out _ );
			if( nIndex == (-1, -1) ) {
				return;
			}
			(int,int) adjustedIndex = adjustStartPoint( nIndex );
			m_CAMData.NewStartPoint = nIndex;
			PropertyChanged?.Invoke();
			m_Viewer.GetAISContext().ClearSelected( true );
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		(int segment, int startPointIndex) adjustStartPoint( (int, int) startPointIndex )
		{
			bool isLastPoint = m_CAMData.CADSegmentList[ startPointIndex.Item1 ].PointList.Count == startPointIndex.Item2 + 1;

			// change start point to next segment first index
			if( isLastPoint ) {
				bool isLastSegment = m_CAMData.CADSegmentList.Count == ( startPointIndex.Item1 + 1 );

				// last segment and last point
				if( isLastSegment ) {
					return (0, 0);
				}
				return (startPointIndex.Item1 + 1, 0);
			}
			return startPointIndex;
		}
	}
}
