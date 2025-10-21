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

	internal class PathSegmentPostData
	{
		public List<PostPath> LeadInPostPath
		{
			get; set;
		}

		public List<PostPath> MainPathPostPath
		{
			get; set;
		}

		public List<PostPath> OverCutPostPath
		{
			get; set;
		}

		public List<PostPath> LeadOutPostPath
		{
			get; set;
		}

		public PostPath CutDownPostPath
		{
			get; set;
		}

		public PostPath LiftUpPostPath
		{
			get; set;
		}

		public PostPath FollowSafePostPath
		{
			get; set;
		}

		public PathSegmentPostData()
		{
			LeadInPostPath = new List<PostPath>();
			MainPathPostPath = new List<PostPath>();
			OverCutPostPath = new List<PostPath>();
			LeadOutPostPath = new List<PostPath>();
			CutDownPostPath = null;
			LiftUpPostPath = null;
			FollowSafePostPath = null;
		}
	}


	public enum EPostPathType
	{
		Line,
		Arc
	}

	internal abstract class PostPath
	{
		public abstract EPostPathType PostPathType
		{
			get;
		}

		public PostPoint StartPoint
		{
			get;
			protected set;
		}

		public PostPoint EndPoint
		{
			get;
			protected set;
		}

		protected PostPath( PostPoint startPoint, PostPoint endPoint )
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}
	}

	internal class LinePostPath : PostPath
	{
		public LinePostPath( PostPoint startPoint, PostPoint endPoint )
			: base( startPoint, endPoint )
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		public override EPostPathType PostPathType
		{
			get
			{
				return EPostPathType.Line;
			}
		}
	}

	internal class ArcPostPath : PostPath
	{
		public ArcPostPath( PostPoint startPoint, PostPoint midPoint, PostPoint endPoint )
			: base( startPoint, endPoint )
		{
			StartPoint = startPoint;
			MidPoint = midPoint;
			EndPoint = endPoint;
		}

		public override EPostPathType PostPathType
		{
			get
			{
				return EPostPathType.Line;
			}
		}

		public PostPoint MidPoint
		{
			get;
			private set;
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
}
