using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public class ContourGeomData : IContourGeomData
	{
		public ContourGeomData( List<PathEdge5D> pathDataList, bool isClosed )
		{
			if( pathDataList == null ) {
				throw new ArgumentNullException( "ContourGeomData constructing argument null" );
			}
			if( pathDataList.Count == 0 ) {
				throw new ArgumentException( "ContourGeomData constructing argument empty pathDataList" );
			}
			m_IsClosed = isClosed;
			DisctereContourHelper.BuildContourGeomData( pathDataList, isClosed,
				out m_CADPointList, out m_ConnectPointMap );
			DisctereContourHelper.GetContourCenterPointAndNormalDir( m_CADPointList, out m_OriCenterPnt, out m_AverageNormalDir );
		}

		public ContourGeomData( List<CADPoint> cadPointList, Dictionary<CADPoint, CADPoint> connectPointMap, bool isClosed )
		{
			if( cadPointList == null || connectPointMap == null ) {
				throw new ArgumentNullException( "ContourGeomData constructing argument null" );
			}
			if( cadPointList.Count == 0 ) {
				throw new ArgumentException( "ContourGeomData constructing argument empty cadPointList" );
			}
			m_CADPointList = cadPointList;
			m_ConnectPointMap = connectPointMap;
			m_IsClosed = isClosed;
			DisctereContourHelper.GetContourCenterPointAndNormalDir( m_CADPointList, out m_OriCenterPnt, out m_AverageNormalDir );
		}

		public PathType PathType
		{
			get
			{
				return PathType.Contour;
			}
		}

		public List<CADPoint> CADPointList
		{
			get
			{
				return m_CADPointList;
			}
		}

		public Dictionary<CADPoint, CADPoint> ConnectPointMap
		{
			get
			{
				return m_ConnectPointMap;
			}
		}

		public bool IsClosed
		{
			get
			{
				return m_IsClosed;
			}
		}

		public gp_Pnt CenterPnt
		{
			get
			{
				return new gp_Pnt( m_OriCenterPnt.XYZ() );
			}
		}

		public gp_Dir AverageNormalDir
		{
			get
			{
				return new gp_Dir( m_AverageNormalDir.XYZ() );
			}
		}

		public void DoTransform( gp_Trsf transform )
		{
			foreach( CADPoint cadPoint in m_CADPointList ) {
				cadPoint.Transform( transform );
			}
			foreach( var oneConnectPoint in m_ConnectPointMap ) {
				oneConnectPoint.Value.Transform( transform );
			}
			m_OriCenterPnt.Transform( transform );
			m_AverageNormalDir.Transform( transform );
		}

		public IGeomData Clone()
		{
			return new ContourGeomData( m_CADPointList, m_ConnectPointMap, m_IsClosed );
		}

		List<CADPoint> m_CADPointList;
		Dictionary<CADPoint, CADPoint> m_ConnectPointMap;
		gp_Pnt m_OriCenterPnt;
		gp_Dir m_AverageNormalDir;
		bool m_IsClosed;
	}
}
