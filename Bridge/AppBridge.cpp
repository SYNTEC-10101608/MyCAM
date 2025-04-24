#include "MyApp.h"

#include <vcclr.h>

using namespace Core;

public ref class AppBridge
{
public:
	AppBridge()
	{
		m_pApp = new MyApp();
	}

	~AppBridge()
	{
		if( m_pApp != nullptr ) {
			delete m_pApp;
			m_pApp = nullptr;
		}
	}

	bool InitViewer( System::IntPtr theWnd )
	{
		// convert the IntPtr to a HWND
		Handle( WNT_Window ) aWNTWindow = new WNT_Window( reinterpret_cast<HWND> (theWnd.ToPointer()) );
		return m_pApp->InitViewer( aWNTWindow );
	}

	bool ImportFile( System::String ^_filePath, int format )
	{
		// convert the managed string to a native string
		TCollection_AsciiString s = toAsciiString( _filePath );
		return m_pApp->ImportFile( s.ToCString(), format );
	}

	// viewer
	void MouseDown( int button, int x, int y )
	{
		m_pApp->MouseDown( button, x, y );
	}

	void MouseMove( int button, int x, int y )
	{
		m_pApp->MouseMove( button, x, y );
	}

	void MouseWheel( int delta, int x, int y )
	{
		m_pApp->MouseWheel( delta, x, y );
	}

	void RedrawView()
	{
		m_pApp->RedrawView();
	}

	void UpdateView()
	{
		m_pApp->UpdateView();
	}

	void ZoomAllView()
	{
		m_pApp->ZoomAllView();
	}

	void UpdateCurrentViewer()
	{
		m_pApp->UpdateCurrentViewer();
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

	MyApp *m_pApp;
};
