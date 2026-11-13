using MyCAM.Post;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public class PostData : ITraversePostData
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

		public PostPoint LiftUpPostSafePlanePoint
		{
			get; set;
		}

		public PostPoint CutDownPostSafePlanePoint
		{
			get; set;
		}

		public List<PostPoint> SafeBoundaryPostPointList
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
			LiftUpPostSafePlanePoint = null;
			CutDownPostSafePlanePoint = null;
		}
	}

	internal class StdPatternPostData : ITraversePostData
	{
		public PostPoint RefPoint
		{
			get; set;
		}

		public PostPoint ProcessStartPoint
		{
			get; set;
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

		public PostPoint LiftUpPostSafePlanePoint
		{
			get; set;
		}

		public PostPoint CutDownPostSafePlanePoint
		{
			get; set;
		}

		public List<PostPoint> SafeBoundaryPostPointList
		{
			get; set;
		}
	}

	internal class FlyCutPostData : ITraversePostData
	{
		public List<PostPoint> LeadInPostPointList
		{
			get; set;
		}

		public List<PostPoint> LeadOutPostPointList
		{
			get; set;
		}

		public List<List<PostPoint>> StepPostPointList
		{
			get; set;
		}

		public PostPoint ProcessStartPoint
		{
			get; set;
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

		public PostPoint LiftUpPostSafePlanePoint
		{
			get; set;
		}

		public PostPoint CutDownPostSafePlanePoint
		{
			get; set;
		}

		public List<PostPoint> SafeBoundaryPostPointList
		{
			get; set;
		}

		public FlyCutPostData()
		{
			LeadInPostPointList = new List<PostPoint>();
			LeadOutPostPointList = new List<PostPoint>();
			StepPostPointList = new List<List<PostPoint>>();
			CutDownPostPoint = null;
			LiftUpPostPoint = null;
			FollowSafeDistance = 0;
			FrogLeapMidPostPoint = null;
			LiftUpPostSafePlanePoint = null;
			CutDownPostSafePlanePoint = null;
		}
	}

	// shared per-point data across products ( machine coordinate point + microjoint flags )
	public abstract class PostPointBase
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

		public bool IsMicroJointStart
		{
			get; set;
		} = false;

		public bool IsMicroJointEnd
		{
			get; set;
		} = false;
	}

	// five-axis post point: shared point + master / slave rotary values
	public class PostPoint : PostPointBase
	{
		public double Master
		{
			get; set;
		}

		public double Slave
		{
			get; set;
		}

		public double TiltAngle_deg
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
			newPoint.TiltAngle_deg = TiltAngle_deg;
			newPoint.IsMicroJointStart = IsMicroJointStart;
			newPoint.IsMicroJointEnd = IsMicroJointEnd;
			return newPoint;
		}
	}

	// articulated robot post point: shared point + tool orientation ( A/B/C ) + joint angles ( C1~C6 )
	public class ArticRobotPostPoint : PostPointBase
	{
		public double A
		{
			get; set;
		}

		public double B
		{
			get; set;
		}

		public double C
		{
			get; set;
		}

		public double C1
		{
			get; set;
		}

		public double C2
		{
			get; set;
		}

		public double C3
		{
			get; set;
		}

		public double C4
		{
			get; set;
		}

		public double C5
		{
			get; set;
		}

		public double C6
		{
			get; set;
		}

		public ArticRobotPostPoint Clone()
		{
			ArticRobotPostPoint newPoint = new ArticRobotPostPoint();
			newPoint.X = X;
			newPoint.Y = Y;
			newPoint.Z = Z;
			newPoint.A = A;
			newPoint.B = B;
			newPoint.C = C;
			newPoint.C1 = C1;
			newPoint.C2 = C2;
			newPoint.C3 = C3;
			newPoint.C4 = C4;
			newPoint.C5 = C5;
			newPoint.C6 = C6;
			newPoint.IsMicroJointStart = IsMicroJointStart;
			newPoint.IsMicroJointEnd = IsMicroJointEnd;
			return newPoint;
		}
	}


}
