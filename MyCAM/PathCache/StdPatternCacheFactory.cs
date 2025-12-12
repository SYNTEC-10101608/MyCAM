using MyCAM.Data;
using OCC.gp;
using System;

namespace MyCAM.PathCache
{
	internal interface IStandardPatternCacheInfoStrategy
	{
		IStdPatternCache CreateCacheInfo( gp_Ax3 coordinateInfo, IStdPatternGeomData standardPatternGeomData, CraftData craftData );
	}

	internal class StandardPatternCacheInfoStrategy<TCacheInfo> : IStandardPatternCacheInfoStrategy
		where TCacheInfo : IStdPatternCache
	{
		readonly Func<gp_Ax3, IStdPatternGeomData, CraftData, TCacheInfo> m_CacheInfoFactory;

		public StandardPatternCacheInfoStrategy( Func<gp_Ax3, IStdPatternGeomData, CraftData, TCacheInfo> cacheInfoFactory )
		{
			m_CacheInfoFactory = cacheInfoFactory ?? throw new ArgumentNullException( nameof( cacheInfoFactory ) );
		}

		public IStdPatternCache CreateCacheInfo( gp_Ax3 coordinateInfo, IStdPatternGeomData standardPatternGeomData, CraftData craftData )
		{
			if( coordinateInfo == null ) {
				throw new ArgumentNullException( nameof( coordinateInfo ) );
			}
			if( standardPatternGeomData == null ) {
				throw new ArgumentNullException( nameof( standardPatternGeomData ) );
			}
			if( craftData == null ) {
				throw new ArgumentNullException( nameof( craftData ) );
			}
			return m_CacheInfoFactory( coordinateInfo, standardPatternGeomData, craftData );
		}
	}

	internal static class StdPatternCacheFactory
	{
		static readonly IStandardPatternCacheInfoStrategy s_CircleStrategy =
			new StandardPatternCacheInfoStrategy<CircleCache>(
				( coord, geom, craft ) => new CircleCache( coord, geom, craft ) );
		static readonly IStandardPatternCacheInfoStrategy s_RectangleStrategy =
			new StandardPatternCacheInfoStrategy<RectangleCache>(
				( coord, geom, craft ) => new RectangleCache( coord, geom, craft ) );
		static readonly IStandardPatternCacheInfoStrategy s_RunwayStrategy =
			new StandardPatternCacheInfoStrategy<RunwayCache>(
				( coord, geom, craft ) => new RunwayCache( coord, geom, craft ) );
		static readonly IStandardPatternCacheInfoStrategy s_PolygonStrategy =
			new StandardPatternCacheInfoStrategy<PolygonCache>(
				( coord, geom, craft ) => new PolygonCache( coord, geom, craft ) );

		public static IStandardPatternCacheInfoStrategy GetStrategy( PathType pathType )
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

		public static IStdPatternCache CreateCacheInfo( gp_Ax3 coordinateInfo, IStdPatternGeomData standardPatternGeomData, CraftData craftData )
		{
			if( standardPatternGeomData == null ) {
				throw new ArgumentNullException( nameof( standardPatternGeomData ) );
			}

			IStandardPatternCacheInfoStrategy strategy = GetStrategy( standardPatternGeomData.PathType );
			if( strategy == null ) {
				throw new ArgumentException( $"No strategy found for PathType: {standardPatternGeomData.PathType}" );
			}

			return strategy.CreateCacheInfo( coordinateInfo, standardPatternGeomData, craftData );
		}
	}
}
