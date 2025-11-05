using OCC.BRep;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using System.Collections.Generic;

namespace MyCAM.Data
{
	internal class ShapeData
	{
		public ShapeData( string szUID, TopoDS_Shape shapeData )
		{
			UID = szUID;
			Shape = shapeData;
		}

		public string UID
		{
			get; private set;
		}

		public TopoDS_Shape Shape
		{
			get; private set;
		}

		public void SewShape( double sewTol )
		{
			TopoDS_Shape result = ShapeTool.SewShape( new List<TopoDS_Shape>() { Shape }, sewTol );
			if( result == null || result.IsNull() ) {
				return;
			}
			Shape = result;
		}

		public virtual void DoTransform( gp_Trsf transform )
		{
			BRepBuilderAPI_Transform shapeTransform = new BRepBuilderAPI_Transform( Shape, transform );
			if( !shapeTransform.IsDone() ) {
				return;
			}
			Shape = shapeTransform.Shape();
		}
	}

	// path data
	internal class PathData : ShapeData
	{
		public PathData( string szUID, TopoDS_Shape shapeData, List<PathEdge5D> pathElementList )
			: base( szUID, shapeData )
		{
			TopoDS_Vertex startVertex = new TopoDS_Vertex();
			TopoDS_Vertex endVertex = new TopoDS_Vertex();
			TopExp.Vertices( TopoDS.ToWire( shapeData ), ref startVertex, ref endVertex );
			gp_Pnt startPoint = BRep_Tool.Pnt( TopoDS.ToVertex( startVertex ) );
			gp_Pnt endPoint = BRep_Tool.Pnt( TopoDS.ToVertex( endVertex ) );
			bool isClosed = startPoint.IsEqual( endPoint, 1e-3 );

			m_CAMData = new CAMData( pathElementList, isClosed );
		}

		// to get path data from file
		public PathData( string szUID, TopoDS_Shape shapeData, CAMData camData )
		: base( szUID, shapeData )
		{
			m_CAMData = camData;
		}

		public CAMData CAMData
		{
			get
			{
				return m_CAMData;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			base.DoTransform( transform );
			m_CAMData.Transform( transform );
		}

		CAMData m_CAMData;
	}

	internal class PathEdge5D
	{
		public PathEdge5D( TopoDS_Edge pathEdge, TopoDS_Face componentFace )
		{
			PathEdge = pathEdge;
			ComponentFace = componentFace;
		}

		public TopoDS_Edge PathEdge
		{
			get; private set;
		}

		public TopoDS_Face ComponentFace
		{
			get; private set;
		}
	}
}
