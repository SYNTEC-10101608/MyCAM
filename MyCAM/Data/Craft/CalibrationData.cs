using OCC.gp;

namespace MyCAM.Data
{
	public class CalibrationData
	{
		public CalibrationData()
		{
		}

		public CalibrationData( gp_Pnt refPnt1, gp_Pnt refPnt2, gp_Pnt refPnt3 )
		{
			Ref_Pnt1 = refPnt1;
			Ref_Pnt2 = refPnt2;
			Ref_Pnt3 = refPnt3;
		}

		public CalibrationData Clone()
		{
			gp_Pnt pnt1 = Ref_Pnt1 == null ? null : new gp_Pnt( Ref_Pnt1.x, Ref_Pnt1.y, Ref_Pnt1.z );
			gp_Pnt pnt2 = Ref_Pnt2 == null ? null : new gp_Pnt( Ref_Pnt2.x, Ref_Pnt2.y, Ref_Pnt2.z );
			gp_Pnt pnt3 = Ref_Pnt3 == null ? null : new gp_Pnt( Ref_Pnt3.x, Ref_Pnt3.y, Ref_Pnt3.z );
			CalibrationData clone = new CalibrationData( pnt1, pnt2, pnt3 );
			clone.DrawingOnly = DrawingOnly;
			clone.SetWorkpieceData( Work_Pnt1, Work_Pnt2, Work_Pnt3, Tolerance );
			clone.SetCalibrationError( CalibrationError, CalculationCompleted, CalculationSucceeded );
			if( HasTransmitted ) {
				clone.MarkTransmitted();
			}
			return clone;
		}

		public void DoTransform( gp_Trsf trsf )
		{
			if( !IsBeenSet ) {
				return;
			}
			Ref_Pnt1.Transform( trsf );
			Ref_Pnt2.Transform( trsf );
			Ref_Pnt3.Transform( trsf );
			ClearCalculation();
		}

		public gp_Pnt Ref_Pnt1
		{
			get
			{
				return m_REFPnt1;
			}
			set
			{
				m_REFPnt1 = value;
				ClearCalculation();
				ChangeState();
			}
		}

		public gp_Pnt Ref_Pnt2
		{
			get
			{
				return m_REFPnt2;
			}
			set
			{
				m_REFPnt2 = value;
				ClearCalculation();
				ChangeState();
			}
		}

		public gp_Pnt Ref_Pnt3
		{
			get
			{
				return m_REFPnt3;
			}
			set
			{
				m_REFPnt3 = value;
				ClearCalculation();
				ChangeState();
			}
		}

		public bool IsBeenSet
		{
			get => m_IsBeenSet;
		}

		public gp_Pnt Work_Pnt1 { get; private set; }
		public gp_Pnt Work_Pnt2 { get; private set; }
		public gp_Pnt Work_Pnt3 { get; private set; }
		public double? Tolerance { get; private set; }
		public double? CalibrationError { get; private set; }
		public bool DrawingOnly { get; set; } = true;
		public bool CalculationCompleted { get; private set; }
		public bool? CalculationSucceeded { get; private set; }
		public bool HasTransmitted { get; private set; }
		public bool HasCalculatedSnapshot => HasCalibrationInputs && CalculationCompleted;
		public bool IsCalculationSuccessful => HasCalculatedSnapshot && CalculationSucceeded != false && CalibrationError.HasValue && CalibrationError.Value <= Tolerance.Value;
		public bool HasCalibrationInputs => IsBeenSet && Work_Pnt1 != null && Work_Pnt2 != null && Work_Pnt3 != null && Tolerance.HasValue;

		public void MarkTransmitted() => HasTransmitted = true;

		public void CopyFrom( CalibrationData data )
		{
			Ref_Pnt1 = CopyPoint( data.Ref_Pnt1 );
			Ref_Pnt2 = CopyPoint( data.Ref_Pnt2 );
			Ref_Pnt3 = CopyPoint( data.Ref_Pnt3 );
			DrawingOnly = data.DrawingOnly;
			SetWorkpieceData( data.Work_Pnt1, data.Work_Pnt2, data.Work_Pnt3, data.Tolerance );
			SetCalibrationError( data.CalibrationError, data.CalculationCompleted, data.CalculationSucceeded );
			if( data.HasTransmitted ) {
				MarkTransmitted();
			}
		}

