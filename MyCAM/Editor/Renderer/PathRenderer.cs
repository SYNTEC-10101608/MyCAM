using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.BRepBuilderAPI;
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

		public PathRenderer( Viewer viewer, ViewManager viewManager, DataManager dataManager )
			: base( viewer, dataManager )
		{
			m_ViewManager = viewManager;
		}

		public override void Show( bool bUpdate = false )
		{
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( List<string> pathIDList, bool bUpdate = false )
		{
			Remove( pathIDList );

			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			foreach( string pathID in pathIDList ) {

				// Show original path if there is any compensation or transformation applied, to visualize the difference
				ShowOriginalPath( pathID );

				IReadOnlyList<gp_Pnt> pointList = GetMainPathPointList( pathID );
				if( pointList == null || pointList.Count < 2 ) {
					continue;
				}

				TopoDS_Wire pathWire = CreatePolylineWire( pointList );
				if( pathWire == null || pathWire.IsNull() ) {
					continue;
				}

				AIS_Shape pathAIS = new AIS_Shape( pathWire );
				pathAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
				pathAIS.SetWidth( 3.0 );

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

			Remove( m_DataManager.PathIDList );

			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			foreach( string pathID in m_DataManager.PathIDList ) {
				IReadOnlyList<gp_Pnt> pointList = GetMainPathPointList( pathID );
				if( pointList == null || pointList.Count < 2 ) {
					continue;
				}

				TopoDS_Wire pathWire = CreatePolylineWire( pointList );
				if( pathWire == null || pathWire.IsNull() ) {
					continue;
				}

				AIS_Shape pathAIS = new AIS_Shape( pathWire );
				pathAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
				pathAIS.SetWidth( 3.0 );
				pathAIS.SetLocalTransformation( trsf );

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
		}

		TopoDS_Wire CreatePolylineWire( IReadOnlyList<gp_Pnt> pointList )
		{
			const double DIST_TOLERANCE = 1e-3;
			if( pointList == null || pointList.Count < 2 ) {
				return null;
			}

			BRepBuilderAPI_MakePolygon polygonMaker = new BRepBuilderAPI_MakePolygon();

			if( pointList.Count > 2 ) {

				gp_Pnt firstPoint = pointList[ 0 ];
				gp_Pnt lastPoint = pointList[ pointList.Count - 1 ];

				// check if the polyline is closed, if yes, do not add the last point again to avoid MoveTo error
				bool isClosed = firstPoint.IsEqual( lastPoint, DIST_TOLERANCE );
				int nComputedPoints = isClosed ? pointList.Count - 1 : pointList.Count;

				for( int i = 0; i < nComputedPoints; i++ ) {
					polygonMaker.Add( pointList[ i ] );
				}

				if( isClosed ) {
					polygonMaker.Close();
				}
			}

			if( !polygonMaker.IsDone() ) {
				return null;
			}

			return polygonMaker.Wire();
		}

		IReadOnlyList<gp_Pnt> GetMainPathPointList( string szPathID )
		{
			// get path type
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType ) ) {
				return new List<gp_Pnt>();
			}
			if( pathType == PathType.Contour ) {
				if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache contourCache ) ) {
					return new List<gp_Pnt>();
				}
				return ( contourCache as ContourCache )?.MainPathPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}
			else if( DataGettingHelper.IsStdPattern( pathType ) ) {
				if( !DataGettingHelper.GetStdPatternCacheByID( szPathID, out IStdPatternCache stdPatternCache ) ) {
					return new List<gp_Pnt>();
				}
				return stdPatternCache.MainPathPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}
			else {
				return new List<gp_Pnt>();
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

		void ShowOriginalPath( string pathID )
		{
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

			TopoDS_Wire pathOriWire = CreatePolylineWire( originalPointList );
			if( pathOriWire == null || pathOriWire.IsNull() ) {
				return;
			}

			AIS_Shape oriPathAIS = new AIS_Shape( pathOriWire );
			oriPathAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_DEEPSKYBLUE1 ) );
			oriPathAIS.SetWidth( 2.0 );

			m_OriginalPathAISDict.Add( pathID, oriPathAIS );
			m_Viewer.GetAISContext().Display( oriPathAIS, false );
			m_Viewer.GetAISContext().Deactivate( oriPathAIS );
		}

		bool IsIdentityTransform( gp_Trsf trsf )
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