using MyCAM.Data;
using OCC.gp;
using System;
using System.Xml.Serialization;

namespace MyCAM.FileManager
{
	[Serializable]
	[XmlRoot( "MachineDataDTO" )]
	public class MachineDataDTOContainer
	{
		[XmlElement( "SpindleTypeMachineDataDTO", typeof( SpindleTypeMachineDataDTO ) )]
		[XmlElement( "TableTypeMachineDataDTO", typeof( TableTypeMachineDataDTO ) )]
		[XmlElement( "MixTypeMachineDataDTO", typeof( MixTypeMachineDataDTO ) )]
		public MachineDataDTO MachineDataDTO
		{
			get; set;
		} = new MachineDataDTO();

		public MachineDataDTOContainer()
		{
		}
	}

	public static class MachineDataDTOManager
	{
		internal static MachineDataDTOContainer ToDTOContainer( FiveAxisMachineData machineData )
		{
			if( machineData == null ) {
				return new MachineDataDTOContainer();
			}
			MachineDataDTO machinDataDTO = null;
			switch( machineData.FiveAxisType ) {
				case FiveAxisType.Spindle:
					machinDataDTO = new SpindleTypeMachineDataDTO( (SpindleTypeMachineData)machineData );
					break;
				case FiveAxisType.Table:
					machinDataDTO = new TableTypeMachineDataDTO( (TableTypeMachineData)machineData );
					break;
				case FiveAxisType.Mix:
					machinDataDTO = new MixTypeMachineDataDTO( (MixTypeMachineData)machineData );
					break;
				default:
					machinDataDTO = new MachineDataDTO( machineData );
					break;
			}
			return new MachineDataDTOContainer { MachineDataDTO = machinDataDTO };
		}

		internal static FiveAxisMachineData ToMachineData( MachineDataDTOContainer container )
		{
			if( container == null || container.MachineDataDTO == null ) {
				throw new ArgumentNullException( "MachineDataDTOConatiner deserialization failed." );
			}
			switch( container.MachineDataDTO ) {
				case SpindleTypeMachineDataDTO spindleDTO:
					return spindleDTO.ToSpindleTypeMachineData();
				case TableTypeMachineDataDTO tableDTO:
					return tableDTO.ToTableTypeMachineData();
				case MixTypeMachineDataDTO mixDTO:
					return mixDTO.ToMixTypeMachineData();
				default:
					throw new ArgumentException( "Unsupported machine configuration." );
			}
		}
	}

	#region DTO

	public class ExternalAxisMachineDataDTO
	{
		public ExternalAxisType Type
		{
			get; set;
		}

		public RotaryAxis Ext1RotaryAxis
		{
			get; set;
		}

		public RotaryAxis Ext2RotaryAxis
		{
			get; set;
		}

		public gp_Vec_DTO MCSToExt1Vec
		{
			get; set;
		} = new gp_Vec_DTO();

		public gp_Vec_DTO Ext1ToExt2Vec
		{
			get; set;
		} = new gp_Vec_DTO();

		public gp_XYZDTO Ext1TiltedVec_deg
		{
			get; set;
		} = new gp_XYZDTO();

		public gp_XYZDTO Ext2TiltedVec_deg
		{
			get; set;
		} = new gp_XYZDTO();

		public RotaryDirection Ext1RotaryDirection
		{
			get; set;
		}

		public RotaryDirection Ext2RotaryDirection
		{
			get; set;
		}

		public double Ext1Start_deg
		{
			get; set;
		}

		public double Ext1End_deg
		{
			get; set;
		}

		public double Ext2Start_deg
		{
			get; set;
		}

		public double Ext2End_deg
		{
			get; set;
		}

		public ExternalAxisMachineDataDTO()
		{
		}

		internal ExternalAxisMachineDataDTO( ExternalAxisMachineData data )
		{
			Type = data.Type;
			Ext1RotaryAxis = data.Ext1RotaryAxis;
			Ext2RotaryAxis = data.Ext2RotaryAxis;
			MCSToExt1Vec = new gp_Vec_DTO( data.MCSToExt1Vec );
			Ext1ToExt2Vec = new gp_Vec_DTO( data.Ext1ToExt2Vec );
			Ext1TiltedVec_deg = new gp_XYZDTO( data.Ext1TiltedVec_deg );
			Ext2TiltedVec_deg = new gp_XYZDTO( data.Ext2TiltedVec_deg );
			Ext1RotaryDirection = data.Ext1RotaryDirection;
			Ext2RotaryDirection = data.Ext2RotaryDirection;
			Ext1Start_deg = data.Ext1Start_deg;
			Ext1End_deg = data.Ext1End_deg;
			Ext2Start_deg = data.Ext2Start_deg;
			Ext2End_deg = data.Ext2End_deg;
		}

