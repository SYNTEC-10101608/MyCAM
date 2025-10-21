using System.Collections.Generic;
using OCC.gp;

namespace MyCAM.Data
{
	internal class ArcCADSegment : CADSegmentBase
	{
		public ArcCADSegment( List<CADPoint> arcPointList, double dTotalLength, double dPointSpace,gp_Pnt circleCenter, gp_Dir centerDir )
			: base( arcPointList, dTotalLength , dPointSpace)
		{
			CenterOfCircle = circleCenter;
			CenterDir = centerDir;
			MidPoint = arcPointList[ arcPointList.Count / 2 ];
		}

		public override EContourType ContourType
		{
			get
			{
				return EContourType.Arc;
			}
		}

		public CADPoint MidPoint
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

		public gp_Pnt CenterOfCircle
		{
			get
			{
				return m_CenterOfCircle;
			}
			private set
			{
				if( value != null ) {
					m_CenterOfCircle = value;
				}
			}
		}

		public gp_Dir CenterDir
		{
			get
			{
				return m_CenterDir;
			}
			set
			{
				if( value != null ) {
					m_CenterDir = value;
				}
			}
		}

		CADPoint m_MidPoint;
		gp_Pnt m_CenterOfCircle;
		gp_Dir m_CenterDir;
	}
}
