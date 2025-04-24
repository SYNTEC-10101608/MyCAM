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
		Import( std::shared_ptr<MyViewer> pViewer );
		bool ImportFile( const Standard_CString filename, int format );
		const TopoDS_Shape &GetImportedShape() const;

	private:

		// imported shape
		TopoDS_Shape m_ImportedShape;

		// method
		void ShowPart() const;
	};
}
