using System;
using System.Collections.Generic;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Post;
using OCC.gp;

namespace MyCAM.Helper
{
	internal static class CollisionHelper
	{
		static readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "SimuEditor" );

		internal static (double[] Min, double[] Max)? BuildLocalAABB( double[] vertices )
		{
			if( vertices == null || vertices.Length < 3 ) {
				return null;
			}
			double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
			double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
			int n = vertices.Length - ( vertices.Length % 3 );
			for( int i = 0; i < n; i += 3 ) {
				double x = vertices[ i ];
				double y = vertices[ i + 1 ];
				double z = vertices[ i + 2 ];
				if( x < minX ) {
					minX = x;
				}
				if( y < minY ) {
					minY = y;
				}
				if( z < minZ ) {
					minZ = z;
				}
				if( x > maxX ) {
					maxX = x;
				}
				if( y > maxY ) {
					maxY = y;
				}
				if( z > maxZ ) {
					maxZ = z;
				}
			}
			return (new[] { minX, minY, minZ }, new[] { maxX, maxY, maxZ });
		}

		internal static bool CalCollisionResult( int frameCount, SimuData.SimuRequiredData.SimuInputSet calNeedData, Dictionary<MachineComponentType, List<gp_Trsf>> FrameTransformMap,
			out Dictionary<MachineComponentType, List<bool>> frameCollisionMap, Action<LoadingBarInfo> reportProgress = null )
		{
			bool bCollisionOk = BuildCollision( frameCount, calNeedData.MachineData, calNeedData.MachineData.ChainListMap, calNeedData.CollisionEngine, calNeedData.AABBMap, FrameTransformMap, out frameCollisionMap, reportProgress );
			return bCollisionOk;
		}

