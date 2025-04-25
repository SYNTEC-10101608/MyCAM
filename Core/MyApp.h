#pragma once

// OCCT headers
#include <TopoDS_Shape.hxx>
#include <AIS_Shape.hxx>
#include <Graphic3d_MaterialAspect.hxx>

#include "CoreCommon.h"
#include "MyViewer.h"
#include "Import.h"
#include "ExtractPattern.h"

using namespace Core;

class CORE_API MyApp
{
public:
	MyApp()
		: m_pViewer( std::make_unique<MyViewer>() )
		, m_pActiveAppPhase( std::make_unique<AppPhaseBase>( nullptr ) )
		, m_pImport( std::make_unique<Import>( m_pViewer ) )
		, m_pExtract( std::make_unique<ExtractPattern>( m_pViewer ) )
		, m_PartShape()
	{
		// the default phase is import
		m_pActiveAppPhase = m_pImport;
		m_pImport->SetImportOKCallback( std::bind( &MyApp::ImportOK, this, std::placeholders::_1 ) );
	}

	bool InitViewer( const Handle( WNT_Window ) theWnd )
	{
		return m_pViewer->InitViewer( theWnd );
	}

	bool ImportFile( const Standard_CString filename, int format )
	{
		return m_pImport->ImportFile( filename, format );
	}

	void ImportOK( const TopoDS_Shape &shape )
	{
		m_PartShape = shape;
		m_pActiveAppPhase = m_pExtract;
		m_pExtract->Init( m_PartShape );
		m_pExtract->SetExtractOKCallback( std::bind( &MyApp::ExtractPatternOK, this, std::placeholders::_1, std::placeholders::_2 ) );
	}

	void ExtractPatternOK( const TopoDS_Shape &partShape, const std::vector<CADData> &cadDataList )
	{
		// implement it later
	}

	// viewer
	void MouseDown( int button, int x, int y )
	{
		if( m_pActiveAppPhase != nullptr ) {
			m_pActiveAppPhase->MouseDown( button, x, y );
		}
	}

	void MouseMove( int button, int x, int y )
	{
		if( m_pActiveAppPhase != nullptr ) {
			m_pActiveAppPhase->MouseMove( button, x, y );
		}
	}

	void MouseWheel( int delta, int x, int y )
	{
		if( m_pActiveAppPhase != nullptr ) {
			m_pActiveAppPhase->MouseWheel( delta, x, y );
		}
	}

	void KeyDown( int key )
	{
		if( m_pActiveAppPhase != nullptr ) {
			m_pActiveAppPhase->KeyDown( key );
		}
	}

	void RedrawView()
	{
		m_pViewer->RedrawView();
	}

	void UpdateView()
	{
		m_pViewer->UpdateView();
	}

	void ZoomAllView()
	{
		m_pViewer->ZoomAllView();
	}

	void UpdateCurrentViewer()
	{
		m_pViewer->UpdateCurrentViewer();
	}

private:
	// viewer
	std::shared_ptr<MyViewer> m_pViewer;

	// AppPhas
	std::shared_ptr<AppPhaseBase> m_pActiveAppPhase;
	std::shared_ptr<Import> m_pImport;
	std::shared_ptr<ExtractPattern> m_pExtract;

	// part
	TopoDS_Shape m_PartShape;
};
