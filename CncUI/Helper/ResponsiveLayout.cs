using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

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
			double scaleY = (double)layoutSize.Height / m_DesignSize.Height;
			container.SuspendLayout();
			try {
				foreach( Item item in m_Items ) {
					Rectangle original = item.Bounds;
					int left = (int)Math.Round( original.Left * scaleX );
					int top = (int)Math.Round( original.Top * scaleY );
					int right = (int)Math.Round( original.Right * scaleX );
					int bottom = (int)Math.Round( original.Bottom * scaleY );
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
		readonly List<Item> m_Items = new List<Item>();
	}
}
