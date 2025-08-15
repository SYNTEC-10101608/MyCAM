using MyCAM.Data;
using MyCAM.Machine;
using OCC.AIS;
using OCC.gp;
using OCC.StlAPI;
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
			machineData.RootNode.AddChild( XNode );
			machineData.RootNode.AddChild( YNode );
			XNode.AddChild( ZNode );
			ZNode.AddChild( MasterNode );
			ZNode.AddChild( SlaveNode );
			SlaveNode.AddChild( ToolNode );
			YNode.AddChild( WorkPieceNode );
			m_WorkPieceChainSet.Clear();
			BuildWorkPieceChain( machineData.RootNode );

			// TODO: we can read any type of 3D file, including stl
			TopoDS_Shape shapeBase = new TopoDS_Shape();
			TopoDS_Shape shapeX = new TopoDS_Shape();
			TopoDS_Shape shapeY = new TopoDS_Shape();
			TopoDS_Shape shapeZ = new TopoDS_Shape();
			TopoDS_Shape shapeMaster = new TopoDS_Shape();
			TopoDS_Shape shapeSlave = new TopoDS_Shape();
			StlAPI_Reader reader = new StlAPI_Reader();
			reader.Read( ref shapeBase, szFolderName + "Base.stl" );
			reader.Read( ref shapeX, szFolderName + "X.stl" );
			reader.Read( ref shapeY, szFolderName + "Y.stl" );
			reader.Read( ref shapeZ, szFolderName + "Z.stl" );
			reader.Read( ref shapeMaster, szFolderName + "Master.stl" );
			reader.Read( ref shapeSlave, szFolderName + "Slave.stl" );
		}

		bool BuildWorkPieceChain( MachineTreeNode root )
		{
			if( root == null ) {
				return false;
			}
			if( root.Type == MachineComponentType.WorkPiece ) {
				return true;
			}
			foreach( MachineTreeNode child in root.Children ) {
				if( BuildWorkPieceChain( child ) ) {
					m_WorkPieceChainSet.Add( root.Type );
					return true;
				}
			}
			return false;
		}

		void BuildChainList( MachineTreeNode root, List<MachineComponentType> chainList )
		{
			if( root == null ) {
				return;
			}
			m_ChainListMap[ root.Type ] = chainList;
			foreach( MachineTreeNode child in root.Children ) {
				BuildChainList( child, new List<MachineComponentType>( chainList ) { child.Type } );
			}
		}
	}
}
