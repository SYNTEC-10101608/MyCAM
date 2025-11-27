using OCC.gp;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Data
{
	// fix: 這個應該叫 segment type
	public enum ESegmentType
	{
		Line,
		Arc,
	}

	#region CAD Segment

	// fix: 命名 "Element" 有點多餘了
	internal interface ICADSegment
	{
		ESegmentType SegmentType
		{
			get;
		}

		CADPoint StartPoint
		{
			get;
		}

		CADPoint EndPoint
		{
			get;
		}

		List<CADPoint> PointList
		{
			get;
		}

		// fix: 建議命名可以直接是 SegmentLength
		double SegmentLength
		{
			get;
		}

		double SubSegmentLength
		{
			get;
		}

		double PerChordLength
		{
			get;
		}

		ICADSegment Clone();

		void Transform( gp_Trsf transForm );

	}

	internal abstract class CADSegmentBase : ICADSegment
	{
		protected CADSegmentBase( List<CADPoint> pointList, double dTotalLength, double dPerArcLength, double dPerChordLength )
		{
			if( pointList == null || pointList.Count < 2 ) {
				throw new System.ArgumentException( "CADSegmentBasis constructing argument pointList null or count less than 2." );
			}
			m_StartPoint = pointList[ 0 ];
			m_EndPoint = pointList[ pointList.Count - 1 ];
			m_PointList = pointList;
			m_TotalLength = dTotalLength;
			m_SubSegmentLength = dPerArcLength;
			m_PerChordLength = dPerChordLength;
		}

		public abstract ESegmentType SegmentType
		{
			get;
		}

		// fix: 下面的屬性 private set 感覺沒什麼意義
		public virtual CADPoint StartPoint
		{
			get
			{
				return m_StartPoint;
			}
		}

		public virtual CADPoint EndPoint
		{
			get
			{
				return m_EndPoint;
			}
		}

		public virtual List<CADPoint> PointList
		{
			get
			{
				return new List<CADPoint>( m_PointList );
			}
		}

		public abstract ICADSegment Clone();

		public double SegmentLength
		{
			get
			{
				return m_TotalLength;
			}
		}

		public double SubSegmentLength
		{
			get
			{
				return m_SubSegmentLength;
			}
		}

		public double PerChordLength
		{
			get
			{
				return m_PerChordLength;
			}
		}

		public virtual void Transform( gp_Trsf transForm )
		{
			foreach( CADPoint point in m_PointList ) {
				point.Transform( transForm );
			}
		}

		// fix: member 在建構子會初始化，這裡就不需要再初始化一次了
		protected List<CADPoint> m_PointList;
		protected CADPoint m_StartPoint;
		protected CADPoint m_EndPoint;
		protected double m_TotalLength;
		protected double m_SubSegmentLength;
		protected double m_PerChordLength;
	}

	internal class LineCADSegment : CADSegmentBase
	{
		public LineCADSegment( List<CADPoint> linePointList, double dTotalLength, double dPerArcLegnth, double dPerChordLength )
			: base( linePointList, dTotalLength, dPerArcLegnth, dPerChordLength )
		{
		}

		public override ESegmentType SegmentType
		{
			get
			{
				return ESegmentType.Line;
			}
		}

		public override ICADSegment Clone()
		{
			List<CADPoint> clonedPointList = new List<CADPoint>();
			foreach( CADPoint point in m_PointList ) {
				// fix: 這邊的 as CADPoint 不需要
				clonedPointList.Add( point.Clone() );
			}
			return new LineCADSegment( clonedPointList, m_TotalLength, m_SubSegmentLength, m_PerChordLength );
		}
	}

	internal class ArcCADSegment : CADSegmentBase
	{
		public ArcCADSegment( List<CADPoint> arcPointList, double dTotalLength, double dPerArcLength, double dPerChordLength )
			: base( arcPointList, dTotalLength, dPerArcLength, dPerChordLength )
		{
			// fix: 這邊是否需要自己保護 count <=2 的情況？
			if( arcPointList.Count <= 2 ) {
				throw new System.ArgumentException( "ArcCADSegment requires at least 3 points to define a valid arc." );
			}
			m_MidIndex = arcPointList.Count / 2;
			MidPoint = arcPointList[ m_MidIndex ];
		}

		public override ESegmentType SegmentType
		{
			get
			{
				return ESegmentType.Arc;
			}
		}

		public CADPoint MidPoint
		{
			get
			{
				return m_MidPoint;
			}

			// fix: 這個 private set 感覺沒什麼意義===>因為base沒做這個動作,這裡我需要開出來讓建構子來設置
			set
			{
				m_MidPoint = value;
			}
		}

		public override ICADSegment Clone()
		{
			List<CADPoint> clonedPointList = new List<CADPoint>();
			foreach( CADPoint point in m_PointList ) {
				clonedPointList.Add( point.Clone() );
			}
			return new ArcCADSegment( clonedPointList, m_TotalLength, m_SubSegmentLength, m_PerChordLength );
		}

		CADPoint m_MidPoint;
		int m_MidIndex = 0;
	}

	#endregion

	#region CAM Segment

	internal interface ICAMSegmentElement
	{
		ESegmentType ContourType
		{
			get;
		}

		CAMPoint2 StartPoint
		{
			get;
		}

		CAMPoint2 EndPoint
		{
			get;
		}

		List<CAMPoint2> CAMPointList
		{
			get;
		}

		double TotalLength
		{
			get;
		}

		double PerChordLength
		{
			get;
		}

		double PerArcLength
		{
			get;
		}

		bool IsModify
		{
			get; set;
		}

		gp_Dir GetStartPointToolVec();

		gp_Dir GetEndPointToolVec();

		void SetStartPointToolVec( gp_Dir startPointToolVec );

		void SetEndPointToolVec( gp_Dir endPointToolVec );

		ICAMSegmentElement Clone();
	}

	internal abstract class CAMSegmentBase : ICAMSegmentElement
	{
		protected CAMSegmentBase( List<CAMPoint2> camPointList, double dTotalLength, double dArcLength, double dChordLength, bool isModifySegment = false )
		{
			if( camPointList.Count < 2 ) {
				throw new System.ArgumentException( " CAMSegmentBasis constructing points are null" );
			}
			StartPoint = camPointList.First().Clone();
			EndPoint = camPointList.Last().Clone();
			CAMPointList = camPointList;
			TotalLength = dTotalLength;
			PerArcLength = dArcLength;
			PerChordLength = dChordLength;
			IsModify = isModifySegment;
		}

		public abstract ESegmentType ContourType
		{
			get;
		}

		public virtual bool IsModify
		{
			get
			{
				return m_isModifySegment;
			}
			set
			{
				m_isModifySegment = value;
			}
		}


		public virtual CAMPoint2 StartPoint
		{
			get
			{
				return m_StartPoint;
			}
			private set
			{
				if( value != null ) {
					m_StartPoint = value;
				}
			}
		}

		public virtual CAMPoint2 EndPoint
		{
			get
			{
				return m_EndPoint;
			}
			private set
			{
				if( value != null ) {
					m_EndPoint = value;
				}
			}
		}

		public virtual List<CAMPoint2> CAMPointList
		{
			get => m_CAMPointList.Select( p => p.Clone() ).ToList();
			private set
			{
				if( value != null ) {
					m_CAMPointList = value;
				}
			}
		}

		public virtual double TotalLength
		{
			get;
			private set;
		}

		public virtual double PerChordLength
		{
			get;
			private set;
		}

		public virtual double PerArcLength
		{
			get;
			private set;
		}

		public virtual gp_Dir GetStartPointToolVec()
		{
			return new gp_Dir( StartPoint.ToolVec.XYZ() );
		}

		public virtual gp_Dir GetEndPointToolVec()
		{
			return new gp_Dir( EndPoint.ToolVec.XYZ() );
		}

		public virtual void SetStartPointToolVec( gp_Dir startPointToolVec )
		{
			if( startPointToolVec == null ) {
				return;
			}
			StartPoint.ToolVec = new gp_Dir( startPointToolVec.XYZ() );
			CalculatePointLisToolVec();
		}

		public virtual void SetEndPointToolVec( gp_Dir endPointToolVec )
		{
			if( endPointToolVec == null ) {
				return;
			}
			EndPoint.ToolVec = new gp_Dir( endPointToolVec.XYZ() );
			CalculatePointLisToolVec();
		}

		public abstract ICAMSegmentElement Clone();

		protected virtual void CalculatePointLisToolVec()
		{
			gp_Vec startPointToolVec = new gp_Vec( StartPoint.ToolVec );
			gp_Vec endPointToolVec = new gp_Vec( EndPoint.ToolVec );

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startPointToolVec, endPointToolVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			gp_Quaternion q = new gp_Quaternion();
			for( int i = 0; i < m_CAMPointList.Count - 1; i++ ) {
				slerp.Interpolate( PerArcLength * i / TotalLength, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				gp_Dir toolVecDir = new gp_Dir( startPointToolVec.Transformed( trsf ) );
				m_CAMPointList[ i ].ToolVec = toolVecDir;
			}
			m_CAMPointList[ m_CAMPointList.Count - 1 ].ToolVec = new gp_Dir( endPointToolVec );
		}

		protected List<CAMPoint2> m_CAMPointList = new List<CAMPoint2>();
		protected CAMPoint2 m_StartPoint;
		protected CAMPoint2 m_EndPoint;
		protected double m_TotalLength = 0.0;
		protected double m_PerArcLength = 0.0;
		protected double m_PerChordLength = 0.0;
		protected bool m_isModifySegment = false;
	}

	internal class LineCAMSegment : CAMSegmentBase
	{
		public LineCAMSegment( List<CAMPoint2> camPointList, double dTotalLength, double dArcLength, double dChordLength, bool isModifySegment = false )
			: base( camPointList, dTotalLength, dArcLength, dChordLength, isModifySegment )
		{
		}

		public override ESegmentType ContourType
		{
			get
			{
				return ESegmentType.Line;
			}
		}

		public override ICAMSegmentElement Clone()
		{
			List<CAMPoint2> clonedPointList = new List<CAMPoint2>();
			foreach( CAMPoint2 point in CAMPointList ) {
				clonedPointList.Add( point.Clone() );
			}
			return new LineCAMSegment( clonedPointList, m_TotalLength, m_PerArcLength, m_PerChordLength, m_isModifySegment );
		}
	}

	internal class ArcCAMSegment : CAMSegmentBase
	{
		public ArcCAMSegment( List<CAMPoint2> camPointList, double dTotalLength, double dArcLength, double dChordLength, bool isModifySegment = false )
			: base( camPointList, dTotalLength, dArcLength, dChordLength, isModifySegment )
		{
			m_MidIndex = camPointList.Count / 2;

			// share pointer
			MidPoint = camPointList[ m_MidIndex ];
		}

		public override ESegmentType ContourType
		{
			get
			{
				return ESegmentType.Arc;
			}
		}

		public override ICAMSegmentElement Clone()
		{
			List<CAMPoint2> clonedPointList = new List<CAMPoint2>();
			foreach( CAMPoint2 point in CAMPointList ) {
				clonedPointList.Add( point.Clone() );
			}
			return new ArcCAMSegment( clonedPointList, m_TotalLength, m_PerArcLength, m_PerChordLength, m_isModifySegment );
		}

		public CAMPoint2 MidPoint
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

		CAMPoint2 m_MidPoint;
		int m_MidIndex = 0;
	}

	internal interface IEnhancedCAMSegmentElement : ICAMSegmentElement
	{
		bool IsStartAtHead
		{
			get; set;
		}     // segment的頭是起點
		bool IsStartAtTail
		{
			get; set;
		}     // segment的尾是起點
		bool IsControlAtHead
		{
			get; set;
		}   // segment的頭是控制點
		bool IsControlAtTail
		{
			get; set;
		}   // segment的尾是控制點
	}

	internal abstract class EnhancedCAMSegmentBase : CAMSegmentBase, IEnhancedCAMSegmentElement
	{
		protected EnhancedCAMSegmentBase( List<CAMPoint2> camPointList, double dTotalLength, double dArcLength, double dChordLength, bool isModifySegment = false )
			: base( camPointList, dTotalLength, dArcLength, dChordLength, isModifySegment )
		{
			IsStartAtHead = false;
			IsStartAtTail = false;
			IsControlAtHead = false;
			IsControlAtTail = false;
		}

		public bool IsStartAtHead
		{
			get; set;
		}
		public bool IsStartAtTail
		{
			get; set;
		}
		public bool IsControlAtHead
		{
			get; set;
		}
		public bool IsControlAtTail
		{
			get; set;
		}
	}

	internal class EnhancedLineCAMSegment : EnhancedCAMSegmentBase
	{
		public EnhancedLineCAMSegment( List<CAMPoint2> camPointList, double dTotalLength, double dArcLength, double dChordLength, bool isModifySegment = false )
			: base( camPointList, dTotalLength, dArcLength, dChordLength, isModifySegment )
		{
		}

		public override ESegmentType ContourType
		{
			get
			{
				return ESegmentType.Line;
			}
		}

		public override ICAMSegmentElement Clone()
		{
			List<CAMPoint2> clonedPointList = new List<CAMPoint2>();
			foreach( CAMPoint2 point in CAMPointList ) {
				clonedPointList.Add( point.Clone() );
			}
			var cloned = new EnhancedLineCAMSegment( clonedPointList, m_TotalLength, m_PerArcLength, m_PerChordLength, m_isModifySegment );
			cloned.IsStartAtHead = IsStartAtHead;
			cloned.IsStartAtTail = IsStartAtTail;
			cloned.IsControlAtHead = IsControlAtHead;
			cloned.IsControlAtTail = IsControlAtTail;
			return cloned;
		}
	}

	internal class EnhancedArcCAMSegment : EnhancedCAMSegmentBase
	{
		public EnhancedArcCAMSegment( List<CAMPoint2> camPointList, double dTotalLength, double dArcLength, double dChordLength, bool isModifySegment = false )
			: base( camPointList, dTotalLength, dArcLength, dChordLength, isModifySegment )
		{
			m_MidIndex = camPointList.Count / 2;
			MidPoint = camPointList[ m_MidIndex ];
		}

		public override ESegmentType ContourType
		{
			get
			{
				return ESegmentType.Arc;
			}
		}

		public override ICAMSegmentElement Clone()
		{
			List<CAMPoint2> clonedPointList = new List<CAMPoint2>();
			foreach( CAMPoint2 point in CAMPointList ) {
				clonedPointList.Add( point.Clone() );
			}
			var cloned = new EnhancedArcCAMSegment( clonedPointList, m_TotalLength, m_PerArcLength, m_PerChordLength, m_isModifySegment );
			cloned.IsStartAtHead = IsStartAtHead;
			cloned.IsStartAtTail = IsStartAtTail;
			cloned.IsControlAtHead = IsControlAtHead;
			cloned.IsControlAtTail = IsControlAtTail;
			return cloned;
		}

		public CAMPoint2 MidPoint
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

		CAMPoint2 m_MidPoint;
		int m_MidIndex = 0;
	}
	#endregion
}
