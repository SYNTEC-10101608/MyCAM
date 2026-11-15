using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MyCAM.Helper;

namespace MyCAM.Editor.Dialog
{
	public partial class StartPointDlg : EditDialogBase<object>
	{
		public Action ModeChanged;
		public Action Apply;
		public Action<StartPointSelectStrategy, bool> AddOrRemoveStrategy;
		public Action<StartPointSelectStrategy, bool> IncreaseOrDecreasePriority;

		public StartPointDlg()
		{
			InitializeComponent();
			m_tbxTolernece.Maximum = decimal.MaxValue;
			m_tbxTolernece.Minimum = 0;

		}

		void m_clstStrategy_Format( object sender, ListControlConvertEventArgs e )
		{
			StartPointSelectStrategy strategy = (StartPointSelectStrategy)e.ListItem;

			switch( strategy ) {
				case StartPointSelectStrategy.LongestSegmentMidpoint:
					e.Value = m_LanguageManager.TryGetString( "LongestSegmentMidpoint", "長邊中點" );
					break;
				case StartPointSelectStrategy.XNegative:
					e.Value = m_LanguageManager.TryGetString( "XNegative", "最小X座標" );
					break;
				case StartPointSelectStrategy.XPositive:
					e.Value = m_LanguageManager.TryGetString( "XPositive", "最大X座標" );
					break;
				case StartPointSelectStrategy.YNegative:
					e.Value = m_LanguageManager.TryGetString( "YNegative", "最小Y座標" );
					break;
				case StartPointSelectStrategy.YPositive:
					e.Value = m_LanguageManager.TryGetString( "YPositive", "最大Y座標" );
					break;
				case StartPointSelectStrategy.ZNegative:
					e.Value = m_LanguageManager.TryGetString( "ZNegative", "最小Z座標" );
					break;
				case StartPointSelectStrategy.ZPositive:
					e.Value = m_LanguageManager.TryGetString( "ZPositive", "最大Z座標" );
					break;
				default:
					e.Value = e.ListItem.ToString();
					break;
			}
		}

		public void ResetParam( StartPointParam param )
		{
			m_bSuppressValueChanged = true;
			m_Param = param;

			if( m_Param == null ) {
				m_pnlStartPoint.Enabled = false;
				m_bSuppressValueChanged = false;
				return;
			}
			m_pnlStartPoint.Enabled = true;

			m_rbtnManualSelect.Enabled = m_Param.IsSingleSelectMode;
			m_rbtnManualSelect.Checked = !m_Param.IsAutoMode;
			m_rbtnAutoSelect.Checked = m_Param.IsAutoMode;
			m_pnlAutoSelect.Enabled = m_Param.IsAutoMode;

			m_tbxTolernece.Value = (decimal)m_Param.Tolerance;

			ResetPriorityRuleList();
			UpdateControlState();

			m_bSuppressValueChanged = false;
		}

		void ResetPriorityRuleList()
		{
			StartPointSelectStrategy? selectedStrategy = GetSelectedStrategy();
			m_clstStrategy.Items.Clear();


			foreach( StartPointSelectStrategy strategy in m_Param.Strategies ) {
				m_clstStrategy.Items.Add( strategy, true );
			}

			foreach( StartPointSelectStrategy strategy in Enum.GetValues( typeof( StartPointSelectStrategy ) ) ) {
				if( !m_Param.Strategies.Contains( strategy ) ) {
					m_clstStrategy.Items.Add( strategy, false );
				}
			}

			for( int i = 0; i < m_clstStrategy.Items.Count; ++i ) {
				if( (StartPointSelectStrategy)m_clstStrategy.Items[ i ] == selectedStrategy ) {
					m_clstStrategy.SelectedIndex = i;
					break;
				}
			}
		}

		void UpdateControlState()
		{
			StartPointSelectStrategy? selectedStrategy = GetSelectedStrategy();
			int priorityIndex = GetSelectedStrategy().HasValue
				? m_Param.Strategies.IndexOf( GetSelectedStrategy().Value )
				: -1;

			m_btnIncreasePriority.Enabled = priorityIndex > 0;
			m_btnDecreasePriority.Enabled = priorityIndex >= 0 && priorityIndex < m_Param.Strategies.Count - 1;
			m_btnApply.Enabled = m_Param.Strategies.Count > 0;
		}

		#region UI events

		void m_rbtnManualSelect_CheckedChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			if( m_rbtnManualSelect.Checked && m_Param.IsAutoMode ) {
				ModeChanged?.Invoke();
			}
		}

		void m_rbtnAutoSelect_CheckedChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			if( m_rbtnAutoSelect.Checked && !m_Param.IsAutoMode ) {
				ModeChanged?.Invoke();
			}
		}

		void m_clstStrategy_ItemCheck( object sender, ItemCheckEventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			StartPointSelectStrategy strategy = (StartPointSelectStrategy)m_clstStrategy.Items[ e.Index ];

			// invoke action after winform UI Check event, to prevent conflict of CheckBoxList
			BeginInvoke( new Action( () =>
			{
				if( !IsDisposed && IsHandleCreated ) {
					AddOrRemoveStrategy?.Invoke( strategy, e.NewValue == CheckState.Checked );
				}
			} ) );
		}

		void m_clstStrategy_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			UpdateControlState();
		}

		void m_btnIncreasePriority_Click( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			StartPointSelectStrategy strategy = GetSelectedStrategy().Value;
			IncreaseOrDecreasePriority.Invoke( strategy, true );
		}

		void m_btnDecreasePriority_Click( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			StartPointSelectStrategy strategy = GetSelectedStrategy().Value;
			IncreaseOrDecreasePriority.Invoke( strategy, false );
		}

		void m_tbxTolerance_DebouncedValueChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			m_Param.Tolerance = (double)m_tbxTolernece.Value;
		}

		void m_btnApply_Click( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			Apply?.Invoke();
		}

		#endregion

		StartPointSelectStrategy? GetSelectedStrategy()
		{
			if( m_clstStrategy.SelectedIndex < 0 ) {
				return null;
			}

			return (StartPointSelectStrategy)m_clstStrategy.SelectedItem;
		}

		bool m_bSuppressValueChanged;
		StartPointParam m_Param;
		LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "StartPointDlg" );
	}

	public class StartPointParam
	{
		public bool IsAutoMode { get; set; }

		public bool IsSingleSelectMode { get; set; }

		public List<StartPointSelectStrategy> Strategies { get; set; }

		public double Tolerance { get; set; }

		public StartPointParam( bool isAutoMode, bool isSinglesElectMode, List<StartPointSelectStrategy> strategies, double tolerance )
		{
			IsAutoMode = isAutoMode;
			IsSingleSelectMode = isSinglesElectMode;
			Strategies = strategies;
			Tolerance = tolerance;
		}
	}
}
