using MyCAM.Data;
using OCC.GCPnts;
using OCC.Geom;
using OCC.GeomAdaptor;
using OCC.gp;
using OCC.Precision;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	public static class LeadHelper
	{
		public static void SetLeadIn( IReadOnlyList<IOrientationPoint> mainPointList,
			out List<IOrientationPoint> leadInPointList,
			LeadData leadInData, bool isPathReverse,
			double maxDeflection = DISCRETE_MAX_DEFLECTION, double maxEdgeLength = DISCRETE_MAX_EDGE_LENGTH )
		{
			leadInPointList = new List<IOrientationPoint>();
			if( mainPointList.Count == 0 ) {
				return;
			}
			LeadParam leadInParam = leadInData.LeadIn;
			bool isChangeLeadDirection = leadInData.IsChangeLeadDirection;
			switch( leadInParam.Type ) {
				case LeadLineType.Line:
					leadInPointList = BuildStraightLeadLine( mainPointList[ 0 ], true, leadInParam.Length, leadInParam.Angle, isChangeLeadDirection, isPathReverse );
					break;
				case LeadLineType.Arc:
					leadInPointList = BuildArcLeadLine( mainPointList[ 0 ], true, leadInParam.Length, leadInParam.Angle, isChangeLeadDirection, isPathReverse, maxDeflection, maxEdgeLength );
					break;
				default:
					break;
			}
		}

		public static void SetLeadOut( IReadOnlyList<IOrientationPoint> mainPointList, IReadOnlyList<IOrientationPoint> overCutPointList,
			out List<IOrientationPoint> leadOutPointList,
			LeadData leadOutData, bool isPathReverse,
			double maxDeflection = DISCRETE_MAX_DEFLECTION, double maxEdgeLength = DISCRETE_MAX_EDGE_LENGTH )
		{
			leadOutPointList = new List<IOrientationPoint>();
			if( mainPointList.Count == 0 ) {
				return;
			}

			// with over cut means lead out first point is over cut last point
			IOrientationPoint leadOutStartPoint;
			if( overCutPointList.Count > 0 ) {
				leadOutStartPoint = overCutPointList[ overCutPointList.Count - 1 ];
			}
			else {
				leadOutStartPoint = mainPointList[ mainPointList.Count - 1 ];
			}
			LeadParam leadOutParam = leadOutData.LeadOut;
			bool isChangeLeadDirection = leadOutData.IsChangeLeadDirection;
			switch( leadOutParam.Type ) {
				case LeadLineType.Line:
					leadOutPointList = BuildStraightLeadLine( leadOutStartPoint, false, leadOutParam.Length, leadOutParam.Angle, isChangeLeadDirection, isPathReverse );
					break;
				case LeadLineType.Arc:
					leadOutPointList = BuildArcLeadLine( leadOutStartPoint, false, leadOutParam.Length, leadOutParam.Angle, isChangeLeadDirection, isPathReverse, maxDeflection, maxEdgeLength );
					break;
				default:
					break;
			}
		}

		// when isLeadIn, the input point is start of path, and is end of lead in
		static List<IOrientationPoint> BuildStraightLeadLine( IOrientationPoint StraightLeadStartOrEndPoint, bool isLeadIn, double dLeadLineLength, double dLeadLineAngle, bool isChangeLeadDirection, bool isReverse )
		{
			List<IOrientationPoint> LeadPointList = new List<IOrientationPoint>();

			// protection 
			if( StraightLeadStartOrEndPoint == null || dLeadLineLength == 0 ) {
				return LeadPointList;
			}

			//  establish a coordinate system through this point and its direction vector and normal vector
			IOrientationPoint startPoint = StraightLeadStartOrEndPoint;
			gp_Dir XVec = new gp_Dir( -startPoint.TangentVec.X(), -startPoint.TangentVec.Y(), -startPoint.TangentVec.Z() );
			gp_Dir ZVec = startPoint.ToolVec;
			gp_Ax3 planeCS = new gp_Ax3( startPoint.Point, ZVec, XVec );

			gp_Dir xDir = planeCS.XDirection();
			gp_Dir yDir = planeCS.YDirection();
			double angleRad = dLeadLineAngle * Math.PI / 180.0;
			double cosA = Math.Cos( angleRad );
			double sinA = Math.Sin( angleRad );

			// vector to remove from original point to the lead line end point
			gp_Vec dirVec2D = new gp_Vec( xDir ).Multiplied( cosA ) + new gp_Vec( yDir ).Multiplied( sinA );
			gp_Pnt leadLineEndPoint = startPoint.Point.Translated( dirVec2D.Multiplied( dLeadLineLength ) );

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
			gp_Dir endPointVector;

			// path new point -> start point
			if( isLeadIn ) {
				endPointVector = new gp_Dir( new gp_Vec( leadLineEndPoint, startPoint.Point ) );
			}

			// path end point -> new point
			else {
				endPointVector = new gp_Dir( new gp_Vec( startPoint.Point, leadLineEndPoint ) );
			}

			// split lead line to several points
			List<gp_Pnt> leadPointList = Get2PntSegmentPnt( leadLineEndPoint, startPoint.Point, 1 );


			for( int i = 0; i < leadPointList.Count; i++ ) {
				IOrientationPoint leadPoint = BuildLeadLinePoint( leadPointList[ i ], endPointVector, startPoint.ToolVec );
				LeadPointList.Add( leadPoint );
			}

			// lead out start point is path end point
			if( isLeadIn == false ) {
				LeadPointList.Reverse();
			}
			return LeadPointList;
		}

		// when isLeadIn, the input point is start of path, and is end of lead in
		static List<IOrientationPoint> BuildArcLeadLine( IOrientationPoint CurveLeadStartOrEndPoint, bool isLeadIn, double dLeadLineLength, double dLeadLineAngle, bool isChangeLeadDirection, bool isReverse, double dDeflection, double dMaxLength )
		{
			// protection
			if( CurveLeadStartOrEndPoint == null || dLeadLineLength <= 0 || dLeadLineAngle <= 0 ) {
				return new List<IOrientationPoint>();
			}

			//  establish a coordinate system through this point and its direction vector and normal vector
			gp_Dir XVec = CurveLeadStartOrEndPoint.TangentVec;
			gp_Dir ZVec = CurveLeadStartOrEndPoint.ToolVec;
			gp_Ax3 leadLinePlane = new gp_Ax3( CurveLeadStartOrEndPoint.Point, ZVec, XVec );

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

			// split the curve into points
			GetCurveSegmentPointsWithTangent( leadLineCurve, circleCenterPnt, ZVec, out List<gp_Pnt> pts, out List<gp_Dir> tangentDirList, dDeflection, dMaxLength );

			// because all cuver start at the point which lead connect with main path, but that should be last point
			pts.Reverse();
			tangentDirList.Reverse();

			// mirror by y axis
			if( isReverse ) {
				gp_Ax1 mirrorAxisY = new gp_Ax1( leadLinePlane.Location(), leadLinePlane.YDirection() );
				gp_Trsf mirrorTrsf = new gp_Trsf();
				mirrorTrsf.SetMirror( mirrorAxisY );
				for( int i = 0; i < pts.Count; i++ ) {

					// mirror point
					pts[ i ].Transform( mirrorTrsf );

					// mirror tangent vec
					tangentDirList[ i ] = new gp_Dir( new gp_Vec( tangentDirList[ i ] ).Transformed( mirrorTrsf ) );
				}
			}

			// mirror by x axis
			if( isChangeLeadDirection ) {
				gp_Ax1 mirrorAxisX = new gp_Ax1( leadLinePlane.Location(), leadLinePlane.XDirection() );
				gp_Trsf mirrorTrsf = new gp_Trsf();
				mirrorTrsf.SetMirror( mirrorAxisX );
				for( int i = 0; i < pts.Count; i++ ) {
					pts[ i ].Transform( mirrorTrsf );
					tangentDirList[ i ] = new gp_Dir( new gp_Vec( tangentDirList[ i ] ).Transformed( mirrorTrsf ) );
				}
			}

			// create cad cam data
			List<IOrientationPoint> resultList = new List<IOrientationPoint>();
			for( int i = 0; i < pts.Count; i++ ) {

				IOrientationPoint leadPoint = BuildLeadLinePoint( pts[ i ], tangentDirList[ i ], CurveLeadStartOrEndPoint.ToolVec );
				resultList.Add( leadPoint );
			}
			return resultList;
		}

		static List<gp_Pnt> Get2PntSegmentPnt( gp_Pnt currentPnt, gp_Pnt nextPnt, double stepLength )
		{
			List<gp_Pnt> pntList = new List<gp_Pnt>();
			gp_Vec dirVec = new gp_Vec( currentPnt, nextPnt );
			double totalLength = dirVec.Magnitude();

			// too short
			if( totalLength < Precision.Confusion() ) {
				pntList.Add( new gp_Pnt( currentPnt.X(), currentPnt.Y(), currentPnt.Z() ) );
				return pntList;
			}
			gp_Dir dir = new gp_Dir( dirVec );
			int nTotalSegmentCount = (int)Math.Floor( totalLength / stepLength );

			// firt point
			pntList.Add( new gp_Pnt( currentPnt.X(), currentPnt.Y(), currentPnt.Z() ) );

			// split each segment
			for( int i = 1; i < nTotalSegmentCount; i++ ) {
				gp_Pnt pi = new gp_Pnt(
					currentPnt.X() + dir.X() * stepLength * i,
					currentPnt.Y() + dir.Y() * stepLength * i,
					currentPnt.Z() + dir.Z() * stepLength * i
				);
				pntList.Add( pi );
			}
			pntList.Add( new gp_Pnt( nextPnt.X(), nextPnt.Y(), nextPnt.Z() ) );
			return pntList;
		}

		static void GetCurveSegmentPointsWithTangent( Geom_TrimmedCurve curve, gp_Pnt circleCenter, gp_Dir circleNormal, out List<gp_Pnt> pointList, out List<gp_Dir> tangentList, double dDeflection, double dMaxLength )
		{
			pointList = new List<gp_Pnt>();
			tangentList = new List<gp_Dir>();
			if( curve == null || dDeflection <= 0 || dMaxLength <= 0 ) {
				return;
			}
			double first = curve.FirstParameter();
			double last = curve.LastParameter();

			// wrap the curve with an adaptor to calculate arc length
			GeomAdaptor_Curve gac = new GeomAdaptor_Curve( curve, first, last );
			double totalLength = GCPnts_AbscissaPoint.Length( gac, first, last, 1.0e-7 );
			double radius = circleCenter.Distance( curve.Value( first ) );

			// calculate the length of each arc segment based on the chord height tolerance
			double dArcMax = 2.0 * Math.Acos( Math.Max( 0.0, 1.0 - dDeflection / radius ) );
			double dStepLength = radius * dArcMax;

			// ensure dStepLength not greater than maxStepLength
			dStepLength = Math.Min( dStepLength, dMaxLength );
			int nTotalSegmentCount = (int)Math.Ceiling( totalLength / dStepLength );

			// start point
			gp_Pnt startpoint = new gp_Pnt();
			gp_Vec startPointTangentVec = new gp_Vec();
			curve.D1( first, ref startpoint, ref startPointTangentVec );
			pointList.Add( startpoint );

			// right-hand rule will be counterclockwise, so it needs to be turned clockwise.
			gp_Dir tangentDir = new gp_Dir( startPointTangentVec );
			tangentDir.Reverse();
			tangentList.Add( tangentDir );

			// each point
			for( int i = 1; i <= nTotalSegmentCount - 1; i++ ) {
				double targetLen = i * dStepLength;

				// arc length exceeds the total length of the curve, set as the end point of the curve
				if( targetLen > totalLength ) {
					targetLen = totalLength;
				}

				// according to the target arc length to get u param
				GCPnts_AbscissaPoint arcParam = new GCPnts_AbscissaPoint( gac, targetLen, first, 1.0e-7, 1000 );
				if( !arcParam.IsDone() )
					break;
				double dCurveParamU = arcParam.Parameter();

				// get point
				gp_Pnt middlePoint = new gp_Pnt();
				gp_Vec middlePointTangentVec = new gp_Vec();
				curve.D1( dCurveParamU, ref middlePoint, ref middlePointTangentVec );

				// avoid overlap point
				if( pointList.Count == 0 || middlePoint.Distance( pointList[ pointList.Count - 1 ] ) > 1.0e-9 ) {
					pointList.Add( middlePoint );
					gp_Dir middliePointTangentDir = new gp_Dir( middlePointTangentVec );
					middliePointTangentDir.Reverse();
					tangentList.Add( middliePointTangentDir );
				}
			}

			// end point
			gp_Pnt pEnd = new gp_Pnt();
			gp_Vec vEnd = new gp_Vec();
			curve.D1( last, ref pEnd, ref vEnd );
			if( pointList.Count == 0 || pEnd.Distance( pointList[ pointList.Count - 1 ] ) > 1.0e-9 ) {
				pointList.Add( pEnd );
				tangentDir = new gp_Dir( vEnd );
				tangentDir.Reverse();
				tangentList.Add( tangentDir );
			}
		}

		// this method is now for building a CAMPoint as IOrientationPoint
		static IOrientationPoint BuildLeadLinePoint( gp_Pnt point, gp_Dir tangentVec, gp_Dir toolVec )
		{
			CADPoint leadPoint = new CADPoint(
				point,
				toolVec, // does not matter
				toolVec, // does not matter
				tangentVec
				);
			CAMPoint leadCAMPoint = new CAMPoint( leadPoint, toolVec );
			return leadCAMPoint;
		}

		const double DISCRETE_MAX_DEFLECTION = 0.01;
		const double DISCRETE_MAX_EDGE_LENGTH = 1.0;
	}
}
