using MyCAM.Helper;
using MyCAM.Helper.CAM;
using OCC.BRep;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using System;
using System.Collections.Generic;
using System.Linq;

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

	// support-bracket shell + the part version it was captured at; internal detail of DataManager
	struct SupportBracketShellRecord
	{
		public static readonly SupportBracketShellRecord Empty = new SupportBracketShellRecord( null, -1 );

		public SupportBracketShellRecord( TopoDS_Shell shell, long partVersion )
		{
			Shell = shell;
			PartVersion = partVersion;
		}

		public TopoDS_Shell Shell
		{
			get;
		}

		public long PartVersion
		{
			get;
		}

		// usable only when shell exists and version matches current part geometry
		public bool IsValid( long currentPartVersion )
		{
			return Shell != null && !Shell.IsNull() && PartVersion == currentPartVersion;
		}
	}

	internal class PathTreeItem
	{
		public string UID
		{
			get;
		}

		public string Name
		{
			get;
		}

		public PathTreeItem( string uid, string name )
		{
			UID = uid;
			Name = name;
		}
	}

	// part data, reference data
	internal class DataManager
	{
		public Action PartChanged;
		public Action<List<string>, bool> FeatureAdded;
		public Action<List<string>> PathAdded;
		public Action NeedOptimizedIK;
		public Action<List<string>> PathRemoved;
		public Action<List<string>> PartRemoved;

		// TODO: this is temp solution
		public DataManager()
		{
			ObjectMap = new Dictionary<string, IObject>();
			PartIDList = new List<string>();
			m_PathIDList = new List<string>();
			m_MachineData = m_DefaultMachineData;

			// get machine data trigger chain rebuild
			BuildChain();
		}

		public void ResetDataManger( Dictionary<string, IObject> objectMap, List<string> partIDList, List<string> pathIDList,
			ShapeIDsStruct shapeIDs, EntryAndExitData entryAndExitData, CalibrationData calibration, SupportBracketData supportBracketParams = null )
		{
			// check shape map is mach with partList & pathList
			Dictionary<string, IObject> checkedObjectMap = new Dictionary<string, IObject>();
			List<string> checkedPartIDList = new List<string>();
			List<string> checkedPathIDList = new List<string>();

			// read part in sequence, skip duplicates
			foreach( string partDataID in partIDList ) {
				if( objectMap.ContainsKey( partDataID ) && !checkedPartIDList.Contains( partDataID ) ) {
					checkedObjectMap[ partDataID ] = objectMap[ partDataID ];
					checkedPartIDList.Add( partDataID );
				}
			}

			// read path in sequence, skip duplicates
			foreach( string pathDataID in pathIDList ) {
				if( !objectMap.TryGetValue( pathDataID, out IObject path ) || checkedPathIDList.Contains( pathDataID ) ) {
					continue;
				}

				checkedObjectMap[ pathDataID ] = objectMap[ pathDataID ];
				checkedPathIDList.Add( pathDataID );

				if( path is GroupPathObject group ) {
					foreach( IPathObject member in group.PathList ) {
						if( objectMap.TryGetValue( member.UID, out IObject memberPath ) ) {
							checkedObjectMap.Add( member.UID, memberPath );
						}
					}
				}
			}
			ObjectMap = checkedObjectMap;
			PartIDList = checkedPartIDList;
			m_PathIDList = checkedPathIDList;
			EntryAndExitData = entryAndExitData;
			CalibrationData = calibration;
			SupportBracketParams = supportBracketParams;
			ResetShapeIDsByDTO( shapeIDs );

			// part set rebuilt from file: drop part-derived records, then re-hook notifications
			NotifyPartSetRebuilt();
			HookPartShapeReplacedEvents();
		}

		public SupportBracketData SupportBracketParams
		{
			get; set;
		}

		// store the shell together with the current part-geometry version
		public void RestoreSupportBracketShell( TopoDS_Shell shell )
		{
			if( shell == null || shell.IsNull() ) {
				return;
			}
			m_SupportBracketShellRecord = new SupportBracketShellRecord( shell, PartGeometryVersion );
		}

		// get the stored shell only when it still matches the current part geometry
		public bool TryGetValidSupportBracketShell( out TopoDS_Shell shell )
		{
			shell = m_SupportBracketShellRecord.IsValid( PartGeometryVersion ) ? m_SupportBracketShellRecord.Shell : null;
			return shell != null;
		}

		// any shell stored in this project, regardless of validity
		public bool HasSupportBracketShell
		{
			get
			{
				return m_SupportBracketShellRecord.Shell != null;
			}
		}

		// bumped whenever part geometry changes (transform / sew / rebuild)
		long PartGeometryVersion
		{
			get; set;
		}

		// bump version to signal part change; the stored shell is kept so we can tell the user it went stale
		void NotifyPartGeometryChanged()
		{
			PartGeometryVersion++;
		}

		// whole part set is replaced (import model / open project): part-derived records no longer belong here
		void NotifyPartSetRebuilt()
		{
			NotifyPartGeometryChanged();
			m_SupportBracketShellRecord = SupportBracketShellRecord.Empty;
		}

		// subscribe ShapeReplaced for every part currently in ObjectMap
		void HookPartShapeReplacedEvents()
		{
			foreach( IObject obj in ObjectMap.Values ) {
				if( obj is PartObject part ) {
					part.ShapeReplaced += OnPartShapeReplaced;
				}
			}
		}

		void OnPartShapeReplaced()
		{
			NotifyPartGeometryChanged();
		}

		public Dictionary<string, IObject> ObjectMap
		{
			get; private set;
		}

		public List<string> PartIDList
		{
			get; private set;
		}

		public IReadOnlyList<string> PathIDList
		{
			get
			{
				return m_PathIDList;
			}
		}

		public List<string> GetEditablePathIDList( List<string> pathIDList )
		{
			List<string> editablePathIDList = new List<string>();
			foreach( string pathID in pathIDList ) {
				if( !DataGettingHelper.GetPathObject( pathID, out IPathObject path ) ) {
					continue;
				}

				if( path is GroupPathObject group ) {
					editablePathIDList.AddRange( group.PathIDList );
				}
				else {
					editablePathIDList.Add( pathID );
				}
			}
			return editablePathIDList;
		}

		public List<string> GetProcessPathIDList( IReadOnlyList<string> pathIDList )
		{
			List<string> processPathIDList = new List<string>();
			for( int i = 0; i < pathIDList.Count; ++i ) {
				if( !DataGettingHelper.GetCraftDataByID( pathIDList[ i ], out CraftData craftData ) ) {
					continue;
				}
				if( craftData.ProcessPath ) {
					processPathIDList.Add( pathIDList[ i ] );
				}
			}
			return processPathIDList;
		}

		public string GetPreviousPath( string pathID )
		{
			int indexInAllPaths = m_PathIDList.IndexOf( pathID );
			if( indexInAllPaths > 0 ) {
				return m_PathIDList[ indexInAllPaths - 1 ];
			}
			return null;
		}

		public FlyCutGroupPathObject AddFlyCutGroup( List<string> pathIDList )
		{
			if( pathIDList == null || pathIDList.Count < 2 ) {
				return null;
			}

			int firstIndex = int.MaxValue;
			HashSet<string> memberIDs = new HashSet<string>();
			List<ContourPathObject> pathList = new List<ContourPathObject>();
			foreach( string pathID in pathIDList ) {
				int index = m_PathIDList.IndexOf( pathID );
				if( string.IsNullOrEmpty( pathID ) || index < 0 || !memberIDs.Add( pathID )
					|| !ObjectMap.TryGetValue( pathID, out IObject obj ) || !( obj is ContourPathObject contour )
					|| contour.UID != pathID ) {
					return null;
				}
				firstIndex = Math.Min( firstIndex, index );
				pathList.Add( contour );
			}

			string szID = PATH_NAME_PREFIX + ( ++m_PathID );
			FlyCutGroupPathObject group = new FlyCutGroupPathObject( szID, pathList );

			ObjectMap.Add( szID, group );
			m_PathIDList.RemoveAll( memberIDs.Contains );
			m_PathIDList.Insert( firstIndex, szID );

			return group;
		}

		public IReadOnlyList<PathTreeItem> GetPathTree()
		{
			List<PathTreeItem> result = new List<PathTreeItem>();

			int pathCount = 0;

			for( int i = 0; i < PathIDList.Count; ++i ) {
				string pathName = PATH_NAME_PREFIX + ++pathCount;
				result.Add( new PathTreeItem( PathIDList[ i ], pathName ) );
			}
			return result;
		}

		public void SetSortedPathIDList( List<string> pathIDList )
		{
			m_PathIDList.Clear();
			m_PathIDList.AddRange( pathIDList );
		}

		public void PathInsert( int index, string pathID )
		{
			m_PathIDList.Insert( index, pathID );
		}

		public void PathRemoveAt( int index )
		{
			m_PathIDList.RemoveAt( index );
		}

		public int PathIndexOf( string pathID )
		{
			return m_PathIDList.IndexOf( pathID );
		}

		// for machine render to get
		public MachineMeshes MachineMeshes
		{
			get
			{
				if( MachineData.MachineMeshes == null ) {
					return new MachineMeshes();
				}
				return MachineData.MachineMeshes;
			}
			set
			{
				if( value != null ) {
					MachineData.MachineMeshes = value;
				}
			}
		}

		public IMachineDataBase MachineData
		{
			get
			{
				if( m_MachineData == null ) {
					m_MachineData = m_DefaultMachineData;
					BuildChain();
				}
				return m_MachineData;
			}
			set
			{
				if( value != null ) {
					m_MachineData = value;

					// get machine data trigger chain rebuild
					BuildChain();
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

		public CalibrationData CalibrationData
		{
			get
			{
				if( m_CalibrationData == null ) {
					m_CalibrationData = new CalibrationData();
				}
				return m_CalibrationData;
			}
			set
			{
				if( value != null ) {
					m_CalibrationData = value;
				}
			}
		}

		public void AddPart( TopoDS_Shape newShape )
		{
			ResetWithNewPart( newShape, out _ );
		}

		// add a new part shape to the existing data without clearing existing parts/paths
		public void AddAdditionalPart( TopoDS_Shape newShape, PartRole role = PartRole.Workpiece )
		{
			if( newShape == null || newShape.IsNull() ) {
				return;
			}

			// Check shapelist count first
			List<TopoDS_Shape> partShapeList = CollectPartShapes( newShape );
			if( partShapeList.Count == 0 ) {
				return;
			}

			List<string> newIDs = new List<string>( partShapeList.Count );
			foreach( TopoDS_Shape partShape in partShapeList ) {
				string szID = GetNewPartID( partShape );
				PartObject newPart = new PartObject( szID, partShape, role );
				newPart.ShapeReplaced += OnPartShapeReplaced;
				ObjectMap[ szID ] = newPart;
				PartIDList.Add( szID );
				newIDs.Add( szID );
			}

			RaiseFeatureAdded( newIDs, false );
		}

		public void AddReferenceFeature( TopoDS_Shape newFeature )
		{
			if( newFeature == null || newFeature.IsNull() ) {
				return;
			}
			string szID = "Ref_" + GetNewPartID( newFeature );
			PartObject newData = new PartObject( szID, newFeature, PartRole.Reference );
			newData.ShapeReplaced += OnPartShapeReplaced;
			ObjectMap[ szID ] = newData;
			PartIDList.Add( szID );
			RaiseFeatureAdded( new List<string>() { szID } );
		}

		// NOTE: the wire must be extracted directly from the face geometry
		public void AddPath( List<TopoDS_Wire> pathWireList, TopTools_IndexedDataMapOfShapeListOfShape allEdgeMap, bool shouldTriggerEvent = true,
			double maxDeflection = GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION,
			double maxEdgeLength = GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH,
			EDiscretizeMethod discretizeMethod = GeometryTool.DEFAULT_DISCRETIZE_METHOD )
		{
			if( pathWireList == null || pathWireList.Count == 0 || allEdgeMap == null ) {
				return;
			}

			// Use the edge map to explode each wire into an ordered edge list, then call the overload that takes edge groups directly
			List<List<TopoDS_Edge>> pathEdgeGroupList = new List<List<TopoDS_Edge>>();
			foreach( var pathWire in pathWireList ) {
				pathEdgeGroupList.Add( ExplodeWireToEdgeList( pathWire ) );
			}
			AddPath( pathEdgeGroupList, allEdgeMap, shouldTriggerEvent, maxDeflection, maxEdgeLength, discretizeMethod );
		}

		// explode a wire into its ordered edge list
		List<TopoDS_Edge> ExplodeWireToEdgeList( TopoDS_Wire pathWire )
		{
			List<TopoDS_Edge> edgeList = new List<TopoDS_Edge>();
			if( pathWire == null ) {
				return edgeList;
			}
			TopExp_Explorer exp = new TopExp_Explorer( pathWire, TopAbs_ShapeEnum.TopAbs_EDGE );
			while( exp.More() ) {
				edgeList.Add( TopoDS.ToEdge( exp.Current() ) );
				exp.Next();
			}
			return edgeList;
		}

		// overload that takes edge groups directly, bypassing wire construction
		public void AddPath( List<List<TopoDS_Edge>> pathEdgeGroupList, TopTools_IndexedDataMapOfShapeListOfShape allEdgeMap, bool shouldTriggerEvent = true,
			double maxDeflection = GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION,
			double maxEdgeLength = GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH,
			EDiscretizeMethod discretizeMethod = GeometryTool.DEFAULT_DISCRETIZE_METHOD )
		{
			if( pathEdgeGroupList == null || pathEdgeGroupList.Count == 0 || allEdgeMap == null ) {
				return;
			}

			// validate each edge group, build path elements, and add to the data manager
			List<string> newPathIDList = new List<string>();
			foreach( var edgeGroup in pathEdgeGroupList ) {
				if( edgeGroup == null || edgeGroup.Count == 0 ) {
					continue;
				}

				// validate edges and build path elements
				List<PathEdge5D> pathElements = ValidateAndBuildPathElements( edgeGroup, allEdgeMap );
				if( pathElements == null ) {
					continue;
				}

				// determine if closed by comparing first and last edge endpoints
				bool isClosed = DetermineIfClosed( pathElements );

				// add valid path
				string szID;
				do {
					szID = "Path_" + ++m_PathID;
				} while( ObjectMap.ContainsKey( szID ) );
				ContourPathObject contourPathObject = new ContourPathObject( szID, pathElements, isClosed,
					maxDeflection, maxEdgeLength, discretizeMethod );
				contourPathObject.CraftData.TraverseData = new TraverseData();

				InitializeStartPoint( contourPathObject, isClosed );

				ObjectMap[ szID ] = contourPathObject;
				newPathIDList.Add( szID );
			}

			// add to path ID list
			m_PathIDList.AddRange( newPathIDList );

			if( newPathIDList.Count > 0 ) {
				NeedOptimizedIK?.Invoke();
			}

			// trigger event if needed
			if( shouldTriggerEvent && newPathIDList.Count > 0 ) {
				PathAdded?.Invoke( newPathIDList );
			}
		}

		// intialize start point to lognest edge mid
		void InitializeStartPoint( ContourPathObject contourPathObject, bool isClosed )
		{
			List<StartPointSelectStrategy> strategy = new List<StartPointSelectStrategy> { StartPointSelectStrategy.LongestSegmentMidpoint };
			List<CAMPoint> camPointList = contourPathObject.ContourCache.MainPathPointList;

			if( isClosed && StartPointHelper.TryChooseByPriority( camPointList, strategy, 0.0, out int camIndex ) ) {
				contourPathObject.CraftData.SetStartPntIndex( camPointList[ camIndex ].InitPathIndex );
			}
		}

		List<PathEdge5D> ValidateAndBuildPathElements( List<TopoDS_Edge> edgeGroup, TopTools_IndexedDataMapOfShapeListOfShape allEdgeMap )
		{
			List<PathEdge5D> pathElements = new List<PathEdge5D>();
			foreach( var oneEdge in edgeGroup ) {

				// edge not found in the edge map
				if( !allEdgeMap.Contains( oneEdge ) || allEdgeMap.FindFromKey( oneEdge ).Size() == 0 ) {
					return null;
				}

				// pick the first face from map, and it should be only one face
				if( allEdgeMap.FindFromKey( oneEdge ).Size() != 1 ) {
					return null;
				}

				// create a PathEdge5D object with the edge and its associated face
				TopoDS_Face oneFace = TopoDS.ToFace( allEdgeMap.FindFromKey( oneEdge ).First().Ptr );
				pathElements.Add( new PathEdge5D( oneEdge, oneFace ) );
			}
			return pathElements;
		}

		// determine if a path is closed by comparing first and last edge endpoints
		bool DetermineIfClosed( List<PathEdge5D> pathElements )
		{
			if( pathElements == null || pathElements.Count == 0 ) {
				return false;
			}
			if( !GetEdgeStartAndEndPoints( pathElements[ 0 ].PathEdge, out gp_Pnt firstStart, out gp_Pnt firstEnd ) ) {
				return false;
			}

			// single edge, closed if its own start and end points coincide
			if( pathElements.Count == 1 ) {
				return firstStart.IsEqual( firstEnd, CONNECT_TOLERANCE );
			}

			// multi edges, closed if the last edge end point goes back to the first edge start point
			if( !GetEdgeStartAndEndPoints( pathElements[ pathElements.Count - 1 ].PathEdge, out _, out gp_Pnt lastEnd ) ) {
				return false;
			}
			return firstStart.IsEqual( lastEnd, CONNECT_TOLERANCE );
		}

		bool GetEdgeStartAndEndPoints( TopoDS_Edge edge, out gp_Pnt startPnt, out gp_Pnt endPnt )
		{
			startPnt = null;
			endPnt = null;
			try {
				TopoDS_Vertex startVertex = new TopoDS_Vertex();
				TopoDS_Vertex endVertex = new TopoDS_Vertex();
				TopExp.Vertices( edge, ref startVertex, ref endVertex, true );
				if( startVertex == null || startVertex.IsNull() || endVertex == null || endVertex.IsNull() ) {
					return false;
				}
				startPnt = BRep_Tool.Pnt( startVertex );
				endPnt = BRep_Tool.Pnt( endVertex );
				return startPnt != null && endPnt != null;
			}
			catch {
				return false;
			}
		}

		public void AddPartAndPathsSilently( TopoDS_Shape newPartShape, List<TopoDS_Wire> pathWireList, TopTools_IndexedDataMapOfShapeListOfShape allEdgeMap )
		{
			ResetWithNewPart( newPartShape, out int nPartCount, false );
			if( nPartCount == 0 ) {
				return;
			}
			AddPath( pathWireList, allEdgeMap, false );
		}

		public void RemovePath( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) || !PathIDList.Contains( pathID ) ) {
				return;
			}

			// cannot remove the path in group
			int index = m_PathIDList.IndexOf( pathID );
			if( index == -1 || !ObjectMap.TryGetValue( pathID, out IObject obj ) ) {
				return;
			}

			IReadOnlyList<string> memberIDs = null;
			if( obj is GroupPathObject group ) {
				memberIDs = group.PathIDList;
				group.Dispose();
			}

			m_PathIDList.RemoveAt( index );
			if( memberIDs != null ) {
				m_PathIDList.InsertRange( index, memberIDs );
			}
			ObjectMap.Remove( pathID );
		}

		// remove all parts of the given role (e.g. all support brackets); paths are untouched
		public void RemovePartsByRole( PartRole role )
		{
			List<string> removedIDs = new List<string>();
			foreach( string partID in PartIDList ) {
				if( ObjectMap.TryGetValue( partID, out IObject obj )
					&& obj is PartObject part
					&& part.Role == role ) {
					removedIDs.Add( partID );
				}
			}
			if( removedIDs.Count == 0 ) {
				return;
			}
			foreach( string partID in removedIDs ) {
				PartIDList.Remove( partID );
				ObjectMap.Remove( partID );
			}
			PartRemoved?.Invoke( removedIDs );
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

		void ClearObjectCommandData()
		{
			EntryAndExitData = new EntryAndExitData();
			CalibrationData = new CalibrationData();
		}

		void RaiseFeatureAdded( List<string> ids, bool bIsShownAsFeature = true )
		{
			FeatureAdded?.Invoke( ids, bIsShownAsFeature );
		}

		void ResetWithNewPart( TopoDS_Shape newShape, out int partCount, bool shouldTriggerEvent = true )
		{
			partCount = 0;
			if( newShape == null || newShape.IsNull() ) {
				return;
			}

			// Check shapelist count before clear
			List<TopoDS_Shape> partShapeList = CollectPartShapes( newShape );
			if( partShapeList.Count == 0 ) {
				return;
			}

			// whole part set is replaced: part geometry changed
			NotifyPartSetRebuilt();

			ResetShapeIDs();
			ObjectMap.Clear();
			PartIDList.Clear();
			m_PathIDList.Clear();
			ClearObjectCommandData();

			// add each part shape to the object map and part ID list
			foreach( TopoDS_Shape partShape in partShapeList ) {
				string szID = GetNewPartID( partShape );
				PartObject newPart = new PartObject( szID, partShape );
				newPart.ShapeReplaced += OnPartShapeReplaced;
				ObjectMap[ szID ] = newPart;
				PartIDList.Add( szID );
				partCount++;
			}
			if( shouldTriggerEvent ) {

				// tell editor to refresh viewer
				PartChanged?.Invoke();
			}
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

		List<TopoDS_Shape> CollectPartShapes( TopoDS_Shape oneShape )
		{
			List<TopoDS_Shape> result = new List<TopoDS_Shape>();
			if( oneShape == null || oneShape.IsNull() ) {
				return result;
			}
			if( oneShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_COMPOUND ) {
				result.Add( oneShape );
				return result;
			}
			foreach( TopoDS_Shape subShape in oneShape.elementsAsList ) {
				result.AddRange( CollectPartShapes( subShape ) );
			}
			return result;
		}

		string GetNewPartID( TopoDS_Shape shape )
		{
			TopAbs_ShapeEnum shapeType = shape.ShapeType();
			string szType = GetShapeTypeName( shapeType );
			if( string.IsNullOrEmpty( szType ) ) {
				return string.Empty; // not a valid shape type
			}

			// find an ID that is not taken yet
			string szNewID;
			do {
				szNewID = szType + "_" + BumpShapeIDCounter( shapeType ).ToString();
			} while( ObjectMap.ContainsKey( szNewID ) );
			return szNewID;
		}

		string GetShapeTypeName( TopAbs_ShapeEnum shapeType )
		{
			switch( shapeType ) {
				case TopAbs_ShapeEnum.TopAbs_SOLID:
					return "Solid";
				case TopAbs_ShapeEnum.TopAbs_SHELL:
					return "Shell";
				case TopAbs_ShapeEnum.TopAbs_FACE:
					return "Face";
				case TopAbs_ShapeEnum.TopAbs_WIRE:
					return "Wire";
				case TopAbs_ShapeEnum.TopAbs_EDGE:
					return "Edge";
				case TopAbs_ShapeEnum.TopAbs_VERTEX:
					return "Vertex";
				default:
					return string.Empty;
			}
		}

		public string GetPathName( string pathID )
		{
			// 1 based index
			int pathOrderIndex = m_PathIDList.IndexOf( pathID );
			return PATH_NAME_PREFIX + ( pathOrderIndex + 1 ).ToString();
		}

		int BumpShapeIDCounter( TopAbs_ShapeEnum shapeType )
		{
			switch( shapeType ) {
				case TopAbs_ShapeEnum.TopAbs_SOLID:
					return ++m_SolidID;
				case TopAbs_ShapeEnum.TopAbs_SHELL:
					return ++m_ShellID;
				case TopAbs_ShapeEnum.TopAbs_FACE:
					return ++m_FaceID;
				case TopAbs_ShapeEnum.TopAbs_WIRE:
					return ++m_WireID;
				case TopAbs_ShapeEnum.TopAbs_EDGE:
					return ++m_EdgeID;
				case TopAbs_ShapeEnum.TopAbs_VERTEX:
					return ++m_VertexID;
				default:
					return 0;
			}
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

		void BuildChain()
		{
			MachineChainBuilder.Build( m_MachineData );
		}

		// data ID
		int m_SolidID = 0;
		int m_ShellID = 0;
		int m_FaceID = 0;
		int m_WireID = 0;
		int m_EdgeID = 0;
		int m_VertexID = 0;
		int m_PathID = 0;

		const string PATH_NAME_PREFIX = "Path_";

		// tolerance to judge if two path edge end points are at the same position
		const double CONNECT_TOLERANCE = 1e-3;

		// machine data
		IMachineDataBase m_MachineData = null;
		readonly SpindleTypeMachineData m_DefaultMachineData = MachineDataFactory.CreateDefaultSpindle();

		// path data
		List<string> m_PathIDList;

		// entry & exit data
		EntryAndExitData m_EntryAndExitData = new EntryAndExitData();

		// calibration data
		CalibrationData m_CalibrationData = new CalibrationData();

		SupportBracketShellRecord m_SupportBracketShellRecord = SupportBracketShellRecord.Empty;
	}
}
