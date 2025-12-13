using System;

namespace MyCAM.Data
{
	public enum LeadGeomType
	{
		None = 0,
		Line,
		Arc,
	}

	public class LeadGeom
	{
		public LeadGeom( LeadGeomType type = LeadGeomType.None, double length = 0, double angle = 0 )
		{
			Type = type;
			Length = length;
			Angle = angle;
		}

		public LeadGeomType Type
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
		public LeadData( LeadGeomType leadInType = LeadGeomType.None, LeadGeomType leadOutType = LeadGeomType.None,
			double dLeadInLength = 0, double dLeadInAngle = 0, double dLeadOutLength = 0, double dLeadOutAngle = 0, bool isChangeLeadDirection = false )
		{
			LeadIn = new LeadGeom( leadInType, dLeadInLength, dLeadInAngle );
			LeadOut = new LeadGeom( leadOutType, dLeadOutLength, dLeadOutAngle );
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
				LeadIn.Type,
				LeadOut.Type,
				LeadIn.Length,
				LeadIn.Angle,
				LeadOut.Length,
				LeadOut.Angle,
				IsChangeLeadDirection
			);
		}

		LeadGeom m_LeadIn;
		LeadGeom m_LeadOut;
		bool m_IsChangeLeadDirection;
	}
}

