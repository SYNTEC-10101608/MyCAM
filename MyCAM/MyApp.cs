using MyCAM.Editor;
using MyCAM.LogManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MyCAM.App
{
	internal static class MyApp
	{
		public enum NoticeType
		{
			Hint,
			Warning,
			Error
		}

		public static Form MainForm
		{
			get => m_MainForm;
			set
			{
				m_MainForm = value;

				if( value is StartupForm f ) {

					// get main form log panel
					m_LogPanel = f.GetLogPanel;

					// send panel to logger
					m_Logger = new LogHandler( m_LogPanel );
				}
			}
		}

		public static LogHandler Logger
		{
			get
			{
				if( m_Logger == null ) {
					throw new InvalidOperationException( "Logger還沒初始化，請先設定 MainForm。" );
				}
				return m_Logger;
			}
		}

		#region Show Dialog at Form Center

		public static void RegistDialogMoveWithMainForm<TEditDataType>( EditDialogBase<TEditDataType> dialog )
		{
			// do not have main form
			if( m_MainForm == null || dialog == null ) {
				return;
			}

			// already registered
			if( m_RegisteredDialogs.Exists( info => info.Dialog == dialog ) ) {
				return;
			}

			// create event handler
			EventHandler locationChangedHandler = ( sender, e ) =>
			{
				if( !dialog.IsDisposed && dialog.Visible ) {
					dialog.OnParentLocationChange();
				}
			};
			EventHandler sizeChangedHandler = ( sender, e ) =>
			{
				if( !dialog.IsDisposed && dialog.Visible ) {
					dialog.OnParentLocationChange();
				}
			};
			EventHandler mainFormActivatedHandler = ( sender, e ) =>
			{
				if( !dialog.IsDisposed && dialog.Visible ) {
					m_MainForm.BeginInvoke( new Action( () =>
					{
						dialog.EnsureDialogOnTop();
					} ) );
				}
			};

			// regist main form event
			m_MainForm.LocationChanged += locationChangedHandler;
			m_MainForm.SizeChanged += sizeChangedHandler;
			m_MainForm.Activated += mainFormActivatedHandler;

			// save event info
			DialogEventInfo eventInfo = new DialogEventInfo
			{
				Dialog = dialog,
				LocationChangedHandler = locationChangedHandler,
				SizeChangedHandler = sizeChangedHandler,
				MainFormActivatedHandler = mainFormActivatedHandler
			};
			m_RegisteredDialogs.Add( eventInfo );

			// when dialog closed, unregister it
			dialog.FormClosed += ( sender, e ) => UnRegistDialogMoveWithMainForm( dialog );
		}

		public static Point CalculateDialogCenterLocation( Form dialog )
		{
			if( m_MainForm == null ) {
				return Point.Empty;
			}

			// cal new form center
			FromSizeInfo mainFormInfo = GetFormSizeInfo();
			int centerX = mainFormInfo.FormCenterX + (int)( ( mainFormInfo.FormWidth - dialog.Width ) / 2 );
			int centerY = mainFormInfo.FormCenterY + (int)( ( mainFormInfo.FormHeight - dialog.Height ) / 2 );

			// make sure dialog is within working area
			Screen screen = GetMainFormScreen();
			Rectangle workingArea = screen.WorkingArea;
			centerX = Math.Max( workingArea.Left,
					  Math.Min( centerX, workingArea.Right - dialog.Width ) );
			centerY = Math.Max( workingArea.Top,
					  Math.Min( centerY, workingArea.Bottom - dialog.Height ) );
			if( workingArea.Width < workingArea.Height ) {
				double offsetRatio = 0.5;
				centerY = (int)( offsetRatio * centerY );
			}
			return new Point( centerX, centerY );
		}

		static FromSizeInfo GetFormSizeInfo()
		{
			if( m_MainForm == null ) {
				throw new InvalidOperationException( "MainForm還沒初始化，請先設定 MainForm。" );
			}
			FromSizeInfo mainFormInfo = new FromSizeInfo();
			mainFormInfo.FormWidth = m_MainForm.Width;
			mainFormInfo.FormHeight = m_MainForm.Height;
			mainFormInfo.FormCenterX = m_MainForm.Location.X;
			mainFormInfo.FormCenterY = m_MainForm.Location.Y;

			return mainFormInfo;
		}

		static Screen GetMainFormScreen()
		{
			if( m_MainForm == null ) {
				throw new InvalidOperationException( "MainForm還沒初始化，請先設定 MainForm。" );
			}
			return Screen.FromControl( m_MainForm );
		}

		static void UnRegistDialogMoveWithMainForm( Form dialog )
		{
			if( m_MainForm == null || dialog == null ) {
				return;
			}

			// find registered event info
			DialogEventInfo eventInfo = m_RegisteredDialogs.Find( info => info.Dialog == dialog );
			if( eventInfo == null ) {
				return;
			}

			// unsubscribe event
			m_MainForm.LocationChanged -= eventInfo.LocationChangedHandler;
			m_MainForm.SizeChanged -= eventInfo.SizeChangedHandler;
			m_MainForm.Activated -= eventInfo.MainFormActivatedHandler;

			// remove from registered list
			m_RegisteredDialogs.Remove( eventInfo );
		}

		#endregion

		static Form m_MainForm;
		static LogHandler m_Logger;
		static Panel m_LogPanel;
		static readonly List<DialogEventInfo> m_RegisteredDialogs = new List<DialogEventInfo>();

		class DialogEventInfo
		{
			public Form Dialog
			{
				get; set;
			}

			public EventHandler LocationChangedHandler
			{
				get; set;
			}

			public EventHandler SizeChangedHandler
			{
				get; set;
			}

			public EventHandler MainFormActivatedHandler
			{
				get; set;
			}
		}

		struct FromSizeInfo
		{
			public double FormWidth;
			public double FormHeight;
			public int FormCenterX;
			public int FormCenterY;
		}

	}
}