using OCC.gp;
using System.Collections.Generic;

namespace MyCAM.Data
{
	internal class MachineTreeNode
	{
		public MachineTreeNode( MachineComponentType type )
		{
			Type = type;
			Children = new List<MachineTreeNode>();
		}

		public MachineComponentType Type
		{
			get; private set;
		}

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

	internal abstract class MachineData
	{
		protected MachineData()
		{
			RootNode = new MachineTreeNode( MachineComponentType.Base );
		}

		public abstract FiveAxisType FiveAxisType
		{
			get;
		}

		public ToolDirection ToolDirection
		{
			get; set;
		}

		public RotaryAxis MasterRotaryAxis
		{
			get; set;
		}

		public RotaryAxis SlaveRotaryAxis
		{
			get; set;
		}

		public RotaryDirection MasterRotaryDirection
		{
			get; set;
		}

		public RotaryDirection SlaveRotaryDirection
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

		public double ToolLength
		{
			get; set;
		}

		public MachineTreeNode RootNode
		{
			get; private set;
		}

		public gp_Dir MasterRotateDir
		{
			get
			{
				switch( MasterRotaryAxis ) {
					case RotaryAxis.X:
						return new gp_Dir( 1, 0, 0 );
					case RotaryAxis.Y:
						return new gp_Dir( 0, 1, 0 );
					case RotaryAxis.Z:
						return new gp_Dir( 0, 0, 1 );
					default:
						return new gp_Dir( 0, 0, 1 );
				}
			}
		}

		public gp_Dir SlaveRotateDir
		{
			get
			{
				switch( SlaveRotaryAxis ) {
					case RotaryAxis.X:
						return new gp_Dir( 1, 0, 0 );
					case RotaryAxis.Y:
						return new gp_Dir( 0, 1, 0 );
					case RotaryAxis.Z:
						return new gp_Dir( 0, 0, 1 );
					default:
						return new gp_Dir( 0, 1, 0 );
				}
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
	}

	internal class SpindleTypeMachineData : MachineData
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
	}

	internal class TableTypeMachineData : MachineData
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
	}

	internal class MixTypeMachineData : MachineData
	{
		public override FiveAxisType FiveAxisType
		{
			get
			{
				return FiveAxisType.Mix;
			}
		}

		public gp_Vec ToolToMasterVec
		{
			get; set;
		}

		public gp_Vec MCSToSlaveVec
		{
			get; set;
		}
	}
}
