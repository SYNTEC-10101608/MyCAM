using MyCAM.Data;
using MyCAM.Machine;
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
		MachineData m_MachineData;
		IPostSolver m_PostSolver;
		List<PostData> m_MCSPostData;

		// editor
		public void EditStart()
		{
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
			CAMData camData = m_CADManager.GetCAMDataList()[ 0 ];
			m_PostSolver.Solve( camData, out _, out m_MCSPostData );
		}

		void RefreshFrame()
		{
			if( m_MCSPostData == null ) {
				return;
			}
			if( m_CurrentFrameIndex < 0 ) {
				m_CurrentFrameIndex = m_MCSPostData.Count - 1;
			}
			if( m_CurrentFrameIndex >= m_MCSPostData.Count ) {
				m_CurrentFrameIndex = 0;
			}

			double G54X = 1150;
			double G54Y = -11;
			double G54Z = -700;

			// gaet the post data
			PostData postData = m_MCSPostData[ m_CurrentFrameIndex ];

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
			SpindleTypeMachineData spindleTypeMachineData = m_MachineData as SpindleTypeMachineData;
			gp_Pnt ptOnSlave = new gp_Pnt();
			ptOnSlave.Translate( spindleTypeMachineData.ToolToSlaveVec );
			gp_Pnt ptOnMaster = ptOnSlave.Translated( spindleTypeMachineData.SlaveToMasterVec );
			gp_Ax1 axisMaster = new gp_Ax1( ptOnMaster, m_PostSolver.MasterRotateDir );
			gp_Trsf trsfMaster = new gp_Trsf();
			trsfMaster.SetRotation( axisMaster, postData.Master );
			if( m_WorkPieceChainSet.Contains( MachineComponentType.Master ) ) {
				trsfMaster.Invert();
			}
			transformMap[ MachineComponentType.Master ] = trsfMaster;

			// set slave rotation
			gp_Ax1 axisSlave = new gp_Ax1( ptOnSlave, m_PostSolver.SlaveRotateDir );
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
			gp_Pnt TestX = new gp_Pnt( 0, 0, 0 );
			TestX.Transform( trsfAllX );
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
			gp_Pnt TestY = new gp_Pnt( 0, 0, 0 );
			TestY.Transform( trsfAllY );
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
			gp_Pnt TestZ = new gp_Pnt( 0, 0, 0 );
			TestZ.Transform( trsfAllZ );
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
			trsfWorkPiece.SetTranslation( new gp_Vec( G54X, G54Y, G54Z ) );
			foreach( var parent in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				if( parent == MachineComponentType.Base ) {
					continue; // base is not transformed
				}
				if( transformMap.ContainsKey( parent ) ) {
					trsfWorkPiece.Multiply( transformMap[ parent ] );
				}
			}
			m_MachineShapeMap[ MachineComponentType.WorkPiece ].SetLocalTransformation( trsfWorkPiece );

			m_Viewer.UpdateView();
		}

		// TODO: read machine data from file
		void ReadMachineData( string szFolderName )
		{
			m_MachineData = new SpindleTypeMachineData();
			m_MachineData.ToolDirection = ToolDirection.Z;
			m_MachineData.MasterRotaryAxis = RotaryAxis.Z;
			m_MachineData.SlaveRotaryAxis = RotaryAxis.Y;
			m_MachineData.MasterRotaryDirection = RotaryDirection.RightHand;
			m_MachineData.SlaveRotaryDirection = RotaryDirection.RightHand;
			m_MachineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			m_MachineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			m_MachineData.ToolLength = 200.0;
			( (SpindleTypeMachineData)m_MachineData ).ToolToSlaveVec = new gp_Vec( 0, 0, 360 );
			( (SpindleTypeMachineData)m_MachineData ).SlaveToMasterVec = new gp_Vec( 0, 0, 0 );

			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );
			m_MachineData.RootNode.AddChild( YNode );
			YNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			MasterNode.AddChild( SlaveNode );
			SlaveNode.AddChild( ToolNode );
			m_MachineData.RootNode.AddChild( XNode );
			XNode.AddChild( WorkPieceNode );

			// build chain list
			m_ChainListMap.Clear();
			BuildChainList( m_MachineData.RootNode, new List<MachineComponentType>() );
			m_WorkPieceChainSet.Clear();
			foreach( var type in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				m_WorkPieceChainSet.Add( type );
			}

			// build solver
			m_PostSolver = new SpindlePostSolver( (SpindleTypeMachineData)m_MachineData );

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
			m_MachineShapeMap[ MachineComponentType.Base ] = new AIS_Shape( shapeBase );
			m_MachineShapeMap[ MachineComponentType.Base ].SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_MachineShapeMap[ MachineComponentType.Base ].SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
			m_Viewer.GetAISContext().Display( m_MachineShapeMap[ MachineComponentType.Base ], false );
			m_MachineShapeMap[ MachineComponentType.XAxis ] = new AIS_Shape( shapeX );
			m_MachineShapeMap[ MachineComponentType.XAxis ].SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_MachineShapeMap[ MachineComponentType.XAxis ].SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			m_Viewer.GetAISContext().Display( m_MachineShapeMap[ MachineComponentType.XAxis ], false );
			m_MachineShapeMap[ MachineComponentType.YAxis ] = new AIS_Shape( shapeY );
			m_MachineShapeMap[ MachineComponentType.YAxis ].SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_MachineShapeMap[ MachineComponentType.YAxis ].SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
			m_Viewer.GetAISContext().Display( m_MachineShapeMap[ MachineComponentType.YAxis ], false );
			m_MachineShapeMap[ MachineComponentType.ZAxis ] = new AIS_Shape( shapeZ );
			m_MachineShapeMap[ MachineComponentType.ZAxis ].SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_MachineShapeMap[ MachineComponentType.ZAxis ].SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			m_Viewer.GetAISContext().Display( m_MachineShapeMap[ MachineComponentType.ZAxis ], false );
			m_MachineShapeMap[ MachineComponentType.Master ] = new AIS_Shape( shapeMaster );
			m_MachineShapeMap[ MachineComponentType.Master ].SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_MachineShapeMap[ MachineComponentType.Master ].SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_YELLOW ) );
			m_Viewer.GetAISContext().Display( m_MachineShapeMap[ MachineComponentType.Master ], false );
			m_MachineShapeMap[ MachineComponentType.Slave ] = new AIS_Shape( shapeSlave );
			m_MachineShapeMap[ MachineComponentType.Slave ].SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_MachineShapeMap[ MachineComponentType.Slave ].SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_PURPLE ) );
			m_Viewer.GetAISContext().Display( m_MachineShapeMap[ MachineComponentType.Slave ], false );

			// make tool
			BRepPrimAPI_MakeCylinder makeTool1 = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, -1 ) ), 10, 195 );
			BRepPrimAPI_MakeCylinder makeTool2 = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, -195 ), new gp_Dir( 0, 0, -1 ) ), 1, 5 );
			BRepAlgoAPI_Fuse makeTool = new BRepAlgoAPI_Fuse( makeTool1.Shape(), makeTool2.Shape() );
			m_MachineShapeMap[ MachineComponentType.Tool ] = new AIS_Shape( makeTool.Shape() );
			m_MachineShapeMap[ MachineComponentType.Tool ].SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_MachineShapeMap[ MachineComponentType.Tool ].SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_PURPLE ) );
			m_Viewer.GetAISContext().Display( m_MachineShapeMap[ MachineComponentType.Tool ], false );

			// make workpiece
			m_MachineShapeMap[ MachineComponentType.WorkPiece ] = m_ViewManager.ViewObjectMap[ m_CADManager.PartIDList[ 0 ] ].AISHandle as AIS_Shape;

			m_Viewer.UpdateView();
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
			if( e.Control && e.KeyCode == Keys.Down ) {
				m_CurrentFrameIndex += 2;
				RefreshFrame();
			}
			if( e.Control && e.KeyCode == Keys.Up ) {
				m_CurrentFrameIndex -= 2;
				RefreshFrame();
			}
		}
		int m_CurrentFrameIndex = 0;
	}
}
