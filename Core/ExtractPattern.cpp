// OCCT headers
#include <TopoDS_Wire.hxx>
#include <AIS_Shape.hxx>
#include <Graphic3d_MaterialAspect.hxx>
#include <Prs3d_Drawer.hxx>
#include <Quantity_Color.hxx>
#include <TopTools_IndexedDataMapOfShapeListOfShape.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <ShapeAnalysis_FreeBounds.hxx>
#include <TopoDS.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopTools_ListOfShape.hxx>

#include "CADData.h"
#include "MyViewer.h"
#include "ShapeTool.h"
#include "ExtractPattern.h"

using namespace Core;
using namespace Core::Tool;

ExtractPattern::ExtractPattern( std::shared_ptr<MyViewer> pViewer )
	: AppPhaseBase( pViewer )
	, m_partShape()
	, m_pViewer( pViewer )
	, m_callback( nullptr )
{
}

AppPhaseType ExtractPattern::GetType() const
{
	return AppPhaseType::ExtractPattern;
}

void ExtractPattern::Init( const TopoDS_Shape &partShape )
{
	m_partShape = partShape;
	SetupViewerStyle();
	ShowPart();
}

void ExtractPattern::SetExtractOKCallback( const ExtractOK &callback )
{
	m_callback = callback;
}

void ExtractPattern::MouseDown( int button, int x, int y )
{
	if( m_pViewer == nullptr ) {
		return;
	}
	if( button != MOUSE_LEFT ) {

		// call base class method
		AppPhaseBase::MouseDown( button, x, y );
		return;
	}
	m_pViewer->ShiftSelect();
}

void ExtractPattern::KeyDown( int key )
{
	if( m_pViewer == nullptr ) {
		return;
	}
	if( key == KEY_ENTER ) {
		OnExtractOK();
	}
}

void ExtractPattern::OnExtractOK()
{
	auto selectedFaces = GetSelectedFaces();
	if( selectedFaces.empty() ) {
		return;
	}
	auto cadDataList = BuildCADData( selectedFaces );
	if( cadDataList.empty() ) {
		return;
	}
	if( m_callback ) {
		m_callback( m_partShape, cadDataList );
	}
}

void ExtractPattern::SetupViewerStyle()
{
	Handle( AIS_InteractiveContext ) ctx = m_pViewer->GetAISContext();
	Handle( Prs3d_Drawer ) d = ctx->HighlightStyle( Prs3d_TypeOfHighlight_LocalSelected );
	d->SetColor( Quantity_Color( Quantity_NOC_RED ) );
	d->SetTransparency( 0.5f );
	d->SetDisplayMode( AIS_Shaded );

	Handle( Prs3d_Drawer ) d1 = ctx->HighlightStyle( Prs3d_TypeOfHighlight_Selected );
	d1->SetColor( Quantity_Color( Quantity_NOC_RED ) );
	d1->SetTransparency( 0.5f );
	d1->SetDisplayMode( AIS_Shaded );
}

void ExtractPattern::ShowPart()
{
	Handle( AIS_Shape ) aisShape = new AIS_Shape( m_partShape );
	aisShape->SetMaterial( Graphic3d_MaterialAspect( Graphic3d_NOM_STEEL ) );
	aisShape->SetDisplayMode( AIS_Shaded );

	Handle( AIS_InteractiveContext ) ctx = m_pViewer->GetAISContext();
	ctx->RemoveAll( false );
	ctx->Display( aisShape, false );
	ctx->UpdateCurrentViewer();
	m_pViewer->AxoView();
	m_pViewer->ZoomAllView();

	ctx->Deactivate();
	ctx->Activate( AIS_Shape::SelectionMode( TopAbs_FACE ) );
}

std::vector<TopoDS_Face> ExtractPattern::GetSelectedFaces()
{
	std::vector<TopoDS_Face> faces;
	Handle( AIS_InteractiveContext ) ctx = m_pViewer->GetAISContext();
	ctx->InitSelected();
	while( ctx->MoreSelected() ) {
		TopoDS_Shape shape = ctx->SelectedShape();
		if( shape.ShapeType() == TopAbs_FACE ) {
			faces.push_back( TopoDS::Face( shape ) );
		}
		ctx->NextSelected();
	}
	return faces;
}

std::vector<CADData> ExtractPattern::BuildCADData( const std::vector<TopoDS_Face> &faces )
{
	std::vector<CADData> dataList;
	TopoDS_Shape sewResult;
	auto wires = GetAllCADContour( faces, sewResult );
	if( wires.empty() ) return dataList;

	TopTools_IndexedDataMapOfShapeListOfShape shellMap, solidMap;
	TopExp::MapShapesAndAncestors( sewResult, TopAbs_EDGE, TopAbs_FACE, shellMap );
	TopExp::MapShapesAndAncestors( m_partShape, TopAbs_EDGE, TopAbs_FACE, solidMap );

	for( const auto &wire : wires ) {
		TopTools_IndexedDataMapOfShapeListOfShape oneShellMap, oneSolidMap;
		for( TopExp_Explorer edgeExp( wire, TopAbs_EDGE ); edgeExp.More(); edgeExp.Next() ) {
			TopoDS_Shape edge = edgeExp.Current();
			if( shellMap.Contains( edge ) && solidMap.Contains( edge ) ) {
				oneShellMap.Add( edge, shellMap.FindFromKey( edge ) );
				oneSolidMap.Add( edge, solidMap.FindFromKey( edge ) );
			}
		}
		dataList.emplace_back( wire, oneShellMap, oneSolidMap );
	}
	return dataList;
}

std::vector<TopoDS_Wire> ExtractPattern::GetAllCADContour( const std::vector<TopoDS_Face> &faces, TopoDS_Shape &sewResult )
{
	std::vector<TopoDS_Wire> wires;
	std::vector<TopoDS_Shape> faceShapes( faces.begin(), faces.end() );
	sewResult = ShapeTool::SewShape( faceShapes );

	std::vector<TopoDS_Shape> faceGroups;
	if( sewResult.ShapeType() == TopAbs_SHELL || sewResult.ShapeType() == TopAbs_FACE ) {
		faceGroups.push_back( sewResult );
	}
	else {
		for( TopExp_Explorer exp( sewResult, TopAbs_SHAPE ); exp.More(); exp.Next() ) {
			faceGroups.push_back( exp.Current() );
		}
	}

	for( const auto &group : faceGroups ) {
		ShapeAnalysis_FreeBounds bounds( group );
		for( TopExp_Explorer wireExp( bounds.GetClosedWires(), TopAbs_WIRE ); wireExp.More(); wireExp.Next() ) {
			wires.push_back( TopoDS::Wire( wireExp.Current() ) );
		}
	}
	return wires;
}
