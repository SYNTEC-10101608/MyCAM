#include "MyApp.h"

#include <vcclr.h>

using namespace Core;

public ref class AppBridge
{
public:
	AppBridge()
	{
		myApp = new MyApp();
	}

	~AppBridge()
	{
		if( myApp != nullptr ) {
			delete myApp;
			myApp = nullptr;
		}
	}

	bool InitViewer( System::IntPtr theWnd )
	{
		// convert the IntPtr to a HWND
		Handle( WNT_Window ) aWNTWindow = new WNT_Window( reinterpret_cast< HWND > ( theWnd.ToPointer() ) );
		return myApp->InitViewer( aWNTWindow );
	}

	bool ImportFile( System::String ^_filePath, int format )
	{
		// convert the managed string to a native string
		TCollection_AsciiString s = toAsciiString( _filePath );
		return myApp->ImportFile( s.ToCString(), format );
	}

	// viewer
	void RedrawView()
	{
		myApp->RedrawView();
	}

	void UpdateView()
	{
		myApp->UpdateView();
	}

	void Zoom( int theX1, int theY1, int theX2, int theY2 )
	{
		myApp->Zoom( theX1, theY1, theX2, theY2 );
	}

	void ZoomAtPoint( int theX1, int theY1, int theX2, int theY2 )
	{

	}

	void StartZoomAtPoint( int theX, int theY )
	{
		myApp->StartZoomAtPoint( theX, theY );
	}

	void Pan( int theX, int theY )
	{
		myApp->Pan( theX, theY );
	}

	void Rotation( int theX, int theY )
	{
		myApp->Rotation( theX, theY );
	}

	void StartRotation( int theX, int theY )
	{
		myApp->StartRotation( theX, theY );
	}

	void ZoomAllView()
	{
		myApp->ZoomAllView();
	}

	void MoveTo( int theX, int theY )
	{
		myApp->MoveTo( theX, theY );
	}

	void UpdateCurrentViewer()
	{
		myApp->UpdateCurrentViewer();
	}

private:
	TCollection_AsciiString toAsciiString( System::String ^theString )
	{
		if( theString == nullptr ) {
			return TCollection_AsciiString();
		}
		pin_ptr<const wchar_t> aPinChars = PtrToStringChars( theString );
		const wchar_t *aWCharPtr = aPinChars;
		if( aWCharPtr == NULL
			|| *aWCharPtr == L'\0' ) {
			return TCollection_AsciiString();
		}
		return TCollection_AsciiString( aWCharPtr );
	}

	MyApp *myApp;
};
