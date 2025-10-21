using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCAM.Data
{
	internal class LineCAMSegment:CAMSegmentBase
	{
		public LineCAMSegment( CAMPoint startPoint, CAMPoint endPoint )
			: base( startPoint, endPoint )
		{
		}

		public override EContourType ContourType
		{
			get
			{
				return EContourType.Line;
			}
		}
	}
}
