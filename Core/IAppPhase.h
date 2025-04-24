#pragma once

#include "CoreCommon.h"

namespace Core
{
	class CORE_API IAppPhase
	{
	public:
		//virtual ~IAppPhase() = default;

		virtual void Enter() = 0;
		virtual void Exit() = 0;

		// mouse events
		virtual void MouseDown( int button, int x, int y ) = 0;
		virtual void MouseMove( int button, int x, int y ) = 0;
		virtual void MouseWheel( int delta, int x, int y ) = 0;

		// key events
		virtual void KeyDown( int key, int x, int y ) = 0;
	};
}
