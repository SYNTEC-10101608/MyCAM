using System;
using System.Windows.Forms;
using CommonUI.Controls;
using CommonUI.Forms;

namespace CncUI.Dialog
{
	public partial class CNCOffsetDialog : DialogBase
	{
		public CNCOffsetDialog()
		{
			InitializeComponent();
			ApplyLanguage();
			m_AxisNames = new[] { m_lblX, m_lblY, m_lblZ, m_lblRotary1, m_lblRotary2 };
			m_Coordinates = new[] { m_lblXValue, m_lblYValue, m_lblZValue, m_lblRotary1Value, m_lblRotary2Value };
			m_ZeroButtons = new[] { m_btnXZero, m_btnYZero, m_btnZZero, m_btnRotary1Zero, m_btnRotary2Zero };
			for( int index = 0; index < m_ZeroButtons.Length; index++ ) {
				int slot = index;
				m_ZeroButtons[ index ].Click += ( sender, e ) => SetZeroClicked?.Invoke( slot );
			}
			m_btnClear.Click += ( sender, e ) => ClearClicked?.Invoke();
			ApplyResources();
		}

		internal LanguageManager.LanguageManager Language { get; } = new LanguageManager.LanguageManager( "CNCOffsetDialog" );

		public void ApplyResources()
		{
			Text = Language.TryGetString( "Title", TITLE );
			foreach( ThemedButton button in m_ZeroButtons ) {
				button.Text = Language.TryGetString( "SetZero", SET_ZERO );
			}
			m_btnClear.Text = Language.TryGetString( "ClearAll", CLEAR_ALL );
		}

		internal event Action<int> SetZeroClicked;
		internal event Action ClearClicked;

		internal void SetAxis( int slot, string name, string coordinate )
		{
			m_AxisNames[ slot ].Text = name ?? "—";
			m_Coordinates[ slot ].Text = coordinate;
		}

		internal void SetCommandsEnabled( bool enabled )
		{
			foreach( ThemedButton button in m_ZeroButtons ) {
				button.Enabled = enabled;
			}
			m_btnClear.Enabled = enabled;
		}

		readonly Label[] m_AxisNames;
		readonly Label[] m_Coordinates;
		readonly ThemedButton[] m_ZeroButtons;

		const string TITLE = "程式座標偏置";
		internal const string SET_ZERO = "設為 0";
		internal const string CLEAR_ALL = "全部清除";
	}
}
