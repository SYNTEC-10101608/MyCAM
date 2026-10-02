using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MyCAM.Data
{
	[Serializable]
	public class MachineTreeNode
	{
		public MachineTreeNode( MachineComponentType type )
		{
			Type = type;
			Children = new List<MachineTreeNode>();
		}

		[XmlAttribute]
		public MachineComponentType Type
		{
			get; private set;
		}

		[XmlElement( "Child" )]
		public List<MachineTreeNode> Children
		{
			get; private set;
		}

		public void AddChild( MachineTreeNode child )
		{
			if( child != null ) {
				Children.Add( child );
			}
		}
	}

	// the rotary axis information of a five-axis machine, derived from FiveAxisMachineData
	public class RotaryAxisConfig
	{
		// the one can be flipped by +-180
		public string RotaryAxisName
		{
			get; set;
		}

		// mix type specialization, MasterOnly / SlaveOnly means single rotary axis machine
		public MixTypeVariant Variant
		{
			get; set;
		} = MixTypeVariant.Mix;

		public bool IsSingleAxis
		{
			get
			{
				return Variant != MixTypeVariant.Mix;
			}
		}

		public string MasterName
		{
			get; set;
		}

		public string SlaveName
		{
			get; set;
		}

		public RotaryAxis RotaryAxisType
		{
			get; set;
		}
	}

	// abstract base holding only shared machine info across products (five-axis / articulated robot)
	public abstract class IMachineDataBase
	{
		protected IMachineDataBase()
		{
			RootNode = new MachineTreeNode( MachineComponentType.Base );

			// unnessary value, just for initialization to avoid null reference exception
			SimulationOffset = new gp_Vec( 0, 0, 0 );

			// default: do not run whole-machine simulation
			FullMachineSimulation = false;

			// chain structures, filled by chain builder
			ChainListMap = new Dictionary<MachineComponentType, List<MachineComponentType>>();
			WorkPieceChain = new HashSet<MachineComponentType>();

			// per component meshes loaded from stl, not serialized
			MachineMeshes = new MachineMeshes();
		}

		public abstract Product Product
		{
			get;
		}

		// all components of this machine
		public abstract IReadOnlyList<MachineComponentType> ComponentTypes
		{
			get;
		}

		// the end of tool side
		public abstract MachineComponentType ToolChainLeaf
		{
			get;
		}

		//  the end of workpiece side
		public abstract MachineComponentType WorkPieceChainLeaf
		{
			get;
		}

		// component -> stl base file name ( without extension ) used to load machine meshes
		public abstract IReadOnlyDictionary<MachineComponentType, string> MeshFileNameMap
		{
			get;
		}

		public gp_Vec SimulationOffset
		{
			get; set;
		}

		// per component meshes loaded from stl, runtime resource ( not serialized / not in DTO )
		[XmlIgnore]
		public MachineMeshes MachineMeshes
		{
			get; set;
		}

		// whether to build default machine tree or
		public bool FullMachineSimulation
		{
			get; set;
		}

		public double ToolLength
		{
			get; set;
		}

		public MachineTreeNode RootNode
		{
			get; internal set;
		}

		// full parent chain ( Base -> self ) for each machine component, built from RootNode
		public Dictionary<MachineComponentType, List<MachineComponentType>> ChainListMap
		{
			get; internal set;
		}

		// parent chain set of the workpiece component, built from RootNode
		public HashSet<MachineComponentType> WorkPieceChain
		{
			get; internal set;
		}

		// build the product-specific default machine tree ( used for partial simulation )
		public abstract MachineTreeNode BuildDefaultTree();

		// whether the chain builder should replace RootNode with the default tree
		public virtual bool NeedBuildDefaultTree()
		{
			return RootNode == null || RootNode.Children.Count == 0;
		}

		// build a MachineTreeNode tree from a tree definition
		protected static MachineTreeNode BuildTree( SimuData.TreeData.MachineTreeDef treeDefinition )
		{
			MachineTreeNode node = new MachineTreeNode( treeDefinition.Type );
			if( treeDefinition.Children != null ) {
				foreach( var child in treeDefinition.Children ) {
					node.AddChild( BuildTree( child ) );
				}
			}
			return node;
		}
	}

	public abstract class FiveAxisMachineData : IMachineDataBase
	{
		public sealed override Product Product
		{
			get
			{
				return Product.FiveAxis;
			}
		}

		public sealed override IReadOnlyList<MachineComponentType> ComponentTypes
		{
			get
			{
				return s_ComponentTypes;
			}
		}

		// five-axis tool side chain ends at the laser component
		public sealed override MachineComponentType ToolChainLeaf
		{
			get
			{
				return MachineComponentType.Laser;
			}
		}

		// five-axis workpiece side chain ends at the workpiece component
		public sealed override MachineComponentType WorkPieceChainLeaf
		{
			get
			{
				return MachineComponentType.WorkPiece;
			}
		}

		// five-axis component -> stl base file name
		public sealed override IReadOnlyDictionary<MachineComponentType, string> MeshFileNameMap
		{
			get
			{
				return s_MeshFileNameMap;
			}
		}

		public abstract FiveAxisType FiveAxisType
		{
			get;
		}

		public ToolDirection ToolDirection
		{
			get; set;
		}

		// to show A/B/C axis in the UI or NC file
		public RotaryAxis MasterRotaryAxis
		{
			get; set;
		}

		public RotaryAxis SlaveRotaryAxis
		{
			get; set;
		}

		// the tube axial direction, falls back to the rotary axis when the machine file does not assign it
		public RotaryAxis TubeDir
		{
			get; set;
		} = RotaryAxis.Z;

		// the rotary axis naming info used by the UI and the NC file
		public RotaryAxisConfig RotaryAxisConfig
		{
			get
			{
				RotaryAxis rotaryAxis = RotaryAxisType;

				// single rotary axis machine is a specialized mix type
				MixTypeVariant variant = MixTypeVariant.Mix;
				if( this is MixTypeMachineData mixTypeMachineData ) {
					variant = mixTypeMachineData.MixTypeVariant;
				}
				return new RotaryAxisConfig
				{
					MasterName = RotaryAxisHelper.ConvertRotaryAxisName( MasterRotaryAxis ),
					SlaveName = RotaryAxisHelper.ConvertRotaryAxisName( SlaveRotaryAxis ),
					RotaryAxisType = rotaryAxis,
					RotaryAxisName = RotaryAxisHelper.ConvertRotaryAxisName( rotaryAxis ),
					Variant = variant
				};
			}
		}

		public RotateAxisDef MasterAxis
		{
			get; set;
		}

		public RotateAxisDef SlaveAxis
		{
			get; set;
		}

		public gp_XYZ MasterTiltedVec_deg
		{
			get; set;
		}

		public gp_XYZ SlaveTiltedVec_deg
		{
			get; set;
		}

		public gp_Dir MasterRotateDir
		{
			get
			{
				return MasterAxis != null ? MasterAxis.RotateDir : RotateAxisDef.RotaryAxisToDir( MasterRotaryAxis );
			}
		}

		public gp_Dir SlaveRotateDir
		{
			get
			{
				return SlaveAxis != null ? SlaveAxis.RotateDir : RotateAxisDef.RotaryAxisToDir( SlaveRotaryAxis );
			}
		}

		public gp_Dir ToolDir
		{
			get
			{
				switch( ToolDirection ) {
					case ToolDirection.X:
						return new gp_Dir( 1, 0, 0 );
					case ToolDirection.Y:
						return new gp_Dir( 0, 1, 0 );
					case ToolDirection.Z:
						return new gp_Dir( 0, 0, 1 );
					default:
						return new gp_Dir( 0, 0, 1 );
				}
			}
		}

		// build MasterAxis / SlaveAxis from the rotary axis / direction / limit info,
		public void InitRotateAxes(
			RotaryDirection masterRotaryDirection, double masterStart_deg, double masterEnd_deg,
			RotaryDirection slaveRotaryDirection, double slaveStart_deg, double slaveEnd_deg )
		{
			MasterAxis = new RotateAxisDef(
				GetMasterRotateCenter(), RotateAxisDef.RotaryAxisToDir( MasterRotaryAxis ),
				masterStart_deg, masterEnd_deg, masterRotaryDirection );
			SlaveAxis = new RotateAxisDef(
				GetSlaveRotateCenter(), RotateAxisDef.RotaryAxisToDir( SlaveRotaryAxis ),
				slaveStart_deg, slaveEnd_deg, slaveRotaryDirection );
		}

		// file do not assign TubeDir, use the rotary axis as the tube direction
		public void InitTubeDir()
		{
			if( TubeDir == RotaryAxis.None ) {
				TubeDir = RotaryAxisType;
			}
		}

		public override bool NeedBuildDefaultTree()
		{
			// partial simulation always uses the default tree
			if( FullMachineSimulation == false ) {
				return true;
			}

			// root must be the Base node
			if( RootNode == null || RootNode.Type != MachineComponentType.Base ) {
				return true;
			}

			// base must have exactly two branches ( tool side / workpiece side )
			if( RootNode.Children.Count != 2 ) {
				return true;
			}

			// each branch must be a single non-branching chain ending at its expected leaf
			MachineComponentType leaf0 = GetSingleChainLeaf( RootNode.Children[ 0 ] );
			MachineComponentType leaf1 = GetSingleChainLeaf( RootNode.Children[ 1 ] );

			// the two leaves must be Laser ( tool ) and WorkPiece, order independent
			bool isValid = ( leaf0 == ToolChainLeaf && leaf1 == WorkPieceChainLeaf )
				|| ( leaf0 == WorkPieceChainLeaf && leaf1 == ToolChainLeaf );
			return isValid == false;
		}

		// build the default tree based on the five-axis machine type
		public override MachineTreeNode BuildDefaultTree()
		{
			switch( FiveAxisType ) {
				case FiveAxisType.Table:
					return BuildTree( SimuData.TreeData.DefaultTableTreeDef );
				case FiveAxisType.Mix:
					return BuildTree( SimuData.TreeData.DefaultMixTreeDef );
				case FiveAxisType.Spindle:
				default:
					return BuildTree( SimuData.TreeData.DefaultSpindleTreeDef );
			}
		}

		// compute the rotate center used to init MasterAxis / SlaveAxis ( from each machine type's own vectors )
		protected abstract gp_Pnt GetMasterRotateCenter();

		protected abstract gp_Pnt GetSlaveRotateCenter();

		// walk a single non-branching chain and return its end leaf type
		MachineComponentType GetSingleChainLeaf( MachineTreeNode node )
		{
			MachineComponentType leaf = MachineComponentType.UnKnow;
			while( node != null ) {
				leaf = node.Type;

				// every node type must be a known machine component
				if( ComponentTypes.Contains( node.Type ) == false ) {
					break;
				}

				// no child means we reached the leaf
				if( node.Children.Count == 0 ) {
					break;
				}

				// branching is not allowed
				if( node.Children.Count > 1 ) {
					return MachineComponentType.UnKnow;
				}
				node = node.Children[ 0 ];
			}
			return leaf;
		}

		// the axis that can be flipped by +-180, table and mix type are driven by the slave one
		RotaryAxis RotaryAxisType
		{
			get
			{
				return ( FiveAxisType == FiveAxisType.Table || FiveAxisType == FiveAxisType.Mix )
					? SlaveRotaryAxis
					: MasterRotaryAxis;
			}
		}

		// all component ( Base excluded )
		static readonly MachineComponentType[] s_ComponentTypes = new[]
		{
			MachineComponentType.XAxis,
			MachineComponentType.YAxis,
			MachineComponentType.ZAxis,
			MachineComponentType.Master,
			MachineComponentType.Slave,
			MachineComponentType.Laser,
			MachineComponentType.WorkPiece,
		};

		// component -> stl base file name ( Laser / WorkPiece have no mesh file )
		static readonly IReadOnlyDictionary<MachineComponentType, string> s_MeshFileNameMap = new Dictionary<MachineComponentType, string>
		{
			{ MachineComponentType.Base, "Base" },
			{ MachineComponentType.XAxis, "X" },
			{ MachineComponentType.YAxis, "Y" },
			{ MachineComponentType.ZAxis, "Z" },
			{ MachineComponentType.Master, "Master" },
			{ MachineComponentType.Slave, "Slave" },
		};
	}

	public class SpindleTypeMachineData : FiveAxisMachineData
	{
		public override FiveAxisType FiveAxisType
		{
			get
			{
				return FiveAxisType.Spindle;
			}
		}

		public gp_Vec ToolToSlaveVec
		{
			get; set;
		}

		public gp_Vec SlaveToMasterVec
		{
			get; set;
		}

		protected override gp_Pnt GetMasterRotateCenter()
		{
			gp_Pnt MCSPt = new gp_Pnt( 0, 0, 0 );
			return MCSPt.Translated( ToolToSlaveVec + SlaveToMasterVec );
		}

		protected override gp_Pnt GetSlaveRotateCenter()
		{
			gp_Pnt MCSPt = new gp_Pnt( 0, 0, 0 );
			return MCSPt.Translated( ToolToSlaveVec );
		}
	}

	public class TableTypeMachineData : FiveAxisMachineData
	{
		public override FiveAxisType FiveAxisType
		{
			get
			{
				return FiveAxisType.Table;
			}
		}

		public gp_Vec MasterToSlaveVec
		{
			get; set;
		}

		public gp_Vec MCSToMasterVec
		{
			get; set;
		}

		protected override gp_Pnt GetMasterRotateCenter()
		{
			gp_Pnt MCSPt = new gp_Pnt( 0, 0, 0 );
			return MCSPt.Translated( MCSToMasterVec );
		}

		protected override gp_Pnt GetSlaveRotateCenter()
		{
			gp_Pnt MCSPt = new gp_Pnt( 0, 0, 0 );
			return MCSPt.Translated( MCSToMasterVec + MasterToSlaveVec );
		}
	}

	public class MixTypeMachineData : FiveAxisMachineData
	{
		public override FiveAxisType FiveAxisType
		{
			get
			{
				return FiveAxisType.Mix;
			}
		}

		// mix type specialization, MasterOnly / SlaveOnly means single rotary axis machine
		public MixTypeVariant MixTypeVariant
		{
			get; set;
		} = MixTypeVariant.Mix;

		public gp_Vec ToolToMasterVec
		{
			get; set;
		}

		public gp_Vec MCSToSlaveVec
		{
			get; set;
		}

		protected override gp_Pnt GetMasterRotateCenter()
		{
			gp_Pnt MCSPt = new gp_Pnt( 0, 0, 0 );
			return MCSPt.Translated( ToolToMasterVec );
		}

		protected override gp_Pnt GetSlaveRotateCenter()
		{
			gp_Pnt MCSPt = new gp_Pnt( 0, 0, 0 );
			return MCSPt.Translated( MCSToSlaveVec );
		}
	}

	// articulated robot machine data, holds 6 joints and end-effector / tool frames
	public class ArticRobotMachineData : IMachineDataBase
	{
		public ArticRobotMachineData()
		{
			// unnessary value, initialize to avoid null reference exception
			FlangeFrame = new gp_Ax3( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );
			ToolFrame = new gp_Ax3( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );
		}

		public const int JOINT_COUNT = 6;

		public override Product Product
		{
			get
			{
				return Product.ArticRobot;
			}
		}

		// articulated robot always uses the default tree ( Base -> Joint1 ~ Joint6 + WorkPiece )
		public override bool NeedBuildDefaultTree()
		{
			return true;
		}

		// build the default articulated robot tree: tool side Base -> Joint1 ~ Joint6, workpiece side Base -> WorkPiece
		public override MachineTreeNode BuildDefaultTree()
		{
			MachineTreeNode baseNode = new MachineTreeNode( MachineComponentType.Base );

			// tool side: Base -> Joint1 -> Joint2 -> ... -> Joint6
			MachineTreeNode parent = baseNode;
			for( int i = 0; i < JOINT_COUNT; i++ ) {
				MachineComponentType jointType = MachineComponentType.Joint1 + i;
				MachineTreeNode jointNode = new MachineTreeNode( jointType );
				parent.AddChild( jointNode );
				parent = jointNode;
			}

			// workpiece side: Base -> WorkPiece
			baseNode.AddChild( new MachineTreeNode( MachineComponentType.WorkPiece ) );
			return baseNode;
		}

		// articulated robot simulated component order: Joint1 ~ Joint6 + WorkPiece ( Base excluded )
		public override IReadOnlyList<MachineComponentType> ComponentTypes
		{
			get
			{
				return s_ComponentTypes;
			}
		}

		// articulated robot tool side chain ends at the last joint ( carries the tool )
		public override MachineComponentType ToolChainLeaf
		{
			get
			{
				return MachineComponentType.Joint1 + ( JOINT_COUNT - 1 );
			}
		}

		// articulated robot workpiece side chain ends at the workpiece component
		public override MachineComponentType WorkPieceChainLeaf
		{
			get
			{
				return MachineComponentType.WorkPiece;
			}
		}

		// articulated robot component -> stl base file name ( Joint1 ~ Joint6 + Base )
		public override IReadOnlyDictionary<MachineComponentType, string> MeshFileNameMap
		{
			get
			{
				return s_MeshFileNameMap;
			}
		}

		// 6 rotary joints, each carries a RotateAxisDef ( rotate center / direction / limit )
		public IReadOnlyList<ArticRobotComponentInfo> Joints
		{
			get
			{
				return m_Joints;
			}
		}

		// end-effector ( flange ) coordinate frame
		public gp_Ax3 FlangeFrame
		{
			get; set;
		}

		// tool coordinate frame relative to flange
		public gp_Ax3 ToolFrame
		{
			get; set;
		}

		// all component ( Base excluded )
		static readonly MachineComponentType[] s_ComponentTypes = new[]
		{
			MachineComponentType.Joint1,
			MachineComponentType.Joint2,
			MachineComponentType.Joint3,
			MachineComponentType.Joint4,
			MachineComponentType.Joint5,
			MachineComponentType.Joint6,
			MachineComponentType.WorkPiece,
			MachineComponentType.T0_Flange,
			MachineComponentType.T1_ToolFrame
		};

		// component -> stl base file name ( WorkPiece has no mesh file )
		static readonly IReadOnlyDictionary<MachineComponentType, string> s_MeshFileNameMap = new Dictionary<MachineComponentType, string>
		{
			{ MachineComponentType.Base, "Base" },
			{ MachineComponentType.Joint1, "Joint1" },
			{ MachineComponentType.Joint2, "Joint2" },
			{ MachineComponentType.Joint3, "Joint3" },
			{ MachineComponentType.Joint4, "Joint4" },
			{ MachineComponentType.Joint5, "Joint5" },
			{ MachineComponentType.Joint6, "Joint6" },
		};

		List<ArticRobotComponentInfo> m_Joints = new List<ArticRobotComponentInfo>();
	}
}
