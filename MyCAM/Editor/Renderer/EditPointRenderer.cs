using System;
using System.Collections.Generic;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;

namespace MyCAM.Editor.Renderer
{
	/// <summary>
	/// Renderer for contour edit control points.
	/// Displays CAD-space marks and (when offset exists) CAM-space marks with a connecting line.
	/// </summary>
	internal class EditPointRenderer : ICAMRenderer
	{
		readonly Viewer m_Viewer;
		readonly string m_PathID;
		readonly IReadOnlyList<gp_Pnt> m_OriginalTrsfCADPointList;
		bool m_IsShow = true;

		readonly List<AIS_Shape> m_CADMarkList = new List<AIS_Shape>();
		readonly List<AIS_Shape> m_CAMMarkList = new List<AIS_Shape>();
		readonly List<AIS_Line> m_OffsetLineList = new List<AIS_Line>();
		readonly List<AIS_Line> m_EndpointPathList = new List<AIS_Line>();

		public EditPointRenderer( Viewer viewer, string pathID, IReadOnlyList<gp_Pnt> originalTrsfCADPointList )
		{
			m_Viewer = viewer;
			m_PathID = pathID;
			m_OriginalTrsfCADPointList = originalTrsfCADPointList;
		}

		public void Show( bool bUpdate = false )
		{
			Remove();

			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			if( !DataGettingHelper.GetContourCacheByID( m_PathID, out ContourCache contourCache ) ) {
				return;
			}
			if( !DataGettingHelper.GetCraftDataByID( m_PathID, out CraftData craftData ) ) {
				return;
			}

			ShowControlPoints( contourCache, craftData );
			ShowEndpointEditedPath( contourCache );

			if( bUpdate ) {
				UpdateView();
			}
		}

		// The reference path includes normal control point edits but excludes endpoint edits.
		private void ShowEndpointEditedPath( ContourCache contourCache )
		{
			List<CAMPoint> camPointList = contourCache.MainPathPointList;
			List<CADPoint> cadPointList = contourCache.TrsfCADPointList;
			if( camPointList == null || cadPointList == null || camPointList.Count < 2 ) {
				return;
			}

			// CAM path order can differ from the offset CAD point order.
			Dictionary<int, int> oriToOffsetMap = BuildOriToOffsetMap( contourCache.OriginalIndexMap );
			List<gp_Pnt> beforePointList = new List<gp_Pnt>( new gp_Pnt[ camPointList.Count ] );
			bool hasMoved = false;
			for( int i = 0; i < camPointList.Count; i++ ) {
				if( !TryGetBeforeEditPoint( camPointList, cadPointList, oriToOffsetMap, i, out gp_Pnt beforePoint ) ) {
					continue;
				}
				beforePointList[ i ] = beforePoint;
				hasMoved |= beforePoint.Distance( camPointList[ i ].Point ) > BASE_PATH_DISPLAY_TOLERANCE;
			}
			if( !hasMoved ) {
				return;
			}

			var aisContext = m_Viewer.GetAISContext();
			for( int i = 0; i < camPointList.Count - 1; i++ ) {
				if( beforePointList[ i ] == null || beforePointList[ i + 1 ] == null ) {
					continue;
				}
				AIS_Line baseSegment = DrawHelper.GetLineAIS(
					beforePointList[ i ], beforePointList[ i + 1 ], BASE_PATH_COLOR, BASE_PATH_WIDTH, true );
				aisContext.Display( baseSegment, false );
				aisContext.Deactivate( baseSegment );
				m_EndpointPathList.Add( baseSegment );
			}
		}

		private bool TryGetBeforeEditPoint(
			List<CAMPoint> camPointList,
			List<CADPoint> cadPointList,
			Dictionary<int, int> oriToOffsetMap,
			int camIndex,
			out gp_Pnt beforePoint )
		{
			beforePoint = null;
			int oriCADIndex = camPointList[ camIndex ].InitPathIndex;

			// The closing CAM point shares its CAD source with the start point.
			if( oriCADIndex == SpecialPointIndex.CLOSED_POINT_INDEX ) {
				oriCADIndex = camPointList[ 0 ].InitPathIndex;
			}

			// Generated points lack a unique original index, and missing indices cannot be mapped.
			if( oriCADIndex < 0 || !oriToOffsetMap.ContainsKey( oriCADIndex ) ) {
				return false;
			}

			int offsetIndex = oriToOffsetMap[ oriCADIndex ];
			if( offsetIndex < 0 || offsetIndex >= cadPointList.Count ) {
				return false;
			}
			beforePoint = cadPointList[ offsetIndex ].Point;
			return true;
		}

		private Dictionary<int, int> BuildOriToOffsetMap( List<int> originalIndexMap )
		{
			Dictionary<int, int> oriToOffsetMap = new Dictionary<int, int>();
			if( originalIndexMap == null ) {
				return oriToOffsetMap;
			}
			for( int i = 0; i < originalIndexMap.Count; i++ ) {
				int oriIdx = originalIndexMap[ i ];
				if( oriIdx >= 0 && !oriToOffsetMap.ContainsKey( oriIdx ) ) {
					oriToOffsetMap[ oriIdx ] = i;
				}
			}
			return oriToOffsetMap;
		}

		public void Remove( bool bUpdate = false )
		{
			var aisContext = m_Viewer.GetAISContext();

			foreach( AIS_Shape mark in m_CADMarkList ) {
				aisContext.Remove( mark, false );
			}
			m_CADMarkList.Clear();

			foreach( AIS_Shape mark in m_CAMMarkList ) {
				aisContext.Remove( mark, false );
			}
			m_CAMMarkList.Clear();

			foreach( AIS_Line line in m_OffsetLineList ) {
				aisContext.Remove( line, false );
			}
			m_OffsetLineList.Clear();

			foreach( AIS_Line line in m_EndpointPathList ) {
				aisContext.Remove( line, false );
			}
			m_EndpointPathList.Clear();

			if( bUpdate ) {
				UpdateView();
			}
		}

