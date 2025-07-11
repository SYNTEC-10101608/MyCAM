using OCC.AIS;
using OCC.TopAbs;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.CAD
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
			get; set;
		}

		public TopoDS_Shape Shape
		{
			get; set;
		}
	}

	internal class ViewObject
	{
		public ViewObject( AIS_InteractiveObject shape )
		{
			AISHandle = shape;
		}

		public bool Visible { get; set; } = true;

		public AIS_InteractiveObject AISHandle { get; set; } = null;
	}

	// TODO: extract view manager to a separate class
	internal class CADManager
	{
		public Action PartChanged;

		public CADManager()
		{
			PartShape = null;
			ShapeDataContainer = new List<ShapeData>();
			ShapeDataMap = new Dictionary<string, ShapeData>();

			// view manager
			ViewObjectMap = new Dictionary<string, ViewObject>();
			TreeNodeMap = new Dictionary<string, TreeNode>();
		}

		public TopoDS_Shape PartShape
		{
			get; private set;
		}

		public List<ShapeData> ShapeDataContainer
		{
			get; private set;
		}

		public Dictionary<string, ShapeData> ShapeDataMap
		{
			get; private set;
		}

		public void AddPart( TopoDS_Shape newShape )
		{
			if( newShape == null || newShape.IsNull() ) {
				return;
			}
			PartShape = newShape;
			ShapeDataContainer = ArrangeShapeData( newShape );
			PartChanged?.Invoke();
		}

		List<ShapeData> ArrangeShapeData( TopoDS_Shape oneShape )
		{
			if( oneShape == null || oneShape.IsNull() ) {
				return new List<ShapeData>();
			}
			if( oneShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_COMPOUND ) {
				string szID = GetNewShapeID( oneShape );
				return new List<ShapeData>() { new ShapeData( szID, oneShape ) };
			}
			List<ShapeData> result = new List<ShapeData>();
			foreach( TopoDS_Shape subShape in oneShape.elementsAsList ) {
				result.AddRange( ArrangeShapeData( subShape ) );
			}
			return result;
		}

		string GetNewShapeID( TopoDS_Shape shape )
		{
			int nID = 0;
			string szType = string.Empty;
			switch( shape.ShapeType() ) {
				case TopAbs_ShapeEnum.TopAbs_SOLID:
					nID = ++m_SolidID;
					szType = "Solid";
					break;
				case TopAbs_ShapeEnum.TopAbs_SHELL:
					nID = ++m_ShellID;
					szType = "Shell";
					break;
				case TopAbs_ShapeEnum.TopAbs_FACE:
					nID = ++m_FaceID;
					szType = "Face";
					break;
				case TopAbs_ShapeEnum.TopAbs_WIRE:
					nID = ++m_WireID;
					szType = "Wire";
					break;
				case TopAbs_ShapeEnum.TopAbs_EDGE:
					nID = ++m_EdgeID;
					szType = "Edge";
					break;
				case TopAbs_ShapeEnum.TopAbs_VERTEX:
					nID = ++m_VertexID;
					szType = "Vertex";
					break;
				default:
					return szType; // not a valid shape type
			}
			return szType + "_" + nID.ToString();
		}

		// data ID
		int m_SolidID = 0;
		int m_ShellID = 0;
		int m_FaceID = 0;
		int m_WireID = 0;
		int m_EdgeID = 0;
		int m_VertexID = 0;

		// view manager
		public Dictionary<string, ViewObject> ViewObjectMap
		{
			get; private set;
		}

		public Dictionary<string, TreeNode> TreeNodeMap
		{
			get; private set;
		}

		public TreeNode PartNode
		{
			get; set;
		}

		public string GetUIDByShape( TopoDS_Shape shape )
		{
			if( shape == null || shape.IsNull() ) {
				return string.Empty;
			}
			foreach( var model in ShapeDataContainer ) {
				if( model.Shape.IsEqual( shape ) ) {
					return model.UID;
				}
			}
			return string.Empty;
		}
	}
}
