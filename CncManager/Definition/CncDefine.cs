namespace CncManager.Definition
{
	// CNC event type for MyCAM log
	public enum ERecconectStage
	{
		Reconnecting,
		Reconnected,
		ReconnectFailed
	}

	// State of the CNC connection managed by CncClient
	public enum CncConnectionState
	{
		Disconnected,
		Connecting,
		Connected,
		Reconnecting,
		Error
	}

	// CNC status mapped from Syntec.OpenCNC ECncStatus
	public enum CncStatus
	{
		NOTREADY,
		READY,
		START,
		FEEDHOLD,
		STOP,
		EXECUTE,
		RECORD,
	}

	// Behavior parameters and handshake constants for CNC connection.
	public static class CncConstants
	{
		#region Connection defaults

		// Default CNC IP address
		public const string DEFAULT_CNC_IP = "192.168.1.200";

		// Default connect timeout (ms)
		public const int DEFAULT_CONNECT_TIMEOUT_MS = 3000;

		// Extra margin for the timeout guard so the API-level timeout fires first
		public const int GUARD_TIMEOUT_MARGIN_MS = 1000;

		// Default API guard timeout (ms) for write/critical operations
		public const int DEFAULT_API_TIMEOUT_MS = 3000;

		#endregion

		#region Heartbeat (keep-alive)

		// Connection checker: heartbeat interval (ms)
		public const int CHECKER_INTERVAL_MS = 200;

		// Connection checker: consider lost after this many consecutive failures, 200ms x 5 = 1000ms (1 second)
		public const int CHECKER_FAIL_THRESHOLD = 5;

		// Max wait time for a background task to exit after cancellation (ms)
		public const int TASK_SHUTDOWN_WAIT_MS = 2000;

		#endregion

		#region Auto-reconnect

		// Wait between reconnect attempts (ms)
		public const int RECONNECT_INTERVAL_MS = 1000;

		// Give up after this many attempts
		public const int RECONNECT_MAX_ATTEMPTS = 5;

		#endregion

		#region R-register handshake

		// Handshake R register shared by PC and CNC
		public const int R_CNC_AND_PC_HANDSHAKE = 17564;

		// Value PC writes to R17564 (both at startup and after each successful check)
		public const int HANDSHAKE_PC_WRITE_VALUE = 5;

		// Expected echo value CNC writes to R17564
		public const int HANDSHAKE_CNC_ECHO_VALUE = 4;

		#endregion

		#region Write verification

		// PLC scanning time for polling R value after write (ms)
		public const int PLC_SCANNING_TIME_MS = 10;

		// Max poll count when checking R write done
		public const int CHECK_COUNT_MAX = 300;

		#endregion
	}
}
