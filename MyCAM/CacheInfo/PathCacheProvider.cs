using MyCAM.Data;
using System;
using System.Collections.Generic;

namespace MyCAM.CacheInfo
{
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
		IProcessPoint GetMainPathStartCAMPoint();
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

	internal class PathCacheStrategy<TString> : IPathCacheStrategy
		where TString : PathObject
	{
		private readonly Func<TString, ICacheInfo> m_CacheInfoGetter;

		public PathCacheStrategy( Func<TString, ICacheInfo> cacheInfoGetter )
		{
			m_CacheInfoGetter = cacheInfoGetter ?? throw new ArgumentNullException( nameof( cacheInfoGetter ) );
		}

		public IMainPathStartPointCache GetMainPathStartPointCache( string szPathID )
		{
			return GetCacheInfo( szPathID ) as IMainPathStartPointCache;
		}

		public ILeadCache GetLeadCache( string szPathID )
		{
			return GetCacheInfo( szPathID ) as ILeadCache;
		}

		public IPathReverseCache GetPathReverseCache( string szPathID )
		{
			return GetCacheInfo( szPathID ) as IPathReverseCache;
		}

		public IToolVecCache GetToolVecCache( string szPathID )
		{
			return GetCacheInfo( szPathID ) as IToolVecCache;
		}

		public IOverCutCache GetOverCutCache( string szPathID )
		{
			return GetCacheInfo( szPathID ) as IOverCutCache;
		}

		public IProcessPathStartEndCache GetProcessPathStartEndCache( string szPathID )
		{
			return GetCacheInfo( szPathID ) as IProcessPathStartEndCache;
		}

		private ICacheInfo GetCacheInfo( string szPathID )
		{
			if( szPathID is TString typedPath ) {
				return m_CacheInfoGetter( typedPath );
			}
			return null;
		}
	}

	internal interface IPathCacheStrategy
	{
		IMainPathStartPointCache GetMainPathStartPointCache( string szPathID );

		ILeadCache GetLeadCache( string szPathID );

		IPathReverseCache GetPathReverseCache( string szPathID );

		IToolVecCache GetToolVecCache( string szPathID );

		IOverCutCache GetOverCutCache( string szPathID );

		IProcessPathStartEndCache GetProcessPathStartEndCache( string szPathID );
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

	internal static class PathCacheProvider
	{
		public static bool TryGetMainPathStartPointCache( string szPathID, out IMainPathStartPointCache cache )
		{
			return TryGetCache( szPathID, s => s.GetMainPathStartPointCache( szPathID ), out cache );
		}

		public static bool TryGetLeadCache( string szPathID, out ILeadCache cache )
		{
			return TryGetCache( szPathID, s => s.GetLeadCache( szPathID ), out cache );
		}

		public static bool TryGetPathReverseCache( string szPathID, out IPathReverseCache cache )
		{
			return TryGetCache( szPathID, s => s.GetPathReverseCache( szPathID ), out cache );
		}

		public static bool TryGetToolVecCache( string szPathID, out IToolVecCache cache )
		{
			return TryGetCache( szPathID, s => s.GetToolVecCache( szPathID ), out cache );
		}

		public static bool TryGetOverCutCache( string szPathID, out IOverCutCache cache )
		{
			return TryGetCache( szPathID, s => s.GetOverCutCache( szPathID ), out cache );
		}

		public static bool TryGetProcessPathStartEndCache( string szPathID, out IProcessPathStartEndCache cache )
		{
			return TryGetCache( szPathID, s => s.GetProcessPathStartEndCache( szPathID ), out cache );
		}

		static bool TryGetCache<TCache>( string szPathID, Func<IPathCacheStrategy, TCache> cacheGetter, out TCache cache )
			where TCache : class
		{
			cache = null;
			if( DataGettingHelper.TryGetPathObject( szPathID, out PathObject pathObject ) ) {
				return false;
			}

			IPathCacheStrategy strategy = PathCacheStrategyFactory.GetStrategy( pathObject.PathType );
			cache = cacheGetter( strategy );
			return cache != null;
		}
	}
}
