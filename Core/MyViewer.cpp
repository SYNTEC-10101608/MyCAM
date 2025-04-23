#include "MyViewer.h"

using namespace Core;

bool MyViewer::InitViewer( const Handle( WNT_Window ) &theWnd )
{
	try {
		Handle( Aspect_DisplayConnection ) aDisplayConnection;
		m_GraphicDriver = new OpenGl_GraphicDriver( aDisplayConnection );
	}
	catch( Standard_Failure ) {
		return false;
	}
	m_Viewer = new V3d_Viewer( m_GraphicDriver );
	m_Viewer->SetDefaultLights();
	m_Viewer->SetLightOn();
	m_View = m_Viewer->CreateView();
	m_View->SetWindow( theWnd );
	if( !theWnd->IsMapped() ) {
		theWnd->Map();
	}
	m_View->SetBackgroundColor( Quantity_Color( 0.0, 0.0, 0.0, Quantity_TOC_RGB ) );
	m_AISContext = new AIS_InteractiveContext( m_Viewer );
	m_AISContext->UpdateCurrentViewer();
	m_View->Redraw();
	m_View->MustBeResized();
	return true;
}

void MyViewer::RedrawView()
{
	if( !m_View.IsNull() ) {
		m_View->Redraw();
	}
}

void MyViewer::UpdateView()
{
	if( !m_View.IsNull() ) {
		m_View->Update();
	}
}

void MyViewer::Zoom( int theX1, int theY1, int theX2, int theY2 )
{
	if( !m_View.IsNull() ) {
		m_View->Zoom( theX1, theY1, theX2, theY2 );
	}
}

void MyViewer::ZoomAtPoint( int theX1, int theY1, int theX2, int theY2 )
{
	if( !m_View.IsNull() ) {
		m_View->ZoomAtPoint( theX1, theY1, theX2, theY2 );
	}
}

void MyViewer::StartZoomAtPoint( int theX, int theY )
{
	if( !m_View.IsNull() ) {
		m_View->StartZoomAtPoint( theX, theY );
	}
}

void MyViewer::Pan( int theX, int theY )
{
	if( !m_View.IsNull() ) {
		m_View->Pan( theX, theY );
	}
}

void MyViewer::Rotation( int theX, int theY )
{
	if( !m_View.IsNull() ) {
		m_View->Rotation( theX, theY );
	}
}

void MyViewer::StartRotation( int theX, int theY )
{
	if( !m_View.IsNull() ) {
		m_View->StartRotation( theX, theY );
	}
}

void MyViewer::ZoomAllView()
{
	if( !m_View.IsNull() ) {
		m_View->FitAll();
		m_View->ZFitAll();
	}
}

void MyViewer::MoveTo( int theX, int theY )
{
	if( !m_AISContext.IsNull() ) {
		m_AISContext->MoveTo( theX, theY, m_View, true );
	}
}

void MyViewer::UpdateCurrentViewer()
{
	if( !m_AISContext.IsNull() ) {
		m_AISContext->UpdateCurrentViewer();
	}
}

const Handle( AIS_InteractiveContext ) &MyViewer::GetAISContext()
{
	return m_AISContext;
}
