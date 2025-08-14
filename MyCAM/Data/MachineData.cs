using MyCAM.Machine;
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

	internal class MachineData
	{
		public MachineData()
		{
			RootNode = new MachineTreeNode( MachineComponentType.Base );
		}

		public virtual FiveAxisType FiveAxisType
		{
			get
			{
				return FiveAxisType.None;
			}
		}

		public ToolDirection ToolDirection
		{
			get; set;
		}

		public RotaryAxis RotaryAxis
		{
			get; set;
		}

		public RotaryDirection RotaryDirection
		{
			get; set;
		}

		public MachineTreeNode RootNode
		{
			get; private set;
		}

		public gp_XYZ MasterTiltedVec_deg
		{
			get; set;
		}

		public gp_XYZ SlaveTiltedVec_deg
		{
			get; set;
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
