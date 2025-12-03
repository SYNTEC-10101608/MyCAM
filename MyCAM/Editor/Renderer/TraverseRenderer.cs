using MyCAM.CacheInfo;
using MyCAM.Data;
using MyCAM.Helper;
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
	/// <summary>
	/// Renderer for traverse paths between cutting paths
	/// </summary>
	internal class TraverseRenderer : CAMRendererBase
	{
		readonly List<AIS_Line> m_TraverseAISList = new List<AIS_Line>();
		readonly List<AIS_Shape> m_FrogLeapAISList = new List<AIS_Shape>();

		public TraverseRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void Show( bool bUpdate = false )
		{
			Remove();

			// no need to show
			if( !m_IsShow ) {
				return;
			}

			// Traverse between paths
			for( int i = 1; i < m_DataManager.PathIDList.Count; i++ ) {
				ContourCacheInfo previousCacheInfo = ( m_DataManager.ObjectMap[ m_DataManager.PathIDList[ i - 1 ] ] as ContourPathObject ).ContourCacheInfo;
				ContourCacheInfo currentCacheInfo = ( m_DataManager.ObjectMap[ m_DataManager.PathIDList[ i ] ] as ContourPathObject ).ContourCacheInfo;
				CraftData currentCraftData = ( m_DataManager.ObjectMap[ m_DataManager.PathIDList[ i ] ] as ContourPathObject ).CraftData;

				// p1: end of previous path
				// p2: lift up point of previous path
				// p3: frog leap middle point (if frog leap)
				// p4: cut down point of current path
				// p5: start of current path
				IProcessPoint p1 = previousCacheInfo.GetProcessEndPoint();
				IProcessPoint p2 = TraverseHelper.GetCutDownOrLiftUpPoint( previousCacheInfo.GetProcessEndPoint(), currentCraftData.TraverseData.LiftUpDistance );
				IProcessPoint p4 = TraverseHelper.GetCutDownOrLiftUpPoint( currentCacheInfo.GetProcessStartPoint(), currentCraftData.TraverseData.CutDownDistance );
				IProcessPoint p5 = currentCacheInfo.GetProcessStartPoint();

				// lift up
				if( currentCraftData.TraverseData.LiftUpDistance > 0 && p1 != null && p2 != null ) {
					AddOneLinearTraverse( p1.Point, p2.Point );
				}

				// frog leap
				if( currentCraftData.TraverseData.FrogLeapDistance > 0 && p2 != null && p4 != null ) {
					IProcessPoint p3 = TraverseHelper.GetFrogLeapMiddlePoint( p2, p4, currentCraftData.TraverseData.FrogLeapDistance );
					if( p3 != null ) {
						GC_MakeArcOfCircle makeCircle = new GC_MakeArcOfCircle( p2.Point, p3.Point, p4.Point );
						if( makeCircle.IsDone() ) {
							Geom_TrimmedCurve arcCurve = makeCircle.Value();
							BRepBuilderAPI_MakeEdge makeEdge = new BRepBuilderAPI_MakeEdge( arcCurve );
							AIS_Shape arcAIS = new AIS_Shape( makeEdge.Shape() );
							arcAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
							arcAIS.SetWidth( 1 );
							arcAIS.SetTransparency( 1 );
							Prs3d_LineAspect prs3D_LineAspect = new Prs3d_LineAspect( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ), Aspect_TypeOfLine.Aspect_TOL_DASH, 1 );
							arcAIS.Attributes().SetWireAspect( prs3D_LineAspect );
							m_FrogLeapAISList.Add( arcAIS );
						}
						else {
							// fallback to normal traverse line
							AddOneLinearTraverse( p2.Point, p4.Point );
						}
					}
					else {
						// fallback to normal traverse line
						AddOneLinearTraverse( p2.Point, p4.Point );
					}
				}

				// normal traverse
				else if( p2 != null && p4 != null ) {
					AddOneLinearTraverse( p2.Point, p4.Point );
				}

				// cut down
				if( currentCraftData.TraverseData.CutDownDistance > 0 && p4 != null && p5 != null ) {
					AddOneLinearTraverse( p4.Point, p5.Point );
				}
			}

			// entry
			if( m_DataManager.EntryAndExitData.EntryDistance > 0 && m_DataManager.PathIDList.Count != 0 ) {
				if( GetCacheInfoByID( m_DataManager.PathIDList.First(), out ICacheInfo cacheInfo ) ) {
					IProcessPoint firstPathStartPoint = cacheInfo.GetProcessStartPoint();
					IProcessPoint entryPoint = TraverseHelper.GetCutDownOrLiftUpPoint( firstPathStartPoint.Clone(), m_DataManager.EntryAndExitData.EntryDistance );
					if( firstPathStartPoint != null && entryPoint != null ) {
						AddOneLinearTraverse( entryPoint.Point, firstPathStartPoint.Point );
					}
				}
			}

			// exit
			if( m_DataManager.EntryAndExitData.ExitDistance > 0 && m_DataManager.PathIDList.Count != 0 ) {
				if( GetCacheInfoByID( m_DataManager.PathIDList.Last(), out ICacheInfo cacheInfo ) ) {
					IProcessPoint lastPathEndPoint = cacheInfo.GetProcessEndPoint();
					IProcessPoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( lastPathEndPoint.Clone(), m_DataManager.EntryAndExitData.ExitDistance );
					if( lastPathEndPoint != null && exitPoint != null ) {
						AddOneLinearTraverse( lastPathEndPoint.Point, exitPoint.Point );
					}
				}
			}

			// Display all lines
			foreach( AIS_Line rapidTraverseAIS in m_TraverseAISList ) {
				m_Viewer.GetAISContext().Display( rapidTraverseAIS, false );
				m_Viewer.GetAISContext().Deactivate( rapidTraverseAIS );
			}
			foreach( AIS_Shape frogLeapAIS in m_FrogLeapAISList ) {
				m_Viewer.GetAISContext().Display( frogLeapAIS, false );
				m_Viewer.GetAISContext().Deactivate( frogLeapAIS );
			}

			if( bUpdate ) {
				UpdateView();
			}
		}

		public override void Remove( bool bUpdate = false )
		{
			// Remove previous lines
			foreach( AIS_Line traverseAIS in m_TraverseAISList ) {
				m_Viewer.GetAISContext().Remove( traverseAIS, false );
			}
			foreach( AIS_Shape frogLeapAIS in m_FrogLeapAISList ) {
				m_Viewer.GetAISContext().Remove( frogLeapAIS, false );
			}
			m_TraverseAISList.Clear();
			m_FrogLeapAISList.Clear();

			if( bUpdate ) {
				UpdateView();
			}
		}

		void AddOneLinearTraverse( gp_Pnt startPnt, gp_Pnt endPnt )
		{
			AIS_Line traverseAIS = GetLineAIS( startPnt, endPnt, Quantity_NameOfColor.Quantity_NOC_RED, true );
			m_TraverseAISList.Add( traverseAIS );
		}

		AIS_Line GetLineAIS( gp_Pnt startPnt, gp_Pnt endPnt, Quantity_NameOfColor color, bool isDashLine )
		{
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( startPnt ), new Geom_CartesianPoint( endPnt ) );
			lineAIS.SetColor( new Quantity_Color( color ) );
			lineAIS.SetWidth( 1 );
			lineAIS.SetTransparency( 1 );
			if( isDashLine ) {
				Prs3d_LineAspect prs3D_LineAspect = new Prs3d_LineAspect( new Quantity_Color( color ), Aspect_TypeOfLine.Aspect_TOL_DASH, 1 );
				lineAIS.Attributes().SetLineAspect( prs3D_LineAspect );
			}
			return lineAIS;
		}
	}
}
