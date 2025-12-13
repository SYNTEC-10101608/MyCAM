using MyCAM.Data;
using System;

namespace MyCAM.PathCache
{
	#region Path Cache Provider

	// this is for user to get any kind of cache they want
	public static class PathCacheProvider
	{
		public static bool TryGetMainPathStartPointCache( string szPathID, out IMainPathStartPointCache cache )
		{
			return TryGetCache( szPathID, out cache );
		}

		public static bool TryGetLeadCache( string szPathID, out ILeadCache cache )
		{
			return TryGetCache( szPathID, out cache );
		}

		public static bool TryGetPathReverseCache( string szPathID, out IPathReverseCache cache )
		{
			return TryGetCache( szPathID, out cache );
		}

		public static bool TryGetToolVecCache( string szPathID, out IToolVecCache cache )
		{
			return TryGetCache( szPathID, out cache );
		}

		public static bool TryGetOverCutCache( string szPathID, out IOverCutCache cache )
		{
			return TryGetCache( szPathID, out cache );
		}

		public static bool TryGetProcessPathStartEndCache( string szPathID, out IProcessPathStartEndCache cache )
		{
			return TryGetCache( szPathID, out cache );
		}

		// this call strategy to get correct cache by path type
		static bool TryGetCache<TCache>( string szPathID, out TCache cache )
			where TCache : class, IPathCache
		{
			cache = null;

			// use DataGettingHelper to get PathObject from string ID
			if( !DataGettingHelper.TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}

			// get strategy based on PathType
			IPathCacheStrategy strategy = PathCacheStrategyFactory.GetStrategy( pathObject.PathType );
			if( strategy == null ) {
				return false;
			}

			// get cache info and cast to requested type
			IPathCache pathCache = strategy.GetPathCache( pathObject );
			cache = pathCache as TCache;
			return cache != null;
		}
	}

	#endregion

	// the below implementation is not that important
	#region Path Cache Strategy Pattern

	internal interface IPathCacheStrategy
	{
		IPathCache GetPathCache( PathObject pathObject );
	}

	internal class PathCacheStrategy<TPathObject> : IPathCacheStrategy
		where TPathObject : PathObject
	{
		private readonly Func<TPathObject, IPathCache> m_PathCacheGetter;

		public PathCacheStrategy( Func<TPathObject, IPathCache> pathCacheGetter )
		{
			m_PathCacheGetter = pathCacheGetter ?? throw new ArgumentNullException( nameof( pathCacheGetter ) );
		}

		public IPathCache GetPathCache( PathObject pathObject )
		{
			if( pathObject is TPathObject typedPathObject ) {
				return m_PathCacheGetter( typedPathObject );
			}
			return null;
		}
	}

	internal static class PathCacheStrategyFactory
	{
		// create singleton strategy instances using generic PathCacheStrategy
		static readonly IPathCacheStrategy s_ContourStrategy =
			new PathCacheStrategy<ContourPathObject>( obj => obj.ContourCache );

		static readonly IPathCacheStrategy s_StandardPatternStrategy =
			new PathCacheStrategy<StdPatternObjectBase>( obj => obj.StandatdPatternCache );

		public static IPathCacheStrategy GetStrategy( PathType pathType )
		{
			switch( pathType ) {
				case PathType.Circle:
				case PathType.Rectangle:
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
				case PathType.Runway:
					return s_StandardPatternStrategy;
				case PathType.Contour:
				default:
					return s_ContourStrategy;
			}
		}
	}

	#endregion
}
