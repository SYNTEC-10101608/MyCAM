using MyCAM.Data;
using MyCAM.Editor.Renderer;
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
		static readonly List<Quantity_NameOfColor> TECH_Layer_Color_List = new List<Quantity_NameOfColor> {
			Quantity_NameOfColor.Quantity_NOC_BLUE,
			Quantity_NameOfColor.Quantity_NOC_DARKORANGE2,
			Quantity_NameOfColor.Quantity_NOC_PURPLE,
			Quantity_NameOfColor.Quantity_NOC_YELLOW2,
			Quantity_NameOfColor.Quantity_NOC_GREEN3,
			Quantity_NameOfColor.Quantity_NOC_TOMATO2,
			Quantity_NameOfColor.Quantity_NOC_YELLOWGREEN,
			Quantity_NameOfColor.Quantity_NOC_BROWN,
			Quantity_NameOfColor.Quantity_NOC_MAGENTA1,
			Quantity_NameOfColor.Quantity_NOC_CYAN1,
		};

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

				IReadOnlyList<gp_Pnt> pointList = ToolVecAndPathVisibleHelper.GetMainPathPointList( pathID );
				if( pointList == null || pointList.Count < 2 ) {
					continue;
				}

				TopoDS_Wire pathWire = ToolVecAndPathVisibleHelper.CreatePolylineWire( pointList );
				if( pathWire == null || pathWire.IsNull() ) {
					continue;
				}

				// set AIS param
				AIS_Shape pathAIS = new AIS_Shape( pathWire );
				int nPathColorIdx = GetColorIndex( pathID );
				pathAIS.SetColor( new Quantity_Color( TECH_Layer_Color_List[ nPathColorIdx ] ) );
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
				if( craftData.CompensatedDistance == 0 && IsIdentityTransform( craftData.CumulativeTrsfMatrix ) ) {
					return;
				}
			}

			IReadOnlyList<gp_Pnt> originalPointList = GetPathOriginalCADPointList( pathID );
			if( originalPointList == null || originalPointList.Count < 2 ) {
				return;
			}

			TopoDS_Wire pathOriWire = ToolVecAndPathVisibleHelper.CreatePolylineWire( originalPointList );
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

		int GetColorIndex( string pathID )
		{
			int nColorIdx = 0;
			if( !m_DataManager.ObjectMap.TryGetValue( pathID, out var obj ) ) {
				return nColorIdx;
			}
			PathObject pathObj = obj as PathObject;
			if( pathObj == null ) {
				return nColorIdx;
			}
			bool isGetDataCraftSuccess = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData );
			if( !isGetDataCraftSuccess || craftData == null ) {
				return nColorIdx;
			}
			int nTechLayer = craftData.TechLayer;
			nColorIdx = nTechLayer - 1;
			if( nColorIdx < 0 || nColorIdx >= TECH_Layer_Color_List.Count ) {
				nColorIdx = 0;
			}
			return nColorIdx;
		}

		static bool IsIdentityTransform( gp_Trsf trsf )
		{
			const double TOLERANCE = 1e-3;
			if( trsf == null ) {
				return true;
			}

			// Check if translation part is zero
			gp_XYZ translation = trsf.TranslationPart();
			if( Math.Abs( translation.X() ) > TOLERANCE ||
				Math.Abs( translation.Y() ) > TOLERANCE ||
				Math.Abs( translation.Z() ) > TOLERANCE ) {
				return false;
			}

			// Check if scale factor is 1
			if( Math.Abs( trsf.ScaleFactor() - 1.0 ) > TOLERANCE ) {
				return false;
			}

			// Check if rotation part is identity matrix
			gp_Mat rotationMatrix = trsf.GetRotation().GetMatrix();
			for( int i = 1; i <= 3; i++ ) {
				for( int j = 1; j <= 3; j++ ) {
					double expectedValue = ( i == j ) ? 1.0 : 0.0;
					if( Math.Abs( rotationMatrix.Value( i, j ) - expectedValue ) > TOLERANCE ) {
						return false;
					}
				}
			}

			return true;
		}
	}
}
