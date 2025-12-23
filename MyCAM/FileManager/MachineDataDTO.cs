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
		internal static MachineDataDTOContainer ToDTOContainer( MachineData machineData )
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

		internal static MachineData ToMachineData( MachineDataDTOContainer container )
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

	public class MachineDataDTO
	{
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
		}

		// constructor from MachineData
		internal MachineDataDTO( MachineData data )
		{
			if( data == null ) {
				return;
			}
			ToolDirection = data.ToolDirection;
			MasterRotaryAxis = data.MasterRotaryAxis;
			SlaveRotaryAxis = data.SlaveRotaryAxis;
			MasterRotaryDirection = data.MasterRotaryDirection;
			SlaveRotaryDirection = data.SlaveRotaryDirection;
			MasterTiltedVec_deg = new gp_XYZDTO( data.MasterTiltedVec_deg );
			SlaveTiltedVec_deg = new gp_XYZDTO( data.SlaveTiltedVec_deg );
			ToolLength = data.ToolLength;
			MasterAxisStart_deg = data.MasterAxisStart_deg;
			MasterAxisEnd_deg = data.MasterAxisEnd_deg;
			SlaveAxisStart_deg = data.SlaveAxisStart_deg;
			SlaveAxisEnd_deg = data.SlaveAxisEnd_deg;
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
				ToolDirection = ToolDirection,
				MasterRotaryAxis = MasterRotaryAxis,
				SlaveRotaryAxis = SlaveRotaryAxis,
				MasterRotaryDirection = MasterRotaryDirection,
				SlaveRotaryDirection = SlaveRotaryDirection,
				MasterTiltedVec_deg = MasterTiltedVec_deg.ToXYZ(),
				SlaveTiltedVec_deg = SlaveTiltedVec_deg.ToXYZ(),
				ToolLength = ToolLength,
				MasterAxisStart_deg = MasterAxisStart_deg,
				MasterAxisEnd_deg = MasterAxisEnd_deg,
				SlaveAxisStart_deg = SlaveAxisStart_deg,
				SlaveAxisEnd_deg = SlaveAxisEnd_deg,
				ToolToSlaveVec = ToolToSlaveVec.ToVec(),
				SlaveToMasterVec = SlaveToMasterVec.ToVec()
			};
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
				ToolDirection = ToolDirection,
				MasterRotaryAxis = MasterRotaryAxis,
				SlaveRotaryAxis = SlaveRotaryAxis,
				MasterRotaryDirection = MasterRotaryDirection,
				SlaveRotaryDirection = SlaveRotaryDirection,
				MasterTiltedVec_deg = MasterTiltedVec_deg.ToXYZ(),
				SlaveTiltedVec_deg = SlaveTiltedVec_deg.ToXYZ(),
				ToolLength = ToolLength,
				MasterAxisStart_deg = MasterAxisStart_deg,
				MasterAxisEnd_deg = MasterAxisEnd_deg,
				SlaveAxisStart_deg = SlaveAxisStart_deg,
				SlaveAxisEnd_deg = SlaveAxisEnd_deg,
				MasterToSlaveVec = MasterToSlaveVec.ToVec(),
				MCSToMasterVec = MCSToMasterVec.ToVec()
			};
			return tableTypeData;
		}
	}

	public class MixTypeMachineDataDTO : MachineDataDTO
	{
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
			ToolToMasterVec = new gp_Vec_DTO( data.ToolToMasterVec );
			MCSToSlaveVec = new gp_Vec_DTO( data.MCSToSlaveVec );
		}

		// convert to MixTypeMachineData
		internal MixTypeMachineData ToMixTypeMachineData()
		{
			MixTypeMachineData mixTypeData = new MixTypeMachineData
			{
				ToolDirection = ToolDirection,
				MasterRotaryAxis = MasterRotaryAxis,
				SlaveRotaryAxis = SlaveRotaryAxis,
				MasterRotaryDirection = MasterRotaryDirection,
				SlaveRotaryDirection = SlaveRotaryDirection,
				MasterTiltedVec_deg = MasterTiltedVec_deg.ToXYZ(),
				SlaveTiltedVec_deg = SlaveTiltedVec_deg.ToXYZ(),
				ToolLength = ToolLength,
				MasterAxisStart_deg = MasterAxisStart_deg,
				MasterAxisEnd_deg = MasterAxisEnd_deg,
				SlaveAxisStart_deg = SlaveAxisStart_deg,
				SlaveAxisEnd_deg = SlaveAxisEnd_deg,
				ToolToMasterVec = ToolToMasterVec.ToVec(),
				MCSToSlaveVec = MCSToSlaveVec.ToVec()
			};
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
