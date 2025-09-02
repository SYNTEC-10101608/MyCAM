namespace MyCAM.Data
{
	internal class LeadType
	{
		public enum LeadLineType
		{
			None = 0,
			Line,
			Arc,
		}
	}

	internal class LeadData
	{
		public LeadData( LeadType.LeadLineType leadInType = LeadType.LeadLineType.None, LeadType.LeadLineType leadOutType = LeadType.LeadLineType.None,
			double dLeadInLength = 0, double dLeadInAngle = 0, double dLeadOutLength = 0, double dLeadOutAngle = 0, bool isChangeLeadDirection = false )
		{
			LeadIn = new LeadParam( leadInType, dLeadInLength, dLeadInAngle );
			LeadOut = new LeadParam( leadOutType, dLeadOutLength, dLeadOutAngle );
			IsChangeLeadDirection = isChangeLeadDirection;
		}

		public LeadParam LeadIn
		{
			get; set;
		}

		public LeadParam LeadOut
		{
			get; set;
		}

		public bool IsChangeLeadDirection
		{
			get; set;
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

		internal class LeadParam
		{
			public LeadParam( LeadType.LeadLineType type = LeadType.LeadLineType.None, double length = 0, double angle = 0 )
			{
				Type = type;
				Length = length;
				Angle = angle;
			}

			public LeadType.LeadLineType Type
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
	}
}

