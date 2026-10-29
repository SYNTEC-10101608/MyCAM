using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper.CAM
{
	internal static class ExternalAxisHelper
	{
		public const double DEFAULT_GROUP_TOLERANCE_DEG = 1e-3;
		const double ROTATION_EPSILON_RAD = 1e-12;

		public static bool IsSameGroup( double externalAxisA_deg, double externalAxisB_deg, double tolerance_deg = DEFAULT_GROUP_TOLERANCE_DEG )
		{
			return Math.Abs( externalAxisA_deg - externalAxisB_deg ) <= tolerance_deg;
		}

		public static gp_Trsf CreateRotationTransformAroundG54Z( double externalAxis_deg )
		{
			gp_Trsf rotation = new gp_Trsf();
			double angle_rad = externalAxis_deg * Math.PI / 180.0;
			if( Math.Abs( angle_rad ) <= ROTATION_EPSILON_RAD ) {
				return rotation;
			}
			rotation.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) ), angle_rad );
			return rotation;
		}

		public static gp_Pnt RotatePointAroundG54Z( gp_Pnt point, double externalAxis_deg )
		{
			if( point == null ) {
				return null;
			}
			gp_Trsf rotation = CreateRotationTransformAroundG54Z( externalAxis_deg );
			return point.Transformed( rotation );
		}

		public static gp_Dir RotateDirAroundG54Z( gp_Dir dir, double externalAxis_deg )
		{
			if( dir == null ) {
				return null;
			}
			gp_Dir transformedDir = new gp_Dir( dir.XYZ() );
			gp_Trsf rotation = CreateRotationTransformAroundG54Z( externalAxis_deg );
			transformedDir.Transform( rotation );
			return transformedDir;
		}

		public static gp_Vec RotateVecAroundG54Z( gp_Vec vec, double externalAxis_deg )
		{
			if( vec == null ) {
				return null;
			}
			gp_Vec transformedVec = new gp_Vec( vec.XYZ() );
			gp_Trsf rotation = CreateRotationTransformAroundG54Z( externalAxis_deg );
			transformedVec.Transform( rotation );
			return transformedVec;
		}
	}

	internal static class ExternalAxisGroupHelper
	{
		public static List<List<string>> BuildContiguousGroups( IReadOnlyList<string> pathIDList, double tolerance_deg = ExternalAxisHelper.DEFAULT_GROUP_TOLERANCE_DEG )
		{
			List<List<string>> groups = new List<List<string>>();
			if( pathIDList == null || pathIDList.Count == 0 ) {
				return groups;
			}

			List<string> currentGroup = new List<string>();
			double? prevExternalAxis = null;
			foreach( string pathID in pathIDList ) {
				if( string.IsNullOrEmpty( pathID ) || !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					FinalizeGroup( groups, currentGroup );
					prevExternalAxis = null;
					continue;
				}

				double currentExternalAxis = craftData.ExternalAxis_deg;
				if( prevExternalAxis.HasValue && !ExternalAxisHelper.IsSameGroup( currentExternalAxis, prevExternalAxis.Value, tolerance_deg ) ) {
					FinalizeGroup( groups, currentGroup );
				}

				currentGroup.Add( pathID );
				prevExternalAxis = currentExternalAxis;
			}

			FinalizeGroup( groups, currentGroup );
			return groups;
		}

		static void FinalizeGroup( List<List<string>> groups, List<string> currentGroup )
		{
			if( currentGroup == null || currentGroup.Count == 0 ) {
				return;
			}
			groups.Add( new List<string>( currentGroup ) );
			currentGroup.Clear();
		}

	}
}
