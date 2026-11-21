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
	//       notified yet, and the restore itself raises no ValueChanged
	//   N2  a change of one step of the last decimal place raises DebouncedValueChanged once
	//   N3  one notch of the mouse wheel changes the value by one Increment, clamped to the range
	//
	// Usage: UICheckControls.exe
	internal static class ControlRunner
	{
		[STAThread]
		static int Main()
		{
			CheckRejectedInput();
			CheckLastDecimalStep();
			CheckMouseWheel();
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

		static void ExpectEvents( string szRule, string szCase, decimal[] expectedArray, List<decimal> actualList )
		{
			m_nCaseCount++;
			string szExpected = string.Join( ", ", expectedArray );
			string szActual = string.Join( ", ", actualList );
			if( szExpected == szActual ) {
				return;
			}
			m_nProblemCount++;
			Console.WriteLine( "FAIL  " + szRule + " " + szCase + ": ValueChanged expected [" + szExpected + "], got ["
				+ szActual + "]" );
		}

		static int m_nCaseCount;
		static int m_nProblemCount;

		// reaches the protected input paths of the control the way user input does
		sealed class ProbeNumeric : DebouncedNumericUpDown
		{
			public void EnterText( string szText )
			{
				Text = szText;
				ValidateEditText();
			}

			public void Wheel( int nDelta )
			{
				OnMouseWheel( new HandledMouseEventArgs( MouseButtons.None, 0, 0, 0, nDelta ) );
			}
		}
	}
}
