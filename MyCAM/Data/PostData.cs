using System.Collections.Generic;

namespace MyCAM.Data
{
	internal class PostData
	{
		public List<PostPoint> LeadInPostPointList
		{
			get; private set;
		}

		public List<PostPoint> MainPathPostPointList
		{
			get; private set;
		}

		public List<PostPoint> OverCutPostPointList
		{
			get; private set;
		}

		public List<PostPoint> LeadOutPostPointList
		{
			get; private set;
		}

		// lift up of previous path, null meaning lift up distance is 0
		public PostPoint LiftUpPostPoint
		{
			get; set;
		}

		// cut down of current path, null meaning cut down distance is 0
		public PostPoint CutDownPostPoint
		{
			get; set;
		}

		public double FollowSafeDistance
		{
			get; set;
		}

		// the center and end point of frog leap
		public PostPoint FrogLeapMidPostPoint
		{
			get; set;
		}

		public PostPoint ProcessStartPoint
		{
			get; set;
		}

		public PostData()
		{
			LeadInPostPointList = new List<PostPoint>();
			MainPathPostPointList = new List<PostPoint>();
			OverCutPostPointList = new List<PostPoint>();
			LeadOutPostPointList = new List<PostPoint>();
			CutDownPostPoint = null;
			LiftUpPostPoint = null;
			FollowSafeDistance = 0;
			FrogLeapMidPostPoint = null;
		}
	}

	internal class PostPoint
	{
		public double X
		{
			get; set;
		}

		public double Y
		{
			get; set;
		}

		public double Z
		{
			get; set;
		}

		public double Master
		{
			get; set;
		}

		public double Slave
		{
			get; set;
		}

		public PostPoint Clone()
		{
			PostPoint newPoint = new PostPoint();
			newPoint.X = X;
			newPoint.Y = Y;
			newPoint.Z = Z;
			newPoint.Master = Master;
			newPoint.Slave = Slave;
			return newPoint;
		}
	}

	internal class PathNCPackage
	{
		public List<ICAMSegmentElement> LeadInSegment
		{
			get; set;
		}

		public List<ICAMSegmentElement> MainPathSegment
		{
			get; set;
		}

		public List<ICAMSegmentElement> OverCutSegment
		{
			get; set;
		}

		public List<ICAMSegmentElement> LeadOutSegment
		{
			get; set;
		}
	}
}
