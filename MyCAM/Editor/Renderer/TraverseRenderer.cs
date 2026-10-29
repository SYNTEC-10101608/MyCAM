using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
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
using System.Collections.Generic;
using System.Linq;

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
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( List<string> pathIDList, bool bUpdate = false )
		{
			// paused, do not rebuild or display
			if( m_IsPauseRefreshAndHide ) {
				return;
			}
			List<string> pathsToUpdate = GetPathsToUpdate( pathIDList );
			Remove( pathsToUpdate );
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}
			ShowTraversePath( pathsToUpdate );
			ShowEntryAndExit();
			DisplayTraverseLines( pathsToUpdate );
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
			RemoveTraverseLines( pathIDList );
			if( bUpdate ) {
				UpdateView();
			}
		}


		public void ShowTrans( gp_Trsf trsf, bool bUpdate = false )
		{
			ShowTrans( m_DataManager.PathIDList, trsf, bUpdate );
		}

		public void ShowTrans( List<string> pathIDList, gp_Trsf trsf, bool bUpdate = false )
		{
			// paused, do not rebuild or display
			if( m_IsPauseRefreshAndHide ) {
				return;
			}
			List<string> pathsToUpdate = GetPathsToUpdate( pathIDList );
			Remove( pathsToUpdate );
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}
			ShowTraversePathWithTrans( pathsToUpdate, trsf );
			ShowEntryAndExitWithTrans( trsf );
			DisplayTraverseLines( pathsToUpdate );
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

		void ShowTraversePath( List<string> pathIDList )
		{
			ShowTraversePathWithTrans( pathIDList, null );
		}

		void ShowTraversePathWithTrans( List<string> pathIDList, gp_Trsf trsf )
		{
			Dictionary<string, string> previousPathInGroupMap = BuildPreviousPathInGroupMap();
			foreach( string currentPathID in pathIDList ) {
				if( !previousPathInGroupMap.TryGetValue( currentPathID, out string previousPathID ) || previousPathID == null ) {
					continue;
				}

				TraverseData currentTraverseData = GetTraverseData( currentPathID );

				// Get process points
				IProcessPoint previousEndPoint = GetProcessEndPoint( previousPathID );
				IProcessPoint currentStartPoint = GetProcessStartPoint( currentPathID );

				// Calculate all traverse points using helper
				if( !TraverseHelper.TryCalculateTraversePoints( previousEndPoint, currentStartPoint, currentTraverseData, out TraversePathResult result ) ) {
					continue;
				}

				List<AIS_Line> traverseLineList = new List<AIS_Line>();
				List<AIS_Shape> frogLeapShapeList = new List<AIS_Shape>();

				// frog leap 
				if( !currentTraverseData.IsSafePlaneEnable && currentTraverseData.FrogLeapDistance > 0 && result.FrogLeapMiddlePoint != null && result.LiftUpPoint.Point != null && result.CutDownPoint.Point != null ) {
					GC_MakeArcOfCircle makeCircle = new GC_MakeArcOfCircle( result.LiftUpPoint.Point, result.FrogLeapMiddlePoint.Point, result.CutDownPoint.Point );
					if( makeCircle.IsDone() ) {
						Geom_TrimmedCurve arcCurve = makeCircle.Value();
						BRepBuilderAPI_MakeEdge makeEdge = new BRepBuilderAPI_MakeEdge( arcCurve );
						AIS_Shape arcAIS = new AIS_Shape( makeEdge.Shape() );
						arcAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
						arcAIS.SetWidth( 1 );
						arcAIS.SetTransparency( 1 );
						Prs3d_LineAspect prs3D_LineAspect = new Prs3d_LineAspect( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ), Aspect_TypeOfLine.Aspect_TOL_DASH, 1 );
						arcAIS.Attributes().SetWireAspect( prs3D_LineAspect );
						if( trsf != null ) {
							arcAIS.SetLocalTransformation( trsf );
						}
						frogLeapShapeList.Add( arcAIS );

						// lift up
						if( currentTraverseData.LiftUpDistance > 0 && result.PreviousPathEnd.Point != null ) {
							AddOneLinearTraverseWithTrans( traverseLineList, result.PreviousPathEnd.Point, result.LiftUpPoint.Point, trsf );
						}

						// cut down
						if( currentTraverseData.CutDownDistance > 0 && result.CurrentPathStart.Point != null ) {
							AddOneLinearTraverseWithTrans( traverseLineList, result.CutDownPoint.Point, result.CurrentPathStart.Point, trsf );
						}
					}
					else {
						List<(gp_Pnt start, gp_Pnt end)> segments = GetTraverseLineSegments( result, currentTraverseData );
						foreach( var seg in segments ) {
							AddOneLinearTraverseWithTrans( traverseLineList, seg.start, seg.end, trsf );
						}
					}
				}
				// safe plane or normal traverse
				else {
					List<(gp_Pnt start, gp_Pnt end)> segments = GetTraverseLineSegments( result, currentTraverseData );
					foreach( var seg in segments ) {
						AddOneLinearTraverseWithTrans( traverseLineList, seg.start, seg.end, trsf );
					}
				}

				if( traverseLineList.Count > 0 ) {
					m_TraverseAISDict[ currentPathID ] = traverseLineList;
				}
				if( frogLeapShapeList.Count > 0 ) {
					m_FrogLeapAISDict[ currentPathID ] = frogLeapShapeList;
				}
			}
		}

		void ShowEntryAndExit()
		{
			ShowEntryAndExitWithTrans( null );
		}

		void ShowEntryAndExitWithTrans( gp_Trsf trsf )
		{
			if( m_DataManager.PathIDList.Count == 0 ) {
				RemoveGroupEntryAndExitLines();
				return;
			}

			RemoveGroupEntryAndExitLines();
			List<List<string>> groups = ExternalAxisGroupHelper.BuildContiguousGroups( m_DataManager.PathIDList, ExternalAxisHelper.DEFAULT_GROUP_TOLERANCE_DEG );
			foreach( List<string> group in groups ) {
				if( group == null || group.Count == 0 ) {
					continue;
				}

				string firstPathID = group.First();
				string lastPathID = group.Last();

				// entry
				if( m_DataManager.EntryAndExitData.EntryDistance > 0 ) {
					IProcessPoint firstPathStartPoint = GetProcessStartPoint( firstPathID );
					if( firstPathStartPoint != null ) {
						IProcessPoint entryPoint = TraverseHelper.GetCutDownOrLiftUpPoint( firstPathStartPoint.Clone(), m_DataManager.EntryAndExitData.EntryDistance );
						if( entryPoint != null ) {
							List<AIS_Line> entryLineList = new List<AIS_Line>();
							AddOneLinearTraverseWithTrans( entryLineList, entryPoint.Point, firstPathStartPoint.Point, trsf );
							m_TraverseAISDict[ KEY_ENTRY_PREFIX + firstPathID ] = entryLineList;
						}
					}
				}

				// exit
				if( m_DataManager.EntryAndExitData.ExitDistance > 0 ) {
					IProcessPoint lastPathEndPoint = GetProcessEndPoint( lastPathID );
					if( lastPathEndPoint != null ) {
						IProcessPoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( lastPathEndPoint.Clone(), m_DataManager.EntryAndExitData.ExitDistance );
						if( exitPoint != null ) {
							List<AIS_Line> exitLineList = new List<AIS_Line>();
							AddOneLinearTraverseWithTrans( exitLineList, lastPathEndPoint.Point, exitPoint.Point, trsf );
							m_TraverseAISDict[ KEY_EXIT_PREFIX + lastPathID ] = exitLineList;
						}
					}
				}
			}
		}

		Dictionary<string, string> BuildPreviousPathInGroupMap()
		{
			Dictionary<string, string> map = new Dictionary<string, string>();
			List<List<string>> groups = ExternalAxisGroupHelper.BuildContiguousGroups( m_DataManager.PathIDList, ExternalAxisHelper.DEFAULT_GROUP_TOLERANCE_DEG );
			foreach( List<string> group in groups ) {
				for( int i = 1; i < group.Count; i++ ) {
					map[ group[ i ] ] = group[ i - 1 ];
				}
			}
			return map;
		}

		List<string> GetPathsToUpdate( List<string> pathIDList )
		{
			HashSet<string> pathsToUpdate = new HashSet<string>();

			foreach( string pathID in pathIDList ) {
				pathsToUpdate.Add( pathID );

				// add next path
				string nextPathID = GetNextPathIDInAllPaths( pathID );
				if( nextPathID != null ) {
					pathsToUpdate.Add( nextPathID );
				}
			}

			// keep the order as in m_DataManager.PathIDList
			List<string> result = new List<string>();
			foreach( string pathID in m_DataManager.PathIDList ) {
				if( pathsToUpdate.Contains( pathID ) ) {
					result.Add( pathID );
				}
			}

			return result;
		}

		string GetNextPathIDInAllPaths( string currentPathID )
		{
			int indexInAllPaths = m_DataManager.PathIDList.IndexOf( currentPathID );
			if( indexInAllPaths >= 0 && indexInAllPaths < m_DataManager.PathIDList.Count - 1 ) {
				return m_DataManager.PathIDList[ indexInAllPaths + 1 ];
			}
			return null;
		}

		void DisplayTraverseLines( List<string> pathIDList )
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

			foreach( var kvp in m_TraverseAISDict.Where( p => p.Key.StartsWith( KEY_ENTRY_PREFIX ) || p.Key.StartsWith( KEY_EXIT_PREFIX ) ) ) {
				foreach( AIS_Line lineAIS in kvp.Value ) {
					m_Viewer.GetAISContext().Display( lineAIS, false );
					m_Viewer.GetAISContext().Deactivate( lineAIS );
				}
			}
		}

		void RemoveTraverseLines( List<string> pathIDList )
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

			RemoveGroupEntryAndExitLines();
		}

		void RemoveGroupEntryAndExitLines()
		{
			List<string> keysToRemove = m_TraverseAISDict.Keys
				.Where( k => k.StartsWith( KEY_ENTRY_PREFIX ) || k.StartsWith( KEY_EXIT_PREFIX ) )
				.ToList();

			foreach( string key in keysToRemove ) {
				foreach( AIS_Line lineAIS in m_TraverseAISDict[ key ] ) {
					m_Viewer.GetAISContext().Remove( lineAIS, false );
				}
				m_TraverseAISDict[ key ].Clear();
				m_TraverseAISDict.Remove( key );
			}
		}

		void AddOneLinearTraverse( List<AIS_Line> lineList, gp_Pnt startPnt, gp_Pnt endPnt )
		{
			AddOneLinearTraverseWithTrans( lineList, startPnt, endPnt, null );
		}

		void AddOneLinearTraverseWithTrans( List<AIS_Line> lineList, gp_Pnt startPnt, gp_Pnt endPnt, gp_Trsf trsf )
		{
			AIS_Line traverseAIS = DrawHelper.GetLineAIS( startPnt, endPnt, Quantity_NameOfColor.Quantity_NOC_RED, 1, true );
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
		TraverseData GetTraverseData( string pathID )
		{
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				return null;
			}
			return craftData.TraverseData;
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
			if( traverseData.IsSafePlaneEnable ) {
				segments.Add( (traverseResult.LiftUpPoint.Point, traverseResult.SafePlaneLiftUpProjPoint) );
				segments.Add( (traverseResult.SafePlaneLiftUpProjPoint, traverseResult.SafePlaneCutDownProjPoint) );
				segments.Add( (traverseResult.SafePlaneCutDownProjPoint, traverseResult.CutDownPoint.Point) );
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
		const string KEY_ENTRY_PREFIX = "KEY_ENTRY_";
		const string KEY_EXIT_PREFIX = "KEY_EXIT_";
	}
}
