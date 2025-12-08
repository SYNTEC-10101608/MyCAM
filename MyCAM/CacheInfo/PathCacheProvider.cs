using MyCAM.Data;
using MyCAM.Data.PathObjectFolder;
using OCC.gp;
using System.Collections.Generic;

namespace MyCAM.CacheInfo
{
	public interface IStartPointCache
	{
		gp_Pnt GetMainPathStartPoint();

		CAMPoint GetFirstCAMPoint();
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

	public interface IPathHeadTailCache
	{
		CAMPoint GetProcessStartPoint();

		CAMPoint GetProcessEndPoint();
	}

	internal interface IPathCacheStrategy
	{
		IStartPointCache GetStartPointCache( PathObject pathObject );
		ILeadCache GetLeadCache( PathObject pathObject );
		IPathReverseCache GetPathReverseCache( PathObject pathObject );
		IToolVecCache GetToolVecCache( PathObject pathObject );
		IOverCutCache GetOverCutCache( PathObject pathObject );
	}

	internal class ContourCacheStrategy : IPathCacheStrategy
	{
		public IStartPointCache GetStartPointCache( PathObject pathObject )
		{
			return ( pathObject as ContourPathObject )?.ContourCacheInfo;
		}

		public ILeadCache GetLeadCache( PathObject pathObject )
		{
			return ( pathObject as ContourPathObject )?.ContourCacheInfo;
		}

		public IPathReverseCache GetPathReverseCache( PathObject pathObject )
		{
			return ( pathObject as ContourPathObject )?.ContourCacheInfo;
		}

		public IToolVecCache GetToolVecCache( PathObject pathObject )
		{
			return ( pathObject as ContourPathObject )?.ContourCacheInfo;
		}

		public IOverCutCache GetOverCutCache( PathObject pathObject )
		{
			return ( pathObject as ContourPathObject )?.ContourCacheInfo;
		}
	}

	internal class CircleCacheStrategy : IPathCacheStrategy
	{
		public IStartPointCache GetStartPointCache( PathObject pathObject )
		{
			return ( pathObject as CirclePathObject )?.CircleCacheInfo;
		}

		public ILeadCache GetLeadCache( PathObject pathObject )
		{
			return ( pathObject as CirclePathObject )?.CircleCacheInfo;
		}

		public IPathReverseCache GetPathReverseCache( PathObject pathObject )
		{
			return ( pathObject as CirclePathObject )?.CircleCacheInfo;
		}

		public IToolVecCache GetToolVecCache( PathObject pathObject )
		{
			return null; // Circle doesn't support tool vector cache
		}

		public IOverCutCache GetOverCutCache( PathObject pathObject )
		{
			return ( pathObject as CirclePathObject )?.CircleCacheInfo;
		}
	}

	internal class RectangleCacheStrategy : IPathCacheStrategy
	{
		public IStartPointCache GetStartPointCache( PathObject pathObject )
		{
			return ( pathObject as RectanglePathObject )?.RectangleCacheInfo;
		}

		public ILeadCache GetLeadCache( PathObject pathObject )
		{
			return ( pathObject as RectanglePathObject )?.RectangleCacheInfo;
		}

		public IPathReverseCache GetPathReverseCache( PathObject pathObject )
		{
			return ( pathObject as RectanglePathObject )?.RectangleCacheInfo;
		}

		public IToolVecCache GetToolVecCache( PathObject pathObject )
		{
			return null; // Rectangle doesn't support tool vector cache
		}

		public IOverCutCache GetOverCutCache( PathObject pathObject )
		{
			return ( pathObject as RectanglePathObject )?.RectangleCacheInfo;
		}
	}

	internal class PolygonCacheStrategy : IPathCacheStrategy
	{
		public IStartPointCache GetStartPointCache( PathObject pathObject )
		{
			return ( pathObject as PolygonPathObject )?.PolygonCacheInfo;
		}

