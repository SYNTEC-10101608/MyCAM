#pragma once

#include <memory>
#include "CoreCommon.h"
#include "MyViewer.h"
#include "IAppPhase.h"

namespace Core
{
	// mouse button
	constexpr int MOUSE_LEFT = 0x100000;
	constexpr int MOUSE_RIGHT = 0x200000;
	constexpr int MOUSE_MID = 0x400000;
	constexpr int KEY_ENTER = 0x0D;
	constexpr int KEY_ESC = 0x1B;

	class CORE_API AppPhaseBase : public IAppPhase
	{
	public:
		AppPhaseBase( std::shared_ptr<MyViewer> pViewer );
		//virtual ~AppPhaseBase() override = default;

		// IAppPhase interface
		void Enter() override;
		void Exit() override;

		// mouse events
		void MouseDown( int button, int x, int y ) override;
		void MouseMove( int button, int x, int y ) override;
		void MouseWheel( int delta, int x, int y ) override;

		// key events
		void KeyDown( int key ) override;

	protected:
		std::shared_ptr<MyViewer> m_pViewer;

	private:
		int m_nXMousePosition;
		int m_nYMousePosition;
		const double ZOOM_Ratio = 0.0002;
	};
}
