using System;
using MyCAM.App;

namespace MyCAM.Editor.Dialog
{
	public partial class GlassDXFImportDlg : FormBase
	{
		public double Radius { get; private set; }
		public double SurfaceHeight { get; private set; }
		public bool Accepted { get; private set; }

		public GlassDXFImportDlg()
		{
			InitializeComponent();
			m_LanguageManager = new LanguageManager.LanguageManager( "GlassDXFImportDlg" );
		}

		void BtnOK_Click( object sender, EventArgs e )
		{
			double radius = (double)m_numRadius.Value;
			if( radius < 0 || radius > MAX_RADIUS ) {

				string message = string.Format( m_LanguageManager.TryGetString( "RadiusRangeError", RADIUS_RANGE_ERROR ), MAX_RADIUS );
				MyApp.Logger.ShowOnLogPanel( message, MyApp.NoticeType.Warning, true );
				return;
			}

			if( radius == 0 ) {
				// Flat plane mode: height is irrelevant
				Radius = 0;
				SurfaceHeight = 0;
				Accepted = true;
				Close();
				return;
			}

			double height = (double)m_numHeight.Value;
			if( height <= 0 || height >= radius ) {

				string message = string.Format( m_LanguageManager.TryGetString( "HeightRangeError", HEIGHT_RANGE_ERROR ), radius );
				MyApp.Logger.ShowOnLogPanel( message, MyApp.NoticeType.Warning, true );
				return;
			}

			Radius = radius;
			SurfaceHeight = height;
			Accepted = true;
			Close();
		}

		void BtnCancel_Click( object sender, EventArgs e )
		{
			Accepted = false;
			Close();
		}

		const double MAX_RADIUS = 10000.0;
		const string RADIUS_RANGE_ERROR = "圓球半徑需介於 0 ~ {0} mm";
		const string HEIGHT_RANGE_ERROR = "曲面高度需為大於 0 且小於 R({0}) 的正數";
		readonly LanguageManager.LanguageManager m_LanguageManager;
	}
}
