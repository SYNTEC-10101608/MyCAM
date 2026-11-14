using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRepBuilderAPI;
using OCC.GC;
using OCC.Geom;
using OCC.gp;
using OCC.Prs3d;
using OCC.Quantity;
using OCCViewer;

namespace MyCAM.Editor.Renderer
{
	internal class TraverseRenderer : CAMRendererBase
	{

		public TraverseRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void SetPauseRefreshAndHide( bool isPause )
		{
			if( m_IsPauseRefreshAndHide == isPause ) {
				return;
			}
			base.SetPauseRefreshAndHide( isPause );
			if( isPause ) {
				// hide all managed AIS objects without destroying them
				foreach( var kvp in m_TraverseAISDict ) {
					foreach( AIS_Line lineAIS in kvp.Value ) {
						m_Viewer.GetAISContext().Erase( lineAIS, false );
					}
				}
				foreach( var kvp in m_FrogLeapAISDict ) {
					foreach( AIS_Shape shapeAIS in kvp.Value ) {
						m_Viewer.GetAISContext().Erase( shapeAIS, false );
					}
				}
			}
			else {
				// re-display all managed AIS objects
				foreach( var kvp in m_TraverseAISDict ) {
					foreach( AIS_Line lineAIS in kvp.Value ) {
						m_Viewer.GetAISContext().Display( lineAIS, false );
						m_Viewer.GetAISContext().Deactivate( lineAIS );
					}
				}
				foreach( var kvp in m_FrogLeapAISDict ) {
					foreach( AIS_Shape shapeAIS in kvp.Value ) {
						m_Viewer.GetAISContext().Display( shapeAIS, false );
						m_Viewer.GetAISContext().Deactivate( shapeAIS );
					}
				}
			}
		}

		public override void Show( bool bUpdate = false )
		{
			ShowTrans( null, bUpdate );
		}

		public override void Remove( bool bUpdate = false )
		{
			List<string> pathIDs = m_TraverseAISDict.Keys.Union( m_FrogLeapAISDict.Keys ).ToList();
			RemoveTraverseLines( pathIDs );
			if( bUpdate ) {
				UpdateView();
			}
		}


		public void ShowTrans( gp_Trsf trsf, bool bUpdate = false )
		{
			// paused, do not rebuild or display
			if( m_IsPauseRefreshAndHide ) {
				return;
			}
			Remove();
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}
			List<string> pathIDList = m_DataManager.GetProcessPathIDList( m_DataManager.PathIDList );

			ShowTraversePathWithTrans( pathIDList, trsf );
			ShowInternalPathsWithTrans( pathIDList, trsf );
			ShowEntryAndExitWithTrans( pathIDList, trsf );
			DisplayTraverseLines( pathIDList );
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void Trans( gp_Trsf trsf, bool bUpdate = false )
		{
			// if no AIS objects exist, fall back to full rebuild
			if( m_TraverseAISDict.Count == 0 && m_FrogLeapAISDict.Count == 0 ) {
				ShowTrans( trsf, bUpdate );
				return;
			}

			// update transform on existing traverse line AIS objects
			foreach( var tra in m_TraverseAISDict ) {
				List<AIS_Line> lineLIst = tra.Value;
				foreach( AIS_Line line in lineLIst ) {
					if( line != null && trsf != null ) {
						line.SetLocalTransformation( trsf );
					}
				}
			}

			// update transform on existing frog leap AIS objects
			foreach( var kvp in m_FrogLeapAISDict ) {
				foreach( AIS_Shape shapeAIS in kvp.Value ) {
					if( shapeAIS != null && trsf != null ) {
						shapeAIS.SetLocalTransformation( trsf );
					}
				}
			}

			if( bUpdate ) {
				UpdateView();
			}
		}

		public void Reset( bool bUpdate = false )
		{
			gp_Trsf trsf = new gp_Trsf();
			ShowTrans( trsf, bUpdate );
			if( bUpdate ) {
				UpdateView();
			}
		}

		void ShowTraversePathWithTrans( IReadOnlyList<string> pathIDList, gp_Trsf trsf )
		{
			for( int i = 1; i < pathIDList.Count; ++i ) {
				AddTraverse( pathIDList[ i - 1 ], pathIDList[ i ], trsf );
			}
		}

		void ShowInternalPathsWithTrans( IReadOnlyList<string> pathIDList, gp_Trsf trsf )
		{
			foreach( string pathID in pathIDList ) {
				if( DataGettingHelper.GetPathObject( pathID, out IPathObject pathObject ) && pathObject is FlyCutGroupPathObject group ) {
					AddFlyCut( group, trsf );
				}
			}
		}

