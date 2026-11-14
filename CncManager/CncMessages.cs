namespace CncManager
{
	internal static class CncMessages
	{
		static readonly LanguageManager.LanguageManager m_Language = new LanguageManager.LanguageManager( "CncMessages" );

		internal static string InvalidConnection => m_Language.TryGetString( "InvalidConnection", INVALID_CONNECTION );
		internal static string LifecycleBusy => m_Language.TryGetString( "LifecycleBusy", LIFECYCLE_BUSY );
		internal static string ConnectionCancelled => m_Language.TryGetString( "ConnectionCancelled", CONNECTION_CANCELLED );
		internal static string PreviousOperationRunning => m_Language.TryGetString( "PreviousOperationRunning", PREVIOUS_OPERATION_RUNNING );
		internal static string SessionEnded => m_Language.TryGetString( "SessionEnded", SESSION_ENDED );
		internal static string ConnectionRejected => m_Language.TryGetString( "ConnectionRejected", CONNECTION_REJECTED );
		internal static string DisconnectPending => m_Language.TryGetString( "DisconnectPending", DISCONNECT_PENDING );
		internal static string NotConnected => m_Language.TryGetString( "NotConnected", NOT_CONNECTED );
		internal static string ResourceBusy => m_Language.TryGetString( "ResourceBusy", RESOURCE_BUSY );
		internal static string OperationBusy => m_Language.TryGetString( "OperationBusy", OPERATION_BUSY );
		internal static string OperationCancelled => m_Language.TryGetString( "OperationCancelled", OPERATION_CANCELLED );
		internal static string WaitStopped => m_Language.TryGetString( "WaitStopped", WAIT_STOPPED );
		internal static string CalibrationCancelled => m_Language.TryGetString( "CalibrationCancelled", CALIBRATION_CANCELLED );
		internal static string CalibrationTimeout => m_Language.TryGetString( "CalibrationTimeout", CALIBRATION_TIMEOUT );
		internal static string InvalidCalibrationResult => m_Language.TryGetString( "InvalidCalibrationResult", INVALID_CALIBRATION_RESULT );
		internal static string ReadProgramCoordinates => m_Language.TryGetString( "ReadProgramCoordinates", READ_PROGRAM_COORDINATES );
		internal static string ReadTolerance => m_Language.TryGetString( "ReadTolerance", READ_TOLERANCE );
		internal static string IncompletePoints => m_Language.TryGetString( "IncompletePoints", INCOMPLETE_POINTS );

		internal static string InvalidMachineParameter( int index, int value ) =>
			string.Format( m_Language.TryGetString( "InvalidMachineParameter", INVALID_MACHINE_PARAMETER ), index, value );

		internal static string InvalidRotaryParameter( int index, int value ) =>
			string.Format( m_Language.TryGetString( "InvalidRotaryParameter", INVALID_ROTARY_PARAMETER ), index, value );

		internal static string AmbiguousHeadAxis( int value, string head, string first, string second ) =>
			string.Format( m_Language.TryGetString( "AmbiguousHeadAxis", AMBIGUOUS_HEAD_AXIS ), value, head, first, second );

		internal static string ReadMachineCoordinate( int index ) =>
			string.Format( m_Language.TryGetString( "ReadMachineCoordinate", READ_MACHINE_COORDINATE ), index );

		internal static string ClearOffset( int index, string error ) =>
			string.Format( m_Language.TryGetString( "ClearOffset", CLEAR_OFFSET ), index, error );

		internal static string AlarmUnavailable( int count ) =>
			string.Format( m_Language.TryGetString( "AlarmUnavailable", ALARM_UNAVAILABLE ), count );

		internal static string AlarmCountRead( int transportStatus, int controllerStatus ) =>
			string.Format( m_Language.TryGetString( "AlarmCountRead", ALARM_COUNT_READ ), transportStatus, controllerStatus );

		internal static string InvalidAlarmCount( int count ) =>
			string.Format( m_Language.TryGetString( "InvalidAlarmCount", INVALID_ALARM_COUNT ), count );

		const string INVALID_CONNECTION = "CNC IP 位址或逾時設定無效。";
		const string LIFECYCLE_BUSY = "CNC 連線生命週期作業忙碌中。";
		const string CONNECTION_CANCELLED = "CNC 連線已取消。";
		const string PREVIOUS_OPERATION_RUNNING = "先前的 SDK 作業仍在執行。";
		const string SESSION_ENDED = "CNC 連線工作階段已結束。";
		const string CONNECTION_REJECTED = "OpenCNC 拒絕連線。";
		const string DISCONNECT_PENDING = "SDK 作業仍在執行，尚未確認斷線。";
		const string NOT_CONNECTED = "CNC 未連線。";
		const string RESOURCE_BUSY = "CNC 資源作業仍在執行。";
		const string OPERATION_BUSY = "CNC 作業仍在執行。";
		const string OPERATION_CANCELLED = "作業已取消。";
		const string WAIT_STOPPED = "已停止等待；SDK 作業可能仍在執行。";
		const string CALIBRATION_CANCELLED = "校正已取消。";
		const string CALIBRATION_TIMEOUT = "三點校正等待超過 5 秒。";
		const string INVALID_CALIBRATION_RESULT = "校正結果旗標無效。";
		const string READ_PROGRAM_COORDINATES = "程式座標讀取失敗。";
		const string READ_TOLERANCE = "誤差容忍讀取失敗（@78406）。";
		const string INCOMPLETE_POINTS = "必須提供三組完整的 XYZ 座標點。";
		const string INVALID_MACHINE_PARAMETER = "Pr{0}={1}，預期為 1、2 或 3。";
		const string INVALID_ROTARY_PARAMETER = "Pr{0}={1}，預期為 400、500 或 600。";
		const string AMBIGUOUS_HEAD_AXIS = "Pr3005={0} 指定擺頭 {1}，無法唯一對應旋轉軸 {2}／{3}。";
		const string READ_MACHINE_COORDINATE = "無法讀取機械座標 #{0}。";
		const string CLEAR_OFFSET = "清除 #{0} 失敗，部分偏置可能已清除：{1}";
		const string ALARM_UNAVAILABLE = "CNC 當前警報資料無法取得，待處理數量：{0}。";
		const string ALARM_COUNT_READ = "CNC 當前警報數量讀取失敗。傳輸狀態：{0}，控制器狀態：{1}。";
		const string INVALID_ALARM_COUNT = "CNC 當前警報數量無效：{0}。";
	}
}
