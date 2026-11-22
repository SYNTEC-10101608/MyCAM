using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CommonUI.Controls;

namespace UICheck
{
	// Behavior check of the shared numeric input (ui-style.instructions.md, sections 5 and 8). Compiled by
	// CheckControls.ps1 into the build output folder; the controls are used without a window.
	//
	//   N1  rejected input (unparseable or out of range) restores the latest valid value, even one that is not
	//       notified yet or is more precise than the shown decimals, and the restore itself raises no ValueChanged;
	//       the range limits themselves are accepted
	//   N2  a change of one step of the last decimal place raises DebouncedValueChanged once; changes within the
	//       debounce interval are notified once, with the last value, not before the interval after the last change
	//   N3  one notch of the mouse wheel changes the value by one Increment, clamped to the range;
	//       ReadOnly blocks the wheel and the up / down buttons; the up / down buttons first reject typed text that
	//       is not validated yet, then step from the latest valid value
	//   N4  SilentSetValue cancels a pending notification without blocking later ones; an empty field restores the
	//       latest valid value on Enter (the key is consumed) and on leaving, and leaving sends a pending notification
	//
	// Usage: UICheckControls.exe
	internal static class ControlRunner
	{
		[STAThread]
		static int Main()
		{
			CheckRejectedInput();
			CheckRangeLimits();
			CheckLastDecimalStep();
			CheckDebounceMerge();
			CheckMouseWheel();
			CheckUpDownButtons();
			CheckSilentSetValue();
			CheckEmptyInput();
			Console.WriteLine( "[controls] " + m_nCaseCount + " cases, " + m_nProblemCount + " problems" );
			return m_nProblemCount == 0 ? 0 : 1;
		}

		static void CheckRejectedInput()
		{
			// one step of the last decimal place, then unparseable text, as in AxisTransformDlg
			ProbeNumeric numeric = CreateNumeric( 0m, -100m, 100m );
			List<decimal> valueList = RecordValueChanged( numeric );
			numeric.Value = 0.001m;
			numeric.EnterText( "abc" );
			Expect( "N1", "unparseable after one step", 0.001m, numeric.Value );
			ExpectEvents( "N1", "unparseable after one step", new decimal[] { 0.001m }, valueList );

			// a change that is still debouncing, then a value above the maximum
			numeric = CreateNumeric( 10m, 0m, 1000m );
			valueList = RecordValueChanged( numeric );
			numeric.Value = 20m;
			numeric.EnterText( "2000" );
			Expect( "N1", "above maximum while debouncing", 20m, numeric.Value );
			ExpectEvents( "N1", "above maximum while debouncing", new decimal[] { 20m }, valueList );

			// a notified value, then a value below the minimum
			numeric = CreateNumeric( 10m, 0m, 1000m );
			numeric.Value = 30m;
			PumpUntil( () => false, 300 );
			valueList = RecordValueChanged( numeric );
			numeric.EnterText( "-5" );
			Expect( "N1", "below minimum after notify", 30m, numeric.Value );
			ExpectEvents( "N1", "below minimum after notify", new decimal[ 0 ], valueList );

			// a loaded value more precise than the 3 shown decimals, then unparseable and out-of-range text
			numeric = CreateNumeric( 1.23456m, -100m, 100m );
			valueList = RecordValueChanged( numeric );
			numeric.EnterText( "abc" );
			Expect( "N1", "unparseable keeps full precision", 1.23456m, numeric.Value );
			numeric.EnterText( "200" );
			Expect( "N1", "above maximum keeps full precision", 1.23456m, numeric.Value );

			// a later validation with nothing typed (e.g. another Enter) must not parse the shown, rounded text
			numeric.Revalidate();
			Expect( "N1", "validation without typing keeps full precision", 1.23456m, numeric.Value );
			ExpectEvents( "N1", "rejected input on a precise value", new decimal[ 0 ], valueList );
		}

		static void CheckRangeLimits()
		{
			ProbeNumeric numeric = CreateNumeric( 20m, 0m, 1000m );
			List<decimal> valueList = RecordValueChanged( numeric );
			numeric.EnterText( "0" );
			Expect( "N1", "minimum accepted", 0m, numeric.Value );
			numeric.EnterText( "1000" );
			Expect( "N1", "maximum accepted", 1000m, numeric.Value );
			numeric.EnterText( "-0.001" );
			Expect( "N1", "just below minimum rejected", 1000m, numeric.Value );
			numeric.EnterText( "1000.001" );
			Expect( "N1", "just above maximum rejected", 1000m, numeric.Value );
			ExpectEvents( "N1", "range limits", new decimal[] { 0m, 1000m }, valueList );
		}

