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
		bool m_IsShow = true;

		readonly List<AIS_Shape> m_CADMarkList = new List<AIS_Shape>();
		readonly List<AIS_Shape> m_CAMMarkList = new List<AIS_Shape>();
		readonly List<AIS_Line> m_OffsetLineList = new List<AIS_Line>();
		readonly List<AIS_Line> m_EndpointPathList = new List<AIS_Line>();
		readonly List<AIS_Shape> m_EndpointMarkList = new List<AIS_Shape>();

		public EditPointRenderer( Viewer viewer, string pathID )
		{
			m_Viewer = viewer;
			m_PathID = pathID;
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

			ShowNormalControlPoints( contourCache, craftData );
			ShowEndpointEditedPath( contourCache );

			if( bUpdate ) {
				UpdateView();
			}
		}

		// Compare each CAM point with its CAD source point to find the segments
		void ShowEndpointEditedPath( ContourCache contourCache )
		{
			List<CAMPoint> camPointList = contourCache.MainPathPointList;
			List<CADPoint> cadPointList = contourCache.TrsfCADPointList;
			if( camPointList == null || cadPointList == null || camPointList.Count < 2 ) {
				return;
			}

			// build the reverse map once: ori CAD index -> offset index
			Dictionary<int, int> oriToOffsetMap = BuildOriToOffsetMap( contourCache.OriginalIndexMap );

			// resolve the position before the endpoint edit for every CAM point
			List<gp_Pnt> beforePointList = new List<gp_Pnt>( new gp_Pnt[ camPointList.Count ] );
			List<bool> movedList = new List<bool>( new bool[ camPointList.Count ] );
			bool hasMoved = false;
			for( int i = 0; i < camPointList.Count; i++ ) {
				if( !TryGetBeforeEditPoint( camPointList, cadPointList, oriToOffsetMap, i, out gp_Pnt beforePoint ) ) {
					continue;
				}
				beforePointList[ i ] = beforePoint;
				movedList[ i ] = IsPointMoved( beforePoint, camPointList[ i ].Point );
				hasMoved |= movedList[ i ];
			}
			if( !hasMoved ) {
				return;
			}

			var aisContext = m_Viewer.GetAISContext();

			// draw base path are with comparison
			int lastIndex = camPointList.Count - 1;
			for( int i = 0; i < camPointList.Count - 1; i++ ) {

				// skipped when either end has no CAD source
				if( beforePointList[ i ] == null || beforePointList[ i + 1 ] == null ) {
					continue;
				}
				AIS_Line baseSegment = DrawHelper.GetLineAIS(
					beforePointList[ i ], beforePointList[ i + 1 ], BASE_PATH_COLOR, BASE_PATH_WIDTH, true );
				aisContext.Display( baseSegment, false );
				aisContext.Deactivate( baseSegment );
				m_EndpointPathList.Add( baseSegment );
			}

			// draw the edited start point and end point with their reference lines
			ShowEndpointMark( beforePointList[ 0 ], camPointList[ 0 ].Point, movedList[ 0 ], START_POINT_COLOR );
			ShowEndpointMark( beforePointList[ lastIndex ], camPointList[ lastIndex ].Point, movedList[ lastIndex ], END_POINT_COLOR );
		}

		// The CAD point that the given CAM point comes from, i.e. its position before the endpoint edit.
		bool TryGetBeforeEditPoint(
			List<CAMPoint> camPointList,
			List<CADPoint> cadPointList,
			Dictionary<int, int> oriToOffsetMap,
			int camIndex,
			out gp_Pnt beforePoint )
		{
			beforePoint = null;
			int oriCADIndex = camPointList[ camIndex ].InitPathIndex;

			// the closed point is a clone of the path start point, so it shares the same CAD source
			if( oriCADIndex == SpecialPointIndex.CLOSED_POINT_INDEX ) {
				oriCADIndex = camPointList[ 0 ].InitPathIndex;
			}

			// offset or micro joint generated points have no CAD source, just skip them
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

		Dictionary<int, int> BuildOriToOffsetMap( List<int> originalIndexMap )
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

		void ShowEndpointMark( gp_Pnt beforePoint, gp_Pnt afterPoint, bool isMoved, Quantity_NameOfColor color )
		{
			// no mark and no zero length line when the endpoint is not moved
			if( beforePoint == null || !isMoved ) {
				return;
			}

			var aisContext = m_Viewer.GetAISContext();

			AIS_Shape endpointMark = CreatePointMark( afterPoint, color );
			aisContext.Display( endpointMark, false );
			aisContext.Deactivate( endpointMark );
			m_CAMMarkList.Add( endpointMark );

			// connect back to the existing path, the original endpoint is not redrawn
			AIS_Line referenceLine = DrawHelper.GetLineAIS( beforePoint, afterPoint, color, OFFSET_LINE_WIDTH, true );
			aisContext.Display( referenceLine, false );
			aisContext.Deactivate( referenceLine );
			m_OffsetLineList.Add( referenceLine );
		}

		static bool IsPointMoved( gp_Pnt beforePoint, gp_Pnt afterPoint )
		{
			return beforePoint.Distance( afterPoint ) > BASE_PATH_DISPLAY_TOLERANCE;
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

			foreach( AIS_Shape mark in m_EndpointMarkList ) {
				aisContext.Remove( mark, false );
			}
			m_EndpointMarkList.Clear();

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

		// Draw the normal contour edit control points:
		void ShowNormalControlPoints( ContourCache contourCache, CraftData craftData )
		{
			var aisContext = m_Viewer.GetAISContext();
			var cadPointList = contourCache.TrsfCADPointList;
			var originalIndexMap = contourCache.OriginalIndexMap;

			foreach( var kvp in craftData.ContourEditMap ) {
				int oriCADIndex = kvp.Key;
				ContourEditData modifyData = kvp.Value;

				// convert ori CAD index to offset index
				int offsetIndex = FindOffsetIndex( originalIndexMap, oriCADIndex );
				if( offsetIndex < 0 || offsetIndex >= cadPointList.Count ) {
					continue;
				}

				// TrsfCADPointList now contains the final result (offset + transform + edit)
				// camPnt = final position (already includes displacement)
				// cadPnt = base position (final position minus displacement)
				gp_Pnt camPnt = cadPointList[ offsetIndex ].Point;
				gp_Pnt cadPnt = new gp_Pnt(
					camPnt.X() - modifyData.DX,
					camPnt.Y() - modifyData.DY,
					camPnt.Z() - modifyData.DZ );

				// draw CAD mark
				AIS_Shape cadMark = CreatePointMark( cadPnt, CAD_MARK_COLOR );
				aisContext.Display( cadMark, false );
				aisContext.Deactivate( cadMark );
				m_CADMarkList.Add( cadMark );

				// skip CAM mark and line when offset is negligible
				bool hasOffset = Math.Abs( modifyData.DX ) > OFFSET_THRESHOLD
							  || Math.Abs( modifyData.DY ) > OFFSET_THRESHOLD
							  || Math.Abs( modifyData.DZ ) > OFFSET_THRESHOLD;
				if( !hasOffset ) {
					continue;
				}

				// draw CAM mark
				AIS_Shape camMark = CreatePointMark( camPnt, CAM_MARK_COLOR );
				aisContext.Display( camMark, false );
				aisContext.Deactivate( camMark );
				m_CAMMarkList.Add( camMark );

				// draw offset line between CAD and CAM point
				AIS_Line offsetLine = DrawHelper.GetLineAIS( cadPnt, camPnt, OFFSET_LINE_COLOR, OFFSET_LINE_WIDTH, true );
				aisContext.Display( offsetLine, false );
				aisContext.Deactivate( offsetLine );
				m_OffsetLineList.Add( offsetLine );
			}
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

		const Quantity_NameOfColor START_POINT_COLOR = Quantity_NameOfColor.Quantity_NOC_PINK;
		const Quantity_NameOfColor END_POINT_COLOR = Quantity_NameOfColor.Quantity_NOC_GREEN;

		// the path before the start / end point edit
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
