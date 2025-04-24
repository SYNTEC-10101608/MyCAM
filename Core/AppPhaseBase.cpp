#include "AppPhaseBase.h"

using namespace Core;

AppPhaseBase::AppPhaseBase( std::shared_ptr<MyViewer> pViewer )
	: m_pViewer( pViewer )
	, m_nXMousePosition( 0 )
	, m_nYMousePosition( 0 )
{
	// Constructor implementation
}

void AppPhaseBase::Enter()
{
	// Initialize the phase
}

void AppPhaseBase::Exit()
{
	// Clean up the phase
}

void AppPhaseBase::MouseDown( int button, int x, int y )
{
	switch( button ) {

		// press down middle button, then start translate the viewer
	case MOUSE_MID: // mid button
		m_nXMousePosition = x;
		m_nYMousePosition = y;
		break;

		// press down right button, then start rotatae the viewer
	case MOUSE_RIGHT: // right button
		m_pViewer->StartRotation( x, y );
		break;
	default:
		break;
	}
}

void AppPhaseBase::MouseMove( int button, int x, int y )
{
	if( m_pViewer == nullptr ) {
		return;
	}
	m_pViewer->MoveTo( x, y );
	switch( button ) {

		// translate the viewer
	case MOUSE_MID: // mid button
		m_pViewer->Pan( x - m_nXMousePosition, m_nYMousePosition - y );
		m_nXMousePosition = x;
		m_nYMousePosition = y;
		break;

		// rotate the viewer
	case MOUSE_RIGHT: // right button
		m_pViewer->Rotation( x, y );
		break;
	default:
		break;
	}
}

void AppPhaseBase::MouseWheel( int delta, int x, int y )
{
	// zoom viewer at start point
	m_pViewer->StartZoomAtPoint( x, y );

	int endX = (int)(x + x * delta * ZOOM_Ratio);
	int endY = (int)(y + y * delta * ZOOM_Ratio);

	// zoom viewer with mouse wheel delta and scaling ratio
	m_pViewer->ZoomAtPoint( x, y, endX, endY );
}

void AppPhaseBase::KeyDown( int key, int x, int y )
{
	// Handle key down event
}
