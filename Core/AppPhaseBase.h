#pragma once

#include <memory>
#include "CoreCommon.h"
#include "MyViewer.h"
#include "IAppPhase.h"

namespace Core
{
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
		void MouseWeel( int delta, int x, int y ) override;

		// key events
		void KeyDown( int key, int x, int y ) override;

	protected:
		std::shared_ptr<MyViewer> m_pViewer;

	private:
		int m_nXMousePosition;
		int m_nYMousePosition;
		const double ZOOM_Ratio = 0.0002;
	};
}