		static bool BuildCollision( int FrameCount, IMachineDataBase MachineData, Dictionary<MachineComponentType, List<MachineComponentType>> ChainListMap,
			CollisionSolver FCLTest, Dictionary<string, (double[] Min, double[] Max)> AABBMap,
			Dictionary<MachineComponentType, List<gp_Trsf>> FrameTransformMap,
			out Dictionary<MachineComponentType, List<bool>> FrameCollisionMap,
			Action<LoadingBarInfo> reportProgress )
		{
			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.PrepareCollision ) );
			FrameCollisionMap = new Dictionary<MachineComponentType, List<bool>>();
			if( MachineData == null || ChainListMap == null || ChainListMap.Count == 0 ) {
				return false;
			}
			if( !ChainListMap.ContainsKey( MachineData.ToolChainLeaf ) || !ChainListMap.ContainsKey( MachineData.WorkPieceChainLeaf ) ) {
				return false;
			}

			// index in this list is the buffer index order 
			IReadOnlyList<MachineComponentType> componentTypes = MachineData.ComponentTypes;
			int componentCount = componentTypes.Count;

			// component type -> buffer index
			Dictionary<MachineComponentType, int> indexMap = new Dictionary<MachineComponentType, int>( componentCount );
			for( int i = 0; i < componentCount; i++ ) {
				indexMap[ componentTypes[ i ] ] = i;
			}

			// frameTrsfByIdx[componentIdx][frameIndex] = gp_Trsf
			List<gp_Trsf>[] frameTrsfByIdx = new List<gp_Trsf>[ componentCount ];

			// each component name
			string[] componentNameByIdx = new string[ componentCount ];

			// [ComponentIdx][boxSize]
			(double[] Min, double[] Max)?[] aabbByIdx = new (double[] Min, double[] Max)?[ componentCount ];
			for( int i = 0; i < componentCount; i++ ) {
				MachineComponentType type = componentTypes[ i ];
				string name = type.ToString();
				componentNameByIdx[ i ] = name;
				FrameTransformMap.TryGetValue( type, out frameTrsfByIdx[ i ] );
				if( AABBMap != null && AABBMap.TryGetValue( name, out var box ) ) {
					aabbByIdx[ i ] = box;
				}
			}

			// toolChainIdx = [componentIdx, componentIdx, componentIdx, ..]
			int[] toolChainIdx = FlattenChain( ChainListMap[ MachineData.ToolChainLeaf ], indexMap );
			List<MachineComponentType> workChain = new List<MachineComponentType>( ChainListMap[ MachineData.WorkPieceChainLeaf ] );

			// add workpiece itself
			workChain.Add( MachineData.WorkPieceChainLeaf );

			//  workChainIdx = [componentIdx, componentIdx, componentIdx, ..]
			int[] workChainIdx = FlattenChain( workChain, indexMap );

			// get the end of tool chain component idx
			int toolLeafIdx = indexMap.TryGetValue( MachineData.ToolChainLeaf, out int leafIdx ) ? leafIdx : -1;

			// collisionByIdx[componentIdx] = [false, false, true,  ...]  
			List<bool>[] collisionByIdx = new List<bool>[ componentCount ];
			for( int i = 0; i < componentCount; i++ ) {
				collisionByIdx[ i ] = new List<bool>( FrameCount );
			}

			// each component trsf matrix => trsfCache[componentIdx] = double[12]  
			double[][] trsfCache = new double[ componentCount ][];

			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.CheckCollision, 0, FrameCount ) );
			for( int i = 0; i < FrameCount; i++ ) {
				BuildPerFrameCollision( i, toolChainIdx, workChainIdx, toolLeafIdx, FCLTest,
					aabbByIdx, componentNameByIdx, frameTrsfByIdx, trsfCache, collisionByIdx );

				// Report progress to the loading bar.
				reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.CheckCollision, i + 1, FrameCount ) );
			}

			// build output Dictionary once
			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.CollectCollisionResults, 0, componentCount ) );
			for( int i = 0; i < componentCount; i++ ) {
				FrameCollisionMap[ componentTypes[ i ] ] = collisionByIdx[ i ];

				// Report progress to the loading bar.
				reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.CollectCollisionResults, i + 1, componentCount ) );
			}
			return true;
		}

		// flatten a component chain into buffer index array; components not in the simulated list ( e.g. Base ) are skipped
		static int[] FlattenChain( List<MachineComponentType> chain, Dictionary<MachineComponentType, int> indexMap )
		{
			List<int> result = new List<int>( chain.Count );
			foreach( MachineComponentType comp in chain ) {
				if( indexMap.TryGetValue( comp, out int idx ) ) {
					result.Add( idx );
				}
			}
			return result.ToArray();
		}

		static void BuildPerFrameCollision( int FrameIndex, int[] ToolChainIdx, int[] WorkChainIdx, int ToolLeafIdx,
			CollisionSolver FCLTest, (double[] Min, double[] Max)?[] AabbByIdx, string[] ComponentNameByIdx,
			List<gp_Trsf>[] FrameTrsfByIdx, double[][] TrsfCache, List<bool>[] CollisionByIdx )
		{
			try {

				// reset cache
				Array.Clear( TrsfCache, 0, TrsfCache.Length );

				// to record this frame collision component
				HashSet<int> collisionResultSet = new HashSet<int>();

				// tool chain
				foreach( int tIdx in ToolChainIdx ) {

					// cache compT transform once per frame
					double[] trsfToolSideComponent = GetFrameTrsf( tIdx, FrameIndex, FrameTrsfByIdx, TrsfCache );
					if( trsfToolSideComponent == null ) {
						continue;
					}

					// work piece chain
					foreach( int wIdx in WorkChainIdx ) {
						double[] trsfWorkpieceSide = GetFrameTrsf( wIdx, FrameIndex, FrameTrsfByIdx, TrsfCache );
						if( trsfWorkpieceSide == null ) {
							continue;
						}

						// skip the expensive native FCL call if the world AABBs are disjoint
						if( !MayCollideByAABB( AabbByIdx[ tIdx ], AabbByIdx[ wIdx ], trsfToolSideComponent, trsfWorkpieceSide ) ) {
							continue;
						}
						if( FCLTest.CheckCollision( ComponentNameByIdx[ tIdx ], ComponentNameByIdx[ wIdx ], trsfToolSideComponent, trsfWorkpieceSide ) ) {

							// record this frame collision component
							collisionResultSet.Add( tIdx );
							collisionResultSet.Add( wIdx );
						}
					}
				}

				// record result for every component except the tool leaf ( tool contacts the workpiece by design )
				for( int i = 0; i < CollisionByIdx.Length; i++ ) {
					if( i == ToolLeafIdx ) {
						continue;
					}
					CollisionByIdx[ i ].Add( collisionResultSet.Contains( i ) );
				}
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "SimuFrameCalcFailed", SIMU_FRAME_CALC_FAILED ), FrameIndex ) + ex, MyApp.NoticeType.Warning );

				// keep every component list length consistent with frame count even if this frame failed
				for( int i = 0; i < CollisionByIdx.Length; i++ ) {
					if( i == ToolLeafIdx ) {
						continue;
					}
					if( CollisionByIdx[ i ].Count <= FrameIndex ) {
						CollisionByIdx[ i ].Add( false );
					}
				}
				return;
			}
		}

		// converted world transform of a component at a frame; cached once per frame, null when unavailable
		static double[] GetFrameTrsf( int idx, int frameIndex, List<gp_Trsf>[] frameTrsfByIdx, double[][] trsfCache )
		{
			if( idx < 0 ) {
				return null;
			}
			if( trsfCache[ idx ] != null ) {
				return trsfCache[ idx ];
			}
			List<gp_Trsf> componentTrsfList = frameTrsfByIdx[ idx ];
			if( componentTrsfList == null || componentTrsfList.Count <= frameIndex ) {
				return null;
			}
			double[] trsf = ConvertTransform( componentTrsfList[ frameIndex ] );
			trsfCache[ idx ] = trsf;
			return trsf;
		}

		static double[] ConvertTransform( gp_Trsf trsf )
		{
			gp_Mat matR = trsf.GetRotation().GetMatrix();
			gp_XYZ vecT = trsf.TranslationPart();
			double[] result = new double[ 12 ];

			// the rotation part
			result[ 0 ] = matR.Value( 1, 1 );
			result[ 1 ] = matR.Value( 1, 2 );
			result[ 2 ] = matR.Value( 1, 3 );
			result[ 3 ] = matR.Value( 2, 1 );
			result[ 4 ] = matR.Value( 2, 2 );
			result[ 5 ] = matR.Value( 2, 3 );
			result[ 6 ] = matR.Value( 3, 1 );
			result[ 7 ] = matR.Value( 3, 2 );
			result[ 8 ] = matR.Value( 3, 3 );

			// the translation part
			result[ 9 ] = vecT.X();
			result[ 10 ] = vecT.Y();
			result[ 11 ] = vecT.Z();
			return result;
		}

		#region AABB Cal

		static void ComputeWorldAABB( double[] localMin, double[] localMax, double[] trsfMatrix,
			out double minX, out double minY, out double minZ,
			out double maxX, out double maxY, out double maxZ )
		{
			minX = double.MaxValue;
			minY = double.MaxValue;
			minZ = double.MaxValue;
			maxX = double.MinValue;
			maxY = double.MinValue;
			maxZ = double.MinValue;
			double[] xs = { localMin[ 0 ], localMax[ 0 ] };
			double[] ys = { localMin[ 1 ], localMax[ 1 ] };
			double[] zs = { localMin[ 2 ], localMax[ 2 ] };
			for( int i = 0; i < 2; i++ ) {
				for( int j = 0; j < 2; j++ ) {
					for( int k = 0; k < 2; k++ ) {
						double x = xs[ i ], y = ys[ j ], z = zs[ k ];
						double wx = trsfMatrix[ 0 ] * x + trsfMatrix[ 1 ] * y + trsfMatrix[ 2 ] * z + trsfMatrix[ 9 ];
						double wy = trsfMatrix[ 3 ] * x + trsfMatrix[ 4 ] * y + trsfMatrix[ 5 ] * z + trsfMatrix[ 10 ];
						double wz = trsfMatrix[ 6 ] * x + trsfMatrix[ 7 ] * y + trsfMatrix[ 8 ] * z + trsfMatrix[ 11 ];
						if( wx < minX ) {
							minX = wx;
						}
						if( wy < minY ) {
							minY = wy;
						}
						if( wz < minZ ) {
							minZ = wz;
						}
						if( wx > maxX ) {
							maxX = wx;
						}
						if( wy > maxY ) {
							maxY = wy;
						}
						if( wz > maxZ ) {
							maxZ = wz;
						}
					}
				}
			}
		}

		static bool MayCollideByAABB( (double[] Min, double[] Max)? box1, (double[] Min, double[] Max)? box2,
			double[] trsf1, double[] trsf2 )
		{
			if( box1 == null || box2 == null ) {
				return true;
			}
			var a = box1.Value;
			var b = box2.Value;
			ComputeWorldAABB( a.Min, a.Max, trsf1, out double aMinX, out double aMinY, out double aMinZ, out double aMaxX, out double aMaxY, out double aMaxZ );
			ComputeWorldAABB( b.Min, b.Max, trsf2, out double bMinX, out double bMinY, out double bMinZ, out double bMaxX, out double bMaxY, out double bMaxZ );
			if( aMaxX < bMinX || bMaxX < aMinX ) {
				return false;
			}
			if( aMaxY < bMinY || bMaxY < aMinY ) {
				return false;
			}
			if( aMaxZ < bMinZ || bMaxZ < aMinZ ) {
				return false;
			}
			return true;
		}

		#endregion

		const string SIMU_FRAME_CALC_FAILED = "模擬第{0}禎運算失敗";
	}
}
