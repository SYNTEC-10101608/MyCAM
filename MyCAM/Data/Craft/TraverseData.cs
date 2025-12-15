using System;

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
			m_LiftUpDistance = liftUpDistance;
			m_CutDownDistance = cutDownDistance;
			m_FollowSafeDistance = followSafeDistance;
			m_FrogLeapDistance = frogLeapDistance;
		}

		public TraverseData()
		{

		}

		public Action PropertyChanged;

		public double LiftUpDistance
		{
			get
			{
				return m_LiftUpDistance;
			}
			set
			{
				if( m_LiftUpDistance != value ) {
					m_LiftUpDistance = value;
					PropertyChanged?.Invoke();
				}
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
				if( m_CutDownDistance != value ) {
					m_CutDownDistance = value;
					PropertyChanged?.Invoke();
				}
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
				if( m_FollowSafeDistance != value ) {
					m_FollowSafeDistance = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public double FrogLeapDistance
		{
			get
			{
				return m_FrogLeapDistance;
			}
			set
			{
				if( m_FrogLeapDistance != value ) {
					m_FrogLeapDistance = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public TraverseData Clone()
		{
			return new TraverseData( LiftUpDistance, CutDownDistance, FollowSafeDistance, FrogLeapDistance );
		}

		public static double LIFT_UP_DISTANCE = 0.0;
		public static double CUT_DOWN_DISTANCE = 0.0;
		public static double FOLLOW_SAFE_DISTANCE = 5.0;
		public static double FROG_LEAP_DISTANCE = 10.0;
		double m_LiftUpDistance = LIFT_UP_DISTANCE;
		double m_CutDownDistance = CUT_DOWN_DISTANCE;
		double m_FollowSafeDistance = FOLLOW_SAFE_DISTANCE;
		double m_FrogLeapDistance = FROG_LEAP_DISTANCE;
	}
}
