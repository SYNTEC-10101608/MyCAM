#include "MyViewer.h"
#include "Import.h"
#include "ShapeTool.h"

#include <vcclr.h>

using namespace Core;

public ref class AppBridge
{
public:
	bool InitViewer(System::IntPtr theWnd)
	{
		MyViewer myViewer;
		Handle(WNT_Window) aWNTWindow = new WNT_Window(reinterpret_cast<HWND> (theWnd.ToPointer()));
		return myViewer.InitViewer(aWNTWindow);
	}

	bool ImportFile(System::String^ _filePath, int format) {

		// Convert the managed string to a native string
		TCollection_AsciiString s = toAsciiString(_filePath);
		Import import;
		return import.ImportFile(s.ToCString(), format);
	}

private:
	TCollection_AsciiString toAsciiString(System::String^ theString)
	{
		if (theString == nullptr)
		{
			return TCollection_AsciiString();
		}
		pin_ptr<const wchar_t> aPinChars = PtrToStringChars(theString);
		const wchar_t* aWCharPtr = aPinChars;
		if (aWCharPtr == NULL
			|| *aWCharPtr == L'\0')
		{
			return TCollection_AsciiString();
		}
		return TCollection_AsciiString(aWCharPtr);
	}
};
