using MyCAM.Data;
using System;

namespace MyCAM.PathCache
{
	#region Path Cache Provider

	// this is for user to get any kind of cache they want
	public static class PathCacheProvider
	{
		public static bool TryGetStdPatternRefPointCache( string szPathID, out IStdPatternRefPointCache cache )
		{
			cache = null;
			IPathCache _cache;
			if( !TryGetPathCacheAndCraftData( szPathID, out _cache, out CraftData craftData ) ) {
				return false;
			}

			// only standard pattern has RefPoint
			IStdPatternCache stdPatternCache = _cache as IStdPatternCache;
			if( stdPatternCache == null ) {
				return false;
			}

			cache = new StdPatternRefPointCache( stdPatternCache );
			return true;
		}

		public static bool TryGetProcessPathStartEndCache( string szPathID, out IProcessPathStartEndCache cache )
		{
			IPathCache _cache;
			if( !TryGetPathCacheAndCraftData( szPathID, out _cache, out CraftData craftData ) ) {
				cache = null;
				return false;
			}

			cache = new ProcessPathStartEndCache( _cache, craftData );
			return true;
		}

		public static bool TryGetMainPathStartPointCache( string szPathID, out IMainPathStartPointCache cache )
		{
			IPathCache _cache;
			if( !TryGetPathCacheAndCraftData( szPathID, out _cache, out CraftData craftData ) ) {
				cache = null;
				return false;
			}

			cache = new MainPathStartPointCache( _cache );
			return true;
		}

		public static bool TryGetOrientationCache( string szPathID, out IOrientationCache cache )
		{
			IPathCache _cache;
			if( !TryGetPathCacheAndCraftData( szPathID, out _cache, out CraftData craftData ) ) {
				cache = null;
				return false;
			}

			cache = new OrientationCache( _cache, craftData );
			return true;
		}

		public static bool TryGetLeadCache( string szPathID, out ILeadCache cache )
		{
			IPathCache _cache;
			if( !TryGetPathCacheAndCraftData( szPathID, out _cache, out CraftData craftData ) ) {
				cache = null;
				return false;
			}

			cache = new LeadCache( _cache, craftData );
			return true;
		}

		public static bool TryGetOverCutCache( string szPathID, out IOverCutCache cache )
		{
			IPathCache _cache;
			if( !TryGetPathCacheAndCraftData( szPathID, out _cache, out CraftData craftData ) ) {
				cache = null;
				return false;
			}

			cache = new OverCutCache( _cache, craftData );
			return true;
		}

		public static bool TryGetPathReverseCache( string szPathID, out IPathReverseCache cache )
		{
			IPathCache _cache;
			if( !TryGetPathCacheAndCraftData( szPathID, out _cache, out CraftData craftData ) ) {
				cache = null;
				return false;
			}

			cache = new PathReverseCache( craftData );
			return true;
		}

		public static bool TryGetToolVecCache( string szPathID, out IToolVecCache cache )
		{
			IPathCache _cache;
			if( !TryGetPathCacheAndCraftData( szPathID, out _cache, out CraftData craftData ) ) {
				cache = null;
				return false;
			}

			cache = new ToolVecCache( _cache, craftData );
			return true;
		}

		// this call strategy to get correct cache by path type
		static bool TryGetPathCacheAndCraftData<TCache>( string szPathID, out TCache cache, out CraftData craftData )
			where TCache : class, IPathCache
		{
			cache = null;
			craftData = null;

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

			// get CraftData
			craftData = pathObject.CraftData;
			return cache != null && craftData != null;
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
			return m_PathCacheGetter( pathObject as TPathObject );
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
