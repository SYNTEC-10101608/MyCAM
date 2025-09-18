using MyCAM.Data;
using MyCAM.Post;
using OCC.AIS;
using OCC.BRepAlgoAPI;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Poly;
using OCC.Quantity;
using OCC.RWStl;
using OCC.TColStd;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class SimuEditor : EditorBase
	{
		public SimuEditor( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			// init frame transform map
			m_FrameTransformMap[ MachineComponentType.Base ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.XAxis ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.YAxis ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.ZAxis ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.Master ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.Slave ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.Tool ] = new List<gp_Trsf>();
			m_FrameTransformMap[ MachineComponentType.WorkPiece ] = new List<gp_Trsf>();

			// init frame collision map
			m_FrameCollisionMap[ MachineComponentType.Base ] = new List<bool>();
			m_FrameCollisionMap[ MachineComponentType.XAxis ] = new List<bool>();
			m_FrameCollisionMap[ MachineComponentType.YAxis ] = new List<bool>();
			m_FrameCollisionMap[ MachineComponentType.ZAxis ] = new List<bool>();
			m_FrameCollisionMap[ MachineComponentType.Master ] = new List<bool>();
			m_FrameCollisionMap[ MachineComponentType.Slave ] = new List<bool>();
			m_FrameCollisionMap[ MachineComponentType.Tool ] = new List<bool>();
			m_FrameCollisionMap[ MachineComponentType.WorkPiece ] = new List<bool>();
		}

		// simulation properties
		MachineData m_MachineData;
		PostSolver m_PostSolver;
		CollisionSolver m_FCLTest;

		// read from machine data
		HashSet<MachineComponentType> m_WorkPieceChainSet = new HashSet<MachineComponentType>();
		Dictionary<MachineComponentType, List<MachineComponentType>> m_ChainListMap = new Dictionary<MachineComponentType, List<MachineComponentType>>();
		Dictionary<MachineComponentType, AIS_InteractiveObject> m_MachineShapeMap = new Dictionary<MachineComponentType, AIS_InteractiveObject>();

		// calculated result
		Dictionary<MachineComponentType, List<gp_Trsf>> m_FrameTransformMap = new Dictionary<MachineComponentType, List<gp_Trsf>>();
		Dictionary<MachineComponentType, List<bool>> m_FrameCollisionMap = new Dictionary<MachineComponentType, List<bool>>();

		// frame control
		int m_FrameCount = 0;
		int m_CurrentFrameIndex = 0;

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
			m_Viewer.KeyDown += OnKeyDown;
		}

		public override void EditEnd()
		{
			m_Viewer.KeyDown -= OnKeyDown;
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
			if( m_DataManager.GetCAMDataList().Count == 0 || m_PostSolver == null ) {
				return;
			}
			foreach( CAMData camData in m_DataManager.GetCAMDataList() ) {
				gp_Vec G54Offset = new gp_Vec( 40, -385, -640 );
				m_PostSolver.G54Offset = G54Offset;
				if( PostHelper.SolvePath( m_PostSolver, camData, out PostData simuPostData, out _ ) == false ) {
					continue;
				}

				// connecting post points of all process paths
				List<PostPoint> currentPathPostPointList = PostHelper.GetConcatenatedPostList( simuPostData );

				// build frame by frame
				foreach( var postPoint in currentPathPostPointList ) {

					// set XYZ transform
					Dictionary<MachineComponentType, gp_Trsf> transformMap = new Dictionary<MachineComponentType, gp_Trsf>();
					gp_Trsf trsfX = new gp_Trsf();
					trsfX.SetTranslation( new gp_Vec( postPoint.X + G54Offset.X(), 0, 0 ) );
					if( m_WorkPieceChainSet.Contains( MachineComponentType.XAxis ) ) {
						trsfX.Invert();
					}
					transformMap[ MachineComponentType.XAxis ] = trsfX;

					gp_Trsf trsfY = new gp_Trsf();
					trsfY.SetTranslation( new gp_Vec( 0, postPoint.Y + G54Offset.Y(), 0 ) );
					if( m_WorkPieceChainSet.Contains( MachineComponentType.YAxis ) ) {
						trsfY.Invert();
					}
					transformMap[ MachineComponentType.YAxis ] = trsfY;

					gp_Trsf trsfZ = new gp_Trsf();
					trsfZ.SetTranslation( new gp_Vec( 0, 0, postPoint.Z + G54Offset.Z() ) );
					if( m_WorkPieceChainSet.Contains( MachineComponentType.ZAxis ) ) {
						trsfZ.Invert();
					}
					transformMap[ MachineComponentType.ZAxis ] = trsfZ;

					// set master rotation
					gp_Pnt ptOnMaster = m_MachineData.PtOnMaster;
					gp_Ax1 axisMaster = new gp_Ax1( ptOnMaster, m_MachineData.MasterRotateDir );
					gp_Trsf trsfMaster = new gp_Trsf();
					trsfMaster.SetRotation( axisMaster, postPoint.Master );
					if( m_WorkPieceChainSet.Contains( MachineComponentType.Master ) ) {
						trsfMaster.Invert();
					}
					transformMap[ MachineComponentType.Master ] = trsfMaster;

					// set slave rotation
					gp_Pnt ptOnSlave = m_MachineData.PtOnSlave;
					gp_Ax1 axisSlave = new gp_Ax1( ptOnSlave, m_MachineData.SlaveRotateDir );
					gp_Trsf trsfSlave = new gp_Trsf();
					trsfSlave.SetRotation( axisSlave, postPoint.Slave );
					if( m_WorkPieceChainSet.Contains( MachineComponentType.Slave ) ) {
						trsfSlave.Invert();
					}
					transformMap[ MachineComponentType.Slave ] = trsfSlave;

					// set tool and workpiece transform
					transformMap[ MachineComponentType.Tool ] = new gp_Trsf();
					gp_Trsf trsfWorkPiece = new gp_Trsf();
					trsfWorkPiece.SetTranslation( G54Offset );
					transformMap[ MachineComponentType.WorkPiece ] = trsfWorkPiece;

					// set chain
					gp_Trsf trsfAllX = GetComponentTrsf( transformMap, MachineComponentType.XAxis );
					gp_Trsf trsfAllY = GetComponentTrsf( transformMap, MachineComponentType.YAxis );
					gp_Trsf trsfAllZ = GetComponentTrsf( transformMap, MachineComponentType.ZAxis );
					gp_Trsf trsfAllMaster = GetComponentTrsf( transformMap, MachineComponentType.Master );
					gp_Trsf trsfAllSlave = GetComponentTrsf( transformMap, MachineComponentType.Slave );
					gp_Trsf trsAllfTool = GetComponentTrsf( transformMap, MachineComponentType.Tool );
					gp_Trsf trsfAllWorkPiece = GetComponentTrsf( transformMap, MachineComponentType.WorkPiece );
					m_FrameTransformMap[ MachineComponentType.XAxis ].Add( trsfAllX );
					m_FrameTransformMap[ MachineComponentType.YAxis ].Add( trsfAllY );
					m_FrameTransformMap[ MachineComponentType.ZAxis ].Add( trsfAllZ );
					m_FrameTransformMap[ MachineComponentType.Master ].Add( trsfAllMaster );
					m_FrameTransformMap[ MachineComponentType.Slave ].Add( trsfAllSlave );
					m_FrameTransformMap[ MachineComponentType.Tool ].Add( trsAllfTool );
					m_FrameTransformMap[ MachineComponentType.WorkPiece ].Add( trsfAllWorkPiece );

					// set collision
					HashSet<MachineComponentType> collisionResiltSet = new HashSet<MachineComponentType>();
					foreach( var compT in m_ChainListMap[ MachineComponentType.Tool ] ) {
						if( compT == MachineComponentType.Base ) {
							continue; // skip base
						}
						foreach( var compW in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
							if( compW == MachineComponentType.Base ) {
								continue; // skip base
							}
							if( m_FCLTest.CheckCollision( compT.ToString(), compW.ToString(),
								ConvertTransform( m_FrameTransformMap[ compT ].Last() ),
								ConvertTransform( m_FrameTransformMap[ compW ].Last() ) ) ) {
								collisionResiltSet.Add( compT );
								collisionResiltSet.Add( compW );
							}
						}
					}
					m_FrameCollisionMap[ MachineComponentType.XAxis ].Add( collisionResiltSet.Contains( MachineComponentType.XAxis ) );
					m_FrameCollisionMap[ MachineComponentType.YAxis ].Add( collisionResiltSet.Contains( MachineComponentType.YAxis ) );
					m_FrameCollisionMap[ MachineComponentType.ZAxis ].Add( collisionResiltSet.Contains( MachineComponentType.ZAxis ) );
					m_FrameCollisionMap[ MachineComponentType.Master ].Add( collisionResiltSet.Contains( MachineComponentType.Master ) );
					m_FrameCollisionMap[ MachineComponentType.Slave ].Add( collisionResiltSet.Contains( MachineComponentType.Slave ) );

					m_FrameCount++;
				}
			}
			// CAMData camData = m_CADManager.GetCAMDataList()[ 0 ];

		}

		gp_Trsf GetComponentTrsf( Dictionary<MachineComponentType, gp_Trsf> transformMap, MachineComponentType type )
		{
			gp_Trsf trsf = new gp_Trsf();
			foreach( var parent in m_ChainListMap[ type ] ) {
				if( parent == MachineComponentType.Base ) {
					continue; // base is not transformed
				}
				if( transformMap.ContainsKey( parent ) ) {
					trsf.Multiply( transformMap[ parent ] );
				}
			}
			trsf.Multiply( transformMap[ type ] );
			return trsf;
		}

		void RefreshFrame()
		{
			if( m_FrameCount == 0 ) {
				return;
			}
			if( m_CurrentFrameIndex < 0 ) {
				m_CurrentFrameIndex = m_FrameCount - 1;
			}
			if( m_CurrentFrameIndex >= m_FrameCount ) {
				m_CurrentFrameIndex = 0;
			}

			// refresh position
			m_MachineShapeMap[ MachineComponentType.XAxis ].SetLocalTransformation( m_FrameTransformMap[ MachineComponentType.XAxis ][ m_CurrentFrameIndex ] );
			m_MachineShapeMap[ MachineComponentType.YAxis ].SetLocalTransformation( m_FrameTransformMap[ MachineComponentType.YAxis ][ m_CurrentFrameIndex ] );
			m_MachineShapeMap[ MachineComponentType.ZAxis ].SetLocalTransformation( m_FrameTransformMap[ MachineComponentType.ZAxis ][ m_CurrentFrameIndex ] );
			m_MachineShapeMap[ MachineComponentType.Master ].SetLocalTransformation( m_FrameTransformMap[ MachineComponentType.Master ][ m_CurrentFrameIndex ] );
			m_MachineShapeMap[ MachineComponentType.Slave ].SetLocalTransformation( m_FrameTransformMap[ MachineComponentType.Slave ][ m_CurrentFrameIndex ] );
			m_MachineShapeMap[ MachineComponentType.Tool ].SetLocalTransformation( m_FrameTransformMap[ MachineComponentType.Tool ][ m_CurrentFrameIndex ] );
			m_MachineShapeMap[ MachineComponentType.WorkPiece ].SetLocalTransformation( m_FrameTransformMap[ MachineComponentType.WorkPiece ][ m_CurrentFrameIndex ] );

			// refresh color
			RefreshComponentColor( MachineComponentType.XAxis );
			RefreshComponentColor( MachineComponentType.YAxis );
			RefreshComponentColor( MachineComponentType.ZAxis );
			RefreshComponentColor( MachineComponentType.Master );
			RefreshComponentColor( MachineComponentType.Slave );
			m_Viewer.UpdateView();
		}

		void RefreshComponentColor( MachineComponentType type )
		{
			SetMeshColor( m_MachineShapeMap[ type ] as AIS_Triangulation, type, m_FrameCollisionMap[ type ][ m_CurrentFrameIndex ] );
			m_Viewer.GetAISContext().Redisplay( m_MachineShapeMap[ type ], false );
		}

		// TODO: read machine data from file
		void ReadMachineData( string szFolderName )
		{
			ReadMixTest();

			Poly_Triangulation shapeBase = RWStl.ReadFile( szFolderName + "\\Base.stl" );
			Poly_Triangulation shapeX = RWStl.ReadFile( szFolderName + "\\X.stl" );
			Poly_Triangulation shapeY = RWStl.ReadFile( szFolderName + "\\Y.stl" );
			Poly_Triangulation shapeZ = RWStl.ReadFile( szFolderName + "\\Z.stl" );
			Poly_Triangulation shapeMaster = RWStl.ReadFile( szFolderName + "\\Master.stl" );
			Poly_Triangulation shapeSlave = RWStl.ReadFile( szFolderName + "\\Slave.stl" );

			m_MachineShapeMap.Clear();
			AIS_Triangulation baseAIS = CreateMeshAIS( shapeBase, MachineComponentType.Base );
			m_Viewer.GetAISContext().Display( baseAIS, false );
			m_MachineShapeMap[ MachineComponentType.Base ] = baseAIS;
			AIS_Triangulation xAIS = CreateMeshAIS( shapeX, MachineComponentType.XAxis );
			m_Viewer.GetAISContext().Display( xAIS, false );
			m_MachineShapeMap[ MachineComponentType.XAxis ] = xAIS;
			AIS_Triangulation yAIS = CreateMeshAIS( shapeY, MachineComponentType.YAxis );
			m_Viewer.GetAISContext().Display( yAIS, false );
			m_MachineShapeMap[ MachineComponentType.YAxis ] = yAIS;
			AIS_Triangulation zAIS = CreateMeshAIS( shapeZ, MachineComponentType.ZAxis );
			m_Viewer.GetAISContext().Display( zAIS, false );
			m_MachineShapeMap[ MachineComponentType.ZAxis ] = zAIS;
			AIS_Triangulation masterAIS = CreateMeshAIS( shapeMaster, MachineComponentType.Master );
			m_Viewer.GetAISContext().Display( masterAIS, false );
			m_MachineShapeMap[ MachineComponentType.Master ] = masterAIS;
			AIS_Triangulation slaveAIS = CreateMeshAIS( shapeSlave, MachineComponentType.Slave );
			m_Viewer.GetAISContext().Display( slaveAIS, false );
			m_MachineShapeMap[ MachineComponentType.Slave ] = slaveAIS;

			m_FCLTest = new CollisionSolver();
			MeshShape( shapeX, out List<double> vertexListX, out List<int> indexListX );
			m_FCLTest.AddModel( MachineComponentType.XAxis.ToString(), indexListX.ToArray(), vertexListX.ToArray() );
			MeshShape( shapeY, out List<double> vertexListY, out List<int> indexListY );
			m_FCLTest.AddModel( MachineComponentType.YAxis.ToString(), indexListY.ToArray(), vertexListY.ToArray() );
			MeshShape( shapeZ, out List<double> vertexListZ, out List<int> indexListZ );
			m_FCLTest.AddModel( MachineComponentType.ZAxis.ToString(), indexListZ.ToArray(), vertexListZ.ToArray() );
			MeshShape( shapeMaster, out List<double> vertexListMaster, out List<int> indexListMaster );
			m_FCLTest.AddModel( MachineComponentType.Master.ToString(), indexListMaster.ToArray(), vertexListMaster.ToArray() );
			MeshShape( shapeSlave, out List<double> vertexListSlave, out List<int> indexListSlave );
			m_FCLTest.AddModel( MachineComponentType.Slave.ToString(), indexListSlave.ToArray(), vertexListSlave.ToArray() );

			// make tool
			BRepPrimAPI_MakeCylinder makeTool1 = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, -1 ) ), 0.2, 1 );
			BRepPrimAPI_MakeCylinder makeTool2 = new BRepPrimAPI_MakeCylinder( new gp_Ax2( new gp_Pnt( 0, 0, -1 ), new gp_Dir( 0, 0, -1 ) ), 0.2, 1 );
			BRepAlgoAPI_Fuse makeTool = new BRepAlgoAPI_Fuse( makeTool1.Shape(), makeTool2.Shape() );
			AIS_Shape toolAIS = new AIS_Shape( makeTool.Shape() );
			toolAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			toolAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_PURPLE ) );
			toolAIS.SetTransparency( 0.8f );
			m_MachineShapeMap[ MachineComponentType.Tool ] = toolAIS;
			m_Viewer.GetAISContext().Display( m_MachineShapeMap[ MachineComponentType.Tool ], false );

			// make workpiece
			m_MachineShapeMap[ MachineComponentType.WorkPiece ] = m_ViewManager.ViewObjectMap[ m_DataManager.PartIDList[ 0 ] ].AISHandle as AIS_Shape;
			m_Viewer.UpdateView();
		}

		AIS_Triangulation CreateMeshAIS( Poly_Triangulation mesh, MachineComponentType type )
		{
			AIS_Triangulation resultAIS = new AIS_Triangulation( mesh );

			// set material aspect, this matter since the default material gives wrong color effect
			Graphic3d_MaterialAspect baseAspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NameOfMaterial_UserDefined );
			resultAIS.SetMaterial( baseAspect );

			// set color
			SetMeshColor( resultAIS, type, false );
			return resultAIS;
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

		void ReadSpindleTest()
		{
			m_MachineData = new SpindleTypeMachineData();
			m_MachineData.ToolDirection = ToolDirection.Z;
			m_MachineData.MasterRotaryAxis = RotaryAxis.Z;
			m_MachineData.SlaveRotaryAxis = RotaryAxis.X;
			m_MachineData.MasterRotaryDirection = RotaryDirection.RightHand;
			m_MachineData.SlaveRotaryDirection = RotaryDirection.RightHand;
			m_MachineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			m_MachineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			m_MachineData.ToolLength = 2.0;
			( m_MachineData as SpindleTypeMachineData ).ToolToSlaveVec = new gp_Vec( -101.20, -0.19, 169.43 );
			( m_MachineData as SpindleTypeMachineData ).SlaveToMasterVec = new gp_Vec( -252.70, 0, 362.98 ) - ( m_MachineData as SpindleTypeMachineData ).ToolToSlaveVec;

			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );
			m_MachineData.RootNode.AddChild( XNode );
			XNode.AddChild( YNode );
			YNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			MasterNode.AddChild( SlaveNode );
			SlaveNode.AddChild( ToolNode );
			m_MachineData.RootNode.AddChild( WorkPieceNode );

			// build chain list
			m_ChainListMap.Clear();
			BuildChainList( m_MachineData.RootNode, new List<MachineComponentType>() );
			m_WorkPieceChainSet.Clear();
			foreach( var type in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				m_WorkPieceChainSet.Add( type );
			}

			// build solver
			m_PostSolver = new PostSolver( m_MachineData );
		}

		void ReadTableTest()
		{
			m_MachineData = new TableTypeMachineData();
			m_MachineData.ToolDirection = ToolDirection.Z;
			m_MachineData.MasterRotaryAxis = RotaryAxis.Y;
			m_MachineData.SlaveRotaryAxis = RotaryAxis.Z;
			m_MachineData.MasterRotaryDirection = RotaryDirection.LeftHand;
			m_MachineData.SlaveRotaryDirection = RotaryDirection.LeftHand;
			m_MachineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			m_MachineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			m_MachineData.ToolLength = 2.0;
			( m_MachineData as TableTypeMachineData ).MCSToMasterVec = new gp_Vec( -80.51, 73.81, -129.55 );
			( m_MachineData as TableTypeMachineData ).MasterToSlaveVec = new gp_Vec( -80.43, -71.67, -94.55 ) - ( m_MachineData as TableTypeMachineData ).MCSToMasterVec;

			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );
			m_MachineData.RootNode.AddChild( YNode );
			YNode.AddChild( XNode );
			XNode.AddChild( MasterNode );
			MasterNode.AddChild( SlaveNode );
			SlaveNode.AddChild( WorkPieceNode );
			m_MachineData.RootNode.AddChild( ZNode );
			ZNode.AddChild( ToolNode );

			// build chain list
			m_ChainListMap.Clear();
			BuildChainList( m_MachineData.RootNode, new List<MachineComponentType>() );
			m_WorkPieceChainSet.Clear();
			foreach( var type in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				m_WorkPieceChainSet.Add( type );
			}

			// build solver
			m_PostSolver = new PostSolver( m_MachineData );
		}

		void ReadMixTest()
		{
			m_MachineData = new MixTypeMachineData();
			m_MachineData.ToolDirection = ToolDirection.Z;
			m_MachineData.MasterRotaryAxis = RotaryAxis.Y;
			m_MachineData.SlaveRotaryAxis = RotaryAxis.Z;
			m_MachineData.MasterRotaryDirection = RotaryDirection.RightHand;
			m_MachineData.SlaveRotaryDirection = RotaryDirection.LeftHand;
			m_MachineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			m_MachineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			m_MachineData.ToolLength = 2.0;
			( m_MachineData as MixTypeMachineData ).ToolToMasterVec = new gp_Vec( 0, 101.2, 169.48 );
			( m_MachineData as MixTypeMachineData ).MCSToSlaveVec = new gp_Vec( 40.81, -384.80, -665.67 );

			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );
			m_MachineData.RootNode.AddChild( YNode );
			YNode.AddChild( SlaveNode );
			SlaveNode.AddChild( WorkPieceNode );
			m_MachineData.RootNode.AddChild( XNode );
			XNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			MasterNode.AddChild( ToolNode );

			// build chain list
			m_ChainListMap.Clear();
			BuildChainList( m_MachineData.RootNode, new List<MachineComponentType>() );
			m_WorkPieceChainSet.Clear();
			foreach( var type in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				m_WorkPieceChainSet.Add( type );
			}

			// build solver
			m_PostSolver = new PostSolver( m_MachineData );
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

		// TODO: this is temporary for testing
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
				m_CurrentFrameIndex += 1;
				RefreshFrame();
			}
			if( e.KeyCode == Keys.Up ) {
				m_CurrentFrameIndex -= 1;
				RefreshFrame();
			}

			// export NC
			if( e.Control && e.KeyCode == Keys.E ) {
				NCWriter writer = new NCWriter( m_DataManager.GetCAMDataList(), m_DataManager.MachineData );
				writer.Convert();
			}
		}
	}
}
