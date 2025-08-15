using MyCAM.Data;
using MyCAM.Machine;
using OCC.AIS;
using OCC.gp;
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

		void BuildSimuData()
		{
			// build G54 to MCS solver
		}

		// TODO: read machine data from file
		void ReadMachineData( string szFolderName )
		{
			SpindleTypeMachineData machineData = new SpindleTypeMachineData();
			machineData.ToolDirection = ToolDirection.Z;
			machineData.MasterRotaryAxis = RotaryAxis.Z;
			machineData.SlaveRotaryAxis = RotaryAxis.Y;
			machineData.MasterRotaryDirection = RotaryDirection.RightHand;
			machineData.SlaveRotaryDirection = RotaryDirection.RightHand;
			machineData.MasterTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.SlaveTiltedVec_deg = new gp_XYZ( 0, 0, 0 );
			machineData.ToolLength = 2.0;
			machineData.ToolToSlaveVec = new gp_Vec( 0, 0, 0 );
			machineData.SlaveToMasterVec = new gp_Vec( 0, 0, 0 );

			// build machine tree
			MachineTreeNode XNode = new MachineTreeNode( MachineComponentType.XAxis );
			MachineTreeNode YNode = new MachineTreeNode( MachineComponentType.YAxis );
			MachineTreeNode ZNode = new MachineTreeNode( MachineComponentType.ZAxis );
			MachineTreeNode MasterNode = new MachineTreeNode( MachineComponentType.Master );
			MachineTreeNode SlaveNode = new MachineTreeNode( MachineComponentType.Slave );
			MachineTreeNode ToolNode = new MachineTreeNode( MachineComponentType.Tool );
			MachineTreeNode WorkPieceNode = new MachineTreeNode( MachineComponentType.WorkPiece );
			machineData.RootNode.AddChild( YNode );
			YNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			MasterNode.AddChild( SlaveNode );
			SlaveNode.AddChild( ToolNode );
			machineData.RootNode.AddChild( XNode );
			XNode.AddChild( WorkPieceNode );
			m_ChainListMap.Clear();
			BuildChainList( machineData.RootNode, new List<MachineComponentType>() );
			m_WorkPieceChainSet.Clear();
			foreach( var type in m_ChainListMap[ MachineComponentType.WorkPiece ] ) {
				m_WorkPieceChainSet.Add( type );
			}

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
			m_MachineShapeMap[ MachineComponentType.XAxis ] = new AIS_Shape( shapeX );
			m_MachineShapeMap[ MachineComponentType.YAxis ] = new AIS_Shape( shapeY );
			m_MachineShapeMap[ MachineComponentType.ZAxis ] = new AIS_Shape( shapeZ );
			m_MachineShapeMap[ MachineComponentType.Master ] = new AIS_Shape( shapeMaster );
			m_MachineShapeMap[ MachineComponentType.Slave ] = new AIS_Shape( shapeSlave );

			// try display
			foreach( var pair in m_MachineShapeMap ) {
				m_Viewer.GetAISContext().Display( pair.Value, false );
			}
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
	}
}
