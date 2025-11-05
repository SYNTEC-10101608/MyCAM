namespace MyCAM.Data
{
	internal class LineCAMSegment : CAMSegmentBase
	{
		public LineCAMSegment( CAMPoint startPoint, CAMPoint endPoint, bool isModifyElement )
			: base( startPoint, endPoint, isModifyElement )
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
