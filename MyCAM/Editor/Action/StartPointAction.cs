using MyCAM.Data;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class StartPointAction : EditActionBase
	{
		public StartPointAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager,
			string szPathID )
			: base( viewer, treeView, cadManager, viewManager )
		{
			if( string.IsNullOrEmpty( szPathID ) ) {
				throw new ArgumentNullException( "StartPointAction constructing argument null" );
			}
			m_PathID = szPathID;
			m_VertexMap = new Dictionary<gp_Pnt, Tuple<CAMData, int>>();
			MakeSelectPoint();
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.Default;
			}
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// disable tree view
			m_TreeView.Enabled = false;

			// deactivate all
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Deactivate( viewObject.AISHandle );
			}

			// show select point
			ShowSelectPoint();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// enable tree view
			m_TreeView.Enabled = true;

			// hide select point
			HideSelectPoint();
			base.End();
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				// end action
				End();
			}
		}

		void MakeSelectPoint()
		{
			// build wire from cad points
			CAMData camData = ( (PathData)m_CADManager.ShapeDataMap[ m_PathID ] ).CAMData;
			BRepBuilderAPI_MakePolygon polygonMaker = new BRepBuilderAPI_MakePolygon();

			// add points to the polygon, we do not add the last point
			for( int i = 0; i < camData.CADPointList.Count - 1; i++ ) {
				polygonMaker.Add( camData.CADPointList[ i ].Point );
				m_VertexMap.Add( camData.CADPointList[ i ].Point, new Tuple<CAMData, int>( camData, i ) );

			}
			if( polygonMaker.IsDone() == false ) {
				return;
			}
			TopoDS_Wire wire = polygonMaker.Wire();
			m_SelectedPointAIS = new AIS_Shape( wire );

			// set invisible
			m_SelectedPointAIS.SetWidth( 0.0 );
		}

		void ShowSelectPoint()
		{
			m_Viewer.GetAISContext().Display( m_SelectedPointAIS, true );
			m_Viewer.GetAISContext().Activate( m_SelectedPointAIS, (int)AISActiveMode.Vertex );
		}

		void HideSelectPoint()
		{
			m_Viewer.GetAISContext().Remove( m_SelectedPointAIS, true );
		}

		string m_PathID;

		// map point on view to index on CAMData
		Dictionary<gp_Pnt, Tuple<CAMData, int>> m_VertexMap;
		AIS_Shape m_SelectedPointAIS;
	}
}
