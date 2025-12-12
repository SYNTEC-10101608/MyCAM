using MyCAM.Data;
using OCC.gp;
using System;

namespace MyCAM.CacheInfo
{
	internal interface IStandardPatternCacheInfoStrategy
	{
		IStandardPatternCacheInfo CreateCacheInfo( gp_Ax3 coordinateInfo, IStdPatternGeomData standardPatternGeomData, CraftData craftData );
	}

	internal class StandardPatternCacheInfoStrategy<TCacheInfo> : IStandardPatternCacheInfoStrategy
		where TCacheInfo : IStandardPatternCacheInfo
	{
		readonly Func<gp_Ax3, IStdPatternGeomData, CraftData, TCacheInfo> m_CacheInfoFactory;

		public StandardPatternCacheInfoStrategy( Func<gp_Ax3, IStdPatternGeomData, CraftData, TCacheInfo> cacheInfoFactory )
		{
			m_CacheInfoFactory = cacheInfoFactory ?? throw new ArgumentNullException( nameof( cacheInfoFactory ) );
		}

		public IStandardPatternCacheInfo CreateCacheInfo( gp_Ax3 coordinateInfo, IStdPatternGeomData standardPatternGeomData, CraftData craftData )
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

	internal static class StandardPatternCacheInfoFactory
	{
		static readonly IStandardPatternCacheInfoStrategy s_CircleStrategy =
			new StandardPatternCacheInfoStrategy<CircleCacheInfo>(
				( coord, geom, craft ) => new CircleCacheInfo( coord, geom, craft ) );
		static readonly IStandardPatternCacheInfoStrategy s_RectangleStrategy =
			new StandardPatternCacheInfoStrategy<RectangleCacheInfo>(
				( coord, geom, craft ) => new RectangleCacheInfo( coord, geom, craft ) );
		static readonly IStandardPatternCacheInfoStrategy s_RunwayStrategy =
			new StandardPatternCacheInfoStrategy<RunwayCacheInfo>(
				( coord, geom, craft ) => new RunwayCacheInfo( coord, geom, craft ) );
		static readonly IStandardPatternCacheInfoStrategy s_PolygonStrategy =
			new StandardPatternCacheInfoStrategy<PolygonCacheInfo>(
				( coord, geom, craft ) => new PolygonCacheInfo( coord, geom, craft ) );

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

		public static IStandardPatternCacheInfo CreateCacheInfo( gp_Ax3 coordinateInfo, IStdPatternGeomData standardPatternGeomData, CraftData craftData )
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
