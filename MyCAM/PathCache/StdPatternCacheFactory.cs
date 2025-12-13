using MyCAM.Data;
using OCC.gp;
using System;

namespace MyCAM.PathCache
{
	internal interface IStdPatternCacheStrategy
	{
		IStdPatternCache CreatePathCache( gp_Ax3 refCoord, IStdPatternGeomData standardPatternGeomData, CraftData craftData );
	}

	internal class StdPatternCacheStrategy<TPathCache> : IStdPatternCacheStrategy
		where TPathCache : IStdPatternCache
	{
		readonly Func<gp_Ax3, IStdPatternGeomData, CraftData, TPathCache> m_PathCacheFactory;

		public StdPatternCacheStrategy( Func<gp_Ax3, IStdPatternGeomData, CraftData, TPathCache> pathCacheFactory )
		{
			m_PathCacheFactory = pathCacheFactory ?? throw new ArgumentNullException( nameof( pathCacheFactory ) );
		}

		public IStdPatternCache CreatePathCache( gp_Ax3 refCoord, IStdPatternGeomData standardPatternGeomData, CraftData craftData )
		{
			if( refCoord == null ) {
				throw new ArgumentNullException( nameof( refCoord ) );
			}
			if( standardPatternGeomData == null ) {
				throw new ArgumentNullException( nameof( standardPatternGeomData ) );
			}
			if( craftData == null ) {
				throw new ArgumentNullException( nameof( craftData ) );
			}
			return m_PathCacheFactory( refCoord, standardPatternGeomData, craftData );
		}
	}

	internal static class StdPatternCacheFactory
	{
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

		public static IStdPatternCacheStrategy GetStrategy( PathType pathType )
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

		public static IStdPatternCache CreatePathCache( gp_Ax3 refCoord, IStdPatternGeomData standardPatternGeomData, CraftData craftData )
		{
			if( standardPatternGeomData == null ) {
				throw new ArgumentNullException( nameof( standardPatternGeomData ) );
			}

			IStdPatternCacheStrategy strategy = GetStrategy( standardPatternGeomData.PathType );
			if( strategy == null ) {
				throw new ArgumentException( $"No strategy found for PathType: {standardPatternGeomData.PathType}" );
			}

			return strategy.CreatePathCache( refCoord, standardPatternGeomData, craftData );
		}
	}
}
