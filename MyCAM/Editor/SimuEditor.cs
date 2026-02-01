using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.Helper;
using MyCAM.PathCache;
using MyCAM.Post;
using OCC.AIS;
using OCC.BRep;
using OCC.BRepAlgoAPI;
using OCC.BRepMesh;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.Poly;
using OCC.Quantity;
using OCC.TColStd;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopLoc;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SimuEditor : EditorBase
	{
		public SimuEditor( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_MachineData = dataManager.MachineData;
			m_PostSolver = new PostSolver( m_MachineData );
			m_TraverseRender = new TraverseRenderer( m_Viewer, m_DataManager );
			m_FCLTest = new CollisionSolver();

			// build chain list
			m_ChainListMap.Clear();
			BuildDefaultMachineTree();

			// all chain need to be build before workpiece chain
			BuildChainList( m_SimulationTreeRoot, new List<MachineComponentType>() );
			BuildWorkpieceChain();
		}

		// simulation properties
		MachineData m_MachineData;
		PostSolver m_PostSolver;
		CollisionSolver m_FCLTest;
		MachineTreeNode m_SimulationTreeRoot = null;

		// read from machine data
		HashSet<MachineComponentType> m_WorkPieceChainSet = new HashSet<MachineComponentType>();
		Dictionary<MachineComponentType, List<MachineComponentType>> m_ChainListMap = new Dictionary<MachineComponentType, List<MachineComponentType>>();
		Dictionary<MachineComponentType, List<AIS_InteractiveObject>> m_MachineShapeMap = new Dictionary<MachineComponentType, List<AIS_InteractiveObject>>();

		// calculated result
		Dictionary<MachineComponentType, List<gp_Trsf>> m_FrameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
		Dictionary<MachineComponentType, List<bool>> m_FrameCollisionMap = new Dictionary<MachineComponentType, List<bool>>();

		// frame control
		const int INIT_FRAME_INDEX = 0;
		const int INIT_FRAME_COUNT = 0;
		int m_FrameCount = INIT_FRAME_COUNT;
		int m_CurrentFrameIndex = INIT_FRAME_INDEX;
		bool m_IsNeedReCal = true;
		Timer m_PlayTimer;

		// machinal UI
		MachineAppearance m_MachineAppearance;
		bool m_IsImportMachine = false;

		// been solved IK point
		List<PostData> m_PostDataList = new List<PostData>();

		// traverse show on viewer
		TraverseRenderer m_TraverseRender;

		// editor
		public override EEditorType Type
		{
			get
			{
				return EEditorType.Simulation;
			}
		}

		public override void EditStart()
		{
			ShowSpecificCAM();

			// set machineShapeBack
			if( m_IsImportMachine ) {
				ReShowMachine();
				RegisterMeshForCollision();

				// set map for workpieces, cause collision simulation need to refresh workpieces color
				SetWorkPiecesMap();

				// 這個步驟沒有導致畫布改變
				RegisterWorkpieceForCollision();
				m_Viewer.UpdateView();
			}

			// get post data
			SetPostDataList();
			m_IsNeedReCal = true;
		}

		public override void EditEnd()
		{
			ClearCash();
			ResetUI();
		}

		public void ImportStl()
		{
			string szFolderName = string.Empty;
			using( FolderBrowserDialog folderDialog = new FolderBrowserDialog() ) {
				folderDialog.ShowNewFolderButton = false;

				// show folder dialog
				if( folderDialog.ShowDialog() != DialogResult.OK ) {
					return;
				}

				// get the folder name
				szFolderName = folderDialog.SelectedPath;
			}

			// get the file name
			if( string.IsNullOrEmpty( szFolderName ) ) {
				return;
			}

			// remove old data
			RemoveMachineShape();
			m_MachineShapeMap.Clear();
			m_IsNeedReCal = true;
			m_IsImportMachine = false;

			// show stl on viewer
			bool isReadSuccess = ReadStlToShowOnViewer( szFolderName );
			if( isReadSuccess == false ) {
				return;
			}
			RegisterMeshForCollision();

			// this step doesn't cause canvas change
			RegisterWorkpieceForCollision();

			// set map for workpieces, cause collision simulation need to refresh workpieces color
			SetWorkPiecesMap();
			m_IsImportMachine = true;
		}

		void BuildSimuData()
		{
			if( m_FCLTest == null || m_PostSolver == null || m_PostSolver.G54Offset == null || m_DataManager.PathIDList == null || m_DataManager.PathIDList.Count == 0 ) {
				return;
			}
			if( m_IsImportMachine == false ) {
				bool isSetSuccesss = GetDefaultStl();
				if( isSetSuccesss == false ) {
					return;
				}
			}

			// need last path end point's vector to calculate exit pnt
			IProcessPoint lastpathLastPoint = GetProcessEndPoint( m_DataManager.PathIDList.Last() );
			SimulationRequiredData calNeedData = new SimulationRequiredData()
			{
				EachPathIKPostDataList = m_PostDataList,
				LastPathLastPnt = lastpathLastPoint,
				EntryAndExitData = m_DataManager.EntryAndExitData,
				PostSolver = m_PostSolver,
				WorkPiecesChaintSet = m_WorkPieceChainSet,
				MachineData = m_MachineData,
				ChainListMap = m_ChainListMap,
				FCLTest = m_FCLTest,
			};
			bool bFrameCalDone = SimulationHelper.BuildSimuData( calNeedData, out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap, out int frameCount );
			if( !bFrameCalDone ) {
				MyApp.Logger.ShowOnLogPanel( "模擬資料建立失敗", MyApp.NoticeType.Warning );
				return;
			}
			bool bCollisionCalDone = CollisionHelper.CalCollisionResult( frameCount, calNeedData, frameTransformMap, out Dictionary<MachineComponentType, List<bool>> frameCollisionMap );
			if( !bCollisionCalDone ) {
				MyApp.Logger.ShowOnLogPanel( "防碰撞資料建立失敗", MyApp.NoticeType.Warning );
				return;
			}
			m_FrameCollisionMap = frameCollisionMap;
			m_FrameTransformMap = frameTransformMap;
			m_FrameCount = frameCount;
		}

		void ClearCash()
		{
			m_IsNeedReCal = false;
			m_FrameCount = INIT_FRAME_COUNT;
			m_CurrentFrameIndex = INIT_FRAME_INDEX;
			foreach( var key in m_FrameTransformMap.Keys.ToList() ) {
				m_FrameTransformMap[ key ].Clear();
			}
		}

		#region Init

		void SetPostDataList()
		{
			List<PostData> postDataList = GetPostDataList();
			m_PostDataList = postDataList;
		}

		List<PostData> GetPostDataList()
		{
			NCWriter writer = new NCWriter( m_DataManager );
			bool bSuccess = writer.ConvertContourSuccess( out string szErrorMessage, out List<PostData> postDataList );
			if( !bSuccess ) {
				MyApp.Logger.ShowOnLogPanel( "後處理失敗, 無法生成模擬: " + szErrorMessage, MyApp.NoticeType.Warning );
				return new List<PostData>();
			}
			return postDataList;
		}

		void BuildChainList( MachineTreeNode root, List<MachineComponentType> chainList )
		{
			if( root == null ) {
				return;
			}
			m_ChainListMap[ root.Type ] = chainList;
			foreach( MachineTreeNode child in root.Children ) {
				BuildChainList( child, new List<MachineComponentType>( chainList ) { root.Type } );
			}
		}

		// this chain calcution will be reverse
		void BuildWorkpieceChain()
		{
			m_WorkPieceChainSet.Clear();

			// protection
			if( m_ChainListMap == null || m_ChainListMap.ContainsKey( MachineComponentType.WorkPiece ) == false ) {
				return;
			}
			foreach( MachineComponentType type in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				m_WorkPieceChainSet.Add( type );
			}
		}

		#endregion

		#region UI Setting

		// for leaving simu editor
		void ResetUI()
		{
			RemoveAllCAMData();
			ResetAIS();
			RemoveMachineShape();
			m_MachineShapeMap.Clear();
		}

		void RemoveAllCAMData()
		{
			List<string> pathIDList = m_DataManager.PathIDList;
			m_TraverseRender.Remove();
			m_Viewer.UpdateView();
		}

		// transform AIS back as original
		void ResetAIS()
		{
			if( m_Viewer == null || m_MachineShapeMap == null ) {
				return;
			}
			foreach( KeyValuePair<MachineComponentType, List<AIS_InteractiveObject>> mapItem in m_MachineShapeMap ) {
				MachineComponentType type = mapItem.Key;
				List<AIS_InteractiveObject> aisList = mapItem.Value;
				if( aisList == null ) {
					continue;
				}
				foreach( AIS_InteractiveObject shape_AIS in aisList ) {
					if( shape_AIS == null ) {
						continue;
					}
					// transform back
					shape_AIS.SetLocalTransformation( new gp_Trsf() );
					if( type == MachineComponentType.WorkPiece ) {
						shape_AIS.SetLocalTransformation( new gp_Trsf() );
						shape_AIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY70 ) );
						shape_AIS.Attributes().SetFaceBoundaryDraw( true );
						shape_AIS.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
						shape_AIS.Attributes().FaceBoundaryAspect().SetWidth( 0.5 );
						shape_AIS.Attributes().FaceBoundaryAspect().SetWidth( 0.5 );
						shape_AIS.Attributes().FaceBoundaryAspect().SetWidth( 0.5 );
					}
				}
			}
		}

		void RemoveMachineShape()
		{
			if( m_Viewer == null || m_MachineShapeMap == null ) {
				return;
			}
			AIS_InteractiveContext aisContext = m_Viewer.GetAISContext();
			foreach( var machinePart in m_MachineShapeMap ) {

				// workpiece still need to show on viewer
				if( machinePart.Value != null && machinePart.Key != MachineComponentType.WorkPiece ) {
					foreach( var machine_AIS in machinePart.Value ) {
						if( machine_AIS != null ) {
							aisContext.Remove( machine_AIS, false );
						}
					}
				}
			}
		}

		// helpers for multi-shape handling per component type
		void AddMachineShapeToMap( MachineComponentType type, AIS_InteractiveObject machineAIS )
		{
			if( machineAIS == null ) {
				return;
			}
			if( !m_MachineShapeMap.ContainsKey( type ) ) {
				m_MachineShapeMap[ type ] = new List<AIS_InteractiveObject>();
			}
			m_MachineShapeMap[ type ].Add( machineAIS );
		}

		void ApplyTransformToAll( MachineComponentType type, gp_Trsf trsf )
		{
			foreach( var aisList in GetMachineShapes( type ) ) {
				aisList?.SetLocalTransformation( trsf );
			}
		}

		void ReShowMachine()
		{
			ShowMachineAISOnViewer( m_MachineAppearance );
			BuildLaserAIS();
		}

		void RegisterMeshForCollision()
		{
			if( m_MachineAppearance == null ) {
				return;
			}
			foreach( var componentType in m_MachineAppearance.Meshes.Keys ) {
				if( componentType == MachineComponentType.UnKnow ) {
					continue;
				}
				if( !m_MachineAppearance.Meshes.ContainsKey( componentType ) ) {
					continue;
				}
				if( m_MachineAppearance.Meshes[ componentType ] == null ) {
					continue;
				}
				Poly_Triangulation triangulation = m_MachineAppearance.Meshes[ componentType ];
				MeshShape( triangulation, out List<double> vertexList, out List<int> indexList );
				m_FCLTest.AddModel( componentType.ToString(), indexList.ToArray(), vertexList.ToArray() );
			}
		}

		void RegisterWorkpieceForCollision()
		{
			List<AIS_Shape> workpieceShapes = new List<AIS_Shape>();

			// get all workpiece shapes
			foreach( var ID in m_DataManager.PartIDList ) {
				workpieceShapes.Add( m_ViewManager.ViewObjectMap[ ID ].AISHandle as AIS_Shape );
			}

			// turn workpiece shapes as 1 triangulation
			bool isTranDone = BuildMergedTriangulation( workpieceShapes, out Poly_Triangulation workPiecesTrian );
			if( isTranDone == false ) {
				return;
			}

			// register to collision solver
			MeshShape( workPiecesTrian, out List<double> vertexListWP, out List<int> indexListWP );
			m_FCLTest.AddModel( MachineComponentType.WorkPiece.ToString(), indexListWP.ToArray(), vertexListWP.ToArray() );
		}

		void ShowTraverse( List<string> pathIDList )
		{
			m_TraverseRender.Show();
			m_Viewer.UpdateView();
		}

		void ShowSpecificCAM()
		{
			// take all path IDs
			List<string> pathIDList = m_DataManager.PathIDList;
			ShowTraverse( pathIDList );
		}

		void ShowMachineAISOnViewer( MachineAppearance machineAppearance )
		{
			if( machineAppearance == null ) {
				return;
			}
			foreach( var componentType in machineAppearance.AisObjects.Keys ) {
				if( componentType == MachineComponentType.UnKnow ) {
					continue;
				}
				if( !machineAppearance.AisObjects.ContainsKey( componentType ) ) {
					continue;
				}
				if( machineAppearance.AisObjects[ componentType ] == null ) {
					continue;
				}
				m_Viewer.GetAISContext().Display( machineAppearance.AisObjects[ componentType ], false );
				AddMachineShapeToMap( componentType, machineAppearance.AisObjects[ componentType ] );
			}
		}

		void SetWorkPiecesMap()
		{
			if( m_DataManager.PartIDList == null || m_DataManager.PartIDList.Count == 0 ) {
				return;
			}
			foreach( var ID in m_DataManager.PartIDList ) {
				AddMachineShapeToMap( MachineComponentType.WorkPiece, m_ViewManager.ViewObjectMap[ ID ].AISHandle as AIS_Shape );
			}
		}

		void MeshShape( Poly_Triangulation tri, out List<double> vertexList, out List<int> indexList )
		{
			vertexList = new List<double>();
			indexList = new List<int>();

			// vertex
			for( int i = 1; i <= tri.NbNodes(); i++ ) {
				gp_Pnt p = tri.Node( i );
				vertexList.Add( p.X() );
				vertexList.Add( p.Y() );
				vertexList.Add( p.Z() );
			}

			// index
			for( int i = 1; i <= tri.NbTriangles(); i++ ) {
				Poly_Triangle triangle = tri.Triangle( i );
				int index1 = 0;
				int index2 = 0;
				int index3 = 0;
				triangle.Get( ref index1, ref index2, ref index3 );
				indexList.Add( index1 - 1 ); // convert to zero-based index
				indexList.Add( index2 - 1 );
				indexList.Add( index3 - 1 );
			}

		}

		bool BuildMergedTriangulation( List<AIS_Shape> aisShapes, out Poly_Triangulation merged, double deflection = 0.1 )
		{
			merged = new Poly_Triangulation();
			if( aisShapes == null ) {
				return false;
			}
			if( aisShapes.Count == 0 ) {
				return false;
			}

			List<gp_Pnt> globalVertices = new List<gp_Pnt>();
			List<Poly_Triangle> globalTriangles = new List<Poly_Triangle>();
			int vertexOffset = 0;

			foreach( AIS_Shape ais in aisShapes ) {
				if( ais == null )
					continue;

				TopoDS_Shape shape = ais.Shape();
				if( shape == null )
					continue;

				// Ensure triangulation
				new BRepMesh_IncrementalMesh( shape, deflection );

				// Traverse faces
				for( TopExp_Explorer exp = new TopExp_Explorer( shape, TopAbs_ShapeEnum.TopAbs_FACE );
					 exp.More();
					 exp.Next() ) {
					TopoDS_Face face = TopoDS.ToFace( exp.Current() );

					TopLoc_Location loc = new TopLoc_Location();
					Poly_Triangulation tri = BRep_Tool.Triangulation( face, ref loc );

					if( tri == null || tri.IsNull() ) {
						continue;
					}

					gp_Trsf trsf = loc.Transformation();
					int nbNodes = tri.NbNodes();
					for( int i = 1; i <= nbNodes; i++ ) {
						gp_Pnt p = tri.Node( i ).Transformed( trsf );
						globalVertices.Add( p );
					}

					int nbTriangles = tri.NbTriangles();
					for( int i = 1; i <= nbTriangles; i++ ) {
						Poly_Triangle t = tri.Triangle( i );
						int a = 0;
						int b = 0;
						int c = 0;
						t.Get( ref a, ref b, ref c );

						globalTriangles.Add( new Poly_Triangle(
							a + vertexOffset,
							b + vertexOffset,
							c + vertexOffset ) );
					}

					vertexOffset += nbNodes;
				}
			}

			if( globalVertices.Count == 0 || globalTriangles.Count == 0 )
				throw new InvalidOperationException( "No triangulation generated from AIS_Shapes." );

			// -----------------------
			// Build merged Poly_Triangulation
			// -----------------------
			merged = new Poly_Triangulation(
						globalVertices.Count,
						globalTriangles.Count,
						false // no UVs
					);

			// Set vertices
			for( int i = 0; i < globalVertices.Count; i++ ) {
				merged.SetNode( i + 1, globalVertices[ i ] ); // Node 1-based
			}

			// Set triangles
			for( int i = 0; i < globalTriangles.Count; i++ ) {
				merged.SetTriangle( i + 1, globalTriangles[ i ] ); // Triangle 1-based
			}

			// Optional: compute normals
			try {
				merged.ComputeNormals();
			}
			catch {
				// ignore if not supported
				return false;
			}

			return true;
		}

		#endregion

		#region Refresh Simultion Result

		void RefreshFrame()
		{
			if( m_FrameCount == 0 ) {
				return;
			}
			if( m_CurrentFrameIndex < 0 ) {
				m_CurrentFrameIndex = m_FrameCount - 1;
			}
			if( m_CurrentFrameIndex >= m_FrameCount ) {
				return;
			}

			// refresh position
			ApplyTransformToAll( MachineComponentType.Laser, m_FrameTransformMap[ MachineComponentType.Laser ][ m_CurrentFrameIndex ] );
			ApplyTransformToAll( MachineComponentType.Tool, m_FrameTransformMap[ MachineComponentType.Tool ][ m_CurrentFrameIndex ] );
			ApplyTransformToAll( MachineComponentType.WorkPiece, m_FrameTransformMap[ MachineComponentType.WorkPiece ][ m_CurrentFrameIndex ] );
			ApplyTransformToAll( MachineComponentType.Slave, m_FrameTransformMap[ MachineComponentType.Slave ][ m_CurrentFrameIndex ] );
			ApplyTransformToAll( MachineComponentType.Master, m_FrameTransformMap[ MachineComponentType.Master ][ m_CurrentFrameIndex ] );

			// traverse transformation is the same as workpiece
			m_TraverseRender.Trans( m_FrameTransformMap[ MachineComponentType.WorkPiece ][ m_CurrentFrameIndex ] );

			// refresh color
			RefreshComponentColor( MachineComponentType.Master );
			RefreshComponentColor( MachineComponentType.Slave );
			RefreshComponentColor( MachineComponentType.Tool );
			RefrshWorkPieces();
			m_Viewer.UpdateView();
		}

		void RefreshComponentColor( MachineComponentType type )
		{
			bool isCollision = m_FrameCollisionMap[ type ][ m_CurrentFrameIndex ];
			foreach( var obj in GetMachineShapes( type ) ) {
				var tri = obj as AIS_Triangulation;
				if( tri != null ) {
					SetMeshColor( tri, type, isCollision );
					m_Viewer.GetAISContext().Redisplay( tri, false );
				}
				else {
					m_Viewer.GetAISContext().Redisplay( obj, false );
				}
			}
		}

		void SetMeshColor( AIS_Triangulation meshAIS, MachineComponentType type, bool isCollision )
		{
			// get color rgb
			Quantity_Color color;
			if( isCollision ) {
				color = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED );
			}
			else {
				switch( type ) {
					case MachineComponentType.XAxis:
						color = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_PINK );
						break;
					case MachineComponentType.YAxis:
						color = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN );
						break;
					case MachineComponentType.ZAxis:
						color = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE );
						break;
					case MachineComponentType.Master:
						color = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_YELLOW );
						break;
					case MachineComponentType.Slave:
						color = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_PURPLE );
						break;
					default:
						color = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY );
						break;
				}
			}
			int R = (int)( color.Red() * 255 );
			int G = (int)( color.Green() * 255 );
			int B = (int)( color.Blue() * 255 );
			int alpha = 255;
			int colorValue = ( alpha << 24 ) | ( B << 16 ) | ( G << 8 ) | R;

			// set mesh color
			TColStd_HArray1OfInteger colorArray = new TColStd_HArray1OfInteger( 1, meshAIS.GetTriangulation().NbNodes(), colorValue );
			meshAIS.SetColors( colorArray );
		}

		void RefrshWorkPieces()
		{
			bool isCollision = m_FrameCollisionMap[ MachineComponentType.WorkPiece ][ m_CurrentFrameIndex ];
			foreach( string ID in m_DataManager.PartIDList ) {
				if( isCollision ) {
					m_ViewManager.ViewObjectMap[ ID ].AISHandle.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_PINK ) );
				}
				else {
					m_ViewManager.ViewObjectMap[ ID ].AISHandle.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY70 ) );
					m_ViewManager.ViewObjectMap[ ID ].AISHandle.Attributes().SetFaceBoundaryDraw( true );
					m_ViewManager.ViewObjectMap[ ID ].AISHandle.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
					m_ViewManager.ViewObjectMap[ ID ].AISHandle.Attributes().FaceBoundaryAspect().SetWidth( 0.5 );
				}
			}
		}

		#endregion

		#region Read Stl

		bool ReadStlToShowOnViewer( string szFolderName )
		{
			// input check
			if( m_Viewer == null || m_ViewManager == null || m_DataManager == null ) {
				return false;
			}
			bool isReadOK = ReadAllMachineStl( szFolderName );
			if( isReadOK == false ) {
				return false;
			}
			ShowMachineAISOnViewer( m_MachineAppearance );
			BuildLaserAIS();
			m_Viewer.UpdateView();
			return true;
		}

		bool ReadAllMachineStl( string szFolderName )
		{

			bool bLoadSuccess = MachineMeshToAISHelper.LoadMachineAppearance( szFolderName, out MachineAppearance machineAppearance );
			if( bLoadSuccess == false ) {
				return false;
			}
			m_MachineAppearance = machineAppearance;
			return true;
		}

		void BuildLaserAIS()
		{
			BRepPrimAPI_MakeCylinder makeTool1 = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, -1 ) ), 0.2, 1 );
			BRepPrimAPI_MakeCylinder makeTool2 = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, -1 ), new gp_Dir( 0, 0, -1 ) ), 0.2, 1 );
			BRepAlgoAPI_Fuse makeTool = new BRepAlgoAPI_Fuse( makeTool1.Shape(), makeTool2.Shape() );
			AIS_Shape toolAIS = new AIS_Shape( makeTool.Shape() );
			toolAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			toolAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_PURPLE ) );
			toolAIS.SetTransparency( 0.8f );
			AddMachineShapeToMap( MachineComponentType.Laser, toolAIS );
			m_Viewer.GetAISContext().Display( toolAIS, false );
		}

		#endregion

		#region Play Simulation

		public void PlaySimulation( int intervalMs = 5 )
		{
			if( m_IsNeedReCal ) {
				ClearCash();
				BuildSimuData();
				m_IsNeedReCal = false;
				m_CurrentFrameIndex = 0;
			}

			if( m_FrameCount <= 0 ) {
				return;
			}

			// initialize timer
			if( m_PlayTimer == null ) {
				m_PlayTimer = new Timer();
				m_PlayTimer.Tick += OnPlayTimerTick;
			}
			m_PlayTimer.Interval = intervalMs > 0 ? intervalMs : 300;
			m_PlayTimer.Start();
		}

		public void StopSimulation()
		{
			if( m_PlayTimer != null ) {
				m_PlayTimer.Stop();
			}
			m_CurrentFrameIndex = 0;
		}

		public void PauseSimulation()
		{
			if( m_PlayTimer != null ) {
				m_PlayTimer.Stop();
			}
		}

		void OnPlayTimerTick( object sender, System.EventArgs e )
		{
			if( m_FrameCount <= 0 ) {
				StopSimulation();
				return;
			}

			if( m_CurrentFrameIndex >= m_FrameCount ) {
				StopSimulation();
				return;
			}
			RefreshFrame();
			m_CurrentFrameIndex += 5;
		}

		#endregion

		#region Default Machine Chain

		void BuildDefaultMachineTree()
		{
			switch( m_MachineData.FiveAxisType ) {
				case FiveAxisType.Table:
					m_SimulationTreeRoot = BuildDefaultTableTree();
					break;
				case FiveAxisType.Mix:
					m_SimulationTreeRoot = BuildDefaultMIxTree();
					break;
				case FiveAxisType.Spindle:
				default:
					m_SimulationTreeRoot = BuildDefaultSpindleTree();
					break;
			}
		}

		MachineTreeNode BuildDefaultSpindleTree()
		{
			// build machine tree
			MachineTreeNode BaseNode = new MachineTreeNode( MachineComponentType.Base );
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode LaserNode = new MachineTreeNode( MachineComponentType.Laser );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );

			// base node first child
			BaseNode.AddChild( XNode );
			XNode.AddChild( YNode );
			YNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			MasterNode.AddChild( SlaveNode );
			SlaveNode.AddChild( ToolNode );
			ToolNode.AddChild( LaserNode );

			// base node sceond child
			BaseNode.AddChild( WorkPieceNode );
			return BaseNode;
		}

		MachineTreeNode BuildDefaultTableTree()
		{
			// build machine tree
			MachineTreeNode BaseNode = new MachineTreeNode( MachineComponentType.Base );
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode LaserNode = new MachineTreeNode( MachineComponentType.Laser );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );

			// base node first child
			BaseNode.AddChild( MasterNode );
			MasterNode.AddChild( SlaveNode );
			SlaveNode.AddChild( WorkPieceNode );

			// base node second child
			BaseNode.AddChild( YNode );
			YNode.AddChild( XNode );
			XNode.AddChild( ZNode );
			ZNode.AddChild( ToolNode );
			ToolNode.AddChild( LaserNode );
			return BaseNode;
		}

		MachineTreeNode BuildDefaultMIxTree()
		{
			// build machine tree
			MachineTreeNode BaseNode = new MachineTreeNode( MachineComponentType.Base );
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode LaserNode = new MachineTreeNode( MachineComponentType.Laser );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );

			// base node first child
			BaseNode.AddChild( SlaveNode );
			SlaveNode.AddChild( WorkPieceNode );

			// base node second child
			BaseNode.AddChild( XNode );
			XNode.AddChild( YNode );
			YNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			MasterNode.AddChild( ToolNode );
			ToolNode.AddChild( LaserNode );
			return BaseNode;
		}

		bool GetDefaultStl()
		{
			string exeDir = AppDomain.CurrentDomain.BaseDirectory;
			string szFolderName;
			switch( m_MachineData.FiveAxisType ) {
				case FiveAxisType.Table:
					szFolderName = Path.Combine( exeDir, "Table" );
					break;
				case FiveAxisType.Mix:
					szFolderName = Path.Combine( exeDir, "Mix" );
					break;
				case FiveAxisType.Spindle:
				default:
					szFolderName = Path.Combine( exeDir, "Spindle" );
					break;
			}
			// show stl on viewer
			bool isReadSuccess = ReadStlToShowOnViewer( szFolderName );
			if( isReadSuccess == false ) {
				return false;
			}
			RegisterMeshForCollision();

			// 這個步驟沒有導致畫布改變
			RegisterWorkpieceForCollision();

			// set map for workpieces, cause collision simulation need to refresh workpieces color
			SetWorkPiecesMap();
			m_IsNeedReCal = true;
			m_IsImportMachine = true;
			return true;
		}

		#endregion

		IEnumerable<AIS_InteractiveObject> GetMachineShapes( MachineComponentType type )
		{
			if( m_MachineShapeMap.TryGetValue( type, out var list ) ) {
				return list;
			}
			return Enumerable.Empty<AIS_InteractiveObject>();
		}

		IProcessPoint GetProcessEndPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetTraverseDataCache( pathID, out ITraverseDataCache traverseDataCache ) ) {
				return null;
			}
			return traverseDataCache.GetProcessEndPoint();
		}
	}
}
