#include "MyViewer.h"

using namespace Core;

bool MyViewer::InitViewer(Handle(WNT_Window) theWnd) {
	try
	{
		Handle(Aspect_DisplayConnection) aDisplayConnection;
		myGraphicDriver = new OpenGl_GraphicDriver(aDisplayConnection);
	}
	catch (Standard_Failure)
	{
		return false;
	}
	myViewer = new V3d_Viewer(myGraphicDriver);
	myViewer->SetDefaultLights();
	myViewer->SetLightOn();
	myView = myViewer->CreateView();
	myView->SetWindow(theWnd);
	if (!theWnd->IsMapped())
	{
		theWnd->Map();
	}
	myView->SetBackgroundColor(Quantity_Color(0.0, 0.0, 0.0, Quantity_TOC_RGB));
	myAISContext = new AIS_InteractiveContext(myViewer);
	myAISContext->UpdateCurrentViewer();
	myView->Redraw();
	myView->MustBeResized();
	return true;
}
