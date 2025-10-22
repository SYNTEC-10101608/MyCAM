namespace MyCAM.Data
{
	public class TraverseData
	{
		public TraverseData( double liftUpDistance, double cutDownDistance, double followSafeDistance, double frogLeapDistance, bool enableFrogLeap )
		{
			LiftUpDistance = liftUpDistance;
			CutDownDistance = cutDownDistance;
			FollowSafeDistance = followSafeDistance;
			FrogLeapDistance = frogLeapDistance;
			EnableFrogLeap = enableFrogLeap;
		}

		public TraverseData()
		{
		}

		public double LiftUpDistance
		{
			get; set;
		}

		public double CutDownDistance
		{
			get; set;
		}

		public double FollowSafeDistance
		{
			get; set;
		}

		public double FrogLeapDistance
		{
			get; set;
		}

		public bool EnableFrogLeap
		{
			get; set;
		}

		public TraverseData Clone()
		{
			return new TraverseData( LiftUpDistance, CutDownDistance, FollowSafeDistance, FrogLeapDistance, EnableFrogLeap );
		}

		public static double LIFT_UP_DISTANCE = 200.0;
		public static double CUT_DOWN_DISTANCE = 200.0;
		public static double FOLLOW_SAFE_DISTANCE = 5.0;
		public static double FROG_LEAP_DISTANCE = 200.0;
		public static bool ENABLE_FROG_LEAP = true;
	}
}



