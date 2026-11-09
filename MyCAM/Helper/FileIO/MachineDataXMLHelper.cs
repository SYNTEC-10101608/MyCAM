using MyCAM.App;
using MyCAM.Data;
using MyCAM.FileManager;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace MyCAM.Helper
{
	public enum EMachineDataLoadStatus
	{
		ReadSuccess,
		TreeInvalid,          // tree node invlaid, will use default tree
		NullFile,             // null file or null root
		NullMachineDataNode,  // xml without MachineData node
		InvalidPrValue
	}

	public enum EFilePrValueStatus
	{
		GetSuccess,
		PrNodeMissing,
		PrValueInvalid,
		PrEnumValueInvalid,
		PrTypeInvalid,
	}

	internal class MachineDataXMLHelper
	{
		#region generate XML from MachineData

		internal static XDocument ConvertMachineDataAndTree2XML( FiveAxisMachineData machineData )
		{
			try {
				if( machineData == null ) {
					throw new ArgumentNullException( nameof( machineData ) );
				}

				// transform to DTO
				MachineDataDTOContainer container = MachineDataDTOManager.ToDTOContainer( machineData );

				// machineDataDTOContainer to XML node
				XElement machineDataXml = ConvertMachineDTO2XML( container );

				// machineTreeNode to XML node
				XElement treeXml = ConvertMachineTreeNode2XML( machineData.RootNode );

				// set base node as default
				if( treeXml == null ) {
					treeXml = new XElement( "Child", new XAttribute( "Type", MachineComponentType.Base.ToString() ) );
				}

				// build outer root
				XElement fileRootNode = new XElement( "MachineDataFile",
										new XElement( "MachineData", machineDataXml.Nodes() ),
										new XElement( "MachineTree", treeXml )
										);
				return new XDocument( new XDeclaration( "1.0", "utf-8", null ), fileRootNode );
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "MachineDataXMLGenerationFailed", MACHINE_DATA_XML_GENERATION_FAILED ), ex.Message ), MyApp.NoticeType.Error );
				return null;
			}
		}

		static XElement ConvertMachineDTO2XML( MachineDataDTOContainer container )
		{
			// blu is um
			const double dRatioMMToBlu = 1000;
			if( container == null || container.MachineDataDTO == null ) {
				throw new ArgumentNullException( nameof( container ), "MachineDataDTO is null" );
			}
			MachineDataDTO machineDataDTO = container.MachineDataDTO;
			XElement root = new XElement( "MachineData" );

			// five axis Type node pr3001
			int machineTypeValue = ConvertMachineDTO2Pr3001( machineDataDTO );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.FiveAxisType, machineTypeValue );

			// common properties
			ConvertPropertyToPrNode( root, (int)MachinePrValue.ToolDirection, (int)machineDataDTO.ToolDirection );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.MasterRotaryAxis, (int)machineDataDTO.MasterRotaryAxis );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.SlaveRotaryAxis, (int)machineDataDTO.SlaveRotaryAxis );
			ConvertPropertyToPrNode( root, MachineParamName.TUBE_DIR, (int)machineDataDTO.TubeDir );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.MasterRotaryDirection, (int)machineDataDTO.MasterRotaryDirection );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.SlaveRotaryDirection, (int)machineDataDTO.SlaveRotaryDirection );

			// turn mm to blu
			ConvertPropertyToPrNode( root, (int)MachinePrValue.ToolLength, machineDataDTO.ToolLength * dRatioMMToBlu );

			// axis range (degrees, no conversion needed)
			ConvertPropertyToPrNode( root, MachineParamName.MASTER_LIMIMT_START_PARAM_NAME, machineDataDTO.MasterAxisStart_deg );
			ConvertPropertyToPrNode( root, MachineParamName.MASTER_LIMIMT_END_PARAM_NAME, machineDataDTO.MasterAxisEnd_deg );
			ConvertPropertyToPrNode( root, MachineParamName.SLAVE_LIMIMT_START_PARAM_NAME, machineDataDTO.SlaveAxisStart_deg );
			ConvertPropertyToPrNode( root, MachineParamName.SLAVE_LIMIMT_END_PARAM_NAME, machineDataDTO.SlaveAxisEnd_deg );

			// mster/slave tilted vector (turn mm to blu)
			ConvertPropertyToPrNode( root, (int)MachinePrValue.MasterTiltedVec_X, machineDataDTO.MasterTiltedVec_deg.X * dRatioMMToBlu );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.MasterTiltedVec_Y, machineDataDTO.MasterTiltedVec_deg.Y * dRatioMMToBlu );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.MasterTiltedVec_Z, machineDataDTO.MasterTiltedVec_deg.Z * dRatioMMToBlu );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.SlaveTiltedVec_X, machineDataDTO.SlaveTiltedVec_deg.X * dRatioMMToBlu );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.SlaveTiltedVec_Y, machineDataDTO.SlaveTiltedVec_deg.Y * dRatioMMToBlu );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.SlaveTiltedVec_Z, machineDataDTO.SlaveTiltedVec_deg.Z * dRatioMMToBlu );

			// whole-machine simulation flag ( 0: off, 1: on )
			ConvertPropertyToPrNode( root, MachineParamName.FULL_MACHINE_SIMULATION, machineDataDTO.FullMachineSimulation ? 1 : 0 );

			// with default 0
			double[] spindleValues = new double[ 6 ]; // 3021~3026
			double[] tableValues = new double[ 6 ];   // 3031~3036
			double[] mixValues = new double[ 6 ];     // 3041~3046
			if( machineDataDTO is SpindleTypeMachineDataDTO spindle ) {
				if( spindle.ToolToSlaveVec != null ) {
					spindleValues[ 0 ] = spindle.ToolToSlaveVec.X;
					spindleValues[ 1 ] = spindle.ToolToSlaveVec.Y;
					spindleValues[ 2 ] = spindle.ToolToSlaveVec.Z;
				}
				if( spindle.SlaveToMasterVec != null ) {
					spindleValues[ 3 ] = spindle.SlaveToMasterVec.X;
					spindleValues[ 4 ] = spindle.SlaveToMasterVec.Y;
					spindleValues[ 5 ] = spindle.SlaveToMasterVec.Z;
				}
			}
			if( machineDataDTO is TableTypeMachineDataDTO table ) {
				;
				if( table.MasterToSlaveVec != null ) {
					tableValues[ 0 ] = table.MasterToSlaveVec.X;
					tableValues[ 1 ] = table.MasterToSlaveVec.Y;
					tableValues[ 2 ] = table.MasterToSlaveVec.Z;
				}
				if( table.MCSToMasterVec != null ) {
					tableValues[ 3 ] = table.MCSToMasterVec.X;
					tableValues[ 4 ] = table.MCSToMasterVec.Y;
					tableValues[ 5 ] = table.MCSToMasterVec.Z;
				}
			}
			if( machineDataDTO is MixTypeMachineDataDTO mix ) {
				;
				if( mix.ToolToMasterVec != null ) {
					mixValues[ 0 ] = mix.ToolToMasterVec.X;
					mixValues[ 1 ] = mix.ToolToMasterVec.Y;
					mixValues[ 2 ] = mix.ToolToMasterVec.Z;
				}
				if( mix.MCSToSlaveVec != null ) {
					mixValues[ 3 ] = mix.MCSToSlaveVec.X;
					mixValues[ 4 ] = mix.MCSToSlaveVec.Y;
					mixValues[ 5 ] = mix.MCSToSlaveVec.Z;
				}
			}

			// add all nodes (even if values are 0)
			root.Add( new XComment( "Spindle Type" ) );
			for( int i = 0; i < 6; i++ ) {
				ConvertPropertyToPrNode( root, (int)MachinePrValue.ToolToSlaveVec_X + i, spindleValues[ i ] * dRatioMMToBlu );
			}
			root.Add( new XComment( "Table Type" ) );
			for( int i = 0; i < 6; i++ ) {
				ConvertPropertyToPrNode( root, (int)MachinePrValue.MasterToSlaveVec_X + i, tableValues[ i ] * dRatioMMToBlu );
			}
			root.Add( new XComment( "Mix Type" ) );
			for( int i = 0; i < 6; i++ ) {
				ConvertPropertyToPrNode( root, (int)MachinePrValue.ToolToMasterVec_X + i, mixValues[ i ] * dRatioMMToBlu );
			}
			WriteExternalAxisData( root, machineDataDTO.ExternalAxes );
			return root;
		}

		static void WriteExternalAxisData( XElement root, ExternalAxisMachineDataDTO data )
		{
			if( data == null || data.Type == ExternalAxisType.None ) {
				return;
			}
			ConvertPropertyToPrNode( root, MachineParamName.EXTERNAL_AXIS_TYPE, (int)data.Type );
			WriteExternalAxisProperties( root, "Ext1", data.Ext1RotaryAxis, data.Ext1RotaryDirection,
				data.MCSToExt1Vec, data.Ext1TiltedVec_deg, data.Ext1Start_deg, data.Ext1End_deg );
			if( data.Type == ExternalAxisType.Dual ) {
				WriteExternalAxisProperties( root, "Ext2", data.Ext2RotaryAxis, data.Ext2RotaryDirection,
					data.Ext1ToExt2Vec, data.Ext2TiltedVec_deg, data.Ext2Start_deg, data.Ext2End_deg );
			}
		}

		static void WriteExternalAxisProperties( XElement root, string prefix, RotaryAxis axis, RotaryDirection direction,
			gp_Vec_DTO offset, gp_XYZDTO tilt, double start, double end )
		{
			ConvertPropertyToPrNode( root, prefix + "_Axis", (int)axis );
			ConvertPropertyToPrNode( root, prefix + "_Direction", (int)direction );
			ConvertPropertyToPrNode( root, prefix + "_Start", start );
			ConvertPropertyToPrNode( root, prefix + "_End", end );
			ConvertPropertyToPrNode( root, prefix + "_Offset_X", offset.X * 1000 );
			ConvertPropertyToPrNode( root, prefix + "_Offset_Y", offset.Y * 1000 );
			ConvertPropertyToPrNode( root, prefix + "_Offset_Z", offset.Z * 1000 );
			ConvertPropertyToPrNode( root, prefix + "_Tilt_X", tilt.X * 1000 );
			ConvertPropertyToPrNode( root, prefix + "_Tilt_Y", tilt.Y * 1000 );
			ConvertPropertyToPrNode( root, prefix + "_Tilt_Z", tilt.Z * 1000 );
		}

		static int ConvertMachineDTO2Pr3001( MachineDataDTO machineDataDTO )
		{
			if( machineDataDTO == null ) {
				throw new ArgumentNullException( nameof( machineDataDTO ) );
			}
			if( machineDataDTO is SpindleTypeMachineDataDTO ) {
				return (int)FiveAxisType.Spindle;
			}
			if( machineDataDTO is TableTypeMachineDataDTO ) {
				return (int)FiveAxisType.Table;
			}
			if( machineDataDTO is MixTypeMachineDataDTO mixDTO ) {
				switch( mixDTO.MixTypeVariant ) {
					case MixTypeVariant.MasterOnly:
						return (int)FiveAxisType.SingleSpindle;
					case MixTypeVariant.SlaveOnly:
						return (int)FiveAxisType.SingleTable;
					default:
						return (int)FiveAxisType.Mix;
				}
			}
			throw new ArgumentException( "Unsupported machine configuration.", nameof( machineDataDTO ) );
		}

		static XElement ConvertMachineTreeNode2XML( MachineTreeNode treeNode )
		{
			if( treeNode == null ) {
				return new XElement( "Child", new XAttribute( "Type", MachineComponentType.Base.ToString() ) );
			}

			// node element is "Child", with attribute "Type"
			XElement nodeElement = new XElement( "Child",
				new XAttribute( "Type", treeNode.Type.ToString() )
			);
			foreach( var childNode in treeNode.Children ) {
				nodeElement.Add( ConvertMachineTreeNode2XML( childNode ) );
			}
			return nodeElement;
		}

		static void ConvertPropertyToPrNode( XElement parent, int nPrID, object prValue )
		{
			parent.Add( new XElement( "Pr",
				new XAttribute( "ID", nPrID ),
				new XAttribute( "Value", prValue ?? 0 ) ) );
		}

		static void ConvertPropertyToPrNode( XElement parent, string szName, object prValue )
		{
			parent.Add( new XElement( "Pr",
				new XAttribute( "ID", szName ),
				new XAttribute( "Value", prValue ?? 0 ) ) );
		}

		#endregion

		#region deserialize XML to machine data

		internal static FiveAxisMachineData ConvertMachineDataFileToMachineData( XDocument machineDataDoc, out EMachineDataLoadStatus status, out int nErrorPrIndex )
		{
			return ConvertMachineDataFileToMachineData( machineDataDoc, out status, out nErrorPrIndex, out _ );
		}

		internal static FiveAxisMachineData ConvertMachineDataFileToMachineData( XDocument machineDataDoc, out EMachineDataLoadStatus status,
			out int nErrorPrIndex, out string errorParameterId )
		{
			// default status
			status = EMachineDataLoadStatus.ReadSuccess;
			nErrorPrIndex = -1;
			errorParameterId = string.Empty;

			// default machine data
			FiveAxisMachineData machineData = MachineDataFactory.CreateDefaultSpindle();
			if( machineDataDoc == null ) {
				status = EMachineDataLoadStatus.NullFile;
				return machineData;
			}
			XElement root = machineDataDoc.Root;
			if( root == null ) {
				status = EMachineDataLoadStatus.NullFile;
				return machineData;
			}

			// it contains machine data information
			XElement machineDataElement = root.Element( "MachineData" );

			// it contains machine tree information
			XElement machineTreeElement = root.Element( "MachineTree" );

			// can be no tree, because it is only used for simulation
			if( machineDataElement == null ) {
				status = EMachineDataLoadStatus.NullMachineDataNode;
				return machineData;
			}

			// turn machineData XML to DTO
			MachineDataDTOContainer container = ConvertXML2MachineData( machineDataElement, out EFilePrValueStatus prValueStatus,
				out nErrorPrIndex, out errorParameterId );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				status = EMachineDataLoadStatus.InvalidPrValue;
				if( string.IsNullOrEmpty( errorParameterId ) && nErrorPrIndex >= 0 ) {
					errorParameterId = nErrorPrIndex.ToString( CultureInfo.InvariantCulture );
				}
				return machineData;
			}

			// turn machineTree XML to MachineTreeNode
			bool bGetTreeSuccess = ConvertXML2MachineTreeNodeSuccess( machineTreeElement, out MachineTreeNode rootNode );

			// convert to MachineData
			machineData = MachineDataDTOManager.ToMachineData( container );
			if( bGetTreeSuccess ) {
				machineData.RootNode = rootNode;
			}
			else {
				status = EMachineDataLoadStatus.TreeInvalid;
			}
			return machineData;
		}

		static MachineDataDTOContainer ConvertXML2MachineData( XElement machineData, out EFilePrValueStatus prValueStatus,
			out int nErrorPrIndex, out string errorParameterId )
		{
			nErrorPrIndex = -1;
			errorParameterId = string.Empty;
			prValueStatus = EFilePrValueStatus.GetSuccess;

			// blu is um
			const double dRatioBluToMM = 0.001;
			if( machineData == null ) {
				throw new ArgumentNullException( nameof( machineData ) );
			}
			MachineDataDTOContainer machineDTOContainer = new MachineDataDTOContainer();
			MachineDataDTO machineDataDTO;

			// get pr3001
			FiveAxisType fiveAxisType = GetPrValue<FiveAxisType>( machineData, (int)MachinePrValue.FiveAxisType, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.FiveAxisType;
				return machineDTOContainer;
			}
			switch( fiveAxisType ) {
				case FiveAxisType.Spindle:
					machineDataDTO = new SpindleTypeMachineDataDTO();
					break;
				case FiveAxisType.Table:
					machineDataDTO = new TableTypeMachineDataDTO();
					break;
				case FiveAxisType.Mix:
					machineDataDTO = new MixTypeMachineDataDTO();
					break;

				// single rotary axis machine is a specialized mix type
				case FiveAxisType.SingleSpindle:
					machineDataDTO = new MixTypeMachineDataDTO { MixTypeVariant = MixTypeVariant.MasterOnly };
					break;
				case FiveAxisType.SingleTable:
					machineDataDTO = new MixTypeMachineDataDTO { MixTypeVariant = MixTypeVariant.SlaveOnly };
					break;
				default:

					// GetPrValue already stop it
					machineDataDTO = new MachineDataDTO();
					break;

			}
			machineDTOContainer.MachineDataDTO = machineDataDTO;

			// normal pr
			machineDataDTO.ToolDirection = GetPrValue<ToolDirection>( machineData, (int)MachinePrValue.ToolDirection, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.ToolDirection;
				return machineDTOContainer;
			}
			machineDataDTO.MasterRotaryAxis = GetPrValue<RotaryAxis>( machineData, (int)MachinePrValue.MasterRotaryAxis, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.MasterRotaryAxis;
				return machineDTOContainer;
			}
			machineDataDTO.SlaveRotaryAxis = GetPrValue<RotaryAxis>( machineData, (int)MachinePrValue.SlaveRotaryAxis, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.SlaveRotaryAxis;
				return machineDTOContainer;
			}

			// optional, old machine files have no this node, the rotary axis is used instead
			machineDataDTO.TubeDir = GetPrValue<RotaryAxis>( machineData, MachineParamName.TUBE_DIR, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				machineDataDTO.TubeDir = RotaryAxis.None;
			}
			machineDataDTO.MasterRotaryDirection = GetPrValue<RotaryDirection>( machineData, (int)MachinePrValue.MasterRotaryDirection, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.MasterRotaryDirection;
				return machineDTOContainer;
			}
			machineDataDTO.SlaveRotaryDirection = GetPrValue<RotaryDirection>( machineData, (int)MachinePrValue.SlaveRotaryDirection, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.SlaveRotaryDirection;
				return machineDTOContainer;
			}

			// turn blu to mm
			machineDataDTO.ToolLength = GetPrValue<double>( machineData, (int)MachinePrValue.ToolLength, out prValueStatus, dRatioBluToMM );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.ToolLength;
				return machineDTOContainer;
			}

			// axis range (degrees, no conversion needed)
			machineDataDTO.MasterAxisStart_deg = GetPrValue<double>( machineData, MachineParamName.MASTER_LIMIMT_START_PARAM_NAME, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.AxialLimit;
				return machineDTOContainer;
			}
			machineDataDTO.MasterAxisEnd_deg = GetPrValue<double>( machineData, MachineParamName.MASTER_LIMIMT_END_PARAM_NAME, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.AxialLimit;
				return machineDTOContainer;
			}
			machineDataDTO.SlaveAxisStart_deg = GetPrValue<double>( machineData, MachineParamName.SLAVE_LIMIMT_START_PARAM_NAME, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.AxialLimit;
				return machineDTOContainer;
			}
			machineDataDTO.SlaveAxisEnd_deg = GetPrValue<double>( machineData, MachineParamName.SLAVE_LIMIMT_END_PARAM_NAME, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.AxialLimit;
				return machineDTOContainer;
			}

			// simulation offset
			machineDataDTO.SimulationOffset.X = GetPrValue<double>( machineData, MachineParamName.SIMULATION_OFFSET_X, out prValueStatus, dRatioBluToMM );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				machineDataDTO.SimulationOffset.X = 0;
			}
			machineDataDTO.SimulationOffset.Y = GetPrValue<double>( machineData, MachineParamName.SIMULATION_OFFSET_Y, out prValueStatus, dRatioBluToMM );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				machineDataDTO.SimulationOffset.Y = 0;
			}
			machineDataDTO.SimulationOffset.Z = GetPrValue<double>( machineData, MachineParamName.SIMULATION_OFFSET_Z, out prValueStatus, dRatioBluToMM );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				machineDataDTO.SimulationOffset.Z = 0;
			}

			// whole-machine simulation flag ( 0: off, 1: on ), default false for old files without this node
			int nFullMachineSimulation = GetPrValue<int>( machineData, MachineParamName.FULL_MACHINE_SIMULATION, out prValueStatus );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				machineDataDTO.FullMachineSimulation = false;
			}
			else {
				machineDataDTO.FullMachineSimulation = nFullMachineSimulation == 1;
			}

			machineDataDTO.MasterTiltedVec_deg.X = GetPrValue<double>( machineData, (int)MachinePrValue.MasterTiltedVec_X, out prValueStatus, dRatioBluToMM );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.MasterTiltedVec_X;
				return machineDTOContainer;
			}
			machineDataDTO.MasterTiltedVec_deg.Y = GetPrValue<double>( machineData, (int)MachinePrValue.MasterTiltedVec_Y, out prValueStatus, dRatioBluToMM );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.MasterTiltedVec_Y;
				return machineDTOContainer;
			}
			machineDataDTO.MasterTiltedVec_deg.Z = GetPrValue<double>( machineData, (int)MachinePrValue.MasterTiltedVec_Z, out prValueStatus, dRatioBluToMM );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.MasterTiltedVec_Z;
				return machineDTOContainer;
			}
			machineDataDTO.SlaveTiltedVec_deg.X = GetPrValue<double>( machineData, (int)MachinePrValue.SlaveTiltedVec_X, out prValueStatus, dRatioBluToMM );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.SlaveTiltedVec_X;
				return machineDTOContainer;
			}
			machineDataDTO.SlaveTiltedVec_deg.Y = GetPrValue<double>( machineData, (int)MachinePrValue.SlaveTiltedVec_Y, out prValueStatus, dRatioBluToMM );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.SlaveTiltedVec_Y;
				return machineDTOContainer;
			}
			machineDataDTO.SlaveTiltedVec_deg.Z = GetPrValue<double>( machineData, (int)MachinePrValue.SlaveTiltedVec_Z, out prValueStatus, dRatioBluToMM );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				nErrorPrIndex = (int)MachinePrValue.SlaveTiltedVec_Z;
				return machineDTOContainer;
			}

			// Spindle Type
			if( machineDataDTO is SpindleTypeMachineDataDTO spindle ) {
				spindle.ToolToSlaveVec.X = GetPrValue<double>( machineData, (int)MachinePrValue.ToolToSlaveVec_X, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.ToolToSlaveVec_X;
					return machineDTOContainer;
				}
				spindle.ToolToSlaveVec.Y = GetPrValue<double>( machineData, (int)MachinePrValue.ToolToSlaveVec_Y, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.ToolToSlaveVec_Y;
					return machineDTOContainer;
				}
				spindle.ToolToSlaveVec.Z = GetPrValue<double>( machineData, (int)MachinePrValue.ToolToSlaveVec_Z, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.ToolToSlaveVec_Z;
					return machineDTOContainer;
				}
				spindle.SlaveToMasterVec.X = GetPrValue<double>( machineData, (int)MachinePrValue.SlaveToMasterVec_X, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.SlaveToMasterVec_X;
					return machineDTOContainer;
				}
				spindle.SlaveToMasterVec.Y = GetPrValue<double>( machineData, (int)MachinePrValue.SlaveToMasterVec_Y, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.SlaveToMasterVec_Y;
					return machineDTOContainer;
				}
				spindle.SlaveToMasterVec.Z = GetPrValue<double>( machineData, (int)MachinePrValue.SlaveToMasterVec_Z, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.SlaveToMasterVec_Z;
					return machineDTOContainer;
				}
			}

			// Table Type
			if( machineDataDTO is TableTypeMachineDataDTO table ) {
				table.MasterToSlaveVec.X = GetPrValue<double>( machineData, (int)MachinePrValue.MasterToSlaveVec_X, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.MasterToSlaveVec_X;
					return machineDTOContainer;
				}
				table.MasterToSlaveVec.Y = GetPrValue<double>( machineData, (int)MachinePrValue.MasterToSlaveVec_Y, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.MasterToSlaveVec_Y;
					return machineDTOContainer;
				}
				table.MasterToSlaveVec.Z = GetPrValue<double>( machineData, (int)MachinePrValue.MasterToSlaveVec_Z, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.MasterToSlaveVec_Z;
					return machineDTOContainer;
				}
				table.MCSToMasterVec.X = GetPrValue<double>( machineData, (int)MachinePrValue.MCSToMasterVec_X, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.MCSToMasterVec_X;
					return machineDTOContainer;
				}
				table.MCSToMasterVec.Y = GetPrValue<double>( machineData, (int)MachinePrValue.MCSToMasterVec_Y, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.MCSToMasterVec_Y;
					return machineDTOContainer;
				}
				table.MCSToMasterVec.Z = GetPrValue<double>( machineData, (int)MachinePrValue.MCSToMasterVec_Z, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.MCSToMasterVec_Z;
					return machineDTOContainer;
				}
			}

			// Mix Type
			if( machineDataDTO is MixTypeMachineDataDTO mix ) {
				mix.ToolToMasterVec.X = GetPrValue<double>( machineData, (int)MachinePrValue.ToolToMasterVec_X, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.ToolToMasterVec_X;
					return machineDTOContainer;
				}
				mix.ToolToMasterVec.Y = GetPrValue<double>( machineData, (int)MachinePrValue.ToolToMasterVec_Y, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.ToolToMasterVec_Y;
					return machineDTOContainer;
				}
				mix.ToolToMasterVec.Z = GetPrValue<double>( machineData, (int)MachinePrValue.ToolToMasterVec_Z, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.ToolToMasterVec_Z;
					return machineDTOContainer;
				}
				mix.MCSToSlaveVec.X = GetPrValue<double>( machineData, (int)MachinePrValue.MCSToSlaveVec_X, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.MCSToSlaveVec_X;
					return machineDTOContainer;
				}
				mix.MCSToSlaveVec.Y = GetPrValue<double>( machineData, (int)MachinePrValue.MCSToSlaveVec_Y, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.MCSToSlaveVec_Y;
					return machineDTOContainer;
				}
				mix.MCSToSlaveVec.Z = GetPrValue<double>( machineData, (int)MachinePrValue.MCSToSlaveVec_Z, out prValueStatus, dRatioBluToMM );
				if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
					nErrorPrIndex = (int)MachinePrValue.MCSToSlaveVec_Z;
					return machineDTOContainer;
				}
			}
			prValueStatus = ReadExternalAxisData( machineData, machineDataDTO, out errorParameterId );
			return machineDTOContainer;
		}

		static EFilePrValueStatus ReadExternalAxisData( XElement root, MachineDataDTO machineDataDTO, out string errorParameterId )
		{
			errorParameterId = string.Empty;
			ExternalAxisMachineDataDTO data = machineDataDTO.ExternalAxes;
			data.Type = GetPrValue<ExternalAxisType>( root, MachineParamName.EXTERNAL_AXIS_TYPE, out EFilePrValueStatus status );
			if( status == EFilePrValueStatus.PrNodeMissing ) {
				return EFilePrValueStatus.GetSuccess;
			}
			if( status != EFilePrValueStatus.GetSuccess || data.Type == ExternalAxisType.None ) {
				if( status != EFilePrValueStatus.GetSuccess ) {
					errorParameterId = MachineParamName.EXTERNAL_AXIS_TYPE;
				}
				return status;
			}
			if( machineDataDTO is TableTypeMachineDataDTO
				|| ( machineDataDTO is MixTypeMachineDataDTO && data.Type == ExternalAxisType.Dual ) ) {
				errorParameterId = MachineParamName.EXTERNAL_AXIS_TYPE;
				return EFilePrValueStatus.PrValueInvalid;
			}
			ReadExternalAxisProperties( root, "Ext1", data.MCSToExt1Vec, data.Ext1TiltedVec_deg, ref status, ref errorParameterId,
				out RotaryAxis ext1Axis, out RotaryDirection ext1Direction, out double ext1Start, out double ext1End );
			data.Ext1RotaryAxis = ext1Axis;
			data.Ext1RotaryDirection = ext1Direction;
			data.Ext1Start_deg = ext1Start;
			data.Ext1End_deg = ext1End;
			if( data.Type == ExternalAxisType.Dual ) {
				ReadExternalAxisProperties( root, "Ext2", data.Ext1ToExt2Vec, data.Ext2TiltedVec_deg, ref status, ref errorParameterId,
					out RotaryAxis ext2Axis, out RotaryDirection ext2Direction, out double ext2Start, out double ext2End );
				data.Ext2RotaryAxis = ext2Axis;
				data.Ext2RotaryDirection = ext2Direction;
				data.Ext2Start_deg = ext2Start;
				data.Ext2End_deg = ext2End;
			}
			if( status != EFilePrValueStatus.GetSuccess ) {
				return status;
			}
			try {
				FiveAxisType type = machineDataDTO is SpindleTypeMachineDataDTO ? FiveAxisType.Spindle
					: machineDataDTO is MixTypeMachineDataDTO ? FiveAxisType.Mix : FiveAxisType.Table;
				data.ToMachineData().ValidateFor( type );
			}
			catch( ArgumentException ) {
				errorParameterId = MachineParamName.EXTERNAL_AXIS_TYPE;
				return EFilePrValueStatus.PrValueInvalid;
			}
			return EFilePrValueStatus.GetSuccess;
		}

		static void ReadExternalAxisProperties( XElement root, string prefix, gp_Vec_DTO offset, gp_XYZDTO tilt,
			ref EFilePrValueStatus status, ref string errorParameterId,
			out RotaryAxis axis, out RotaryDirection direction, out double start, out double end )
		{
			axis = ReadExternalAxisValue<RotaryAxis>( root, prefix + "_Axis", ref status, ref errorParameterId );
			direction = ReadExternalAxisValue<RotaryDirection>( root, prefix + "_Direction", ref status, ref errorParameterId );
			start = ReadExternalAxisValue<double>( root, prefix + "_Start", ref status, ref errorParameterId );
			end = ReadExternalAxisValue<double>( root, prefix + "_End", ref status, ref errorParameterId );
			offset.X = ReadExternalAxisValue<double>( root, prefix + "_Offset_X", ref status, ref errorParameterId, 0.001 );
			offset.Y = ReadExternalAxisValue<double>( root, prefix + "_Offset_Y", ref status, ref errorParameterId, 0.001 );
			offset.Z = ReadExternalAxisValue<double>( root, prefix + "_Offset_Z", ref status, ref errorParameterId, 0.001 );
			tilt.X = ReadExternalAxisValue<double>( root, prefix + "_Tilt_X", ref status, ref errorParameterId, 0.001 );
			tilt.Y = ReadExternalAxisValue<double>( root, prefix + "_Tilt_Y", ref status, ref errorParameterId, 0.001 );
			tilt.Z = ReadExternalAxisValue<double>( root, prefix + "_Tilt_Z", ref status, ref errorParameterId, 0.001 );
			if( status == EFilePrValueStatus.GetSuccess && start > end ) {
				status = EFilePrValueStatus.PrValueInvalid;
				errorParameterId = prefix + "_Start";
			}
		}

		static T ReadExternalAxisValue<T>( XElement root, string name, ref EFilePrValueStatus status,
			ref string errorParameterId, double ratio = 1 ) where T : struct
		{
			if( status != EFilePrValueStatus.GetSuccess ) {
				return default( T );
			}
			T value = default( T );
			if( typeof( T ) == typeof( double ) ) {
				XElement node = root.Elements( "Pr" ).FirstOrDefault( element => (string)element.Attribute( "ID" ) == name );
				if( node == null ) {
					status = EFilePrValueStatus.PrNodeMissing;
				}
				else if( double.TryParse( (string)node.Attribute( "Value" ), NumberStyles.Float, CultureInfo.InvariantCulture, out double number ) ) {
					number *= ratio;
					if( double.IsNaN( number ) || double.IsInfinity( number ) ) {
						status = EFilePrValueStatus.PrValueInvalid;
					}
					else {
						value = (T)(object)number;
					}
				}
				else {
					status = EFilePrValueStatus.PrValueInvalid;
				}
			}
			else {
				value = GetPrValue<T>( root, name, out status, ratio );
				if( status == EFilePrValueStatus.GetSuccess && typeof( T ) == typeof( RotaryAxis ) && (RotaryAxis)(object)value == RotaryAxis.None ) {
					status = EFilePrValueStatus.PrValueInvalid;
				}
			}
			if( status != EFilePrValueStatus.GetSuccess ) {
				errorParameterId = name;
			}
			return value;
		}

		static T GetPrValue<T>( XElement parent, int nPrId, out EFilePrValueStatus prValueStatus, double dRatio = 1 ) where T : struct
		{
			return GetPrValue<T>( parent, nPrId.ToString(), out prValueStatus, dRatio );
		}

		// if input <T> is enum, return enum value ; if input <T> is int, return int value
		static T GetPrValue<T>( XElement parent, string szPrId, out EFilePrValueStatus prValueStatus, double dRatio = 1 ) where T : struct
		{
			prValueStatus = EFilePrValueStatus.GetSuccess;
			T defaultValue = new T();

			// get that pr node
			XElement prNode = parent.Elements( "Pr" ).FirstOrDefault( node => (string)node.Attribute( "ID" ) == szPrId );

			// can't find that node
			if( prNode == null ) {
				prValueStatus = EFilePrValueStatus.PrNodeMissing;
				return defaultValue;
			}

			// get node value
			string szPrValue = prNode.Attribute( "Value" )?.Value;

			// output is int
			if( typeof( T ) == typeof( int ) ) {
				if( int.TryParse( szPrValue, out int nAttribute ) ) {

					// set int as object then cast to T
					return (T)(object)( (int)( nAttribute * dRatio ) );
				}

				// turn int failed
				prValueStatus = EFilePrValueStatus.PrValueInvalid;
				return defaultValue;
			}

			// output is double
			if( typeof( T ) == typeof( double ) ) {
				if( double.TryParse( szPrValue, out double dAttribue ) ) {
					return (T)(object)( dAttribue * dRatio );
				}

				// turn int failed
				prValueStatus = EFilePrValueStatus.PrValueInvalid;
				return defaultValue;
			}

			// output is enum
			if( typeof( T ).IsEnum ) {

				// get enum 
				if( int.TryParse( szPrValue, out int nAttribute ) ) {
					if( Enum.IsDefined( typeof( T ), nAttribute ) ) {
						return (T)Enum.ToObject( typeof( T ), nAttribute );
					}
					prValueStatus = EFilePrValueStatus.PrEnumValueInvalid;
					return defaultValue;
				}
			}
			prValueStatus = EFilePrValueStatus.PrTypeInvalid;
			return defaultValue;
		}

		static bool ConvertXML2MachineTreeNodeSuccess( XElement rootElement, out MachineTreeNode machineTreeNode )
		{
			machineTreeNode = new MachineTreeNode( MachineComponentType.Base );
			if( rootElement == null ) {
				return false;
			}

			// use first layer <MachineTree> to find child layer <Base>
			XElement baseElement = rootElement.Element( "Child" );

			// no child node in machine tree
			if( baseElement == null ) {
				return false;
			}
			XAttribute typeAttribute = baseElement.Attribute( "Type" );

			// first layer child node must be Base type
			if( typeAttribute == null || !string.Equals( typeAttribute.Value, "Base", StringComparison.OrdinalIgnoreCase ) ) {
				return false;
			}

			// start from base node
			if( ConvertTreeChildElementToNode( baseElement, out machineTreeNode ) ) {
				return true;
			}
			return false;
		}

		// deserilize child node
		static bool ConvertTreeChildElementToNode( XElement xmlElement, out MachineTreeNode node )
		{
			node = new MachineTreeNode( MachineComponentType.Base );
			if( xmlElement == null ) {
				return false;
			}
			string szMachineNodeType = xmlElement.Attribute( "Type" )?.Value;

			// must get component type record in enum
			if( !Enum.TryParse( szMachineNodeType, out MachineComponentType component ) ) {
				return false;
			}
			node = new MachineTreeNode( component );
			foreach( var childElem in xmlElement.Elements( "Child" ) ) {

				if( ConvertTreeChildElementToNode( childElem, out MachineTreeNode childNode ) ) {
					node.AddChild( childNode );
				}
				else {
					return false;
				}
			}
			return true;
		}

		#endregion

		static readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "MachineDataXMLHelper" );

		const string MACHINE_DATA_XML_GENERATION_FAILED = "機台資料 XML 產生失敗：{0}";
	}
}
