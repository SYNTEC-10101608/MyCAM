using MyCAM.Data;
using System;
using System.IO;
using System.Text;

namespace MyCAM.Post
{
	internal interface IStdPatternNCStrategy
	{
		void WriteNCCode( StreamWriter writer, StdPatternPostData postData, CraftData craftData, IGeomData geomData, ITraverseWriter commandWriter );
	}

	internal abstract class StdPatternNCStrategyBase : IStdPatternNCStrategy
	{
		public void WriteNCCode( StreamWriter writer, StdPatternPostData postData, CraftData craftData, IGeomData geomData, ITraverseWriter commandWriter )
		{
			// Validate and cast geometry data
			if( !ValidateAndCastGeometry( geomData ) ) {
				throw new ArgumentException( $"Invalid geometry data type for {GetPatternName()} pattern" );
			}

			// Build common parameters
			string commonParams = BuildCommonParameters( postData, craftData, commandWriter );

			// Build pattern-specific parameters
			string specificParams = BuildSpecificParameters( geomData, craftData );

			// Write complete NC command
			writer.WriteLine( $"G65 P\"{GetSubroutineName()}\" {commonParams} {specificParams};" );
		}

		// validate and cast IGeomData to the expected type.
		protected abstract bool ValidateAndCastGeometry( IGeomData geomData );

		// build pattern-specific parameters (e.g., diameter for circle, width/length for rectangle).
		protected abstract string BuildSpecificParameters( IGeomData geomData, CraftData craftData );

		// get the subroutine name (e.g., "SY_CIRC", "SY_RECT")
		protected abstract string GetSubroutineName();

		// get pattern name for error messages.
		protected abstract string GetPatternName();

		// build common parameters shared by all standard patterns.
		protected string BuildCommonParameters( StdPatternPostData postData, CraftData craftData, ITraverseWriter commandWriter )
		{
			var sb = new StringBuilder();

			// Reference point (X/Y/Z)
			sb.Append( $"X{postData.RefPoint.X.ToString( "F3" )}" );
			sb.Append( $" Y{postData.RefPoint.Y.ToString( "F3" )}" );
			sb.Append( $" Z{postData.RefPoint.Z.ToString( "F3" )}" );

			// Rotary axes
			sb.Append( " " );
			sb.Append( commandWriter.GetRotaryAxisCommand(
				postData.RefPoint.Master / Math.PI * 180,
				postData.RefPoint.Slave / Math.PI * 180 ) );

			return sb.ToString();
		}

		// build common lead and craft parameters (E/R/Q/H/V).
		protected string BuildCommonLeadAndCraftParameters( CraftData craftData, double rotatedAngle_deg )
		{
			var sb = new StringBuilder();

			double linearLeadInLength = craftData.LeadData.LeadIn.StraightLength;
			double arcLeadOutLength = craftData.LeadData.LeadIn.ArcLength;

			sb.Append( $" E{linearLeadInLength.ToString( "F3" )}" );
			sb.Append( $" R{arcLeadOutLength.ToString( "F3" )}" );
			sb.Append( $" Q{rotatedAngle_deg.ToString( "F3" )}" );
			sb.Append( " H1" );
			sb.Append( $" V{craftData.OverCutLength.ToString( "F3" )}" );

			return sb.ToString();
		}
	}

	internal class CircleNCStrategy : StdPatternNCStrategyBase
	{
		protected override bool ValidateAndCastGeometry( IGeomData geomData )
		{
			return geomData is CircleGeomData;
		}

		protected override string BuildSpecificParameters( IGeomData geomData, CraftData craftData )
		{
			var circleGeomData = (CircleGeomData)geomData;
			var sb = new StringBuilder();

			sb.Append( $" D{circleGeomData.Diameter.ToString( "F3" )}" );
			sb.Append( BuildCommonLeadAndCraftParameters( craftData, circleGeomData.RotatedAngle_deg ) );

			return sb.ToString();
		}

		protected override string GetSubroutineName() => "SY_CIRC";

		protected override string GetPatternName() => "Circle";
	}

	internal class RectangleNCStrategy : StdPatternNCStrategyBase
	{
		protected override bool ValidateAndCastGeometry( IGeomData geomData )
		{
			return geomData is RectangleGeomData;
		}

