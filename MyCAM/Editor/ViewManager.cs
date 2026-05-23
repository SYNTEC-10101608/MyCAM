using MyCAM.Data;
using MyCAM.Editor.Renderer;
using OCC.AIS;
using OCC.Aspect;
using OCC.gp;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopoDS;
using OCC.TopTools;
using OCCViewer;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal static class ViewHelper
	{
		public static AIS_Shape CreatePartAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY70 ) );
			aisShape.Attributes().SetFaceBoundaryDraw( true );
			aisShape.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( COLOR_FACEBOUNDARY ) );
			aisShape.Attributes().FaceBoundaryAspect().SetWidth( FACE_BOUNDARY_WIDTH );

			// Vertex style
			if( shape.ShapeType() == TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				aisShape.Attributes().PointAspect().SetTypeOfMarker( Aspect_TypeOfMarker.Aspect_TOM_BALL );
				aisShape.Attributes().PointAspect().SetColor( new Quantity_Color( COLOR_FEATURE_DEFAULT ) );
				aisShape.Attributes().PointAspect().SetScale( POINT_SIZE );
			}
			return aisShape;
		}

		public static AIS_Shape CreateFeatureAIS( TopoDS_Shape shape, Quantity_NameOfColor color = COLOR_FEATURE_DEFAULT )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( color ) );
			aisShape.SetWidth( LINE_WIDTH );

			// Vertex style
			if( shape.ShapeType() == TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				aisShape.Attributes().PointAspect().SetTypeOfMarker( Aspect_TypeOfMarker.Aspect_TOM_BALL );
				aisShape.Attributes().PointAspect().SetColor( new Quantity_Color( color ) );
				aisShape.Attributes().PointAspect().SetScale( POINT_SIZE );
			}
			return aisShape;
		}

		public static AIS_Shape CreatePathAIS( TopoDS_Shape shape, double lineWidth = LINE_WIDTH )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( COLOR_PATH ) );
			aisShape.SetWidth( lineWidth );
			return aisShape;
		}

		public const Quantity_NameOfColor COLOR_PATH = Quantity_NameOfColor.Quantity_NOC_BLUE;
		public const Quantity_NameOfColor COLOR_FEATURE_DEFAULT = Quantity_NameOfColor.Quantity_NOC_BROWN3;
		public const Quantity_NameOfColor COLOR_FACEBOUNDARY = Quantity_NameOfColor.Quantity_NOC_BLACK;
		public const double FACE_BOUNDARY_WIDTH = 0.5;
		public const int LINE_WIDTH = 2;
		public const int POINT_SIZE = 3;
	}

	internal static class SelectViewHelper
	{
		public static AIS_Shape CreateFaceAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( COLOR_DEFAULT ) );
			aisShape.Attributes().SetFaceBoundaryDraw( true );
			aisShape.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( COLOR_FACEBOUNDARY ) );
			aisShape.Attributes().FaceBoundaryAspect().SetWidth( FACE_BOUNDARY_WIDTH );
			return aisShape;
		}

		public static AIS_Shape CreateEdgeAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( COLOR_DEFAULT ) );
			aisShape.SetWidth( LINE_WIDTH );
			return aisShape;
		}

		public const Quantity_NameOfColor COLOR_DEFAULT = Quantity_NameOfColor.Quantity_NOC_GRAY50;
		public const Quantity_NameOfColor COLOR_FACEBOUNDARY = Quantity_NameOfColor.Quantity_NOC_BLACK;
		public const double FACE_BOUNDARY_WIDTH = 0.5;
		public const int LINE_WIDTH = 2;
	}

	internal class ViewObject
	{
		public ViewObject( AIS_InteractiveObject shape )
		{
			AISHandle = shape;
			Visible = true;
		}

		public bool Visible
		{
			get; set;
		}

		public AIS_InteractiveObject AISHandle
		{
			get; set;
		}
	}

	internal class ViewManager
	{
		static readonly List<Quantity_NameOfColor> TECH_Layer_Color_List = new List<Quantity_NameOfColor> {
			Quantity_NameOfColor.Quantity_NOC_BLUE,
			Quantity_NameOfColor.Quantity_NOC_DARKORANGE2,
			Quantity_NameOfColor.Quantity_NOC_PURPLE,
			Quantity_NameOfColor.Quantity_NOC_YELLOW2,
			Quantity_NameOfColor.Quantity_NOC_GREEN3,
			Quantity_NameOfColor.Quantity_NOC_TOMATO2,
			Quantity_NameOfColor.Quantity_NOC_YELLOWGREEN,
			Quantity_NameOfColor.Quantity_NOC_BROWN,
			Quantity_NameOfColor.Quantity_NOC_MAGENTA1,
			Quantity_NameOfColor.Quantity_NOC_CYAN1,
		};

		readonly Viewer m_Viewer;
		readonly HashSet<string> m_PartIDSet = new HashSet<string>();
		readonly HashSet<string> m_PathIDSet = new HashSet<string>();
		readonly TopTools_DataMapOfShapeInteger m_ShapeToIDHashMap = new TopTools_DataMapOfShapeInteger();
		readonly Dictionary<int, string> m_HashToIDDict = new Dictionary<int, string>();
		readonly Dictionary<int, TopoDS_Shape> m_HashToShapeDict = new Dictionary<int, TopoDS_Shape>();

		public ViewManager( Viewer viewer )
		{
			m_Viewer = viewer;
			ViewObjectMap = new Dictionary<string, ViewObject>();
			TreeNodeMap = new Dictionary<string, TreeNode>();
			PartNode = new TreeNode( "Part" );
			PathNode = new TreeNode( "Path" );
		}

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

		public TreeNode PathNode
		{
			get; private set;
		}

		#region Part Management

		public void AddPart( string partID, TopoDS_Shape shape, bool isFeature = false )
		{
			// validate input
			if( string.IsNullOrEmpty( partID ) || shape == null || shape.IsNull() ) {
				return;
			}

			// ensure no duplicate entry
			RemovePart( partID );

			// create AIS object based on type (feature vs part)
			AIS_Shape aisShape = isFeature
				? ViewHelper.CreateFeatureAIS( shape )
				: ViewHelper.CreatePartAIS( shape );

			// register shape-ID mapping for selection lookup
			RegisterShapeIDMapping( shape, partID );

			// add to view object map and display in viewer
			m_PartIDSet.Add( partID );
			ViewObjectMap.Add( partID, new ViewObject( aisShape ) );
			m_Viewer.GetAISContext().Display( aisShape, false );

			// add tree node under PartNode
			TreeNode node = new TreeNode( partID );
			PartNode.Nodes.Add( node );
			TreeNodeMap.Add( partID, node );
		}

		public void RemovePart( string partID )
		{
			// skip if not registered
			if( !ViewObjectMap.ContainsKey( partID ) ) {
				return;
			}

			// unregister shape-ID mapping
			ViewObject viewObject = ViewObjectMap[ partID ];
			AIS_Shape aisShape = viewObject.AISHandle as AIS_Shape;
			if( aisShape != null ) {
				TopoDS_Shape shape = aisShape.Shape();
				if( shape != null && !shape.IsNull() ) {
					UnregisterShapeIDMapping( shape );
				}
			}

			// remove from viewer and maps
			m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			ViewObjectMap.Remove( partID );
			m_PartIDSet.Remove( partID );

			// remove tree node
			if( TreeNodeMap.ContainsKey( partID ) ) {
				TreeNode node = TreeNodeMap[ partID ];
				PartNode.Nodes.Remove( node );
				TreeNodeMap.Remove( partID );
			}
		}

		public void ClearParts()
		{
			List<string> partIDs = new List<string>( m_PartIDSet );
			foreach( string id in partIDs ) {
				RemovePart( id );
			}
		}

		#endregion

		#region Path Management

		public void AddPath( string pathID )
		{
			// ensure no duplicate entry
			RemovePath( pathID );

			// get path point data
			IReadOnlyList<gp_Pnt> pointList = ToolVecAndPathVisibleHelper.GetMainPathPointList( pathID );
			if( pointList == null || pointList.Count < 2 ) {
				return;
			}

			// create polyline wire geometry
			TopoDS_Wire pathWire = ToolVecAndPathVisibleHelper.CreatePolylineWire( pointList );
			if( pathWire == null || pathWire.IsNull() ) {
				return;
			}

			// create AIS with TechLayer color
			AIS_Shape pathAIS = new AIS_Shape( pathWire );
			int nPathColorIdx = GetPathColorIndex( pathID );
			pathAIS.SetColor( new Quantity_Color( TECH_Layer_Color_List[ nPathColorIdx ] ) );
			pathAIS.SetWidth( 3.0 );

			// register shape-ID mapping for selection lookup
			RegisterShapeIDMapping( pathWire, pathID );
			m_PathIDSet.Add( pathID );

			// add to view object map and display in viewer
			if( ViewObjectMap.ContainsKey( pathID ) ) {
				ViewObjectMap.Remove( pathID );
			}
			ViewObjectMap.Add( pathID, new ViewObject( pathAIS ) );
			m_Viewer.GetAISContext().Display( pathAIS, false );
		}

		public void AddPaths( List<string> pathIDList )
		{
			foreach( string pathID in pathIDList ) {
				AddPath( pathID );
			}
		}

		public void RemovePath( string pathID )
		{
			// skip if not a registered path
			if( !ViewObjectMap.ContainsKey( pathID ) || !m_PathIDSet.Contains( pathID ) ) {
				return;
			}

			// unregister shape-ID mapping
			ViewObject viewObject = ViewObjectMap[ pathID ];
			AIS_Shape pathAIS = viewObject.AISHandle as AIS_Shape;
			if( pathAIS != null ) {
				TopoDS_Shape shape = pathAIS.Shape();
				if( shape != null && !shape.IsNull() ) {
					UnregisterShapeIDMapping( shape );
				}
			}

			// remove from viewer and maps
			m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			ViewObjectMap.Remove( pathID );
			m_PathIDSet.Remove( pathID );
		}

		public void RemovePaths( List<string> pathIDList )
		{
			foreach( string pathID in pathIDList ) {
				RemovePath( pathID );
			}
		}

		public void ClearPaths()
		{
			List<string> pathIDs = new List<string>( m_PathIDSet );
			RemovePaths( pathIDs );
		}

		public void UpdatePath( string pathID )
		{
			// if path not registered, add it as new
			if( !ViewObjectMap.ContainsKey( pathID ) || !m_PathIDSet.Contains( pathID ) ) {
				AddPath( pathID );
				return;
			}

			// if AIS handle invalid, re-add
			AIS_Shape pathAIS = ViewObjectMap[ pathID ].AISHandle as AIS_Shape;
			if( pathAIS == null ) {
				AddPath( pathID );
				return;
			}

			// unregister old shape mapping before replacing geometry
			TopoDS_Shape oldShape = pathAIS.Shape();
			if( oldShape != null && !oldShape.IsNull() ) {
				UnregisterShapeIDMapping( oldShape );
			}

			// get updated path point data
			IReadOnlyList<gp_Pnt> pointList = ToolVecAndPathVisibleHelper.GetMainPathPointList( pathID );
			if( pointList == null || pointList.Count < 2 ) {
				RemovePath( pathID );
				return;
			}

			// create new wire geometry
			TopoDS_Wire pathWire = ToolVecAndPathVisibleHelper.CreatePolylineWire( pointList );
			if( pathWire == null || pathWire.IsNull() ) {
				RemovePath( pathID );
				return;
			}

			// update color in case TechLayer changed
			int nPathColorIdx = GetPathColorIndex( pathID );
			pathAIS.SetColor( new Quantity_Color( TECH_Layer_Color_List[ nPathColorIdx ] ) );

			// replace geometry and register new mapping
			pathAIS.Set( pathWire );
			RegisterShapeIDMapping( pathWire, pathID );
			m_Viewer.GetAISContext().Redisplay( pathAIS, false );
		}

		public void UpdatePaths( List<string> pathIDList )
		{
			foreach( string pathID in pathIDList ) {
				UpdatePath( pathID );
			}
		}

		public void ShowPathTrsf( gp_Trsf trsf )
		{
			foreach( string pathID in m_PathIDSet ) {
				if( ViewObjectMap.TryGetValue( pathID, out ViewObject viewObject ) ) {
					viewObject.AISHandle.SetLocalTransformation( trsf );
				}
			}
		}

		#endregion

		#region Tree Node Management

		public void AddPathNode( string nodeID )
		{
			if( TreeNodeMap.ContainsKey( nodeID ) ) {
				return;
			}
			TreeNode node = new TreeNode( nodeID );
			PathNode.Nodes.Add( node );
			TreeNodeMap.Add( nodeID, node );
		}

		public void RemovePathNodes( int count )
		{
			int total = PathNode.Nodes.Count;
			for( int i = total; i > total - count; i-- ) {
				string key = "Path_" + i.ToString();
				TreeNodeMap.Remove( key );
				PathNode.Nodes.RemoveAt( i - 1 );
			}
		}

		#endregion

		#region Clear All

		public void ClearAll()
		{
			// remove all AIS objects from viewer
			foreach( ViewObject viewObject in ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			}

			// clear shape-ID bidirectional mappings
			m_ShapeToIDHashMap.Clear();
			m_HashToIDDict.Clear();
			m_HashToShapeDict.Clear();

			// clear view object and tree node maps
			ViewObjectMap.Clear();
			TreeNodeMap.Clear();
			m_PartIDSet.Clear();
			m_PathIDSet.Clear();

			// clear tree view nodes
			PartNode.Nodes.Clear();
			PathNode.Nodes.Clear();
		}

		#endregion

		#region Shape-ID Mapping

		public string GetUIDByShape( TopoDS_Shape shape )
		{
			if( shape == null || shape.IsNull() ) {
				return string.Empty;
			}

			if( m_ShapeToIDHashMap.IsBound( shape ) ) {
				int idHash = m_ShapeToIDHashMap.Find( shape );
				if( m_HashToIDDict.TryGetValue( idHash, out string id ) ) {
					return id;
				}
			}

			return string.Empty;
		}

		public TopoDS_Shape GetShapeByUID( string id )
		{
			if( string.IsNullOrEmpty( id ) ) {
				return null;
			}

			int idHash = id.GetHashCode();
			if( m_HashToShapeDict.TryGetValue( idHash, out TopoDS_Shape shape ) ) {
				return shape;
			}

			return null;
		}

		void RegisterShapeIDMapping( TopoDS_Shape shape, string id )
		{
			if( shape == null || shape.IsNull() || string.IsNullOrEmpty( id ) ) {
				return;
			}

			int idHash = id.GetHashCode();
			m_ShapeToIDHashMap.Bind( shape, idHash );
			m_HashToIDDict[ idHash ] = id;
			m_HashToShapeDict[ idHash ] = shape;
		}

		void UnregisterShapeIDMapping( TopoDS_Shape shape )
		{
			if( shape == null || shape.IsNull() ) {
				return;
			}

			if( m_ShapeToIDHashMap.IsBound( shape ) ) {
				int idHash = m_ShapeToIDHashMap.Find( shape );
				m_ShapeToIDHashMap.UnBind( shape );
				m_HashToIDDict.Remove( idHash );
				m_HashToShapeDict.Remove( idHash );
			}
		}

		#endregion

		int GetPathColorIndex( string pathID )
		{
			int nColorIdx = 0;
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				return nColorIdx;
			}
			if( craftData == null ) {
				return nColorIdx;
			}
			int nTechLayer = craftData.TechLayer;
			nColorIdx = nTechLayer - 1;
			if( nColorIdx < 0 || nColorIdx >= TECH_Layer_Color_List.Count ) {
				nColorIdx = 0;
			}
			return nColorIdx;
		}
	}
}