		static void CheckLastDecimalStep()
		{
			ProbeNumeric numeric = CreateNumeric( 1m, -100m, 100m );
			int nNotifyCount = 0;
			decimal notifiedValue = 0m;
			numeric.DebouncedValueChanged += ( sender, e ) =>
			{
				nNotifyCount++;
				notifiedValue = numeric.Value;
			};
			numeric.Value = 1.001m;
			PumpUntil( () => nNotifyCount > 0, 1000 );

			// let a second, wrong notification arrive if there is one
			PumpUntil( () => false, 300 );
			Expect( "N2", "notify count of one step", 1m, nNotifyCount );
			Expect( "N2", "notified value of one step", 1.001m, notifiedValue );
		}

		static void CheckDebounceMerge()
		{
			// interval 500 ms, the second change about 200 ms after the first: a timer that is not restarted by the
			// second change fires about 300 ms after it, inside the window that must stay silent
			ProbeNumeric numeric = CreateNumeric( 10m, 0m, 100m );
			numeric.DebounceInterval = 500;
			List<decimal> notifiedList = RecordDebounced( numeric );
			numeric.Value = 20m;
			PumpUntil( () => false, 200 );
			numeric.Value = 21m;
			PumpUntil( () => notifiedList.Count > 0, 400 );
			ExpectEvents( "N2", "no notification within the interval after the last change", new decimal[ 0 ],
				notifiedList, "DebouncedValueChanged" );
			PumpUntil( () => notifiedList.Count > 0, 2000 );
			PumpUntil( () => false, 600 );
			ExpectEvents( "N2", "two changes within the interval", new decimal[] { 21m },
				notifiedList, "DebouncedValueChanged" );
		}

		static void CheckMouseWheel()
		{
			ProbeNumeric numeric = CreateNumeric( 10m, 0m, 11m );
			numeric.Increment = 0.5m;
			numeric.Wheel( 120 );
			Expect( "N3", "one notch up", 10.5m, numeric.Value );
			numeric.Wheel( -120 );
			Expect( "N3", "one notch down", 10m, numeric.Value );
			numeric.Value = 10.8m;
			numeric.Wheel( 120 );
			Expect( "N3", "notch up clamped to maximum", 11m, numeric.Value );

			numeric = CreateNumeric( 0.2m, 0m, 11m );
			numeric.Increment = 0.5m;
			numeric.Wheel( -120 );
			Expect( "N3", "notch down clamped to minimum", 0m, numeric.Value );

			numeric = CreateNumeric( 5m, 0m, 11m );
			numeric.ReadOnly = true;
			numeric.Wheel( 120 );
			numeric.UpButton();
			numeric.DownButton();
			numeric.DownButton();
			Expect( "N3", "read-only blocks wheel and buttons", 5m, numeric.Value );
		}

		static void CheckUpDownButtons()
		{
			// typed text that is not validated yet, then a button at once; the step starts from the latest valid value
			CheckButtonAfterTyping( 20m, "2000", true, 21m );
			CheckButtonAfterTyping( 20m, "2000", false, 19m );
			CheckButtonAfterTyping( 20m, "-1", true, 21m );
			CheckButtonAfterTyping( 20m, "-1", false, 19m );
			CheckButtonAfterTyping( 1.23456m, "2000", false, 0.23456m );

			// valid typed text is accepted first, then stepped
			ProbeNumeric numeric = CreateNumeric( 20m, 0m, 1000m );
			List<decimal> valueList = RecordValueChanged( numeric );
			numeric.TypeText( "30" );
			numeric.UpButton();
			ExpectEvents( "N3", "valid text then up button", new decimal[] { 30m, 31m }, valueList );
		}

		static void CheckButtonAfterTyping( decimal startValue, string szText, bool isUp, decimal expected )
		{
			ProbeNumeric numeric = CreateNumeric( startValue, 0m, 1000m );
			List<decimal> valueList = RecordValueChanged( numeric );
			numeric.TypeText( szText );
			if( isUp ) {
				numeric.UpButton();
			}
			else {
				numeric.DownButton();
			}
			string szCase = "'" + szText + "' then " + ( isUp ? "up" : "down" ) + " button from " + startValue;
			Expect( "N3", szCase, expected, numeric.Value );
			ExpectEvents( "N3", szCase, new decimal[] { expected }, valueList );
		}

		static void CheckSilentSetValue()
		{
			ProbeNumeric numeric = CreateNumeric( 10m, 0m, 100m );
			List<decimal> notifiedList = RecordDebounced( numeric );
			numeric.Value = 20m;
			numeric.SilentSetValue = 30m;
			PumpUntil( () => false, 300 );
			ExpectEvents( "N4", "silent set cancels the pending notification", new decimal[ 0 ],
				notifiedList, "DebouncedValueChanged" );
			numeric.Value = 30.001m;
			PumpUntil( () => notifiedList.Count > 0, 1000 );
			PumpUntil( () => false, 300 );
			ExpectEvents( "N4", "change after silent set is notified", new decimal[] { 30.001m },
				notifiedList, "DebouncedValueChanged" );
		}

