#pragma once

#include "CoreCommon.h"

// for OCC graphic
#include <Aspect_DisplayConnection.hxx>
#include <WNT_Window.hxx>

// for object display
#include <V3d_Viewer.hxx>
#include <V3d_View.hxx>
#include <AIS_InteractiveContext.hxx>
#include <OpenGl_GraphicDriver.hxx>


namespace Core {
	class CORE_API MyViewer {
	public:
		bool InitViewer(Handle(WNT_Window) theWnd);

	private:
		// fields
		Handle(V3d_Viewer) myViewer;
		Handle(V3d_View) myView;
		Handle(AIS_InteractiveContext) myAISContext;
		Handle(OpenGl_GraphicDriver) myGraphicDriver;
	};
}