		void AddFlyCut( FlyCutGroupPathObject group, gp_Trsf trsf )
		{
			List<AIS_Line> traverseLineList = new List<AIS_Line>();
			foreach( FlyCutGroupPathCache.FlyStepCache flyStep in group.FlyCutCache.FlyCutStepCacheList.OfType<FlyCutGroupPathCache.FlyStepCache>() ) {
				List<gp_Pnt> points = flyStep.PointList;
				for( int j = 1; j < points.Count; ++j ) {
					AddOneLinearTraverseWithTrans( traverseLineList, points[ j - 1 ], points[ j ], trsf, FLYCUT_COLOR );
				}
			}
			if( traverseLineList.Count == 0 ) {
				return;
			}
			if( !m_TraverseAISDict.ContainsKey( group.UID ) ) {
				m_TraverseAISDict[ group.UID ] = traverseLineList;
			}
			else {
				m_TraverseAISDict[ group.UID ].AddRange( traverseLineList );
			}
		}



		void AddTraverse( string previousPathID, string currentPathID, gp_Trsf trsf )
		{
			TraverseCalculationInfo calculationInfo = TraverseHelper.BuildTraverseCalculationInfo( m_DataManager );
			if( !DataGettingHelper.GetCraftDataByID( currentPathID, out CraftData craftData ) ) {
				return;
			}

			TraverseData currentTraverseData = craftData.TraverseData;

			// Get process points
			IProcessPoint previousEndPoint = GetProcessEndPoint( previousPathID );
			IProcessPoint currentStartPoint = GetProcessStartPoint( currentPathID );

			// Calculate all traverse points using helper
			if( !TraverseHelper.TryCalculateTraversePoints( previousEndPoint, currentStartPoint, currentTraverseData, calculationInfo, out TraversePathResult result ) ) {
				return;
			}

			List<AIS_Line> traverseLineList = new List<AIS_Line>();
			List<AIS_Shape> frogLeapShapeList = new List<AIS_Shape>();

			// frog leap 
			if( currentTraverseData.TraverseMode == TraverseData.TraverseType.FrogLeap && currentTraverseData.FrogLeapDistance > 0
				&& result.FrogLeapMiddlePoint != null && result.LiftUpPoint.Point != null && result.CutDownPoint.Point != null ) {
				GC_MakeArcOfCircle makeCircle = new GC_MakeArcOfCircle( result.LiftUpPoint.Point, result.FrogLeapMiddlePoint.Point, result.CutDownPoint.Point );
				if( makeCircle.IsDone() ) {
					Geom_TrimmedCurve arcCurve = makeCircle.Value();
					BRepBuilderAPI_MakeEdge makeEdge = new BRepBuilderAPI_MakeEdge( arcCurve );
					AIS_Shape arcAIS = new AIS_Shape( makeEdge.Shape() );
					arcAIS.SetColor( new Quantity_Color( TRAVERSE_COLOR ) );
					arcAIS.SetWidth( 1 );
					arcAIS.SetTransparency( 1 );
					Prs3d_LineAspect prs3D_LineAspect = new Prs3d_LineAspect( new Quantity_Color( TRAVERSE_COLOR ), Aspect_TypeOfLine.Aspect_TOL_DASH, 1 );
					arcAIS.Attributes().SetWireAspect( prs3D_LineAspect );
					if( trsf != null ) {
						arcAIS.SetLocalTransformation( trsf );
					}
					frogLeapShapeList.Add( arcAIS );

					// lift up
					if( currentTraverseData.LiftUpDistance > 0 && result.PreviousPathEnd.Point != null ) {
						AddOneLinearTraverseWithTrans( traverseLineList, result.PreviousPathEnd.Point, result.LiftUpPoint.Point, trsf, TRAVERSE_COLOR );
					}

					// cut down
					if( currentTraverseData.CutDownDistance > 0 && result.CurrentPathStart.Point != null ) {
						AddOneLinearTraverseWithTrans( traverseLineList, result.CutDownPoint.Point, result.CurrentPathStart.Point, trsf, TRAVERSE_COLOR );
					}
				}
				else {
					List<(gp_Pnt start, gp_Pnt end)> segments = GetTraverseLineSegments( result, currentTraverseData );
					foreach( var seg in segments ) {
						AddOneLinearTraverseWithTrans( traverseLineList, seg.start, seg.end, trsf, TRAVERSE_COLOR );
					}
				}
			}
			// safe plane or normal traverse
			else {
				List<(gp_Pnt start, gp_Pnt end)> segments = GetTraverseLineSegments( result, currentTraverseData );
				foreach( var seg in segments ) {
					AddOneLinearTraverseWithTrans( traverseLineList, seg.start, seg.end, trsf, TRAVERSE_COLOR );
				}
			}

			if( traverseLineList.Count > 0 ) {
				m_TraverseAISDict[ currentPathID ] = traverseLineList;
			}
			if( frogLeapShapeList.Count > 0 ) {
				m_FrogLeapAISDict[ currentPathID ] = frogLeapShapeList;
			}
		}

