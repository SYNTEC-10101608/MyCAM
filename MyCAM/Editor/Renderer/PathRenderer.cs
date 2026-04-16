using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.gp;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Editor
{
	internal class PathRenderer : CAMRendererBase
	{
		readonly Dictionary<string, AIS_Shape> m_MainPathAISDict = new Dictionary<string, AIS_Shape>();
		readonly Dictionary<string, AIS_Shape> m_OriginalPathAISDict = new Dictionary<string, AIS_Shape>();
		ViewManager m_ViewManager;
		bool m_IsPauseRefresh = false;

		public PathRenderer( Viewer viewer, ViewManager viewManager, DataManager dataManager )
			: base( viewer, dataManager )
		{
			m_ViewManager = viewManager;
		}

		public void SetPauseRefresh( bool isPause )
		{
			if( m_IsPauseRefresh == isPause ) {
				return;
			}
			m_IsPauseRefresh = isPause;
			if( isPause ) {
				// hide all managed AIS objects without destroying them
				foreach( var kvp in m_MainPathAISDict ) {
					m_Viewer.GetAISContext().Erase( kvp.Value, false );
				}
				foreach( var kvp in m_OriginalPathAISDict ) {
					m_Viewer.GetAISContext().Erase( kvp.Value, false );
				}
			}
			else {
				// re-display all managed AIS objects
				foreach( var kvp in m_MainPathAISDict ) {
					m_Viewer.GetAISContext().Display( kvp.Value, false );
				}
				foreach( var kvp in m_OriginalPathAISDict ) {
					m_Viewer.GetAISContext().Display( kvp.Value, false );
					m_Viewer.GetAISContext().Deactivate( kvp.Value );
				}
			}
		}

		public override void Show( bool bUpdate = false )
		{
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( List<string> pathIDList, bool bUpdate = false )
		{
			ShowSpecifyPath( pathIDList, bUpdate );
		}

		public override void Remove( bool bUpdate = false )
		{
			Remove( m_DataManager.PathIDList, bUpdate );
		}

		public void Remove( List<string> pathIDList, bool bUpdate = false )
		{
			RemovePaths( pathIDList );
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void ShowTrans( gp_Trsf trsf, bool bUpdate = false )
		{
			ShowSpecifyPath( m_DataManager.PathIDList, bUpdate, trsf );
		}

		void ShowSpecifyPath( List<string> pathIDList, bool bUpdate, gp_Trsf trsf = null )
		{
			// paused, do not rebuild or display
			if( m_IsPauseRefresh ) {
				return;
			}

			Remove( pathIDList );
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			// render each path
			foreach( string pathID in pathIDList ) {

				// Show original path if there is any compensation or transformation applied, to visualize the difference
				ShowOriginalPath( pathID, trsf );

				IReadOnlyList<gp_Pnt> pointList = RendererHelper.GetMainPathPointList( pathID );
				if( pointList == null || pointList.Count < 2 ) {
					continue;
				}

				TopoDS_Wire pathWire = RendererHelper.CreatePolylineWire( pointList );
				if( pathWire == null || pathWire.IsNull() ) {
					continue;
				}

				// set AIS param
				AIS_Shape pathAIS = new AIS_Shape( pathWire );
				int nPathColorIdx = RendererHelper.GetColorIndex( pathID, m_DataManager );
				pathAIS.SetColor( new Quantity_Color( RendererHelper.TECH_Layer_Color_List[ nPathColorIdx ] ) );
				pathAIS.SetWidth( 3.0 );
				if( trsf != null ) {
					pathAIS.SetLocalTransformation( trsf );
				}

				// Register to DataManager for shape-ID mapping
				m_DataManager.RegisterShapeIDMapping( pathWire, pathID );

				// Local storage
				m_MainPathAISDict.Add( pathID, pathAIS );

				if( m_ViewManager.ViewObjectMap.ContainsKey( pathID ) ) {
					m_ViewManager.ViewObjectMap.Remove( pathID );
				}
				m_ViewManager.ViewObjectMap.Add( pathID, new ViewObject( pathAIS ) );
				m_Viewer.GetAISContext().Display( pathAIS, false );
			}

			if( bUpdate ) {
				UpdateView();
			}
		}

		public void Reset( bool bUpdate = false )
		{
			gp_Trsf theTrsf = new gp_Trsf();
			ShowTrans( theTrsf, bUpdate );
			if( bUpdate ) {
				UpdateView();
			}
		}

		void RemovePaths( List<string> pathIDList )
		{
			// Unregister from DataManager
			foreach( string pathID in pathIDList ) {
				TopoDS_Wire wire = GetWireFromPathID( pathID );
				if( wire != null && !wire.IsNull() ) {
					m_DataManager.UnregisterShapeIDMapping( wire );
				}
			}

			foreach( string pathID in pathIDList ) {
				if( m_MainPathAISDict.TryGetValue( pathID, out AIS_Shape pathAIS ) ) {
					m_Viewer.GetAISContext().Remove( pathAIS, false );
					m_MainPathAISDict.Remove( pathID );
				}
			}

			foreach( string pathID in pathIDList ) {
				if( m_OriginalPathAISDict.TryGetValue( pathID, out AIS_Shape oriPathAIS ) ) {
					m_Viewer.GetAISContext().Remove( oriPathAIS, false );
					m_OriginalPathAISDict.Remove( pathID );
				}
			}
		}

		TopoDS_Wire GetWireFromPathID( string pathID )
		{
			if( !m_MainPathAISDict.TryGetValue( pathID, out AIS_Shape pathAIS ) ) {
				return null;
			}

			TopoDS_Shape shape = pathAIS.Shape();
			if( shape == null || shape.IsNull() || shape.ShapeType() != TopAbs_ShapeEnum.TopAbs_WIRE ) {
				return null;
			}

			return TopoDS.ToWire( shape );
		}

		IReadOnlyList<gp_Pnt> GetPathOriginalCADPointList( string pathID )
		{
			IReadOnlyList<gp_Pnt> pathlist = new List<gp_Pnt>();
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return new List<gp_Pnt>();
			}
			if( !DataGettingHelper.GetPathObject( pathID, out PathObject pathObject ) ) {
				return new List<gp_Pnt>();
			}
			if( pathType == PathType.Contour ) {
				ContourPathObject contourPathObject = pathObject as ContourPathObject;
				pathlist = contourPathObject?.GeomData.CADPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}
			else if( DataGettingHelper.IsStdPattern( pathType ) ) {
				StdPatternObjectBase stdPatternPathObject = pathObject as StdPatternObjectBase;
				pathlist = stdPatternPathObject?.ContourPathObject?.GeomData?.CADPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}
			else {
				return new List<gp_Pnt>();
			}
			return pathlist;
		}

		void ShowOriginalPath( string pathID, gp_Trsf trsf = null )
		{
			trsf = trsf ?? new gp_Trsf();
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				return;
			}

			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return;
			}

			if( pathType == PathType.Contour ) {
				if( craftData.CompensatedDistance == 0 && RendererHelper.IsIdentityTransform( craftData.CumulativeTrsfMatrix ) ) {
					return;
				}
			}

			IReadOnlyList<gp_Pnt> originalPointList = GetPathOriginalCADPointList( pathID );
			if( originalPointList == null || originalPointList.Count < 2 ) {
				return;
			}

			TopoDS_Wire pathOriWire = RendererHelper.CreatePolylineWire( originalPointList );
			if( pathOriWire == null || pathOriWire.IsNull() ) {
				return;
			}

			AIS_Shape oriPathAIS = new AIS_Shape( pathOriWire );
			oriPathAIS.SetLocalTransformation( trsf );
			oriPathAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_DEEPSKYBLUE1 ) );
			oriPathAIS.SetWidth( 2.0 );

			m_OriginalPathAISDict.Add( pathID, oriPathAIS );
			m_Viewer.GetAISContext().Display( oriPathAIS, false );
			m_Viewer.GetAISContext().Deactivate( oriPathAIS );
		}
	}
}
