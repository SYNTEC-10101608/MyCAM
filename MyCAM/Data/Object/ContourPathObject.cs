using MyCAM.App;
using MyCAM.PathCache;
using OCC.BRep;
using OCC.gp;
using OCC.TopExp;
using OCC.TopoDS;
using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public class ContourPathObject : PathObject
	{
		public ContourPathObject( string szUID, TopoDS_Shape shape, List<PathEdge5D> pathDataList )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || pathDataList == null ) {
				throw new ArgumentNullException( "ContourPathObject constructing argument null" );
			}
			if( pathDataList.Count == 0 ) {
				throw new ArgumentException( "ContourPathObject constructing argument empty pathDataList" );
			}

			bool isClosed = DetermineIfClosed( shape );
			m_ContourGeomData = new ContourGeomData( pathDataList, isClosed );
			m_CraftData = new CraftData();
			m_ContourCache = new ContourCache( m_ContourGeomData, m_CraftData );
		}

		// this is for the file read constructor
		public ContourPathObject( string szUID, TopoDS_Shape shape, ContourGeomData geomData, CraftData craftData )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || geomData == null || craftData == null ) {
				throw new ArgumentNullException( "ContourPathObject constructing argument null" );
			}
			m_ContourGeomData = geomData;
			m_CraftData = craftData;
			m_ContourCache = new ContourCache( m_ContourGeomData, m_CraftData );
			BuildUnClosePathStartEndPntParam();
		}

		public ContourGeomData GeomData
		{
			get
			{
				return m_ContourGeomData;
			}
		}

		public ContourCache ContourCache
		{
			get
			{
				return m_ContourCache;
			}
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Contour;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			// fix:
			// Step1:tranform shape first
			base.DoTransform( transform );

			// Step2:then transform the geom data
			m_ContourGeomData.DoTransform( transform );

			// Step3:recalculate cache, currently dont really need gp_trsf
			m_ContourCache.DoTransform( transform );
		}

		// old file may have no start end point param for unclosed path
		void BuildUnClosePathStartEndPntParam()
		{
			if( m_ContourGeomData.IsClosed ) {
				return;
			}
			int lastIndex = m_ContourCache.MainPathPointList.Count - 1;

			// cad end point is in the map
			m_CraftData.ToolVecModifyMap.TryGetValue( lastIndex, out ToolVecModifyData toolVecModifyData );
			if (toolVecModifyData == null ) {
				return;
			}
			ToolVecModifyData modifyData = toolVecModifyData.Clone();
			if( m_CraftData.IsPathReverse ) {
				m_CraftData.StartPntToolVecData.StartPnt = modifyData;
				return;
			}
			m_CraftData.StartPntToolVecData.EndPnt = modifyData;
		}

		bool DetermineIfClosed( TopoDS_Shape shapeData )
		{
			if( shapeData == null || shapeData.IsNull() )
				return false;

			try {
				TopoDS_Vertex startVertex = new TopoDS_Vertex();
				TopoDS_Vertex endVertex = new TopoDS_Vertex();
				TopExp.Vertices( TopoDS.ToWire( shapeData ), ref startVertex, ref endVertex );

				gp_Pnt startPoint = BRep_Tool.Pnt( TopoDS.ToVertex( startVertex ) );
				gp_Pnt endPoint = BRep_Tool.Pnt( TopoDS.ToVertex( endVertex ) );

				return startPoint.IsEqual( endPoint, 1e-3 );
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"Error occurred while determining a closed path.: {ex.Message}", MyApp.NoticeType.Warning );
				return false;
			}
		}

		ContourGeomData m_ContourGeomData;
		ContourCache m_ContourCache;
	}
}
