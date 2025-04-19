#pragma once

#include "CoreCommon.h"
#include "MyViewer.h"
#include "Import.h"

using namespace Core;

class CORE_API MyApp
{
public:
	bool InitViewer(Handle(WNT_Window) theWnd)
	{
		return myViewer.InitViewer(theWnd);
	}

	bool ImportFile(const Standard_CString filename, int format)
	{
		return import.ImportFile(filename, format);
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

private:
	MyViewer myViewer;
	Import import;
};
