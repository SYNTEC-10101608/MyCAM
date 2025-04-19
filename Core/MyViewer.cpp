#include "MyViewer.h"

using namespace Core;

bool MyViewer::InitViewer( Handle( WNT_Window ) theWnd )
{
	try {
		Handle( Aspect_DisplayConnection ) aDisplayConnection;
		myGraphicDriver = new OpenGl_GraphicDriver( aDisplayConnection );
	}
	catch( Standard_Failure ) {
		return false;
	}
	myViewer = new V3d_Viewer( myGraphicDriver );
	myViewer->SetDefaultLights();
	myViewer->SetLightOn();
	myView = myViewer->CreateView();
	myView->SetWindow( theWnd );
	if( !theWnd->IsMapped() ) {
		theWnd->Map();
	}
	myView->SetBackgroundColor( Quantity_Color( 0.0, 0.0, 0.0, Quantity_TOC_RGB ) );
	myAISContext = new AIS_InteractiveContext( myViewer );
	myAISContext->UpdateCurrentViewer();
	myView->Redraw();
	myView->MustBeResized();
	return true;
}

void MyViewer::RedrawView()
{
	if( !myView.IsNull() ) {
		myView->Redraw();
	}
}

void MyViewer::UpdateView()
{
	if( !myView.IsNull() ) {
		myView->Update();
	}
}

void MyViewer::Zoom( int theX1, int theY1, int theX2, int theY2 )
{
	if( !myView.IsNull() ) {
		myView->Zoom( theX1, theY1, theX2, theY2 );
	}
}

void MyViewer::ZoomAtPoint( int theX1, int theY1, int theX2, int theY2 )
{
	if( !myView.IsNull() ) {
		myView->ZoomAtPoint( theX1, theY1, theX2, theY2 );
	}
}

void MyViewer::StartZoomAtPoint( int theX, int theY )
{
	if( !myView.IsNull() ) {
		myView->StartZoomAtPoint( theX, theY );
	}
}

void MyViewer::Pan( int theX, int theY )
{
	if( !myView.IsNull() ) {
		myView->Pan( theX, theY );
	}
}

void MyViewer::Rotation( int theX, int theY )
{
	if( !myView.IsNull() ) {
		myView->Rotation( theX, theY );
	}
}

void MyViewer::StartRotation( int theX, int theY )
{
	if( !myView.IsNull() ) {
		myView->StartRotation( theX, theY );
	}
}

void MyViewer::ZoomAllView()
{
	if( !myView.IsNull() ) {
		myView->FitAll();
		myView->ZFitAll();
	}
}

void MyViewer::MoveTo( int theX, int theY )
{
	if( !myAISContext.IsNull() ) {
		myAISContext->MoveTo( theX, theY, myView, true );
	}
}

void MyViewer::UpdateCurrentViewer()
{
	if( !myAISContext.IsNull() ) {
		myAISContext->UpdateCurrentViewer();
	}
}

Handle( AIS_InteractiveContext ) MyViewer::GetAISContext()
{
	return myAISContext;
}