		internal ExternalAxisMachineData ToMachineData()
		{
			return new ExternalAxisMachineData
			{
				Type = Type,
				Ext1RotaryAxis = Ext1RotaryAxis,
				Ext2RotaryAxis = Ext2RotaryAxis,
				MCSToExt1Vec = MCSToExt1Vec.ToVec(),
				Ext1ToExt2Vec = Ext1ToExt2Vec.ToVec(),
				Ext1TiltedVec_deg = Ext1TiltedVec_deg.ToXYZ(),
				Ext2TiltedVec_deg = Ext2TiltedVec_deg.ToXYZ(),
				Ext1RotaryDirection = Ext1RotaryDirection,
				Ext2RotaryDirection = Ext2RotaryDirection,
				Ext1Start_deg = Ext1Start_deg,
				Ext1End_deg = Ext1End_deg,
				Ext2Start_deg = Ext2Start_deg,
				Ext2End_deg = Ext2End_deg
			};
		}
	}

	public class MachineDataDTO
	{
		public ExternalAxisMachineDataDTO ExternalAxes
		{
			get; set;
		} = new ExternalAxisMachineDataDTO();

		public ToolDirection ToolDirection
		{
			get; set;
		} = new ToolDirection();

		public RotaryAxis MasterRotaryAxis
		{
			get; set;
		} = new RotaryAxis();

		public RotaryAxis SlaveRotaryAxis
		{
			get; set;
		} = new RotaryAxis();

		// None means the machine file does not assign it, the rotary axis is used instead
		public RotaryAxis TubeDir
		{
			get; set;
		} = RotaryAxis.None;

		public RotaryDirection MasterRotaryDirection
		{
			get; set;
		} = new RotaryDirection();

		public RotaryDirection SlaveRotaryDirection
		{
			get; set;
		} = new RotaryDirection();

		public gp_XYZDTO MasterTiltedVec_deg
		{
			get; set;
		} = new gp_XYZDTO();

		public gp_XYZDTO SlaveTiltedVec_deg
		{
			get; set;
		} = new gp_XYZDTO();

		public gp_Vec_DTO SimulationOffset
		{
			get; set;
		} = new gp_Vec_DTO();

		public bool FullMachineSimulation
		{
			get; set;
		}

		public double ToolLength
		{
			get; set;
		}

		public double MasterAxisStart_deg
		{
			get; set;
		}

		public double MasterAxisEnd_deg
		{
			get; set;
		}

		public double SlaveAxisStart_deg
		{
			get; set;
		}

		public double SlaveAxisEnd_deg
		{
			get; set;
		}

		// parameterless constructor for XmlSerializer
		internal MachineDataDTO()
		{
			// default: do not run whole-machine simulation
			FullMachineSimulation = false;
		}

		// constructor from MachineData
		internal MachineDataDTO( FiveAxisMachineData data )
		{
			if( data == null ) {
				return;
			}
			ToolDirection = data.ToolDirection;
			data.ExternalAxes.ValidateFor( data.FiveAxisType );
			ExternalAxes = new ExternalAxisMachineDataDTO( data.ExternalAxes );
			MasterRotaryAxis = data.MasterRotaryAxis;
			SlaveRotaryAxis = data.SlaveRotaryAxis;
			TubeDir = data.TubeDir;
			MasterRotaryDirection = data.MasterAxis != null ? data.MasterAxis.RotaryDirection : RotaryDirection.None;
			SlaveRotaryDirection = data.SlaveAxis != null ? data.SlaveAxis.RotaryDirection : RotaryDirection.None;
			MasterTiltedVec_deg = new gp_XYZDTO( data.MasterTiltedVec_deg );
			SlaveTiltedVec_deg = new gp_XYZDTO( data.SlaveTiltedVec_deg );
			SimulationOffset = new gp_Vec_DTO( data.SimulationOffset );
			FullMachineSimulation = data.FullMachineSimulation;
			ToolLength = data.ToolLength;
			MasterAxisStart_deg = data.MasterAxis != null ? data.MasterAxis.LimitStart : 0;
			MasterAxisEnd_deg = data.MasterAxis != null ? data.MasterAxis.LimitEnd : 0;
			SlaveAxisStart_deg = data.SlaveAxis != null ? data.SlaveAxis.LimitStart : 0;
			SlaveAxisEnd_deg = data.SlaveAxis != null ? data.SlaveAxis.LimitEnd : 0;
		}
	}

