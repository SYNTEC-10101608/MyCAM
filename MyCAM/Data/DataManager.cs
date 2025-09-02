using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	// part data, reference data
	internal class DataManager
	{
		public Action PartChanged;
		public Action<List<string>> FeatureAdded;
		public Action<List<string>> PathAdded;

		public DataManager()
		{
			ShapeDataMap = new Dictionary<string, ShapeData>();
			PartIDList = new List<string>();
			PathIDList = new List<string>();
		}

		public Dictionary<string, ShapeData> ShapeDataMap
		{
			get; private set;
		}

		public List<string> PartIDList
		{
			get; private set;
		}

		public List<string> PathIDList
		{
			get; private set;
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
			ShapeDataMap.Clear();
			PartIDList.Clear();
			PathIDList.Clear();

			// update all datas
			foreach( var shapeData in newShapeData ) {
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
			string szID = "Ref_" + GetNewPartID( newFeature );
			ShapeData newData = new ShapeData( szID, newFeature );
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
					string szID = "Path_" + ++m_PathID;
					PathData pathData = new PathData( szID, pathWire, pathElements );
					ShapeDataMap[ szID ] = pathData;
					newPathIDList.Add( szID );
				}
			}

			// add to path ID list
			PathIDList.AddRange( newPathIDList );
			PathAdded?.Invoke( newPathIDList );
		}

		public void RemovePath( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) || !PathIDList.Contains( pathID ) ) {
				return;
			}
			PathIDList.Remove( pathID );
			ShapeDataMap.Remove( pathID );
		}

		public List<CAMData> GetCAMDataList()
		{
			List<CAMData> camDataList = new List<CAMData>();
			foreach( string pathID in PathIDList ) {
				camDataList.Add( ( (PathData)ShapeDataMap[ pathID ] ).CAMData );
			}
			return camDataList;
		}

		List<ShapeData> ArrangeShapeData( TopoDS_Shape oneShape )
		{
			if( oneShape == null || oneShape.IsNull() ) {
				return new List<ShapeData>();
			}
			if( oneShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_COMPOUND ) {
				string szID = GetNewPartID( oneShape );
				return new List<ShapeData>() { new ShapeData( szID, oneShape ) };
			}
			List<ShapeData> result = new List<ShapeData>();
			foreach( TopoDS_Shape subShape in oneShape.elementsAsList ) {
				result.AddRange( ArrangeShapeData( subShape ) );
			}
			return result;
		}

		string GetNewPartID( TopoDS_Shape shape )
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
			m_PathID = 0;
		}

		// data ID
		int m_SolidID = 0;
		int m_ShellID = 0;
		int m_FaceID = 0;
		int m_WireID = 0;
		int m_EdgeID = 0;
		int m_VertexID = 0;
		int m_PathID = 0;

		public string GetUIDByShape( TopoDS_Shape shape )
		{
			if( shape == null || shape.IsNull() ) {
				return string.Empty;
			}
			foreach( var model in ShapeDataMap.Values ) {
				if( model.Shape.IsEqual( shape ) ) {
					return model.UID;
				}
			}
			return string.Empty;
		}
	}
}
