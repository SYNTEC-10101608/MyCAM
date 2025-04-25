// OCCT headers
#include <TopoDS_Shape.hxx>
#include <BRep_Builder.hxx>
#include <BRepTools.hxx>
#include <STEPControl_Reader.hxx>
#include <IGESControl_Reader.hxx>
#include <XSControl_Reader.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include<AIS_Shape.hxx>

// c++ headers
#include <vector>
#include <string>

// project headers
#include "ShapeTool.h"
#include "Import.h"

using namespace Core::Tool;
using namespace Core;

Import::Import( std::shared_ptr<MyViewer> pViewer )
	: AppPhaseBase( pViewer )
	, m_Callback( nullptr )
	, m_ImportedShape()
{
}

AppPhaseType Import::GetType() const
{
	return AppPhaseType::Import;
}

void Import::SetImportOKCallback( const ImportOK &callback )
{
	m_Callback = callback;
}

bool Import::ImportFile( const Standard_CString filename, int format )
{
	std::unique_ptr<XSControl_Reader> reader;
	switch( format ) {
	case 1:
		reader = std::make_unique<STEPControl_Reader>();
		break;
	case 2:
		reader = std::make_unique<IGESControl_Reader>();
		break;
	default:
		reader = std::make_unique<XSControl_Reader>();
		break;
	}
	IFSelect_ReturnStatus status = reader->ReadFile( filename );

	// check the status
	if( status != IFSelect_RetDone ) {
		return false;
	}
	reader->TransferRoots();

	// prevent from empty shape or null shape
	if( reader->NbShapes() == 0 ) {
		return false;
	}
	if( reader->OneShape().IsNull() ) {
		return false;
	}

	// sew the shape
	std::vector<TopoDS_Shape> shapes = { reader->OneShape() };
	m_ImportedShape = ShapeTool::SewShape( shapes );
	ShowPart();
	return true;
}

void Import::KeyDown( int key )
{
	if( m_ImportedShape.IsNull() ) {
		return;
	}
	if( key == KEY_ENTER ) {
		OnImportOK();
	}
}

void Import::OnImportOK()
{
	if( m_Callback ) {
		m_Callback( m_ImportedShape );
	}
}

void Import::ShowPart()
{
	// create AIS shape
	Handle( AIS_Shape ) aisShape = new AIS_Shape( m_ImportedShape );
	Graphic3d_MaterialAspect aspect( Graphic3d_NameOfMaterial::Graphic3d_NOM_STEEL );
	aisShape->SetMaterial( aspect );
	aisShape->SetDisplayMode( 1 );

	// display the shape
	Handle( AIS_InteractiveContext ) ctx = m_pViewer->GetAISContext();
	ctx->RemoveAll( false );
	ctx->Display( aisShape, false );
	ctx->UpdateCurrentViewer();
	m_pViewer->ZoomAllView();
}