	public class SpindleTypeMachineDataDTO : MachineDataDTO
	{
		public gp_Vec_DTO ToolToSlaveVec
		{
			get; set;
		} = new gp_Vec_DTO();

		public gp_Vec_DTO SlaveToMasterVec
		{
			get; set;
		} = new gp_Vec_DTO();

		// parameterless constructor for XmlSerializer
		internal SpindleTypeMachineDataDTO() : base()
		{
		}

		// constructor from SpindleTypeMachineData
		internal SpindleTypeMachineDataDTO( SpindleTypeMachineData data ) : base( data )
		{
			if( data == null ) {
				return;
			}
			ToolToSlaveVec = new gp_Vec_DTO( data.ToolToSlaveVec );
			SlaveToMasterVec = new gp_Vec_DTO( data.SlaveToMasterVec );
		}

		// convert to SpindleTypeMachineData
		internal SpindleTypeMachineData ToSpindleTypeMachineData()
		{
			SpindleTypeMachineData spindleTypeData = new SpindleTypeMachineData
			{
				ExternalAxes = ExternalAxes?.ToMachineData() ?? new ExternalAxisMachineData(),
				ToolDirection = ToolDirection,
				MasterRotaryAxis = MasterRotaryAxis,
				SlaveRotaryAxis = SlaveRotaryAxis,
				MasterTiltedVec_deg = MasterTiltedVec_deg.ToXYZ(),
				SlaveTiltedVec_deg = SlaveTiltedVec_deg.ToXYZ(),
				SimulationOffset = SimulationOffset.ToVec(),
				FullMachineSimulation = FullMachineSimulation,
				ToolLength = ToolLength,
				ToolToSlaveVec = ToolToSlaveVec.ToVec(),
				SlaveToMasterVec = SlaveToMasterVec.ToVec(),
				TubeDir = TubeDir
			};

			// the machine file may not assign the tube direction
			spindleTypeData.InitTubeDir();

			// cal rotate center by itself
			spindleTypeData.InitRotateAxes(
				MasterRotaryDirection, MasterAxisStart_deg, MasterAxisEnd_deg,
				SlaveRotaryDirection, SlaveAxisStart_deg, SlaveAxisEnd_deg );
			return spindleTypeData;
		}
	}

	public class TableTypeMachineDataDTO : MachineDataDTO
	{
		public gp_Vec_DTO MasterToSlaveVec
		{
			get; set;
		} = new gp_Vec_DTO();

		public gp_Vec_DTO MCSToMasterVec
		{
			get; set;
		} = new gp_Vec_DTO();

		// parameterless constructor for XmlSerializer
		internal TableTypeMachineDataDTO() : base()
		{
		}

		// constructor from TableTypeMachineData
		internal TableTypeMachineDataDTO( TableTypeMachineData data ) : base( data )
		{
			if( data == null ) {
				return;
			}

			MasterToSlaveVec = new gp_Vec_DTO( data.MasterToSlaveVec );
			MCSToMasterVec = new gp_Vec_DTO( data.MCSToMasterVec );
		}

		// convert to TableTypeMachineData
		internal TableTypeMachineData ToTableTypeMachineData()
		{
			TableTypeMachineData tableTypeData = new TableTypeMachineData
			{
				ExternalAxes = ExternalAxes?.ToMachineData() ?? new ExternalAxisMachineData(),
				ToolDirection = ToolDirection,
				MasterRotaryAxis = MasterRotaryAxis,
				SlaveRotaryAxis = SlaveRotaryAxis,
				MasterTiltedVec_deg = MasterTiltedVec_deg.ToXYZ(),
				SlaveTiltedVec_deg = SlaveTiltedVec_deg.ToXYZ(),
				SimulationOffset = SimulationOffset.ToVec(),
				FullMachineSimulation = FullMachineSimulation,
				ToolLength = ToolLength,
				MasterToSlaveVec = MasterToSlaveVec.ToVec(),
				MCSToMasterVec = MCSToMasterVec.ToVec(),
				TubeDir = TubeDir
			};

			// the machine file may not assign the tube direction
			tableTypeData.InitTubeDir();

			// cal rotate center by itself
			tableTypeData.InitRotateAxes(
				MasterRotaryDirection, MasterAxisStart_deg, MasterAxisEnd_deg,
				SlaveRotaryDirection, SlaveAxisStart_deg, SlaveAxisEnd_deg );
			return tableTypeData;
		}
	}

