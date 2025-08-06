using DataStructure;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using System;
using System.Collections.Generic;

namespace MyCAM.CAD
{
	// part data, reference data
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
			Transform = new gp_Trsf(); // initialize to identity transform
		}

		public List<PathEdge5D> Edge5DList
		{
			get; private set;
		}

		public gp_Trsf Transform
		{
			get; private set;
		}

		public override void DoTransform( gp_Trsf transform )
		{
			base.DoTransform( transform );
			Transform.PreMultiply( transform );
		}
	}

	// TODO: extract view manager to a separate class
	internal class CADManager
	{
		public Action PartChanged;
		public Action<List<string>> FeatureAdded;
		public Action<List<string>> PathAdded;

		public CADManager()
		{
			ShapeDataContainer = new List<ShapeData>();
			ShapeDataMap = new Dictionary<string, ShapeData>();
			PartIDList = new HashSet<string>();
			PathIDList = new HashSet<string>();
			PartShape = null;
		}

		// TODO: it is not good to giving out the pointer of field
		public List<ShapeData> ShapeDataContainer
		{
			get; private set;
		}

		public Dictionary<string, ShapeData> ShapeDataMap
		{
			get; private set;
		}

		public HashSet<string> PartIDList
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
			PartIDList.Clear();
			PathIDList.Clear();

			// update all datas
			PartShape = newShape;
			ShapeDataContainer.AddRange( newShapeData );
			foreach( var shapeData in ShapeDataContainer ) {
				ShapeDataMap[ shapeData.UID ] = shapeData;
				PartIDList.Add( shapeData.UID );
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
			PartIDList.Add( szID );
			FeatureAdded?.Invoke( new List<string>() { szID } );
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

				// add valid path
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
			PathAdded?.Invoke( newPathIDList );
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
