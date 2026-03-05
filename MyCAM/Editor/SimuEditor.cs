using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.Helper;
using MyCAM.PathCache;
using MyCAM.Post;
using OCC.AIS;
using OCC.BRep;
using OCC.BRepMesh;
using OCC.gp;
using OCC.Poly;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopLoc;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SimuEditor : EditorBase
	{
		public Action<bool> ReadStlSuccess;

		public SimuEditor( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
		}

		public void Init()
		{
			m_MachineData = m_DataManager.MachineData;
			m_PostSolver = new PostSolver( m_MachineData );
			m_TraverseRender = new TraverseRenderer( m_Viewer, m_DataManager );
			m_MachineRender = new MachineRender( m_Viewer, m_DataManager );
			m_DefaultAction = new SelectPathAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );

			// user change tree node will change which frame index to show
			( m_DefaultAction as SelectPathAction ).SelectionChange += OnPathSelectionChange;
			m_collisionEngine = new CollisionSolver();

			// build chain list
			m_ChainListMap.Clear();
			m_ChainListMap = m_DataManager.MachineChainListMap;
			m_SimulationTreeRoot = m_DataManager.SimulationTreeRoot;
			m_WorkPieceChainSet = m_DataManager.WorkPieceChain;

			// import machine stl
			bool isMachineAISReady = m_MachineRender.IsWithMachineAIS;
			ReadStlSuccess?.Invoke( isMachineAISReady );

			// import false still can calculate FK
			if( isMachineAISReady == false ) {
				MyApp.Logger.ShowOnLogPanel( "機構圖檔讀取失敗", MyApp.NoticeType.Warning );
				return;
			}
		}

		// simulation properties
		MachineData m_MachineData;
		PostSolver m_PostSolver;
		CollisionSolver m_collisionEngine;
		MachineTreeNode m_SimulationTreeRoot = null;

		// read from machine data
		HashSet<MachineComponentType> m_WorkPieceChainSet = new HashSet<MachineComponentType>();
		Dictionary<MachineComponentType, List<MachineComponentType>> m_ChainListMap = new Dictionary<MachineComponentType, List<MachineComponentType>>();

		// calculated result
		Dictionary<MachineComponentType, List<gp_Trsf>> m_FrameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
		Dictionary<MachineComponentType, List<bool>> m_FrameCollisionMap = new Dictionary<MachineComponentType, List<bool>>();
		List<SimuData.ResultData.PathStartEndIndex> m_PathStartEndIndex = new List<SimuData.ResultData.PathStartEndIndex>();
		List<bool> m_CollisionFrame = new List<bool>();

		// frame control
		const int INIT_FRAME_INDEX = 0;
		const int INIT_FRAME_COUNT = 0;
		const int INIT_PATH_INDEX = 0;
		const int INIT_SPEED_LEVEL = 2;
		int m_FrameCount = INIT_FRAME_COUNT;
		int m_CurrentFrameIndex = INIT_FRAME_INDEX;
		int m_CurrentPathValue;
		bool m_IsNeedReCal = true;
		Timer m_PlayTimer;

		// machinal UI
		bool m_IsImportMachine = false;

		// been solved IK point
		List<PostData> m_PostDataList = new List<PostData>();

		// render
		TraverseRenderer m_TraverseRender;
		MachineRender m_MachineRender;

		// speed should refer to which defined in SimuData
		int m_SpeedLevel = INIT_SPEED_LEVEL;

		// property for trigger change tree node highlight
		int CurrentPath
		{
			get
			{
				return m_CurrentPathValue;
			}
			set
			{
				m_CurrentPathValue = value;
				ChangeSelectTree();
			}
		}

		enum SimuCalStatus
		{
			Faild,
			FrameCalDone,
			CollisionCalDone
		}

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
			base.EditStart();

			// init tree
			m_TreeView.Nodes.Add( m_ViewManager.PathNode );
			m_ViewManager.PathNode.ExpandAll();

			// haven't calculate simu result, can't use tree to select which path want to play
			m_TreeView.Enabled = false;
			if( m_TreeView is MultiSelectTreeView treeView ) {
				treeView.isAllowMultiSelect = false;
			}
			ShowTraverse();

			// set machineShapeBack
			// show ais -> regist shape for collision
			if( m_MachineRender.IsWithMachineAIS ) {
				m_IsImportMachine = true;
				m_MachineRender.Show( true );
				RegisterAllSimuMemberForCollision();
			}

			// get post data
			SetPostDataList();
			m_IsNeedReCal = true;
		}

		public override void EditEnd()
		{
			StopSimulation();
			ClearCash();
			ResetUI();

			// clear tree
			m_TreeView.Nodes.Clear();
			if( m_TreeView is MultiSelectTreeView treeView ) {
				treeView.isAllowMultiSelect = true;
			}
			base.EditEnd();
		}

		#region Play Simulation

		public void PlayPrePath()
		{
			bool isRunning = false;
			if( m_PlayTimer != null ) {
				isRunning = m_PlayTimer.Enabled;
			}
			PauseSimulation();
			CalSimulationResult();

			// protection
			if( m_FrameCount <= INIT_FRAME_INDEX ) {
				return;
			}

			// is already first path, if so, stay in first path and frame 0
			if( CurrentPath <= INIT_PATH_INDEX ) {
				CurrentPath = INIT_PATH_INDEX;
			}
			else {

				// change index to previous path start index
				m_CurrentFrameIndex = m_PathStartEndIndex[ CurrentPath - 1 ].StartIndex;

				// trigger tree select change
				CurrentPath--;
			}
			if( isRunning == false ) {
				RefreshFrame();
				return;
			}
			Play();
		}

		public void PlayNextPath()
		{
			bool isRunning = false;
			if( m_PlayTimer != null ) {
				isRunning = m_PlayTimer.Enabled;
			}
			PauseSimulation();
			CalSimulationResult();

			// protection
			if( m_FrameCount <= INIT_FRAME_INDEX ) {
				return;
			}
			if( CurrentPath <= INIT_PATH_INDEX ) {
				CurrentPath = INIT_PATH_INDEX;
			}

			// it is already last path
			if( CurrentPath >= m_DataManager.PathIDList.Count - 1 ) {

				// change index to last path start index
				m_CurrentFrameIndex = m_PathStartEndIndex[ CurrentPath ].StartIndex;
			}
			else {
				// change index to next path start index
				m_CurrentFrameIndex = m_PathStartEndIndex[ CurrentPath + 1 ].StartIndex;

				// trigger tree select change
				CurrentPath++;
			}
			if( isRunning == false ) {
				RefreshFrame();
				return;
			}
			Play();
		}

		public void MoveToPreCollision()
		{
			// becaus if enable tree , the action of refresh tree will triger to select frist index of that path
			PauseSimulation( false );

			// if simulation result haven't been calculated
			CalSimulationResult();
			if( m_FrameCount <= INIT_FRAME_INDEX ) {
				m_TreeView.Enabled = true;
				return;
			}

			// find the last collision before this point
			for( int i = m_CurrentFrameIndex - 1; i >= 0; i-- ) {
				if( m_CollisionFrame[ i ] ) {
					m_CurrentFrameIndex = i;
					RefreshFrame();
					RefreshTree();
					m_TreeView.Enabled = true;
					return;
				}
			}
			MyApp.Logger.ShowOnLogPanel( "已到達第一個碰撞點", MyApp.NoticeType.Hint );
			m_TreeView.Enabled = true;
		}

		public void MoveToNextCollision()
		{
			// becaus if enable tree , the action of refresh tree will triger to select frist index of that path
			PauseSimulation( false );

			// if simulation result haven't been calculated
			CalSimulationResult();
			if( m_FrameCount <= INIT_FRAME_INDEX ) {
				m_TreeView.Enabled = true;
				return;
			}
			for( int i = m_CurrentFrameIndex + 1; i < m_FrameCount; i++ ) {
				if( m_CollisionFrame[ i ] ) {
					m_CurrentFrameIndex = i;
					RefreshFrame();
					RefreshTree();
					m_TreeView.Enabled = true;
					return;
				}
			}
			MyApp.Logger.ShowOnLogPanel( "已到達最後一個碰撞點", MyApp.NoticeType.Hint );
			m_TreeView.Enabled = true;
		}

		public void PrePoint()
		{
			// becaus if enable tree , the action of refresh tree will triger to select frist index of that path
			PauseSimulation( false );
			CalSimulationResult();

			// protection
			if( m_FrameCount <= INIT_FRAME_INDEX ) {
				m_TreeView.Enabled = true;
				return;
			}

			// now is at the path start point
			if( m_CurrentFrameIndex == m_PathStartEndIndex[ CurrentPath ].StartIndex ) {

				if( CurrentPath > INIT_PATH_INDEX ) {
					// trigger tree select change
					CurrentPath--;
				}
			}
			if( m_CurrentFrameIndex <= INIT_FRAME_INDEX ) {
				m_CurrentFrameIndex = INIT_FRAME_INDEX;
			}
			else {
				m_CurrentFrameIndex--;
			}
			RefreshFrame();
			m_TreeView.Enabled = true;
		}

		public void NextPoint()
		{
			// becaus if enable tree , the action of refresh tree will triger to select frist index of that path
			PauseSimulation( false );
			CalSimulationResult();

			// protection
			if( m_FrameCount <= INIT_FRAME_INDEX ) {
				m_TreeView.Enabled = true;
				return;
			}

			// is already at last frame of last path
			if( m_CurrentFrameIndex == m_PathStartEndIndex.Last().EndIndex ) {
				return;
			}

			// now is at the path end point
			if( m_CurrentFrameIndex == m_PathStartEndIndex[ CurrentPath ].EndIndex ) {

				// trigger tree select change
				CurrentPath++;
			}
			m_CurrentFrameIndex++;
			RefreshFrame();
			m_TreeView.Enabled = true;
		}

		public void SpeedUp()
		{
			if( m_SpeedLevel < SimuData.SpeedData.SpeedRateSheet.Count - 1 ) {
				m_SpeedLevel++;
				if( m_PlayTimer != null ) {

					// set new interval
					m_PlayTimer.Interval = SimuData.SpeedData.SpeedRateSheet[ m_SpeedLevel ].Interval;
				}
			}
		}

		public void SlowDown()
		{
			if( m_SpeedLevel > 0 ) {
				m_SpeedLevel--;
				if( m_PlayTimer != null ) {
					m_PlayTimer.Interval = SimuData.SpeedData.SpeedRateSheet[ m_SpeedLevel ].Interval;
				}
			}
		}

		public void PlaySimulation()
		{
			CalSimulationResult();
			if( m_FrameCount <= INIT_FRAME_INDEX ) {
				return;
			}
			Play();
		}

		public void CheckExitCollision()
		{
			PauseSimulation();
			CalSimulationResult();
			if( m_CollisionFrame != null && m_CollisionFrame.Count > 0 ) {
				if( m_CollisionFrame.Contains( true ) ) {
					MyApp.Logger.ShowOnLogPanel( "路徑有碰撞,請檢察模擬結果", MyApp.NoticeType.Warning, true );
					return;
				}
				MyApp.Logger.ShowOnLogPanel( "路徑沒有碰撞", MyApp.NoticeType.Warning, true );
			}
		}

		public void StopSimulation()
		{
			if( m_PlayTimer != null ) {
				m_PlayTimer.Stop();
			}
			m_TreeView.Enabled = true;
			m_CurrentFrameIndex = INIT_FRAME_INDEX;

			// will trigger tree select change
			CurrentPath = INIT_PATH_INDEX;
			RefreshFrame();
		}

		public void PauseSimulation( bool enableTreeView = true )
		{
			if( m_PlayTimer != null ) {
				m_PlayTimer.Stop();
			}
			if( enableTreeView ) {
				m_TreeView.Enabled = true;
			}
			else {
				m_TreeView.Enabled = false;
			}
		}

		void Play()
		{
			// initialize timer
			if( m_PlayTimer == null ) {
				m_PlayTimer = new Timer();
				m_PlayTimer.Tick += OnPlayTimerTick;
			}
			m_PlayTimer.Interval = SimuData.SpeedData.SpeedRateSheet[ m_SpeedLevel ].Interval;
			m_PlayTimer.Start();
			m_TreeView.Enabled = false;
		}

		#endregion

		#region Init

		void SetPostDataList()
		{
			List<PostData> postDataList = GetPostDataList();
			m_PostDataList = postDataList;
		}

		List<PostData> GetPostDataList()
		{
			NCWriter writer = new NCWriter( m_DataManager );
			bool bSuccess = writer.ConvertPostDataListSuccess( out string szErrorMessage, out List<PostData> postDataList );
			if( !bSuccess ) {
				MyApp.Logger.ShowOnLogPanel( "後處理失敗, 無法生成模擬: " + szErrorMessage, MyApp.NoticeType.Warning );
				return new List<PostData>();
			}
			return postDataList;
		}

		#endregion

		#region Calculate Simulation Result

		SimuCalStatus BuildSimuData( out SimuData.ResultData.SimuCalResult simuCalResult )
		{
			const int EMPTY_Collection = 0;
			simuCalResult = new SimuData.ResultData.SimuCalResult();

			// get data to cal traverse path and all FK result
			SimuData.SimuRequiredData.SimuInputSet calNeedData = GetSimulationInputData();

			// protection
			if( calNeedData.MachineData == null
				|| calNeedData.ChainListMap == null
				|| calNeedData.PostSolver == null
				|| calNeedData.WorkPiecesChaintSet == null
				|| calNeedData.CollisionEngine == null
				|| calNeedData.WorkPiecesChaintSet.Count == EMPTY_Collection
				|| calNeedData.ChainListMap.Count == EMPTY_Collection
				 ) {
				return SimuCalStatus.Faild;
			}

			// get frame transformation map
			bool bFrameCalDone = SimulationHelper.BuildFrameTransMap( calNeedData, out Dictionary<MachineComponentType, List<gp_Trsf>> frameTransformMap, out List<SimuData.ResultData.PathStartEndIndex> pathStartEndIndexList, out int frameCount );
			if( !bFrameCalDone ) {
				return SimuCalStatus.Faild;
			}

			// record traf result
			simuCalResult.FrameTrasfMap = frameTransformMap;
			simuCalResult.FrameCount = frameCount;
			simuCalResult.PathStartEndIdxList = pathStartEndIndexList;

			// get collision result
			bool bCollisionCalDone = CollisionHelper.CalCollisionResult( frameCount, calNeedData, frameTransformMap, out Dictionary<MachineComponentType, List<bool>> frameCollisionMap );
			if( !bCollisionCalDone ) {
				return SimuCalStatus.FrameCalDone;
				;
			}
			simuCalResult.FrameCollisionMap = frameCollisionMap;
			return SimuCalStatus.CollisionCalDone;
		}

		SimuData.SimuRequiredData.SimuInputSet GetSimulationInputData()
		{
			SimuData.SimuRequiredData.SimuInputSet calNeedData = new SimuData.SimuRequiredData.SimuInputSet();

			// protection
			if( m_DataManager == null || m_DataManager.PathIDList == null || m_DataManager.PathIDList.Count == 0 ) {
				return calNeedData;
			}

			// need last path end point's vector to calculate exit pnt
			IProcessPoint lastpathLastPoint = GetProcessEndPoint( m_DataManager.PathIDList.Last() );
			calNeedData = new SimuData.SimuRequiredData.SimuInputSet()
			{
				EachPathIKPostDataList = m_PostDataList,
				LastPathLastPnt = lastpathLastPoint,
				EntryAndExitData = m_DataManager.EntryAndExitData,
				PostSolver = m_PostSolver,
				WorkPiecesChaintSet = m_WorkPieceChainSet,
				MachineData = m_MachineData,
				ChainListMap = m_ChainListMap,
				CollisionEngine = m_collisionEngine,
			};
			return calNeedData;
		}

		IProcessPoint GetProcessEndPoint( string pathID )
		{
			return CacheHelper.GetProcessEndPoint( pathID );
		}

		void RecordCollisionFrame()
		{
			if( m_FrameCollisionMap == null ) {
				return;
			}
			// initialize list with false
			m_CollisionFrame = Enumerable.Repeat( false, m_FrameCount ).ToList();

			// each component
			foreach( KeyValuePair<MachineComponentType, List<bool>> collisionList in m_FrameCollisionMap ) {
				if( collisionList.Value != null && collisionList.Value.Count > INIT_FRAME_COUNT && m_FrameCount == collisionList.Value.Count ) {
					for( int i = 0; i < collisionList.Value.Count; i++ ) {
						if( collisionList.Value[ i ] ) {

							// if any component is collision in this frame, then this frame is collision frame
							m_CollisionFrame[ i ] = true;
						}
					}
				}
			}
		}

		void ClearCash()
		{
			m_IsNeedReCal = true;
			m_FrameCount = INIT_FRAME_COUNT;
			m_CurrentFrameIndex = INIT_FRAME_INDEX;
			m_CollisionFrame.Clear();
			m_PathStartEndIndex.Clear();
			m_FrameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
			m_FrameCollisionMap = new Dictionary<MachineComponentType, List<bool>>();
		}

		#endregion

		#region UI Setting

		// for leaving simu editor
		void ResetUI()
		{
			RemoveAllCAMData();
			ResetWorkPieceAIS();
			m_MachineRender.Remove();
			m_Viewer.UpdateView();
		}

		void RefreshTree()
		{
			int currentPathIndex = GetCurrentPathByframe();

			// it will trigger tree select change
			if( currentPathIndex < INIT_PATH_INDEX ) {
				currentPathIndex = INIT_PATH_INDEX;
				return;
			}
			CurrentPath = currentPathIndex;
		}

		void RemoveAllCAMData()
		{
			m_TraverseRender.Remove();
		}

		// transform AIS back to original
		void ResetWorkPieceAIS()
		{
			if( m_Viewer == null ) {
				return;
			}
			foreach( string ID in m_DataManager.PartIDList ) {
				m_ViewManager.ViewObjectMap[ ID ].AISHandle.SetLocalTransformation( new gp_Trsf() );
				m_ViewManager.ViewObjectMap[ ID ].AISHandle.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY70 ) );
				m_ViewManager.ViewObjectMap[ ID ].AISHandle.Attributes().SetFaceBoundaryDraw( true );
				m_ViewManager.ViewObjectMap[ ID ].AISHandle.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
				m_ViewManager.ViewObjectMap[ ID ].AISHandle.Attributes().FaceBoundaryAspect().SetWidth( 0.5 );
				m_ViewManager.ViewObjectMap[ ID ].AISHandle.Attributes().FaceBoundaryAspect().SetWidth( 0.5 );
				m_ViewManager.ViewObjectMap[ ID ].AISHandle.Attributes().FaceBoundaryAspect().SetWidth( 0.5 );
			}
		}

		void RegisterAllSimuMemberForCollision()
		{
			RegisterMachineForCollision();
			RegisterWorkpieceForCollision();
		}

		void RegisterMachineForCollision()
		{
			if( m_DataManager.MachineMeshes == null ) {
				return;
			}
			foreach( var componentType in m_DataManager.MachineMeshes.Meshes.Keys ) {
				if( componentType == MachineComponentType.UnKnow ) {
					continue;
				}
				if( !m_DataManager.MachineMeshes.Meshes.ContainsKey( componentType ) ) {
					continue;
				}
				if( m_DataManager.MachineMeshes.Meshes[ componentType ] == null || m_DataManager.MachineMeshes.Meshes[ componentType ].IsNull() ) {
					continue;
				}
				Poly_Triangulation triangulation = m_DataManager.MachineMeshes.Meshes[ componentType ];
				MeshShape( triangulation, out List<double> vertexList, out List<int> indexList );
				m_collisionEngine.AddModel( componentType.ToString(), indexList.ToArray(), vertexList.ToArray() );
			}
		}

		void OnPathSelectionChange()
		{
			// is not triggered by simulation, because simulation will disable treeview
			if( m_TreeView.Enabled == false ) {
				return;
			}
			ChangeSimuPath();
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
			m_collisionEngine.AddModel( MachineComponentType.WorkPiece.ToString(), indexListWP.ToArray(), vertexListWP.ToArray() );
		}

		void ShowTraverse()
		{
			m_TraverseRender.Show();
			m_Viewer.UpdateView();
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

		bool BuildMergedTriangulation( List<AIS_Shape> aisShapeList, out Poly_Triangulation merged, double deflection = 0.1 )
		{
			merged = new Poly_Triangulation();
			if( aisShapeList == null || aisShapeList.Count == 0 ) {
				return false;
			}

			// all shape vertex ( world coordinates )
			List<gp_Pnt> globalVertices = new List<gp_Pnt>();
			List<Poly_Triangle> globalTriangles = new List<Poly_Triangle>();
			int vertexOffset = 0;

			foreach( AIS_Shape ais in aisShapeList ) {
				if( ais == null ) {
					continue;
				}
				TopoDS_Shape shape = ais.Shape();
				if( shape == null ) {
					continue;
				}

				// ensure triangulation
				new BRepMesh_IncrementalMesh( shape, deflection );

				// get face ( Poly_Triangulation is on face)
				for( TopExp_Explorer exp = new TopExp_Explorer( shape, TopAbs_ShapeEnum.TopAbs_FACE );
					 exp.More();
					 exp.Next() ) {
					TopoDS_Face face = TopoDS.ToFace( exp.Current() );

					// world coordinates
					TopLoc_Location worldCoordinate = new TopLoc_Location();

					// is face local coordinates
					Poly_Triangulation faceLocalTri = BRep_Tool.Triangulation( face, ref worldCoordinate );

					if( faceLocalTri == null || faceLocalTri.IsNull() ) {
						continue;
					}
					gp_Trsf trsf = worldCoordinate.Transformation();
					int nbNodes = faceLocalTri.NbNodes();
					for( int i = 1; i <= nbNodes; i++ ) {
						gp_Pnt p = faceLocalTri.Node( i ).Transformed( trsf );
						globalVertices.Add( p );
					}
					int nbTriangles = faceLocalTri.NbTriangles();
					for( int i = 1; i <= nbTriangles; i++ ) {
						Poly_Triangle faceTriangle = faceLocalTri.Triangle( i );
						int a = 0;
						int b = 0;
						int c = 0;
						faceTriangle.Get( ref a, ref b, ref c );
						globalTriangles.Add( new Poly_Triangle(
							a + vertexOffset,
							b + vertexOffset,
							c + vertexOffset ) );
					}
					vertexOffset += nbNodes;
				}
			}
			if( globalVertices.Count == 0 || globalTriangles.Count == 0 ) {
				merged = new Poly_Triangulation();
				return false;
			}
			merged = new Poly_Triangulation(
						globalVertices.Count,
						globalTriangles.Count,
						false // no UVs
					);

			// set vertices
			for( int i = 0; i < globalVertices.Count; i++ ) {
				merged.SetNode( i + 1, globalVertices[ i ] ); // Node 1-based
			}

			// set triangles
			for( int i = 0; i < globalTriangles.Count; i++ ) {
				merged.SetTriangle( i + 1, globalTriangles[ i ] ); // Triangle 1-based
			}

			// optional: compute normals
			try {
				merged.ComputeNormals();
			}
			catch {
				// ignore if not supported
				return false;
			}

			return true;
		}

		void ChangeSelectTree()
		{
			if( m_DefaultAction is SelectPathAction selectAction ) {
				selectAction.ClearSelection();
				if( CurrentPath < INIT_PATH_INDEX || CurrentPath >= m_DataManager.PathIDList.Count ) {
					return;
				}
				selectAction.SelectPathByID( m_DataManager.PathIDList[ CurrentPath ] );
			}
		}

		#endregion

		#region Refresh Simultion Result

		void RefreshFrame()
		{
			if( m_FrameCount == 0 ) {
				return;
			}
			if( m_CurrentFrameIndex < 0 ) {
				return;
			}
			if( m_CurrentFrameIndex >= m_FrameCount ) {
				return;
			}
			m_MachineRender.TransAndSetColor( m_CurrentFrameIndex );

			// traverse transformation is the same as workpiece
			m_TraverseRender.Trans( m_FrameTransformMap[ MachineComponentType.WorkPiece ][ m_CurrentFrameIndex ] );
			RenderWorkPieces();
			m_Viewer.UpdateView();
		}

		void RenderWorkPieces()
		{
			SetWorkPieceTransform();
			SetWorkPiecesColor();
		}

		void SetWorkPiecesColor()
		{
			if( m_FrameCollisionMap == null
			|| m_FrameCollisionMap.ContainsKey( MachineComponentType.WorkPiece ) == false
			|| m_CurrentFrameIndex >= m_FrameCollisionMap[ MachineComponentType.WorkPiece ].Count ) {
				return;
			}
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

		void SetWorkPieceTransform()
		{
			if( m_FrameTransformMap == null
			|| m_FrameTransformMap.ContainsKey( MachineComponentType.WorkPiece ) == false
			|| m_CurrentFrameIndex >= m_FrameTransformMap[ MachineComponentType.WorkPiece ].Count ) {
				return;
			}
			gp_Trsf trsf = m_FrameTransformMap[ MachineComponentType.WorkPiece ][ m_CurrentFrameIndex ];
			foreach( string ID in m_DataManager.PartIDList ) {
				m_ViewManager.ViewObjectMap[ ID ].AISHandle.SetLocalTransformation( trsf );
			}
		}

		#endregion

		#region Play Simulation

		int GetCurrentPathByframe()
		{
			for( int i = 0; i < m_PathStartEndIndex.Count; i++ ) {
				if( m_CurrentFrameIndex >= m_PathStartEndIndex[ i ].StartIndex && m_CurrentFrameIndex <= m_PathStartEndIndex[ i ].EndIndex ) {
					return i;
				}
			}
			return -1;
		}

		void ChangeSimuPath()
		{
			List<string> selectedIDs = m_DefaultAction.GetSelectedIDs();
			if( selectedIDs == null || selectedIDs.Count != 1 ) {
				return;
			}
			string selectedID = selectedIDs.First();

			for( int i = 0; i < m_DataManager.PathIDList.Count; i++ ) {
				if( string.Equals( m_DataManager.PathIDList[ i ], selectedID, StringComparison.Ordinal ) ) {
					m_CurrentFrameIndex = FindStartIndex( i );
					break;
				}
			}
		}

		int FindStartIndex( int pathIndex )
		{
			if( m_PathStartEndIndex == null ) {
				return 0;
			}
			if( pathIndex >= m_PathStartEndIndex.Count ) {
				return 0;
			}
			return m_PathStartEndIndex[ pathIndex ].StartIndex;
		}

		void CalSimulationResult()
		{
			if( m_DataManager == null || m_DataManager.PathIDList == null || m_DataManager.PathIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "尚無路徑能夠參與模擬", MyApp.NoticeType.Hint );
				return;
			}
			if( m_IsNeedReCal ) {
				ClearCash();
				SimuCalStatus status = BuildSimuData( out SimuData.ResultData.SimuCalResult simuCalResult );

				// set result to m_
				SetSimuResult( status, simuCalResult );

				// after all data is ready
				RecordCollisionFrame();
				m_MachineRender.SetSimuData( m_FrameTransformMap, m_FrameCollisionMap );
				m_IsNeedReCal = false;
			}
		}

		void SetSimuResult( SimuCalStatus status, SimuData.ResultData.SimuCalResult simuCalResult )
		{
			if( status == SimuCalStatus.Faild ) {
				MyApp.Logger.ShowOnLogPanel( "模擬計算失敗，請確認機台設定及路徑設定是否正確", MyApp.NoticeType.Warning );
				return;
			}
			m_FrameTransformMap = simuCalResult.FrameTrasfMap ?? new Dictionary<MachineComponentType, List<gp_Trsf>>();
			m_FrameCount = simuCalResult.FrameCount;
			m_PathStartEndIndex = simuCalResult.PathStartEndIdxList ?? new List<SimuData.ResultData.PathStartEndIndex>();
			if( status == SimuCalStatus.FrameCalDone ) {
				MyApp.Logger.ShowOnLogPanel( "模擬計算完成，但碰撞計算失敗", MyApp.NoticeType.Warning );
				return;
			}
			if( status == SimuCalStatus.CollisionCalDone ) {
				m_FrameCollisionMap = simuCalResult.FrameCollisionMap;
				return;
			}
		}

		void OnPlayTimerTick( object sender, System.EventArgs e )
		{
			if( m_FrameCount <= 0 ) {
				StopSimulation();
				return;
			}
			if( m_CurrentFrameIndex >= m_FrameCount - 1 ) {
				StopSimulation();
				return;
			}

			// UI would not change anymore so no need to simu
			if( m_IsImportMachine == false ) {
				StopSimulation();
				return;
			}
			RefreshFrame();
			CalNextFrameIndex();
		}

		void CalNextFrameIndex()
		{
			int nCurrentPathIndex = GetCurrentPathByframe();
			int nNextFrameIndex = m_CurrentFrameIndex + SimuData.SpeedData.SpeedRateSheet[ m_SpeedLevel ].FrameIncrease >= m_FrameCount
							? m_FrameCount - 1 // move too far
							: m_CurrentFrameIndex + SimuData.SpeedData.SpeedRateSheet[ m_SpeedLevel ].FrameIncrease;
			int lastCollisionIndex = nNextFrameIndex;

			// get the last collision frame
			for( int i = m_CurrentFrameIndex + 1; i <= nNextFrameIndex; i++ ) {
				if( m_CollisionFrame[ i ] ) {
					lastCollisionIndex = i;
				}
			}
			if( lastCollisionIndex != nNextFrameIndex ) {
				m_CurrentFrameIndex = lastCollisionIndex;
			}
			else {
				m_CurrentFrameIndex = nNextFrameIndex;
			}
			int newPathIndex = GetCurrentPathByframe();
			if( nCurrentPathIndex != newPathIndex ) {

				// trigger tree select change
				CurrentPath = newPathIndex;
			}
		}

		#endregion

		#region Default Machine Chain

		public void BuildDefaultMachineTree()
		{
			switch( m_MachineData.FiveAxisType ) {
				case FiveAxisType.Table:
					m_SimulationTreeRoot = BuildTree( SimuData.TreeData.DefaultTableTreeDef );
					break;
				case FiveAxisType.Mix:
					m_SimulationTreeRoot = BuildTree( SimuData.TreeData.DefaultMixTreeDef );
					break;
				case FiveAxisType.Spindle:
				default:
					m_SimulationTreeRoot = BuildTree( SimuData.TreeData.DefaultSpindleTreeDef );
					break;
			}
		}

		MachineTreeNode BuildTree( SimuData.TreeData.MachineTreeDef treeDefnition )
		{
			MachineTreeNode baseNode = new MachineTreeNode( treeDefnition.Type );
			if( treeDefnition.Children != null ) {
				foreach( var child in treeDefnition.Children ) {
					baseNode.AddChild( BuildTree( child ) );
				}
			}
			return baseNode;
		}

		#endregion
	}
}
