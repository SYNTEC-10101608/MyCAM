using MyCAM.Data;
using OCC.TopoDS;
using OCCTool;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	internal static class CADSegmentBuilder
	{
		const int LOWEST_PointsToBuildSegment = 2;
		const int LOWEST_PointsToBuildArcSegment = 3;

		public static bool BuildCADSegment( List<PathEdge5D> pathEdge5DList, out List<ICADSegment> cadSegmentList )
		{
			cadSegmentList = new List<ICADSegment>();
			if( pathEdge5DList == null || pathEdge5DList.Count == 0 ) {
				return false;
			}

			// go through the contour edges
			for( int i = 0; i < pathEdge5DList.Count; i++ ) {
				TopoDS_Edge edge = pathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = pathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = pathEdge5DList[ i ].ComponentFace; // TODO: set solid face

				// this curve is line use equal length split
				if( GeometryTool.IsLine( edge, out _, out _ ) ) {
					DiscreteCADError result = CADDiscreteHelper.DiscretizeLine( edge, shellFace, out DiscretizedCADData cadSegBuildData );
					if( result != DiscreteCADError.Done ) {
						return false;
					}
					if( !BuildCADSegment( cadSegBuildData, ESegmentType.Line, out ICADSegment cadSegment ) ) {
						return false;
					}
					cadSegmentList.Add( cadSegment );
				}

				// this curve is arc choose the best option from the two options (chord error vs equal length)
				else if( GeometryTool.IsCircularArc( edge, out _, out _, out _, out _ ) ) {
					DiscreteCADError result = CADDiscreteHelper.DiscretizeArc( edge, shellFace, out List<DiscretizedCADData> cadSegBuildDataList, Math.PI / 2 );
					if( result != DiscreteCADError.Done || cadSegBuildDataList == null || cadSegBuildDataList.Count == 0 ) {
						return false;
					}
					for( int j = 0; j < cadSegBuildDataList.Count; j++ ) {
						if( !BuildCADSegment( cadSegBuildDataList[ j ], ESegmentType.Arc, out ICADSegment cadSegment ) ) {
							return false;
						}
						cadSegmentList.Add( cadSegment );
					}
				}

				// use chord error split
				else {

					// separate this bspline
					DiscreteCADError result = CADDiscreteHelper.DiscretizeBspline( edge, shellFace, out List<DiscretizedCADData> cadSegmentBuildDataList );
					if( result != DiscreteCADError.Done || cadSegmentBuildDataList == null || cadSegmentBuildDataList.Count == 0 ) {
						return false;
					}
					for( int j = 0; j < cadSegmentBuildDataList.Count; j++ ) {
						if( !BuildCADSegment( cadSegmentBuildDataList[ j ], ESegmentType.Line, out ICADSegment cadSegment ) ) {
							return false;
						}
						cadSegmentList.Add( cadSegment );
					}
				}
			}
			return true;
		}

		public static bool BuildCADSegment( DiscretizedCADData data, ESegmentType contourType, out ICADSegment cadSegment )
		{
			cadSegment = null;
			if( data.DiscCADPointList == null || data.DiscCADPointList.Count < LOWEST_PointsToBuildSegment ) {
				return false;
			}
			if( contourType == ESegmentType.Line ) {
				cadSegment = new LineCADSegment( data.DiscCADPointList, data.SegmentLength, data.SubSegmentLength, data.SubChordLength );
				return true;
			}
			if( contourType == ESegmentType.Arc ) {

				// arc is too short, build line instead
				if( data.DiscCADPointList.Count < LOWEST_PointsToBuildArcSegment ) {
					cadSegment = new LineCADSegment( data.DiscCADPointList, data.SegmentLength, data.SubSegmentLength, data.SubChordLength );
				}
				else {
					cadSegment = new ArcCADSegment( data.DiscCADPointList, data.SegmentLength, data.SubSegmentLength, data.SubChordLength );
				}
				return true;
			}
			return false;
		}
	}
}
