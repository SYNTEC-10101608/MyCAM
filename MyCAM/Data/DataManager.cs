using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	// TODO : temp struct for DTO transfer 
	internal struct ShapeIDsStruct
	{
		public int Solid_ID;
		public int Shell_ID;
		public int Face_ID;
		public int Wire_ID;
		public int Edge_ID;
		public int Vertex_ID;
		public int Path_ID;
	}

	// part data, reference data
	internal class DataManager
	{
		public Action PartChanged;
		public Action<List<string>> FeatureAdded;
		public Action<List<string>> PathAdded;

		// TODO: this is temp solution
		public DataManager()
		{
			ObjectMap = new Dictionary<string, IObject>();
			PartIDList = new List<string>();
			PathIDList = new List<string>();
			m_MachineData = m_DefaultMachineData;
		}

		public void ResetDataManger( Dictionary<string, IObject> objectMap, List<string> partIDList, List<string> pathIDList, ShapeIDsStruct shapeIDs, EntryAndExitData entryAndExitData )
		{
			// check shape map is mach with partList & pathList
			Dictionary<string, IObject> checkedObjectMap = new Dictionary<string, IObject>();
			List<string> checkedPartIDList = new List<string>();
			List<string> checkedPathIDList = new List<string>();

			// read part in sequence
			foreach( var partDataID in partIDList ) {
				if( objectMap.ContainsKey( partDataID ) ) {
					checkedObjectMap[ partDataID ] = objectMap[ partDataID ];
					checkedPartIDList.Add( partDataID );
				}
			}

			// read path in sequence
			foreach( var pathDataID in pathIDList ) {
				if( objectMap.ContainsKey( pathDataID ) ) {
					checkedObjectMap[ pathDataID ] = objectMap[ pathDataID ];
					checkedPathIDList.Add( pathDataID );
				}
			}
			ObjectMap = checkedObjectMap;
			PartIDList = checkedPartIDList;
			PathIDList = checkedPathIDList;
			EntryAndExitData = entryAndExitData;
			ResetShapeIDsByDTO( shapeIDs );
		}

		public Dictionary<string, IObject> ObjectMap
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

		public MachineData MachineData
		{
			get
			{
				if( m_MachineData == null ) {
					return m_DefaultMachineData;
				}
				return m_MachineData;
			}
			set
			{
				if( value != null ) {
					m_MachineData = value;
				}
			}
		}

		public EntryAndExitData EntryAndExitData
		{
			get
			{
				if( m_EntryAndExitData == null ) {
					m_EntryAndExitData = new EntryAndExitData();
				}
				return m_EntryAndExitData;
			}
			set
			{
				if( value != null ) {
					m_EntryAndExitData = value;
				}
			}
		}

		public void AddPart( TopoDS_Shape newShape )
		{
			if( newShape == null || newShape.IsNull() ) {
				return;
			}
			List<PartObject> newPartObjectList = ArrangePartObject( newShape );
			if( newPartObjectList.Count == 0 ) {
				return; // no valid object
			}

			// clear all datas
			ResetShapeIDs();
			ObjectMap.Clear();
			PartIDList.Clear();
			PathIDList.Clear();

			// update all datas
			foreach( var objectData in newPartObjectList ) {
				ObjectMap[ objectData.UID ] = objectData;
				PartIDList.Add( objectData.UID );
			}
			PartChanged?.Invoke();
		}

		public void AddReferenceFeature( TopoDS_Shape newFeature )
		{
			if( newFeature == null || newFeature.IsNull() ) {
				return;
			}
			string szID = "Ref_" + GetNewPartID( newFeature );
			PartObject newData = new PartObject( szID, newFeature );
			ObjectMap[ szID ] = newData;
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
					ContourPathObject contourPathObject = new ContourPathObject( szID, pathWire, pathElements );
					contourPathObject.CraftData.TraverseData = new TraverseData();
					ObjectMap[ szID ] = contourPathObject;
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
			ObjectMap.Remove( pathID );
		}

		public ShapeIDsStruct GetShapeIDsForDTO()
		{
			return new ShapeIDsStruct()
			{
				Solid_ID = m_SolidID,
				Shell_ID = m_ShellID,
				Face_ID = m_FaceID,
				Wire_ID = m_WireID,
				Edge_ID = m_EdgeID,
				Vertex_ID = m_VertexID,
				Path_ID = m_PathID,
			};
		}

		void ResetShapeIDsByDTO( ShapeIDsStruct structShapeIDs )
		{
			m_SolidID = structShapeIDs.Solid_ID;
			m_ShellID = structShapeIDs.Shell_ID;
			m_FaceID = structShapeIDs.Face_ID;
			m_WireID = structShapeIDs.Wire_ID;
			m_EdgeID = structShapeIDs.Edge_ID;
			m_VertexID = structShapeIDs.Vertex_ID;
			m_PathID = structShapeIDs.Path_ID;
		}

		List<PartObject> ArrangePartObject( TopoDS_Shape oneShape )
		{
			if( oneShape == null || oneShape.IsNull() ) {
				return new List<PartObject>();
			}
			if( oneShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_COMPOUND ) {
				string szID = GetNewPartID( oneShape );
				return new List<PartObject>() { new PartObject( szID, oneShape ) };
			}
			List<PartObject> result = new List<PartObject>();
			foreach( TopoDS_Shape subShape in oneShape.elementsAsList ) {
				result.AddRange( ArrangePartObject( subShape ) );
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

		public string GetUIDByShape( TopoDS_Shape shape )
		{
			if( shape == null || shape.IsNull() ) {
				return string.Empty;
			}
			foreach( var model in ObjectMap.Values ) {
				if( !DataGettingHelper.GetShapeObject( model.UID, out IShapeObject shapeObject ) ) {
					continue;
				}
				if( shapeObject.Shape.IsEqual( shape ) ) {
					return model.UID;
				}
			}
			return string.Empty;
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

		// machine data
		MachineData m_MachineData = null;
		readonly MixTypeMachineData m_DefaultMachineData = new MixTypeMachineData()
		{
			ToolDirection = ToolDirection.Z,
			MasterRotaryAxis = RotaryAxis.Y,
			SlaveRotaryAxis = RotaryAxis.Z,
			MasterRotaryDirection = RotaryDirection.RightHand,
			SlaveRotaryDirection = RotaryDirection.LeftHand,
			MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 ),
			SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 ),
			ToolLength = 2.0,
			ToolToMasterVec = new gp_Vec( 0, 101.2, 169.48 ),
			MCSToSlaveVec = new gp_Vec( 40.81, -384.80, -665.67 ),
		};

		// entry & exit data
		EntryAndExitData m_EntryAndExitData = new EntryAndExitData();
	}
}
