using CncManager.Definition;
using Syntec.OpenCNC;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CncManager.CNCCore
{
	// The only class in this project that should reference Syntec.OpenCNC
	public class CncApi
	{
		public static bool IsConnected
		{
			get
			{
				return m_IsConnected;
			}
		}

		internal CncApi( string cncIp, int timeoutMs )
		{
			m_CncIp = cncIp;
			m_TimeoutMs = timeoutMs;
			m_SyntecCnc = new SyntecAPI.Cnc();
			m_Current = this;
		}

		#region Event Subscription

		internal Task<CncResult<bool>> SubscribeAllEventsAsync()
		{
			return ExecuteResourceAsync( () => { SubscribeEvents(); return true; } );
		}

		void SubscribeEvents()
		{
			if( m_IsSubscribed ) {
				return;
			}
			m_IsSubscribed = true;
			Cnc.OnResetHappen += Cnc_OnResetHappen;
			Cnc.OnAlarmChanged += Cnc_OnAlarmChanged;
			Cnc.OnParamChanged += Cnc_OnParamChanged;
			Cnc.OnCncStatusChanged += Cnc_OnCncStatusChanged;
			Cnc.OnCheckedRegisterValueChanged += Cnc_OnCheckedRegisterValueChanged;
			m_SyntecCnc.OnConnectionError += SyntecCnc_OnConnectionError;
		}

		internal void UnsubscribeAllEvents()
		{
			if( !m_IsSubscribed ) {
				return;
			}
			Cnc.OnResetHappen -= Cnc_OnResetHappen;
			Cnc.OnAlarmChanged -= Cnc_OnAlarmChanged;
			Cnc.OnParamChanged -= Cnc_OnParamChanged;
			Cnc.OnCncStatusChanged -= Cnc_OnCncStatusChanged;
			Cnc.OnCheckedRegisterValueChanged -= Cnc_OnCheckedRegisterValueChanged;
			m_SyntecCnc.OnConnectionError -= SyntecCnc_OnConnectionError;
			m_IsSubscribed = false;
		}

		#endregion

		#region Event Handlers

		void Cnc_OnResetHappen( object sender, EventArgs e )
		{
			if( IsCurrent && IsConnected ) {
				OnCNCResetHappen?.Invoke( this, EventArgs.Empty );
			}
		}

		void Cnc_OnAlarmChanged( object sender, EventArgs e )
		{
			if( IsCurrent && IsConnected ) {
				OnCNCAlarmChanged?.Invoke( this, EventArgs.Empty );
			}
		}

		void Cnc_OnParamChanged( object sender, EventArgs e )
		{
			if( IsCurrent && IsConnected ) {
				OnParamChanged?.Invoke( this, EventArgs.Empty );
			}
		}

		async void Cnc_OnCncStatusChanged( CncStatusChangeArgs e )
		{
			if( !IsCurrent ) {
				return;
			}
			try {
				CncResult<CncStatus> result;
				do {
					result = await GetCncStatusAsync().ConfigureAwait( false );
					if( result.Status == CncResultStatus.Busy ) {
						await Task.Delay( CncConstants.PLC_SCANNING_TIME_MS ).ConfigureAwait( false );
					}
				} while( IsCurrent && IsConnected && result.Status == CncResultStatus.Busy );
				if( IsCurrent && IsConnected && result.IsSuccess ) {
					OnCncStatusChanged?.Invoke( result.Data );
				}
			}
			catch( Exception ex ) {
				OperationFailed?.Invoke( ex.Message );
			}
		}

		async void Cnc_OnCheckedRegisterValueChanged( RegisterValueChangeArgs e )
		{
			if( !IsCurrent ) {
				return;
			}
			int index = e.RegisterAddress;
			try {
				CncResult<int> result;
				do {
					result = await ReadRRegisterAsync( index ).ConfigureAwait( false );
					if( result.Status == CncResultStatus.Busy ) {
						await Task.Delay( CncConstants.PLC_SCANNING_TIME_MS ).ConfigureAwait( false );
					}
				} while( IsCurrent && IsConnected && result.Status == CncResultStatus.Busy );
				if( IsCurrent && IsConnected && result.IsSuccess ) {
					RRegisterChanged?.Invoke( index, result.Data );
				}
			}
			catch( Exception ex ) {
				OperationFailed?.Invoke( ex.Message );
			}
		}

		void SyntecCnc_OnConnectionError( SyntecAPI.Cnc.ConnectionCode eErrCode )
		{
			if( !IsCurrent || !m_IsConnected ) {
				return;
			}
			m_IsConnected = false;
			ConnectionError?.Invoke();
		}

		#endregion

		#region Connection

		internal async Task<CncResult<bool>> ConnectAsync( CancellationToken cancellationToken )
		{
			if( !IsCurrent ) {
				return CncResult<bool>.Failure( CncResultStatus.Cancelled, CncMessages.SessionEnded );
			}
			var result = await m_Guard.ExecuteAsync( () => OcApiTCP.Connect( m_CncIp, m_TimeoutMs ),
				m_TimeoutMs + CncConstants.GUARD_TIMEOUT_MARGIN_MS, cancellationToken,
				connected => { if( connected ) { OcApiTCP.DisconnectEx(); } } ).ConfigureAwait( false );
			if( result.IsSuccess && !result.Data ) {
				return CncResult<bool>.Failure( CncResultStatus.Failed, CncMessages.ConnectionRejected );
			}
			if( result.IsSuccess ) {
				if( !IsCurrent || cancellationToken.IsCancellationRequested ) {
					await DisconnectAsync().ConfigureAwait( false );
					return CncResult<bool>.Failure( CncResultStatus.Cancelled, CncMessages.SessionEnded );
				}
				m_IsConnected = true;
			}
			return result;
		}

		internal async Task<CncResult<bool>> DisconnectAsync()
		{
			m_IsConnected = false;
			if( !await m_Guard.WaitForIdleAsync( CncConstants.DEFAULT_API_TIMEOUT_MS ).ConfigureAwait( false ) ) {
				return CncResult<bool>.Failure( CncResultStatus.TimedOut, CncMessages.DisconnectPending );
			}
			return await m_Guard.ExecuteAsync( () =>
			{
				UnsubscribeAllEvents();
				OcApiTCP.DisconnectEx();
				return true;
			} ).ConfigureAwait( false );
		}

		internal void Invalidate()
		{
			m_Active = false;
			if( m_Current == this ) {
				m_IsConnected = false;
			}
		}

		#endregion

		#region Axis

		public static Task<CncResult<CncAxis[]>> ReadAxesAsync()
		{
			return ExecuteResourceAsync( () =>
			{
				string firstName = CncAxis.GetRotaryName( CncConstants.PR_ROTARY_AXIS_1_NAME, Cnc.Param[ CncConstants.PR_ROTARY_AXIS_1_NAME ] );
				string secondName = CncAxis.GetRotaryName( CncConstants.PR_ROTARY_AXIS_2_NAME, Cnc.Param[ CncConstants.PR_ROTARY_AXIS_2_NAME ] );
				CncAxisRole firstRole;
				CncAxisRole secondRole;
				int machineType = Cnc.Param[ CncConstants.PR_MACHINE_TYPE ];
				switch( machineType ) {
					case 1:
						firstRole = secondRole = CncAxisRole.Head;
						break;
					case 2:
						firstRole = secondRole = CncAxisRole.Table;
						break;
					case 3:
						int headAxis = Cnc.Param[ CncConstants.PR_HEAD_AXIS ];
						if( headAxis < 1 || headAxis > 3 ) {
							throw new InvalidOperationException( CncMessages.InvalidMachineParameter( CncConstants.PR_HEAD_AXIS, headAxis ) );
						}
						string headName = CncAxis.GetRotaryName( CncConstants.PR_HEAD_AXIS, ( headAxis + 3 ) * 100 );
						if( ( firstName == headName ) == ( secondName == headName ) ) {
							throw new InvalidOperationException( CncMessages.AmbiguousHeadAxis( headAxis, headName, firstName, secondName ) );
						}
						firstRole = firstName == headName ? CncAxisRole.Head : CncAxisRole.Table;
						secondRole = secondName == headName ? CncAxisRole.Head : CncAxisRole.Table;
						break;
					default:
						throw new InvalidOperationException( CncMessages.InvalidMachineParameter( CncConstants.PR_MACHINE_TYPE, machineType ) );
				}
				return new CncAxis[] {
					new CncAxis( "X", 1 ),
					new CncAxis( "Y", 2 ),
					new CncAxis( "Z", 3 ),
					new CncAxis( firstName, 4, firstRole ),
					new CncAxis( secondName, 5, secondRole )
				};
			} );
		}

		#endregion

		#region R Register

		public static Task<CncResult<int>> ReadRRegisterAsync( int index )
		{
			return ExecuteResourceAsync( () => Cnc.PLC.RRegister[ index ] );
		}

		public static bool WriteRRegister( int index, int value )
		{
			return ExecuteResource( delegate { Cnc.PLC.RRegister[ index ] = value; return true; } ).IsSuccess;
		}

		#endregion

		#region Global Variable (@)

		public static Task<CncResult<double?>> ReadGlobalVariableAsync( int index )
		{
			return ExecuteResourceAsync( () => ConvertVariableValue( Cnc.GlobalVar[ index ] ) );
		}

		#endregion

		#region System Variable (#)

		public static Task<CncResult<bool>> WriteCalibrationInputAsync( int index, double value, CancellationToken cancellationToken )
		{
			if( !( index >= CncConstants.GLOBAL_VAR_DRAWING_POINT_BASE && index < CncConstants.GLOBAL_VAR_DRAWING_POINT_BASE + 9 ) &&
				!( index >= CncConstants.GLOBAL_VAR_WORKPIECE_POINT_BASE && index < CncConstants.GLOBAL_VAR_WORKPIECE_POINT_BASE + 9 ) &&
				index != CncConstants.GLOBAL_VAR_CALIBRATION_TOLERANCE ) {
				throw new ArgumentOutOfRangeException( nameof( index ) );
			}
			if( double.IsNaN( value ) || double.IsInfinity( value ) || ( index == CncConstants.GLOBAL_VAR_CALIBRATION_TOLERANCE && value < 0 ) ) {
				throw new ArgumentOutOfRangeException( nameof( value ) );
			}
			CncApi current = m_Current;
			return ExecuteResourceAsync( () =>
			{
				CheckOffsetOperation( current, cancellationToken );
				Cnc.GlobalVar[ index ] = value;
				return true;
			}, reportFailure: false );
		}

		public static Task<CncResult<double[]>> ReadCalibrationPositionAsync( CancellationToken cancellationToken )
		{
			CncApi current = m_Current;
			return ExecuteResourceAsync( () =>
			{
				double[] coordinates = new double[ 3 ];
				for( int axis = 0; axis < coordinates.Length; axis++ ) {
					CheckOffsetOperation( current, cancellationToken );
					double? value = ConvertVariableValue( Cnc.SystemVar[ CncConstants.FIRST_COORD_ID, CncConstants.SYSTEM_VAR_PROGRAM_BASE + axis + 1 ] );
					if( !value.HasValue || double.IsNaN( value.Value ) || double.IsInfinity( value.Value ) ) {
						throw new InvalidOperationException( CncMessages.ReadProgramCoordinates );
					}
					coordinates[ axis ] = value.Value;
				}
				return coordinates;
			}, reportFailure: false );
		}

		public static Task<CncResult<double>> ReadCalibrationToleranceAsync( CancellationToken cancellationToken )
		{
			CncApi current = m_Current;
			return ExecuteResourceAsync( () =>
			{
				CheckOffsetOperation( current, cancellationToken );
				double? value = ConvertVariableValue( Cnc.GlobalVar[ CncConstants.GLOBAL_VAR_CALIBRATION_TOLERANCE ] );
				if( !value.HasValue || double.IsNaN( value.Value ) || double.IsInfinity( value.Value ) || value.Value < 0 ) {
					throw new InvalidOperationException( CncMessages.ReadTolerance );
				}
				return value.Value;
			}, reportFailure: false );
		}

		public static Task<CncResult<bool>> WriteCalibrationInputsAsync( double[] drawing, double[] workpiece, double tolerance, CancellationToken cancellationToken )
		{
			ValidateCalibrationInputs( drawing, workpiece, tolerance );
			drawing = (double[])drawing.Clone();
			workpiece = (double[])workpiece.Clone();
			CncApi current = m_Current;
			return ExecuteResourceAsync( () =>
			{
				for( int index = 0; index < 9; index++ ) {
					CheckOffsetOperation( current, cancellationToken );
					Cnc.GlobalVar[ CncConstants.GLOBAL_VAR_DRAWING_POINT_BASE + index ] = drawing[ index ];
					CheckOffsetOperation( current, cancellationToken );
					Cnc.GlobalVar[ CncConstants.GLOBAL_VAR_WORKPIECE_POINT_BASE + index ] = workpiece[ index ];
				}
				CheckOffsetOperation( current, cancellationToken );
				Cnc.GlobalVar[ CncConstants.GLOBAL_VAR_CALIBRATION_TOLERANCE ] = tolerance;
				return true;
			}, reportFailure: false );
		}

		static void ValidateCalibrationInputs( double[] drawing, double[] workpiece, double tolerance )
		{
			if( drawing == null || drawing.Length != 9 || workpiece == null || workpiece.Length != 9 ) {
				throw new ArgumentException( CncMessages.IncompletePoints );
			}
			foreach( double value in drawing ) {
				if( double.IsNaN( value ) || double.IsInfinity( value ) ) {
					throw new ArgumentOutOfRangeException( nameof( drawing ) );
				}
			}
			foreach( double value in workpiece ) {
				if( double.IsNaN( value ) || double.IsInfinity( value ) ) {
					throw new ArgumentOutOfRangeException( nameof( workpiece ) );
				}
			}
			if( double.IsNaN( tolerance ) || double.IsInfinity( tolerance ) || tolerance < 0 ) {
				throw new ArgumentOutOfRangeException( nameof( tolerance ) );
			}
		}

		public static async Task<CncResult<CncCalibrationResult>> CalculateCalibrationAsync( double[] drawing, double[] workpiece, double tolerance, CancellationToken cancellationToken,
			Action transmitted = null )
		{
			ValidateCalibrationInputs( drawing, workpiece, tolerance );
			drawing = (double[])drawing.Clone();
			workpiece = (double[])workpiece.Clone();
			CncApi current = m_Current;
			var start = await ExecuteResourceAsync( () =>
			{
				for( int index = 0; index < 9; index++ ) {
					CheckOffsetOperation( current, cancellationToken );
					Cnc.GlobalVar[ CncConstants.GLOBAL_VAR_DRAWING_POINT_BASE + index ] = drawing[ index ];
					CheckOffsetOperation( current, cancellationToken );
					Cnc.GlobalVar[ CncConstants.GLOBAL_VAR_WORKPIECE_POINT_BASE + index ] = workpiece[ index ];
				}
				CheckOffsetOperation( current, cancellationToken );
				Cnc.GlobalVar[ CncConstants.GLOBAL_VAR_CALIBRATION_TOLERANCE ] = tolerance;
				CheckOffsetOperation( current, cancellationToken );
				Cnc.GlobalVar[ CncConstants.GLOBAL_VAR_CALIBRATION_RESULT ] = 0d;
				CheckOffsetOperation( current, cancellationToken );
				Cnc.PLC.RRegister[ CncConstants.R_PollingAction ] = (int)PollingAction.CalculateCalibration;
				return true;
			}, reportFailure: false ).ConfigureAwait( false );
			if( !start.IsSuccess ) {
				return CncResult<CncCalibrationResult>.Failure( start.Status, start.ErrorMessage );
			}
			transmitted?.Invoke();
			var watch = System.Diagnostics.Stopwatch.StartNew();
			while( watch.ElapsedMilliseconds < CncConstants.CALIBRATION_TIMEOUT_MS ) {
				if( cancellationToken.IsCancellationRequested ) {
					return CncResult<CncCalibrationResult>.Failure( CncResultStatus.Cancelled, CncMessages.CalibrationCancelled );
				}
				var result = await ExecuteResourceAsync( () =>
				{
					CheckOffsetOperation( current, cancellationToken );
					double? flag = ConvertVariableValue( Cnc.GlobalVar[ CncConstants.GLOBAL_VAR_CALIBRATION_RESULT ] );
					if( flag == 0 ) {
						return (CncCalibrationResult)null;
					}
					if( flag != 1 && flag != -1 ) {
						throw new InvalidOperationException( CncMessages.InvalidCalibrationResult );
					}
					CheckOffsetOperation( current, cancellationToken );
					double? error = null;
					try {
						error = ConvertVariableValue( Cnc.GlobalVar[ CncConstants.GLOBAL_VAR_CALIBRATION_ERROR ] );
					}
					catch( Exception ex ) {
						System.Diagnostics.Trace.TraceError( "Calibration error read failed after terminal result: {0}", ex );
					}
					CheckOffsetOperation( current, cancellationToken );
					if( !error.HasValue || double.IsNaN( error.Value ) || double.IsInfinity( error.Value ) || error.Value < 0 ) {
						error = null;
					}
					return new CncCalibrationResult( flag == 1, error );
				}, reportFailure: false ).ConfigureAwait( false );
				if( !result.IsSuccess ) {
					return CncResult<CncCalibrationResult>.Failure( result.Status, result.ErrorMessage );
				}
				if( watch.ElapsedMilliseconds >= CncConstants.CALIBRATION_TIMEOUT_MS ) {
					break;
				}
				if( result.Data != null ) {
					return CncResult<CncCalibrationResult>.Success( result.Data );
				}
				await Task.Delay( CncConstants.RESOURCE_POLL_INTERVAL_MS ).ConfigureAwait( false );
			}
			return CncResult<CncCalibrationResult>.Failure( CncResultStatus.TimedOut, CncMessages.CalibrationTimeout );
		}

		public static async Task<double?> ReadSystemVariableAsync( int coordID, int index )
		{
			var result = await ExecuteResourceAsync<double?>( delegate
			{
				return ConvertVariableValue( Cnc.SystemVar[ coordID, index ] );
			} ).ConfigureAwait( false );
			return result.Data;
		}

		public static Task<CncResult<bool>> SetProgramOffsetAsync( int axisID, CancellationToken cancellationToken )
		{
			if( axisID < 1 || axisID > 5 ) {
				throw new ArgumentOutOfRangeException( nameof( axisID ) );
			}
			CncApi current = m_Current;
			return ExecuteResourceAsync( () =>
			{
				CheckOffsetOperation( current, cancellationToken );
				int source = CncConstants.SYSTEM_VAR_MACHINE_BASE + axisID;
				double? value = ConvertVariableValue( Cnc.SystemVar[ CncConstants.FIRST_COORD_ID, source ] );
				if( !value.HasValue || double.IsNaN( value.Value ) || double.IsInfinity( value.Value ) ) {
					throw new InvalidOperationException( CncMessages.ReadMachineCoordinate( source ) );
				}
				CheckOffsetOperation( current, cancellationToken );
				Cnc.SystemVar[ CncConstants.FIRST_COORD_ID, CncConstants.SYSTEM_VAR_PROGRAM_OFFSET_BASE + axisID ] = value.Value;
				return true;
			}, reportFailure: false );
		}

		public static Task<CncResult<bool>> ClearProgramOffsetsAsync( CancellationToken cancellationToken )
		{
			CncApi current = m_Current;
			return ExecuteResourceAsync( () =>
			{
				for( int axisID = 1; axisID <= 5; axisID++ ) {
					CheckOffsetOperation( current, cancellationToken );
					int index = CncConstants.SYSTEM_VAR_PROGRAM_OFFSET_BASE + axisID;
					try {
						Cnc.SystemVar[ CncConstants.FIRST_COORD_ID, index ] = 0d;
					}
					catch( Exception ex ) {
						throw new InvalidOperationException( CncMessages.ClearOffset( index, ex.Message ), ex );
					}
				}
				return true;
			}, reportFailure: false );
		}

		static void CheckOffsetOperation( CncApi current, CancellationToken cancellationToken )
		{
			cancellationToken.ThrowIfCancellationRequested();
			if( current == null || !current.IsCurrent || !IsConnected ) {
				throw new InvalidOperationException( CncMessages.SessionEnded );
			}
		}

		static double? ConvertVariableValue( object value )
		{
			return value == null ? (double?)null : Convert.ToDouble( value );
		}

		#endregion

		#region Coordinate State Variable (Q)

		public static Task<CncResult<int>> ReadQVariableAsync( int coordID, int index )
		{
			return ExecuteResourceAsync( () => Cnc.CoordStateVar[ coordID, index ].Value );
		}

		#endregion

		#region K Variable

		public static Task<CncResult<int>> ReadKVariableAsync( int index )
		{
			return ExecuteResourceAsync( delegate { return Cnc.State[ index ]; } );
		}

		#endregion

		#region CNC Alarms

		public static Task<CncResult<CncAlarm[]>> ReadPendingAlarmsAsync()
		{
			return ExecuteResourceAsync( () =>
			{
				if( ReadPendingAlarmCount() == 0 ) {
					return new CncAlarm[ 0 ];
				}
				TAlarmPending[] pending = Cnc.Alarm.DumpAlarmPending();
				if( pending == null ) {
					int count = ReadPendingAlarmCount();
					if( count == 0 ) {
						return new CncAlarm[ 0 ];
					}
					throw new InvalidOperationException( CncMessages.AlarmUnavailable( count ) );
				}
				CncAlarm[] alarms = new CncAlarm[ pending.Length ];
				for( int index = 0; index < pending.Length; index++ ) {
					TAlarmPending alarm = pending[ index ];
					alarms[ index ] = new CncAlarm( (int)alarm.ClassID, alarm.ClassID.ToString().Replace( "ALARMCLASS_", string.Empty ),
						alarm.ObjectID, alarm.AlarmID, alarm.SubAlarmID, alarm.Time, alarm.HintText );
				}
				return alarms;
			}, reportFailure: false );
		}


		static int ReadPendingAlarmCount()
		{
			// SDK PendingCount returns 0 on failure; P/Invoke preserves both status codes.
			// Command: (category << 10) | function; low 10 bits hold the function.
			// CNC category = 1; pending-count function = 74; command = 1098.
			const int getPendingAlarmSize = ( 1 << 10 ) | 74;
			int input = 0;
			Out_OCK_NcGetPendingAlarmSizeParams output = new Out_OCK_NcGetPendingAlarmSizeParams();
			int status = OCAPI_MultiTCPKrnlAPI( IntPtr.Zero, getPendingAlarmSize, ref input, 0,
				ref output, Marshal.SizeOf( typeof( Out_OCK_NcGetPendingAlarmSizeParams ) ) );
			if( status != 0 || output.hr != 0 ) {
				throw new InvalidOperationException( CncMessages.AlarmCountRead( status, output.hr ) );
			}
			if( output.nValue < 0 ) {
				throw new InvalidOperationException( CncMessages.InvalidAlarmCount( output.nValue ) );
			}
			return output.nValue;
		}

		[DllImport( "OCAPI.dll", ExactSpelling = true )]
		static extern int OCAPI_MultiTCPKrnlAPI( IntPtr linkHandle, int code, ref int input, int inputSize,
			ref Out_OCK_NcGetPendingAlarmSizeParams output, int outputSize );

		#endregion

		#region CNC Status

		public static Task<CncResult<string>> ReadMainProgramAsync( int coordID )
		{
			return ExecuteResourceAsync( () => Cnc.State.MainProgram( coordID ) );
		}

		public static Task<CncResult<CncStatus>> GetCncStatusAsync()
		{
			return ExecuteResourceAsync( () => ConvertECncStatusToCncStatus( Cnc.State.CncStatus ) );
		}

		static CncStatus ConvertECncStatusToCncStatus( Cnc.ECncStatus eCncStatus )
		{
			switch( eCncStatus ) {
				case Cnc.ECncStatus.CNCSTATUS_NOT_READY:
					return CncStatus.NOTREADY;
				case Cnc.ECncStatus.CNCSTATUS_M_READY:
					return CncStatus.READY;
				case Cnc.ECncStatus.CNCSTATUS_C_START:
					return CncStatus.START;
				case Cnc.ECncStatus.CNCSTATUS_FEEDHOLD:
					return CncStatus.FEEDHOLD;
				case Cnc.ECncStatus.CNCSTATUS_B_STOP:
					return CncStatus.STOP;
				case Cnc.ECncStatus.CNCSTATUS_EXECUTE:
					return CncStatus.EXECUTE;
				case Cnc.ECncStatus.CNCSTATUS_RECORD:
					return CncStatus.RECORD;
				default:
					return CncStatus.NOTREADY;
			}
		}

		#endregion

		#region CNC Operations

		public static Task<CncResult<bool>> AssignMainProgramAsync( string programName, CancellationToken cancellationToken )
		{
			return ExecuteResourceAsync( () =>
			{
				cancellationToken.ThrowIfCancellationRequested();
				Cnc.AssignMainProgram( CncConstants.FIRST_COORD_ID, programName, -1, -1 );
				return true;
			} );
		}

		public static bool ExecuteBlocks( string blocks )
		{
			return ExecuteResource( () => { Cnc.ExecuteBlocks( blocks ); return true; } ).IsSuccess;
		}

		public static bool ResetCnc()
		{
			return ExecuteResource( delegate { Cnc.SWP.ResetCnc(); return true; } ).IsSuccess;
		}

		#endregion

		// raw events; subscribe CncManager's re-wrapped events instead
		internal event Action<CncStatus> OnCncStatusChanged;
		internal event Action<int, int> RRegisterChanged;
		internal event EventHandler OnCNCResetHappen;
		internal event EventHandler OnCNCAlarmChanged;
		internal event EventHandler OnParamChanged;

		// raised when OpenCNC detects connection error (socket drop, reset, etc.)
		internal event Action ConnectionError;

		internal event Action<string> OperationFailed;

		internal static bool HasPendingOperation => m_Guard.HasPendingOperation;
		bool IsCurrent => m_Active && m_Current == this;

		internal static Task<CncResult<bool>> CheckHeartbeatAsync( bool initialize )
		{
			return ExecuteResourceAsync( () =>
			{
				if( !initialize && Cnc.PLC.RRegister[ CncConstants.R_CNC_AND_PC_HANDSHAKE ] != CncConstants.HANDSHAKE_CNC_ECHO_VALUE ) {
					return false;
				}
				Cnc.PLC.RRegister[ CncConstants.R_CNC_AND_PC_HANDSHAKE ] = CncConstants.HANDSHAKE_PC_WRITE_VALUE;
				return true;
			} );
		}

		static CncResult<T> ExecuteResource<T>( Func<T> operation )
		{
			CncApi current = m_Current;
			if( current == null || !current.IsCurrent || !IsConnected ) {
				return CncResult<T>.Failure( CncResultStatus.NotConnected, CncMessages.NotConnected );
			}
			if( !m_ResourceGate.Wait( CncConstants.DEFAULT_API_TIMEOUT_MS ) ) {
				return CncResult<T>.Failure( CncResultStatus.Busy, CncMessages.ResourceBusy );
			}
			try {
				if( !current.IsCurrent || !IsConnected ) {
					return CncResult<T>.Failure( CncResultStatus.NotConnected, CncMessages.SessionEnded );
				}
				var result = m_Guard.Execute( delegate
				{
					if( !current.IsCurrent || !IsConnected ) {
						throw new InvalidOperationException( CncMessages.SessionEnded );
					}
					return operation();
				} );
				if( current.IsCurrent && result.Status == CncResultStatus.Failed ) {
					current.OperationFailed?.Invoke( result.ErrorMessage );
				}
				return result;
			}
			finally {
				m_ResourceGate.Release();
			}
		}

		static async Task<CncResult<T>> ExecuteResourceAsync<T>( Func<T> operation, bool reportFailure = true )
		{
			CncApi current = m_Current;
			if( current == null || !current.IsCurrent || !IsConnected ) {
				return CncResult<T>.Failure( CncResultStatus.NotConnected, CncMessages.NotConnected );
			}
			await m_ResourceGate.WaitAsync().ConfigureAwait( false );
			try {
				if( !current.IsCurrent || !IsConnected ) {
					return CncResult<T>.Failure( CncResultStatus.NotConnected, CncMessages.SessionEnded );
				}
				var result = await m_Guard.ExecuteAsync( () =>
				{
					if( !current.IsCurrent || !IsConnected ) {
						throw new InvalidOperationException( CncMessages.SessionEnded );
					}
					return operation();
				} ).ConfigureAwait( false );
				if( current.IsCurrent && ( result.Status == CncResultStatus.Failed || result.Status == CncResultStatus.TimedOut ) ) {
					if( reportFailure ) {
						current.OperationFailed?.Invoke( result.ErrorMessage );
					}
					if( result.Status == CncResultStatus.TimedOut ) {
						m_IsConnected = false;
						current.ConnectionError?.Invoke();
					}
				}
				return result;
			}
			finally {
				m_ResourceGate.Release();
			}
		}

		// Syntec OpenCNC API wrapper
		readonly string m_CncIp;
		readonly int m_TimeoutMs;
		readonly SyntecAPI.Cnc m_SyntecCnc;
		bool m_IsSubscribed;
		volatile bool m_Active = true;
		static readonly CncApiTimeoutGuard m_Guard = new CncApiTimeoutGuard();
		static readonly SemaphoreSlim m_ResourceGate = new SemaphoreSlim( 1, 1 );
		static volatile CncApi m_Current;

		// shared connection flag; only one CNC connection exists at a time
		static volatile bool m_IsConnected;
	}
}
