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
		public static double FOLLOW_SAFE_DISTANCE = 0.0;
	}

	public class TraverseData
	{
		public enum TraverseType
		{
			FrogLeap,
			SafePlane,
			SafeCylinder,
			SafeSphere
		}

		public TraverseData( double liftUpDistance, double cutDownDistance, double followSafeDistance,
			TraverseType traverseMode, double frogLeapDistance, double safePlaneDistance, double safeCylinderRadius, double safeSphereRadius )
		{
			m_LiftUpDistance = liftUpDistance;
			m_CutDownDistance = cutDownDistance;
			m_FollowSafeDistance = followSafeDistance;
			m_TraverseMode = traverseMode;
			m_FrogLeapDistance = frogLeapDistance;
			m_SafePlaneDistance = safePlaneDistance;
			m_SafeCylinderRadius = safeCylinderRadius;
			m_SafeSphereRadius = safeSphereRadius;
		}

		public TraverseData()
		{

		}

		public Action PropertyChanged;

		public TraverseType TraverseMode
		{
			get
			{
				return m_TraverseMode;
			}
			set
			{
				if( m_TraverseMode != value ) {
					m_TraverseMode = value;
					PropertyChanged?.Invoke();
				}
			}
		}

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

		public double SafePlaneDistance
		{
			get
			{
				return m_SafePlaneDistance;
			}
			set
			{
				if( m_SafePlaneDistance != value ) {
					m_SafePlaneDistance = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public double SafeCylinderRadius
		{
			get
			{
				return m_SafeCylinderRadius;
			}
			set
			{
				if( m_SafeCylinderRadius != value ) {
					m_SafeCylinderRadius = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public double SafeSphereRadius
		{
			get
			{
				return m_SafeSphereRadius;
			}
			set
			{
				if( m_SafeSphereRadius != value ) {
					m_SafeSphereRadius = value;
					PropertyChanged?.Invoke();
				}
			}
		}

		public TraverseData Clone()
		{
			return new TraverseData( LiftUpDistance, CutDownDistance, FollowSafeDistance, TraverseMode, FrogLeapDistance, SafePlaneDistance, SafeCylinderRadius, SafeSphereRadius );
		}

		public static double LIFT_UP_DISTANCE = 0.0;
		public static double CUT_DOWN_DISTANCE = 0.0;
		public static double FOLLOW_SAFE_DISTANCE = 0.0;
		public static TraverseType TRAVERSE_MODE = TraverseType.FrogLeap;
		public static double FROG_LEAP_DISTANCE = 10.0;
		public static double SAFE_PLANE_DISTANCE = 20.0;
		public static double SAFE_CYLINDER_RADIUS = 20.0;
		public static double SAFE_SPHERE_RADIUS = 20.0;
		double m_LiftUpDistance = LIFT_UP_DISTANCE;
		double m_CutDownDistance = CUT_DOWN_DISTANCE;
		double m_FollowSafeDistance = FOLLOW_SAFE_DISTANCE;
		TraverseType m_TraverseMode = TRAVERSE_MODE;
		double m_FrogLeapDistance = FROG_LEAP_DISTANCE;
		double m_SafePlaneDistance = SAFE_PLANE_DISTANCE;
		double m_SafeCylinderRadius = SAFE_CYLINDER_RADIUS;
		double m_SafeSphereRadius = SAFE_SPHERE_RADIUS;

	}
}
