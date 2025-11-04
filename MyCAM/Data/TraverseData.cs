namespace MyCAM.Data
{
	public class EntryAndExitData
	{
		public EntryAndExitData( double entryDistance, double exitDistance, double followSafeDistance )
		{
			EntryDistance = entryDistance;
			ExitDistance = exitDistance;
			FollowSafeDistance = followSafeDistance;
		}

		public EntryAndExitData()
		{
			EntryDistance = ENTRY_DISTANCE;
			ExitDistance = EXIT_DISTANCE;
			FollowSafeDistance = FOLLOW_SAFE_DISTANCE;
		}

		public double EntryDistance
		{
			get; set;
		}

		public double ExitDistance
		{
			get; set;
		}

		public double FollowSafeDistance
		{
			get; set;
		}

		public EntryAndExitData Clone()
		{
			return new EntryAndExitData( EntryDistance, ExitDistance, FollowSafeDistance );
		}

		public static double ENTRY_DISTANCE = 100.0;
		public static double EXIT_DISTANCE = 100.0;
		public static double FOLLOW_SAFE_DISTANCE = 5.0;
	}

	public class TraverseData
	{
		public TraverseData( double liftUpDistance, double cutDownDistance, double followSafeDistance, double frogLeapDistance )
		{
			LiftUpDistance = liftUpDistance;
			CutDownDistance = cutDownDistance;
			FollowSafeDistance = followSafeDistance;
			FrogLeapDistance = frogLeapDistance;
		}

		public TraverseData()
		{
			LiftUpDistance = LIFT_UP_DISTANCE;
			CutDownDistance = CUT_DOWN_DISTANCE;
			FollowSafeDistance = FOLLOW_SAFE_DISTANCE;
			FrogLeapDistance = FROG_LEAP_DISTANCE;
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

		public TraverseData Clone()
		{
			return new TraverseData( LiftUpDistance, CutDownDistance, FollowSafeDistance, FrogLeapDistance );
		}

		public static double LIFT_UP_DISTANCE = 0.0;
		public static double CUT_DOWN_DISTANCE = 0.0;
		public static double FOLLOW_SAFE_DISTANCE = 5.0;
		public static double FROG_LEAP_DISTANCE = 10.0;
	}
}
