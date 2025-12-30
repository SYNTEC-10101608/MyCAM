using System;

namespace MyCAM.Data
{
	public class LeadGeom
	{
		public LeadGeom( double straightLength = 0, double arclength = 0, double angle = 90 )
		{
			StraightLength = straightLength;
			ArcLength = arclength;
			Angle_deg = angle;
		}

		public double StraightLength
		{
			get; set;
		}

		public double ArcLength
		{
			get; set;
		}

		public double Angle_deg
		{
			get; set;
		}
	}

	public class LeadData
	{
		public LeadData( double dLeadInStraightLineLength = 0, double dLeadInArcLength = 0, double dLeadInAngle = 90,
						double dLeadOutStraightLineLength = 0, double dLeadOutArcLength = 0, double dLeadOutAngle = 90,
						bool isChangeLeadDirection = false )
		{
			LeadIn = new LeadGeom( dLeadInStraightLineLength, dLeadInArcLength, dLeadInAngle );
			LeadOut = new LeadGeom( dLeadOutStraightLineLength, dLeadOutArcLength, dLeadOutAngle );
			IsChangeLeadDirection = isChangeLeadDirection;
		}

		public Action PropertyChanged;

		public LeadGeom LeadIn
		{
			get
			{
				if( m_LeadIn == null ) {
					m_LeadIn = new LeadGeom();
				}
				return m_LeadIn;
			}
			set
			{
				if( m_LeadIn != value ) {
					m_LeadIn = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public LeadGeom LeadOut
		{
			get
			{
				if( m_LeadOut == null ) {
					m_LeadOut = new LeadGeom();
				}
				return m_LeadOut;
			}
			set
			{
				if( m_LeadOut != value ) {
					m_LeadOut = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public bool IsChangeLeadDirection
		{
			get
			{
				return m_IsChangeLeadDirection;
			}
			set
			{
				if( m_IsChangeLeadDirection != value ) {
					m_IsChangeLeadDirection = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public LeadData Clone()
		{
			return new LeadData(
				LeadIn.StraightLength,
				LeadIn.ArcLength,
				LeadIn.Angle_deg,
				LeadOut.StraightLength,
				LeadOut.ArcLength,
				LeadOut.Angle_deg,
				IsChangeLeadDirection
			);
		}

		LeadGeom m_LeadIn;
		LeadGeom m_LeadOut;
		bool m_IsChangeLeadDirection;
	}
}

