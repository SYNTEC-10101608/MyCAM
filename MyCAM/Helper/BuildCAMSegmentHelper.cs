using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Helper
{
	internal static class BuildCAMSegmentHelper
	{
		public static bool BuildCAMSegment( CAMData camdata, out List<ICAMSegmentElement> camSegmentList )
		{
			camSegmentList = new List<ICAMSegmentElement>();
			if( camdata.CADSegmentList == null || camdata.CADSegmentList.Count == 0 ) {
				return false;
			}

			// forward arrangement
			camSegmentList = GetMainPathOrderedSegment( camdata );
			if( camdata.IsReverse ) {
				ReverseCAMSegmentList( ref camSegmentList );
			}
			GetMainPathOrderedSegment_New( camdata );
			return true;
		}

		public static bool BuildCAMSegment_New( CAMData camData, out PathCAMSegmentPage camSegmentPage )
		{
			camSegmentPage = new PathCAMSegmentPage();

			if( camData.CADSegmentList == null || camData.CADSegmentList.Count == 0 ) {
				return false;
			}

			List<ICAMSegmentElement> leadInSegment = BuildLeadCAMSegment( camData, true );
			camSegmentPage.LeadInSegment = leadInSegment;

			// forward arrangement of main path
			List<ICAMSegmentElement> camSegmentList = GetMainPathOrderedSegment_New2( camData );
			if( camData.IsReverse ) {
				ReverseCAMSegmentList( ref camSegmentList );
			}
			camSegmentPage.MainPathSegment = camSegmentList;

			List<ICAMSegmentElement> leadOutSegment = BuildLeadCAMSegment( camData, false );
			camSegmentPage.LeadOutSegment = leadOutSegment;
			return true;
		}

		public static CAMPoint GetCAMPoint( CADPoint cadPoint, bool isToolVecReverse )
		{
			if( cadPoint == null ) {
				return null;
			}
			gp_Dir toolDir = isToolVecReverse ? cadPoint.NormalVec_1st.Reversed() : cadPoint.NormalVec_1st;
			CAMPoint camPoint = new CAMPoint( cadPoint, toolDir );
			return camPoint;
		}

		public static gp_Vec GetModifyToolVecByMap( CAMData camData, (int, int) nTargetPnt )
		{
			CADPoint targetCADPnt = camData.CADSegmentList[ nTargetPnt.Item1 ].PointList[ nTargetPnt.Item2 ];
			CAMPoint targetCAMPnt = GetCAMPoint( targetCADPnt, camData.IsToolVecReverse );
			camData.ToolVecModifyMap_New.TryGetValue( nTargetPnt, out var value1 );
			gp_Vec startVec = GetVecFromAB( targetCAMPnt,
				value1.Item1 * Math.PI / 180,
				value1.Item2 * Math.PI / 180
				);
			return startVec;
		}

		public static gp_Vec GetVecFromAB( CAMPoint camPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( camPoint.ToolVec );
			}

			// get the x, y, z direction
			gp_Dir x = camPoint.CADPoint.TangentVec;
			gp_Dir z = camPoint.CADPoint.NormalVec_1st;
			gp_Dir y = z.Crossed( x );

			// X:Y:Z = tanA:tanB:1
			double X = 0;
			double Y = 0;
			double Z = 0;
			if( dRA_rad == 0 ) {
				X = 0;
				Z = 1;
			}
			else {
				X = dRA_rad < 0 ? -1 : 1;
				Z = X / Math.Tan( dRA_rad );
			}
			Y = Z * Math.Tan( dRB_rad );
			gp_Dir dir1 = new gp_Dir( x.XYZ() * X + y.XYZ() * Y + z.XYZ() * Z );
			return new gp_Vec( dir1.XYZ() );
		}

		static List<ICAMSegmentElement> GetMainPathOrderedSegment( CAMData camdata )
		{
			List<ICAMSegmentElement> camSegmentList = new List<ICAMSegmentElement>();
			(int, int) startPointIndex = camdata.NewStartPoint;

			// segment - line
			if( camdata.CADSegmentList[ startPointIndex.Item1 ].ContourType == EContourType.Line ) {

				// separate start index segment
				List<LineCAMSegment> startSegmentSeparatedList = SeparateCADLineSegmentToCAMSegment(
																(LineCADSegment)camdata.CADSegmentList[ startPointIndex.Item1 ], startPointIndex.Item2, camdata.IsToolVecReverse );
				// down cast
				List<ICAMSegmentElement> startSegmentSeparatedListTransformed = startSegmentSeparatedList.Cast<ICAMSegmentElement>().ToList();

				// add start segment
				camSegmentList.Add( startSegmentSeparatedList[ 0 ] );

				// add the rest segments
				camSegmentList.AddRange( BuildOrderedCAMListAfterStartSegment( camdata, startSegmentSeparatedListTransformed ) );
			}

			// segment - arc
			else {
				List<ArcCAMSegment> startSegmentSeparatedList = SeparateCADArcSegmentToCAMSegment(
																(ArcCADSegment)camdata.CADSegmentList[ startPointIndex.Item1 ], startPointIndex.Item2, camdata.IsToolVecReverse );
				List<ICAMSegmentElement> startSegmentSeparatedListTransformed = startSegmentSeparatedList.Cast<ICAMSegmentElement>().ToList();
				camSegmentList.Add( startSegmentSeparatedList[ 0 ] );
				camSegmentList.AddRange( BuildOrderedCAMListAfterStartSegment( camdata, startSegmentSeparatedListTransformed ) );
			}
			return camSegmentList;
		}

		static List<ICAMSegmentElement> GetMainPathOrderedSegment_New2( CAMData camdata )
		{
			if (camdata == null || camdata.BreakedCAMSegmentList == null || camdata.BreakedCAMSegmentList.Count == 0 ) {
				return new List<ICAMSegmentElement>();
			}
			int nStartPointIndex = camdata.StartPointIndex;
			List<ICAMSegmentElement> camSegmentList = new List<ICAMSegmentElement>();

			camSegmentList.AddRange( camdata.BreakedCAMSegmentList.GetRange( nStartPointIndex, camdata.BreakedCAMSegmentList.Count - nStartPointIndex ) );
			if( nStartPointIndex > 0 ) {
				camSegmentList.AddRange( camdata.BreakedCAMSegmentList.GetRange( 0, nStartPointIndex +1) );
			}
			return camSegmentList;
		}

		static List<((int SegmentIdx, int PointIdxt) start, (int SegmentIdx, int PointIdx) end)> GetMainPathOrderedSegment_New( CAMData camData )
		{
			if( camData.ToolVecModifyMap_New.Count <= 1 ) {

				List<((int SegmentIdx, int PointIdxt) start, (int SegmentIdx, int PointIdx) end)> unSeparateIntervalList = new List<((int SegmentIdx, int PointIdxt) start, (int SegmentIdx, int PointIdx) end)>();
				for( int i = 0; i < camData.CADSegmentList.Count; i++ ) {
					if( i == 0 ) {
						unSeparateIntervalList.Add( ((i, 0), (i, camData.CADSegmentList[ i ].PointList.Count - 1)) );
						continue;
					}
					else {
						unSeparateIntervalList.Add( (unSeparateIntervalList.Last().end, (i, camData.CADSegmentList[ i ].PointList.Count - 1)) );
						continue;
					}
				}
				return unSeparateIntervalList;
			}

			// 獲得有哪些區間
			List<((int SegmentIdx, int PointIdxt) start, (int SegmentIdx, int PointIdx) end)> interpolateIntervalList = GetInterpolateIntervalList( camData );

			List<((int SegmentIdx, int PointIdxt) start, (int SegmentIdx, int PointIdx) end)> separateIntervalList = new List<((int SegmentIdx, int PointIdxt) start, (int SegmentIdx, int PointIdx) end)>();

			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {
				if( interpolateIntervalList[ i ].start.SegmentIdx == interpolateIntervalList[ i ].end.SegmentIdx ) {
					separateIntervalList.Add( interpolateIntervalList[ i ] );
					continue;
				}


				if( interpolateIntervalList[ i ].end.SegmentIdx > interpolateIntervalList[ i ].start.SegmentIdx ) {
					for( int j = interpolateIntervalList[ i ].start.SegmentIdx; j <= interpolateIntervalList[ i ].end.SegmentIdx; j++ ) {

						// 這個區段第一個
						if( j == interpolateIntervalList[ i ].start.SegmentIdx ) {
							int nThisSegmentEndPntIndx = camData.CADSegmentList[ interpolateIntervalList[ i ].start.SegmentIdx ].PointList.Count - 1;
							separateIntervalList.Add( (interpolateIntervalList[ i ].start, (interpolateIntervalList[ i ].start.SegmentIdx, nThisSegmentEndPntIndx)) );
							continue;
						}

						// 這個區段中間
						if( j > interpolateIntervalList[ i ].start.SegmentIdx && j < interpolateIntervalList[ i ].end.SegmentIdx ) {
							separateIntervalList.Add( (separateIntervalList.Last().end, (j, camData.CADSegmentList[ j ].PointList.Count - 1)) );
							continue;
						}

						// 這個區段最後
						if( j == interpolateIntervalList[ i ].end.SegmentIdx ) {
							separateIntervalList.Add( (separateIntervalList.Last().end, (j, interpolateIntervalList[ i ].end.PointIdx)) );
							continue;
						}
					}
					continue;
				}

				// 迴轉處
				for( int k = interpolateIntervalList[ i ].start.SegmentIdx; k < camData.CADSegmentList.Count; k++ ) {

					// 這個區段第一個
					if( k == interpolateIntervalList[ i ].start.SegmentIdx ) {
						int nThisSegmentEndPntIndx = camData.CADSegmentList[ interpolateIntervalList[ i ].start.SegmentIdx ].PointList.Count - 1;
						separateIntervalList.Add( (interpolateIntervalList[ i ].start, (interpolateIntervalList[ i ].start.SegmentIdx, nThisSegmentEndPntIndx)) );
						continue;
					}

					// 這個區段中間
					if( k > interpolateIntervalList[ i ].start.SegmentIdx && k < interpolateIntervalList[ i ].end.SegmentIdx ) {
						separateIntervalList.Add( (separateIntervalList.Last().end, (k, camData.CADSegmentList[ k ].PointList.Count - 1)) );
						continue;
					}
				}

				for( int k = 0; k <= interpolateIntervalList[ i ].end.SegmentIdx; k++ ) {
					if( k != interpolateIntervalList[ i ].end.SegmentIdx ) {
						separateIntervalList.Add( (( separateIntervalList.Last().end ), (k, camData.CADSegmentList[ k ].PointList.Count - 1)) );
						continue;
					}
					else {
						separateIntervalList.Add( (( separateIntervalList.Last().end ), interpolateIntervalList[ i ].end) );
					}
				}
			}
			return separateIntervalList;

		}

		static void ReverseCAMSegmentList( ref List<ICAMSegmentElement> camSegmentList )
		{
			if( camSegmentList == null || camSegmentList.Count == 0 ) {
				return;
			}

			// change segment order
			camSegmentList.Reverse();

			// reverse start / end point
			for( int i = 0; i < camSegmentList.Count; i++ ) {
				if( camSegmentList[ i ].ContourType == EContourType.Line ) {
					LineCAMSegment reversedLineCAMSegment = new LineCAMSegment( camSegmentList[ i ].EndPoint, camSegmentList[ i ].StartPoint,false );
					camSegmentList[ i ] = reversedLineCAMSegment;
					continue;
				}
				if( camSegmentList[ i ].ContourType == EContourType.Arc ) {
					ArcCAMSegment oldSegment = (ArcCAMSegment)camSegmentList[ i ];
					ArcCAMSegment reversedArcCAMSegment = new ArcCAMSegment( oldSegment.EndPoint, oldSegment.StartPoint, oldSegment.MidPoint,false );
					camSegmentList[ i ] = reversedArcCAMSegment;
				}
			}
		}

		static List<LineCAMSegment> SeparateCADLineSegmentToCAMSegment( LineCADSegment lineSegment, int nStartPointIndex, bool isToolVecReverse )
		{
			if( nStartPointIndex < 0 || nStartPointIndex >= lineSegment.PointList.Count ) {
				return null;
			}
			if( nStartPointIndex == 0 ) {
				List<LineCAMSegment> singleLineSegment = new List<LineCAMSegment> {
					BuildCAMLineSegment(lineSegment.PointList,  isToolVecReverse)};
				return singleLineSegment;
			}
			LineCAMSegment endPartLineSegment = BuildCAMLineSegment( lineSegment.PointList.GetRange( 0, nStartPointIndex + 1 ), isToolVecReverse );
			LineCAMSegment startPartLineSegment = BuildCAMLineSegment( lineSegment.PointList.GetRange( nStartPointIndex, lineSegment.PointList.Count - nStartPointIndex ), isToolVecReverse );
			List<LineCAMSegment> separatedLineSegmentList = new List<LineCAMSegment>{
															startPartLineSegment,
															endPartLineSegment,};
			return separatedLineSegmentList;
		}

		static List<ArcCAMSegment> SeparateCADArcSegmentToCAMSegment( ArcCADSegment arcSegment, int startPointIndex, bool isToolVecReverse )
		{
			if( startPointIndex < 0 || startPointIndex >= arcSegment.PointList.Count ) {
				return null;
			}
			if( startPointIndex == 0 ) {
				List<ArcCAMSegment> singleArcSegmentList = new List<ArcCAMSegment> {
					BuildCAMArcSegment( arcSegment.PointList, isToolVecReverse )};
				return singleArcSegmentList;
			}

			ArcCAMSegment endPartArcSegment = BuildCAMArcSegment( arcSegment.PointList.GetRange( 0, startPointIndex + 1 ), isToolVecReverse );
			ArcCAMSegment startPartArcSegment = BuildCAMArcSegment( arcSegment.PointList.GetRange( startPointIndex, arcSegment.PointList.Count - startPointIndex ), isToolVecReverse );
			List<ArcCAMSegment> separatedArcSegmentList = new List<ArcCAMSegment>{
														endPartArcSegment,
														startPartArcSegment};
			return separatedArcSegmentList;
		}

		static List<ICAMSegmentElement> BuildOrderedCAMListAfterStartSegment( CAMData camdata, List<ICAMSegmentElement> startSegmentSeparatedList )
		{
			List<ICAMSegmentElement> cAMSegmentElements = new List<ICAMSegmentElement>();
			(int, int) startPointIndex = camdata.NewStartPoint;

			// 起點下一個到最後
			for( int i = startPointIndex.Item1 + 1; i < camdata.CADSegmentList.Count; i++ ) {
				cAMSegmentElements.Add( BuildCAMSegment( camdata.CADSegmentList[ i ], camdata.IsToolVecReverse ) );
			}

			// 第一個到起點前
			for( int i = 0; i < startPointIndex.Item1; i++ ) {
				cAMSegmentElements.Add( BuildCAMSegment( camdata.CADSegmentList[ i ], camdata.IsToolVecReverse ) );
			}

			// 起點這個segment
			if( startSegmentSeparatedList.Count > 1 ) {
				for( int i = 1; i < startSegmentSeparatedList.Count; i++ ) {
					cAMSegmentElements.Add( startSegmentSeparatedList[ i ] );
				}
			}
			return cAMSegmentElements;
		}

		static ICAMSegmentElement BuildCAMSegment( ICADSegmentElement cadSegment, bool isToolVecReverse )
		{
			if( cadSegment.ContourType == EContourType.Line ) {
				return BuildCAMLineSegment( cadSegment.PointList, isToolVecReverse );
			}
			else {
				return BuildCAMArcSegment( cadSegment.PointList, isToolVecReverse );
			}
		}

		static List<ICAMSegmentElement> BuildLeadCAMSegment( CAMData camData, bool isLeadIn )
		{
			List<ICAMSegmentElement> LeadCADSegment = new List<ICAMSegmentElement>();
			if( isLeadIn ) {
				if( camData.LeadLineParam.LeadIn.Type != LeadLineType.None ) {
					ICADSegmentElement leadInCADSegment = BuildLeadCADSegment( camData, isLeadIn );
					ICAMSegmentElement leadInCAMSegment = BuildCAMSegment( leadInCADSegment, camData.IsToolVecReverse );
					LeadCADSegment.Add( leadInCAMSegment );
					return LeadCADSegment;
				}
			}
			if( camData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
				ICADSegmentElement leadInCADSegment = BuildLeadCADSegment( camData, isLeadIn );
				ICAMSegmentElement leadInCAMSegment = BuildCAMSegment( leadInCADSegment, camData.IsToolVecReverse );
				LeadCADSegment.Add( leadInCAMSegment );
				return LeadCADSegment;
			}
			return LeadCADSegment;
		}

		static ICADSegmentElement BuildLeadCADSegment( CAMData camData, bool leadin )
		{
			int nStartPointSegmentIndex = camData.StartPointIndex;
			
			(int, int) startPointIndex = camData.NewStartPoint;
			CADPoint startCadPoint = camData.BreakedCAMSegmentList[ nStartPointSegmentIndex ].StartPoint.CADPoint;
			gp_Dir startPointToolVec = camData.BreakedCAMSegmentList[ nStartPointSegmentIndex ].StartPoint.ToolVec;

			if( leadin ) {
				if( camData.LeadLineParam.LeadIn.Type == LeadLineType.Line ) {

					LeadHelper.BuildStraightLeadLine_New( startCadPoint, startPointToolVec, true, camData.LeadLineParam.LeadIn.Length, camData.LeadLineParam.LeadIn.Angle, camData.LeadLineParam.IsChangeLeadDirection, camData.IsReverse, out gp_Pnt leadLineEndPoint, out _ );

					// lead in start from lead end
					List<gp_Pnt> leadPointList = new List<gp_Pnt>() { leadLineEndPoint, startCadPoint.Point };
					List<CADPoint> leadCadPointList = new List<CADPoint>();
					for( int i = 0; i < leadPointList.Count; i++ ) {
						CADPoint cadPoint = new CADPoint( leadPointList[ i ], startPointToolVec, startPointToolVec, startCadPoint.TangentVec );
						leadCadPointList.Add( cadPoint );
					}
					LineCADSegment lineCADSegment = new LineCADSegment( leadCadPointList, 0, 0 );
					return lineCADSegment;
				}
				if( camData.LeadLineParam.LeadIn.Type == LeadLineType.Arc ) {
					LeadHelper.BuildArcLead_New( startCadPoint, startPointToolVec, true, camData.LeadLineParam.LeadIn.Length, camData.LeadLineParam.LeadIn.Angle, camData.LeadLineParam.IsChangeLeadDirection, camData.IsReverse, out gp_Pnt leadEndPnt, out gp_Pnt leadMidPnt, out _ );
					List<gp_Pnt> leadPntList = new List<gp_Pnt>() { leadEndPnt, leadMidPnt, startCadPoint.Point };
					List<CADPoint> leadCadPointList = new List<CADPoint>();
					for( int i = 0; i < leadPntList.Count; i++ ) {
						CADPoint cadPoint = new CADPoint( leadPntList[ i ], startPointToolVec, startPointToolVec, startCadPoint.TangentVec );
						leadCadPointList.Add( cadPoint );
					}
					ArcCADSegment arcCADSegment = new ArcCADSegment( leadCadPointList, 0, 0 );
					return arcCADSegment;
				}
			}

			if( camData.LeadLineParam.LeadOut.Type == LeadLineType.Line ) {

				LeadHelper.BuildStraightLeadLine_New( startCadPoint, startPointToolVec, false, camData.LeadLineParam.LeadOut.Length, camData.LeadLineParam.LeadOut.Angle, camData.LeadLineParam.IsChangeLeadDirection, camData.IsReverse, out gp_Pnt leadLineEndPoint, out _ );

				// lead out start from start point
				List<gp_Pnt> leadPointList = new List<gp_Pnt>() { startCadPoint.Point, leadLineEndPoint };
				List<CADPoint> leadCadPointList = new List<CADPoint>();
				for( int i = 0; i < leadPointList.Count; i++ ) {
					CADPoint cadPoint = new CADPoint( leadPointList[ i ], startPointToolVec, startPointToolVec, startCadPoint.TangentVec );
					leadCadPointList.Add( cadPoint );
				}
				LineCADSegment lineCADSegment = new LineCADSegment( leadCadPointList, 0, 0 );
				return lineCADSegment;
			}
			if( camData.LeadLineParam.LeadIn.Type == LeadLineType.Arc ) {
				LeadHelper.BuildArcLead_New( startCadPoint, startPointToolVec, false, camData.LeadLineParam.LeadOut.Length, camData.LeadLineParam.LeadOut.Angle, camData.LeadLineParam.IsChangeLeadDirection, camData.IsReverse, out gp_Pnt leadEndPnt, out gp_Pnt leadMidPnt, out _ );
				List<gp_Pnt> leadPntList = new List<gp_Pnt>() { startCadPoint.Point, leadMidPnt, leadEndPnt };
				List<CADPoint> leadCadPointList = new List<CADPoint>();
				for( int i = 0; i < leadPntList.Count; i++ ) {
					CADPoint cadPoint = new CADPoint( leadPntList[ i ], startPointToolVec, startPointToolVec, startCadPoint.TangentVec );
					leadCadPointList.Add( cadPoint );
				}
				ArcCADSegment arcCADSegment = new ArcCADSegment( leadCadPointList, leadEndPnt.Distance( startCadPoint.Point ), leadEndPnt.Distance( startCadPoint.Point ) );
				return arcCADSegment;
			}
			return null;
		}

		public static gp_Dir GetStartPointToolVec( CAMData camData )
		{
			gp_Dir startPointToolVec = new gp_Dir();
			if( camData.ToolVecModifyMap_New.Count == 0 ) {
				CADPoint cadPoint = camData.CADSegmentList[ camData.NewStartPoint.Item1 ].PointList[ camData.NewStartPoint.Item2 ];
				startPointToolVec = cadPoint.NormalVec_1st;
				return startPointToolVec;
			}
			if( camData.ToolVecModifyMap_New.Count == 1 ) {
				(int, int) targetPointSegmentIndex = camData.ToolVecModifyMap_New.Keys.First();
				CADPoint targetPoint = camData.CADSegmentList[ targetPointSegmentIndex.Item1 ].PointList[ targetPointSegmentIndex.Item2 ];
				CAMPoint targetCAMPoint = BuildCAMSegmentHelper.GetCAMPoint( targetPoint, camData.IsToolVecReverse );
				gp_Vec newVec = GetVecFromAB( targetCAMPoint,
					camData.ToolVecModifyMap_New.Values.First().Item1 * Math.PI / 180,
					camData.ToolVecModifyMap_New.Values.First().Item2 * Math.PI / 180 );
				startPointToolVec = new gp_Dir( newVec );
				return startPointToolVec;
			}
			List<((int, int), (int, int))> interpolateIntervalList = GetInterpolateIntervalList( camData );
			((int, int), (int, int)) startPointSegmentRange = GetTargetPntSegmentRange( interpolateIntervalList, camData.NewStartPoint, out _ );
			InterpolateToolVec( camData, startPointSegmentRange.Item1, startPointSegmentRange.Item2, out double dTotalLength );
			gp_Vec segmentsStartVec = BuildCAMSegmentHelper.GetModifyToolVecByMap( camData, startPointSegmentRange.Item1 );
			gp_Vec segmentsEdnVec = BuildCAMSegmentHelper.GetModifyToolVecByMap( camData, startPointSegmentRange.Item2 );
			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( segmentsStartVec, segmentsEdnVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			double weight = GetPntToolVecWeight( camData, startPointSegmentRange, camData.NewStartPoint, dTotalLength );

			gp_Quaternion q = new gp_Quaternion();
			slerp.Interpolate( weight, ref q );
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetRotation( q );
			gp_Dir toolVecDir = new gp_Dir( segmentsStartVec.Transformed( trsf ) );
			return toolVecDir;

		}

		public static gp_Dir GetSegmentPointToolVec( CAMData camData, (int segment, int pointIndex) targetPoint )
		{
			if (camData.ToolVecModifyMap_New.Count == 1 ) {
				
					(int, int) targetPointSegmentIndex = camData.ToolVecModifyMap_New.Keys.First();
					CADPoint modifyPoint = camData.CADSegmentList[ targetPointSegmentIndex.Item1 ].PointList[ targetPointSegmentIndex.Item2 ];
					CAMPoint targetCAMPoint = BuildCAMSegmentHelper.GetCAMPoint( modifyPoint, camData.IsToolVecReverse );
					gp_Vec newVec = GetVecFromAB( targetCAMPoint,
						camData.ToolVecModifyMap_New.Values.First().Item1 * Math.PI / 180,
						camData.ToolVecModifyMap_New.Values.First().Item2 * Math.PI / 180 );
					gp_Dir assignDir = new gp_Dir( newVec );
				return assignDir;
				
				
			}

			List<((int, int), (int, int))> interpolateIntervalList = GetInterpolateIntervalList( camData );
			((int, int), (int, int)) targetPointSegmentRange = GetTargetPntSegmentRange( interpolateIntervalList, targetPoint, out _ );
			InterpolateToolVec( camData, targetPointSegmentRange.Item1, targetPointSegmentRange.Item2, out double dTotalLength );
			gp_Vec segmentsStartVec = GetModifyToolVecByMap( camData, targetPointSegmentRange.Item1 );
			gp_Vec segmentsEdnVec = GetModifyToolVecByMap( camData, targetPointSegmentRange.Item2 );

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( segmentsStartVec, segmentsEdnVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			double weight = GetPntToolVecWeight( camData, targetPointSegmentRange, targetPoint, dTotalLength );

			gp_Quaternion q = new gp_Quaternion();
			slerp.Interpolate( weight, ref q );
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetRotation( q );
			gp_Dir toolVecDir = new gp_Dir( segmentsStartVec.Transformed( trsf ) );
			return toolVecDir;
		}

		public static List<(gp_Pnt point, gp_Dir toolVec, bool isModiyToolVec)> InterpolateToolVec( CAMData camdata, (int, int) nStartIndex, (int, int) nEndIndex, out double dTotalLength )
		{
			List<(gp_Pnt point, double dDistance, bool isModifyToolVec)> toolVecLocation = new List<(gp_Pnt point, double dDistance, bool isModifyToolVec)>();
			List<(gp_Pnt point, gp_Dir toolVec, bool isModiyToolVec)> points = new List<(gp_Pnt point, gp_Dir toolVecbool, bool isModiyToolVec)>();
			
			// 計算長度
			dTotalLength = 0;

			// 同一個segment內
			if( nStartIndex.Item1 == nEndIndex.Item1 && nStartIndex.Item2 < nEndIndex.Item2 ) {

				if( camdata.CADSegmentList[ nStartIndex.Item1 ] is LineCADSegment lineCADSegment ) {
					toolVecLocation.AddRange( GetLineToolVecLocation( lineCADSegment, nStartIndex.Item2, true, nEndIndex.Item2, true, ref dTotalLength, false ) );
				}
				if( camdata.CADSegmentList[ nStartIndex.Item1 ] is ArcCADSegment arcCADSegment ) {
					toolVecLocation.AddRange( GetArcToolVecLoaction( arcCADSegment, nStartIndex.Item2, true, nEndIndex.Item2, true, ref dTotalLength, false ) );
				}
			}
			else {
				// 前一段到後幾段
				if( nStartIndex.Item1 < nEndIndex.Item1 ) {

					if( camdata.CADSegmentList[ nStartIndex.Item1 ] is LineCADSegment lineCADSegment ) {
						toolVecLocation.AddRange( GetLineToolVecLocation( lineCADSegment, nStartIndex.Item2, true, lineCADSegment.PointList.Count - 1, false, ref dTotalLength, false ) );
					}
					if( camdata.CADSegmentList[ nStartIndex.Item1 ] is ArcCADSegment arcCADSegment ) {
						toolVecLocation.AddRange( GetArcToolVecLoaction( arcCADSegment, nStartIndex.Item2, true, arcCADSegment.PointList.Count - 1, false, ref dTotalLength, false ) );
					}


					for( int i = nStartIndex.Item1 + 1; i < nEndIndex.Item1; i++ ) {

						if( camdata.CADSegmentList[ i ] is LineCADSegment lineCADSegment_1 ) {
							toolVecLocation.AddRange( GetLineToolVecLocation( lineCADSegment_1, 0, false, lineCADSegment_1.PointList.Count - 1, false, ref dTotalLength, false ) );
						}
						if( camdata.CADSegmentList[ i ] is ArcCADSegment arcCADSegment_1 ) {
							toolVecLocation.AddRange( GetArcToolVecLoaction( arcCADSegment_1, 0, false, arcCADSegment_1.PointList.Count - 1, false, ref dTotalLength, false ) );
						}
					}

					// last Segment length
					if( camdata.CADSegmentList[ nEndIndex.Item1 ] is LineCADSegment lineCADSegment_2 ) {
						toolVecLocation.AddRange( GetLineToolVecLocation( lineCADSegment_2, 0, false, nEndIndex.Item2, true, ref dTotalLength, false ) );
					}
					if( camdata.CADSegmentList[ nEndIndex.Item1 ] is ArcCADSegment arcCADSegment_2 ) {
						toolVecLocation.AddRange( GetArcToolVecLoaction( arcCADSegment_2, 0, false, nEndIndex.Item2, true, ref dTotalLength, false ) );
					}
				}

				// 最後一段到前面
				else {

					if( camdata.CADSegmentList[ nStartIndex.Item1 ] is LineCADSegment lineCADSegment ) {
						toolVecLocation.AddRange( GetLineToolVecLocation( lineCADSegment, nStartIndex.Item2, true, lineCADSegment.PointList.Count - 1, false, ref dTotalLength, false ) );
					}
					if( camdata.CADSegmentList[ nStartIndex.Item1 ] is ArcCADSegment arcCADSegment ) {
						toolVecLocation.AddRange( GetArcToolVecLoaction( arcCADSegment, nStartIndex.Item2, true, arcCADSegment.PointList.Count - 1, false, ref dTotalLength, false ) );
					}
					for( int i = nStartIndex.Item1 + 1; i < camdata.CADSegmentList.Count; i++ ) {
						if( camdata.CADSegmentList[ i ] is LineCADSegment lineCADSegment_1 ) {
							toolVecLocation.AddRange( GetLineToolVecLocation( lineCADSegment_1, 0, false, lineCADSegment_1.PointList.Count - 1, false, ref dTotalLength, false ) );
						}
						if( camdata.CADSegmentList[ i ] is ArcCADSegment arcCADSegment_1 ) {
							toolVecLocation.AddRange( GetArcToolVecLoaction( arcCADSegment_1, 0, false, arcCADSegment_1.PointList.Count - 1, false, ref dTotalLength, false ) );
						}
					}

					for( int i = 0; i < nEndIndex.Item1; i++ ) {
						if( camdata.CADSegmentList[ i ] is LineCADSegment lineCADSegment_1 ) {
							toolVecLocation.AddRange( GetLineToolVecLocation( lineCADSegment_1, 0, false, lineCADSegment_1.PointList.Count - 1, false, ref dTotalLength, false ) );
						}
						if( camdata.CADSegmentList[ i ] is ArcCADSegment arcCADSegment_1 ) {
							toolVecLocation.AddRange( GetArcToolVecLoaction( arcCADSegment_1, 0, false, arcCADSegment_1.PointList.Count - 1, false, ref dTotalLength, false ) );
						}
					}

					// first Segment length
					if( camdata.CADSegmentList[ nEndIndex.Item1 ] is LineCADSegment lineCADSegment_2 ) {
						toolVecLocation.AddRange( GetLineToolVecLocation( lineCADSegment_2, 0, false, nEndIndex.Item2, true, ref dTotalLength, false ) );
					}
					if( camdata.CADSegmentList[ nEndIndex.Item1 ] is ArcCADSegment arcCADSegment_2 ) {
						toolVecLocation.AddRange( GetArcToolVecLoaction( arcCADSegment_2, 0, false, nEndIndex.Item2, true, ref dTotalLength, false ) );
					}
				}
			}


			// get the start and end tool vector
			gp_Vec startVec = BuildCAMSegmentHelper.GetModifyToolVecByMap( camdata, ( nStartIndex ) );
			gp_Vec endVec = BuildCAMSegmentHelper.GetModifyToolVecByMap( camdata, ( nEndIndex ) );

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
			double accumulatedDistance = 0;

			for( int i = 0; i < toolVecLocation.Count; i++ ) {

				accumulatedDistance += toolVecLocation[ i ].dDistance;
				double t = accumulatedDistance / dTotalLength;

				gp_Quaternion q = new gp_Quaternion();
				slerp.Interpolate( t, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				gp_Dir toolVecDir = new gp_Dir( startVec.Transformed( trsf ) );
				points.Add( (toolVecLocation[ i ].point, toolVecDir, toolVecLocation[ i ].isModifyToolVec) );

			}
			return points;
		}

		static List<(gp_Pnt point, double dDistance, bool isModiyToolVec)> GetArcToolVecLoaction( ArcCADSegment arcCADSegment, int nStartIndex, bool IsSModify, int nEndIndex, bool IsEModify, ref double dTotalLength, bool IsNeedToDrawStart )
		{
			List<(gp_Pnt point, double dDistance, bool isModiyToolVec)> pointToolVecList = new List<(gp_Pnt point, double dDistance, bool isModiyToolVec)>();

			if( IsNeedToDrawStart ) {
				pointToolVecList.Add( (arcCADSegment.PointList[ nStartIndex ].Point, 0, IsSModify) );
			}
			int midIndex = arcCADSegment.PointList.Count / 2;

			// 圓弧在中間
			if( nStartIndex < midIndex && nEndIndex > midIndex ) {
				double dLenghtToMid = ( midIndex - nStartIndex ) * arcCADSegment.PointSpace;
				dTotalLength += dLenghtToMid;
				pointToolVecList.Add( (arcCADSegment.MidPoint.Point, dLenghtToMid, false) );

				double dLengthToArcEnd = ( nEndIndex - midIndex ) * arcCADSegment.PointSpace;
				dTotalLength += dLengthToArcEnd;
				pointToolVecList.Add( (arcCADSegment.PointList[ nEndIndex ].Point, dLengthToArcEnd, IsEModify) );
			}
			else {
				double dLengthFromStartToArcEnd = ( nEndIndex - nStartIndex ) * arcCADSegment.PointSpace;
				dTotalLength += dLengthFromStartToArcEnd;

				// 這段結尾
				pointToolVecList.Add( (arcCADSegment.PointList[ nEndIndex ].Point, dLengthFromStartToArcEnd, IsEModify) );
			}
			return pointToolVecList;
		}

		static List<(gp_Pnt point, double dDistance, bool isModiyToolVec)> GetLineToolVecLocation( LineCADSegment lineCADSegment, int nStartIndex, bool IsSModify, int nEndIndex, bool IsEModify, ref double dTotalLength, bool IsNeedToDrawStart )
		{

			List<(gp_Pnt point, double dDistance, bool isModiyToolVec)> pointToolVecList = new List<(gp_Pnt point, double dDistance, bool isModiyToolVec)>();
			if( IsNeedToDrawStart ) {
				pointToolVecList.Add( (lineCADSegment.PointList[ nStartIndex ].Point, 0, IsSModify) );
			}
			double dDistanceFromStartToLineEnd = ( nEndIndex - nStartIndex ) * lineCADSegment.PointSpace;
			dTotalLength += dDistanceFromStartToLineEnd;

			// 這段結尾
			pointToolVecList.Add( (lineCADSegment.PointList[ nEndIndex ].Point, dDistanceFromStartToLineEnd, IsEModify) );

			return pointToolVecList;
		}

		public static List<((int, int), (int, int))> GetInterpolateIntervalList( CAMData camData )
		{
			// sort the modify data by index
			List<(int, int)> indexInOrder = camData.ToolVecModifyMap_New.Keys.ToList();
			indexInOrder.Sort();

			List<((int, int), (int, int))> intervalList = new List<((int, int), (int, int))>();
			if( camData.IsClosed ) {

				int firstModifySegment = indexInOrder[ 0 ].Item1;
				// for closed path, the index is wrapped
				for( int i = 0; i < indexInOrder.Count; i++ ) {
					int nextIndex = ( i + 1 ) % indexInOrder.Count;
					intervalList.Add( (indexInOrder[ i ], indexInOrder[ nextIndex ]) );
				}
			}
			else {
				for( int i = 0; i < indexInOrder.Count - 1; i++ ) {
					intervalList.Add( (indexInOrder[ i ], indexInOrder[ i + 1 ]) );
				}
			}
			return intervalList;
		}

		static ((int, int), (int, int)) GetTargetPntSegmentRange(
	List<((int segmentIndex, int pointIndex), (int segmentIndex, int pointIndex))> segments,
	(int segmentIndex, int pointIndex) target, out int index )
		{
			index = -1;
			for( int i = 0; i < segments.Count; i++ ) {
				(int segmentIndex, int pointIndex) start = segments[ i ].Item1;
				(int segmentIndex, int pointIndex) end = segments[ i ].Item2;

				// case 1: 同一個 segment
				if( start.segmentIndex == end.segmentIndex ) {
					if( target.segmentIndex == start.segmentIndex &&
						target.pointIndex >= start.pointIndex &&
						target.pointIndex <= end.pointIndex )
						index = i;
					return segments[ i ];
				}
				// case 2: 順向區間
				else if( start.segmentIndex < end.segmentIndex ) {
					if( target.segmentIndex > start.segmentIndex && target.segmentIndex < end.segmentIndex ) {
						index = i;
						return segments[ i ];
					}

					if( target.segmentIndex == start.segmentIndex && target.pointIndex >= start.pointIndex ) {
						index = i;
						return segments[ i ];
					}


					if( target.segmentIndex == end.segmentIndex && target.pointIndex <= end.pointIndex ) {
						index = i;
						return segments[ i ];
					}

				}
				// case 3: 迴轉區間（封閉環）
				else // start.segmentIndex > end.segmentIndex
				{
					bool inBeforeWrap =
						( target.segmentIndex > start.segmentIndex ) ||
						( target.segmentIndex == start.segmentIndex && target.pointIndex >= start.pointIndex );

					bool inAfterWrap =
						( target.segmentIndex < end.segmentIndex ) ||
						( target.segmentIndex == end.segmentIndex && target.pointIndex <= end.pointIndex );

					if( inBeforeWrap || inAfterWrap ) {
						index = i;
						return segments[ i ];
					}
				}
			}

			return ((-1, -1), (-1, -1));
		}


		static double GetPntToolVecWeight( CAMData camData, ((int segmentIndex, int pointIndex) startSegment, (int segmentIndex, int pointIndex) endSegment) segmentRange, (int segmentIndex, int pointIndex) targetPntInfo, double dSegmentRangeLength )
		{
			// all pnt in the same segment
			if( segmentRange.startSegment.segmentIndex == segmentRange.endSegment.segmentIndex ) {

				double dSegmentStartToStartPoint = ( targetPntInfo.pointIndex - segmentRange.startSegment.pointIndex ) * camData.CADSegmentList[ segmentRange.startSegment.segmentIndex ].PointSpace;
				double weight = dSegmentStartToStartPoint / dSegmentRangeLength;
				return weight;
			}

			// start segment index < end segment index
			if( segmentRange.startSegment.segmentIndex < segmentRange.endSegment.segmentIndex ) {

				double dSegmentStartToStartPoint = 0;
				for( int i = segmentRange.startSegment.segmentIndex; i <= segmentRange.endSegment.segmentIndex; i++ ) {
					if( i == targetPntInfo.segmentIndex && targetPntInfo.segmentIndex == segmentRange.startSegment.segmentIndex ) {
						dSegmentStartToStartPoint += ( targetPntInfo.pointIndex - segmentRange.startSegment.pointIndex ) * camData.CADSegmentList[ i ].PointSpace;
						break;
					}
					if( i == targetPntInfo.segmentIndex ) {
						dSegmentStartToStartPoint += ( targetPntInfo.pointIndex ) * camData.CADSegmentList[ i ].PointSpace;
						break;
					}
					if( i == segmentRange.startSegment.segmentIndex && i != targetPntInfo.segmentIndex ) {
						dSegmentStartToStartPoint += ( camData.CADSegmentList[ i ].PointList.Count - 1 - segmentRange.startSegment.pointIndex ) * camData.CADSegmentList[ i ].PointSpace;
					}
					else {
						dSegmentStartToStartPoint += camData.CADSegmentList[ i ].TotalLength;
					}
				}
				double weight = dSegmentStartToStartPoint / dSegmentRangeLength;
				return weight;
			}

			// start segment index > end segment
			double dSegmentStartToStartPoint_1 = 0;

			// 在後面這段
			if( targetPntInfo.segmentIndex > segmentRange.endSegment.segmentIndex ) {
				for( int i = segmentRange.startSegment.segmentIndex; i < camData.CADSegmentList.Count; i++ ) {
					if( i == segmentRange.startSegment.segmentIndex && i == targetPntInfo.segmentIndex ) {
						dSegmentStartToStartPoint_1 += ( targetPntInfo.pointIndex - segmentRange.startSegment.pointIndex ) * camData.CADSegmentList[ i ].PointSpace;
						double weight = dSegmentStartToStartPoint_1 / dSegmentRangeLength;
						return weight;
					}
					if( i == segmentRange.startSegment.segmentIndex ) {
						dSegmentStartToStartPoint_1 += ( camData.CADSegmentList[ i ].PointList.Count - 1 - segmentRange.startSegment.pointIndex ) * camData.CADSegmentList[ i ].PointSpace;
						continue;
					}
					if( i == targetPntInfo.segmentIndex ) {
						dSegmentStartToStartPoint_1 += targetPntInfo.pointIndex * camData.CADSegmentList[ i ].PointSpace;
						double weight = dSegmentStartToStartPoint_1 / dSegmentRangeLength;
						return weight;
					}

					dSegmentStartToStartPoint_1 += camData.CADSegmentList[ i ].TotalLength;

				}
			}

			// 在前面那段
			else {

				for( int i = segmentRange.startSegment.segmentIndex; i < camData.CADSegmentList.Count; i++ ) {
					if( i == segmentRange.startSegment.segmentIndex ) {
						dSegmentStartToStartPoint_1 += ( camData.CADSegmentList[ i ].PointList.Count - 1 - segmentRange.startSegment.pointIndex ) * camData.CADSegmentList[ i ].PointSpace;
						continue;
					}
					dSegmentStartToStartPoint_1 += camData.CADSegmentList[ i ].TotalLength;
				}
				for( int i = 0; i <= targetPntInfo.segmentIndex; i++ ) {
					if( i == targetPntInfo.segmentIndex ) {
						dSegmentStartToStartPoint_1 += targetPntInfo.pointIndex * camData.CADSegmentList[ i ].PointSpace;
						break;
					}
					dSegmentStartToStartPoint_1 += camData.CADSegmentList[ i ].TotalLength;
				}
				double weight = dSegmentStartToStartPoint_1 / dSegmentRangeLength;
				return weight;
			}
			return -1;
		}


		public static LineCAMSegment BuildCAMLineSegment( List<CADPoint> linePointList, bool isToolVecReverse )
		{
			LineCAMSegment camLineSegment = null;
			if( linePointList == null || linePointList.Count < 2 ) {
				return camLineSegment;
			}
			CAMPoint startCAMPoint = GetCAMPoint( linePointList[ 0 ], isToolVecReverse );
			CAMPoint endCAMPoint = GetCAMPoint( linePointList[ linePointList.Count - 1 ], isToolVecReverse );
			camLineSegment = new LineCAMSegment( startCAMPoint, endCAMPoint,false );
			return camLineSegment;
		}

		public static ArcCAMSegment BuildCAMArcSegment( List<CADPoint> arcPointList, bool isToolVecReverse )
		{
			ArcCAMSegment camArcSegment = null;
			if( arcPointList == null || arcPointList.Count < 2 ) {
				return camArcSegment;
			}
			CAMPoint startCAMPoint = GetCAMPoint( arcPointList[ 0 ], isToolVecReverse );
			CAMPoint endCAMPoint = GetCAMPoint( arcPointList[ arcPointList.Count - 1 ], isToolVecReverse );

			if( arcPointList.Count % 2 != 0 ) {
				CAMPoint realMidCAMPoint = GetCAMPoint( arcPointList[ arcPointList.Count / 2 ], isToolVecReverse );
				camArcSegment = new ArcCAMSegment( startCAMPoint, endCAMPoint, realMidCAMPoint,false );
				return camArcSegment;
			}

			CADPoint EstimatedMidCADPoint = GetArcMidPoint( arcPointList );
			CAMPoint EstimatedMidCAMPoint = GetCAMPoint( EstimatedMidCADPoint, isToolVecReverse );
			camArcSegment = new ArcCAMSegment( startCAMPoint, endCAMPoint, EstimatedMidCAMPoint,false );
			return camArcSegment;
		}

		static CADPoint GetArcMidPoint( List<CADPoint> arcPointList )
		{
			CADPoint midCadPoint = null;
			if( arcPointList == null || arcPointList.Count < 2 ) {
				return midCadPoint;
			}

			// two point is really close 
			if( arcPointList.Count == 2 ) {
				gp_Pnt midPnt = new gp_Pnt(
						 ( arcPointList[ 0 ].Point.X() + arcPointList[ 1 ].Point.X() ) / 2,
						 ( arcPointList[ 0 ].Point.Y() + arcPointList[ 1 ].Point.Y() ) / 2,
						 ( arcPointList[ 0 ].Point.Z() + arcPointList[ 1 ].Point.Z() ) / 2
					);
				midCadPoint = new CADPoint( midPnt,
						AverageDir( arcPointList[ 0 ].NormalVec_1st, arcPointList[ 1 ].NormalVec_1st ),
						AverageDir( arcPointList[ 0 ].NormalVec_1st, arcPointList[ 1 ].NormalVec_1st ),
						AverageDir( arcPointList[ 0 ].TangentVec, arcPointList[ 1 ].TangentVec ) );
				return midCadPoint;
			}
			return arcPointList[ arcPointList.Count / 2 ];
		}

		static gp_Dir AverageDir( gp_Dir d1, gp_Dir d2 )
		{
			gp_Vec vector = new gp_Vec( d1 );
			vector.Add( new gp_Vec( d2 ) );

			// two directions opposite
			if( vector.Magnitude() < 1e-3 ) {
				return new gp_Dir( d1.XYZ() );
			}

			vector.Normalize();
			return new gp_Dir( vector );
		}
	}
}
