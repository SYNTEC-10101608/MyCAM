using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.BRepAdaptor;
using OCC.GC;
using OCC.GCPnts;
using OCC.Geom;
using OCC.GeomAdaptor;
using OCC.gp;
using OCCTool;

namespace MyCAM.Helper
{
	internal static class LeadHelper
	{
		public static List<ICAMSegment> BuildLeadCAMSegment( CraftData craftData, ICAMSegment CAMSegConnectWithStartPnt, bool isLeadIn )
		{
			List<ICAMSegment> LeadCADSegment = new List<ICAMSegment>();
			if( isLeadIn ) {
				if( craftData.LeadLineParam.LeadIn.Type != LeadLineType.None ) {
					ICAMSegment leadInCAMSegment = BuildLeadSegment( craftData, CAMSegConnectWithStartPnt, isLeadIn );
					LeadCADSegment.Add( leadInCAMSegment );
					return LeadCADSegment;
				}
			}
			if( craftData.LeadLineParam.LeadOut.Type != LeadLineType.None ) {
				ICAMSegment leadInCAMSegment = BuildLeadSegment( craftData, CAMSegConnectWithStartPnt, isLeadIn );
				LeadCADSegment.Add( leadInCAMSegment );
				return LeadCADSegment;
			}
			return LeadCADSegment;
		}

