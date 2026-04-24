using MyCAM.Data;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCCViewer;

namespace MyCAM.Editor.Renderer
{
	internal class CoordIconRenderer : CAMRendererBase
	{
		public CoordIconRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void Show( bool bUpdate = false )
		{
			ShowTrihedron( null, bUpdate );
		}


		public void Show( gp_Pnt position, bool bUpdate = false )
		{
			ShowTrihedron( position, bUpdate );
		}

		public override void Remove( bool bUpdate = false )
		{
			m_Viewer.GetAISContext().Remove( m_CoordIconAIS, false );
		}

		public void ShowTrihedron( gp_Pnt position, bool bUpdate = false )
		{
			if( m_CoordIconAIS != null ) {
				m_Viewer.GetAISContext().Remove( m_CoordIconAIS, false );
			}

			// build at origin and then move to position
			var ax2 = new gp_Ax2( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ), new gp_Dir( 1, 0, 0 ) );
			var trihedron = new AIS_Trihedron( new Geom_Axis2Placement( ax2 ) );
			trihedron.SetColor( new Quantity_Color( DEFAULT_Color ) );
			trihedron.SetSize( 50.0 );
			trihedron.SetAxisColor( new Quantity_Color( DEFAULT_Color ) );
			trihedron.SetTextColor( new Quantity_Color( DEFAULT_Color ) );
			trihedron.SetArrowColor( new Quantity_Color( DEFAULT_Color ) );
			m_Viewer.GetAISContext().Display( trihedron, false );
			m_Viewer.GetAISContext().Deactivate( trihedron );
			m_CoordIconAIS = trihedron;

			// move the trihedron to the position, otherwise keep it at origin
			if( position != null ) {
				var trsf = new gp_Trsf();
				trsf.SetTranslation( new gp_Vec( position.X(), position.Y(), position.Z() ) );
				Trans( trsf );
			}

			if( bUpdate ) {
				UpdateView();
			}
		}

		public void Trans( gp_Trsf trsf )
		{
			if( m_CoordIconAIS != null ) {
				m_CoordIconAIS.SetLocalTransformation( trsf );
			}
		}

		AIS_Trihedron m_CoordIconAIS;
		Quantity_NameOfColor DEFAULT_Color = Quantity_NameOfColor.Quantity_NOC_GRAY;
	}
}
