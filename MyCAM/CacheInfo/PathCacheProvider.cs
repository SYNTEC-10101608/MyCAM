using MyCAM.Data;
using MyCAM.Data.PathObjectFolder;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.CacheInfo
{
	public interface IStandardPatternCacheInfo
	{
		PathType PathType
		{
			get;
		}

		void DoTransform( gp_Trsf transform );
	}

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
		IReadOnlyList<IProcessPoint> GetToolVecList();

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
		IProcessPoint GetProcessStartPoint();

		IProcessPoint GetProcessEndPoint();
	}

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
		// create singleton strategy instances using generic PathCacheStrategy
		static readonly IPathCacheStrategy s_ContourStrategy =
			new PathCacheStrategy<ContourPathObject>( p => p.ContourCacheInfo );

		static readonly IPathCacheStrategy s_StandardPatternStrategy =
			new PathCacheStrategy<StandardPatternBasedPathObject>( p => p.CacheInfo );

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

		static bool TryGetCache<TCache>( PathObject pathObject, Func<IPathCacheStrategy, TCache> cacheGetter, out TCache cache )
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
