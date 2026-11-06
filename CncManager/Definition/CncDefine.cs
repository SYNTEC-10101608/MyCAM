using System;

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

	// Jog step values written to R6046.
	public enum JogStep
	{
		Step5 = 5,
		Step10 = 10,
		Step50 = 50,
		Step100 = 100,
		Step200 = 200
	}

	// Controller action codes written to R18784.
	public enum PollingAction
	{
		Start = 4,
		Air = 13,
		NormalCalibration = 21,
		FeedHold = 61,
		Continue = 62,
		ResumeCutting = 69,
		HomeProgram = 101,
		HomeMachine = 102,
		Laser = 103,
		FollowOn = 104,
		FollowOff = 105,
		Focus = 109,
		SafePosition = 133,
		ShowCNCMainScreen = 503,
		ResumeCutParameters = 504,
		ProcessSettings = 505,
		CalculateCalibration = 506
	}

	// Rotary axis mounted on the head or table.
	public enum CncAxisRole
	{
		Head,
		Table
	}

	public sealed class CncAxis
	{
		public CncAxis( string name, int id, CncAxisRole? role = null )
		{
			if( id < 1 || id > CncConstants.COORDINATE_AXIS_COUNT ) {
				throw new ArgumentOutOfRangeException( nameof( id ), "Invalid CNC axis ID." );
			}
			Name = name;
			ID = id;
			Role = role;
		}

		// Display name: X/Y/Z or the configured rotary A/B/C.
		public string Name { get; }
		// 1-based coordinate axis index, independent of the display name.
		public int ID { get; }
		// Rotary mounting role; null for linear axes.
		public CncAxisRole? Role { get; }

		public static string GetRotaryName( int parameterIndex, int value )
		{
			switch( value ) {
				case 400: return "A";
				case 500: return "B";
				case 600: return "C";
				default: throw new ArgumentOutOfRangeException( nameof( value ), CncMessages.InvalidRotaryParameter( parameterIndex, value ) );
			}
		}
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

		#region Resource polling

		public const int RESOURCE_POLL_INTERVAL_MS = 200;

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

		#region Machining commands

		public const int R_SignalLock = 18783;
		public const int R_PollingAction = 18784;
		public const int R_STOP_REQUEST = 17541;
		public const int STOP_REQUEST = 1;
		public const int R_CONTINUE_BLOCKED = 18933;
		public const int R_RESUME_CUTTING_BLOCKED_1 = 18931;
		public const int R_RESUME_CUTTING_BLOCKED_2 = 18932;
		public const int K_CNC_STATUS = 4;
		public const int FIRST_COORD_ID = 1;
		public const int SYSTEM_VAR_1820 = 1820;

		#endregion

		#region Jog commands

		public const int R_JOG_COMMAND = 18785;
		public const int R_JOG_MODE = 18786;
		public const int R_JOG_STEP = 6046;

		#endregion

		#region Panel commands

		public const int R_OuterTiggerSource = 17543;
		public const int R_CNC_MODE = 13;
		public const int HANDWHEEL_MODE = 2;
		public const int R_HANDWHEEL_SIMULATION = 17575;
		public const int SYSTEM_VAR_ENDPOINT = 1014;
		public const int ENDPOINT_ACTIVE = 43004;
		public const int R_FEED_OVERRIDE = 6047;
		public const int R_SHUTTER = 17462;
		public const int R_AIR_STATE = 18852;
		public const int R_FOLLOW_STATE = 17605;
		public const int R_THREE_POINT_CALIBRATION = 6173;
		public const int GLOBAL_VAR_LASER_STATE = 77071;
		public const int GLOBAL_VAR_CALIBRATION_TOLERANCE = 78406;
		public const int GLOBAL_VAR_CALIBRATION_ERROR = 78407;
		public const int GLOBAL_VAR_CALIBRATION_RESULT = 78408;
		public const int GLOBAL_VAR_DRAWING_POINT_BASE = 78410;
		public const int GLOBAL_VAR_WORKPIECE_POINT_BASE = 78420;
		public const int CALIBRATION_TIMEOUT_MS = 5000;

		#endregion

		#region Machining monitor

		public const int PR_ROTARY_AXIS_1_NAME = 324;
		public const int PR_ROTARY_AXIS_2_NAME = 325;
		public const int PR_MACHINE_TYPE = 3001;
		public const int PR_HEAD_AXIS = 3005;
		public const int COORDINATE_AXIS_COUNT = 6;
		public const int SYSTEM_VAR_MACHINE_BASE = 1320;
		public const int Q_VAR_MACHINE_BASE = 120;
		public const int SYSTEM_VAR_PROGRAM_BASE = 1340;
		public const int SYSTEM_VAR_PROGRAM_OFFSET_BASE = 20020;
		public const int R_CAPACITANCE = 17600;
		public const int R_HEIGHT_AXIS_POSITION = 17614;
		public const int R_FOLLOW_HEIGHT = 17602;
		public const int R_ACTUAL_HEIGHT = 17603;
		public const int R_FEED_SPEED = 700;
		public const int R_FOCUS_POSITION = 17755;
		public const int GLOBAL_VAR_POWER = 77072;
		public const int GLOBAL_VAR_FREQUENCY = 77074;
		public const int GLOBAL_VAR_DUTY_CYCLE = 77073;
		public const int GLOBAL_VAR_MACHINING_HOURS = 77922;
		public const int GLOBAL_VAR_MACHINING_MINUTES = 77923;
		public const int GLOBAL_VAR_MACHINING_SECONDS = 77924;

		#endregion

		#region Write verification

		// PLC scanning time for polling R value after write (ms)
		public const int PLC_SCANNING_TIME_MS = 10;

		// Max poll count when checking R write done
		public const int CHECK_COUNT_MAX = 300;

		#endregion
	}
}
