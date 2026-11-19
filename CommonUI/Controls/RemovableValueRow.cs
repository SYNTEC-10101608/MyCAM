using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CommonUI.Theme;

namespace CommonUI.Controls
{
	/// <summary>
	/// A row created at runtime in a list of values: index + numeric box + unit + delete button.
	/// The caller gives the value range, decimal places, unit and delete text; the layout follows the theme metrics.
	/// </summary>
	public class RemovableValueRow : Panel
	{
		/// <summary>
		/// Height of a row; rows are stacked without extra spacing.
		/// </summary>
		public const int ROW_HEIGHT = ThemeMetrics.ROW_PITCH;

		/// <summary>
		/// Width of a row.
		/// </summary>
		public const int ROW_WIDTH = INDEX_WIDTH + ThemeMetrics.CONTROL_GAP + VALUE_WIDTH + ThemeMetrics.CONTROL_GAP +
			UNIT_WIDTH + ThemeMetrics.CONTROL_GAP + DELETE_WIDTH;

		/// <summary>
		/// Create a row.
		/// </summary>
		/// <param name="dInitialValue">Initial value, set without raising <see cref="ValueCommitted"/>.</param>
		/// <param name="minimum">Smallest value the field accepts.</param>
		/// <param name="maximum">Largest value the field accepts.</param>
		/// <param name="nDecimalPlaces">Decimal places of the field.</param>
		/// <param name="szUnit">Unit text, e.g. "mm".</param>
		/// <param name="szDeleteText">Text of the delete button.</param>
		/// <param name="indexColor">Data color of the index (e.g. an axis color); null for the body color.</param>
		public RemovableValueRow( double dInitialValue, decimal minimum, decimal maximum, int nDecimalPlaces,
			string szUnit, string szDeleteText, Color? indexColor = null )
		{
			int nValueLeft = INDEX_WIDTH + ThemeMetrics.CONTROL_GAP;
			int nUnitLeft = nValueLeft + VALUE_WIDTH + ThemeMetrics.CONTROL_GAP;
			int nDeleteLeft = nUnitLeft + UNIT_WIDTH + ThemeMetrics.CONTROL_GAP;
			m_lblIndex = new Label
			{
				AutoSize = false,
				Location = new Point( 0, ThemeMetrics.LABEL_OFFSET ),
				Size = new Size( INDEX_WIDTH, ThemeMetrics.INPUT_HEIGHT - ThemeMetrics.LABEL_OFFSET ),
				TextAlign = ContentAlignment.TopLeft
			};
			if( indexColor.HasValue ) {
				m_lblIndex.ForeColor = indexColor.Value;
			}
			m_numValue = new DebouncedNumericUpDown
			{
				Location = new Point( nValueLeft, 0 ),
				Size = new Size( VALUE_WIDTH, ThemeMetrics.INPUT_HEIGHT ),
				Minimum = minimum,
				Maximum = maximum,
				DecimalPlaces = nDecimalPlaces,
				Increment = 1,
				RestrictInvalidCharacters = true
			};
			m_numValue.SilentSetValue = (decimal)dInitialValue;
			m_lblUnit = new ThemedLabel
			{
				AutoSize = false,
				Role = TextRole.Secondary,
				Location = new Point( nUnitLeft, ThemeMetrics.LABEL_OFFSET ),
				Size = new Size( UNIT_WIDTH, ThemeMetrics.INPUT_HEIGHT - ThemeMetrics.LABEL_OFFSET ),
				Text = szUnit
			};
			m_btnDelete = new ThemedButton
			{
				Location = new Point( nDeleteLeft, 0 ),
				Size = new Size( DELETE_WIDTH, ThemeMetrics.INPUT_HEIGHT ),
				Text = szDeleteText,
				TabStop = false
			};
			Controls.Add( m_lblIndex );
			Controls.Add( m_numValue );
			Controls.Add( m_lblUnit );
			Controls.Add( m_btnDelete );

			m_numValue.DebouncedValueChanged += OnDebouncedValueChanged;
			m_numValue.ValueRejected += OnNumValueRejected;
			m_btnDelete.Click += OnDeleteClick;

			Size = new Size( ROW_WIDTH, ROW_HEIGHT );
		}

		/// <summary>
		/// Occurs when the delete button is clicked; the owner removes the row.
		/// </summary>
		public event Action<RemovableValueRow> DeleteRequested;

		/// <summary>
		/// Occurs when a valid value is committed (debounced).
		/// </summary>
		public event Action<RemovableValueRow> ValueCommitted;

		/// <summary>
		/// Occurs when the input is out of range or not a number; the value reverts to the last valid one.
		/// </summary>
		public event EventHandler<ValueRejectedEventArgs> ValueRejected;

		/// <summary>
		/// The numeric box of the row.
		/// </summary>
		public DebouncedNumericUpDown ValueControl => m_numValue;

		/// <summary>
		/// The current valid value of the row.
		/// </summary>
		public double LastValidValue => (double)m_numValue.Value;

		/// <summary>
		/// True if the delete button is enabled.
		/// </summary>
		public bool CanDelete
		{
			get => m_btnDelete.Enabled;
			set => m_btnDelete.Enabled = value;
		}

		/// <summary>
		/// Show the 1-based position of the row in its list.
		/// </summary>
		/// <param name="nRowID">1-based row number.</param>
		public void SetRowID( int nRowID )
		{
			m_lblIndex.Text = "#" + nRowID.ToString( CultureInfo.InvariantCulture );
		}

		/// <summary>
		/// Stack rows from the top of their container.
		/// </summary>
		/// <param name="rowList">Rows in display order.</param>
		public static void Arrange( System.Collections.Generic.IList<RemovableValueRow> rowList )
		{
			if( rowList == null ) {
				return;
			}
			for( int i = 0; i < rowList.Count; i++ ) {
				rowList[ i ].SetRowID( i + 1 );
				rowList[ i ].CanDelete = rowList.Count > 1;
				rowList[ i ].Location = new Point( 0, i * ROW_HEIGHT );
			}
		}

		void OnDebouncedValueChanged( object sender, EventArgs e )
		{
			ValueCommitted?.Invoke( this );
		}

		void OnNumValueRejected( object sender, ValueRejectedEventArgs e )
		{
			ValueRejected?.Invoke( this, e );
		}

		void OnDeleteClick( object sender, EventArgs e )
		{
			DeleteRequested?.Invoke( this );
		}

		const int INDEX_WIDTH = 28;
		const int VALUE_WIDTH = 80;
		const int UNIT_WIDTH = 28;
		const int DELETE_WIDTH = 56;

		readonly Label m_lblIndex;
		readonly DebouncedNumericUpDown m_numValue;
		readonly ThemedLabel m_lblUnit;
		readonly ThemedButton m_btnDelete;
	}
}
