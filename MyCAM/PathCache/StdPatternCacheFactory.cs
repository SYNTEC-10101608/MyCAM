using MyCAM.Data;
using System;

namespace MyCAM.PathCache
{
	internal static class StdPatternCacheFactory
	{
		public static StdPatternCacheBase CreateStdPatternCache( IStdPatternGeomData stdPatternGeomData, CraftData craftData )
		{
			if( stdPatternGeomData == null || craftData == null ) {
				throw new ArgumentNullException( "StdPatternCache construct parameters null." );
			}
			IStdPatternCacheStrategy strategy = GetStrategy( stdPatternGeomData.PathType );
			if( strategy == null ) {
				throw new ArgumentException( $"No strategy found for PathType: {stdPatternGeomData.PathType}" );
			}
			return strategy.CreatePathCache( stdPatternGeomData, craftData );
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
				( geom, craft ) => new CircleCache( geom, craft ) );
		static readonly IStdPatternCacheStrategy s_RectangleStrategy =
			new StdPatternCacheStrategy<RectangleCache>(
				( geom, craft ) => new RectangleCache( geom, craft ) );
		static readonly IStdPatternCacheStrategy s_RunwayStrategy =
			new StdPatternCacheStrategy<RunwayCache>(
				( geom, craft ) => new RunwayCache( geom, craft ) );
		static readonly IStdPatternCacheStrategy s_PolygonStrategy =
			new StdPatternCacheStrategy<PolygonCache>(
				( geom, craft ) => new PolygonCache( geom, craft ) );
	}

	internal interface IStdPatternCacheStrategy
	{
		StdPatternCacheBase CreatePathCache( IStdPatternGeomData stdPatternGeomData, CraftData craftData );
	}

	internal class StdPatternCacheStrategy<TPathCache> : IStdPatternCacheStrategy
		where TPathCache : StdPatternCacheBase
	{
		readonly Func<IStdPatternGeomData, CraftData, TPathCache> m_PathCacheFactory;

		public StdPatternCacheStrategy( Func<IStdPatternGeomData, CraftData, TPathCache> pathCacheFactory )
		{
			m_PathCacheFactory = pathCacheFactory ?? throw new ArgumentNullException( nameof( pathCacheFactory ) );
		}

		public StdPatternCacheBase CreatePathCache( IStdPatternGeomData stdPatternGeomData, CraftData craftData )
		{
			if( stdPatternGeomData == null || craftData == null ) {
				throw new ArgumentNullException( "PathCache construct parameters null." );
			}
			return m_PathCacheFactory( stdPatternGeomData, craftData );
		}
	}
}
