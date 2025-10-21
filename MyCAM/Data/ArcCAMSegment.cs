namespace MyCAM.Data
{
	internal class ArcCAMSegment : CAMSegmentBase
	{
		public ArcCAMSegment( CAMPoint startPoint, CAMPoint endPoint, CAMPoint midPoint )
			: base( startPoint, endPoint )
		{
			MidPoint = midPoint;
		}

		public override EContourType ContourType
		{
			get
			{
				return EContourType.Arc;
			}
		}

		public CAMPoint MidPoint
		{
			get
			{
				return m_MidPoint;
			}
			private set
			{
				if( value != null ) {
					m_MidPoint = value;
				}
			}
		}

		CAMPoint m_MidPoint;
	}
}