		static void CheckEmptyInput()
		{
			// leaving an empty field while a change is pending
			ProbeNumeric numeric = CreateNumeric( 10m, 0m, 100m );
			List<decimal> valueList = RecordValueChanged( numeric );
			List<decimal> notifiedList = RecordDebounced( numeric );
			numeric.Value = 20m;
			numeric.ClearText();
			numeric.LeaveField();
			Expect( "N4", "empty field on leave", 20m, numeric.Value );
			ExpectEvents( "N4", "empty field on leave", new decimal[] { 20m }, valueList );
			ExpectEvents( "N4", "leave sends the pending notification", new decimal[] { 20m },
				notifiedList, "DebouncedValueChanged" );

			// Enter on an empty field after the value is notified
			numeric = CreateNumeric( 10m, 0m, 100m );
			numeric.Value = 20m;
			PumpUntil( () => false, 300 );
			valueList = RecordValueChanged( numeric );
			numeric.ClearText();
			KeyEventArgs keyArgs = numeric.PressEnter();
			Expect( "N4", "empty field on Enter", 20m, numeric.Value );
			ExpectEvents( "N4", "empty field on Enter", new decimal[ 0 ], valueList );
			ExpectTrue( "N4", "Enter on an empty field is consumed", keyArgs.Handled && keyArgs.SuppressKeyPress );
			ExpectTrue( "N4", "empty field shows the value again", numeric.Text.Length > 0 );
		}

		static ProbeNumeric CreateNumeric( decimal initialValue, decimal minimum, decimal maximum )
		{
			ProbeNumeric numeric = new ProbeNumeric();
			numeric.Minimum = minimum;
			numeric.Maximum = maximum;
			numeric.SilentSetValue = initialValue;
			return numeric;
		}

		static List<decimal> RecordValueChanged( NumericUpDown numeric )
		{
			List<decimal> valueList = new List<decimal>();
			numeric.ValueChanged += ( sender, e ) => valueList.Add( numeric.Value );
			return valueList;
		}

		static List<decimal> RecordDebounced( DebouncedNumericUpDown numeric )
		{
			List<decimal> notifiedList = new List<decimal>();
			numeric.DebouncedValueChanged += ( sender, e ) => notifiedList.Add( numeric.Value );
			return notifiedList;
		}

		// the debounce timer is a WinForms timer, so messages are pumped while waiting
		static void PumpUntil( Func<bool> isDone, int nTimeoutMs )
		{
			Stopwatch watch = Stopwatch.StartNew();
			while( !isDone() && watch.ElapsedMilliseconds < nTimeoutMs ) {
				Application.DoEvents();
				Thread.Sleep( 10 );
			}
		}

		static void Expect( string szRule, string szCase, decimal expected, decimal actual )
		{
			m_nCaseCount++;
			if( expected == actual ) {
				return;
			}
			m_nProblemCount++;
			Console.WriteLine( "FAIL  " + szRule + " " + szCase + ": expected " + expected + ", got " + actual );
		}

		static void ExpectTrue( string szRule, string szCase, bool isPassed )
		{
			m_nCaseCount++;
			if( isPassed ) {
				return;
			}
			m_nProblemCount++;
			Console.WriteLine( "FAIL  " + szRule + " " + szCase );
		}

		static void ExpectEvents( string szRule, string szCase, decimal[] expectedArray, List<decimal> actualList,
			string szEventName = "ValueChanged" )
		{
			m_nCaseCount++;
			string szExpected = string.Join( ", ", expectedArray );
			string szActual = string.Join( ", ", actualList );
			if( szExpected == szActual ) {
				return;
			}
			m_nProblemCount++;
			Console.WriteLine( "FAIL  " + szRule + " " + szCase + ": " + szEventName + " expected [" + szExpected
				+ "], got [" + szActual + "]" );
		}

		static int m_nCaseCount;
		static int m_nProblemCount;

		// reaches the protected input paths of the control the way user input does
		sealed class ProbeNumeric : DebouncedNumericUpDown
		{
			// type into the inner editor, then validate as Enter or leaving does
			public void EnterText( string szText )
			{
				TypeText( szText );
				ValidateEditText();
			}

			public void Revalidate()
			{
				ValidateEditText();
			}

			public void Wheel( int nDelta )
			{
				OnMouseWheel( new HandledMouseEventArgs( MouseButtons.None, 0, 0, 0, nDelta ) );
			}

			// the user deletes the text; it is validated only on Enter or on leaving
			public void ClearText()
			{
				TypeText( string.Empty );
			}

			// unlike setting Text of the control, typing into the editor does not validate at once
			public void TypeText( string szText )
			{
				foreach( Control child in Controls ) {
					TextBox editor = child as TextBox;
					if( editor != null ) {
						editor.Text = szText;
						return;
					}
				}
				throw new InvalidOperationException( "No numeric editor found." );
			}

			public KeyEventArgs PressEnter()
			{
				KeyEventArgs keyArgs = new KeyEventArgs( Keys.Enter );
				OnKeyDown( keyArgs );
				return keyArgs;
			}

			public void LeaveField()
			{
				OnLostFocus( EventArgs.Empty );
			}
		}
	}
}
