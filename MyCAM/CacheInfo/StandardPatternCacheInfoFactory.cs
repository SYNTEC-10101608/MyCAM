using MyCAM.Data;
using MyCAM.Data.GeomDataFolder;
using OCC.gp;
using System;

namespace MyCAM.CacheInfo
{
	/// <summary>
	/// Strategy interface for creating CacheInfo objects
	/// </summary>
	internal interface IStandardPatternCacheInfoStrategy
	{
		IStandardPatternCacheInfo CreateCacheInfo( gp_Ax3 coordinateInfo, IStandardPatternGeomData standardPatternGeomData, CraftData craftData );
	}

	/// <summary>
	/// Generic strategy implementation for creating specific CacheInfo types
	/// </summary>
	internal class StandardPatternCacheInfoStrategy<TCacheInfo> : IStandardPatternCacheInfoStrategy
		where TCacheInfo : class, IStandardPatternCacheInfo
	{
		readonly Func<gp_Ax3, IStandardPatternGeomData, CraftData, TCacheInfo> m_CacheInfoFactory;

		public StandardPatternCacheInfoStrategy( Func<gp_Ax3, IStandardPatternGeomData, CraftData, TCacheInfo> cacheInfoFactory )
		{
			m_CacheInfoFactory = cacheInfoFactory ?? throw new ArgumentNullException( nameof( cacheInfoFactory ) );
		}

		public IStandardPatternCacheInfo CreateCacheInfo( gp_Ax3 coordinateInfo, IStandardPatternGeomData standardPatternGeomData, CraftData craftData )
		{
			if( coordinateInfo == null )
				throw new ArgumentNullException( nameof( coordinateInfo ) );
			if( standardPatternGeomData == null )
				throw new ArgumentNullException( nameof( standardPatternGeomData ) );
			if( craftData == null )
				throw new ArgumentNullException( nameof( craftData ) );

			return m_CacheInfoFactory( coordinateInfo, standardPatternGeomData, craftData );
		}
	}

	/// <summary>
	/// Factory for creating standard pattern CacheInfo objects
	/// Uses strategy pattern to delegate creation based on PathType
	/// External callers only need to provide: gp_Ax3, IStandardPatternGeomData, CraftData
	/// Returns IStandardPatternCacheInfo interface
	/// </summary>
	internal static class StandardPatternCacheInfoFactory
	{
		// Strategy instances for each pattern type
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

		/// <summary>
		/// Gets the appropriate strategy based on PathType
		/// </summary>
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

		/// <summary>
		/// Unified entry point for creating CacheInfo
		/// Automatically determines the correct strategy based on PathType
		/// Returns IStandardPatternCacheInfo interface for type safety
		/// </summary>
		public static IStandardPatternCacheInfo CreateCacheInfo( gp_Ax3 coordinateInfo, IStandardPatternGeomData standardPatternGeomData, CraftData craftData )
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