		protected override string BuildSpecificParameters( IGeomData geomData, CraftData craftData )
		{
			var rectangleGeomData = (RectangleGeomData)geomData;
			var sb = new StringBuilder();

			sb.Append( $" U{rectangleGeomData.Length.ToString( "F3" )}" );
			sb.Append( $" W{rectangleGeomData.Width.ToString( "F3" )}" );
			sb.Append( $" D{rectangleGeomData.CornerRadius.ToString( "F3" )}" );
			sb.Append( $" T{( craftData.StartPointIndex + 1 )}" );
			sb.Append( BuildCommonLeadAndCraftParameters( craftData, rectangleGeomData.RotatedAngle_deg ) );

			return sb.ToString();
		}

		protected override string GetSubroutineName() => "SY_RECT";

		protected override string GetPatternName() => "Rectangle";
	}

	internal class RunwayNCStrategy : StdPatternNCStrategyBase
	{
		protected override bool ValidateAndCastGeometry( IGeomData geomData )
		{
			return geomData is RunwayGeomData;
		}

		protected override string BuildSpecificParameters( IGeomData geomData, CraftData craftData )
		{
			var runwayGeomData = (RunwayGeomData)geomData;
			var sb = new StringBuilder();

			sb.Append( $" U{runwayGeomData.Length.ToString( "F3" )}" );
			sb.Append( $" W{runwayGeomData.Width.ToString( "F3" )}" );
			sb.Append( $" T{( craftData.StartPointIndex + 1 )}" );
			sb.Append( BuildCommonLeadAndCraftParameters( craftData, runwayGeomData.RotatedAngle_deg ) );

			return sb.ToString();
		}

		protected override string GetSubroutineName() => "SY_RUNWAY";

		protected override string GetPatternName() => "Runway";
	}

	internal class PolygonNCStrategy : StdPatternNCStrategyBase
	{
		protected override bool ValidateAndCastGeometry( IGeomData geomData )
		{
			return geomData is PolygonGeomData;
		}

		protected override string BuildSpecificParameters( IGeomData geomData, CraftData craftData )
		{
			var polygonGeomData = (PolygonGeomData)geomData;
			var sb = new StringBuilder();

			sb.Append( $" U{polygonGeomData.Sides.ToString()}" );
			sb.Append( $" W{polygonGeomData.SideLength.ToString( "F3" )}" );
			sb.Append( $" D{polygonGeomData.CornerRadius.ToString( "F3" )}" );
			sb.Append( $" T{( craftData.StartPointIndex + 1 )}" );
			sb.Append( BuildCommonLeadAndCraftParameters( craftData, polygonGeomData.RotatedAngle_deg ) );

			return sb.ToString();
		}

		protected override string GetSubroutineName() => "SY_POLYGON";

		protected override string GetPatternName() => "Polygon";
	}

	internal static class StandardPatternNCStrategyFactory
	{
		static readonly CircleNCStrategy s_CircleStrategy = new CircleNCStrategy();
		static readonly RectangleNCStrategy s_RectangleStrategy = new RectangleNCStrategy();
		static readonly RunwayNCStrategy s_RunwayStrategy = new RunwayNCStrategy();
		static readonly PolygonNCStrategy s_PolygonStrategy = new PolygonNCStrategy();

		public static IStdPatternNCStrategy GetStrategy( PathType pathType )
		{
			switch( pathType ) {
				case PathType.Circle:
					return s_CircleStrategy;
				case PathType.Rectangle:
					return s_RectangleStrategy;
				case PathType.Runway:
					return s_RunwayStrategy;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					return s_PolygonStrategy;
				default:
					throw new ArgumentException( $"Unsupported path type: {pathType}" );
			}
		}
	}

	internal static class StdPatternNCWriter
	{
		public static void WriteStandardPatternCutting( ITraverseWriter traverseWriter, PathType pathType, StdPatternPostData postData, CraftData craftData, IGeomData geomData, int nIndex )
		{
			if( traverseWriter == null || postData == null || craftData == null || geomData == null ) {
				throw new ArgumentNullException( "StandardPatternNCWriter arguments cannot be null" );
			}

			StreamWriter writer = traverseWriter.Writer;

			// write comment and N code
			writer.WriteLine( "// Cutting" + nIndex );
			writer.WriteLine( "N" + nIndex );

			// write traverse section
			TraverseWriterHelper.WriteTraverse( traverseWriter, postData );

			// get strategy and write cutting code
			IStdPatternNCStrategy strategy = StandardPatternNCStrategyFactory.GetStrategy( pathType );
			strategy.WriteNCCode( writer, postData, craftData, geomData, traverseWriter );
		}
	}
}
