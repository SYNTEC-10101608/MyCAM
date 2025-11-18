using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.Geom;
using OCC.gp;
using OCCTool;

namespace MyCAM.Helper
{
	internal static class LeadHelper
	{
		public static List<ICAMSegmentElement> BuildLeadCAMSegment( CraftData craftData, ICAMSegmentElement CAMSegConnectWithStartPnt, bool isLeadIn )
		{
			List<ICAMSegmentElement> LeadCADSegment = new List<ICAMSegmentElement>();
			if( isLeadIn ) {
				if( craftData.LeadLineParam.LeadIn.Type != LeadLineType.None ) {
					ICAMSegmentElement leadInCAMSegment = BuildLeadCADSegment( craftData, CAMSegConnectWithStartPnt, isLeadIn );
					LeadCADSegment.Add( leadInCAMSegment );
					return LeadCADSegment;
				}
			}
			if( craftData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
				ICAMSegmentElement leadInCAMSegment = BuildLeadCADSegment( craftData, CAMSegConnectWithStartPnt, isLeadIn );
				LeadCADSegment.Add( leadInCAMSegment );
				return LeadCADSegment;
			}
			return LeadCADSegment;
		}

		public static Geom_Curve BuildLeadGeom( bool isLeadIn, LeadLineType leadLineType, CAMPoint2 pntConnectWithLead, double dLeadLineLength, double dLeadLineAngle, bool isChangeLeadDirection, bool isReverse, out gp_Pnt leadEndPnt, out gp_Pnt leadMidPnt, out gp_Dir leadDir )
		{
			if( isLeadIn ) {
				return BuildLeadGeomByParam( true, leadLineType, pntConnectWithLead, dLeadLineLength, dLeadLineAngle, isChangeLeadDirection, isReverse , out leadEndPnt, out  leadMidPnt, out  leadDir );
			}
			return BuildLeadGeomByParam( false, leadLineType, pntConnectWithLead, dLeadLineLength, dLeadLineAngle, isChangeLeadDirection, isReverse, out leadEndPnt, out  leadMidPnt, out  leadDir );
		}

		static Geom_Curve BuildStraightLead( CAMPoint2 pntConnectWithPath, bool isLeadIn, double dLeadLineLength, double dLeadLineAngle, bool isChangeLeadDirection, bool isReverse, out gp_Pnt leadLineEndPoint, out gp_Dir leadDir )
		{
			leadLineEndPoint = new gp_Pnt();
			leadDir = new gp_Dir();
			// protection 
			if( dLeadLineLength <= 0 ) {
				return null;
			}

			//  establish a coordinate system through this point and its direction vector and normal vector
			gp_Dir XVec = new gp_Dir( -pntConnectWithPath.TangentVec.X(), -pntConnectWithPath.TangentVec.Y(), -pntConnectWithPath.TangentVec.Z() );
			gp_Dir ZVec = pntConnectWithPath.ToolVec;
			if( GeometryTool.IsParallel( XVec, ZVec ) ) {
				return null;
			}
			gp_Ax3 planeCS = new gp_Ax3( pntConnectWithPath.Point, ZVec, XVec );

			gp_Dir xDir = planeCS.XDirection();
			gp_Dir yDir = planeCS.YDirection();
			double angleRad = dLeadLineAngle * Math.PI / 180.0;
			double cosA = Math.Cos( angleRad );
			double sinA = Math.Sin( angleRad );

			// vector to remove from original point to the lead line end point
			gp_Vec dirVec2D = new gp_Vec( xDir ).Multiplied( cosA ) + new gp_Vec( yDir ).Multiplied( sinA );
			leadLineEndPoint = pntConnectWithPath.Point.Translated( dirVec2D.Multiplied( dLeadLineLength ) );

			// flip by y axis
			if( ( isLeadIn == false && isReverse == false ) || ( isLeadIn && isReverse ) ) {
				gp_Trsf mirrorTrsf = new gp_Trsf();
				mirrorTrsf.SetMirror( new gp_Ax1( planeCS.Location(), planeCS.YDirection() ) );
				leadLineEndPoint.Transform( mirrorTrsf );

			}

			// flip by x axis
			if( isChangeLeadDirection ) {
				gp_Trsf mirrorTrsf = new gp_Trsf();
				mirrorTrsf.SetMirror( new gp_Ax1( planeCS.Location(), planeCS.XDirection() ) );
				leadLineEndPoint.Transform( mirrorTrsf );
			}

			gp_Vec startToEndVec = new gp_Vec( pntConnectWithPath.Point, leadLineEndPoint );
			gp_Dir startToEndDir = new gp_Dir( startToEndVec );


			Geom_Line geomLine = new Geom_Line( pntConnectWithPath.Point, startToEndDir );
			Geom_TrimmedCurve trimmedLine = new Geom_TrimmedCurve( geomLine, 0, pntConnectWithPath.Point.Distance( leadLineEndPoint ) );
			leadDir = startToEndDir;
			if( isLeadIn ) {
				leadDir.Reverse();
			}
			return trimmedLine;
		}

