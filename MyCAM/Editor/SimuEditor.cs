using MyCAM.Data;
using MyCAM.Post;
using OCC.AIS;
using OCC.BRepAlgoAPI;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.Quantity;
using OCC.STEPControl;
using OCC.TopoDS;
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
