using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyCAM.Layout
{
	// The only place that sizes the regions of the main window (ui-style.instructions.md, section 6).
	// Calculate works on sizes only; Update places the host panels with its result.
	internal sealed class MainLayout
	{
		public MainLayout( Panel background, Panel tree, Panel viewer, Panel bottom, Panel operationHost,
			Panel monitorHost, Panel messages, Panel log, Panel verticalHost, Panel verticalAlarmHost,
			ToolStripItem machiningEntry, ToolStripItem controllerEntry )
		{
			m_Background = background;
			m_Tree = tree;
			m_Viewer = viewer;
			m_Bottom = bottom;
			m_OperationHost = operationHost;
			m_MonitorHost = monitorHost;
			m_Messages = messages;
			m_Log = log;
			m_VerticalHost = verticalHost;
			m_VerticalAlarmHost = verticalAlarmHost;
			m_MachiningEntry = machiningEntry;
			m_ControllerEntry = controllerEntry;
		}

		/// <summary>
		/// Bounds of every region for the given background size and state; no control is touched.
		/// </summary>
		/// <param name="areaSize">Client size of the background below the tool bars.</param>
		/// <param name="isVertical">Portrait main window.</param>
		/// <param name="showPanels">The CNC operation and monitor panels are shown.</param>
		/// <param name="machiningActive">The machining page is active.</param>
		/// <param name="operationPreferredSize">Preferred size of the CNC operation panel.</param>
		/// <param name="monitorPreferredSize">Preferred size of the CNC monitor panel (used when shown).</param>
		/// <returns>Bounds of the regions, each relative to the panel it is placed in.</returns>
		public static MainLayoutRegions Calculate( Size areaSize, bool isVertical, bool showPanels,
			bool machiningActive, Size operationPreferredSize, Size monitorPreferredSize )
		{
			if( isVertical ) {
				return CalculateVertical( areaSize );
			}
			return CalculateHorizontal( areaSize, showPanels, machiningActive, operationPreferredSize,
				monitorPreferredSize );
		}

		public void Update( Size windowSize, bool cncAvailable, bool machiningActive, Size operationPreferredSize,
			Control operationPanel, Control monitorPanel, Control alarmPanel, bool hasAlarmContent )
		{
			if( m_Updating ) {
				return;
			}
			bool isVertical = windowSize.Height > windowSize.Width;
			bool showPanels = operationPanel != null && monitorPanel != null;
			Size operationSize = operationPanel?.GetPreferredSize( Size.Empty ) ?? operationPreferredSize;
			Size monitorSize = showPanels ? monitorPanel.GetPreferredSize( Size.Empty ) : Size.Empty;
			MainLayoutRegions regions = Calculate( m_Background.ClientSize, isVertical, showPanels, machiningActive,
				operationSize, monitorSize );
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
				m_Tree.Bounds = regions.Tree;
				m_Viewer.Bounds = regions.Viewer;
				m_Bottom.Bounds = regions.Bottom;
				m_Messages.Bounds = regions.Messages;
				m_VerticalHost.Bounds = regions.Vertical;
				m_OperationHost.Bounds = regions.Operation;
				m_MonitorHost.Bounds = regions.Monitor;
				m_VerticalAlarmHost.Bounds = regions.VerticalAlarm;
				FitAlarmToHost();
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
						}
						finally {
							m_Updating = false;
							UpdateMessages( windowSize, alarmPanel, hasAlarmContent );
						}
					}
				}
			}
		}

		/// <summary>
		/// Screen rectangle that dialogs are centered in: the whole window in landscape, the CAM area above the
		/// CNC host in portrait (whether the CNC panels are shown or not).
		/// </summary>
		/// <param name="mainForm">Main window.</param>
		/// <returns>Screen rectangle of the dialog area.</returns>
		public Rectangle GetDialogArea( Form mainForm )
		{
			Rectangle bounds = mainForm.Bounds;
			bool isVertical = mainForm.Height > mainForm.Width;
			if( isVertical == false || m_VerticalHost.IsDisposed || m_VerticalHost.Parent == null ) {
				return bounds;
			}
			int nHostTop = m_VerticalHost.Parent.PointToScreen( m_VerticalHost.Location ).Y;
			int nCAMHeight = nHostTop - bounds.Top;

			// the CNC host is not laid out yet
			if( nCAMHeight <= 0 || nCAMHeight > bounds.Height ) {
				return bounds;
			}
			return new Rectangle( bounds.Left, bounds.Top, bounds.Width, nCAMHeight );
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
			int logHeight = !isVertical && show ? height * HORIZONTAL_LOG_PERCENT / 100 : height;
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

		// landscape: tree | 3D | CNC operation column (when the panels are shown) on top, messages and the CNC monitor
		// below; the CNC operation column runs down to the bottom
		static MainLayoutRegions CalculateHorizontal( Size areaSize, bool showPanels, bool machiningActive,
			Size operationPreferredSize, Size monitorPreferredSize )
		{
			int nWidth = areaSize.Width;
			int nHeight = areaSize.Height;

			// columns: the tree is a share of the width left by the operation column, even while it is not shown,
			// so the tree keeps its width when the panels open
			int nOperationCell = operationPreferredSize.Width + HOST_MARGIN * 2;
			int nOperationColumn = (int)Math.Ceiling( nOperationCell * HORIZONTAL_COLUMN_SCALE );
			int nTreeColumn = (int)( Math.Max( 0, nWidth - nOperationColumn ) * HORIZONTAL_TREE_PERCENT / 100F );
			int nShownOperationColumn = showPanels ? nOperationColumn : 0;
			int nViewerColumn = Math.Max( 0, nWidth - nTreeColumn - nShownOperationColumn );

			// rows: the bottom area is a share of the height, or holds the whole monitor panel when it is shown
			int nBottomRow;
			int nTopRow;
			if( showPanels ) {
				int nMonitorRow = monitorPreferredSize.Height + HOST_MARGIN * 2;
				int nShareRow = (int)Math.Round( nHeight * HORIZONTAL_MONITOR_ROW_RATIO );
				nBottomRow = Math.Max( nMonitorRow, nShareRow );
				nTopRow = nHeight - nBottomRow;
			}
			else {
				float fBottomPercent = machiningActive ?
					HORIZONTAL_MACHINING_BOTTOM_PERCENT : HORIZONTAL_BOTTOM_PERCENT;
				nTopRow = (int)( nHeight * ( 100F - fBottomPercent ) / 100F );
				nBottomRow = (int)( nHeight * fBottomPercent / 100F );
			}

			// the messages line up with the tree once the machining page or the panels are in use
			bool alignMessagesWithTree = machiningActive || showPanels;
			int nBottomWidth = nTreeColumn + nViewerColumn;
			if( showPanels == false ) {
				nBottomWidth = nWidth;
			}
			int nMessagesColumn = alignMessagesWithTree ? nTreeColumn : nBottomWidth;
			MainLayoutRegions regions = new MainLayoutRegions();
			regions.Tree = Inset( new Rectangle( 0, 0, nTreeColumn, nTopRow ) );
			regions.Viewer = Inset( new Rectangle( nTreeColumn, 0, nViewerColumn, nTopRow ) );
			int nOperationLeft = nTreeColumn + nViewerColumn;
			regions.Operation = Inset( new Rectangle( nOperationLeft, 0, nShownOperationColumn, nHeight ) );
			regions.Bottom = new Rectangle( 0, nTopRow, nBottomWidth, nBottomRow );
			regions.Messages = Inset( new Rectangle( 0, 0, nMessagesColumn, nBottomRow ) );
			regions.Monitor = InsetMonitor( new Rectangle( nMessagesColumn, 0, nBottomWidth - nMessagesColumn,
				nBottomRow ) );
			regions.Vertical = Rectangle.Empty;
			regions.VerticalAlarm = Rectangle.Empty;
			return regions;
		}

		// portrait: tree | 3D on top, the messages below, then the CNC host with the monitor and alarms on the left and
		// the operation panel on the right
		static MainLayoutRegions CalculateVertical( Size areaSize )
		{
			int nWidth = areaSize.Width;
			int nHeight = areaSize.Height;
			int nTreeColumn = (int)( nWidth * VERTICAL_TREE_PERCENT / 100F );
			int nViewerColumn = (int)( nWidth * ( 100F - VERTICAL_TREE_PERCENT ) / 100F );
			int nTopRow = (int)( nHeight * VERTICAL_TOP_PERCENT / 100F );
			int nMessagesRow = (int)( nHeight * VERTICAL_MESSAGES_PERCENT / 100F );

			// the CNC host takes the rest, so rounding never leaves a gap at the bottom
			int nCNCRow = Math.Max( 0, nHeight - nTopRow - nMessagesRow );
			MainLayoutRegions regions = new MainLayoutRegions();
			regions.Tree = Inset( new Rectangle( 0, 0, nTreeColumn, nTopRow ) );
			regions.Viewer = Inset( new Rectangle( nTreeColumn, 0, nViewerColumn, nTopRow ) );
			regions.Bottom = new Rectangle( 0, nTopRow, nWidth, nMessagesRow );
			regions.Messages = Inset( new Rectangle( 0, 0, nWidth, nMessagesRow ) );
			regions.Vertical = Inset( new Rectangle( 0, nTopRow + nMessagesRow, nWidth, nCNCRow ) );

			// inside the CNC host
			int nHostWidth = regions.Vertical.Width;
			int nHostHeight = regions.Vertical.Height;
			int nLeftWidth = (int)Math.Round( nHostWidth * VERTICAL_LEFT_RATIO );
			int nMonitorHeight = (int)Math.Round( nHostHeight * ( 1 - VERTICAL_ALARM_RATIO ) );
			regions.Monitor = InsetMonitor( new Rectangle( 0, 0, nLeftWidth, nMonitorHeight ) );
			int nAlarmHeight = nHostHeight - nMonitorHeight;
			regions.VerticalAlarm = Inset( new Rectangle( 0, nMonitorHeight, nLeftWidth, nAlarmHeight ) );
			regions.Operation = Inset( new Rectangle( nLeftWidth, 0, nHostWidth - nLeftWidth, nHostHeight ) );
			return regions;
		}

		// each host keeps HOST_MARGIN from the edge of its cell, which leaves a gap between neighboring regions
		static Rectangle Inset( Rectangle cell )
		{
			return InsetBy( cell, HOST_MARGIN, HOST_MARGIN, HOST_MARGIN, HOST_MARGIN );
		}

		// the monitor panel reaches the right edge of its cell
		static Rectangle InsetMonitor( Rectangle cell )
		{
			return InsetBy( cell, HOST_MARGIN, HOST_MARGIN, 0, HOST_MARGIN );
		}

		static Rectangle InsetBy( Rectangle cell, int nLeft, int nTop, int nRight, int nBottom )
		{
			int nWidth = Math.Max( 0, cell.Width - nLeft - nRight );
			int nHeight = Math.Max( 0, cell.Height - nTop - nBottom );
			return new Rectangle( cell.Left + nLeft, cell.Top + nTop, nWidth, nHeight );
		}

		void SetPanelHosts( bool isVertical )
		{
			Panel operationParent = isVertical ? m_VerticalHost : m_Background;
			Panel monitorParent = isVertical ? m_VerticalHost : m_Bottom;
			if( m_OperationHost.Parent != operationParent ) {
				operationParent.Controls.Add( m_OperationHost );
			}
			if( m_MonitorHost.Parent != monitorParent ) {
				monitorParent.Controls.Add( m_MonitorHost );
			}
		}


		void FitAlarmToHost()
		{
			foreach( Control alarm in m_VerticalAlarmHost.Controls ) {
				alarm.Bounds = m_VerticalAlarmHost.ClientRectangle;
			}
		}

		// gap kept by each host from the edge of its region
		const int HOST_MARGIN = 3;

		// landscape: tree share of the width left by the CNC operation column, which is its panel width plus 10 %
		const float HORIZONTAL_TREE_PERCENT = 22F;
		const float HORIZONTAL_COLUMN_SCALE = 1.1F;

		// landscape: bottom area share of the height, larger on the machining page; at least a quarter with the panels
		const float HORIZONTAL_BOTTOM_PERCENT = 15F;
		const float HORIZONTAL_MACHINING_BOTTOM_PERCENT = 25F;
		const double HORIZONTAL_MONITOR_ROW_RATIO = 0.25;

		// landscape: log share of the messages height while CNC alarms are shown below it
		const int HORIZONTAL_LOG_PERCENT = 40;

		// portrait: tree share of the width; top area and messages shares of the height (the CNC host takes the rest)
		const float VERTICAL_TREE_PERCENT = 15F;
		const float VERTICAL_TOP_PERCENT = 54F;
		const float VERTICAL_MESSAGES_PERCENT = 6F;

		// portrait CNC host: monitor and alarms width share, alarms height share of the left side
		const double VERTICAL_LEFT_RATIO = 0.55;
		const double VERTICAL_ALARM_RATIO = 0.2;

		readonly Panel m_Background;
		readonly Panel m_Tree;
		readonly Panel m_Viewer;
		readonly Panel m_Bottom;
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
