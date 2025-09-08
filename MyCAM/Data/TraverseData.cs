namespace MyCAM.Data
{
	public class TraverseData
	{
		public TraverseData( double liftUpDistance, double cutDownDistance, double followSafeDistance )
		{
			m_LiftUpDistance = liftUpDistance;
			m_FollowSafeDistance = followSafeDistance;
			m_CutDownDistance = cutDownDistance;
		}

		public TraverseData()
		{

		}

		public double LiftUpDistance
		{
			get
			{
				return m_LiftUpDistance;
			}
			set
			{
				m_LiftUpDistance = value;
			}
		}

		public double CutDownDistance
		{
			get
			{
				return m_CutDownDistance;
			}
			set
			{
				m_CutDownDistance = value;
			}
		}

		public double FollowSafeDistance
		{
			get
			{
				return m_FollowSafeDistance;
			}
			set
			{
				m_FollowSafeDistance = value;
			}
		}

		public TraverseData Clone()
		{
			return new TraverseData( LiftUpDistance, CutDownDistance, FollowSafeDistance );
		}

		public static double LIFT_UP_DISTANCE = 200.0;
		public static double CUT_DOWN_DISTANCE = 200.0;
		public static double FOLLOW_SAFE_DISTANCE = 5.0;

		double m_LiftUpDistance = LIFT_UP_DISTANCE;
		double m_CutDownDistance = CUT_DOWN_DISTANCE;
		double m_FollowSafeDistance = FOLLOW_SAFE_DISTANCE;
	}
}



