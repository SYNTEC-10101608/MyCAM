using System;

namespace MyCAM.Data
{
	public enum LeadLineType
	{
		None = 0,
		Line,
		Arc,
	}

	public class LeadParam
	{
		public LeadParam( LeadLineType type = LeadLineType.None, double length = 0, double angle = 0 )
		{
			Type = type;
			Length = length;
			Angle = angle;
		}

		public LeadLineType Type
		{
			get; set;
		}

		public double Length
		{
			get; set;
		}

		public double Angle
		{
			get; set;
		}
	}

	public class LeadData
	{
		public LeadData( LeadLineType leadInType = LeadLineType.None, LeadLineType leadOutType = LeadLineType.None,
			double dLeadInLength = 0, double dLeadInAngle = 0, double dLeadOutLength = 0, double dLeadOutAngle = 0, bool isChangeLeadDirection = false )
		{
			LeadIn = new LeadParam( leadInType, dLeadInLength, dLeadInAngle );
			LeadOut = new LeadParam( leadOutType, dLeadOutLength, dLeadOutAngle );
			IsChangeLeadDirection = isChangeLeadDirection;
		}

		public Action LeadPropertyChanged;

		public LeadParam LeadIn
		{
			get
			{
				if( m_LeadIn == null ) {
					m_LeadIn = new LeadParam();
				}
				return m_LeadIn;
			}
			set
			{
				if( m_LeadIn != value ) {
					m_LeadIn = value;
					LeadPropertyChanged?.Invoke();
				}
			}
		}

		public LeadParam LeadOut
		{
			get
			{
				if( m_LeadOut == null ) {
					m_LeadOut = new LeadParam();
				}
				return m_LeadOut;
			}
			set
			{
				if( m_LeadOut != value ) {
					m_LeadOut = value;
					LeadPropertyChanged?.Invoke();
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
					LeadPropertyChanged?.Invoke();
				}
			}
		}

		public LeadData Clone()
		{
			return new LeadData(
				LeadIn.Type,
				LeadOut.Type,
				LeadIn.Length,
				LeadIn.Angle,
				LeadOut.Length,
				LeadOut.Angle,
				IsChangeLeadDirection
			);
		}

		LeadParam m_LeadIn;
		LeadParam m_LeadOut;
		bool m_IsChangeLeadDirection;
	}
}

