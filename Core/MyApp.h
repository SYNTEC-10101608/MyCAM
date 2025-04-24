#pragma once

// OCCT headers
#include <TopoDS_Shape.hxx>
#include <AIS_Shape.hxx>
#include <Graphic3d_MaterialAspect.hxx>

#include "CoreCommon.h"
#include "MyViewer.h"
#include "Import.h"

using namespace Core;

class CORE_API MyApp
{
public:
	MyApp()
		: m_pViewer( std::make_unique<MyViewer>() )
		, m_pAppPhase( std::make_unique<AppPhaseBase>( m_pViewer ) )
		, m_PartShape()
	{
	}

	bool InitViewer( const Handle( WNT_Window ) theWnd )
	{
		return m_pViewer->InitViewer( theWnd );
	}

	bool ImportFile( const Standard_CString filename, int format )
	{
		std::shared_ptr<Import> pImport = std::make_shared<Import>( m_pViewer );
		m_pAppPhase = pImport;
		bool isImportSucess = pImport->ImportFile( filename, format );
		if( !isImportSucess || pImport->GetImportedShape().IsNull() ) {
			return false;
		}
	}

	// viewer
	void MouseDown( int button, int x, int y ) {
		if( m_pAppPhase == nullptr ) {
			return;
		}
		m_pAppPhase->MouseDown( button, x, y );
	}

	void MouseMove( int button, int x, int y ) {
		if( m_pAppPhase == nullptr ) {
			return;
		}
		m_pAppPhase->MouseMove( button, x, y );
	}

	void MouseWheel( int delta, int x, int y ) {
		if( m_pAppPhase == nullptr ) {
			return;
		}
		m_pAppPhase->MouseWheel( delta, x, y );
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
	// private methods
	// import file
	void ShowPart()
	{
		// create AIS shape
		Handle( AIS_Shape ) aisShape = new AIS_Shape( m_PartShape );
		Graphic3d_MaterialAspect aspect( Graphic3d_NameOfMaterial::Graphic3d_NOM_STEEL );
		aisShape->SetMaterial( aspect );
		aisShape->SetDisplayMode( 1 );

		// display the shape
		m_pViewer->GetAISContext()->Display( aisShape, false );
		m_pViewer->GetAISContext()->UpdateCurrentViewer();
		m_pViewer->ZoomAllView();
	}

private:
	// privaye fields
	// viewer
	std::shared_ptr<MyViewer> m_pViewer;

	// AppPhas
	std::shared_ptr<AppPhaseBase> m_pAppPhase;

	// part
	TopoDS_Shape m_PartShape;
};
