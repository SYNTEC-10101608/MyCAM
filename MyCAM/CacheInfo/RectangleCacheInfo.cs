using MyCAM.Data;
using MyCAM.Data.GeomDataFolder;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.CacheInfo
{
	public class RectangleCacheInfo : IStandardPatternCacheInfo, IProcessPathStartEndCache, IMainPathStartPointCache, ILeadCache, IPathReverseCache, IOverCutCache, IToolVecCache
	{
		public RectangleCacheInfo( gp_Ax3 coordinateInfo, IStandardPatternGeomData geomData, CraftData craftData )
		{
			if( geomData == null || craftData == null || !( geomData is RectangleGeomData rectangleGeomData ) ) {
				throw new ArgumentNullException( "RectangleCacheInfo constructing argument error" );
			}
			m_CoordinateInfo = coordinateInfo;
			m_RectangleGeomData = rectangleGeomData;
			m_CraftData = craftData;
			m_CraftData.ParameterChanged += SetCraftDataDirty;
			BuildCAMPointList();
		}

		public PathType PathType
		{
			get
			{
				return PathType.Rectangle;
			}
		}

		#region Computation Result
		public List<CAMPoint> StartPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_StartPointList;
			}
		}

		public List<CAMPoint> LeadInCAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_LeadInCAMPointList;
			}
		}

		public List<CAMPoint> LeadOutCAMPointList
		{
			get;
		}

		public List<CAMPoint> OverCutCAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_OverCutPointList;
			}
		}

		#endregion

		public CAMPoint GetProcessRefPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			return new CAMPoint( new CADPoint( m_CoordinateInfo.Location(), m_CoordinateInfo.Direction(), m_CoordinateInfo.XDirection(), m_CoordinateInfo.YDirection() ), m_CoordinateInfo.Direction() );
		}

		public IProcessPoint GetProcessStartPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			if( LeadInCAMPointList.Count != 0 ) {
				return LeadInCAMPointList[ 0 ].Clone();
			}
			return m_StartPointList[ m_CraftData.StartPointIndex ].Clone();
		}

		public IProcessPoint GetProcessEndPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			if( OverCutCAMPointList.Count != 0 ) {
				return OverCutCAMPointList[ OverCutCAMPointList.Count - 1 ].Clone();
			}
			return m_StartPointList[ m_CraftData.StartPointIndex ].Clone();
		}

		public IProcessPoint GetMainPathStartCAMPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			return m_StartPointList[ m_CraftData.StartPointIndex ].Clone();
		}

		public IReadOnlyList<IProcessPoint> GetToolVecList()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			return m_StartPointList.Cast<IProcessPoint>().ToList();
		}

		public bool IsToolVecModifyPoint( ISetToolVecPoint point )
		{
			return false;
		}

		public bool IsClosed
		{
			get
			{
				return true;
			}
		}

		public bool IsPathReverse
		{
			get
			{
				return m_CraftData.IsReverse;
			}
		}

		public LeadData LeadData
		{
			get
			{
				return m_CraftData.LeadLineParam;
			}
		}

		public double OverCutLength
		{
			get
			{
				return m_CraftData.OverCutLength;
			}
		}

		public void Transform( gp_Trsf transform )
		{
			m_CoordinateInfo.Transform( transform );
			BuildCAMPointList();
		}

		void BuildCAMPointList()
		{
			m_IsCraftDataDirty = false;
			m_StartPointList = RectangleCacheInfoExtensions.GetStartPointList( m_CoordinateInfo, m_RectangleGeomData.Length, m_RectangleGeomData.Width );
		}

		void SetCraftDataDirty()
		{
			if( !m_IsCraftDataDirty ) {
				m_IsCraftDataDirty = true;
			}
		}

		RectangleGeomData m_RectangleGeomData;
		gp_Ax3 m_CoordinateInfo;
		string m_szPathID;
		List<CAMPoint> m_StartPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();
		CraftData m_CraftData;

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
	}

	internal static class RectangleCacheInfoExtensions
	{
		internal static List<CAMPoint> GetStartPointList( gp_Ax3 coordinateInfo, double longSideLength, double shortSideLength )
		{
			gp_Pnt centerPoint = coordinateInfo.Location();
			gp_Pln plane = new gp_Pln( coordinateInfo );
			double halfL = longSideLength / 2.0;
			double halfW = shortSideLength / 2.0;

			gp_Dir local_X_pos = gp.DX();
			gp_Dir local_X_neg = gp.DX().Reversed();
			gp_Dir local_Y_pos = gp.DY();
			gp_Dir local_Y_neg = gp.DY().Reversed();
			gp_Dir local_Z_pos = gp.DZ();

			gp_Pnt local_Pnt_L_Pos = new gp_Pnt( halfL, 0, 0 );
			gp_Dir local_N1_L_Pos = local_Z_pos;
			gp_Dir local_N2_L_Pos = local_X_neg;
			gp_Dir local_Tan_L_Pos = local_Y_neg;
			gp_Dir local_Tool_L_Pos = local_Z_pos;

			gp_Pnt local_Pnt_S_Neg = new gp_Pnt( 0, -halfW, 0 );
			gp_Dir local_N1_S_Neg = local_Z_pos;
			gp_Dir local_N2_S_Neg = local_Y_pos;
			gp_Dir local_Tan_S_Neg = local_X_neg;
			gp_Dir local_Tool_S_Neg = local_Z_pos;

			gp_Ax3 targetCoordSystem = new gp_Ax3( centerPoint, plane.Axis().Direction(), plane.XAxis().Direction() );

			gp_Ax3 finalCoordSystem = targetCoordSystem.Rotated( plane.Axis(), 0 );
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( finalCoordSystem, new gp_Ax3() );

			CADPoint cad_L_Pos = new CADPoint(
				local_Pnt_L_Pos.Transformed( transformation ),
				local_N1_L_Pos.Transformed( transformation ),
				local_N2_L_Pos.Transformed( transformation ),
				local_Tan_L_Pos.Transformed( transformation )
			);
			CAMPoint cam_L_Pos = new CAMPoint( cad_L_Pos, local_Tool_L_Pos.Transformed( transformation ) );

			CADPoint cad_S_Neg = new CADPoint(
				local_Pnt_S_Neg.Transformed( transformation ),
				local_N1_S_Neg.Transformed( transformation ),
				local_N2_S_Neg.Transformed( transformation ),
				local_Tan_S_Neg.Transformed( transformation )
			);
			CAMPoint cam_S_Neg = new CAMPoint( cad_S_Neg, local_Tool_S_Neg.Transformed( transformation ) );

			List<CAMPoint> resultList = new List<CAMPoint>
			{
				cam_L_Pos,
				cam_S_Neg,
			};

			return resultList;
		}
	}
}
