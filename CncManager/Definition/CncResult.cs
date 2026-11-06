namespace CncManager.Definition
{
	// Controller calibration result carried by CncResult<T>.
	public sealed class CncCalibrationResult
	{
		public CncCalibrationResult( bool succeeded, double? deviation )
		{
			Succeeded = succeeded;
			Deviation = deviation;
		}

		// Controller-reported calibration success, not call success.
		public bool Succeeded { get; }
		// Controller-reported deviation; null if unavailable or invalid.
		public double? Deviation { get; }
	}

	// Outcome of a CNC call or workflow, not the machine operating state.
	public enum CncResultStatus
	{
		Success,
		NotConnected,
		Busy,
		Failed,
		TimedOut,
		Cancelled,
	}

	public sealed class CncResult<T>
	{
		CncResult( CncResultStatus status, T data, string errorMessage )
		{
			Status = status;
			Data = data;
			ErrorMessage = errorMessage;
		}

		// Outcome of the CNC call or workflow.
		public CncResultStatus Status { get; }
		// Returned payload; default(T) on failure, could be null on success.
		public T Data { get; }
		// Failure details; null on success.
		public string ErrorMessage { get; }
		// Call success only
		public bool IsSuccess => Status == CncResultStatus.Success;

		public static CncResult<T> Success( T data )
		{
			return new CncResult<T>( CncResultStatus.Success, data, null );
		}

		public static CncResult<T> Failure( CncResultStatus status, string errorMessage )
		{
			if( status == CncResultStatus.Success ) {
				throw new System.ArgumentException( "Use Success for successful results.", nameof( status ) );
			}
			return new CncResult<T>( status, default( T ), errorMessage );
		}
	}
}
