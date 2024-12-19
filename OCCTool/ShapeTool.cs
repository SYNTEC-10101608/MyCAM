using OCC.BRep;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.ShapeAnalysis;
using OCC.TopoDS;
using System.Collections.Generic;

namespace OCCTool
{
	public class ShapeTool
	{
		public static TopoDS_Compound MakeCompound( List<TopoDS_Shape> source )
		{
			TopoDS_Compound compound = new TopoDS_Compound();
			TopoDS_Shape compoundShape = (TopoDS_Shape)compound;
			TopoDS_Builder builder = new TopoDS_Builder();
			builder.MakeCompound( ref compound );
			foreach( TopoDS_Shape shape in source ) {
				builder.Add( ref compoundShape, shape );
			}
			return compound;
		}

		public static TopoDS_Shape SewShape( List<TopoDS_Shape> shapeList )
		{
			// split the faces into shells
			double dSewingTolerance = 0.001;
			BRepBuilderAPI_Sewing sewing = new BRepBuilderAPI_Sewing( dSewingTolerance );
			foreach( TopoDS_Shape shape in shapeList ) {
				sewing.Add( shape );
			}
			sewing.Perform();
			return sewing.SewedShape();
		}

		//TODO: refine this method
		public static List<List<TopoDS_Edge>> SortEdgeList( List<TopoDS_Edge> originalEdgeList )
		{
			int nNumOfSegments = originalEdgeList.Count;
			List<List<TopoDS_Edge>> sortedEdgeList = new List<List<TopoDS_Edge>>();
			sortedEdgeList.Add( originalEdgeList );

			bool bCanFitInWire = false;
			int nLastIndex = 0;

			// add edges in same wire to other list
			for( int i = 0; i < nNumOfSegments; i++ ) {

				if( bCanFitInWire == false ) {
					sortedEdgeList.Add( new List<TopoDS_Edge>() );
					sortedEdgeList[ nLastIndex + 1 ].Add( sortedEdgeList[ 0 ][ 0 ] );
					sortedEdgeList[ 0 ].RemoveAt( 0 );
					nLastIndex = sortedEdgeList.Count - 1;
				}

				for( int j = 0; j < sortedEdgeList[ 0 ].Count; j++ ) {

					TopoDS_Edge targetEdge = sortedEdgeList[ 0 ][ j ];
					bCanFitInWire = CheckEdgeCanFitInWire( sortedEdgeList[ nLastIndex ], ref targetEdge );

					if( bCanFitInWire ) {
						sortedEdgeList[ 0 ].RemoveAt( j );
						j--;
						break;
					}
				}
				if( sortedEdgeList[ 0 ].Count == 0 ) {
					break;
				}
			}
			sortedEdgeList.RemoveAt( 0 );

			return sortedEdgeList;
		}

		static bool CheckEdgeCanFitInWire( List<TopoDS_Edge> edgeList, ref TopoDS_Edge targetEdge )
		{
			int nLastIndex = edgeList.Count - 1;

			// get start end vertex of target edge
			TopoDS_Vertex targetEdgeStartVertex = new TopoDS_Vertex();
			TopoDS_Vertex targetEdgeEndVertex = new TopoDS_Vertex();
			ShapeAnalysis.FindBounds( targetEdge, ref targetEdgeStartVertex, ref targetEdgeEndVertex );

			// get start end vertex of start edge
			TopoDS_Edge firstEdge = edgeList[ 0 ];
			TopoDS_Vertex firstEdgeStartVertex = new TopoDS_Vertex();
			TopoDS_Vertex firstEdgeEndVertex = new TopoDS_Vertex();
			ShapeAnalysis.FindBounds( firstEdge, ref firstEdgeStartVertex, ref firstEdgeEndVertex );

			// get start end vertex of end edge
			TopoDS_Edge lastEdge = edgeList[ nLastIndex ];
			TopoDS_Vertex lastEdgeStartVertex = new TopoDS_Vertex();
			TopoDS_Vertex lastEdgeEndVertex = new TopoDS_Vertex();
			ShapeAnalysis.FindBounds( lastEdge, ref lastEdgeStartVertex, ref lastEdgeEndVertex );

			if( IsVertexEqual( firstEdgeStartVertex, targetEdgeEndVertex ) ) {
				edgeList.Insert( 0, targetEdge );
				return true;
			}

			if( IsVertexEqual( lastEdgeEndVertex, targetEdgeStartVertex ) ) {
				edgeList.Add( targetEdge );
				return true;
			}

			// change the direction of edge for wrong direction
			if( IsVertexEqual( firstEdgeStartVertex, targetEdgeStartVertex ) ) {
				targetEdge.Reverse();
				edgeList.Insert( 0, targetEdge );
				return true;
			}

			if( IsVertexEqual( lastEdgeEndVertex, targetEdgeEndVertex ) ) {
				targetEdge.Reverse();
				edgeList.Add( targetEdge );
				return true;
			}

			return false;
		}

		static bool IsVertexEqual( TopoDS_Vertex vtx1, TopoDS_Vertex vtx2 )
		{
			gp_Pnt gpPoint1 = BRep_Tool.Pnt( vtx1 );
			gp_Pnt gpPoint2 = BRep_Tool.Pnt( vtx2 );

			if( gpPoint1.IsEqual( gpPoint2, 0.001 ) ) {
				return true;
			}
			return false;
		}
	}
}
