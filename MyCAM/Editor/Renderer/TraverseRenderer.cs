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
using System.Collections.Generic;
using System.Linq;
using static MyCAM.Helper.TraverseHelper;

namespace MyCAM.Editor.Renderer
{
	internal class TraverseRenderer : CAMRendererBase
	{
		readonly Dictionary<string, List<AIS_Line>> m_TraverseAISDict = new Dictionary<string, List<AIS_Line>>();
		readonly Dictionary<string, List<AIS_Shape>> m_FrogLeapAISDict = new Dictionary<string, List<AIS_Shape>>();

		public TraverseRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void Show( bool bUpdate = false )
		{
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( List<string> pathIDList, bool bUpdate = false )
		{
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

		void ShowTraversePath( List<string> pathIDList )
		{
			foreach( string currentPathID in pathIDList ) {
				string previousPathID = GetPreviousPathIDInAllPaths( currentPathID );
				if( previousPathID == null ) {
					continue;
				}

				TraverseData currentTraverseData = GetTraverseData( currentPathID );

				// Get process points
				IProcessPoint previousEndPoint = GetProcessEndPoint( previousPathID );
				IProcessPoint currentStartPoint = GetProcessStartPoint( currentPathID );

				// Calculate all traverse points using helper
				if( !TraverseHelper.TryCalculateTraversePoints( previousEndPoint, currentStartPoint, currentTraverseData, out TraverseHelper.TraversePathResult result ) ) {
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
						frogLeapShapeList.Add( arcAIS );

						// lift up
						if( currentTraverseData.LiftUpDistance > 0 && result.PreviousPathEnd.Point != null ) {
							AddOneLinearTraverse( traverseLineList, result.PreviousPathEnd.Point, result.LiftUpPoint.Point );
						}

						// cut down
						if( currentTraverseData.CutDownDistance > 0 && result.CurrentPathStart.Point != null ) {
							AddOneLinearTraverse( traverseLineList, result.CutDownPoint.Point, result.CurrentPathStart.Point );
						}
					}
					else {
						List<(gp_Pnt start, gp_Pnt end)> segments = GetTraverseLineSegments( result, currentTraverseData );
						foreach( var seg in segments ) {
							AddOneLinearTraverse( traverseLineList, seg.start, seg.end );
						}
					}
				}
				// safe plane or normal traverse
				else {
					List<(gp_Pnt start, gp_Pnt end)> segments = GetTraverseLineSegments( result, currentTraverseData );
					foreach( var seg in segments ) {
						AddOneLinearTraverse( traverseLineList, seg.start, seg.end );
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
			if( m_DataManager.PathIDList.Count == 0 ) {
				return;
			}

			// entry
			if( m_DataManager.EntryAndExitData.EntryDistance > 0 ) {
				string firstPathID = m_DataManager.PathIDList.First();
				IProcessPoint firstPathStartPoint = GetProcessStartPoint( firstPathID );
				if( firstPathStartPoint != null ) {
					IProcessPoint entryPoint = GetCutDownOrLiftUpPoint( firstPathStartPoint.Clone(), m_DataManager.EntryAndExitData.EntryDistance );
					if( entryPoint != null ) {
						List<AIS_Line> entryLineList = new List<AIS_Line>();
						AddOneLinearTraverse( entryLineList, entryPoint.Point, firstPathStartPoint.Point );
						m_TraverseAISDict[ "EntryTraverse" ] = entryLineList;
					}
				}
			}

			// exit
			if( m_DataManager.EntryAndExitData.ExitDistance > 0 ) {
				string lastPathID = m_DataManager.PathIDList.Last();
				IProcessPoint lastPathEndPoint = GetProcessEndPoint( lastPathID );
				if( lastPathEndPoint != null ) {
					IProcessPoint exitPoint = GetCutDownOrLiftUpPoint( lastPathEndPoint.Clone(), m_DataManager.EntryAndExitData.ExitDistance );
					if( exitPoint != null ) {
						List<AIS_Line> exitLineList = new List<AIS_Line>();
						AddOneLinearTraverse( exitLineList, lastPathEndPoint.Point, exitPoint.Point );
						m_TraverseAISDict[ "ExitTraverse" ] = exitLineList;
					}
				}
			}
		}

		string GetPreviousPathIDInAllPaths( string currentPathID )
		{
			int indexInAllPaths = m_DataManager.PathIDList.IndexOf( currentPathID );
			if( indexInAllPaths > 0 ) {
				return m_DataManager.PathIDList[ indexInAllPaths - 1 ];
			}
			return null;
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

			if( m_TraverseAISDict.ContainsKey( "EntryTraverse" ) ) {
				foreach( AIS_Line lineAIS in m_TraverseAISDict[ "EntryTraverse" ] ) {
					m_Viewer.GetAISContext().Display( lineAIS, false );
					m_Viewer.GetAISContext().Deactivate( lineAIS );
				}
			}
			if( m_TraverseAISDict.ContainsKey( "ExitTraverse" ) ) {
				foreach( AIS_Line lineAIS in m_TraverseAISDict[ "ExitTraverse" ] ) {
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

			if( m_TraverseAISDict.ContainsKey( "EntryTraverse" ) ) {
				foreach( AIS_Line lineAIS in m_TraverseAISDict[ "EntryTraverse" ] ) {
					m_Viewer.GetAISContext().Remove( lineAIS, false );
				}
				m_TraverseAISDict[ "EntryTraverse" ].Clear();
				m_TraverseAISDict.Remove( "EntryTraverse" );
			}
			if( m_TraverseAISDict.ContainsKey( "ExitTraverse" ) ) {
				foreach( AIS_Line lineAIS in m_TraverseAISDict[ "ExitTraverse" ] ) {
					m_Viewer.GetAISContext().Remove( lineAIS, false );
				}
				m_TraverseAISDict[ "ExitTraverse" ].Clear();
				m_TraverseAISDict.Remove( "ExitTraverse" );
			}
		}

		void AddOneLinearTraverse( List<AIS_Line> lineList, gp_Pnt startPnt, gp_Pnt endPnt )
		{
			AIS_Line traverseAIS = DrawHelper.GetLineAIS( startPnt, endPnt, Quantity_NameOfColor.Quantity_NOC_RED, 1, true );
			lineList.Add( traverseAIS );
		}

		IProcessPoint GetProcessStartPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetTraverseDataCache( pathID, out ITraverseDataCache traverseDataCache ) ) {
				return null;
			}
			return traverseDataCache.GetProcessStartPoint();
		}

		IProcessPoint GetProcessEndPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetTraverseDataCache( pathID, out ITraverseDataCache traverseDataCache ) ) {
				return null;
			}
			return traverseDataCache.GetProcessEndPoint();
		}

		TraverseData GetTraverseData( string pathID )
		{
			if( !PathCacheProvider.TryGetTraverseDataCache( pathID, out ITraverseDataCache traverseDataCache ) ) {
				return new TraverseData();
			}
			return traverseDataCache.TraverseData;
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
	}
}
