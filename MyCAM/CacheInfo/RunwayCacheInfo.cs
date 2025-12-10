using MyCAM.Data;
using MyCAM.Data.GeomDataFolder;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.CacheInfo
{
	public class RunwayCacheInfo : IProcessPathStartEndCache, IMainPathStartPointCache, ILeadCache, IPathReverseCache, IOverCutCache, IToolVecCache
	{
		public RunwayCacheInfo( gp_Ax3 coordinateInfo, RunwayGeomData runwayGeomData, CraftData craftData )
		{
			if( runwayGeomData == null || craftData == null ) {
				throw new ArgumentNullException( "RunwayCacheInfo constructing argument null" );
			}
			m_CoordinateInfo = coordinateInfo;
			m_RunwayGeomData = runwayGeomData;
			m_CraftData = craftData;
			m_CraftData.ParameterChanged += SetCraftDataDirty;
			BuildCAMPointList();
		}

		public PathType PathType
		{
			get
			{
				return PathType.Runway;
			}
		}

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

		public CAMPoint GetProcessRefPoint()
		{
			// Calculate runway parameters
			double length = m_RunwayGeomData.Length;
			double width = m_RunwayGeomData.Width;
			double radius = width / 2.0;
			double straightLength = length - width;

			// Left arc center position in local coordinate system
			gp_Pnt leftArcCenter;

			if( straightLength <= 0.001 ) {

				// Pure circle case: center is at origin
				leftArcCenter = new gp_Pnt( 0, 0, 0 );
			}
			else {

				// Runway shape: left arc center is at (-straightLength/2, 0, 0)
				double halfStraight = straightLength / 2.0;
				leftArcCenter = new gp_Pnt( -halfStraight, 0, 0 );
			}

			// Transform local coordinates to world coordinate system
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				m_CoordinateInfo.Location(),
				m_CoordinateInfo.Direction(),
				m_CoordinateInfo.XDirection()
			);
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );
			gp_Pnt worldLeftArcCenter = leftArcCenter.Transformed( transformation );

			return new CAMPoint(
				new CADPoint(
					worldLeftArcCenter,
					m_CoordinateInfo.Direction(),
					m_CoordinateInfo.XDirection(),
					m_CoordinateInfo.YDirection()
				),
				m_CoordinateInfo.Direction()
			);
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

		public void DoTransform( gp_Trsf transform )
		{
			m_CoordinateInfo.Transform( transform );
			BuildCAMPointList();
		}

		void BuildCAMPointList()
		{
			m_IsCraftDataDirty = false;
			m_StartPointList = RunwayCacheInfoExtensions.GetStartPointList( m_CoordinateInfo, m_RunwayGeomData.Length, m_RunwayGeomData.Width );
		}

		void SetCraftDataDirty()
		{
			if( !m_IsCraftDataDirty ) {
				m_IsCraftDataDirty = true;
			}
		}

		RunwayGeomData m_RunwayGeomData;
		gp_Ax3 m_CoordinateInfo;
		string m_szPathID;
		List<CAMPoint> m_StartPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();
		CraftData m_CraftData;

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
	}

	internal static class RunwayCacheInfoExtensions
	{
		internal static List<CAMPoint> GetStartPointList( gp_Ax3 coordinateInfo, double length, double width )
		{
			gp_Pnt centerPoint = coordinateInfo.Location();
			gp_Pln plane = new gp_Pln( coordinateInfo );
			double radius = width / 2.0;
			double straightLength = length - width;

			// runway shape start points:
			// 1. arc midpoint intersecting with positive X-axis
			// 2. long edge midpoint intersecting with negative Y-axis
			double halfStraight = straightLength / 2.0;
			gp_Dir local_X_pos = gp.DX();
			gp_Dir local_X_neg = gp.DX().Reversed();
			gp_Dir local_Y_pos = gp.DY();
			gp_Dir local_Y_neg = gp.DY().Reversed();
			gp_Dir local_Z_pos = gp.DZ();

			// 1. arc midpoint intersecting with positive X-axis (right arc point on X-axis)
			gp_Pnt local_Pnt_RightArc = new gp_Pnt( halfStraight + radius, 0, 0 );
			gp_Dir local_N1_RightArc = local_Z_pos;
			gp_Dir local_N2_RightArc = local_X_neg;
			gp_Dir local_Tan_RightArc = local_Y_neg;
			gp_Dir local_Tool_RightArc = local_Z_pos;

			// 2. long edge midpoint intersecting with negative Y-axis (bottom edge midpoint)
			gp_Pnt local_Pnt_BottomEdge = new gp_Pnt( 0, -radius, 0 );
			gp_Dir local_N1_BottomEdge = local_Z_pos;
			gp_Dir local_N2_BottomEdge = local_Y_pos;
			gp_Dir local_Tan_BottomEdge = local_X_neg;
			gp_Dir local_Tool_BottomEdge = local_Z_pos;

			// create coordinate transformation
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				centerPoint,
				plane.Axis().Direction(),
				plane.XAxis().Direction()
			);

			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );

			// transform right arc point
			CADPoint cad_RightArc = new CADPoint(
				local_Pnt_RightArc.Transformed( transformation ),
				local_N1_RightArc.Transformed( transformation ),
				local_N2_RightArc.Transformed( transformation ),
				local_Tan_RightArc.Transformed( transformation )
			);
			CAMPoint cam_RightArc = new CAMPoint( cad_RightArc, local_Tool_RightArc.Transformed( transformation ) );

			// transform bottom edge midpoint
			CADPoint cad_BottomEdge = new CADPoint(
				local_Pnt_BottomEdge.Transformed( transformation ),
				local_N1_BottomEdge.Transformed( transformation ),
				local_N2_BottomEdge.Transformed( transformation ),
				local_Tan_BottomEdge.Transformed( transformation )
			);
			CAMPoint cam_BottomEdge = new CAMPoint( cad_BottomEdge, local_Tool_BottomEdge.Transformed( transformation ) );

			List<CAMPoint> resultList = new List<CAMPoint>
			{
				cam_RightArc,
				cam_BottomEdge,
			};

			return resultList;
		}
	}
}