		static Geom_Curve BuildArcLead( CAMPoint2 pntConnectWithPath, bool isLeadIn, double dLeadLineLength, double dLeadLineAngle, bool isChangeLeadDirection, bool isReverse, out gp_Pnt leadEndPnt, out gp_Pnt leadMidPnt, out gp_Dir leadDir )
		{
			leadEndPnt = new gp_Pnt();
			leadMidPnt = new gp_Pnt();
			leadDir = new gp_Dir();

			// protection
			if( dLeadLineLength <= 0 || dLeadLineAngle <= 0 ) {
				return null;
			}

			//  establish a coordinate system through this point and its direction vector and normal vector
			gp_Dir XVec = pntConnectWithPath.TangentVec;
			gp_Dir ZVec = pntConnectWithPath.ToolVec;
			if( GeometryTool.IsParallel( XVec, ZVec ) ) {
				return null;
			}

			gp_Ax3 leadLinePlane = new gp_Ax3( pntConnectWithPath.Point, ZVec, XVec );

			// circle center shifted along -Y
			gp_Vec movingDirection = new gp_Vec( leadLinePlane.YDirection() );
			movingDirection.Reverse();
			gp_Pnt circleCenterPnt = new gp_Pnt( leadLinePlane.Location().XYZ() + movingDirection.Multiplied( dLeadLineLength ).XYZ() );

			// circle definition
			gp_Ax2 circleAx2 = new gp_Ax2( circleCenterPnt, leadLinePlane.Direction(), leadLinePlane.YDirection() );
			Geom_Circle geomCircle = new Geom_Circle( new gp_Circ( circleAx2, dLeadLineLength ) );

			// compute angles
			double dArcAngle;
			if( isLeadIn ) {
				dArcAngle = dLeadLineAngle * Math.PI / 180.0;
			}
			else {
				dArcAngle = -dLeadLineAngle * Math.PI / 180.0;
			}
			Geom_TrimmedCurve leadLineCurve;
			if( isLeadIn ) {
				leadLineCurve = new Geom_TrimmedCurve( geomCircle, 0, dArcAngle, true );
			}
			else {
				leadLineCurve = new Geom_TrimmedCurve( geomCircle, dArcAngle, Math.PI * 2, true );
			}

			// mirror by y axis
			if( isReverse ) {
				gp_Trsf mirror = new gp_Trsf();
				mirror.SetMirror( new gp_Ax1( leadLinePlane.Location(), leadLinePlane.YDirection() ) );
				leadLineCurve.Transform( mirror );
			}

			// mirror by x axis
			if( isChangeLeadDirection ) {
				gp_Trsf mirror = new gp_Trsf();
				mirror.SetMirror( new gp_Ax1( leadLinePlane.Location(), leadLinePlane.XDirection() ) );
				leadLineCurve.Transform( mirror );
			}
			gp_Vec tangentVec = new gp_Vec();
			leadMidPnt = leadLineCurve.Value( ( leadLineCurve.LastParameter() + leadLineCurve.FirstParameter() ) * 0.5 );

			if( isLeadIn ) {
				leadLineCurve.D1( leadLineCurve.LastParameter(), ref leadEndPnt, ref tangentVec );
			}
			else {
				leadLineCurve.D1( leadLineCurve.FirstParameter(), ref leadEndPnt, ref tangentVec );
			}
			tangentVec.Reverse();
			leadDir = new gp_Dir( tangentVec );
			return leadLineCurve;
		}

