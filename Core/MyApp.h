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
		, m_PartShape()
	{
	}

	bool InitViewer( const Handle( WNT_Window ) theWnd )
	{
		return m_pViewer->InitViewer( theWnd );
	}

	bool ImportFile( const Standard_CString filename, int format )
	{
		Import import;
		bool isImportSucess = import.ImportFile( filename, format );
		if( !isImportSucess || import.GetImportedShape().IsNull() ) {
			return false;
		}
		m_PartShape = import.GetImportedShape();
		ShowPart();
	}

	// viewer
	void RedrawView()
	{
		m_pViewer->RedrawView();
	}

	void UpdateView()
	{
		m_pViewer->UpdateView();
	}

	void Zoom( int theX1, int theY1, int theX2, int theY2 )
	{
		m_pViewer->Zoom( theX1, theY1, theX2, theY2 );
	}

	void ZoomAtPoint( int theX1, int theY1, int theX2, int theY2 )
	{
		m_pViewer->ZoomAtPoint( theX1, theY1, theX2, theY2 );
	}

	void StartZoomAtPoint( int theX, int theY )
	{
		m_pViewer->StartZoomAtPoint( theX, theY );
	}

	void Pan( int theX, int theY )
	{
		m_pViewer->Pan( theX, theY );
	}

	void Rotation( int theX, int theY )
	{
		m_pViewer->Rotation( theX, theY );
	}

	void StartRotation( int theX, int theY )
	{
		m_pViewer->StartRotation( theX, theY );
	}

	void ZoomAllView()
	{
		m_pViewer->ZoomAllView();
	}

	void MoveTo( int theX, int theY )
	{
		m_pViewer->MoveTo( theX, theY );
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
	std::unique_ptr<MyViewer> m_pViewer;

	// part
	TopoDS_Shape m_PartShape;
};
