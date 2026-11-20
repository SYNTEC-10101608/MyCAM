using System.Drawing;
using MyCAM.Data;
using OCC.Quantity;

namespace MyCAM.Editor
{
	/// <summary>
	/// The only definition of the tool vector interpolation colors (data colors): the 3D segment color of the
	/// tool vector editor and the legend swatch of ToolVectorDlg both come from here.
	/// </summary>
	internal static class ToolVecInterpolateColor
	{
		/// <summary>
		/// 3D color of the segments interpolated with the given type.
		/// </summary>
		/// <param name="interpType">Interpolation type.</param>
		/// <returns>OCCT named color; the normal color for an unknown type.</returns>
		public static Quantity_NameOfColor GetPathColor( EToolVecInterpolateType interpType )
		{
			switch( interpType ) {
				case EToolVecInterpolateType.VectorInterpolation:
					return Quantity_NameOfColor.Quantity_NOC_ORANGE;
				case EToolVecInterpolateType.TiltAngleInterpolation:
					return Quantity_NameOfColor.Quantity_NOC_CYAN1;
				case EToolVecInterpolateType.MasterNormalSlaveInterpolation:
					return Quantity_NameOfColor.Quantity_NOC_YELLOW;
				case EToolVecInterpolateType.MasterInterpolationSlaveNormal:
					return Quantity_NameOfColor.Quantity_NOC_MAGENTA1;
				case EToolVecInterpolateType.Normal:
				default:
					return Quantity_NameOfColor.Quantity_NOC_BLUE;
			}
		}

		/// <summary>
		/// Legend swatch color of the given type, the same color as the 3D segments.
		/// </summary>
		/// <param name="interpType">Interpolation type.</param>
		/// <returns>Swatch color.</returns>
		public static Color GetSwatchColor( EToolVecInterpolateType interpType )
		{
			return DataColor.ToScreenColor( GetPathColor( interpType ) );
		}
	}
}
