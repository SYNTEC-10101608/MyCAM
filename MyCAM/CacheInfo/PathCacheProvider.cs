using MyCAM.Data;
using MyCAM.Data.PathObjectFolder;
using System;
using System.Collections.Generic;

namespace MyCAM.CacheInfo
{
	public interface IMainPathStartPointCache
	{
		IProcessPoint GetMainPathStartCAMPoint();
	}

	public interface ILeadCache
	{
		List<CAMPoint> LeadInCAMPointList
		{
			get;
		}

		List<CAMPoint> LeadOutCAMPointList
		{
			get;
		}

		LeadData LeadData
		{
			get;
		}
	}

	public interface IPathReverseCache
	{
		bool IsPathReverse
		{
			get;
		}
	}

	public interface IToolVecCache
	{
		List<CAMPoint> GetToolVecList();

		bool IsToolVecModifyPoint( ISetToolVecPoint point );
	}

	public interface IOverCutCache
	{
		List<CAMPoint> OverCutCAMPointList
		{
			get;
		}

		double OverCutLength
		{
			get;
		}
	}

	public interface IProcessPathStartEndCache
	{
		CAMPoint GetProcessStartPoint();

		CAMPoint GetProcessEndPoint();
	}

	/// <summary>
	/// Generic strategy for path cache access using type-safe property accessor
	/// Eliminates code duplication by using a generic approach
	/// </summary>
	/// <typeparam name="TPathObject">Type of PathObject (e.g., ContourPathObject, CirclePathObject)</typeparam>
	internal class PathCacheStrategy<TPathObject> : IPathCacheStrategy
		where TPathObject : PathObject
	{
		private readonly Func<TPathObject, object> m_CacheInfoGetter;

		public PathCacheStrategy( Func<TPathObject, object> cacheInfoGetter )
		{
			m_CacheInfoGetter = cacheInfoGetter ?? throw new ArgumentNullException( nameof( cacheInfoGetter ) );
		}

		public IMainPathStartPointCache GetMainPathStartPointCache( PathObject pathObject )
		{
			return GetCacheInfo( pathObject ) as IMainPathStartPointCache;
		}

		public ILeadCache GetLeadCache( PathObject pathObject )
		{
			return GetCacheInfo( pathObject ) as ILeadCache;
		}

		public IPathReverseCache GetPathReverseCache( PathObject pathObject )
		{
			return GetCacheInfo( pathObject ) as IPathReverseCache;
		}

		public IToolVecCache GetToolVecCache( PathObject pathObject )
		{
			return GetCacheInfo( pathObject ) as IToolVecCache;
		}

		public IOverCutCache GetOverCutCache( PathObject pathObject )
		{
			return GetCacheInfo( pathObject ) as IOverCutCache;
		}

		private object GetCacheInfo( PathObject pathObject )
		{
			if( pathObject is TPathObject typedPath ) {
				return m_CacheInfoGetter( typedPath );
			}
			return null;
		}
	}

	internal interface IPathCacheStrategy
	{
		IMainPathStartPointCache GetMainPathStartPointCache( PathObject pathObject );
		ILeadCache GetLeadCache( PathObject pathObject );
		IPathReverseCache GetPathReverseCache( PathObject pathObject );
		IToolVecCache GetToolVecCache( PathObject pathObject );
		IOverCutCache GetOverCutCache( PathObject pathObject );
	}

	internal static class PathCacheStrategyFactory
	{
		// Create singleton strategy instances using generic PathCacheStrategy
		private static readonly IPathCacheStrategy s_ContourStrategy = 
			new PathCacheStrategy<ContourPathObject>( p => p.ContourCacheInfo );
		
		private static readonly IPathCacheStrategy s_CircleStrategy = 
			new PathCacheStrategy<CirclePathObject>( p => p.CircleCacheInfo );
		
		private static readonly IPathCacheStrategy s_RectangleStrategy = 
			new PathCacheStrategy<RectanglePathObject>( p => p.RectangleCacheInfo );
		
		private static readonly IPathCacheStrategy s_PolygonStrategy = 
			new PathCacheStrategy<PolygonPathObject>( p => p.PolygonCacheInfo );
		
		private static readonly IPathCacheStrategy s_RunwayStrategy = 
			new PathCacheStrategy<RunwayPathObject>( p => p.RunwayCacheInfo );

		public static IPathCacheStrategy GetStrategy( PathType pathType )
		{
			switch( pathType ) {
				case PathType.Circle:
					return s_CircleStrategy;
				case PathType.Rectangle:
					return s_RectangleStrategy;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					return s_PolygonStrategy;
				case PathType.Runway:
					return s_RunwayStrategy;
				case PathType.Contour:
				default:
					return s_ContourStrategy;
			}
		}
	}

	internal static class PathCacheProvider
	{
		public static bool TryGetMainPathStartPointCache( PathObject pathObject, out IMainPathStartPointCache cache )
		{
			return TryGetCache( pathObject, s => s.GetMainPathStartPointCache( pathObject ), out cache );
		}

		public static bool TryGetLeadCache( PathObject pathObject, out ILeadCache cache )
		{
			return TryGetCache( pathObject, s => s.GetLeadCache( pathObject ), out cache );
		}

		public static bool TryGetPathReverseCache( PathObject pathObject, out IPathReverseCache cache )
		{
			return TryGetCache( pathObject, s => s.GetPathReverseCache( pathObject ), out cache );
		}

		public static bool TryGetToolVecCache( PathObject pathObject, out IToolVecCache cache )
		{
			return TryGetCache( pathObject, s => s.GetToolVecCache( pathObject ), out cache );
		}

		public static bool TryGetOverCutCache( PathObject pathObject, out IOverCutCache cache )
		{
			return TryGetCache( pathObject, s => s.GetOverCutCache( pathObject ), out cache );
		}

		/// <summary>
		/// Generic helper method to reduce duplication in TryGet methods
		/// </summary>
		private static bool TryGetCache<TCache>( PathObject pathObject, Func<IPathCacheStrategy, TCache> cacheGetter, out TCache cache )
			where TCache : class
		{
			cache = null;
			if( pathObject == null ) {
				return false;
			}

			IPathCacheStrategy strategy = PathCacheStrategyFactory.GetStrategy( pathObject.PathType );
			cache = cacheGetter( strategy );
			return cache != null;
		}
	}
}
