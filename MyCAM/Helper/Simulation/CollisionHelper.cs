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

		internal static bool CalCollisionResult( int frameCount, SimuData.SimuRequiredData.SimuInputSet calNeedData, Dictionary<MachineComponentType, List<gp_Trsf>> FrameTransformMap, out Dictionary<MachineComponentType, List<bool>> frameCollisionMap )
		{
			bool bCollisionOk = BuildCollision( frameCount, calNeedData.ChainListMap, calNeedData.CollisionEngine, calNeedData.AABBMap, FrameTransformMap, out frameCollisionMap );
			return bCollisionOk;
		}

		static bool BuildCollision( int FrameCount, Dictionary<MachineComponentType, List<MachineComponentType>> ChainListMap,
			CollisionSolver FCLTest, Dictionary<string, (double[] Min, double[] Max)> AABBMap,
			Dictionary<MachineComponentType, List<gp_Trsf>> FrameTransformMap,
			out Dictionary<MachineComponentType, List<bool>> FrameCollisionMap )
		{
			FrameCollisionMap = new Dictionary<MachineComponentType, List<bool>>();
			if( ChainListMap == null || ChainListMap.Count == 0 ) {
				return false;
			}

			// init frame collision map
			FrameCollisionMap[ MachineComponentType.Base ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.XAxis ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.YAxis ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.ZAxis ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.Master ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.Slave ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.Laser ] = new List<bool>();
			FrameCollisionMap[ MachineComponentType.WorkPiece ] = new List<bool>();
			for( int i = 0; i < FrameCount; i++ ) {
				BuildPerFrameCollision( i, ChainListMap, FCLTest, AABBMap, FrameTransformMap, ref FrameCollisionMap );
			}
			return true;
		}

		static void BuildPerFrameCollision( int FrameIndex, Dictionary<MachineComponentType, List<MachineComponentType>> ChainListMap,
			CollisionSolver FCLTest, Dictionary<string, (double[] Min, double[] Max)> AabbMap,
			Dictionary<MachineComponentType, List<gp_Trsf>> FrameTransformMap, ref Dictionary<MachineComponentType, List<bool>> FrameCollisionMap )
		{
			try {
				// set collision
				HashSet<MachineComponentType> collisionResiltSet = new HashSet<MachineComponentType>();
				foreach( var compT in ChainListMap[ MachineComponentType.Laser ] ) {
					if( compT == MachineComponentType.Base ) {

						// skip base
						continue; 
					}
					List<MachineComponentType> compWList = new List<MachineComponentType>( ChainListMap[ MachineComponentType.WorkPiece ] );
					
					// add workpiece itself
					compWList.Add( MachineComponentType.WorkPiece );

					// cache compT transform once per frame (shared across all compW iterations).
					if( !FrameTransformMap.TryGetValue( compT, out var compTList ) || compTList.Count <= FrameIndex ) {
						continue;
					}
					string componentName_T = compT.ToString();
					double[] trsfT = ConvertTransform( compTList[ FrameIndex ] );

					foreach( var compW in compWList ) {
						if( !FrameTransformMap.TryGetValue( compW, out var comWList ) || comWList.Count <= FrameIndex ) {
							continue;
						}
						string componentName_W = compW.ToString();
						double[] trsfWorkpieceSide = ConvertTransform( comWList[ FrameIndex ] );

						//  skip the expensive native FCL call if the world AABBs are disjoint.
						if( !MayCollideByAABB( AabbMap, componentName_T, componentName_W, trsfT, trsfWorkpieceSide ) ) {
							continue;
						}
						if( FCLTest.CheckCollision( componentName_T, componentName_W, trsfT, trsfWorkpieceSide ) ) {
							collisionResiltSet.Add( compT );
							collisionResiltSet.Add( compW );
						}
					}
				}
				FrameCollisionMap[ MachineComponentType.XAxis ].Add( collisionResiltSet.Contains( MachineComponentType.XAxis ) );
				FrameCollisionMap[ MachineComponentType.YAxis ].Add( collisionResiltSet.Contains( MachineComponentType.YAxis ) );
				FrameCollisionMap[ MachineComponentType.ZAxis ].Add( collisionResiltSet.Contains( MachineComponentType.ZAxis ) );
				FrameCollisionMap[ MachineComponentType.Master ].Add( collisionResiltSet.Contains( MachineComponentType.Master ) );
				FrameCollisionMap[ MachineComponentType.Slave ].Add( collisionResiltSet.Contains( MachineComponentType.Slave ) );
				FrameCollisionMap[ MachineComponentType.WorkPiece ].Add( collisionResiltSet.Contains( MachineComponentType.WorkPiece ) );
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "SimuFrameCalcFailed", SIMU_FRAME_CALC_FAILED ), FrameIndex ) + ex, MyApp.NoticeType.Warning );
				return;
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

		static bool MayCollideByAABB( Dictionary<string, (double[] Min, double[] Max)> aabbMap,
			string id1, string id2, double[] trsf1, double[] trsf2 )
		{
			if( aabbMap == null ) {
				return true;
			}
			if( !aabbMap.TryGetValue( id1, out var a ) ) {
				return true;
			}
			if( !aabbMap.TryGetValue( id2, out var b ) ) {
				return true;
			}
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