		void ShowEntryAndExitWithTrans( IReadOnlyList<string> pathIDList, gp_Trsf trsf )
		{
			if( pathIDList.Count == 0 ) {
				return;
			}

			// entry
			if( m_DataManager.EntryAndExitData.EntryDistance > 0 ) {
				string firstPathID = pathIDList.First();
				IProcessPoint firstPathStartPoint = GetProcessStartPoint( firstPathID );
				if( firstPathStartPoint != null ) {
					IProcessPoint entryPoint = TraverseHelper.GetCutDownOrLiftUpPoint( firstPathStartPoint.Clone(), m_DataManager.EntryAndExitData.EntryDistance );
					if( entryPoint != null ) {
						List<AIS_Line> entryLineList = new List<AIS_Line>();
						AddOneLinearTraverseWithTrans( entryLineList, entryPoint.Point, firstPathStartPoint.Point, trsf, TRAVERSE_COLOR );
						m_TraverseAISDict[ KEY_ENTRY ] = entryLineList;
					}
				}
			}

			// exit
			if( m_DataManager.EntryAndExitData.ExitDistance > 0 ) {
				string lastPathID = pathIDList.Last();
				IProcessPoint lastPathEndPoint = GetProcessEndPoint( lastPathID );
				if( lastPathEndPoint != null ) {
					IProcessPoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( lastPathEndPoint.Clone(), m_DataManager.EntryAndExitData.ExitDistance );
					if( exitPoint != null ) {
						List<AIS_Line> exitLineList = new List<AIS_Line>();
						AddOneLinearTraverseWithTrans( exitLineList, lastPathEndPoint.Point, exitPoint.Point, trsf, TRAVERSE_COLOR );
						m_TraverseAISDict[ KEY_EXIT ] = exitLineList;
					}
				}
			}
		}

		void DisplayTraverseLines( IReadOnlyList<string> pathIDList )
		{
			foreach( string pathID in pathIDList ) {
				if( m_TraverseAISDict.ContainsKey( pathID ) ) {
					foreach( AIS_Line lineAIS in m_TraverseAISDict[ pathID ] ) {
						m_Viewer.GetAISContext().Display( lineAIS, false );
						m_Viewer.GetAISContext().Deactivate( lineAIS );
					}
				}
				if( m_FrogLeapAISDict.ContainsKey( pathID ) ) {
					foreach( AIS_Shape shapeAIS in m_FrogLeapAISDict[ pathID ] ) {
						m_Viewer.GetAISContext().Display( shapeAIS, false );
						m_Viewer.GetAISContext().Deactivate( shapeAIS );
					}
				}
			}

			if( m_TraverseAISDict.ContainsKey( KEY_ENTRY ) ) {
				foreach( AIS_Line lineAIS in m_TraverseAISDict[ KEY_ENTRY ] ) {
					m_Viewer.GetAISContext().Display( lineAIS, false );
					m_Viewer.GetAISContext().Deactivate( lineAIS );
				}
			}
			if( m_TraverseAISDict.ContainsKey( KEY_EXIT ) ) {
				foreach( AIS_Line lineAIS in m_TraverseAISDict[ KEY_EXIT ] ) {
					m_Viewer.GetAISContext().Display( lineAIS, false );
					m_Viewer.GetAISContext().Deactivate( lineAIS );
				}
			}
		}

		void RemoveTraverseLines( IReadOnlyList<string> pathIDList )
		{
			foreach( string pathID in pathIDList ) {
				if( m_TraverseAISDict.ContainsKey( pathID ) ) {
					foreach( AIS_Line lineAIS in m_TraverseAISDict[ pathID ] ) {
						m_Viewer.GetAISContext().Remove( lineAIS, false );
					}
					m_TraverseAISDict[ pathID ].Clear();
					m_TraverseAISDict.Remove( pathID );
				}
				if( m_FrogLeapAISDict.ContainsKey( pathID ) ) {
					foreach( AIS_Shape shapeAIS in m_FrogLeapAISDict[ pathID ] ) {
						m_Viewer.GetAISContext().Remove( shapeAIS, false );
					}
					m_FrogLeapAISDict[ pathID ].Clear();
					m_FrogLeapAISDict.Remove( pathID );
				}
			}

			if( m_TraverseAISDict.ContainsKey( KEY_ENTRY ) ) {
				foreach( AIS_Line lineAIS in m_TraverseAISDict[ KEY_ENTRY ] ) {
					m_Viewer.GetAISContext().Remove( lineAIS, false );
				}
				m_TraverseAISDict[ KEY_ENTRY ].Clear();
				m_TraverseAISDict.Remove( KEY_ENTRY );
			}
			if( m_TraverseAISDict.ContainsKey( KEY_EXIT ) ) {
				foreach( AIS_Line lineAIS in m_TraverseAISDict[ KEY_EXIT ] ) {
					m_Viewer.GetAISContext().Remove( lineAIS, false );
				}
				m_TraverseAISDict[ KEY_EXIT ].Clear();
				m_TraverseAISDict.Remove( KEY_EXIT );
			}
		}

