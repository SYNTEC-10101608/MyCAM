using System;

namespace MyCAM.Editor.Dialog
{
	public partial class FlyCutDlg : EditDialogBase<object>
	{
		public Action AddFlyCut;
		public Action RemoveFlyCut;
		public Action<bool, bool> PositionChanged;
		public Action<bool> DirectionChanged;
		public Action<double> GroupWidthChanged;

		public FlyCutDlg()
		{
			InitializeComponent();
			m_nudGroupWidth.Minimum = 0;

		}

		public void ResetParam( FlyCutParam param )
		{
			if( param == null ) {
				return;
			}

			m_bSuppressValueChanged = true;
			m_Param = param;

			m_rbtnLeftTop.Checked = m_Param.IsLeftOrRight && m_Param.IsTopOrBottom;
			m_rbtnLeftBottom.Checked = m_Param.IsLeftOrRight && !m_Param.IsTopOrBottom;
			m_rbtnRightTop.Checked = !m_Param.IsLeftOrRight && m_Param.IsTopOrBottom;
			m_rbtnRightBottom.Checked = !m_Param.IsLeftOrRight && !m_Param.IsTopOrBottom;

			m_rbtnVertical.Checked = m_Param.IsVerticalOrHorizontal;
			m_rbtnHorizontal.Checked = !m_Param.IsVerticalOrHorizontal;

			m_nudGroupWidth.SilentSetValue = (decimal)m_Param.GroupWidth;
			m_lblGroupWidth.Text = m_Param.IsGroupWidthAngle
				? m_LanguageManager.TryGetString( "GroupAngle", GROUP_ANGLE )
				: m_LanguageManager.TryGetString( "GroupWidth", GROUP_WIDTH );

			m_bSuppressValueChanged = false;
		}

		void m_btnAdd_Click( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			AddFlyCut?.Invoke();
		}

		void m_btnRemove_Click( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			RemoveFlyCut?.Invoke();
		}

		void StartPosition_CheckChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			bool isLeftOrRight = m_rbtnLeftTop.Checked || m_rbtnLeftBottom.Checked;
			bool isTopOrBottom = m_rbtnLeftTop.Checked || m_rbtnRightTop.Checked;

			PositionChanged?.Invoke( isLeftOrRight, isTopOrBottom );
		}

		void Direction_CheckChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			bool isVerticalOrHorizontal = m_rbtnVertical.Checked;

			DirectionChanged?.Invoke( isVerticalOrHorizontal );
		}

		void m_nudGroupWidth_ValueChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			GroupWidthChanged?.Invoke( (double)m_nudGroupWidth.Value );
		}

		bool m_bSuppressValueChanged = false;
		FlyCutParam m_Param = null;

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "FlyCutDlg" );

		const string GROUP_ANGLE = "分組角度";
		const string GROUP_WIDTH = "分組間距";

	}

	public class FlyCutParam
	{
		public bool IsLeftOrRight
		{
			get;
			set;
		}

		public bool IsTopOrBottom
		{
			get;
			set;
		}

		public bool IsVerticalOrHorizontal
		{
			get;
			set;
		}

		public double GroupWidth
		{
			get;
			set;
		}

		public bool IsGroupWidthAngle
		{
			get;
			set;
		}

		public FlyCutParam( bool isLeftOrRight, bool isTopOrBottom, bool isVerticalOrHorizontal, double groupWidth, bool isGroupWidthAngle )
		{
			IsLeftOrRight = isLeftOrRight;
			IsTopOrBottom = isTopOrBottom;
			IsVerticalOrHorizontal = isVerticalOrHorizontal;
			GroupWidth = groupWidth;
			IsGroupWidthAngle = isGroupWidthAngle;
		}
	}
}
