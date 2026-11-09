using MyCAM.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal partial class ExternalAxisDlg : EditDialogBase<ExternalAxisEditData>
	{
		public ExternalAxisDlg( ExternalAxisType type, List<string> pathIDs, List<string> pathNames, List<ExternalAxisPos> positions )
		{
			if( type != ExternalAxisType.Single && type != ExternalAxisType.Dual ) {
				throw new ArgumentException( "An enabled external axis configuration is required." );
			}
			if( pathIDs == null || pathIDs.Count == 0 || pathNames == null || positions == null
				|| pathIDs.Count != pathNames.Count || pathIDs.Count != positions.Count ) {
				throw new ArgumentException( "The selected paths, names and positions must correspond." );
			}
			m_Type = type;
			m_PathIDs = new List<string>( pathIDs );
			InitializeComponent();
			ApplyResourcesToUI();
			m_lblExt2.Visible = m_txbExt2.Visible = type == ExternalAxisType.Dual;
			m_txbExt1.Text = CommonAngleText( positions.Select( pos => pos.Ext1 ) );
			m_txbExt2.Text = CommonAngleText( positions.Select( pos => pos.Ext2 ) );
			m_cbxPreviewPath.Items.AddRange( pathNames.Cast<object>().ToArray() );
			m_cbxPreviewPath.SelectedIndex = 0;
		}

		void ApplyResourcesToUI()
		{
			Text = m_LanguageManager.TryGetString( "ExternalAxisDlg", Text );
			m_lblHint.Text = m_LanguageManager.TryGetString( "ExternalAxisHint", m_lblHint.Text );
			m_lblPreviewPath.Text = m_LanguageManager.TryGetString( "ExternalAxisPreviewPath", m_lblPreviewPath.Text );
			m_btnPreview.Text = m_LanguageManager.TryGetString( "m_btnPreview", m_btnPreview.Text );
			m_btnConfirm.Text = m_LanguageManager.TryGetString( "m_btnConfirm", m_btnConfirm.Text );
			m_btnCancel.Text = m_LanguageManager.TryGetString( "m_btnCancel", m_btnCancel.Text );
		}

		static string CommonAngleText( IEnumerable<double> angles )
		{
			double[] values = angles.Distinct().ToArray();
			return values.Length == 1 ? values[ 0 ].ToString( "G17", CultureInfo.CurrentCulture ) : string.Empty;
		}

		static bool TryParseAngle( string text, out double? angle )
		{
			angle = null;
			if( string.IsNullOrWhiteSpace( text ) ) {
				return true;
			}
			if( !double.TryParse( text, NumberStyles.Float, CultureInfo.CurrentCulture, out double value )
				|| double.IsNaN( value ) || double.IsInfinity( value ) ) {
				return false;
			}
			angle = value;
			return true;
		}

		bool TryGetEditData( out ExternalAxisEditData data )
		{
			data = null;
			double? ext2 = null;
			bool valid = TryParseAngle( m_txbExt1.Text, out double? ext1 );
			if( m_Type == ExternalAxisType.Dual ) {
				valid = TryParseAngle( m_txbExt2.Text, out ext2 ) && valid;
			}
			if( !valid ) {
				m_lblError.Text = m_LanguageManager.TryGetString( "ExternalAxisFiniteAngle", "請輸入有限角度數值；留白則保留各路徑原值。" );
				return false;
			}
			m_lblError.Text = string.Empty;
			data = new ExternalAxisEditData( ext1, ext2, m_PathIDs[ m_cbxPreviewPath.SelectedIndex ] );
			return true;
		}

		void m_btnPreview_Click( object sender, EventArgs e )
		{
			if( TryGetEditData( out ExternalAxisEditData data ) ) {
				RaisePreview( data );
			}
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			if( TryGetEditData( out ExternalAxisEditData data ) ) {
				RaiseConfirm( data );
			}
		}

		void m_btnCancel_Click( object sender, EventArgs e )
		{
			Close();
		}

		readonly ExternalAxisType m_Type;
		readonly List<string> m_PathIDs;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ExternalAxisDlg" );
	}
}
