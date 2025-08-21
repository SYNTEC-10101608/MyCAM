using MyCAM.Data;
using MyCAM.Post;
using OCC.AIS;
using OCC.BRep;
using OCC.BRepAlgoAPI;
using OCC.BRepMesh;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.Poly;
using OCC.Quantity;
using OCC.RWStl;
using OCC.STEPControl;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopLoc;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SimuEditor
	{
		public SimuEditor( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
		{
			if( viewer == null || treeView == null || cadManager == null || viewManager == null ) {
				throw new ArgumentNullException( "CAMEditor consturcting argument null." );
			}

			// data manager
			m_CADManager = cadManager;

			// user interface
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			m_Viewer.KeyDown += OnKeyDown;
		}

		// data manager
		DataManager m_CADManager;

		// user interface
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;

		// simulation properties
		HashSet<MachineComponentType> m_WorkPieceChainSet = new HashSet<MachineComponentType>();
		Dictionary<MachineComponentType, List<MachineComponentType>> m_ChainListMap = new Dictionary<MachineComponentType, List<MachineComponentType>>();
		Dictionary<MachineComponentType, AIS_Shape> m_MachineShapeMap = new Dictionary<MachineComponentType, AIS_Shape>();
		PostSolver m_PostSolver;
		List<PostData> m_SimuPostData;

		// editor
		public void EditStart()
		{
			BuildSimuData();
		}

		public void EditEnd()
		{
		}

		// APIs
		public void ImportMachine()
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
			ReadMachineData( szFolderName );
		}

		public void BuildSimuData()
		{
			if( m_CADManager.GetCAMDataList().Count == 0 || m_PostSolver == null ) {
				return;
			}
			CAMData camData = m_CADManager.GetCAMDataList()[ 0 ];
			m_PostSolver.Solve( camData, out _, out m_SimuPostData );
		}

		void RefreshFrame()
		{
			if( m_SimuPostData == null ) {
				return;
			}
			if( m_CurrentFrameIndex < 0 ) {
				m_CurrentFrameIndex = m_SimuPostData.Count - 1;
			}
			if( m_CurrentFrameIndex >= m_SimuPostData.Count ) {
				m_CurrentFrameIndex = 0;
			}

			double G54X = 0;
			double G54Y = 0;
			double G54Z = 100;

			// gaet the post data
			PostData postData = m_SimuPostData[ m_CurrentFrameIndex ];

			// set XYZ transform
			Dictionary<MachineComponentType, gp_Trsf> transformMap = new Dictionary<MachineComponentType, gp_Trsf>();
			gp_Trsf trsfX = new gp_Trsf();
			trsfX.SetTranslation( new gp_Vec( postData.X + G54X, 0, 0 ) );
			if( m_WorkPieceChainSet.Contains( MachineComponentType.XAxis ) ) {
				trsfX.Invert();
			}
			transformMap[ MachineComponentType.XAxis ] = trsfX;

			gp_Trsf trsfY = new gp_Trsf();
			trsfY.SetTranslation( new gp_Vec( 0, postData.Y + G54Y, 0 ) );
			if( m_WorkPieceChainSet.Contains( MachineComponentType.YAxis ) ) {
				trsfY.Invert();
			}
			transformMap[ MachineComponentType.YAxis ] = trsfY;

			gp_Trsf trsfZ = new gp_Trsf();
			trsfZ.SetTranslation( new gp_Vec( 0, 0, postData.Z + G54Z ) );
			if( m_WorkPieceChainSet.Contains( MachineComponentType.ZAxis ) ) {
				trsfZ.Invert();
			}
			transformMap[ MachineComponentType.ZAxis ] = trsfZ;

			// set master rotation
			gp_Pnt ptOnMaster = m_PostSolver.RotaryAxisSolver.PtOnMaster;
			gp_Ax1 axisMaster = new gp_Ax1( ptOnMaster, m_PostSolver.RotaryAxisSolver.MasterRotateDir );
			gp_Trsf trsfMaster = new gp_Trsf();
			trsfMaster.SetRotation( axisMaster, postData.Master );
			if( m_WorkPieceChainSet.Contains( MachineComponentType.Master ) ) {
				trsfMaster.Invert();
			}
			transformMap[ MachineComponentType.Master ] = trsfMaster;

			// set slave rotation
			gp_Pnt ptOnSlave = m_PostSolver.RotaryAxisSolver.PtOnSlave;
			gp_Ax1 axisSlave = new gp_Ax1( ptOnSlave, m_PostSolver.RotaryAxisSolver.SlaveRotateDir );
			gp_Trsf trsfSlave = new gp_Trsf();
			trsfSlave.SetRotation( axisSlave, postData.Slave );
			if( m_WorkPieceChainSet.Contains( MachineComponentType.Slave ) ) {
				trsfSlave.Invert();
			}
			transformMap[ MachineComponentType.Slave ] = trsfSlave;

			// set chain
			gp_Trsf trsfAllX = new gp_Trsf();
			foreach( var parent in m_ChainListMap[ MachineComponentType.XAxis ] ) {
				if( parent == MachineComponentType.Base ) {
					continue; // base is not transformed
				}
				if( transformMap.ContainsKey( parent ) ) {
					trsfAllX.Multiply( transformMap[ parent ] );
				}
			}
			trsfAllX.Multiply( trsfX );
			m_MachineShapeMap[ MachineComponentType.XAxis ].SetLocalTransformation( trsfAllX );

			gp_Trsf trsfAllY = new gp_Trsf();
			foreach( var parent in m_ChainListMap[ MachineComponentType.YAxis ] ) {
				if( parent == MachineComponentType.Base ) {
					continue; // base is not transformed
				}
				if( transformMap.ContainsKey( parent ) ) {
					trsfAllY.Multiply( transformMap[ parent ] );
				}
			}
			trsfAllY.Multiply( trsfY );
			m_MachineShapeMap[ MachineComponentType.YAxis ].SetLocalTransformation( trsfAllY );

			gp_Trsf trsfAllZ = new gp_Trsf();
			foreach( var parent in m_ChainListMap[ MachineComponentType.ZAxis ] ) {
				if( parent == MachineComponentType.Base ) {
					continue; // base is not transformed
				}
				if( transformMap.ContainsKey( parent ) ) {
					trsfAllZ.Multiply( transformMap[ parent ] );
				}
			}
			trsfAllZ.Multiply( trsfZ );
			m_MachineShapeMap[ MachineComponentType.ZAxis ].SetLocalTransformation( trsfAllZ );

			gp_Trsf trsfAllMaster = new gp_Trsf();
			foreach( var parent in m_ChainListMap[ MachineComponentType.Master ] ) {
				if( parent == MachineComponentType.Base ) {
					continue; // base is not transformed
				}
				if( transformMap.ContainsKey( parent ) ) {
					trsfAllMaster.Multiply( transformMap[ parent ] );
				}
			}
			trsfAllMaster.Multiply( trsfMaster );
			m_MachineShapeMap[ MachineComponentType.Master ].SetLocalTransformation( trsfAllMaster );

			gp_Trsf trsfAllSlave = new gp_Trsf();
			foreach( var parent in m_ChainListMap[ MachineComponentType.Slave ] ) {
				if( parent == MachineComponentType.Base ) {
					continue; // base is not transformed
				}
				if( transformMap.ContainsKey( parent ) ) {
					trsfAllSlave.Multiply( transformMap[ parent ] );
				}
			}
			trsfAllSlave.Multiply( trsfSlave );
			m_MachineShapeMap[ MachineComponentType.Slave ].SetLocalTransformation( trsfAllSlave );

			// display tool
			gp_Trsf trsfTool = new gp_Trsf();
			foreach( var parent in m_ChainListMap[ MachineComponentType.Tool ] ) {
				if( parent == MachineComponentType.Base ) {
					continue; // base is not transformed
				}
				if( transformMap.ContainsKey( parent ) ) {
					trsfTool.Multiply( transformMap[ parent ] );
				}
			}
			m_MachineShapeMap[ MachineComponentType.Tool ].SetLocalTransformation( trsfTool );

			// display workpiece
			gp_Trsf trsfWorkPiece = new gp_Trsf();
			foreach( var parent in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				if( parent == MachineComponentType.Base ) {
					continue; // base is not transformed
				}
				if( transformMap.ContainsKey( parent ) ) {
					trsfWorkPiece.Multiply( transformMap[ parent ] );
				}
			}
			gp_Trsf trsfG54 = new gp_Trsf();
			trsfG54.SetTranslation( new gp_Vec( G54X, G54Y, G54Z ) );
			trsfWorkPiece.Multiply( trsfG54 );
			m_MachineShapeMap[ MachineComponentType.WorkPiece ].SetLocalTransformation( trsfWorkPiece );
			m_Viewer.UpdateView();
		}

		// TODO: read machine data from file
		void ReadMachineData( string szFolderName )
		{
			ReadSpindleTest();

			// TODO: we can read any type of 3D file, including stl
			STEPControl_Reader readerBase = new STEPControl_Reader();
			readerBase.ReadFile( szFolderName + "\\Base.stp" );
			readerBase.TransferRoots();
			TopoDS_Shape shapeBase = readerBase.OneShape();
			STEPControl_Reader readerX = new STEPControl_Reader();
			readerX.ReadFile( szFolderName + "\\X.stp" );
			readerX.TransferRoots();
			TopoDS_Shape shapeX = readerX.OneShape();
			STEPControl_Reader readerY = new STEPControl_Reader();
			readerY.ReadFile( szFolderName + "\\Y.stp" );
			readerY.TransferRoots();
			TopoDS_Shape shapeY = readerY.OneShape();
			STEPControl_Reader readerZ = new STEPControl_Reader();
			readerZ.ReadFile( szFolderName + "\\Z.stp" );
			readerZ.TransferRoots();
			TopoDS_Shape shapeZ = readerZ.OneShape();
			STEPControl_Reader readerMaster = new STEPControl_Reader();
			readerMaster.ReadFile( szFolderName + "\\Master.stp" );
			readerMaster.TransferRoots();
			TopoDS_Shape shapeMaster = readerMaster.OneShape();
			STEPControl_Reader readerSlave = new STEPControl_Reader();
			readerSlave.ReadFile( szFolderName + "\\Slave.stp" );
			readerSlave.TransferRoots();
			TopoDS_Shape shapeSlave = readerSlave.OneShape();
			m_MachineShapeMap.Clear();
			AIS_Shape baseAIS = new AIS_Shape( shapeBase );
			baseAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			baseAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
			//m_Viewer.GetAISContext().Display( baseAIS, false );
			m_MachineShapeMap[ MachineComponentType.Base ] = baseAIS;
			AIS_Shape xAIS = new AIS_Shape( shapeX );
			xAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			xAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_HOTPINK ) );
			m_Viewer.GetAISContext().Display( xAIS, false );
			m_MachineShapeMap[ MachineComponentType.XAxis ] = xAIS;
			AIS_Shape yAIS = new AIS_Shape( shapeY );
			yAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			yAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
			m_Viewer.GetAISContext().Display( yAIS, false );
			m_MachineShapeMap[ MachineComponentType.YAxis ] = yAIS;
			AIS_Shape zAIS = new AIS_Shape( shapeZ );
			zAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			zAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			m_Viewer.GetAISContext().Display( zAIS, false );
			m_MachineShapeMap[ MachineComponentType.ZAxis ] = zAIS;
			AIS_Shape masterAIS = new AIS_Shape( shapeMaster );
			masterAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			masterAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_YELLOW ) );
			m_Viewer.GetAISContext().Display( masterAIS, false );
			m_MachineShapeMap[ MachineComponentType.Master ] = masterAIS;
			AIS_Shape slaveAIS = new AIS_Shape( shapeSlave );
			slaveAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			slaveAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_PURPLE ) );
			m_Viewer.GetAISContext().Display( slaveAIS, false );
			m_MachineShapeMap[ MachineComponentType.Slave ] = slaveAIS;

			// make tool
			BRepPrimAPI_MakeCylinder makeTool1 = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, -1 ) ), 1, 1 );
			BRepPrimAPI_MakeCylinder makeTool2 = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, -1 ), new gp_Dir( 0, 0, -1 ) ), 1, 1 );
			BRepAlgoAPI_Fuse makeTool = new BRepAlgoAPI_Fuse( makeTool1.Shape(), makeTool2.Shape() );
			m_MachineShapeMap[ MachineComponentType.Tool ] = new AIS_Shape( makeTool.Shape() );
			m_MachineShapeMap[ MachineComponentType.Tool ].SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_MachineShapeMap[ MachineComponentType.Tool ].SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_PURPLE ) );
			m_Viewer.GetAISContext().Display( m_MachineShapeMap[ MachineComponentType.Tool ], false );

			// make workpiece
			m_MachineShapeMap[ MachineComponentType.WorkPiece ] = m_ViewManager.ViewObjectMap[ m_CADManager.PartIDList[ 0 ] ].AISHandle as AIS_Shape;
			m_Viewer.UpdateView();
		}

		void ReadSpindleTest()
		{
			SpindleTypeMachineData machineData = new SpindleTypeMachineData();
			machineData.ToolDirection = ToolDirection.Z;
			machineData.MasterRotaryAxis = RotaryAxis.Z;
			machineData.SlaveRotaryAxis = RotaryAxis.X;
			machineData.MasterRotaryDirection = RotaryDirection.RightHand;
			machineData.SlaveRotaryDirection = RotaryDirection.RightHand;
			machineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.ToolLength = 2.0;
			machineData.ToolToSlaveVec = new gp_Vec( -101.20, -0.19, 169.43 );
			machineData.SlaveToMasterVec = new gp_Vec( -252.70, 0, 362.98 ) - machineData.ToolToSlaveVec;

			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );
			machineData.RootNode.AddChild( XNode );
			XNode.AddChild( YNode );
			YNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			MasterNode.AddChild( SlaveNode );
			SlaveNode.AddChild( ToolNode );
			machineData.RootNode.AddChild( WorkPieceNode );

			// build chain list
			m_ChainListMap.Clear();
			BuildChainList( machineData.RootNode, new List<MachineComponentType>() );
			m_WorkPieceChainSet.Clear();
			foreach( var type in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				m_WorkPieceChainSet.Add( type );
			}

			// build solver
			m_PostSolver = new PostSolver( machineData );
		}

		void ReadTableTest()
		{
			TableTypeMachineData machineData = new TableTypeMachineData();
			machineData.ToolDirection = ToolDirection.Z;
			machineData.MasterRotaryAxis = RotaryAxis.Y;
			machineData.SlaveRotaryAxis = RotaryAxis.Z;
			machineData.MasterRotaryDirection = RotaryDirection.LeftHand;
			machineData.SlaveRotaryDirection = RotaryDirection.LeftHand;
			machineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.ToolLength = 2.0;
			machineData.MCSToMasterVec = new gp_Vec( -80.51, 73.81, -129.55 );
			machineData.MasterToSlaveVec = new gp_Vec( -80.43, -71.67, -94.55 ) - machineData.MCSToMasterVec;

			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );
			machineData.RootNode.AddChild( YNode );
			YNode.AddChild( XNode );
			XNode.AddChild( MasterNode );
			MasterNode.AddChild( SlaveNode );
			SlaveNode.AddChild( WorkPieceNode );
			machineData.RootNode.AddChild( ZNode );
			ZNode.AddChild( ToolNode );

			// build chain list
			m_ChainListMap.Clear();
			BuildChainList( machineData.RootNode, new List<MachineComponentType>() );
			m_WorkPieceChainSet.Clear();
			foreach( var type in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				m_WorkPieceChainSet.Add( type );
			}

			// build solver
			m_PostSolver = new PostSolver( machineData );
		}

		void ReadMixTest()
		{
			MixTypeMachineData machineData = new MixTypeMachineData();
			machineData.ToolDirection = ToolDirection.Z;
			machineData.MasterRotaryAxis = RotaryAxis.Y;
			machineData.SlaveRotaryAxis = RotaryAxis.Z;
			machineData.MasterRotaryDirection = RotaryDirection.RightHand;
			machineData.SlaveRotaryDirection = RotaryDirection.LeftHand;
			machineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.ToolLength = 2.0;
			machineData.ToolToMasterVec = new gp_Vec( 0, 101.2, 169.48 );
			machineData.MCSToSlaveVec = new gp_Vec( 40.81, -384.80, -665.67 );

			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );
			machineData.RootNode.AddChild( YNode );
			YNode.AddChild( SlaveNode );
			SlaveNode.AddChild( WorkPieceNode );
			machineData.RootNode.AddChild( XNode );
			XNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			MasterNode.AddChild( ToolNode );

			// build chain list
			m_ChainListMap.Clear();
			BuildChainList( machineData.RootNode, new List<MachineComponentType>() );
			m_WorkPieceChainSet.Clear();
			foreach( var type in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				m_WorkPieceChainSet.Add( type );
			}

			// build solver
			m_PostSolver = new PostSolver( machineData );
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

		// this is a temp function to test load stl
		void TestLoadSTL()
		{
			Poly_Triangulation tri = RWStl.ReadFile( "Slave.stl" );
			AIS_Triangulation triAIS = new AIS_Triangulation( tri );
			m_Viewer.GetAISContext().Display( triAIS, true );
			m_Viewer.UpdateView();
		}

		// this is a temp function to test collision detection using bullet tool
		void TestColDet()
		{
			// create a box as shapeA
			BRepPrimAPI_MakeBox boxMaker = new BRepPrimAPI_MakeBox( new gp_Pnt( -50, -50, -50 ), 100, 100, 100 );
			TopoDS_Shape shapeA = boxMaker.Shape();
			AIS_Shape m_ShapeA = new AIS_Shape( shapeA );
			m_ShapeA.SetColor( COLOR_DEFAULT );
			m_ShapeA.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			//m_ShapeA.SetTransparency( 0.5 );
			m_Viewer.GetAISContext().Display( m_ShapeA, true );
			MeshShape( shapeA, out List<double> vertexListA, out List<int> indexListA );

			// create a sphere as shapeB
			BRepPrimAPI_MakeSphere sphereMaker = new BRepPrimAPI_MakeSphere( new gp_Pnt( 0, 0, 0 ), 50 );
			TopoDS_Shape shapeB = sphereMaker.Shape();
			AIS_Shape m_ShapeB = new AIS_Shape( shapeB );
			m_ShapeB.SetColor( COLOR_DEFAULT );
			m_ShapeB.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			//m_ShapeB.SetTransparency( 0.5 );
			m_Viewer.GetAISContext().Display( m_ShapeB, true );
			MeshShape( shapeB, out List<double> vertexListB, out List<int> indexListB );

			// create a cylinder as shapeC
			BRepPrimAPI_MakeCylinder cylinderMaker = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, -300 ), new gp_Dir( 0, 0, 1 ) ), 50, 600 );
			TopoDS_Shape shapeC = cylinderMaker.Shape();
			AIS_Shape m_ShapeC = new AIS_Shape( shapeC );
			m_ShapeC.SetColor( COLOR_DEFAULT );
			m_ShapeC.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_ShapeC.SetTransparency( 0.5 );
			m_Viewer.GetAISContext().Display( m_ShapeC, true );
			MeshShape( shapeC, out List<double> vertexListC, out List<int> indexListC );

			// create list of trsf presenting shape position on each frame
			const int frameCount = 20;
			List<gp_Trsf> trsfAList = new List<gp_Trsf>();
			List<gp_Trsf> trsfBList = new List<gp_Trsf>();
			List<gp_Trsf> trsfCList = new List<gp_Trsf>();
			for( int i = 0; i <= frameCount; i++ ) {
				double pos = -300 + i * 30;
				gp_Trsf trsfA = new gp_Trsf();
				trsfA.SetTranslation( new gp_Vec( -75, 300, 0 ) );
				trsfAList.Add( trsfA );
				gp_Trsf trsfB = new gp_Trsf();
				trsfB.SetTranslation( new gp_Vec( 75, -300, 0 ) );
				trsfBList.Add( trsfB );
				gp_Trsf trsfC = new gp_Trsf();
				gp_Quaternion quat = new gp_Quaternion( new gp_Vec( 1, 0, 0 ), Math.PI * i / frameCount );
				trsfC.SetRotationPart( quat );
				trsfC.SetTranslationPart( new gp_Vec( pos, 0, 0 ) );
				trsfCList.Add( trsfC );
			}

			// create collision detection tool
			FCLTest fclTest = new FCLTest();
			fclTest.AddModel( "ShapeA", indexListA.ToArray(), vertexListA.ToArray() );
			fclTest.AddModel( "ShapeB", indexListB.ToArray(), vertexListB.ToArray() );
			fclTest.AddModel( "ShapeC", indexListC.ToArray(), vertexListC.ToArray() );

			// check collision at each frame
			List<bool> colDetA = new List<bool>();
			List<bool> colDetB = new List<bool>();
			List<bool> colDetC = new List<bool>();
			for( int i = 0; i < frameCount; i++ ) {

				// check collision
				bool bColAB = fclTest.CheckCollision( "ShapeA", "ShapeB", ConvertTransform( trsfAList[ i ] ), ConvertTransform( trsfBList[ i ] ) );
				bool bColAC = fclTest.CheckCollision( "ShapeA", "ShapeC", ConvertTransform( trsfAList[ i ] ), ConvertTransform( trsfCList[ i ] ) );
				bool bColBC = fclTest.CheckCollision( "ShapeB", "ShapeC", ConvertTransform( trsfBList[ i ] ), ConvertTransform( trsfCList[ i ] ) );
				colDetA.Add( bColAB || bColAC );
				colDetB.Add( bColAB || bColBC );
				colDetC.Add( bColAC || bColBC );
			}

			// simulate frame by frame
			m_Viewer.TopView();
			m_Viewer.KeyDown += ( e ) =>
			{
				if( e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ) {
					if( e.KeyCode == Keys.Down ) {
						m_FrameIndex++;
						if( m_FrameIndex >= frameCount ) {
							m_FrameIndex = 0;
						}
					}
					else if( e.KeyCode == Keys.Up ) {
						m_FrameIndex--;
						if( m_FrameIndex < 0 ) {
							m_FrameIndex = frameCount - 1;
						}
					}

					// apply transformation to shapes
					m_ShapeA.SetLocalTransformation( trsfAList[ m_FrameIndex ] );
					m_ShapeB.SetLocalTransformation( trsfBList[ m_FrameIndex ] );
					m_ShapeC.SetLocalTransformation( trsfCList[ m_FrameIndex ] );

					// show color to indicate collision
					m_ShapeA.SetColor( colDetA[ m_FrameIndex ] ? COLOR_COLDET : COLOR_DEFAULT );
					m_ShapeB.SetColor( colDetB[ m_FrameIndex ] ? COLOR_COLDET : COLOR_DEFAULT );
					m_ShapeC.SetColor( colDetC[ m_FrameIndex ] ? COLOR_COLDET : COLOR_DEFAULT );
					m_Viewer.UpdateView();
				}
			};
		}
		int m_FrameIndex = 0;
		readonly Quantity_Color COLOR_COLDET = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED );
		readonly Quantity_Color COLOR_DEFAULT = new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE );

		void MeshShape( TopoDS_Shape shape, out List<double> vertexList, out List<int> indexList )
		{
			vertexList = new List<double>();
			indexList = new List<int>();

			// mesh the shape
			BRepMesh_IncrementalMesh meshMaker = new BRepMesh_IncrementalMesh( shape, 0.1 );
			meshMaker.Perform();
			if( !meshMaker.IsDone() ) {
				MessageBox.Show( "Error: Mesh shape failed." );
				return;
			}

			// get the mesh data
			TopExp_Explorer faceExp = new TopExp_Explorer( shape, TopAbs_ShapeEnum.TopAbs_FACE );
			while( faceExp.More() ) {
				TopoDS_Face face = TopoDS.ToFace( faceExp.Current() );
				int startIndexOfFace = vertexList.Count / 3;

				// get triangulation data
				TopLoc_Location loc = new TopLoc_Location();
				Poly_Triangulation tri = BRep_Tool.Triangulation( face, ref loc );
				for( int i = 1; i <= tri.NbNodes(); i++ ) {
					gp_Pnt p = tri.Node( i );
					vertexList.Add( p.X() );
					vertexList.Add( p.Y() );
					vertexList.Add( p.Z() );
				}

				// the start vertex index of this face
				for( int i = 1; i <= tri.NbTriangles(); i++ ) {
					Poly_Triangle triangle = tri.Triangle( i );
					int index1 = 0;
					int index2 = 0;
					int index3 = 0;
					triangle.Get( ref index1, ref index2, ref index3 );
					indexList.Add( startIndexOfFace + index1 - 1 ); // convert to zero-based index
					indexList.Add( startIndexOfFace + index2 - 1 );
					indexList.Add( startIndexOfFace + index3 - 1 );
				}
				faceExp.Next();
			}
		}

		double[] ConvertTransform( gp_Trsf trsf )
		{
			gp_Mat matR = trsf.GetRotation().GetMatrix();
			gp_XYZ vecT = trsf.TranslationPart();
			double[] result = new double[ 12 ];

			// the rotation part
			result[ 0 ] = matR.Value( 1, 1 );
			result[ 1 ] = matR.Value( 1, 2 );
			result[ 2 ] = matR.Value( 1, 3 );
			result[ 3 ] = matR.Value( 2, 1 );
			result[ 4 ] = matR.Value( 2, 2 );
			result[ 5 ] = matR.Value( 2, 3 );
			result[ 6 ] = matR.Value( 3, 1 );
			result[ 7 ] = matR.Value( 3, 2 );
			result[ 8 ] = matR.Value( 3, 3 );

			// the translation part
			result[ 9 ] = vecT.X();
			result[ 10 ] = vecT.Y();
			result[ 11 ] = vecT.Z();
			return result;
		}

		// TODO: this is temporary foe testing
		void OnKeyDown( KeyEventArgs e )
		{
			// import
			if( e.Control && e.KeyCode == Keys.I ) {
				ImportMachine();
			}

			// build
			if( e.Control && e.KeyCode == Keys.B ) {
				BuildSimuData();
			}

			// refresh frame
			if( e.KeyCode == Keys.Down ) {
				m_CurrentFrameIndex += 2;
				RefreshFrame();
			}
			if( e.KeyCode == Keys.Up ) {
				m_CurrentFrameIndex -= 2;
				RefreshFrame();
			}

			// export NC
			if( e.Control && e.KeyCode == Keys.E ) {
				NCWriter writer = new NCWriter( m_CADManager.GetCAMDataList(), m_PostSolver );
				writer.Convert();
			}
		}
		int m_CurrentFrameIndex = 0;
	}
}
