using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Helper
{
	internal static class BuildCAMSegmentHelper
	{
		public static bool GetCAMSegmentPage( CAMData camData, out PathCAMSegmentPage camSegmentPage )
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

		public static CAMPoint GetCAMPointWithAssignDir( CADPoint cadPoint, gp_Dir assignDir)
		{
			if( cadPoint == null ) {
				return null;
			}
			CAMPoint camPoint = new CAMPoint( cadPoint, assignDir );
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


		static List<ICAMSegmentElement> GetMainPathOrderedSegment_New2( CAMData camdata )
		{
			if (camdata == null || camdata.BreakedCAMSegmentList == null || camdata.BreakedCAMSegmentList.Count == 0 ) {
				return new List<ICAMSegmentElement>();
			}
			List<ICAMSegmentElement> camSegmentList = camdata.BreakedCAMSegmentList;
			return camSegmentList;
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

		static ICAMSegmentElement BuildCAMSegment( ICADSegmentElement cadSegment, bool isToolVecReverse )
		{
			if( cadSegment.ContourType == EContourType.Line ) {
				return BuildCAMLineSegment( cadSegment.PointList, isToolVecReverse );
			}
			else {
				return BuildCAMArcSegment( cadSegment.PointList, isToolVecReverse );
			}
		}

		public static List<ICAMSegmentElement> BuildLeadCAMSegment( CAMData camData, bool isLeadIn )
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
			CADPoint startCadPoint = camData.BreakedCAMSegmentList.First().StartPoint.CADPoint;
			gp_Dir startPointToolVec = camData.BreakedCAMSegmentList.First().StartPoint.ToolVec;

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
			if( camData.LeadLineParam.LeadOut.Type == LeadLineType.Arc ) {
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

		public static LineCAMSegment BuildCAMLineSegment( List<CADPoint> linePointList, bool isToolVecReverse, bool isModify = false )
		{
			LineCAMSegment camLineSegment = null;
			if( linePointList == null || linePointList.Count < 2 ) {
				return camLineSegment;
			}
			CAMPoint startCAMPoint = GetCAMPoint( linePointList[ 0 ], isToolVecReverse );
			CAMPoint endCAMPoint = GetCAMPoint( linePointList[ linePointList.Count - 1 ], isToolVecReverse );
			camLineSegment = new LineCAMSegment( startCAMPoint, endCAMPoint, isModify );
			return camLineSegment;
		}

		public static ArcCAMSegment BuildCAMArcSegment( List<CADPoint> arcPointList, bool isToolVecReverse, bool isModify = false )
		{
			ArcCAMSegment camArcSegment = null;
			if( arcPointList == null || arcPointList.Count < 2 ) {
				return camArcSegment;
			}
			CAMPoint startCAMPoint = GetCAMPoint( arcPointList[ 0 ], isToolVecReverse );
			CAMPoint endCAMPoint = GetCAMPoint( arcPointList[ arcPointList.Count - 1 ], isToolVecReverse );

			if( arcPointList.Count % 2 != 0 ) {
				CAMPoint realMidCAMPoint = GetCAMPoint( arcPointList[ arcPointList.Count / 2 ], isToolVecReverse );
				camArcSegment = new ArcCAMSegment( startCAMPoint, endCAMPoint, realMidCAMPoint, isModify );
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
