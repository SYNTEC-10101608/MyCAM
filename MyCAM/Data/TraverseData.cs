namespace MyCAM.Data
{
	public class EntryAndExitData
	{
		public EntryAndExitData( double entryDistance, double exitDistance )
		{
			EntryDistance = entryDistance;
			ExitDistance = exitDistance;
		}

		public double EntryDistance
		{
			get; set;
		}

		public double ExitDistance
		{
			get; set;
		}

		public EntryAndExitData Clone()
		{
			return new EntryAndExitData( EntryDistance, ExitDistance );
		}
	}

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
			LiftUpDistance = LIFT_UP_DISTANCE;
			CutDownDistance = CUT_DOWN_DISTANCE;
			FollowSafeDistance = FOLLOW_SAFE_DISTANCE;
			FrogLeapDistance = FROG_LEAP_DISTANCE;
			EnableFrogLeap = ENABLE_FROG_LEAP;
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

		public static double LIFT_UP_DISTANCE = 0.0;
		public static double CUT_DOWN_DISTANCE = 0.0;
		public static double FOLLOW_SAFE_DISTANCE = 5.0;
		public static double FROG_LEAP_DISTANCE = 10.0;
		public static bool ENABLE_FROG_LEAP = true;
	}
}



