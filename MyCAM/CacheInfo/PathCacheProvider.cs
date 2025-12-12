using MyCAM.Data;
using System;
using System.Collections.Generic;

namespace MyCAM.CacheInfo
{
	#region Cache Interface Definitions

	public interface ICacheInfo
	{
		PathType PathType
		{
			get;
		}
	}

	public interface IStandardPatternCacheInfo : ICacheInfo
	{
	}

	public interface IStandardPatternRefPointCache
	{
		IProcessPoint GetProcessRefPoint();
	}

	public interface IMainPathStartPointCache : ICacheInfo
	{
		CAMPoint GetMainPathStartCAMPoint();
	}

	public interface ILeadCache : ICacheInfo
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

	public interface IPathReverseCache : ICacheInfo
	{
		bool IsPathReverse
		{
			get;
		}
	}

	public interface IToolVecCache : ICacheInfo
	{
		IReadOnlyList<IProcessPoint> GetToolVecList();

		bool IsToolVecModifyPoint( ISetToolVecPoint point );
	}

	public interface IOverCutCache : ICacheInfo
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

	public interface IProcessPathStartEndCache : ICacheInfo
	{
		IProcessPoint GetProcessStartPoint();

		IProcessPoint GetProcessEndPoint();
	}

	#endregion

	#region Path Cache Strategy Pattern

	internal interface IPathCacheStrategy
	{
		ICacheInfo GetCacheInfo( PathObject pathObject );
	}

	internal class PathCacheStrategy<TPathObject> : IPathCacheStrategy
		where TPathObject : PathObject
	{
		private readonly Func<TPathObject, ICacheInfo> m_CacheInfoGetter;

		public PathCacheStrategy( Func<TPathObject, ICacheInfo> cacheInfoGetter )
		{
			m_CacheInfoGetter = cacheInfoGetter ?? throw new ArgumentNullException( nameof( cacheInfoGetter ) );
		}

		public ICacheInfo GetCacheInfo( PathObject pathObject )
		{
			if( pathObject is TPathObject typedPathObject ) {
				return m_CacheInfoGetter( typedPathObject );
			}
			return null;
		}
	}

	internal static class PathCacheStrategyFactory
	{
		// create singleton strategy instances using generic PathCacheStrategy
		static readonly IPathCacheStrategy s_ContourStrategy =
			new PathCacheStrategy<ContourPathObject>( p => p.ContourCacheInfo );

		static readonly IPathCacheStrategy s_StandardPatternStrategy =
			new PathCacheStrategy<StandardPatternBasedPathObject>( p => p.StandatdPatternCacheInfo );

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

	#region Path Cache Provider

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

		static bool TryGetCache<TCache>( string szPathID, out TCache cache )
			where TCache : class, ICacheInfo
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
			ICacheInfo cacheInfo = strategy.GetCacheInfo( pathObject );
			cache = cacheInfo as TCache;
			return cache != null;
		}
	}

	#endregion
}
