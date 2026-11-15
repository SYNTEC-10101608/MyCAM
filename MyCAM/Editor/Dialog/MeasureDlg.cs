using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal partial class MeasureDlg : EditDialogBase<EMeasureType>
	{
		public MeasureDlg()
		{
			InitializeComponent();
			InitMethodComboBox();
		}

		public Action<EMeasureType> MeasureMethodChanged;

		public void Initialize()
		{
			EMeasureType defaultType = EMeasureType.PointCoordinate;

			// temporarily unsubscribe to prevent triggering event during initialization
			m_cmbMethod.SelectedIndexChanged -= m_cmbMethod_SelectedIndexChanged;

			m_cmbMethod.SelectedIndex = (int)defaultType;
			m_lblGuide.Text = GetGuideText( defaultType );
			m_txtResult.Text = string.Empty;

			// re-subscribe event handler
			m_cmbMethod.SelectedIndexChanged += m_cmbMethod_SelectedIndexChanged;

			// trigger business logic once in a unified manner
			MeasureMethodChanged?.Invoke( defaultType );
		}

		public void ShowResult( string result )
		{
			m_txtResult.Text = result;
		}

		public void ClearResult()
		{
			m_txtResult.Text = string.Empty;
		}

		protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
		{
			if( keyData == Keys.Escape ) {
				Close();
				return true;
			}
			return base.ProcessCmdKey( ref msg, keyData );
		}

		void InitMethodComboBox()
		{
			m_cmbMethod.Items.Add( m_LanguageManager.TryGetString( "Method_PointCoordinate", METHOD_POINT_COORDINATE ) );
			m_cmbMethod.Items.Add( m_LanguageManager.TryGetString( "Method_LineLength", METHOD_LINE_LENGTH ) );
			m_cmbMethod.Items.Add( m_LanguageManager.TryGetString( "Method_CircleArcRadius", METHOD_CIRCLE_ARC_RADIUS ) );
			m_cmbMethod.Items.Add( m_LanguageManager.TryGetString( "Method_DistanceBetweenTwoPoints", METHOD_DISTANCE_BETWEEN_TWO_POINTS ) );
			m_cmbMethod.Items.Add( m_LanguageManager.TryGetString( "Method_AngleBetweenLines", METHOD_ANGLE_BETWEEN_LINES ) );
		}

		void m_cmbMethod_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( m_cmbMethod.SelectedIndex < 0 ) {
				return;
			}

			EMeasureType type = (EMeasureType)m_cmbMethod.SelectedIndex;
			m_lblGuide.Text = GetGuideText( type );
			ClearResult();

			// notify action to switch measure mode
			MeasureMethodChanged?.Invoke( type );
		}

		string GetGuideText( EMeasureType type )
		{
			switch( type ) {
				case EMeasureType.PointCoordinate:
					return m_LanguageManager.TryGetString( "Guide_PointCoordinate", GUIDE_POINT_COORDINATE );
				case EMeasureType.LineLength:
					return m_LanguageManager.TryGetString( "Guide_LineLength", GUIDE_LINE_LENGTH );
				case EMeasureType.CircleArcRadius:
					return m_LanguageManager.TryGetString( "Guide_CircleArcRadius", GUIDE_CIRCLE_ARC_RADIUS );
				case EMeasureType.DistanceBetweenTwoPoints:
					return m_LanguageManager.TryGetString( "Guide_DistanceBetweenTwoPoints", GUIDE_DISTANCE_BETWEEN_TWO_POINTS );
				case EMeasureType.AngleBetweenLines:
					return m_LanguageManager.TryGetString( "Guide_AngleBetweenLines", GUIDE_ANGLE_BETWEEN_LINES );
				default:
					return m_LanguageManager.TryGetString( "m_lblGuide_Default", GUIDE_DEFAULT );
			}
		}

		const string GUIDE_POINT_COORDINATE = "請點擊一個工件上的頂點，以量測其座標";
		const string GUIDE_LINE_LENGTH = "請點擊一條邊，以量測其長度";
		const string GUIDE_CIRCLE_ARC_RADIUS = "請點擊一個圓或圓弧邊，以量測其半徑";
		const string GUIDE_DISTANCE_BETWEEN_TWO_POINTS = "請依序點擊兩個頂點，以量測其距離";
		const string GUIDE_ANGLE_BETWEEN_LINES = "請依序點擊兩條線段，以量測其夾角";
		const string GUIDE_DEFAULT = "請選擇量測方式";
		const string METHOD_POINT_COORDINATE = "點的座標";
		const string METHOD_LINE_LENGTH = "線段長度";
		const string METHOD_CIRCLE_ARC_RADIUS = "圓弧半徑";
		const string METHOD_DISTANCE_BETWEEN_TWO_POINTS = "兩點距離";
		const string METHOD_ANGLE_BETWEEN_LINES = "兩線夾角";

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "MeasureDlg" );
	}
}
