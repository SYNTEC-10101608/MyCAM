using MyCAM.Data;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCC.TopoDS;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Editor.Renderer
{
	internal static class ToolVecAndPathVisibleHelper
	{
		// get all main point
		public static IReadOnlyList<gp_Pnt> GetMainPathPointList( string szPathID )
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

		// draw polyline wire for main path
		public static TopoDS_Wire CreatePolylineWire( IReadOnlyList<gp_Pnt> pointList )
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

		#region Tool Vec

		public static Dictionary<string, List<AIS_Line>> BuildToolVecAISDict( List<string> pathIDList, gp_Trsf trsf )
		{
			Dictionary<string, List<AIS_Line>> result = new Dictionary<string, List<AIS_Line>>();
			foreach( string szPathID in pathIDList ) {
				IReadOnlyList<IProcessPoint> toolVecPointList = GetToolVecPointList( szPathID );
				DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData );
				if( toolVecPointList == null || toolVecPointList.Count == 0 ) {
					continue;
				}
				List<AIS_Line> toolVecAISList = new List<AIS_Line>();
				result.Add( szPathID, toolVecAISList );
				for( int i = 0; i < toolVecPointList.Count; i++ ) {
					IProcessPoint point = toolVecPointList[ i ];
					AIS_Line toolVecAIS = CreateVecAIS( point.Point, point.ToolVec );
					// first and last points are always highlighted red
					bool isModifiedStartEndPnt = i == 0 && craftData.StartPntToolVecData.StartPnt.AngleData != null || i == toolVecPointList.Count - 1 && craftData.StartPntToolVecData.EndPnt.AngleData != null;
					if( isModifiedStartEndPnt || point.IsToolVecModPoint ) {
						toolVecAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
						toolVecAIS.SetWidth( 4 );
					}
					if( trsf != null ) {
						toolVecAIS.SetLocalTransformation( trsf );
					}
					toolVecAISList.Add( toolVecAIS );
				}
			}
			return result;
		}

		static AIS_Line CreateVecAIS( gp_Pnt point, gp_Dir dir )
		{
			gp_Pnt endPoint = new gp_Pnt( point.XYZ() + dir.XYZ() * 10 );
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( point ), new Geom_CartesianPoint( endPoint ) );
			lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			lineAIS.SetWidth( 1 );
			return lineAIS;
		}

		static IReadOnlyList<IProcessPoint> GetToolVecPointList( string pathID )
		{
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return null;
			}
			if( pathType == PathType.Contour ) {
				if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache contourCache ) ) {
					return null;
				}
				return contourCache.MainPathPointList;
			}
			else if( DataGettingHelper.IsStdPattern( pathType ) ) {
				if( !DataGettingHelper.GetStdPatternCacheByID( pathID, out IStdPatternCache stdPatternCache ) ) {
					return null;
				}
				return stdPatternCache.KeyCAMPointList;
			}
			else {
				return null;
			}
		}

		#endregion
	}
}