		void AddOneLinearTraverseWithTrans( List<AIS_Line> lineList, gp_Pnt startPnt, gp_Pnt endPnt, gp_Trsf trsf, Quantity_NameOfColor color )
		{
			AIS_Line traverseAIS = DrawHelper.GetLineAIS( startPnt, endPnt, color, 1, true );
			if( trsf != null ) {
				traverseAIS.SetLocalTransformation( trsf );
			}
			lineList.Add( traverseAIS );
		}

		IProcessPoint GetProcessStartPoint( string pathID )
		{
			return CacheHelper.GetProcessStartPoint( pathID );
		}

		IProcessPoint GetProcessEndPoint( string pathID )
		{
			return CacheHelper.GetProcessEndPoint( pathID );
		}
		static List<(gp_Pnt start, gp_Pnt end)> GetTraverseLineSegments( TraversePathResult traverseResult, TraverseData traverseData )
		{
			List<(gp_Pnt, gp_Pnt)> segments = new List<(gp_Pnt, gp_Pnt)>();

			// lift up
			if( traverseData.LiftUpDistance > 0 &&
				traverseResult.PreviousPathEnd != null &&
				traverseResult.LiftUpPoint != null ) {
				segments.Add( (traverseResult.PreviousPathEnd.Point, traverseResult.LiftUpPoint.Point) );
			}

			// traverse (depends on mode)
			if( traverseData.TraverseMode == TraverseData.TraverseType.SafePlane ) {
				segments.Add( (traverseResult.LiftUpPoint.Point, traverseResult.SafePlaneLiftUpProjPoint) );
				segments.Add( (traverseResult.SafePlaneLiftUpProjPoint, traverseResult.SafePlaneCutDownProjPoint) );
				segments.Add( (traverseResult.SafePlaneCutDownProjPoint, traverseResult.CutDownPoint.Point) );
			}
			else if( traverseData.TraverseMode == TraverseData.TraverseType.SafeCylinder ||
				traverseData.TraverseMode == TraverseData.TraverseType.SafeSphere ) {
				List<gp_Pnt> points = traverseResult.SafeBoundaryPointList;
				if( points == null || points.Count == 0 ) {
					segments.Add( (traverseResult.LiftUpPoint.Point, traverseResult.CutDownPoint.Point) );
				}
				else {
					segments.Add( (traverseResult.LiftUpPoint.Point, points.First()) );
					for( int i = 1; i < points.Count; ++i ) {
						segments.Add( (points[ i - 1 ], points[ i ]) );
					}
					segments.Add( (points.Last(), traverseResult.CutDownPoint.Point) );
				}
			}
			else {
				if( traverseData.FrogLeapDistance == 0 || traverseResult.FrogLeapMiddlePoint == null ) {
					segments.Add( (traverseResult.LiftUpPoint.Point, traverseResult.CutDownPoint.Point) );
				}
			}

			// cut down
			if( traverseData.CutDownDistance > 0 &&
				traverseResult.CutDownPoint != null &&
				traverseResult.CurrentPathStart != null ) {
				segments.Add( (traverseResult.CutDownPoint.Point, traverseResult.CurrentPathStart.Point) );
			}

			return segments;
		}

		readonly Dictionary<string, List<AIS_Line>> m_TraverseAISDict = new Dictionary<string, List<AIS_Line>>();
		readonly Dictionary<string, List<AIS_Shape>> m_FrogLeapAISDict = new Dictionary<string, List<AIS_Shape>>();
		const string KEY_ENTRY = "KEY_ENTRY";
		const string KEY_EXIT = "KEY_EXIT";

		const Quantity_NameOfColor TRAVERSE_COLOR = Quantity_NameOfColor.Quantity_NOC_RED;
		const Quantity_NameOfColor FLYCUT_COLOR = Quantity_NameOfColor.Quantity_NOC_BLUE;
	}
}
