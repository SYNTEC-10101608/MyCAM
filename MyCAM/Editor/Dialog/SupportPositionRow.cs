using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	internal class SupportPositionRow : Panel
	{
		internal const int RowHeight = 24;

		const int IndexWidth = 20;
		const int ValueWidth = 52;
		const int UnitWidth = 28;
		const int DeleteWidth = 42;

		readonly Label m_lblIndex;
		readonly DebouncedNumericUpDown m_nudValue;
		readonly Label m_lblUnit;
		readonly Button m_btnDelete;

		internal SupportPositionRow( double initialValue, Color? labelColor = null, string deleteButtonText = "Delete" )
		{
			Color foreColor = labelColor ?? SystemColors.ControlText;

			m_lblIndex = new Label
			{
				Location = new Point( 0, 4 ),
				Size = new Size( IndexWidth, 20 ),
				TextAlign = ContentAlignment.MiddleLeft,
				ForeColor = foreColor
			};
			m_nudValue = new DebouncedNumericUpDown
			{
				Location = new Point( IndexWidth + 4, 2 ),
				Size = new Size( ValueWidth, 25 ),
				Minimum = -99999,
				Maximum = 99999,
				DecimalPlaces = 1,
				Increment = 1,
				RestrictInvalidCharacters = true,
				ForeColor = foreColor
			};
			m_nudValue.SilentSetValue = (decimal)initialValue;
			m_lblUnit = new Label
			{
				Location = new Point( IndexWidth + 4 + ValueWidth + 4, 2 ),
				Size = new Size( UnitWidth, 25 ),
				Text = "mm",
				TextAlign = ContentAlignment.MiddleLeft
			};
			m_btnDelete = new Button
			{
				Size = new Size( DeleteWidth, 22 ),
				Location = new Point( IndexWidth + 4 + ValueWidth + 4 + UnitWidth + 4, 2 ),
				Text = deleteButtonText,
				UseVisualStyleBackColor = true,
				TabStop = false
			};
			Controls.Add( m_lblIndex );
			Controls.Add( m_nudValue );
			Controls.Add( m_lblUnit );
			Controls.Add( m_btnDelete );

			m_nudValue.DebouncedValueChanged += OnDebouncedValueChanged;
			m_nudValue.ValueRejected += OnNudValueRejected;
			m_btnDelete.Click += OnDeleteClick;

			Height = RowHeight;
		}

		internal event Action<SupportPositionRow> DeleteRequested;
		internal event Action<SupportPositionRow> ValueCommitted;
		internal event EventHandler<ValueRejectedEventArgs> ValueRejected;

		internal DebouncedNumericUpDown ValueControl => m_nudValue;

		internal double LastValidValue => (double)m_nudValue.Value;

		internal bool CanDelete
		{
			get { return m_btnDelete.Enabled; }
			set { m_btnDelete.Enabled = value; }
		}

		internal void SetIndex( int oneBasedIndex )
		{
			m_lblIndex.Text = "#" + oneBasedIndex.ToString( CultureInfo.InvariantCulture );
		}

		internal void SetRowWidth()
		{
			int contentWidth = IndexWidth + 4 + ValueWidth + 4 + UnitWidth + 8 + DeleteWidth;
			Width = contentWidth;
			m_nudValue.Size = new Size( ValueWidth, 25 );
			m_nudValue.Location = new Point( IndexWidth + 4, 2 );
			m_lblUnit.Location = new Point( IndexWidth + 4 + ValueWidth + 4, 2 );
			m_btnDelete.Location = new Point( IndexWidth + 4 + ValueWidth + 4 + UnitWidth + 4, 2 );
		}

		internal bool TryGetValue( out double value )
		{
			value = (double)m_nudValue.Value;
			return true;
		}

		void OnDebouncedValueChanged( object sender, EventArgs e )
		{
			ValueCommitted?.Invoke( this );
		}

		void OnNudValueRejected( object sender, ValueRejectedEventArgs e )
		{
			ValueRejected?.Invoke( this, e );
		}

		void OnDeleteClick( object sender, EventArgs e )
		{
			DeleteRequested?.Invoke( this );
		}
	}
}
