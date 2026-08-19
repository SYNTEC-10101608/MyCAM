using System.Collections.Generic;

namespace MyCAM.Data
{
	// build the chain structures ( ChainListMap / WorkPieceChain ) of a machine data from its RootNode
	internal static class MachineChainBuilder
	{
		public static void Build( IMachineDataBase machineData )
		{
			if( machineData == null ) {
				return;
			}

			// replace RootNode with the product-specific default tree when needed ( e.g. partial simulation )
			if( machineData.NeedBuildDefaultTree() ) {
				machineData.RootNode = machineData.BuildDefaultTree();
			}

			//build ChainListMap from RootNode
			Dictionary<MachineComponentType, List<MachineComponentType>> chainListMap = new Dictionary<MachineComponentType, List<MachineComponentType>>();
			BuildMachineChainList( machineData.RootNode, new List<MachineComponentType>(), chainListMap );
			machineData.ChainListMap = chainListMap;

			// build WorkPieceChain from ChainListMap
			machineData.WorkPieceChain = BuildWorkpieceChain( chainListMap, machineData.WorkPieceChainLeaf );
		}

		// root node should be base node
		static void BuildMachineChainList( MachineTreeNode root, List<MachineComponentType> chainList, Dictionary<MachineComponentType, List<MachineComponentType>> chainListMap )
		{
			if( root == null ) {
				return;
			}
			chainListMap[ root.Type ] = chainList;
			foreach( MachineTreeNode child in root.Children ) {
				BuildMachineChainList( child, new List<MachineComponentType>( chainList ) { root.Type }, chainListMap );
			}
		}

		static HashSet<MachineComponentType> BuildWorkpieceChain( Dictionary<MachineComponentType, List<MachineComponentType>> chainListMap, MachineComponentType workPieceChainLeaf )
		{
			HashSet<MachineComponentType> workPieceChain = new HashSet<MachineComponentType>();

			// protection
			if( chainListMap == null || chainListMap.ContainsKey( workPieceChainLeaf ) == false ) {
				return workPieceChain;
			}
			foreach( MachineComponentType type in chainListMap[ workPieceChainLeaf ] ) {
				workPieceChain.Add( type );
			}
			return workPieceChain;
		}
	}
}
