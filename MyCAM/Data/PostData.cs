using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Data
{
	public enum EPostPathType
	{
		Line,
		Arc
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

		List<PostPoint> PostPointList
		{
			get; set;
		}

		List<PostPoint> GetPostPointList();
	}

	internal class LinePost : ISegmentPostData
	{
		public LinePost( List<PostPoint> postPointList, bool isModify )
		{
			PostPointList = postPointList;
			IsModify = isModify;
			StartPoint = postPointList.First();
			EndPoint = postPointList.Last();
		}

		public PostPoint StartPoint
		{
			get; set;
		}

		public PostPoint EndPoint
		{
			get; set;
		}

		public List<PostPoint> PostPointList
		{
			get; set;
		}

		public bool IsModify
		{
			get; protected set;
		}

		public EPostPathType PostPathType
		{
			get
			{
				return EPostPathType.Line;
			}
		}

		public List<PostPoint> GetPostPointList()
		{
			return PostPointList;
		}
	}

	internal class ArcPost : ISegmentPostData
	{
		public PostPoint MidPoint
		{
			get; private set;
		}

		public ArcPost( List<PostPoint> postPointList, bool isModify )
		{
			PostPointList = postPointList;
			StartPoint = PostPointList.First();
			EndPoint = PostPointList.Last();
			int midIndex = postPointList.Count / 2;
			MidPoint = postPointList[ midIndex ];
		}

		public PostPoint StartPoint
		{
			get; set;
		}

		public PostPoint EndPoint
		{
			get; set;
		}

		public PostPoint MidPnt
		{
			get
			{
				return MidPoint;
			}
		}

		public List<PostPoint> PostPointList
		{
			get; set;
		}

		public bool IsModify
		{
			get; protected set;
		}

		public EPostPathType PostPathType
		{
			get
			{
				return EPostPathType.Arc;
			}
		}

		public List<PostPoint> GetPostPointList()
		{
			return PostPointList;
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
		public List<ICAMSegment> LeadInSegment
		{
			get; set;
		}

		public List<ICAMSegment> MainPathSegment
		{
			get; set;
		}

		public List<int> CtrlSegIdx
		{
			get; set;
		}

		public List<ICAMSegment> OverCutSegment
		{
			get; set;
		}

		public List<ICAMSegment> LeadOutSegment
		{
			get; set;
		}

		public CAMPoint ProcessStartPnt
		{
			get; set;
		}

		public CAMPoint ProcessEndPnt
		{
			get; set;
		}

		public double LiftUpDistance
		{
			get; set;
		}

		public double FrogLeapDistance
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

		public double EntryDistance
		{
			get;set;
		}

		public double Entry_FollowSafeDistance
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
