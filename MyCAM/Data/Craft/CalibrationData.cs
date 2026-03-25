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
			return new CalibrationData( pnt1, pnt2, pnt3 );
		}

		public void DoTransform( gp_Trsf trsf )
		{
			if( !IsBeenSet ) {
				return;
			}
			Ref_Pnt1.Transform( trsf );
			Ref_Pnt2.Transform( trsf );
			Ref_Pnt3.Transform( trsf );
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
				ChangeState();
			}
		}

		public bool IsBeenSet
		{
			get => m_IsBeenSet;
		}

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