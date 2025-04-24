#pragma once

#include <TopoDS_Shape.hxx>
#include <TopoDS_Face.hxx>
#include <functional>
#include <vector>
#include "MyViewer.h"
#include "CADData.h"
#include "AppPhaseBase.h"

using namespace Core::DataStructure;

namespace Core
{
	class ExtractPattern : public AppPhaseBase
	{
	public:
		using ExtractOK = std::function<void( const TopoDS_Shape &, const std::vector<CADData> & )>;

		ExtractPattern( const TopoDS_Shape &partShape, std::shared_ptr<MyViewer> pViewer );
		void OnExtractOK();

		// override mousedown event
		void MouseDown( int button, int x, int y ) override;

		// override keydown event
		void KeyDown( int key ) override;

	private:
		TopoDS_Shape m_partShape;
		std::shared_ptr<MyViewer> m_pViewer;
		ExtractOK m_callback;

		void SetupViewerStyle();
		void ShowPart();
		std::vector<TopoDS_Face> GetSelectedFaces();
		std::vector<CADData> BuildCADData( const std::vector<TopoDS_Face> &faces );
		std::vector<TopoDS_Wire> GetAllCADContour( const std::vector<TopoDS_Face> &faces, TopoDS_Shape &sewResult );
	};
}
