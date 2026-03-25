using MyCAM.Data;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Quantity;
using OCC.TCollection;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor.Renderer
{
	internal class REFPointRenderer : CAMRendererBase
	{
		AIS_Shape m_REFPnt1 = null;
		AIS_Shape m_REFPnt2 = null;
		AIS_Shape m_REFPnt3 = null;
		AIS_TextLabel m_REFText1 = null;
		AIS_TextLabel m_REFText2 = null;
		AIS_TextLabel m_REFText3 = null;


		public REFPointRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void Show( bool bUpdate = false )
		{
			Remove();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( false );

			// show point ais
			bool isGetREFPntSuccess = DataGettingHelper.GetREFPnt( out CalibrationData calibrationData );
			if( !isGetREFPntSuccess || calibrationData == null ) {
				return;
			}
			if( calibrationData.Ref_Pnt1 != null ) {
				m_REFPnt1 = DrawPntOnViewer( calibrationData.Ref_Pnt1, PNT_Color );
			}
			if( calibrationData.Ref_Pnt2 != null ) {
				m_REFPnt2 = DrawPntOnViewer( calibrationData.Ref_Pnt2, PNT_Color );
			}
			if( calibrationData.Ref_Pnt3 != null ) {
				m_REFPnt3 = DrawPntOnViewer( calibrationData.Ref_Pnt3, PNT_Color );
			}

			// show text label
			if( calibrationData.Ref_Pnt1 != null ) {
				m_REFText1 = WriteTextOnViewer( calibrationData.Ref_Pnt1, 1 );
			}
			if( calibrationData.Ref_Pnt2 != null ) {
				m_REFText2 = WriteTextOnViewer( calibrationData.Ref_Pnt2, 2 );
			}
			if( calibrationData.Ref_Pnt3 != null ) {
				m_REFText3 = WriteTextOnViewer( calibrationData.Ref_Pnt3, 3 );
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		public override void Remove( bool bUpdate = false )
		{
			// remove point ais
			if( m_REFPnt1 != null && !m_REFPnt1.IsNull() ) {
				m_Viewer.GetAISContext().Remove( m_REFPnt1, false );
			}
			if( m_REFPnt2 != null && !m_REFPnt2.IsNull() ) {
				m_Viewer.GetAISContext().Remove( m_REFPnt2, false );
			}
			if( m_REFPnt3 != null && !m_REFPnt3.IsNull() ) {
				m_Viewer.GetAISContext().Remove( m_REFPnt3, false );
			}

			// remove text label
			if( m_REFText1 != null && !m_REFText1.IsNull() ) {
				m_Viewer.GetAISContext().Remove( m_REFText1, false );
			}
			if( m_REFText2 != null && !m_REFText2.IsNull() ) {
				m_Viewer.GetAISContext().Remove( m_REFText2, false );
			}
			if( m_REFText3 != null && !m_REFText3.IsNull() ) {
				m_Viewer.GetAISContext().Remove( m_REFText3, false );
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		public int GetSelectIndex()
		{
			var viewerContext = m_Viewer.GetAISContext();
			viewerContext.InitSelected();
			if( !viewerContext.MoreSelected() ) {
				return 0;
			}

			// get select vertex
			TopoDS_Shape selectedShape = viewerContext.SelectedShape();
			bool isGetREFPntSuccess = DataGettingHelper.GetREFPnt( out CalibrationData calibrationData );
			if( !isGetREFPntSuccess || calibrationData == null ) {
				return 0;
			}

			if( calibrationData.Ref_Pnt1 != null && m_REFPnt1 != null && !m_REFPnt1.IsNull() && selectedShape.IsSame( m_REFPnt1.Shape() ) ) {
				return 1;
			}
			if( calibrationData.Ref_Pnt2 != null && m_REFPnt2 != null && !m_REFPnt2.IsNull() && selectedShape.IsSame( m_REFPnt2.Shape() ) ) {
				return 2;
			}
			if( calibrationData.Ref_Pnt3 != null && m_REFPnt3 != null && !m_REFPnt3.IsNull() && selectedShape.IsSame( m_REFPnt3.Shape() ) ) {
				return 3;
			}
			return 0;
		}

		const Quantity_NameOfColor PNT_Color = Quantity_NameOfColor.Quantity_NOC_RED;

		AIS_Shape DrawPntOnViewer( gp_Pnt point, Quantity_NameOfColor color )
		{
			if( point == null ) {
				return new AIS_Shape( new TopoDS_Vertex() );
			}
			BRepBuilderAPI_MakeVertex mkVertex = new BRepBuilderAPI_MakeVertex( point );
			TopoDS_Vertex vertex = mkVertex.Vertex();
			AIS_Shape aisShape = ViewHelper.CreateFeatureAIS( vertex, color );
			m_Viewer.GetAISContext().Display( aisShape, false );
			return aisShape;
		}

		AIS_TextLabel WriteTextOnViewer( gp_Pnt point, int nPntIdx )
		{
			// avoid text label overlap with point
			const int nLocationOffset = 1;
			const Quantity_NameOfColor PNT_Color = Quantity_NameOfColor.Quantity_NOC_RED;
			if( point == null ) {
				return new AIS_TextLabel();
			}

			// create text label ais
			AIS_TextLabel textLabel = new AIS_TextLabel();
			textLabel.SetText( new TCollection_ExtendedString( "P" + nPntIdx ) );
			gp_Pnt textLocation = new gp_Pnt( point.x + nLocationOffset, point.y + nLocationOffset, point.z + nLocationOffset );
			textLabel.SetPosition( textLocation );
			textLabel.SetColor( new Quantity_Color( PNT_Color ) );
			textLabel.SetHeight( 20 );
			textLabel.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
			m_Viewer.GetAISContext().Display( textLabel, false );
			m_Viewer.GetAISContext().Deactivate( textLabel );
			return textLabel;
		}
	}
}
