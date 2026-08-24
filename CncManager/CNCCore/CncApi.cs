using CncManager.Definition;
using Syntec.OpenCNC;
using Syntec.OpenCNC.Threading;
using System;
using System.Collections.Generic;

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
			m_TimeoutGuard = new CncApiTimeoutGuard();
			m_SyntecCnc = new SyntecAPI.Cnc();
		}

		#region Event Subscription


		public static void SetRRegisterCheckList( int[] checkList )
		{
			Cnc.SetCheckRegister( checkList );
		}

		internal void SubscribeAllEvents()
		{
			Cnc.OnResetHappen += Cnc_OnResetHappen;
			Cnc.OnAlarmChanged += Cnc_OnAlarmChanged;
			Cnc.OnParamChanged += Cnc_OnParamChanged;
			Cnc.OnCncStatusChanged += Cnc_OnCncStatusChanged;
			Cnc.OnCheckedRegisterValueChanged += Cnc_OnCheckedRegisterValueChanged;
			m_SyntecCnc.OnConnectionError += SyntecCnc_OnConnectionError;
		}

		internal void UnsubscribeAllEvents()
		{
			Cnc.OnResetHappen -= Cnc_OnResetHappen;
			Cnc.OnAlarmChanged -= Cnc_OnAlarmChanged;
			Cnc.OnParamChanged -= Cnc_OnParamChanged;
			Cnc.OnCncStatusChanged -= Cnc_OnCncStatusChanged;
			Cnc.OnCheckedRegisterValueChanged -= Cnc_OnCheckedRegisterValueChanged;
			m_SyntecCnc.OnConnectionError -= SyntecCnc_OnConnectionError;
		}

		#endregion

		#region Event Handlers

		void Cnc_OnResetHappen( object sender, EventArgs e )
		{
			OnCNCResetHappen?.Invoke( sender, e );
		}

		void Cnc_OnAlarmChanged( object sender, EventArgs e )
		{
			OnCNCAlarmChanged?.Invoke( sender, e );
		}

		void Cnc_OnParamChanged( object sender, EventArgs e )
		{
			OnParamChanged?.Invoke( sender, e );
		}

		void Cnc_OnCncStatusChanged( CncStatusChangeArgs e )
		{
			if( !m_IsConnected ) {
				return;
			}
			CncStatus status = ConvertECncStatusToCncStatus( Cnc.State.CncStatus );
			OnCncStatusChanged?.Invoke( status );
		}

		void Cnc_OnCheckedRegisterValueChanged( RegisterValueChangeArgs e )
		{
			if( RRegisterChanged == null ) {
				return;
			}
			int? value = ReadRRegister( e.RegisterAddress );
			if( value.HasValue ) {
				RRegisterChanged( e.RegisterAddress, value.Value );
			}
		}

		void SyntecCnc_OnConnectionError( SyntecAPI.Cnc.ConnectionCode eErrCode )
		{
			lock( m_ConnectLock ) {
				if( !m_IsConnected ) {
					return;
				}
				m_IsConnected = false;
			}
			ConnectionError?.Invoke();
		}

		#endregion

		#region Connection

		internal bool Connect( bool raiseConnectFailed = true )
		{
			// prevent concurrent Connect/Disconnect calls
			lock( m_ConnectLock ) {
				if( m_IsConnected ) {
					return true;
				}

				bool isConnected;
				try {
					// use timeout guard to prevent hanging indefinitely
					isConnected = m_TimeoutGuard.TryExecute( () =>
					{
						return OcApiTCP.Connect( m_CncIp, m_TimeoutMs );
					}, timeoutMs: m_TimeoutMs + CncConstants.GUARD_TIMEOUT_MARGIN_MS );
				}
				catch( Exception ex ) {
					if( raiseConnectFailed ) {
						ConnectFailed?.Invoke( ex.Message );
					}
					return false;
				}

				if( !isConnected ) {
					if( raiseConnectFailed ) {
						ConnectFailed?.Invoke( null );
					}
					return false;
				}

				m_IsConnected = true;
				return true;
			}
		}

		internal void Disconnect()
		{
			lock( m_ConnectLock ) {
				try {
					OcApiTCP.DisconnectEx();
				}
				catch( Exception ) {
					// ignore
				}
				finally {
					m_IsConnected = false;
				}
			}
		}

		#endregion

		#region R Register

		public static int? ReadRRegister( int index )
		{
			if( !m_IsConnected ) {
				return null;
			}
			try {
				return (int?)Cnc.PLC.RRegister[ index ];
			}
			catch {
				return null;
			}
		}

		public static int[] ReadRRegisterRange( int startIndex, int endIndex )
		{
			if( !m_IsConnected ) {
				return null;
			}
			if( startIndex < 0 || endIndex < startIndex ) {
				return null;
			}
			try {
				List<int> values = new List<int>();
				for( int i = startIndex; i <= endIndex; i++ ) {
					values.Add( Cnc.PLC.RRegister[ i ] );
				}
				return values.ToArray();
			}
			catch {
				return null;
			}
		}

		public static bool WriteRRegister( int index, int value )
		{
			if( !m_IsConnected ) {
				return false;
			}
			try {
				Cnc.PLC.RRegister[ index ] = value;
				return true;
			}
			catch {
				return false;
			}
		}

		public static bool WriteRRegisterAndCheckDone( int index, int value )
		{
			if( !WriteRRegister( index, value ) ) {
				return false;
			}

			for( int count = 0; count < CncConstants.CHECK_COUNT_MAX; count++ ) {
				int? current = ReadRRegister( index );
				if( !current.HasValue ) {
					return false;
				}
				if( current.Value == value ) {
					return true;
				}
				MMI_Thread.Sleep( CncConstants.PLC_SCANNING_TIME_MS );
			}

			return false;
		}

		#endregion

		#region Global Variable (@)

		public static double? ReadGlobalVariable( int index )
		{
			if( !m_IsConnected ) {
				return null;
			}
			try {
				object GlobalVarValue = Cnc.GlobalVar[ index ];
				if( GlobalVarValue == null ) {
					return null;
				}
				if( !double.TryParse( GlobalVarValue.ToString(), out double value ) ) {
					return null;
				}
				return value;
			}
			catch {
				return null;
			}
		}

		public static bool WriteGlobalVariable( int index, double value )
		{
			if( !m_IsConnected ) {
				return false;
			}
			try {
				Cnc.GlobalVar[ index ] = value;
				return true;
			}
			catch {
				return false;
			}
		}

		#endregion

		#region CNC Status

		public static CncStatus? GetCncStatus()
		{
			if( !m_IsConnected ) {
				return null;
			}
			try {
				return ConvertECncStatusToCncStatus( Cnc.State.CncStatus );
			}
			catch {
				return null;
			}
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
				default:
					return CncStatus.NOTREADY;
			}
		}

		#endregion

		#region CNC Operations

		public static bool CycleStart()
		{
			if( !m_IsConnected ) {
				return false;
			}
			try {
				Cnc.SWP.CycleStart();
				return true;
			}
			catch {
				return false;
			}
		}

		public static bool FeedHold()
		{
			if( !m_IsConnected ) {
				return false;
			}
			try {
				Cnc.SWP.FeedHold();
				return true;
			}
			catch {
				return false;
			}
		}

		public static bool ResetCnc()
		{
			if( !m_IsConnected ) {
				return false;
			}
			try {
				Cnc.SWP.ResetCnc();
				return true;
			}
			catch {
				return false;
			}
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

		// raised when connect attempt fails, carries error message for logging
		internal event Action<string> ConnectFailed;

		// Syntec OpenCNC API wrapper
		readonly CncApiTimeoutGuard m_TimeoutGuard;
		readonly string m_CncIp;
		readonly int m_TimeoutMs;
		readonly SyntecAPI.Cnc m_SyntecCnc;

		// lock to prevent concurrent Connect/Disconnect calls
		readonly object m_ConnectLock = new object();

		// shared connection flag; only one CNC connection exists at a time
		static bool m_IsConnected;
	}
}
