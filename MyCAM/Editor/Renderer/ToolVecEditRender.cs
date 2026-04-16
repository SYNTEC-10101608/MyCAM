using MyCAM.Data;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor.Renderer
{
	internal class ToolVecEditRender : CAMRendererBase
	{
		// tool vec
		readonly Dictionary<string, List<AIS_Line>> m_ToolVecAISDict = new Dictionary<string, List<AIS_Line>>();

		// path
		readonly Dictionary<string, List<AIS_Shape>> m_MainPathAISDict = new Dictionary<string, List<AIS_Shape>>();
		readonly Dictionary<string, AIS_Shape> m_OriginalPathAISDict = new Dictionary<string, AIS_Shape>();
		List<string> m_PathIDList = new List<string>();

		public ToolVecEditRender( Viewer viewer, DataManager dataManager, List<string> pathIDList )
			: base( viewer, dataManager )
		{
			m_PathIDList = pathIDList;
		}

		public override void Show( bool bUpdate = false )
		{
			Show( m_PathIDList, bUpdate );
		}

		public void Show( List<string> pathIDList, bool bUpdate = false )
		{
			BuildAndDisplay( pathIDList, null, bUpdate );
		}

		public override void Remove( bool bUpdate = false )
		{
			Remove( m_PathIDList, bUpdate );
		}

		public void Remove( List<string> pathIDList, bool bUpdate = false )
		{
			RemoveToolVec( pathIDList );
			RemovePaths( pathIDList );
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void ShowTrans( gp_Trsf trsf, bool bUpdate = false )
		{
			BuildAndDisplay( m_PathIDList, trsf, bUpdate );
		}

		public void Trans( gp_Trsf trsf, bool bUpdate = false )
		{
			// if no AIS objects exist, fall back to full rebuild
			if( m_ToolVecAISDict.Count == 0 && m_MainPathAISDict.Count == 0 ) {
				ShowTrans( trsf, bUpdate );
				return;
			}

			// update transform on existing tool vec AIS objects
			foreach( var kvp in m_ToolVecAISDict ) {
				foreach( AIS_Line toolVecAIS in kvp.Value ) {
					if( trsf != null ) {
						toolVecAIS.SetLocalTransformation( trsf );
					}
				}
			}

			// update transform on existing path AIS objects
			foreach( var kvp in m_MainPathAISDict ) {
				foreach( AIS_Shape pathAIS in kvp.Value ) {
					if( trsf != null ) {
						pathAIS.SetLocalTransformation( trsf );
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
		}

		void BuildAndDisplay( List<string> pathIDList, gp_Trsf trsf, bool bUpdate )
		{
			Remove( pathIDList );

			// no need to show
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			// build and display tool vec
			BuildAndDisplayToolVec( pathIDList, trsf );

			// build and display path
			BuildAndDisplayPath( pathIDList, trsf );

			if( bUpdate ) {
				UpdateView();
			}
		}

		#region Tool Vec

		void RemoveToolVec( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( m_ToolVecAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> toolVecAISList = m_ToolVecAISDict[ szPathID ];
					foreach( AIS_Line toolVecAIS in toolVecAISList ) {
						m_Viewer.GetAISContext().Remove( toolVecAIS, false );
					}
					m_ToolVecAISDict[ szPathID ].Clear();
					m_ToolVecAISDict.Remove( szPathID );
				}
			}
		}

		void BuildAndDisplayToolVec( List<string> pathIDList, gp_Trsf trsf )
		{
			// build tool vec
			foreach( string szPathID in pathIDList ) {
				IReadOnlyList<IProcessPoint> toolVecPointList = GetToolVecPointList( szPathID );
				if( toolVecPointList == null || toolVecPointList.Count == 0 ) {
					continue;
				}
				List<AIS_Line> toolVecAISList = new List<AIS_Line>();
				m_ToolVecAISDict.Add( szPathID, toolVecAISList );
				for( int i = 0; i < toolVecPointList.Count; i++ ) {
					IProcessPoint point = toolVecPointList[ i ];
					AIS_Line toolVecAIS = GetVecAIS( point.Point, point.ToolVec );
					if( i == 0 || i == toolVecPointList.Count - 1 ) {
						toolVecAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
						toolVecAIS.SetWidth( 4 );
						if( trsf != null ) {
							toolVecAIS.SetLocalTransformation( trsf );
						}
						toolVecAISList.Add( toolVecAIS );
						continue;
					}
					if( point.IsToolVecModPoint ) {
						toolVecAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
						toolVecAIS.SetWidth( 4 );
					}
					if( trsf != null ) {
						toolVecAIS.SetLocalTransformation( trsf );
					}
					toolVecAISList.Add( toolVecAIS );
				}
			}

			// display the tool vec
			foreach( string szPathID in pathIDList ) {
				if( m_ToolVecAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> toolVecAISList = m_ToolVecAISDict[ szPathID ];
					foreach( AIS_Line toolVecAIS in toolVecAISList ) {
						m_Viewer.GetAISContext().Display( toolVecAIS, false );
						m_Viewer.GetAISContext().Deactivate( toolVecAIS );
					}
				}
			}
		}

		static AIS_Line GetVecAIS( gp_Pnt point, gp_Dir dir )
		{
			gp_Pnt endPoint = new gp_Pnt( point.XYZ() + dir.XYZ() * 10 );
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( point ), new Geom_CartesianPoint( endPoint ) );
			lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			lineAIS.SetWidth( 1 );
			return lineAIS;
		}

		static IReadOnlyList<IProcessPoint> GetToolVecPointList( string pathID )
		{
			// get path type
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return null;
			}

			// for contour
			if( pathType == PathType.Contour ) {
				if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache contourCache ) ) {
					return null;
				}
				return contourCache.MainPathPointList;
			}

			// for standard pattern
			else if( DataGettingHelper.IsStdPattern( pathType ) ) {
				if( !DataGettingHelper.GetStdPatternCacheByID( pathID, out IStdPatternCache stdPatternCache ) ) {
					return null;
				}
				return stdPatternCache.KeyCAMPointList;
			}

			// other type path do not support tool vec
			else {
				return null;
			}
		}

		#endregion

		#region Path

		void BuildAndDisplayPath( List<string> pathIDList, gp_Trsf trsf )
		{
			// render each path
			foreach( string pathID in pathIDList ) {
				IReadOnlyList<gp_Pnt> pointList = RendererHelper.GetMainPathPointList( pathID );
				if( pointList == null || pointList.Count < 2 ) {
					continue;
				}

				// get interpolation interval list to split path into segments by interpolation type
				List<Tuple<int, int, EToolVecInterpolateType>> intervalList = GetInterpolateTypeRegion( pathID );
				List<AIS_Shape> segmentAISList = new List<AIS_Shape>();

				if( intervalList == null || intervalList.Count == 0 ) {
					// no interval data, render as single wire with default color
					TopoDS_Wire pathWire = RendererHelper.CreatePolylineWire( pointList );
					if( pathWire == null || pathWire.IsNull() ) {
						continue;
					}
					AIS_Shape pathAIS = new AIS_Shape( pathWire );
					pathAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
					pathAIS.SetWidth( 3.0 );
					if( trsf != null ) {
						pathAIS.SetLocalTransformation( trsf );
					}
					segmentAISList.Add( pathAIS );
				}
				else {
					// render each segment with a color based on its interpolation type
					foreach( var interval in intervalList ) {
						int startIdx = interval.Item1;
						int endIdx = interval.Item2;
						EToolVecInterpolateType interpType = interval.Item3;

						// clamp to valid range
						if( startIdx < 0 ) { startIdx = 0; }
						if( endIdx >= pointList.Count ) { endIdx = pointList.Count - 1; }
						if( endIdx - startIdx < 1 ) {
							continue;
						}

						// extract sub-point list for this segment (inclusive on both ends)
						List<gp_Pnt> segmentPoints = new List<gp_Pnt>();
						for( int i = startIdx; i <= endIdx; i++ ) {
							segmentPoints.Add( pointList[ i ] );
						}

						TopoDS_Wire segmentWire = RendererHelper.CreatePolylineWire( segmentPoints );
						if( segmentWire == null || segmentWire.IsNull() ) {
							continue;
						}

						AIS_Shape segmentAIS = new AIS_Shape( segmentWire );
						segmentAIS.SetColor( new Quantity_Color( GetInterpolateColor( interpType ) ) );
						segmentAIS.SetWidth( 3.0 );
						if( trsf != null ) {
							segmentAIS.SetLocalTransformation( trsf );
						}
						segmentAISList.Add( segmentAIS );
					}
				}

				// store and display all segments for this path
				m_MainPathAISDict.Add( pathID, segmentAISList );
				foreach( AIS_Shape segAIS in segmentAISList ) {
					m_Viewer.GetAISContext().Display( segAIS, false );
					m_Viewer.GetAISContext().Deactivate( segAIS );
				}
			}
		}

		static List<Tuple<int, int, EToolVecInterpolateType>> GetInterpolateTypeRegion( string szPathID )
		{
			List<Tuple<int, int, EToolVecInterpolateType>> modifyMap = new List<Tuple<int, int, EToolVecInterpolateType>>();
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType ) ) {
				return modifyMap;
			}
			if( pathType == PathType.Contour ) {
				if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
					return modifyMap;
				}
				ContourCache contourCache = pathCache as ContourCache;
				modifyMap = contourCache.GetMapedModifyMap();
			}
			return modifyMap;
		}

		static Quantity_NameOfColor GetInterpolateColor( EToolVecInterpolateType interpType )
		{
			switch( interpType ) {
				case EToolVecInterpolateType.VectorInterpolation:
					return Quantity_NameOfColor.Quantity_NOC_ORANGE;
				case EToolVecInterpolateType.TiltAngleInterpolation:
					return Quantity_NameOfColor.Quantity_NOC_CYAN1;
				case EToolVecInterpolateType.Normal:
				default:
					return Quantity_NameOfColor.Quantity_NOC_BLUE;
			}
		}

		void RemovePaths( List<string> pathIDList )
		{
			foreach( string pathID in pathIDList ) {
				if( m_MainPathAISDict.TryGetValue( pathID, out List<AIS_Shape> pathAISList ) ) {
					foreach( AIS_Shape pathAIS in pathAISList ) {
						m_Viewer.GetAISContext().Remove( pathAIS, false );
					}
					pathAISList.Clear();
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

		#endregion
	}
}