		public void SetShow( bool isShow )
		{
			m_IsShow = isShow;
		}

		public void SetPauseRefreshAndHide( bool isPause )
		{
			// EditPointRenderer does not need to implement pause functionality
		}

		public void UpdateView()
		{
			m_Viewer.UpdateView();
		}

		AIS_Shape CreatePointMark( gp_Pnt point, Quantity_NameOfColor color )
		{
			BRepBuilderAPI_MakeVertex mkVertex = new BRepBuilderAPI_MakeVertex( point );
			TopoDS_Vertex vertex = mkVertex.Vertex();
			return ViewHelper.CreateFeatureAIS( vertex, color );
		}

		private void ShowControlPoints( ContourCache contourCache, CraftData craftData )
		{
			List<CAMPoint> camPointList = contourCache.MainPathPointList;
			if( camPointList.Count == 0 ) {
				return;
			}
			List<int> originalIndexMap = contourCache.OriginalIndexMap;
			StartPntContourEditParam endpointData = craftData.StartPntContourEditData;
			int startOriginalIndex = camPointList[ 0 ].InitPathIndex;
			for( int i = 0; i < camPointList.Count; i++ ) {
				CAMPoint camPoint = camPointList[ i ];
				int originalIndex = camPoint.InitPathIndex;
				bool isEndpoint = ( i == 0 && endpointData.StartPnt != null )
					|| ( i == camPointList.Count - 1 && endpointData.EndPnt != null );
				bool isNormalControlPoint = craftData.ContourEditMap.ContainsKey( originalIndex );
				if( !isEndpoint && !isNormalControlPoint ) {
					continue;
				}

				// The closing CAM point shares its original CAD index with the start point.
				if( isEndpoint && SpecialPointIndex.IsClosedPointIndex( originalIndex ) ) {
					originalIndex = startOriginalIndex;
				}
				int offsetIndex = FindOffsetIndex( originalIndexMap, originalIndex );
				if( offsetIndex < 0 && !isEndpoint ) {
					continue;
				}
				if( offsetIndex < 0 || offsetIndex >= m_OriginalTrsfCADPointList.Count ) {
					string szMessage = isEndpoint ? "The original contour endpoint could not be found."
						: "The original contour control point could not be found.";

					// A later invalid point must not leave earlier marks visible.
					Remove();
					throw new InvalidOperationException( szMessage );
				}

				// Resolve repeated indices the same way as point selection.
				bool isMappedControlPoint = originalIndex == startOriginalIndex
					? i == 0
					: contourCache.CADToCAMIndexMap.TryGetValue( originalIndex, out int camIndex ) && i == camIndex;
				if( !isEndpoint && !isMappedControlPoint ) {
					continue;
				}
				ShowControlPoint( m_OriginalTrsfCADPointList[ offsetIndex ], camPoint.Point );
			}
		}

		private void ShowControlPoint( gp_Pnt originalPoint, gp_Pnt currentPoint )
		{
			var aisContext = m_Viewer.GetAISContext();
			AIS_Shape cadMark = CreatePointMark( originalPoint, CAD_MARK_COLOR );
			aisContext.Display( cadMark, false );
			aisContext.Deactivate( cadMark );
			m_CADMarkList.Add( cadMark );

			gp_Vec displacement = new gp_Vec( originalPoint, currentPoint );
			bool hasOffset = Math.Abs( displacement.X() ) > OFFSET_THRESHOLD
				|| Math.Abs( displacement.Y() ) > OFFSET_THRESHOLD
				|| Math.Abs( displacement.Z() ) > OFFSET_THRESHOLD;
			if( !hasOffset ) {
				return;
			}

			AIS_Shape camMark = CreatePointMark( currentPoint, CAM_MARK_COLOR );
			aisContext.Display( camMark, false );
			aisContext.Deactivate( camMark );
			m_CAMMarkList.Add( camMark );

			AIS_Line offsetLine = DrawHelper.GetLineAIS(
				originalPoint, currentPoint, OFFSET_LINE_COLOR, OFFSET_LINE_WIDTH, true );
			aisContext.Display( offsetLine, false );
			aisContext.Deactivate( offsetLine );
			m_OffsetLineList.Add( offsetLine );
		}

		int FindOffsetIndex( List<int> originalIndexMap, int oriCADIndex )
		{
			if( originalIndexMap == null ) {
				return oriCADIndex;
			}
			for( int i = 0; i < originalIndexMap.Count; i++ ) {
				if( originalIndexMap[ i ] == oriCADIndex ) {
					return i;
				}
			}
			return -1;
		}

		const Quantity_NameOfColor BASE_PATH_COLOR = Quantity_NameOfColor.Quantity_NOC_MAGENTA2;
		const double BASE_PATH_WIDTH = 2.0;
		const double BASE_PATH_DISPLAY_TOLERANCE = 1e-2;

		const Quantity_NameOfColor CAD_MARK_COLOR = Quantity_NameOfColor.Quantity_NOC_ORANGE;
		const Quantity_NameOfColor CAM_MARK_COLOR = Quantity_NameOfColor.Quantity_NOC_BLUE;
		const Quantity_NameOfColor OFFSET_LINE_COLOR = Quantity_NameOfColor.Quantity_NOC_BLACK;
		const double OFFSET_LINE_WIDTH = 1.5;
		const double OFFSET_THRESHOLD = 1e-3;
	}
}
