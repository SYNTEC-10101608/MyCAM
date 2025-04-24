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


namespace Core
{
	class CORE_API MyViewer
	{
	public:
		bool InitViewer( const Handle( WNT_Window ) &theWnd );
		void RedrawView();
		void UpdateView();
		void Zoom( int theX1, int theY1, int theX2, int theY2 );
		void ZoomAtPoint( int theX1, int theY1, int theX2, int theY2 );
		void StartZoomAtPoint( int theX, int theY );
		void Pan( int theX, int theY );
		void Rotation( int theX, int theY );
		void StartRotation( int theX, int theY );
		void ZoomAllView();
		void MoveTo( int theX, int theY );
		void UpdateCurrentViewer();
		const Handle( AIS_InteractiveContext ) &GetAISContext();
		void AxoView();
		void ShiftSelect();

	private:
		// fields
		Handle( V3d_Viewer ) m_Viewer;
		Handle( V3d_View ) m_View;
		Handle( AIS_InteractiveContext ) m_AISContext;
		Handle( OpenGl_GraphicDriver ) m_GraphicDriver;
	};
}