		public ILeadCache GetLeadCache( PathObject pathObject )
		{
			return ( pathObject as PolygonPathObject )?.PolygonCacheInfo;
		}

		public IPathReverseCache GetPathReverseCache( PathObject pathObject )
		{
			return ( pathObject as PolygonPathObject )?.PolygonCacheInfo;
		}

		public IToolVecCache GetToolVecCache( PathObject pathObject )
		{
			return null; // Polygon doesn't support tool vector cache
		}

		public IOverCutCache GetOverCutCache( PathObject pathObject )
		{
			return ( pathObject as PolygonPathObject )?.PolygonCacheInfo;
		}
	}

	internal class RunwayCacheStrategy : IPathCacheStrategy
	{
		public IStartPointCache GetStartPointCache( PathObject pathObject )
		{
			return ( pathObject as RunwayPathObject )?.RunwayCacheInfo;
		}

		public ILeadCache GetLeadCache( PathObject pathObject )
		{
			return ( pathObject as RunwayPathObject )?.RunwayCacheInfo;
		}

		public IPathReverseCache GetPathReverseCache( PathObject pathObject )
		{
			return ( pathObject as RunwayPathObject )?.RunwayCacheInfo;
		}

		public IToolVecCache GetToolVecCache( PathObject pathObject )
		{
			return null; // Runway doesn't support tool vector cache
		}

		public IOverCutCache GetOverCutCache( PathObject pathObject )
		{
			return ( pathObject as RunwayPathObject )?.RunwayCacheInfo;
		}
	}

	internal static class PathCacheStrategyFactory
	{
		private static readonly ContourCacheStrategy s_ContourStrategy = new ContourCacheStrategy();
		private static readonly CircleCacheStrategy s_CircleStrategy = new CircleCacheStrategy();
		private static readonly RectangleCacheStrategy s_RectangleStrategy = new RectangleCacheStrategy();
		private static readonly PolygonCacheStrategy s_PolygonStrategy = new PolygonCacheStrategy();
		private static readonly RunwayCacheStrategy s_RunwayStrategy = new RunwayCacheStrategy();

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
		public static bool TryGetStartPointCache( PathObject pathObject, out IStartPointCache cache )
		{
			cache = null;
			if( pathObject == null ) {
				return false;
			}

			IPathCacheStrategy strategy = PathCacheStrategyFactory.GetStrategy( pathObject.PathType );
			cache = strategy.GetStartPointCache( pathObject );
			return cache != null;
		}

		public static bool TryGetLeadCache( PathObject pathObject, out ILeadCache cache )
		{
			cache = null;
			if( pathObject == null ) {
				return false;
			}

			IPathCacheStrategy strategy = PathCacheStrategyFactory.GetStrategy( pathObject.PathType );
			cache = strategy.GetLeadCache( pathObject );
			return cache != null;
		}

		public static bool TryGetPathReverseCache( PathObject pathObject, out IPathReverseCache cache )
		{
			cache = null;
			if( pathObject == null ) {
				return false;
			}

			IPathCacheStrategy strategy = PathCacheStrategyFactory.GetStrategy( pathObject.PathType );
			cache = strategy.GetPathReverseCache( pathObject );
			return cache != null;
		}

		public static bool TryGetToolVecCache( PathObject pathObject, out IToolVecCache cache )
		{
			cache = null;
			if( pathObject == null ) {
				return false;
			}

			IPathCacheStrategy strategy = PathCacheStrategyFactory.GetStrategy( pathObject.PathType );
			cache = strategy.GetToolVecCache( pathObject );
			return cache != null;
		}

		public static bool TryGetOverCutCache( PathObject pathObject, out IOverCutCache cache )
		{
			cache = null;
			if( pathObject == null ) {
				return false;
			}

			IPathCacheStrategy strategy = PathCacheStrategyFactory.GetStrategy( pathObject.PathType );
			cache = strategy.GetOverCutCache( pathObject );
			return cache != null;
		}
	}
}