		static Geom_Curve BuildLeadGeomByParam( bool isLeadIn, LeadLineType leadLineType, CAMPoint2 pntConnectWithLead, double dLeadLineLength, double dLeadLineAngle, bool isChangeLeadDirection, bool isReverse, out gp_Pnt leadEndPnt, out gp_Pnt leadMidPnt, out gp_Dir leadDir )
		{
			leadDir = new gp_Dir();
			leadEndPnt = new gp_Pnt();
			leadMidPnt = new gp_Pnt();
			if( leadLineType == LeadLineType.Line ) {
				return BuildStraightLead( pntConnectWithLead, isLeadIn, dLeadLineLength, dLeadLineAngle, isChangeLeadDirection, isReverse, out leadEndPnt, out leadDir );
			}
			if( leadLineType == LeadLineType.Arc ) {
				return BuildArcLead( pntConnectWithLead, isLeadIn, dLeadLineLength, dLeadLineAngle, isChangeLeadDirection, isReverse, out leadEndPnt, out leadMidPnt, out leadDir );
			}
			return null;
		}

		static ICAMSegmentElement BuildLeadCADSegment( CraftData craftData, ICAMSegmentElement CAMSegConnectWithLead, bool isLeadin )
		{
			CAMPoint2 pntConnectWithPath = isLeadin ? CAMSegConnectWithLead.StartPoint: CAMSegConnectWithLead.EndPoint;
			gp_Dir LeadToolVec = pntConnectWithPath.ToolVec;
			LeadParam leadParam = isLeadin ? craftData.LeadLineParam.LeadIn : craftData.LeadLineParam.LeadOut;

			// draw arc / line lead
			if( leadParam.Type == LeadLineType.Line ) {
				LeadHelper.BuildStraightLead(
					pntConnectWithPath, isLeadin,
					leadParam.Length, leadParam.Angle,
					craftData.LeadLineParam.IsChangeLeadDirection, craftData.IsReverse,
					out gp_Pnt endPnt, out _
				);

				// start point and end point
				var leadPoints = isLeadin
					? new List<gp_Pnt> { endPnt, pntConnectWithPath.Point }     // Lead In：from outside to start point
					: new List<gp_Pnt> { pntConnectWithPath.Point, endPnt };     // Lead Out： from start point to outside

				return CreateLeadCAMSegment( leadPoints, LeadToolVec );
			}
			else if( leadParam.Type == LeadLineType.Arc ) {
				LeadHelper.BuildArcLead(
					 pntConnectWithPath, isLeadin,
					leadParam.Length, leadParam.Angle,
					craftData.LeadLineParam.IsChangeLeadDirection, craftData.IsReverse,
					out gp_Pnt endPnt, out gp_Pnt midPnt, out _
				);
				var leadPoints = isLeadin
					? new List<gp_Pnt> { endPnt, midPnt, pntConnectWithPath.Point }    // Lead In
					: new List<gp_Pnt> { pntConnectWithPath.Point, midPnt, endPnt };   // Lead Out

				return CreateArcLeadCAMSegment( leadPoints, LeadToolVec );
			}
			return null;
		}

		static LineCAMSegment CreateLeadCAMSegment( List<gp_Pnt> points, gp_Dir toolVec )
		{
			List<CAMPoint2> camPointList = points.Select( p => new CAMPoint2( p, toolVec, toolVec, toolVec, toolVec ) ).ToList();
			return new LineCAMSegment( camPointList, 0, 0, 0 );
		}

		static ArcCAMSegment CreateArcLeadCAMSegment( List<gp_Pnt> points, gp_Dir toolVec )
		{
			List<CAMPoint2> camPointList = points.Select( p => new CAMPoint2( p, toolVec, toolVec, toolVec, toolVec ) ).ToList();
			return new ArcCAMSegment( camPointList, 0, 0, 0 );
		}
	}
}
