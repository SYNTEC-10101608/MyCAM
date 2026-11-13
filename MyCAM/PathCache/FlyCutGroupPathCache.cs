using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public enum FlyCutType
	{
		Circular
	}

	public class FlyCutGroupPathCache : IPathCache, ITransformableCache, IDisposable
	{
		public FlyCutGroupPathCache( IReadOnlyList<ContourPathObject> pathList, CraftData craftData )
		{
			m_PathList = pathList.ToList();
			m_CraftData = craftData;
			m_SubscribedCraftData = m_PathList.Select( path => path.CraftData ).ToList();

			m_FlyCutStepCacheList = new List<IFlyCutStepCache>();
			BuildCircularFlyCut( m_PathList );

			m_MainPathPointList = new List<CAMPoint>();
			BuildMainPathPointList();

			foreach( CraftData memberData in m_SubscribedCraftData ) {
				memberData.CADFactorChanged += SetCADDataDirty;
				memberData.CAMFactorChanged += SetCAMDataDirty;
			}

		}

		public void Dispose()
		{
			foreach( CraftData memberData in m_SubscribedCraftData ) {
				memberData.CADFactorChanged -= SetCADDataDirty;
				memberData.CAMFactorChanged -= SetCAMDataDirty;
			}
			m_SubscribedCraftData.Clear();
		}

		public FlyCutType FlyCutType
		{
			get
			{
				return FlyCutType.Circular;
			}
		}

		public IReadOnlyList<string> PathIDList
		{
			get
			{
				return m_PathList.Select( path => path.UID ).ToList();
			}
		}

		public List<CAMPoint> MainPathPointList
		{
			get
			{
				UpdateCache();
				return m_MainPathPointList;
			}
		}

		public List<CAMPoint> LeadInPointList
		{
			get
			{
				ContourPathObject firstPath = m_PathList.First();
				LeadHelper.SetLeadIn( firstPath.ContourCache.MainPathPointList.First().Clone(),
					out List<CAMPoint> leadInPointList, m_CraftData.LeadData, firstPath.CraftData.IsPathReverse,
					firstPath.CraftData.IsToolVecReverse );
				return leadInPointList;
			}
		}

		public List<CAMPoint> LeadOutPointList
		{
			get
			{
				ContourPathObject lastPath = m_PathList.Last();
				LeadHelper.SetLeadOut( lastPath.ContourCache.MainPathPointList.Last().Clone(),
					out List<CAMPoint> leadOutPointList, m_CraftData.LeadData, lastPath.CraftData.IsPathReverse,
					lastPath.CraftData.IsToolVecReverse );
				return leadOutPointList;
			}
		}

		public List<CAMPoint> OverCutPointList
		{
			get
			{
				return new List<CAMPoint>();
			}
		}

		public gp_Ax3 RefCoord
		{
			get
			{
				// TODO: compute ref coord from all path
				return m_PathList.First().ContourCache.RefCoord;
			}
		}

		public gp_Ax1 ComputeRefCenterDir
		{
			get
			{
				// TODO: compute ref center dir from all path
				return m_PathList.First().ContourCache.ComputeRefCenterDir;
			}
		}

		public void DoTransform( gp_Trsf transform )
		{
			for( int i = 0; i < m_PathList.Count; ++i ) {
				m_PathList[ i ]?.ContourCache.DoTransform( transform );
			}
		}

		#region Post Processing

		public CAMPoint GetProcessStartPoint()
		{
			List<CAMPoint> leadInPointList = LeadInPointList;
			if( leadInPointList.Count > 0 ) {
				return leadInPointList.First().Clone();
			}
			return m_PathList.First().ContourCache.MainPathPointList.First().Clone();
		}

		public CAMPoint GetProcessEndPoint()
		{
			List<CAMPoint> leadOutPointList = LeadOutPointList;
			if( leadOutPointList.Count > 0 ) {
				return leadOutPointList.Last().Clone();
			}
			return m_PathList.Last().ContourCache.MainPathPointList.Last().Clone();
		}

		#endregion

		void BuildCircularFlyCut( IReadOnlyList<ContourPathObject> pathList )
		{
			m_FlyCutStepCacheList.Clear();

			ContourPathObject firstPath = pathList[ 0 ];
			m_FlyCutStepCacheList.Add( new CutStepCache( firstPath, 0, firstPath.ContourCache.MainPathPointList.Count - 1 ) );

			for( int i = 1; i < pathList.Count; ++i ) {

				ContourPathObject fromPath = pathList[ i - 1 ];
				ContourPathObject toPath = pathList[ i ];

				List<CAMPoint> fromPoints = fromPath.ContourCache.MainPathPointList;
				List<CAMPoint> toPoints = toPath.ContourCache.MainPathPointList;
				CAMPoint start = fromPoints.Last();
				CAMPoint end = toPoints.First();

				gp_Dir departureTangent = fromPath.CraftData.IsPathReverse ? start.TangentVec.Reversed() : start.TangentVec;
				gp_Dir arrivalTangent = toPath.CraftData.IsPathReverse ? end.TangentVec.Reversed() : end.TangentVec;
				gp_Vec departureCurvature = FlyCutHelper.EstimateEndpointCurvature( fromPoints, true, departureTangent );
				gp_Vec arrivalCurvature = FlyCutHelper.EstimateEndpointCurvature( toPoints, false, arrivalTangent );

				FlyStepCache flyStep = new FlyStepCache();
				if( flyStep.TryUpdate( start, end, departureTangent, arrivalTangent, departureCurvature, arrivalCurvature ) ) {
					m_FlyCutStepCacheList.Add( flyStep );
					m_FlyCutStepCacheList.Add( new CutStepCache( toPath, 0, toPath.ContourCache.MainPathPointList.Count - 1 ) );
				}
			}
		}

		void BuildMainPathPointList()
		{
			m_MainPathPointList.Clear();
			for( int i = 0; i < m_FlyCutStepCacheList.Count; ++i ) {
				if( m_FlyCutStepCacheList[ i ] is CutStepCache stepCache ) {
					m_MainPathPointList.AddRange( stepCache.CAMPointList );
				}
			}
		}

		void UpdateCache()
		{
			if( !m_IsCADFactorDirty && !m_IsCAMFactorDirty ) {
				return;
			}
			BuildCircularFlyCut( m_PathList );
			BuildMainPathPointList();
			m_IsCADFactorDirty = false;
			m_IsCAMFactorDirty = false;
		}

		public List<IFlyCutStepCache> FlyCutStepCacheList
		{
			get
			{
				UpdateCache();
				return m_FlyCutStepCacheList;
			}
		}

		List<IFlyCutStepCache> m_FlyCutStepCacheList;

		List<CAMPoint> m_MainPathPointList;

		public interface IFlyCutStepCache
		{
			List<CAMPoint> CAMPointList
			{
				get;
			}

			List<gp_Pnt> PointList
			{
				get;
			}
		}

		public class CutStepCache : IFlyCutStepCache
		{
			public CutStepCache( ContourPathObject path, int startCAMIndex, int endCAMIndex )
			{
				Path = path;
				m_StartCAMIndex = startCAMIndex;
				m_EndCAMIndex = endCAMIndex;
			}

			public ContourPathObject Path { get; private set; }

			int m_StartCAMIndex;
			int m_EndCAMIndex;

			public List<CAMPoint> CAMPointList
			{
				get
				{
					return Path?.ContourCache.MainPathPointList.GetRange( m_StartCAMIndex, m_EndCAMIndex - m_StartCAMIndex + 1 );
				}
			}

			public List<gp_Pnt> PointList
			{
				get
				{
					return CAMPointList?.Select( point => point.Point ).ToList();
				}
			}
		}

		public class FlyStepCache : IFlyCutStepCache
		{
			public TopoDS_Edge Edge { get; private set; }

			public List<CAMPoint> CAMPointList { get; private set; }

			public List<gp_Pnt> PointList
			{
				get
				{
					return CAMPointList?.Select( point => point.Point ).ToList();
				}
			}

			public bool TryUpdate( CAMPoint start, CAMPoint end, gp_Dir departureTangent, gp_Dir arrivalTangent )
			{
				return TryUpdate( start, end, departureTangent, arrivalTangent, null, null );
			}

			public bool TryUpdate( CAMPoint start, CAMPoint end, gp_Dir departureTangent, gp_Dir arrivalTangent,
				gp_Vec departureCurvature, gp_Vec arrivalCurvature )
			{
				Clear();
				if( start == null || end == null ) {
					return false;
				}
				if( !FlyCutHelper.TryCalculateFlightCAM( start, end, departureTangent, arrivalTangent,
					departureCurvature, arrivalCurvature, out TopoDS_Edge edge, out List<CAMPoint> camPoints ) ) {
					return false;
				}

				CAMPointList = camPoints;
				Edge = edge;

				return true;
			}

			public void Clear()
			{
				Edge = null;
				CAMPointList?.Clear();
				CAMPointList = null;
			}
		}

		void SetCAMDataDirty()
		{
			if( !m_IsCAMFactorDirty ) {
				m_IsCAMFactorDirty = true;
			}
		}

		void SetCADDataDirty()
		{
			if( !m_IsCADFactorDirty ) {
				m_IsCADFactorDirty = true;
			}
		}

		bool m_IsCAMFactorDirty = false;
		bool m_IsCADFactorDirty = false;

		IReadOnlyList<ContourPathObject> m_PathList;
		CraftData m_CraftData;
		readonly List<CraftData> m_SubscribedCraftData;
	}
}