		public static Geom_Curve BuildLeadGeom( bool isLeadIn, LeadLineType leadLineType, CAMPoint2 pntConnectWithLead, double dLeadLineLength, double dLeadLineAngle, bool isChangeLeadDirection, bool isReverse, out gp_Pnt leadEndPnt, out gp_Pnt leadMidPnt, out gp_Dir leadDir )
		{
			if( isLeadIn ) {
				return BuildLeadGeomByParam( true, leadLineType, pntConnectWithLead, dLeadLineLength, dLeadLineAngle, isChangeLeadDirection, isReverse, out leadEndPnt, out leadMidPnt, out leadDir );
			}
			return BuildLeadGeomByParam( false, leadLineType, pntConnectWithLead, dLeadLineLength, dLeadLineAngle, isChangeLeadDirection, isReverse, out leadEndPnt, out leadMidPnt, out leadDir );
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

		static ICAMSegment BuildLeadSegment( CraftData craftData, ICAMSegment CAMSegConnectWithLead, bool isLeadin )
		{
			CAMPoint2 pntConnectWithPath = isLeadin ? CAMSegConnectWithLead.StartPoint : CAMSegConnectWithLead.EndPoint;
			gp_Dir LeadToolVec = pntConnectWithPath.ToolVec;
			LeadParam leadParam = isLeadin ? craftData.LeadLineParam.LeadIn : craftData.LeadLineParam.LeadOut;

			// draw arc / line lead
			if( leadParam.Type == LeadLineType.Line ) {
				BuildStraightLead(
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

		static LineCAMSegment CreateLeadCAMSegment( List<gp_Pnt> points, gp_Dir toolVec, double maxSegmentLength = 1.0 )
		{
			if( points == null || points.Count < 2 ) {
				return null;
			}
			gp_Pnt startPoint = points.First();
			gp_Pnt endPoint = points.Last();
			List<CAMPoint2> camPointList = CreateEqualLengthDiscretizedPoints( startPoint, endPoint, toolVec, maxSegmentLength,
				out double totalLength, out double eachSegmentLength );

			if( camPointList == null || camPointList.Count < 2 ) {
				camPointList = points.Select( p => new CAMPoint2( p, toolVec, toolVec, toolVec, toolVec ) ).ToList();
				totalLength = startPoint.Distance( endPoint );
				eachSegmentLength = totalLength;
			}
			return new LineCAMSegment( camPointList, totalLength, eachSegmentLength, eachSegmentLength );
		}

		static ArcCAMSegment CreateArcLeadCAMSegment( List<gp_Pnt> points, gp_Dir toolVec, double maxSegmentLength = 1.0 )
		{
			if( points == null || points.Count != 3 ) {
				List<CAMPoint2> camPointList = points?.Select( p => new CAMPoint2( p, toolVec, toolVec, toolVec, toolVec ) ).ToList();
				return new ArcCAMSegment( camPointList ?? new List<CAMPoint2>(), 0, 0, 0 );
			}

			gp_Pnt startPoint = points[ 0 ];
			gp_Pnt midPoint = points[ 1 ];
			gp_Pnt endPoint = points[ 2 ];

			List<CAMPoint2> discretizedPoints = CreateArcEqualLengthDiscretizedPoints( startPoint, midPoint, endPoint,
				toolVec, maxSegmentLength, out double totalLength, out double eachArcLength, out double eachChordLength );

			if( discretizedPoints == null || discretizedPoints.Count < 2 ) {
				discretizedPoints = points.Select( p => new CAMPoint2( p, toolVec, toolVec, toolVec, toolVec ) ).ToList();
				totalLength = 0;
				eachArcLength = 0;
				eachChordLength = 0;
			}

			return new ArcCAMSegment( discretizedPoints, totalLength, eachArcLength, eachChordLength );
		}

		static List<CAMPoint2> CreateEqualLengthDiscretizedPoints( gp_Pnt startPoint, gp_Pnt endPoint, gp_Dir toolVec,
		double maxSegmentLength, out double totalLength, out double eachSegmentLength )
		{
			totalLength = 0;
			eachSegmentLength = 0;

			try {
				totalLength = startPoint.Distance( endPoint );
				if( totalLength <= 0 ) {
					return null;
				}

				int segmentCount = (int)Math.Ceiling( totalLength / maxSegmentLength );
				if( segmentCount <= 0 ) {
					segmentCount = 1;
				}

				eachSegmentLength = totalLength / segmentCount;
				gp_Vec lineVec = new gp_Vec( startPoint, endPoint );
				lineVec.Normalize();
				gp_Dir tangentDir = new gp_Dir( lineVec );

				// doesn't matter
				gp_Dir normalVec = toolVec;

				List<CAMPoint2> camPointList = new List<CAMPoint2>();
				for( int i = 0; i <= segmentCount; i++ ) {
					double t = (double)i / segmentCount;
					gp_Pnt currentPoint = new gp_Pnt(
						startPoint.X() + t * ( endPoint.X() - startPoint.X() ),
						startPoint.Y() + t * ( endPoint.Y() - startPoint.Y() ),
						startPoint.Z() + t * ( endPoint.Z() - startPoint.Z() )
					);

					CAMPoint2 camPoint = new CAMPoint2( currentPoint, normalVec, normalVec, tangentDir, toolVec );
					camPointList.Add( camPoint );
				}
				return camPointList;
			}
			catch( Exception ) {
				return null;
			}
		}

		static List<CAMPoint2> CreateArcEqualLengthDiscretizedPoints( gp_Pnt startPoint, gp_Pnt midPoint, gp_Pnt endPoint,
	gp_Dir toolVec, double maxSegmentLength, out double totalLength, out double eachArcLength, out double eachChordLength )
		{
			totalLength = 0;
			eachArcLength = 0;
			eachChordLength = 0;

			try {

				// 3 points to arc
				GC_MakeArcOfCircle makeCircle = new GC_MakeArcOfCircle( startPoint, midPoint, endPoint );
				if( !makeCircle.IsDone() ) {
					return null;
				}
				Geom_TrimmedCurve arcCurve = makeCircle.Value();
				if( arcCurve == null || arcCurve.IsNull() ) {
					return null;
				}
				GeomAdaptor_Curve adaptorCurve = new GeomAdaptor_Curve( arcCurve );
				double dStartU = adaptorCurve.FirstParameter();
				double dEndU = adaptorCurve.LastParameter();
				totalLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dStartU, dEndU );
				if( totalLength <= 0 ) {
					return null;
				}
				List<double> paramList = CADDiscreteHelper.DiscretizeArcOrLineByLength(
					dStartU, dEndU, maxSegmentLength, totalLength, out eachArcLength );
				if( paramList == null || paramList.Count < 2 ) {
					return null;
				}

				// doesn't matter
				gp_Dir uselessVec = toolVec;
				List<CAMPoint2> camPointList = new List<CAMPoint2>();

				foreach( double u in paramList ) {
					gp_Pnt point = adaptorCurve.Value( u );
					CAMPoint2 camPoint = new CAMPoint2( point, uselessVec, uselessVec, uselessVec, toolVec );
					camPointList.Add( camPoint );
				}

				if( camPointList.Count >= 2 ) {
					eachChordLength = camPointList[ 0 ].Point.Distance( camPointList[ 1 ].Point );
				}

				return camPointList;
			}
			catch( Exception ) {
				return null;
			}
		}
	}
}
