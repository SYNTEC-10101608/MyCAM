using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public class ContourGeomData
	{
		public ContourGeomData( string szUID, List<PathEdge5D> pathDataList, bool isClosed )
		{
			if( pathDataList == null ) {
				throw new ArgumentNullException( "ContourGeomData constructing argument null" );
			}
			if( pathDataList.Count == 0 ) {
				throw new ArgumentException( "ContourGeomData constructing argument empty pathDataList" );
			}
			UID = szUID;
			DisctereContourHelper.BuildContourGeomData( pathDataList, isClosed,
				out m_CADPointList, out m_ConnectPointMap );
		}

		public ContourGeomData( string szUID, List<CADPoint> cadPointList, Dictionary<CADPoint, CADPoint> connectPointMap )
		{
			if( cadPointList == null || connectPointMap == null ) {
				throw new ArgumentNullException( "ContourGeomData constructing argument null" );
			}
			if( cadPointList.Count == 0 ) {
				throw new ArgumentException( "ContourGeomData constructing argument empty cadPointList" );
			}
			UID = szUID;
			m_CADPointList = cadPointList;
			m_ConnectPointMap = connectPointMap;
		}

		public string UID
		{
			get; private set;
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

		public void DoTransform( gp_Trsf transform )
		{
			foreach( CADPoint cadPoint in m_CADPointList ) {
				cadPoint.Transform( transform );
			}
			foreach( var oneConnectPoint in m_ConnectPointMap ) {
				oneConnectPoint.Value.Transform( transform );
			}
		}

		List<CADPoint> m_CADPointList;
		Dictionary<CADPoint, CADPoint> m_ConnectPointMap;
	}
}
