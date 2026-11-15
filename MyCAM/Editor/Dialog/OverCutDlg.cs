using System;
using MyCAM.App;

namespace MyCAM.Editor.Dialog
{
	public partial class OverCutDlg : EditDialogBase<double>
	{
		public OverCutDlg( double overCutLength )
		{
			InitializeComponent();
			m_OverCutLength = overCutLength;
			m_tbxOverCutLength.SilentSetValue = (decimal)m_OverCutLength;
		}

		public Func<double, bool> CheckValueGeomRestriction;

		void m_tbxOverCutLength_DebouncedValueChanged( object sender, EventArgs e )
		{
			PreviewOverCutResult();
		}

		void m_btnSure_Click( object sender, EventArgs e )
		{
			if( IsValidParam() ) {
				RaiseConfirm( m_OverCutLength );
			}
		}

		void PreviewOverCutResult()
		{
			if( IsValidParam() ) {
				RaisePreview( m_OverCutLength );
			}
		}

		bool IsValidParam()
		{
			double dOverCutLength = (double)m_tbxOverCutLength.Value;
			if( dOverCutLength < 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_NegativeLength", ERROR_NEGATIVE_LENGTH ), MyApp.NoticeType.Warning );
				return false;
			}
			if( CheckValueGeomRestriction?.Invoke( dOverCutLength ) == false ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_ExceedGeometry", ERROR_EXCEED_GEOMETRY ), MyApp.NoticeType.Warning );
				return false;
			}
			m_OverCutLength = dOverCutLength;
			return true;
		}

		double m_OverCutLength = 0;

		const string ERROR_NEGATIVE_LENGTH = "長度需要大於0";
		const string ERROR_EXCEED_GEOMETRY = "過切長度超出幾何限制";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "OverCutDlg" );
	}
}
