using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyCAM.Layout
{
	internal sealed class MainLayout
	{
		public MainLayout( TableLayoutPanel background, TableLayoutPanel bottom, Panel operationHost, Panel monitorHost,
			Panel messages, Panel log, Panel verticalHost, Panel verticalAlarmHost,
			ToolStripItem machiningEntry, ToolStripItem controllerEntry )
		{
			m_Background = background;
			m_Bottom = bottom;
			m_OperationHost = operationHost;
			m_MonitorHost = monitorHost;
			m_Messages = messages;
			m_Log = log;
			m_VerticalHost = verticalHost;
			m_VerticalAlarmHost = verticalAlarmHost;
			m_MachiningEntry = machiningEntry;
			m_ControllerEntry = controllerEntry;
			m_VerticalHost.SizeChanged += OnVerticalHostSizeChanged;
		}

		public void Update( Size windowSize, bool cncAvailable, bool machiningActive, Size operationPreferredSize,
			Control operationPanel, Control monitorPanel, Control alarmPanel, bool hasAlarmContent )
		{
			if( m_Updating ) {
				return;
			}
			bool isVertical = windowSize.Height > windowSize.Width;
			bool showPanels = operationPanel != null && monitorPanel != null;
			bool showHorizontalPanels = !isVertical && showPanels;
			bool alignMessagesWithTree = !isVertical && ( machiningActive || showPanels );
			Size operationSize = operationPanel?.GetPreferredSize( Size.Empty ) ?? operationPreferredSize;
			int operationWidth = (int)Math.Ceiling( ( operationSize.Width + m_OperationHost.Margin.Horizontal ) * HORIZONTAL_COLUMN_SCALE );
			float treeWidth = isVertical ? TREE_COLUMN_WIDTH :
				(int)( Math.Max( 0, m_Background.ClientSize.Width - operationWidth ) * HORIZONTAL_TREE_COLUMN_WIDTH / 100F );
			m_Updating = true;
			m_Background.SuspendLayout();
			m_Bottom.SuspendLayout();
			m_VerticalHost.SuspendLayout();
			try {
				SetPanelHosts( isVertical );
				m_MachiningEntry.Visible = true;
				m_MachiningEntry.Enabled = cncAvailable;
				m_ControllerEntry.Visible = true;
				m_VerticalHost.Visible = isVertical;
				m_OperationHost.Visible = showPanels;
				m_MonitorHost.Visible = showPanels;
				if( !isVertical ) {
					m_Background.SetRowSpan( m_OperationHost, showHorizontalPanels ? 2 : 1 );
				}
				m_Background.SetColumnSpan( m_Bottom, showHorizontalPanels ? 2 : 3 );
				m_Bottom.ColumnStyles[ 0 ].SizeType = alignMessagesWithTree ? SizeType.Absolute : SizeType.Percent;
				m_Bottom.ColumnStyles[ 0 ].Width = alignMessagesWithTree ? treeWidth : 100F;
				m_Bottom.ColumnStyles[ 1 ].SizeType = alignMessagesWithTree ? SizeType.Percent : SizeType.Absolute;
				m_Bottom.ColumnStyles[ 1 ].Width = alignMessagesWithTree ? 100F : 0F;
				m_Background.ColumnStyles[ 0 ].SizeType = isVertical ? SizeType.Percent : SizeType.Absolute;
				m_Background.ColumnStyles[ 0 ].Width = treeWidth;
				m_Background.ColumnStyles[ 1 ].Width = isVertical ? 100F - TREE_COLUMN_WIDTH : 100F;
				m_Background.ColumnStyles[ 2 ].SizeType = SizeType.Absolute;
				if( showHorizontalPanels ) {
					Size monitorSize = monitorPanel.GetPreferredSize( Size.Empty );
					m_Background.ColumnStyles[ 2 ].Width = operationWidth;
					m_Background.RowStyles[ 0 ].SizeType = SizeType.Percent;
					m_Background.RowStyles[ 0 ].Height = 100F;
					m_Background.RowStyles[ 1 ].SizeType = SizeType.Absolute;
					m_Background.RowStyles[ 1 ].Height = Math.Max( monitorSize.Height + m_MonitorHost.Margin.Vertical,
						(int)Math.Round( m_Background.ClientSize.Height * 0.25 ) );
				}
				else {
					float bottomHeight = isVertical ? 6F : ( machiningActive ? 25F : 15F );
					m_Background.ColumnStyles[ 2 ].Width = 0F;
					m_Background.RowStyles[ 0 ].SizeType = SizeType.Percent;
					m_Background.RowStyles[ 0 ].Height = isVertical ? 54F : 100F - bottomHeight;
					m_Background.RowStyles[ 1 ].SizeType = SizeType.Percent;
					m_Background.RowStyles[ 1 ].Height = bottomHeight;
				}
				m_Background.RowStyles[ 2 ].SizeType = isVertical ? SizeType.Percent : SizeType.Absolute;
				m_Background.RowStyles[ 2 ].Height = isVertical ? 40F : 0F;
			}
			finally {
				try {
					m_Background.ResumeLayout( true );
				}
				finally {
					try {
						m_Bottom.ResumeLayout( true );
					}
					finally {
						try {
							m_VerticalHost.ResumeLayout( true );
							if( isVertical ) {
								LayoutVerticalPanels();
							}
						}
						finally {
							m_Updating = false;
							UpdateMessages( windowSize, alarmPanel, hasAlarmContent );
						}
					}
				}
			}
		}

		public void FitPanels( Control operationPanel, Control monitorPanel )
		{
			operationPanel.Bounds = m_OperationHost.ClientRectangle;
			monitorPanel.Bounds = m_MonitorHost.ClientRectangle;
		}

		public void UpdateMessages( Size windowSize, Control alarmPanel, bool hasAlarmContent )
		{
			if( m_Messages.IsDisposed || m_Log.IsDisposed ) {
				return;
			}
			bool isVertical = windowSize.Height > windowSize.Width;
			bool show = alarmPanel != null && !alarmPanel.IsDisposed && hasAlarmContent;
			int width = m_Messages.ClientSize.Width;
			int height = m_Messages.ClientSize.Height;
			int logHeight = !isVertical && show ? height * 40 / 100 : height;
			m_Messages.SuspendLayout();
			m_VerticalAlarmHost.SuspendLayout();
			try {
				m_Log.SetBounds( 0, 0, width, logHeight );
				if( alarmPanel != null && !alarmPanel.IsDisposed ) {
					Panel host = isVertical ? m_VerticalAlarmHost : m_Messages;
					if( alarmPanel.Parent != host ) {
						alarmPanel.Visible = false;
						host.Controls.Add( alarmPanel );
					}
					alarmPanel.Bounds = isVertical ? host.ClientRectangle : new Rectangle( 0, logHeight, width, height - logHeight );
					alarmPanel.Visible = show;
				}
			}
			finally {
				m_Messages.ResumeLayout( true );
				m_VerticalAlarmHost.ResumeLayout( true );
			}
		}

		void SetPanelHosts( bool isVertical )
		{
			if( isVertical ) {
				if( m_OperationHost.Parent != m_VerticalHost ) {
					m_VerticalHost.Controls.Add( m_OperationHost );
				}
				if( m_MonitorHost.Parent != m_VerticalHost ) {
					m_VerticalHost.Controls.Add( m_MonitorHost );
				}
				m_OperationHost.Dock = DockStyle.None;
				m_MonitorHost.Dock = DockStyle.None;
			}
			else {
				if( m_OperationHost.Parent != m_Background ) {
					m_Background.Controls.Add( m_OperationHost, 2, 0 );
				}
				if( m_MonitorHost.Parent != m_Bottom ) {
					m_Bottom.Controls.Add( m_MonitorHost, 1, 0 );
				}
				m_OperationHost.Dock = DockStyle.Fill;
				m_MonitorHost.Dock = DockStyle.Fill;
			}
		}

		void OnVerticalHostSizeChanged( object sender, EventArgs e )
		{
			if( m_Updating || m_VerticalHost.IsDisposed || m_VerticalHost.Disposing || m_OperationHost.Parent != m_VerticalHost ) {
				return;
			}
			LayoutVerticalPanels();
		}

		void LayoutVerticalPanels()
		{
			int width = m_VerticalHost.ClientSize.Width;
			int height = m_VerticalHost.ClientSize.Height;
			int leftWidth = (int)Math.Round( width * VERTICAL_LEFT_RATIO );
			int monitorHeight = (int)Math.Round( height * ( 1 - VERTICAL_ALARM_RATIO ) );
			SetBoundsInArea( m_MonitorHost, new Rectangle( 0, 0, leftWidth, monitorHeight ) );
			SetBoundsInArea( m_VerticalAlarmHost, new Rectangle( 0, monitorHeight, leftWidth, height - monitorHeight ) );
			SetBoundsInArea( m_OperationHost, new Rectangle( leftWidth, 0, width - leftWidth, height ) );
			foreach( Control alarm in m_VerticalAlarmHost.Controls ) {
				alarm.Bounds = m_VerticalAlarmHost.ClientRectangle;
			}
		}

		static void SetBoundsInArea( Panel panel, Rectangle area )
		{
			Padding margin = panel.Margin;
			panel.SetBounds( area.Left + margin.Left, area.Top + margin.Top,
				Math.Max( 0, area.Width - margin.Horizontal ), Math.Max( 0, area.Height - margin.Vertical ) );
		}

		const float TREE_COLUMN_WIDTH = 15F;
		const float HORIZONTAL_TREE_COLUMN_WIDTH = 22F;
		const float HORIZONTAL_COLUMN_SCALE = 1.1F;
		const double VERTICAL_LEFT_RATIO = 0.55;
		const double VERTICAL_ALARM_RATIO = 0.2;

		readonly TableLayoutPanel m_Background;
		readonly TableLayoutPanel m_Bottom;
		readonly Panel m_OperationHost;
		readonly Panel m_MonitorHost;
		readonly Panel m_Messages;
		readonly Panel m_Log;
		readonly Panel m_VerticalHost;
		readonly Panel m_VerticalAlarmHost;
		readonly ToolStripItem m_MachiningEntry;
		readonly ToolStripItem m_ControllerEntry;
		bool m_Updating;
	}
}