		public bool HasSameInputs( CalibrationData other ) => other != null &&
			SamePoint( Ref_Pnt1, other.Ref_Pnt1 ) && SamePoint( Ref_Pnt2, other.Ref_Pnt2 ) && SamePoint( Ref_Pnt3, other.Ref_Pnt3 ) &&
			SamePoint( Work_Pnt1, other.Work_Pnt1 ) && SamePoint( Work_Pnt2, other.Work_Pnt2 ) && SamePoint( Work_Pnt3, other.Work_Pnt3 ) && Tolerance == other.Tolerance;

		public static bool SamePoint( gp_Pnt first, gp_Pnt second ) => first == null || second == null ? first == second
			: first.x == second.x && first.y == second.y && first.z == second.z;

		public static double Round( double value )
		{
			if( double.IsNaN( value ) || double.IsInfinity( value ) ) {
				throw new System.ArgumentOutOfRangeException( nameof( value ) );
			}
			if( value > (double)decimal.MinValue && value < (double)decimal.MaxValue ) {
				return (double)decimal.Round( System.Convert.ToDecimal( value ), 3, System.MidpointRounding.AwayFromZero );
			}
			return System.Math.Round( value, 3, System.MidpointRounding.AwayFromZero );
		}

		public void SetCalibrationInputs( gp_Pnt point1, gp_Pnt point2, gp_Pnt point3, double tolerance )
		{
			if( !IsBeenSet || !IsFinite( point1 ) || !IsFinite( point2 ) || !IsFinite( point3 ) || !IsFinite( Ref_Pnt1 ) || !IsFinite( Ref_Pnt2 ) || !IsFinite( Ref_Pnt3 ) ||
				double.IsNaN( tolerance ) || double.IsInfinity( tolerance ) || tolerance < 0 ) {
				throw new System.ArgumentException( "Invalid calibration inputs." );
			}
			SetWorkpieceData( point1, point2, point3, tolerance );
		}

		public void SetWorkpieceData( gp_Pnt point1, gp_Pnt point2, gp_Pnt point3, double? tolerance )
		{
			if( ( point1 != null && !IsFinite( point1 ) ) || ( point2 != null && !IsFinite( point2 ) ) || ( point3 != null && !IsFinite( point3 ) ) ||
				( tolerance.HasValue && ( double.IsNaN( tolerance.Value ) || double.IsInfinity( tolerance.Value ) || tolerance < 0 ) ) ) {
				throw new System.ArgumentException( "Invalid workpiece data." );
			}
			Work_Pnt1 = CopyPoint( point1 );
			Work_Pnt2 = CopyPoint( point2 );
			Work_Pnt3 = CopyPoint( point3 );
			Tolerance = tolerance;
			ClearCalculation();
		}

		public void SetCalibrationError( double? error, bool? completed = null, bool? succeeded = null )
		{
			if( error.HasValue && ( !HasCalibrationInputs || double.IsNaN( error.Value ) || double.IsInfinity( error.Value ) || error.Value < 0 ) ) {
				throw new System.ArgumentException( "Invalid calibration error." );
			}
			CalibrationError = error;
			CalculationCompleted = HasCalibrationInputs && ( completed ?? error.HasValue );
			CalculationSucceeded = CalculationCompleted ? succeeded : null;
		}

		void ClearCalculation()
		{
			CalibrationError = null;
			CalculationCompleted = false;
			CalculationSucceeded = null;
		}

		static gp_Pnt CopyPoint( gp_Pnt point ) => point == null ? null : new gp_Pnt( point.x, point.y, point.z );
		static bool IsFinite( gp_Pnt point ) => point != null && !double.IsNaN( point.x ) && !double.IsInfinity( point.x ) &&
			!double.IsNaN( point.y ) && !double.IsInfinity( point.y ) && !double.IsNaN( point.z ) && !double.IsInfinity( point.z );

		bool m_IsBeenSet = false;
		gp_Pnt m_REFPnt1;
		gp_Pnt m_REFPnt2;
		gp_Pnt m_REFPnt3;

		void ChangeState()
		{
			if( m_REFPnt1 != null && m_REFPnt2 != null && m_REFPnt3 != null ) {
				m_IsBeenSet = true;
				return;
			}
			m_IsBeenSet = false;
		}
	}
}
