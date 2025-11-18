using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Data
{
	public enum EPostPathType
	{
		Line,
		Arc,
		DispersionLine,
		DispersionArc
	}

	internal class PathSegmentPostData
	{
		public List<IPostPath> LeadInPostPath
		{
			get; set;
		}

		public List<IPostPath> MainPathPostPath
		{
			get; set;
		}

		public List<IPostPath> OverCutPostPath
		{
			get; set;
		}

		public List<IPostPath> LeadOutPostPath
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

		public PostPoint ProcessStartPoint
		{
			get; set;
		}

		public PathSegmentPostData()
		{
			LeadInPostPath = new List<IPostPath>();
			MainPathPostPath = new List<IPostPath>();
			OverCutPostPath = new List<IPostPath>();
			LeadOutPostPath = new List<IPostPath>();
			CutDownPostPoint = null;
			LiftUpPostPoint = null;
			FollowSafeDistance = 0;
			FrogLeapMidPostPoint = null;
		}
	}

	internal interface IPostPath
	{
		EPostPathType PostPathType
		{
			get;
		}

		PostPoint StartPoint
		{
			get;
		}

		PostPoint EndPoint
		{
			get;
		}

		List<PostPoint> GetPostPointList();
	}

	internal abstract class UndispersionPostPath : IPostPath
	{
		public abstract EPostPathType PostPathType
		{
			get;
		}

		public PostPoint StartPoint
		{
			get; protected set;
		}

		public PostPoint EndPoint
		{
			get; protected set;
		}

		public virtual List<PostPoint> GetPostPointList()
		{
			List<PostPoint> pointList = new List<PostPoint>();
			pointList.Add( StartPoint );
			pointList.Add( EndPoint );
			return pointList;
		}

		protected UndispersionPostPath( PostPoint startPoint, PostPoint endPoint )
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}
	}


	internal class LinePostPath : UndispersionPostPath
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

	internal class ArcPostPath : UndispersionPostPath
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

		public override List<PostPoint> GetPostPointList()
		{
			List<PostPoint> pointList = new List<PostPoint>();
			pointList.Add( StartPoint );
			pointList.Add( MidPoint );
			pointList.Add( EndPoint );
			return pointList;
		}
	}

	internal abstract class DispersionPostPath : IPostPath
	{
		public abstract EPostPathType PostPathType
		{
			get;
		}

		public PostPoint StartPoint
		{
			get; protected set;
		}

		public PostPoint EndPoint
		{
			get; protected set;
		}

		public List<PostPoint> PostPointList
		{
			get; protected set;
		}

		public virtual List<PostPoint> GetPostPointList()
		{
			return new List<PostPoint>( PostPointList );
		}

		protected DispersionPostPath( List<PostPoint> postPointList )
		{
			if( postPointList == null || postPointList.Count < 2 ) {
				throw new System.ArgumentException( "DispersionPostPath requires at least 2 points" );
			}
			PostPointList = postPointList;
			StartPoint = postPointList.First();
			EndPoint = postPointList.Last();
		}
	}
	internal class DispersionLinePostPath : DispersionPostPath
	{
		public DispersionLinePostPath( List<PostPoint> postPointList )
			: base( postPointList )
		{
		}

		public override EPostPathType PostPathType
		{
			get
			{
				return EPostPathType.DispersionLine;
			}
		}
	}

	internal class DispersionArcPostPath : DispersionPostPath
	{
		public PostPoint MidPoint
		{
			get; private set;
		}

		public DispersionArcPostPath( List<PostPoint> postPointList )
			: base( postPointList )
		{
			int midIndex = postPointList.Count / 2;
			MidPoint = postPointList[ midIndex ];
		}

		public override EPostPathType PostPathType
		{
			get
			{
				return EPostPathType.DispersionArc;
			}
		}
	}


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
