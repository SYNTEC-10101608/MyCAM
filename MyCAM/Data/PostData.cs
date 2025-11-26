using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Data
{
	// re: 應該叫 PostSegmentType?
	public enum EPostPathType
	{
		Line,
		Arc
	}

	// re: 我建議命名如果需要前綴自的話可以叫 ContourPostData
	internal class PathPostData
	{
		// re: 以下命名的尾綴 PostPath 感覺有點多餘，因為這個類別本身就叫 PathPostData 了
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

	// re: 命名建議是 IPostSegment 比較符合風格，衍生類別也統一 (LinePostSegment, ArcPostSegment)
	internal interface ISegmentPostData
	{
		// re: naming
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

		// re: 看起來這個屬性其實沒在用了
		bool IsModify
		{
			get;
		}

		List<PostPoint> PostPointList
		{
			get; set;
		}

		// re: 上面已經開 get，這個就多餘了
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
		// re: 移到下面
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
			IsModify = isModify;
		}

		public PostPoint StartPoint
		{
			get; set;
		}

		public PostPoint EndPoint
		{
			get; set;
		}

		// re: 沒用到
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


	// re: 下面這坨看起來用不到?
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
