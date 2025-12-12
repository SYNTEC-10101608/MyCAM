using MyCAM.Data;
using System;
using System.IO;

namespace MyCAM.Post
{
	internal interface IStandardPatternNCStrategy
	{
		void WriteNCCode( StreamWriter writer, StandardPatternPostData postData, CraftData craftData, IGeomData geomData, Func<double, double, string> getRotaryAxisCommand );
	}

	internal class CircleNCStrategy : IStandardPatternNCStrategy
	{
		public void WriteNCCode( StreamWriter writer, StandardPatternPostData postData, CraftData craftData, IGeomData geomData, Func<double, double, string> getRotaryAxisCommand )
		{
			CircleGeomData circleGeomData = geomData as CircleGeomData;
			if( circleGeomData == null ) {
				throw new ArgumentException( "Invalid geometry data type for Circle pattern" );
			}

			double linearLeadInLength = craftData.LeadLineParam.LeadIn.Type == LeadLineType.Line ? craftData.LeadLineParam.LeadIn.Length : 0;
			double arcLeadOutLength = craftData.LeadLineParam.LeadIn.Type == LeadLineType.Arc ? craftData.LeadLineParam.LeadIn.Length : 0;

			writer.WriteLine( "G65 P\"SY_CIRC\"" +
				" X" + postData.RefPoint.X.ToString( "F3" ) +
				" Y" + postData.RefPoint.Y.ToString( "F3" ) +
				" Z" + postData.RefPoint.Z.ToString( "F3" ) +
				" " + getRotaryAxisCommand( postData.RefPoint.Master / Math.PI * 180, postData.RefPoint.Slave / Math.PI * 180 ) +
				" D" + circleGeomData.Diameter.ToString( "F3" ) +
				" E" + linearLeadInLength.ToString( "F3" ) +
				" R" + arcLeadOutLength.ToString( "F3" ) +
				" Q" + circleGeomData.RotatedAngle_deg.ToString( "F3" ) +
				" H1" +
				" V" + craftData.OverCutLength.ToString( "F3" ) + ";" );
		}
	}

	internal class RectangleNCStrategy : IStandardPatternNCStrategy
	{
		public void WriteNCCode( StreamWriter writer, StandardPatternPostData postData, CraftData craftData, IGeomData geomData, Func<double, double, string> getRotaryAxisCommand )
		{
			RectangleGeomData rectangleGeomData = geomData as RectangleGeomData;
			if( rectangleGeomData == null ) {
				throw new ArgumentException( "Invalid geometry data type for Rectangle pattern" );
			}

			double linearLeadInLength = craftData.LeadLineParam.LeadIn.Type == LeadLineType.Line ? craftData.LeadLineParam.LeadIn.Length : 0;
			double arcLeadOutLength = craftData.LeadLineParam.LeadIn.Type == LeadLineType.Arc ? craftData.LeadLineParam.LeadIn.Length : 0;

			writer.WriteLine( "G65 P\"SY_RECT\"" +
				" X" + postData.RefPoint.X.ToString( "F3" ) +
				" Y" + postData.RefPoint.Y.ToString( "F3" ) +
				" Z" + postData.RefPoint.Z.ToString( "F3" ) +
				" " + getRotaryAxisCommand( postData.RefPoint.Master / Math.PI * 180, postData.RefPoint.Slave / Math.PI * 180 ) +
				" U" + rectangleGeomData.Length.ToString( "F3" ) +
				" W" + rectangleGeomData.Width.ToString( "F3" ) +
				" D" + rectangleGeomData.CornerRadius.ToString( "F3" ) +
				" T" + ( craftData.StartPointIndex + 1 ).ToString() +
				" E" + linearLeadInLength.ToString( "F3" ) +
				" R" + arcLeadOutLength.ToString( "F3" ) +
				" Q" + rectangleGeomData.RotatedAngle_deg.ToString( "F3" ) +
				" H1" +
				" V" + craftData.OverCutLength.ToString( "F3" ) + ";" );
		}
	}

	internal class RunwayNCStrategy : IStandardPatternNCStrategy
	{
		public void WriteNCCode( StreamWriter writer, StandardPatternPostData postData, CraftData craftData, IGeomData geomData, Func<double, double, string> getRotaryAxisCommand )
		{
			RunwayGeomData runwayGeomData = geomData as RunwayGeomData;
			if( runwayGeomData == null ) {
				throw new ArgumentException( "Invalid geometry data type for Runway pattern" );
			}

			double linearLeadInLength = craftData.LeadLineParam.LeadIn.Type == LeadLineType.Line ? craftData.LeadLineParam.LeadIn.Length : 0;
			double arcLeadOutLength = craftData.LeadLineParam.LeadIn.Type == LeadLineType.Arc ? craftData.LeadLineParam.LeadIn.Length : 0;

			writer.WriteLine( "G65 P\"SY_RUNWAY\"" +
				" X" + postData.RefPoint.X.ToString( "F3" ) +
				" Y" + postData.RefPoint.Y.ToString( "F3" ) +
				" Z" + postData.RefPoint.Z.ToString( "F3" ) +
				" " + getRotaryAxisCommand( postData.RefPoint.Master / Math.PI * 180, postData.RefPoint.Slave / Math.PI * 180 ) +
				" U" + runwayGeomData.Length.ToString( "F3" ) +
				" W" + runwayGeomData.Width.ToString( "F3" ) +
				" T" + ( craftData.StartPointIndex + 1 ).ToString() +
				" E" + linearLeadInLength.ToString( "F3" ) +
				" R" + arcLeadOutLength.ToString( "F3" ) +
				" Q" + runwayGeomData.RotatedAngle_deg.ToString( "F3" ) +
				" H1" +
				" V" + craftData.OverCutLength.ToString( "F3" ) + ";" );
		}
	}

