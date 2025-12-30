using MyCAM.Data;

namespace MyCAM.Post
{
	internal interface INCPackage
	{
		TraverseData TraverseData
		{
			get;
		}

		IProcessPoint ProcessStartPoint
		{
			get;
		}
	}

	internal interface ITraversePostData
	{
		PostPoint ProcessStartPoint
		{
			get; set;
		}

		PostPoint LiftUpPostPoint
		{
			get; set;
		}

		PostPoint CutDownPostPoint
		{
			get; set;
		}

		PostPoint FrogLeapMidPostPoint
		{
			get; set;
		}

		double FollowSafeDistance
		{
			get; set;
		}
	}
}
