using MyCAM.Data;
using OCC.gp;
using System;

namespace MyCAM.PathCache
{
	internal static class PathCacheFactory
	{
		public static IContourCache CreateContourCache( IContourGeomData contourGeomData, CraftData craftData )
		{
			if( contourGeomData == null || craftData == null ) {
				throw new ArgumentNullException( "ConourCache construct parameters null." );
			}
			return new ContourCache( (ContourGeomData)contourGeomData, craftData );
		}

		public static IStdPatternCache CreateStdPatternCache( gp_Ax3 refCoord, IStdPatternGeomData stdPatternGeomData, CraftData craftData )
		{
			if( refCoord == null || stdPatternGeomData == null || craftData == null ) {
				throw new ArgumentNullException( "StdPatternCache construct parameters null." );
			}
			IStdPatternCacheStrategy strategy = GetStrategy( stdPatternGeomData.PathType );
			if( strategy == null ) {
				throw new ArgumentException( $"No strategy found for PathType: {stdPatternGeomData.PathType}" );
			}
			return strategy.CreatePathCache( refCoord, stdPatternGeomData, craftData );
		}

		static IStdPatternCacheStrategy GetStrategy( PathType pathType )
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
				case PathType.Contour:
				default:
					return null;
			}
		}

		static readonly IStdPatternCacheStrategy s_CircleStrategy =
			new StdPatternCacheStrategy<CircleCache>(
				( coord, geom, craft ) => new CircleCache( coord, geom, craft ) );
		static readonly IStdPatternCacheStrategy s_RectangleStrategy =
			new StdPatternCacheStrategy<RectangleCache>(
				( coord, geom, craft ) => new RectangleCache( coord, geom, craft ) );
		static readonly IStdPatternCacheStrategy s_RunwayStrategy =
			new StdPatternCacheStrategy<RunwayCache>(
				( coord, geom, craft ) => new RunwayCache( coord, geom, craft ) );
		static readonly IStdPatternCacheStrategy s_PolygonStrategy =
			new StdPatternCacheStrategy<PolygonCache>(
				( coord, geom, craft ) => new PolygonCache( coord, geom, craft ) );
	}

	internal interface IStdPatternCacheStrategy
	{
		IStdPatternCache CreatePathCache( gp_Ax3 refCoord, IStdPatternGeomData stdPatternGeomData, CraftData craftData );
	}

	internal class StdPatternCacheStrategy<TPathCache> : IStdPatternCacheStrategy
		where TPathCache : IStdPatternCache
	{
		readonly Func<gp_Ax3, IStdPatternGeomData, CraftData, TPathCache> m_PathCacheFactory;

		public StdPatternCacheStrategy( Func<gp_Ax3, IStdPatternGeomData, CraftData, TPathCache> pathCacheFactory )
		{
			m_PathCacheFactory = pathCacheFactory ?? throw new ArgumentNullException( nameof( pathCacheFactory ) );
		}

		public IStdPatternCache CreatePathCache( gp_Ax3 refCoord, IStdPatternGeomData stdPatternGeomData, CraftData craftData )
		{
			if( refCoord == null || stdPatternGeomData == null || craftData == null ) {
				throw new ArgumentNullException( "PathCache construct parameters null." );
			}
			return m_PathCacheFactory( refCoord, stdPatternGeomData, craftData );
		}
	}
}
