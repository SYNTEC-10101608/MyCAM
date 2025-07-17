using DataStructure;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	// part data, reference data, component face data
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

		public virtual void DoTransform( gp_Trsf transform )
		{
			BRepBuilderAPI_Transform shapeTransform = new BRepBuilderAPI_Transform( Shape, transform );
			Shape = shapeTransform.Shape();
		}
	}

	// path data
	internal class PathData : ShapeData
	{
		public PathData( string szUID, TopoDS_Shape shapeData, List<PathEdge5D> pathElementList )
			: base( szUID, shapeData )
		{
			Edge5DList = pathElementList;
		}

		public List<PathEdge5D> Edge5DList
		{
			get; private set;
		}

		public gp_Trsf Transform
		{
			get
			{
				return m_Trsf;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			base.DoTransform( transform );
			m_Trsf.Multiply( transform );
		}

		gp_Trsf m_Trsf = new gp_Trsf();
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
		public Action<List<string>, EFeatureType> FeatureAdded;

		public CADManager()
		{
			ShapeDataContainer = new List<ShapeData>();
			ShapeDataMap = new Dictionary<string, ShapeData>();
			ComponetFaceIDList = new HashSet<string>();
			PathIDList = new HashSet<string>();
			PartShape = null;

			// view manager
			PartNode = new TreeNode( "Part" );
			ComponentFaceNode = new TreeNode( "Component Face" );
			PathNode = new TreeNode( "Path" );
			ViewObjectMap = new Dictionary<string, ViewObject>();
			TreeNodeMap = new Dictionary<string, TreeNode>();
		}

		public List<ShapeData> ShapeDataContainer
		{
			get; private set;
		}

		public Dictionary<string, ShapeData> ShapeDataMap
		{
			get; private set;
		}

		public HashSet<string> ComponetFaceIDList
		{
			get; private set;
		}

		public HashSet<string> PathIDList
		{
			get; private set;
		}

		// TODO: we may dont need this later
		public TopoDS_Shape PartShape
		{
			get; set;
		}

		public void AddPart( TopoDS_Shape newShape )
		{
			if( newShape == null || newShape.IsNull() ) {
				return;
			}
			List<ShapeData> newShapeData = ArrangeShapeData( newShape );
			if( newShapeData.Count == 0 ) {
				return; // no valid shape data
			}

			// clear all datas
			ResetShapeIDs();
			ShapeDataContainer.Clear();
			ShapeDataMap.Clear();
			ComponetFaceIDList.Clear();
			PathIDList.Clear();

			// update all datas
			PartShape = newShape;
			ShapeDataContainer.AddRange( newShapeData );
			foreach( var shapeData in ShapeDataContainer ) {
				ShapeDataMap[ shapeData.UID ] = shapeData;
			}
			PartChanged?.Invoke();
		}

		public void AddReferenceFeature( TopoDS_Shape newFeature )
		{
			if( newFeature == null || newFeature.IsNull() ) {
				return;
			}
			string szID = "Ref_" + GetNewShapeID( newFeature );
			ShapeData newData = new ShapeData( szID, newFeature );
			ShapeDataContainer.Add( newData );
			ShapeDataMap[ szID ] = newData;
			FeatureAdded?.Invoke( new List<string>() { szID }, EFeatureType.Reference );
		}

		public void AddComponentFaceFeature( List<TopoDS_Shape> newFaceList )
		{
			if( newFaceList == null || newFaceList.Count == 0 ) {
				return;
			}
			List<string> newComponetFaceIDList = new List<string>();
			foreach( var oneNewFace in newFaceList ) {
				if( oneNewFace == null || oneNewFace.IsNull() ) {
					continue;
				}
				string szID = "ComponentFace_" + GetNewShapeID( oneNewFace );
				ShapeData newData = new ShapeData( szID, oneNewFace );
				ShapeDataContainer.Add( newData );
				ShapeDataMap[ szID ] = newData;
				newComponetFaceIDList.Add( szID );
			}

			// add to component face ID list
			ComponetFaceIDList.UnionWith( newComponetFaceIDList );
			FeatureAdded?.Invoke( newComponetFaceIDList, EFeatureType.ComponentFace );
		}

		// the edge map contains all edges in all wire to be add
		public void AddPath( List<TopoDS_Wire> pathWireList, TopTools_IndexedDataMapOfShapeListOfShape allEdgeMap )
		{
			if( pathWireList == null || pathWireList.Count == 0 || allEdgeMap == null ) {
				return;
			}
			List<string> newPathIDList = new List<string>();
			foreach( var pathWire in pathWireList ) {

				// explore the path wire to get edges and component faces
				TopExp_Explorer exp = new TopExp_Explorer( pathWire, TopAbs_ShapeEnum.TopAbs_EDGE );
				List<PathEdge5D> pathElements = new List<PathEdge5D>();
				bool isValidPath = true;
				while( exp.More() ) {
					TopoDS_Edge oneEdge = TopoDS.ToEdge( exp.Current() );

					// edge not found in the edge map
					if( !allEdgeMap.Contains( oneEdge ) || allEdgeMap.FindFromKey( oneEdge ).Size() == 0 ) {
						isValidPath = false;
						break;
					}

					// pick the first face from map, and it should be only one face
					if( allEdgeMap.FindFromKey( oneEdge ).Size() != 1 ) {
						isValidPath = false;
						break;
					}
					TopoDS_Face oneFace = TopoDS.ToFace( allEdgeMap.FindFromKey( oneEdge ).First().Ptr );
					pathElements.Add( new PathEdge5D( oneEdge, oneFace ) );
					exp.Next();
				}
				if( isValidPath ) {
					string szID = "Path_" + GetNewShapeID( pathWire );
					PathData pathData = new PathData( szID, pathWire, pathElements );
					ShapeDataContainer.Add( pathData );
					ShapeDataMap[ szID ] = pathData;
					newPathIDList.Add( szID );
				}
			}

			// add to path ID list
			PathIDList.UnionWith( newPathIDList );
			FeatureAdded?.Invoke( newPathIDList, EFeatureType.Path );
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

		void ResetShapeIDs()
		{
			m_SolidID = 0;
			m_ShellID = 0;
			m_FaceID = 0;
			m_WireID = 0;
			m_EdgeID = 0;
			m_VertexID = 0;
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
			get; private set;
		}

		public TreeNode ComponentFaceNode
		{
			get; private set;
		}

		public TreeNode PathNode
		{
			get; private set;
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
