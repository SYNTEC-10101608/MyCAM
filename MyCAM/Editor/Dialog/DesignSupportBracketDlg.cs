using MyCAM.App;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class DesignSupportBracketDlg : EditDialogBase<double>
	{
		internal const int MaxPositionsPerAxis = 30;

		internal Action NextStep;
		internal Action BackStep;
		internal Action<SupportBracketParams> Generate;
		internal Action<SupportBracketParams> ParameterChanged;
		internal Action<string> SaveDxf;
		internal Action SkipSave;

		public DesignSupportBracketDlg()
		{
			InitializeComponent();
			m_LanguageManager = new LanguageManager.LanguageManager( "DesignSupportBracketDlg" );
			ApplyResourcesToUI();
			LoadImages();
			RegisterDebouncedHandlers();
			ShowSelectFacePanel();
		}

		internal string XExtensionName => m_lblXExtension.Text;

		internal string YExtensionName => m_lblYExtension.Text;

		internal string BaseThicknessName => m_lblBaseThickness.Text;

		internal string EngagementDepthName => m_lblEngagementDepth.Text;

		internal string XPinCountName => m_lblXPinCount.Text;

		internal string YPinCountName => m_lblYPinCount.Text;

		internal string WallThicknessName => m_lblWallThickness.Text;

		internal string RibHeightName => m_lblRibHeight.Text;

		internal string ZExtensionName => m_lblZExtension.Text;

		internal string PinLengthName => m_lblPinLength.Text;

		void ApplyResourcesToUI()
		{
			Text = m_LanguageManager.TryGetString( "DesignSupportBracketDlg", Text );
			m_lblHint.Text = m_LanguageManager.TryGetString( "m_lblHint", m_lblHint.Text );
			m_btnNext.Text = m_LanguageManager.TryGetString( "m_btnNext", m_btnNext.Text );
			m_lblSectionBase.Text = m_LanguageManager.TryGetString( "m_lblSectionBase", m_lblSectionBase.Text );
			m_lblXExtension.Text = m_LanguageManager.TryGetString( "m_lblXExtension", m_lblXExtension.Text );
			m_lblYExtension.Text = m_LanguageManager.TryGetString( "m_lblYExtension", m_lblYExtension.Text );
			m_lblSectionBracket.Text = m_LanguageManager.TryGetString( "m_lblSectionBracket", m_lblSectionBracket.Text );
			m_lblXPositionsTitle.Text = m_LanguageManager.TryGetString( "m_lblXPositionsTitle", m_lblXPositionsTitle.Text );
			m_lblYPositionsTitle.Text = m_LanguageManager.TryGetString( "m_lblYPositionsTitle", m_lblYPositionsTitle.Text );
			m_btnAddXPosition.Text = m_LanguageManager.TryGetString( "m_btnAddXPosition", m_btnAddXPosition.Text );
			m_btnAddYPosition.Text = m_LanguageManager.TryGetString( "m_btnAddYPosition", m_btnAddYPosition.Text );
			m_lblSectionBracketHeight.Text = m_LanguageManager.TryGetString( "m_lblSectionBracketHeight", m_lblSectionBracketHeight.Text );
			m_lblBaseThickness.Text = m_LanguageManager.TryGetString( "m_lblBaseThickness", m_lblBaseThickness.Text );
			m_lblEngagementDepth.Text = m_LanguageManager.TryGetString( "m_lblEngagementDepth", m_lblEngagementDepth.Text );
			m_lblXPinCount.Text = m_LanguageManager.TryGetString( "m_lblXPinCount", m_lblXPinCount.Text );
			m_lblYPinCount.Text = m_LanguageManager.TryGetString( "m_lblYPinCount", m_lblYPinCount.Text );
			m_lblWallThickness.Text = m_LanguageManager.TryGetString( "m_lblWallThickness", m_lblWallThickness.Text );
			m_lblRibHeight.Text = m_LanguageManager.TryGetString( "m_lblRibHeight", m_lblRibHeight.Text );
			m_lblZExtension.Text = m_LanguageManager.TryGetString( "m_lblZExtension", m_lblZExtension.Text );
			m_lblPinLength.Text = m_LanguageManager.TryGetString( "m_lblPinLength", m_lblPinLength.Text );
			m_lblSectionCrossFit.Text = m_LanguageManager.TryGetString( "m_lblSectionCrossFit", m_lblSectionCrossFit.Text );
			m_lblSectionBracketCount.Text = m_LanguageManager.TryGetString( "m_lblSectionBracketCount", m_lblSectionBracketCount.Text );
			m_lblSectionBottomPinSize.Text = m_LanguageManager.TryGetString( "m_lblSectionBottomPinSize", m_lblSectionBottomPinSize.Text );
			m_lblSectionBracketHeight.Text = m_LanguageManager.TryGetString( "m_lblSectionBracketHeight", m_lblSectionBracketHeight.Text );
			m_btnBack.Text = m_LanguageManager.TryGetString( "m_btnBack", m_btnBack.Text );
			m_btnGenerate.Text = m_LanguageManager.TryGetString( "m_btnGenerate", m_btnGenerate.Text );
			m_lblSaveDxfHint.Text = m_LanguageManager.TryGetString( "m_lblSaveDxfHint", m_lblSaveDxfHint.Text );
			m_btnBrowseSavePath.Text = m_LanguageManager.TryGetString( "m_btnBrowseSavePath", m_btnBrowseSavePath.Text );
			m_btnSaveDxf.Text = m_LanguageManager.TryGetString( "m_btnSaveDxf", m_btnSaveDxf.Text );
			m_btnSkipSave.Text = m_LanguageManager.TryGetString( "m_btnSkipSave", m_btnSkipSave.Text );
		}

		void LoadImages()
		{
			m_picCrossGap.Image = Properties.Resources.Support_CrossGap;
			m_picPinDimension.Image = Properties.Resources.Support_PinDimension;
		}

		internal void ResetPositionListsFromBBox( double[] xDefaults, double[] yDefaults )
		{
			// Remove existing rows
			foreach( var row in m_XRows ) {
				row.DeleteRequested -= OnPositionRowDeleteRequested;
				row.ValueCommitted -= OnPositionRowValueCommitted;
				row.ValueRejected -= OnPositionRowValueRejected;
				row.Dispose();
			}

			foreach( var row in m_YRows ) {
				row.DeleteRequested -= OnPositionRowDeleteRequested;
				row.ValueCommitted -= OnPositionRowValueCommitted;
				row.ValueRejected -= OnPositionRowValueRejected;
				row.Dispose();
			}

			m_XRows.Clear();
			m_YRows.Clear();
			m_pnlXPositions.Controls.Clear();
			m_pnlYPositions.Controls.Clear();
			InitializePositionListsCore( xDefaults, yDefaults );
		}

		void InitializePositionListsCore( double[] xDefaults, double[] yDefaults )
		{
			foreach( double v in xDefaults ) {
				AddPositionRow( ref m_XRows, m_pnlXPositions, v, System.Drawing.Color.DarkGreen );
			}

			foreach( double v in yDefaults ) {
				AddPositionRow( ref m_YRows, m_pnlYPositions, v, System.Drawing.Color.RoyalBlue );
			}

			RefreshRowIndicesAndLayout( m_XRows, m_pnlXPositions );
			RefreshRowIndicesAndLayout( m_YRows, m_pnlYPositions );
			UpdateAddButtonsEnabled();
		}

		internal void ShowSelectFacePanel()
		{
			ShowPanel( m_pnlSelectFace );
		}

		internal void ShowInputParametersPanel()
		{
			ShowPanel( m_pnlInputParameters );
		}

		internal void ShowSaveDxfPanel( string defaultOutputDir )
		{
			m_txbSavePath.Text = defaultOutputDir;
			ShowPanel( m_pnlSaveDxf );
		}

		void ShowPanel( Panel activePanel )
		{
			SuspendLayout();

			// Hide all panels first so the window resizes without showing stale content
			m_pnlSelectFace.Visible = false;
			m_pnlInputParameters.Visible = false;
			m_pnlSaveDxf.Visible = false;

			ClientSize = new System.Drawing.Size(
				activePanel.Right + activePanel.Left,
				activePanel.Bottom + activePanel.Top );
			ResumeLayout( false );
			PerformLayout();

			// Show the target panel only after the window has been resized
			activePanel.Visible = true;
			Refresh();
		}

		internal void TriggerInitialParameterChanged()
		{
			RaiseParameterChanged();
		}

		internal void SetFaceSelected( bool hasSelection )
		{
			m_btnNext.Enabled = hasSelection;
		}

		SupportBracketParams CollectParams()
		{
			return new SupportBracketParams
			{
				XExtension = (double)m_txbXExtension.Value,
				YExtension = (double)m_txbYExtension.Value,
				BaseThick = (double)m_txbBaseThickness.Value,
				XPositions = CollectPositions( m_XRows ),
				YPositions = CollectPositions( m_YRows ),
				CrossGap = (double)m_txbEngagementDepth.Value,
				XPinCount = (int)m_txbXPinCount.Value,
				YPinCount = (int)m_txbYPinCount.Value,
				WallThick = (double)m_txbWallThickness.Value,
				RibHeight = (double)m_txbRibHeight.Value,
				ZExtension = (double)m_txbZExtension.Value,
				PinLength = (double)m_txbPinLength.Value,
			};
		}

		static double[] CollectPositions( List<SupportPositionRow> rows )
		{
			var list = new List<double>( rows.Count );
			foreach( var row in rows ) {
				list.Add( row.LastValidValue );
			}
			return list.ToArray();
		}

		void RaiseParameterChanged()
		{
			ParameterChanged?.Invoke( CollectParams() );
		}

		void m_btnNext_Click( object sender, EventArgs e )
		{
			NextStep?.Invoke();
		}

		void m_btnBack_Click( object sender, EventArgs e )
		{
			BackStep?.Invoke();
		}

		void m_btnGenerate_Click( object sender, EventArgs e )
		{
			Generate?.Invoke( CollectParams() );
		}

		void m_btnSaveDxf_Click( object sender, EventArgs e )
		{
			SaveDxf?.Invoke( m_txbSavePath.Text );
		}

		void m_btnSkipSave_Click( object sender, EventArgs e )
		{
			SkipSave?.Invoke();
		}

		void m_btnAddXPosition_Click( object sender, EventArgs e )
		{
			AddPositionRowAndUpdate( ref m_XRows, m_pnlXPositions, 0.0 );
		}

		void m_btnAddYPosition_Click( object sender, EventArgs e )
		{
			AddPositionRowAndUpdate( ref m_YRows, m_pnlYPositions, 0.0 );
		}

		void AddPositionRowAndUpdate( ref List<SupportPositionRow> targetRows, Panel targetPanel, double defaultValue )
		{
			if( targetRows.Count >= MaxPositionsPerAxis ) {
				return;
			}
			System.Drawing.Color color = ( targetRows == m_XRows ) ? System.Drawing.Color.DarkGreen : System.Drawing.Color.RoyalBlue;
			var row = AddPositionRow( ref targetRows, targetPanel, defaultValue, color );
			RefreshRowIndicesAndLayout( targetRows, targetPanel );
			UpdateAddButtonsEnabled();
			targetPanel.ScrollControlIntoView( row );
			row.ValueControl.Focus();
			row.ValueControl.Select( 0, row.ValueControl.Text.Length );
			RaiseParameterChanged();
		}

		SupportPositionRow AddPositionRow(
			ref List<SupportPositionRow> targetRows,
			Panel targetPanel,
			double defaultValue,
			System.Drawing.Color? labelColor = null )
		{
			string deleteText = m_LanguageManager.TryGetString( "m_btnDeletePosition", DefaultDeletePosition );
			var row = new SupportPositionRow( defaultValue, labelColor, deleteText );
			row.DeleteRequested += OnPositionRowDeleteRequested;
			row.ValueCommitted += OnPositionRowValueCommitted;
			row.ValueRejected += OnPositionRowValueRejected;
			targetRows.Add( row );
			targetPanel.Controls.Add( row );
			return row;
		}

		void OnPositionRowDeleteRequested( SupportPositionRow row )
		{
			var targetRows = m_XRows.Contains( row ) ? m_XRows : m_YRows;
			var targetPanel = ( targetRows == m_XRows ) ? m_pnlXPositions : m_pnlYPositions;

			if( targetRows.Count <= 1 ) {
				return;
			}

			targetRows.Remove( row );
			targetPanel.Controls.Remove( row );
			row.Dispose();

			RefreshRowIndicesAndLayout( targetRows, targetPanel );
			UpdateAddButtonsEnabled();
			RaiseParameterChanged();
		}

		void OnPositionRowValueCommitted( SupportPositionRow row )
		{
			RaiseParameterChanged();
		}

		void OnPositionRowValueRejected( object sender, ValueRejectedEventArgs e )
		{
			OnValueRejected( sender, e );
		}

		void RefreshRowIndicesAndLayout( List<SupportPositionRow> targetRows, Panel targetPanel )
		{
			targetPanel.SuspendLayout();

			// Reset scroll position to top-left before re-layout, so that the user sees the first row after adding/removing rows.
			targetPanel.AutoScrollPosition = new System.Drawing.Point( 0, 0 );
			int rowWidth = targetPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4;
			if( rowWidth < 200 ) {
				rowWidth = 200;
			}
			int y = 2;
			for( int i = 0; i < targetRows.Count; i++ ) {
				var row = targetRows[ i ];
				row.SetIndex( i + 1 );
				row.SetRowWidth();
				row.CanDelete = ( targetRows.Count > 1 );
				row.Location = new System.Drawing.Point( 2, y );
				y += row.Height + 2;
			}
			targetPanel.ResumeLayout();
		}

		void UpdateAddButtonsEnabled()
		{
			m_btnAddXPosition.Enabled = m_XRows.Count < MaxPositionsPerAxis;
			m_btnAddYPosition.Enabled = m_YRows.Count < MaxPositionsPerAxis;
		}

		void RegisterDebouncedHandlers()
		{
			// Preview params (affects live preview)
			foreach( DebouncedNumericUpDown nud in new[] {
					m_txbXExtension, m_txbYExtension,
					m_txbRibHeight, m_txbZExtension } ) {
				nud.DebouncedValueChanged += OnPreviewParamChanged;
			}
			foreach( DebouncedNumericUpDown nud in new[] {
					m_txbXExtension, m_txbYExtension,
					m_txbZExtension, m_txbRibHeight,
					m_txbEngagementDepth, m_txbBaseThickness,
					m_txbWallThickness, m_txbPinLength,
					m_txbXPinCount, m_txbYPinCount } ) {
				nud.RestrictInvalidCharacters = true;
				nud.ValueRejected += OnValueRejected;
			}
		}

		void OnPreviewParamChanged( object sender, EventArgs e )
		{
			RaiseParameterChanged();
		}

		void OnValueRejected( object sender, ValueRejectedEventArgs e )
		{
			string template = e.IsBelowMinimum
				? m_LanguageManager.TryGetString( "Warn_BelowMinimum", WARN_BELOW_MINIMUM )
				: m_LanguageManager.TryGetString( "Warn_AboveMaximum", WARN_ABOVE_MAXIMUM );
			string message = string.Format( template, e.InputValue, e.Limit );
			MyApp.Logger.ShowOnLogPanel( message, MyApp.NoticeType.Warning );
		}

		void m_btnBrowseSavePath_Click( object sender, EventArgs e )
		{
			using( var dlg = new FolderBrowserDialog() ) {
				dlg.Description = m_LanguageManager.TryGetString( "m_dlgBrowseSavePath_Description", DefaultBrowseSavePathDescription );
				dlg.SelectedPath = m_txbSavePath.Text;
				dlg.ShowNewFolderButton = true;
				if( dlg.ShowDialog( this ) == DialogResult.OK ) {
					m_txbSavePath.Text = dlg.SelectedPath;
				}
			}
		}

		readonly LanguageManager.LanguageManager m_LanguageManager;
		List<SupportPositionRow> m_XRows = new List<SupportPositionRow>();
		List<SupportPositionRow> m_YRows = new List<SupportPositionRow>();

		const string DefaultDeletePosition = "刪除";
		const string DefaultBrowseSavePathDescription = "選擇儲存資料夾";
		const string WARN_BELOW_MINIMUM = "輸入值 {0} 小於最小值 {1}";
		const string WARN_ABOVE_MAXIMUM = "輸入值 {0} 大於最大值 {1}";
	}
}
