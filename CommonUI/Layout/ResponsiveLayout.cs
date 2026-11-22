using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CommonUI.Controls;
using CommonUI.Theme;

namespace CommonUI.Layout
{
	/// <summary>
	/// Proportional layout of an embedded panel (ui-style.instructions.md, section 6): remembers the designer
	/// bounds of the children and scales them to the size the main window gives. A titled card keeps its title bar
	/// height and the title bar controls keep their size at the right edge. A container of cards keeps the margin and
	/// the gaps around its cards; only the cards are scaled. Dialogs do not use it.
	/// </summary>
	public sealed class ResponsiveLayout
	{
		/// <summary>
		/// Remember the current bounds of the children; the current client size is the design size.
		/// </summary>
		public ResponsiveLayout( Control container ) : this( container, container.ClientSize )
		{
		}

		/// <summary>
		/// Remember the current bounds of the children against the given design size.
		/// </summary>
		public ResponsiveLayout( Control container, Size designSize )
		{
			m_DesignSize = designSize;

			// a titled card keeps its title bar height; only the content below it is scaled
			SectionPanel card = container as SectionPanel;
			m_TitleHeight = card != null && !string.IsNullOrEmpty( card.Text ) ? ThemeMetrics.SECTION_TITLE_HEIGHT : 0;
			foreach( Control control in container.Controls ) {
				m_Items.Add( new Item( control ) );
			}

			// cards side by side or stacked keep the panel margin and the gaps between them at their token sizes,
			// so only the spans the cards cover are scaled; other content is scaled as a whole
			bool isCardContainer = m_Items.Count > 0 && m_Items.TrueForAll( item => item.Control is SectionPanel );
			if( isCardContainer ) {
				m_HorizontalSpanList = MergeSpans( m_Items.ConvertAll( item =>
					new Span( item.Bounds.Left, item.Bounds.Right ) ) );
				m_VerticalSpanList = MergeSpans( m_Items.ConvertAll( item =>
					new Span( item.Bounds.Top, item.Bounds.Bottom ) ) );
			}
			else {
				m_HorizontalSpanList = new List<Span> { new Span( 0, designSize.Width ) };
				m_VerticalSpanList = new List<Span> { new Span( m_TitleHeight, designSize.Height ) };
			}
		}

		/// <summary>
		/// Scale the children to the current client size of the container.
		/// </summary>
		public void Apply( Control container )
		{
			Apply( container, container.ClientSize );
		}

		/// <summary>
		/// Scale the children to the given size.
		/// </summary>
		public void Apply( Control container, Size layoutSize )
		{
			if( m_DesignSize.Width <= 0 || m_DesignSize.Height <= 0 ||
				layoutSize.Width <= 0 || layoutSize.Height <= 0 ) {
				return;
			}
			AxisScale scaleX = new AxisScale( m_HorizontalSpanList, m_DesignSize.Width, layoutSize.Width );
			AxisScale scaleY = new AxisScale( m_VerticalSpanList, m_DesignSize.Height, layoutSize.Height );
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
					int left = scaleX.Map( original.Left );
					int right = scaleX.Map( original.Right );
					int top = scaleY.Map( original.Top );
					int bottom = scaleY.Map( original.Bottom );
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

		// design coordinates [Start, End) on one axis
		struct Span
		{
			public Span( int nStart, int nEnd )
			{
				Start = nStart;
				End = nEnd;
			}

			public readonly int Start;
			public readonly int End;
		}

		// maps the design coordinates of one axis to the layout length: the spans stretch by one scale,
		// the rest (margins, gaps, the title bar) keeps its design length
		sealed class AxisScale
		{
			public AxisScale( List<Span> spanList, int nDesignLength, int nLayoutLength )
			{
				m_SpanList = spanList;
				int nSpanLength = 0;
				foreach( Span span in spanList ) {
					nSpanLength += span.End - span.Start;
				}
				int nFixedLength = nDesignLength - nSpanLength;

				// nothing to stretch
				if( nSpanLength <= 0 ) {
					m_dScale = 1;
					return;
				}
				m_dScale = Math.Max( 0, nLayoutLength - nFixedLength ) / (double)nSpanLength;
			}

			public int Map( int nValue )
			{
				// each span before the value adds its stretched part
				double dOffset = 0;
				foreach( Span span in m_SpanList ) {
					int nCovered = Math.Min( nValue, span.End ) - span.Start;
					if( nCovered > 0 ) {
						dOffset += nCovered * ( m_dScale - 1 );
					}
				}
				return (int)Math.Round( nValue + dOffset );
			}

			readonly List<Span> m_SpanList;
			readonly double m_dScale;
		}

		// sorted spans with the overlapping and touching ones merged
		static List<Span> MergeSpans( List<Span> spanList )
		{
			spanList.Sort( ( a, b ) => a.Start.CompareTo( b.Start ) );
			List<Span> mergedList = new List<Span>();
			foreach( Span span in spanList ) {
				int nLastIndex = mergedList.Count - 1;
				bool isOverlapping = nLastIndex >= 0 && span.Start <= mergedList[ nLastIndex ].End;
				if( isOverlapping ) {
					int nEnd = Math.Max( mergedList[ nLastIndex ].End, span.End );
					mergedList[ nLastIndex ] = new Span( mergedList[ nLastIndex ].Start, nEnd );
				}
				else {
					mergedList.Add( span );
				}
			}
			return mergedList;
		}

		readonly Size m_DesignSize;
		readonly int m_TitleHeight;
		readonly List<Item> m_Items = new List<Item>();
		readonly List<Span> m_HorizontalSpanList;
		readonly List<Span> m_VerticalSpanList;
	}
}
