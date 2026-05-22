using System;
using System.Windows.Forms;
using MyCAM.App;

namespace MyCAM.Editor.Dialog
{
	public partial class GlassDXFImportDlg : Form
	{
		public double Radius { get; private set; }
		public double SurfaceHeight { get; private set; }
		public bool Accepted { get; private set; }

		public GlassDXFImportDlg()
		{
			InitializeComponent();
		}

		void BtnOK_Click( object sender, EventArgs e )
		{
			double radius = (double)m_tbxRadius.Value;
			if( radius < 0 || radius > MAX_RADIUS ) {
				MyApp.Logger.ShowOnLogPanel( $"圓球半徑需介於 0 ~ {MAX_RADIUS} mm", MyApp.NoticeType.Warning, true );
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

			double height = (double)m_tbxHeight.Value;
			if( height <= 0 || height >= radius ) {
				MyApp.Logger.ShowOnLogPanel( $"曲面高度需為大於 0 且小於 R({radius}) 的正數", MyApp.NoticeType.Warning, true );
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
	}
}
