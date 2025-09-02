using System.Collections.Generic;

namespace MyCAM.Data
{
	internal class PostData
	{
		public List<PostPoint> LeadInPostPointList { get; set; }

		public List<PostPoint> MainPathPostPointList { get; set; }

		public List<PostPoint> OverCutPostPointList { get; set; }

		public List<PostPoint> LeadOutPostPointList { get; set; }

		public PostData()
		{
			LeadInPostPointList = new List<PostPoint>();
			MainPathPostPointList = new List<PostPoint>();
			OverCutPostPointList = new List<PostPoint>();
			LeadOutPostPointList = new List<PostPoint>();
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
	}
}
