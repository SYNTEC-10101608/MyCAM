using MyCAM.App;
using MyCAM.Data;
using MyCAM.Post;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	internal static class CollisionHelper
	{
		internal static bool CalCollisionResult( int frameCount, SimulationRequiredData calNeedData, Dictionary<MachineComponentType, List<gp_Trsf>> FrameTransformMap, out Dictionary<MachineComponentType, List<bool>> frameCollisionMap )
		{
			bool bCollisionOk = BuildCollision( frameCount, calNeedData.ChainListMap, calNeedData.FCLTest, FrameTransformMap, out frameCollisionMap );
			return bCollisionOk;
		}

		static bool BuildCollision( int FrameCount, Dictionary<MachineComponentType, List<MachineComponentType>> ChainListMap,
			CollisionSolver FCLTest, Dictionary<MachineComponentType, List<gp_Trsf>> FrameTransformMap,
			out Dictionary<MachineComponentType, List<bool>> FrameCollisionMap )
		{
			FrameCollisionMap = new Dictionary<MachineComponentType, List<bool>>();

			// init frame collision map
			FrameCollisionMap[ MachineComponentType.Base ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.XAxis ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.YAxis ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.ZAxis ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.Master ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.Slave ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.Laser ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.WorkPiece ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.Tool ] = new List<bool>();
			for( int i = 0; i < FrameCount; i++ ) {
				BuildPerFrameCollision( i, ChainListMap, FCLTest, FrameTransformMap, ref FrameCollisionMap );
			}
			//BuildWorkPieceAndTool( FrameCount, FCLTest, FrameTransformMap, ref FrameCollisionMap );
			return true;
		}

		static void BuildPerFrameCollision( int FrameIndex, Dictionary<MachineComponentType, List<MachineComponentType>> ChainListMap,
			CollisionSolver FCLTest, Dictionary<MachineComponentType, List<gp_Trsf>> FrameTransformMap, ref Dictionary<MachineComponentType,
			List<bool>> FrameCollisionMap )
		{
			try {
				// set collision
				HashSet<MachineComponentType> collisionResiltSet = new HashSet<MachineComponentType>();
				foreach( var compT in ChainListMap[ MachineComponentType.Laser ] ) {
					if( compT == MachineComponentType.Base ) {
						continue; // skip base
					}
					List<MachineComponentType> compWList = new List<MachineComponentType>( ChainListMap[ MachineComponentType.WorkPiece ] );
					compWList.Add( MachineComponentType.WorkPiece ); // add workpiece itself
					foreach( var compW in compWList ) {
						if( compW == MachineComponentType.Base ) {
							continue; // skip base
						}
						if( FrameTransformMap.TryGetValue( compT, out var compTList ) && FrameTransformMap.TryGetValue( compW, out var comWList )
							&& compTList.Count > FrameIndex && comWList.Count > FrameIndex ) {
							if( FCLTest.CheckCollision( compT.ToString(), compW.ToString(), ConvertTransform( compTList[ FrameIndex ] ), ConvertTransform( comWList[ FrameIndex ] ) ) ) {
								collisionResiltSet.Add( compT );
								collisionResiltSet.Add( compW );
							}
						}
					}
				}
				FrameCollisionMap[ MachineComponentType.XAxis ].Add( collisionResiltSet.Contains( MachineComponentType.XAxis ) );
				FrameCollisionMap[ MachineComponentType.YAxis ].Add( collisionResiltSet.Contains( MachineComponentType.YAxis ) );
				FrameCollisionMap[ MachineComponentType.ZAxis ].Add( collisionResiltSet.Contains( MachineComponentType.ZAxis ) );
				FrameCollisionMap[ MachineComponentType.Master ].Add( collisionResiltSet.Contains( MachineComponentType.Master ) );
				FrameCollisionMap[ MachineComponentType.Slave ].Add( collisionResiltSet.Contains( MachineComponentType.Slave ) );
				FrameCollisionMap[ MachineComponentType.Tool ].Add( collisionResiltSet.Contains( MachineComponentType.Tool ) );
				FrameCollisionMap[ MachineComponentType.WorkPiece ].Add( collisionResiltSet.Contains( MachineComponentType.WorkPiece ) );
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"模擬第{FrameIndex}禎運算失敗" + ex, MyApp.NoticeType.Warning );
				return;
			}
		}

		static void BuildWorkPieceAndTool( int FrameCount, CollisionSolver FCLTest, Dictionary<MachineComponentType, List<gp_Trsf>> FrameTransformMap, ref Dictionary<MachineComponentType, List<bool>> FrameCollisionMap )
		{
			for( int i = 0; i < FrameCount; i++ ) {
				try {
					// set collision
					HashSet<MachineComponentType> collisionResiltSet = new HashSet<MachineComponentType>();
					if( FrameTransformMap.TryGetValue( MachineComponentType.Tool, out var compTList ) && FrameTransformMap.TryGetValue( MachineComponentType.WorkPiece, out var comWList )
						&& compTList.Count >= FrameCount && comWList.Count >= FrameCount ) {
						if( FCLTest.CheckCollision( MachineComponentType.Tool.ToString(), MachineComponentType.WorkPiece.ToString(),
							ConvertTransform( compTList[ i ] ),
							ConvertTransform( comWList[ i ] ) ) ) {
							collisionResiltSet.Add( MachineComponentType.Tool );
							collisionResiltSet.Add( MachineComponentType.WorkPiece );
						}
					}
					FrameCollisionMap[ MachineComponentType.WorkPiece ].Add( collisionResiltSet.Contains( MachineComponentType.WorkPiece ) );

					if( collisionResiltSet.Contains( MachineComponentType.Tool ) ) {
						List<bool> boolList = FrameCollisionMap[ MachineComponentType.Tool ];
						boolList[ i ] = true;
					}
				}
				catch( Exception ex ) {
					MyApp.Logger.ShowOnLogPanel( $"模擬第{i}禎運算失敗" + ex, MyApp.NoticeType.Warning );
					return;
				}
			}
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
	}
}
