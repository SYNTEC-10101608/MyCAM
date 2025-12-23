using MyCAM.Data;
using MyCAM.FileManager;
using System;
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

		internal static XDocument ConvertMachineDataAndTree2XML( MachineData machineData )
		{
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
			int machineTypeValue;
			if( machineDataDTO is SpindleTypeMachineDataDTO ) {
				machineTypeValue = 1;
			}
			else if( machineDataDTO is TableTypeMachineDataDTO ) {
				machineTypeValue = 2;
			}
			else if( machineDataDTO is MixTypeMachineDataDTO ) {
				machineTypeValue = 3;
			}
			else {
				throw new ArgumentException( "Unsupported machine configuration." );
			}
			ConvertPropertyToPrNode( root, (int)MachinePrValue.FiveAxisType, machineTypeValue );

			// common properties
			ConvertPropertyToPrNode( root, (int)MachinePrValue.ToolDirection, (int)machineDataDTO.ToolDirection );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.MasterRotaryAxis, (int)machineDataDTO.MasterRotaryAxis );
			ConvertPropertyToPrNode( root, (int)MachinePrValue.SlaveRotaryAxis, (int)machineDataDTO.SlaveRotaryAxis );
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
			return root;
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

		internal static MachineData ConvertMachineDataFileToMachineData( XDocument machineDataDoc, out EMachineDataLoadStatus status, out int nErrorPrIndex )
		{
			// default status
			status = EMachineDataLoadStatus.ReadSuccess;
			nErrorPrIndex = -1;

			// default machine data
			MachineData machineData = new MixTypeMachineData();
			if( machineDataDoc == null ) {
				status = EMachineDataLoadStatus.NullFile;
				return machineData;
			}
			XElement root = machineDataDoc.Root;
			if( root == null ) {
				status = EMachineDataLoadStatus.NullFile;
				return machineData;
			}

			// it contains machine data infomation
			XElement machineDataElement = root.Element( "MachineData" );

			// it contains machine tree infomation
			XElement machineTreeElement = root.Element( "MachineTree" );

			// can be no tree, because it is only used for simulation
			if( machineDataElement == null ) {
				status = EMachineDataLoadStatus.NullMachineDataNode;
				return machineData;
			}

			// turn maxhineData XML to DTO
			MachineDataDTOContainer container = ConvertXML2MachineData( machineDataElement, out EFilePrValueStatus prValueStatus, out nErrorPrIndex );
			if( prValueStatus != EFilePrValueStatus.GetSuccess ) {
				status = EMachineDataLoadStatus.InvalidPrValue;
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

		static MachineDataDTOContainer ConvertXML2MachineData( XElement machineData, out EFilePrValueStatus prValueStatus, out int nErrorPrIndex )
		{
			nErrorPrIndex = -1;
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
			return machineDTOContainer;
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
	}
}
