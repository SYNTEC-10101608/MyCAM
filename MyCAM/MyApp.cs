using CommonData;
using FileTranslator;
using MyCAM.Editor;
using MyCAM.LogManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VNCFormsApp;

namespace MyCAM.App
{
	internal static class MyApp
	{
		static string m_szCNCIP = string.Empty;

		const string CUST_post_file_Name = "CustPost.cpf";

		public enum NoticeType
		{
			Hint,
			Warning,
			Error
		}

		public static string CNCIP
		{
			get
			{
				return m_szCNCIP;
			}
			set
			{
				m_szCNCIP = value;
			}
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

		public static VNCUserControl VNCScreen
		{
			get => m_VNCUserControl;
			set
			{
				if( m_VNCUserControl != null ) {
					m_VNCUserControl.CloseVNCProcess();
					m_VNCUserControl.Dispose();
				}
				m_VNCUserControl = value;
			}
		}

		public static CustPost CustomizedPostInfo
		{
			get => m_CustPost;
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

		#region Read Customized Post File

		public static void SetCustPost()
		{
			// read cust post file
			string filePath = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, CUST_post_file_Name );
			if( !System.IO.File.Exists( filePath ) ) {
				Logger.ShowOnLogPanel( $"客製後處理檔案不存在", NoticeType.Hint );

				// use default value
				m_CustPost = new CustPost();
				return;
			}
			string fileContent;

			// protect io exception
			try {
				fileContent = System.IO.File.ReadAllText( filePath );
			}
			catch( Exception ) {
				Logger.ShowOnLogPanel( $"讀取客製化後處理器檔案失敗", NoticeType.Error );

				// use default value
				m_CustPost = new CustPost();
				return;
			}

			// translate to DTO
			TxtToCustPostTranslator custPostTranslater = new TxtToCustPostTranslator();
			bool isGetAllCustContent = custPostTranslater.Translate( fileContent, out CustPost post );
			if( isGetAllCustContent == false ) {
				Logger.ShowOnLogPanel( $"客製化後處理器檔案內容不完整或格式錯誤，請確認檔案內容。", NoticeType.Error, true );
			}

			// store info
			m_CustPost = post;
		}

		#endregion

		static Form m_MainForm;
		static LogHandler m_Logger;
		static Panel m_LogPanel;
		static VNCUserControl m_VNCUserControl;
		static readonly List<DialogEventInfo> m_RegisteredDialogs = new List<DialogEventInfo>();
		static CustPost m_CustPost = null;

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