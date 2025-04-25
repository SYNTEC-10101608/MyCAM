#pragma once

#include "CoreCommon.h"
#include"MyViewer.h"
#include "AppPhaseBase.h"
#include <string>

namespace Core
{
	class CORE_API Import : public AppPhaseBase
	{
	public:
		using ImportOK = std::function<void( const TopoDS_Shape & )>;

		Import( std::shared_ptr<MyViewer> pViewer );
		AppPhaseType GetType() const override;
		void SetImportOKCallback( const ImportOK &callback );
		bool ImportFile( const Standard_CString filename, int format );

		// override keydown event
		void KeyDown( int key ) override;

	private:
		TopoDS_Shape m_ImportedShape;
		ImportOK m_Callback;

		void OnImportOK();
		void ShowPart();
	};
}
