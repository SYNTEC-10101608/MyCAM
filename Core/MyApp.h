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
	bool InitViewer( Handle( WNT_Window ) theWnd )
	{
		return myViewer.InitViewer( theWnd );
	}

	bool ImportFile( const Standard_CString filename, int format )
	{
		Import import;
		bool isImportSucess = import.ImportFile( filename, format );
		if( !isImportSucess || import.GetImportedShape().IsNull() ) {
			return false;
		}
		ShowPart( import.GetImportedShape() );
	}

	// viewer
	void RedrawView()
	{
		myViewer.RedrawView();
	}

	void UpdateView()
	{
		myViewer.UpdateView();
	}

	void Zoom( int theX1, int theY1, int theX2, int theY2 )
	{
		myViewer.Zoom( theX1, theY1, theX2, theY2 );
	}

	void ZoomAtPoint( int theX1, int theY1, int theX2, int theY2 )
	{
		myViewer.ZoomAtPoint( theX1, theY1, theX2, theY2 );
	}

	void StartZoomAtPoint( int theX, int theY )
	{
		myViewer.StartZoomAtPoint( theX, theY );
	}

	void Pan( int theX, int theY )
	{
		myViewer.Pan( theX, theY );
	}

	void Rotation( int theX, int theY )
	{
		myViewer.Rotation( theX, theY );
	}

	void StartRotation( int theX, int theY )
	{
		myViewer.StartRotation( theX, theY );
	}

	void ZoomAllView()
	{
		myViewer.ZoomAllView();
	}

	void MoveTo( int theX, int theY )
	{
		myViewer.MoveTo( theX, theY );
	}

	void UpdateCurrentViewer()
	{
		myViewer.UpdateCurrentViewer();
	}

private:
	// show part
	void ShowPart( TopoDS_Shape partShape )
	{
		// create AIS shape
		Handle( AIS_Shape ) aisShape = new AIS_Shape( partShape );
		Graphic3d_MaterialAspect aspect( Graphic3d_NameOfMaterial::Graphic3d_NOM_STEEL );
		aisShape->SetMaterial( aspect );
		aisShape->SetDisplayMode( 1 );

		// display the shape
		myViewer.GetAISContext()->Display( aisShape, false );
		myViewer.GetAISContext()->UpdateCurrentViewer();
		myViewer.ZoomAllView();
	}

	// fields
	MyViewer myViewer;
};