	internal class PolygonNCStrategy : IStandardPatternNCStrategy
	{
		public void WriteNCCode( StreamWriter writer, StandardPatternPostData postData, CraftData craftData, IGeomData geomData, Func<double, double, string> getRotaryAxisCommand )
		{
			PolygonGeomData polygonGeomData = geomData as PolygonGeomData;
			if( polygonGeomData == null ) {
				throw new ArgumentException( "Invalid geometry data type for Polygon pattern" );
			}

			double linearLeadInLength = craftData.LeadLineParam.LeadIn.Type == LeadLineType.Line ? craftData.LeadLineParam.LeadIn.Length : 0;
			double arcLeadOutLength = craftData.LeadLineParam.LeadIn.Type == LeadLineType.Arc ? craftData.LeadLineParam.LeadIn.Length : 0;

			writer.WriteLine( "G65 P\"SY_POLYGON\"" +
				" X" + postData.RefPoint.X.ToString( "F3" ) +
				" Y" + postData.RefPoint.Y.ToString( "F3" ) +
				" Z" + postData.RefPoint.Z.ToString( "F3" ) +
				" " + getRotaryAxisCommand( postData.RefPoint.Master / Math.PI * 180, postData.RefPoint.Slave / Math.PI * 180 ) +
				" U" + polygonGeomData.Sides.ToString() +
				" W" + polygonGeomData.SideLength.ToString( "F3" ) +
				" D" + polygonGeomData.CornerRadius.ToString( "F3" ) +
				" T" + ( craftData.StartPointIndex + 1 ).ToString() +
				" E" + linearLeadInLength.ToString( "F3" ) +
				" R" + arcLeadOutLength.ToString( "F3" ) +
				" Q" + polygonGeomData.RotatedAngle_deg.ToString( "F3" ) +
				" H1" +
				" V" + craftData.OverCutLength.ToString( "F3" ) + ";" );
		}
	}

	internal static class StandardPatternNCStrategyFactory
	{
		private static readonly CircleNCStrategy s_CircleStrategy = new CircleNCStrategy();
		private static readonly RectangleNCStrategy s_RectangleStrategy = new RectangleNCStrategy();
		private static readonly RunwayNCStrategy s_RunwayStrategy = new RunwayNCStrategy();
		private static readonly PolygonNCStrategy s_PolygonStrategy = new PolygonNCStrategy();

		public static IStandardPatternNCStrategy GetStrategy( PathType pathType )
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

	internal static class StandardPatternNCWriter
	{
		public static void WriteStandardPatternTraverse(
			StreamWriter writer,
			StandardPatternPostData postData,
			Action<StreamWriter, PostPoint, double> writeLinearTraverse,
			Action<StreamWriter, PostPoint, PostPoint, double> writeFrogLeap )
		{
			if( writer == null || postData == null ) {
				throw new ArgumentNullException( "WriteStandardPatternTraverse arguments cannot be null" );
			}

			// lift up
			if( postData.LiftUpPostPoint != null ) {
				writeLinearTraverse( writer, postData.LiftUpPostPoint, 0 );
			}

			// frog leap with cut down
			if( postData.FrogLeapMidPostPoint != null && postData.CutDownPostPoint != null ) {
				writeFrogLeap( writer, postData.FrogLeapMidPostPoint, postData.CutDownPostPoint, 0 );

				// cut down
				writeLinearTraverse( writer, postData.StartPoint, postData.FollowSafeDistance );
			}

			// form leap without cut down
			else if( postData.FrogLeapMidPostPoint != null && postData.CutDownPostPoint == null ) {
				writeFrogLeap( writer, postData.FrogLeapMidPostPoint, postData.StartPoint, postData.FollowSafeDistance );
			}

			// no frog leap
			else if( postData.FrogLeapMidPostPoint == null && postData.CutDownPostPoint != null ) {
				writeLinearTraverse( writer, postData.CutDownPostPoint, 0 );

				// cut down
				writeLinearTraverse( writer, postData.StartPoint, postData.FollowSafeDistance );
			}

			// no frog leap and no cut down
			else {
				writeLinearTraverse( writer, postData.StartPoint, postData.FollowSafeDistance );
			}
		}

		public static void WriteStandardPatternCutting(
			StreamWriter writer,
			PathType pathType,
			StandardPatternPostData postData,
			CraftData craftData,
			IGeomData geomData,
			int nIndex,
			Action<StreamWriter, PostPoint, double> writeLinearTraverse,
			Action<StreamWriter, PostPoint, PostPoint, double> writeFrogLeap,
			Func<double, double, string> getRotaryAxisCommand )
		{
			if( writer == null || postData == null || craftData == null || geomData == null ) {
				throw new ArgumentNullException( "StandardPatternNCWriter arguments cannot be null" );
			}

			// Write comment and N code
			writer.WriteLine( "// Cutting" + nIndex );
			writer.WriteLine( "N" + nIndex );

			// Write traverse section
			WriteStandardPatternTraverse( writer, postData, writeLinearTraverse, writeFrogLeap );

			// Get strategy and write cutting code
			IStandardPatternNCStrategy strategy = StandardPatternNCStrategyFactory.GetStrategy( pathType );
			strategy.WriteNCCode( writer, postData, craftData, geomData, getRotaryAxisCommand );
		}
	}
}
