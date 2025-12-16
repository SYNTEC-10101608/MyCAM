using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public class CircleCache : StdPatternCacheBase
	{
		public CircleCache( gp_Ax3 refCoord, IStdPatternGeomData geomData, CraftData craftData )
			: base( refCoord, craftData )
		{
			if( geomData == null || !( geomData is CircleGeomData circleGeomData ) ) {
				throw new ArgumentNullException( "CircleCache constructing argument error - invalid geomData" );
			}
			m_CircleGeomData = circleGeomData;
			m_StartCADPointList = StdPatternStartPointFactory.GetStartPointList( m_RefCoord, circleGeomData );
			BuildCAMPointList();
		}

		protected override void BuildCAMPointList()
		{
			ClearCraftDataDirty();
			m_RefPoint = new CAMPoint( new CADPoint( m_RefCoord.Location(), m_RefCoord.Direction(), m_RefCoord.XDirection(), m_RefCoord.YDirection() ), m_RefCoord.Direction() );

			// build initial CAM point list
			m_StartCAMPointList = new List<CAMPoint>();
			for( int i = 0; i < m_StartCADPointList.Count; i++ ) {
				CADPoint cadPoint = m_StartCADPointList[ i ];
				CAMPoint camPoint = new CAMPoint( cadPoint );
				m_StartCAMPointList.Add( camPoint );
			}
			SetStartPoint();

			// set tool vector
			List<ISetToolVecPoint> toolVecPointList = m_StartCAMPointList.Cast<ISetToolVecPoint>().ToList();
			ToolVecHelper.SetToolVec( ref toolVecPointList, m_CraftData.ToolVecModifyMap, true, m_CraftData.IsToolVecReverse );


		}

		void SetStartPoint()
		{
			// rearrange cam points to start from the start index
			if( m_CraftData.StartPointIndex != 0 ) {
				List<CAMPoint> newStartPointList = new List<CAMPoint>();
				for( int i = 0; i < m_StartCAMPointList.Count; i++ ) {
					newStartPointList.Add( m_StartCAMPointList[ ( i + m_CraftData.StartPointIndex ) % m_StartCAMPointList.Count ] );
				}
				m_StartCAMPointList = newStartPointList;
			}
			m_StartCAMPointList.Add( m_StartCAMPointList[ 0 ].Clone() ); // close the circle
		}

		CircleGeomData m_CircleGeomData;
	}
}
