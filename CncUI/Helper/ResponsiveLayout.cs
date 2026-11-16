using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Controls;
using CommonUI.Theme;

namespace CncUI.Controls
{
	internal sealed class ResponsiveLayout
	{
		public ResponsiveLayout( Control container ) : this( container, container.ClientSize )
		{
		}

		public ResponsiveLayout( Control container, Size designSize )
		{
			m_DesignSize = designSize;

			// a titled card keeps its title bar height; only the content below it is scaled
			SectionPanel card = container as SectionPanel;
			m_TitleHeight = card != null && !string.IsNullOrEmpty( card.Text ) ? ThemeMetrics.SECTION_TITLE_HEIGHT : 0;
			foreach( Control control in container.Controls ) {
				m_Items.Add( new Item( control ) );
			}
		}

		public void Apply( Control container )
		{
			Apply( container, container.ClientSize );
		}

		public void Apply( Control container, Size layoutSize )
		{
			if( m_DesignSize.Width <= 0 || m_DesignSize.Height <= 0 ||
				layoutSize.Width <= 0 || layoutSize.Height <= 0 ) {
				return;
			}
			double scaleX = (double)layoutSize.Width / m_DesignSize.Width;
			double scaleY = (double)Math.Max( 1, layoutSize.Height - m_TitleHeight ) /
				Math.Max( 1, m_DesignSize.Height - m_TitleHeight );
			container.SuspendLayout();
			try {
				foreach( Item item in m_Items ) {
					Rectangle original = item.Bounds;

					// controls in the title bar (card actions) keep their design size and stay at the right edge
					bool isInTitleBar = original.Top < m_TitleHeight;
					if( isInTitleBar ) {
						int titleLeft = original.Left + layoutSize.Width - m_DesignSize.Width;
						item.Control.SetBounds( titleLeft, original.Top, original.Width, original.Height );
						continue;
					}
					int left = (int)Math.Round( original.Left * scaleX );
					int right = (int)Math.Round( original.Right * scaleX );
					int top = m_TitleHeight + (int)Math.Round( ( original.Top - m_TitleHeight ) * scaleY );
					int bottom = m_TitleHeight + (int)Math.Round( ( original.Bottom - m_TitleHeight ) * scaleY );
					item.Control.SetBounds( left, top, Math.Max( 1, right - left ), Math.Max( 1, bottom - top ) );
					item.Children?.Apply( item.Control );
				}
			}
			finally {
				container.ResumeLayout( false );
			}
			container.Invalidate();
		}

		sealed class Item
		{
			public Item( Control control )
			{
				Control = control;
				Bounds = control.Bounds;
				if( control is Panel ) {
					Children = new ResponsiveLayout( control );
				}
			}

			public readonly Control Control;
			public readonly Rectangle Bounds;
			public readonly ResponsiveLayout Children;
		}

		readonly Size m_DesignSize;
		readonly int m_TitleHeight;
		readonly List<Item> m_Items = new List<Item>();
	}
}
