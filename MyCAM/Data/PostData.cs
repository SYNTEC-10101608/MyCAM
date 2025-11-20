using System;
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

	internal class PathPostData
	{
		public List<ISegmentPostData> LeadInPostPath
		{
			get; set;
		}

		public List<ISegmentPostData> MainPathPostPath
		{
			get; set;
		}

		public List<ISegmentPostData> OverCutPostPath
		{
			get; set;
		}

		public List<ISegmentPostData> LeadOutPostPath
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

		public PathPostData()
		{
			LeadInPostPath = new List<ISegmentPostData>();
			MainPathPostPath = new List<ISegmentPostData>();
			OverCutPostPath = new List<ISegmentPostData>();
			LeadOutPostPath = new List<ISegmentPostData>();
			CutDownPostPoint = null;
			LiftUpPostPoint = null;
			FollowSafeDistance = 0;
			FrogLeapMidPostPoint = null;
		}
	}

	internal interface ISegmentPostData
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

		bool IsModify
		{
			get;
		}

		List<PostPoint> GetPostPointList();
	}

	internal abstract class NormalPost : ISegmentPostData
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

		public bool IsModify
		{
			get;
			private set;
		} = false;

		public virtual List<PostPoint> GetPostPointList()
		{
			List<PostPoint> pointList = new List<PostPoint>();
			pointList.Add( StartPoint );
			pointList.Add( EndPoint );
			return pointList;
		}

		protected NormalPost( PostPoint startPoint, PostPoint endPoint, bool isModify )
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
			IsModify = isModify;
		}
	}

	internal class LinePost : NormalPost
	{
		public LinePost( PostPoint startPoint, PostPoint endPoint, bool isModify )
			: base( startPoint, endPoint, isModify )
		{
		}

		public override EPostPathType PostPathType
		{
			get
			{
				return EPostPathType.Line;
			}
		}
	}

	internal class ArcPost : NormalPost
	{
		public ArcPost( PostPoint startPoint, PostPoint midPoint, PostPoint endPoint, bool isModify )
			: base( startPoint, endPoint, isModify )
		{
			MidPoint = midPoint;
		}

		public override EPostPathType PostPathType
		{
			get
			{
				return EPostPathType.Arc;
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

	internal abstract class SplitPost : ISegmentPostData
	{
		public abstract EPostPathType PostPathType
		{
			get;
		}

		public bool IsModify
		{
			get;
		} = false;

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
			return PostPointList;
		}

		protected SplitPost( List<PostPoint> postPointList, bool isModify )
		{
			if( postPointList == null || postPointList.Count < 2 ) {
				throw new System.ArgumentException( "DispersionPostPath requires at least 2 points" );
			}
			PostPointList = postPointList;
			StartPoint = postPointList.First();
			EndPoint = postPointList.Last();
			IsModify = isModify;
		}
	}

	internal class SplitLinePost : SplitPost
	{
		public SplitLinePost( List<PostPoint> postPointList, bool isModify )
			: base( postPointList, isModify )
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

	internal class SplitArcPostPath : SplitPost
	{
		public PostPoint MidPoint
		{
			get; private set;
		}

		public SplitArcPostPath( List<PostPoint> postPointList, bool isModify )
			: base( postPointList, isModify )
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

		public bool IsInSigularRabge
		{
			get; set;
		} = false;

		public PostPoint Clone()
		{
			PostPoint newPoint = new PostPoint();
			newPoint.X = X;
			newPoint.Y = Y;
			newPoint.Z = Z;
			newPoint.Master = Master;
			newPoint.Slave = Slave;
			newPoint.IsInSigularRabge = IsInSigularRabge;
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

	internal class PathIKSolveInfo
	{
		public List<segmentIKInfo> LeadInIKSoveInfo
		{
			get; set;
		}

		public List<segmentIKInfo> MainPathIKSoveInfo
		{
			get; set;
		}

		public List<segmentIKInfo> OverCutIKSoveInfo
		{
			get; set;
		}

		public List<segmentIKInfo> LeadOutIKSoveInfo
		{
			get; set;
		}
	}

	internal class segmentIKInfo
	{
		public List<CAMPoint2> CAMPointList
		{
			get; set;
		}

		public List<Tuple<double, double>> RotateAngleList
		{
			get; set;
		}
		public List<bool> SigularTagList
		{
			get; set;
		}

		public segmentIKInfo()
		{
			RotateAngleList = new List<Tuple<double, double>>();
			SigularTagList = new List<bool>();
			CAMPointList = new List<CAMPoint2>();
		}
	}
}
