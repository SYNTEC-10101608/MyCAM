using System.Drawing;

namespace MyCAM.Layout
{
	/// <summary>
	/// Bounds of the main window regions calculated by MainLayout, each relative to the panel it is placed in.
	/// </summary>
	internal sealed class MainLayoutRegions
	{
		/// <summary>
		/// Tree host, in the background.
		/// </summary>
		public Rectangle Tree
		{
			get; set;
		}

		/// <summary>
		/// 3D viewer host, in the background.
		/// </summary>
		public Rectangle Viewer
		{
			get; set;
		}

		/// <summary>
		/// Bottom area of the messages (and the CNC monitor panel in landscape), in the background.
		/// </summary>
		public Rectangle Bottom
		{
			get; set;
		}

		/// <summary>
		/// Messages host, in the bottom area.
		/// </summary>
		public Rectangle Messages
		{
			get; set;
		}

		/// <summary>
		/// CNC operation panel host: in the background in landscape, in the vertical CNC host in portrait.
		/// </summary>
		public Rectangle Operation
		{
			get; set;
		}

		/// <summary>
		/// CNC monitor panel host: in the bottom area in landscape, in the vertical CNC host in portrait.
		/// </summary>
		public Rectangle Monitor
		{
			get; set;
		}

		/// <summary>
		/// Vertical CNC host below the messages, in the background; empty in landscape.
		/// </summary>
		public Rectangle Vertical
		{
			get; set;
		}

		/// <summary>
		/// CNC alarm host under the monitor panel, in the vertical CNC host; empty in landscape.
		/// </summary>
		public Rectangle VerticalAlarm
		{
			get; set;
		}
	}
}