	public class MixTypeMachineDataDTO : MachineDataDTO
	{
		public MixTypeVariant MixTypeVariant
		{
			get; set;
		} = MixTypeVariant.Mix;

		public gp_Vec_DTO ToolToMasterVec
		{
			get; set;
		} = new gp_Vec_DTO();

		public gp_Vec_DTO MCSToSlaveVec
		{
			get; set;
		} = new gp_Vec_DTO();

		// parameterless constructor for XmlSerializer
		internal MixTypeMachineDataDTO() : base()
		{

		}

		// constructor from MixTypeMachineData
		internal MixTypeMachineDataDTO( MixTypeMachineData data ) : base( data )
		{
			if( data == null ) {
				return;
			}
			MixTypeVariant = data.MixTypeVariant;
			ToolToMasterVec = new gp_Vec_DTO( data.ToolToMasterVec );
			MCSToSlaveVec = new gp_Vec_DTO( data.MCSToSlaveVec );
		}

		// convert to MixTypeMachineData
		internal MixTypeMachineData ToMixTypeMachineData()
		{
			MixTypeMachineData mixTypeData = new MixTypeMachineData
			{
				ExternalAxes = ExternalAxes?.ToMachineData() ?? new ExternalAxisMachineData(),
				MixTypeVariant = MixTypeVariant,
				ToolDirection = ToolDirection,
				MasterRotaryAxis = MasterRotaryAxis,
				SlaveRotaryAxis = SlaveRotaryAxis,
				MasterTiltedVec_deg = MasterTiltedVec_deg.ToXYZ(),
				SlaveTiltedVec_deg = SlaveTiltedVec_deg.ToXYZ(),
				SimulationOffset = SimulationOffset.ToVec(),
				FullMachineSimulation = FullMachineSimulation,
				ToolLength = ToolLength,
				ToolToMasterVec = ToolToMasterVec.ToVec(),
				MCSToSlaveVec = MCSToSlaveVec.ToVec(),
				TubeDir = TubeDir
			};

			// the machine file may not assign the tube direction
			mixTypeData.InitTubeDir();

			// cal rotate center by itself
			mixTypeData.InitRotateAxes(
				MasterRotaryDirection, MasterAxisStart_deg, MasterAxisEnd_deg,
				SlaveRotaryDirection, SlaveAxisStart_deg, SlaveAxisEnd_deg );
			return mixTypeData;
		}
	}

	public class gp_DirDTO
	{
		public double X
		{
			get; set;
		}

		public double Y
		{
			get; set;
		}

		public double Z
		{
			get; set;
		}

		internal gp_DirDTO()
		{
		}

		internal gp_DirDTO( gp_Dir dir )
		{
			if( dir == null ) {
				return;
			}
			X = dir.X();
			Y = dir.Y();
			Z = dir.Z();
		}

		internal gp_Dir ToDir()
		{
			return new gp_Dir( X, Y, Z );
		}
	}

	public class gp_XYZDTO
	{
		public double X
		{
			get; set;
		}

		public double Y
		{
			get; set;
		}

		public double Z
		{
			get; set;
		}

		internal gp_XYZDTO()
		{
		}

		internal gp_XYZDTO( gp_XYZ xyz )
		{
			if( xyz == null ) {
				return;
			}
			X = xyz.X();
			Y = xyz.Y();
			Z = xyz.Z();
		}

		internal gp_XYZ ToXYZ()
		{
			return new gp_XYZ( X, Y, Z );
		}
	}

	public class gp_PointDTO
	{
		public double X
		{
			get; set;
		}

		public double Y
		{
			get; set;
		}

		public double Z
		{
			get; set;
		}

		internal gp_PointDTO()
		{
		}

		internal gp_PointDTO( gp_Pnt point )
		{
			if( point == null ) {
				return;
			}
			X = point.X();
			Y = point.Y();
			Z = point.Z();
		}

		internal gp_Pnt ToPnt()
		{
			return new gp_Pnt( X, Y, Z );
		}
	}

	public class gp_Vec_DTO
	{
		public double X
		{
			get; set;
		}

		public double Y
		{
			get; set;
		}

		public double Z
		{
			get; set;
		}

		internal gp_Vec_DTO()
		{
		}

		internal gp_Vec_DTO( gp_Vec vector )
		{
			if( vector == null ) {
				return;
			}
			X = vector.X();
			Y = vector.Y();
			Z = vector.Z();
		}

		internal gp_Vec ToVec()
		{
			return new gp_Vec( X, Y, Z );
		}
	}

	#endregion
}
