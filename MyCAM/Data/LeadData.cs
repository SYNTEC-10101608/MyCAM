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
		public LeadParam LeadIn { get; set; }

		public LeadParam LeadOut { get; set; }

		public bool IsChangeLeadDirection { get; set; }

		public LeadData( LeadType.LeadLineType leadInType, LeadType.LeadLineType leadOutType, double dLeadInLength, double dLeadInAngle, double dLeadOutLength, double dLeadOutAngle, bool isChangeLeadDirection )
		{
			LeadIn = new LeadParam( leadInType, dLeadInLength, dLeadInAngle );
			LeadOut = new LeadParam( leadOutType, dLeadOutLength, dLeadOutAngle );
			IsChangeLeadDirection = isChangeLeadDirection;
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
			public LeadType.LeadLineType Type;
			public double Length;
			public double Angle;
			public LeadParam( LeadType.LeadLineType type = LeadType.LeadLineType.None, double length = 0, double angle = 0 )
			{
				Type = type;
				Length = length;
				Angle = angle;
			}
		}
	}
}

