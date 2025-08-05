using OCC.BOPTools;
using OCC.BRep;
using OCC.BRepGProp;
using OCC.Geom;
using OCC.gp;
using OCC.GProp;
using OCC.TopAbs;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.CAM
{
	public class CADPoint
	{
		public CADPoint( gp_Pnt point, gp_Dir normalVec_1st, gp_Dir normalVec_2nd, gp_Dir tangentVec )
		{
			Point = point;
			NormalVec_1st = normalVec_1st;
			NormalVec_2nd = normalVec_2nd;
			TangentVec = tangentVec;
		}

		public gp_Pnt Point
		{
			get; private set;
		}

		public gp_Dir NormalVec_1st
		{
			get; private set;
		}

		// normal vector on co-face
		public gp_Dir NormalVec_2nd
		{
			get; private set;
		}

		// tangent vector on path
		public gp_Dir TangentVec
		{
			get; private set;
		}

		public CADPoint Clone()
		{
			return new CADPoint( Point, NormalVec_1st, NormalVec_2nd, TangentVec );
		}
	}

	// currently assuming CAM = CAD + ToolVec
	public class CAMPoint
	{
		public CAMPoint( CADPoint cadPoint, gp_Dir toolVec )
		{
			CADPoint = cadPoint;
			ToolVec = toolVec;
		}

		public CADPoint CADPoint
		{
			get; private set;
		}

		public gp_Dir ToolVec
		{
			get; private set;
		}

		public CAMPoint Clone()
		{
			return new CAMPoint( CADPoint.Clone(), new gp_Dir( ToolVec.XYZ() ) );
		}
	}

	public class CAMData
	{
		// CAD property
		//public CAMData( CADData cadData )
		//{
		//	CADData = cadData;

		//	// build raw data
		//	BuildCADPointList();
		//	BuildCAMPointList();
		//}

		//public CADData CADData
		//{
		//	get; private set;
		//}

		//public List<CADPoint> CADPointList
		//{
		//	get; private set;
		//}

		//// CAM property
		//public List<CAMPoint> CAMPointList
		//{
		//	get
		//	{
		//		if( m_IsDirty ) {
		//			BuildCAMPointList();
		//			m_IsDirty = false;
		//		}
		//		return m_CAMPointList;
		//	}
		//}

		//public bool IsReverse
		//{
		//	get
		//	{
		//		return m_IsReverse;
		//	}
		//	set
		//	{
		//		if( m_IsReverse != value ) {
		//			m_IsReverse = value;
		//			m_IsDirty = true;
		//		}
		//	}
		//}

		//public int StartPoint
		//{
		//	get
		//	{
		//		return m_StartPoint;
		//	}
		//	set
		//	{
		//		if( m_StartPoint != value ) {
		//			m_StartPoint = value;
		//			m_IsDirty = true;
		//		}
		//	}
		//}

		//public void SetToolVecModify( int index, double dRA_deg, double dRB_deg )
		//{
		//	if( m_ToolVecModifyMap.ContainsKey( index ) ) {
		//		m_ToolVecModifyMap[ index ] = new Tuple<double, double>( dRA_deg, dRB_deg );
		//	}
		//	else {
		//		m_ToolVecModifyMap.Add( index, new Tuple<double, double>( dRA_deg, dRB_deg ) );
		//	}
		//	m_IsDirty = true;
		//}

		//public void GetToolVecModify( int index, out double dRA_deg, out double dRB_deg )
		//{
		//	if( m_ToolVecModifyMap.ContainsKey( index ) ) {
		//		dRA_deg = m_ToolVecModifyMap[ index ].Item1;
		//		dRB_deg = m_ToolVecModifyMap[ index ].Item2;
		//	}
		//	else {
		//		dRA_deg = 0;
		//		dRB_deg = 0;
		//	}
		//}

		//public HashSet<int> GetToolVecModifyIndex()
		//{
		//	HashSet<int> result = new HashSet<int>();
		//	foreach( int nIndex in m_ToolVecModifyMap.Keys ) {
		//		result.Add( nIndex );
		//	}
		//	return result;
		//}

		//// view testing
		//public List<int> EdgeStartIndex
		//{
		//	get; private set;
		//}

		//// backing fields
		//List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		//Dictionary<int, Tuple<double, double>> m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>();
		//bool m_IsReverse = false;
		//int m_StartPoint = 0;

		//// dirty flag
		//bool m_IsDirty = false;

		//void BuildCADPointList()
		//{
		//	CADPointList = new List<CADPoint>();
		//	EdgeStartIndex = new List<int>();
		//	if( CADData == null ) {
		//		return;
		//	}

		//	// go through the contour edges
		//	for( int i = 0; i < CADData.PathDataList.Count; i++ ) {
		//		TopoDS_Edge edge = CADData.PathDataList[ i ].PathEdge;
		//		TopoDS_Face shellFace = CADData.PathDataList[ i ].ComponentFace;
		//		TopoDS_Face solidFace = CADData.PathDataList[ i ].ComponentFace; // TODO: set solid face
		//		gp_Trsf transform = CADData.InnerTrsf;

		//		// break the edge into segment points by interval
		//		const double dSegmentLength = 0.01;
		//		EdgeStartIndex.Add( CADPointList.Count );
		//		CADPointList.AddRange( GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), shellFace, solidFace, transform, dSegmentLength ) );
		//	}
		//}

		//List<CADPoint> GetEdgeSegmentPoints( TopoDS_Edge edge, TopoDS_Face shellFace, TopoDS_Face solidFace, gp_Trsf transform,
		//	double dSegmentLength )
		//{
		//	List<CADPoint> result = new List<CADPoint>();

		//	// get target edge length
		//	GProp_GProps system = new GProp_GProps();
		//	BRepGProp.LinearProperties( edge, ref system );
		//	double dEdgeLength = system.Mass();

		//	// get segment count
		//	int nSegments = (int)Math.Ceiling( dEdgeLength / dSegmentLength );

		//	// get curve parameters
		//	double dStartU = 0;
		//	double dEndU = 0;
		//	Geom_Curve oneGeomCurve = BRep_Tool.Curve( edge, ref dStartU, ref dEndU );

		//	// swap when reversed
		//	if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
		//		(dEndU, dStartU) = (dStartU, dEndU);
		//	}

		//	// get increment value
		//	double dIncrement = ( dEndU - dStartU ) / nSegments;

		//	// TODO: this is equal U but not equal length
		//	for( int i = 0; i < nSegments; i++ ) {
		//		double U = dStartU + dIncrement * i;

		//		// get point
		//		gp_Pnt point = oneGeomCurve.Value( U );

		//		// get shell normal (1st)
		//		gp_Dir normalVec_1st = new gp_Dir();
		//		BOPTools_AlgoTools3D.GetNormalToFaceOnEdge( edge, shellFace, U, ref normalVec_1st );
		//		if( shellFace.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
		//			normalVec_1st.Reverse();
		//		}

		//		// TODO: get solid normal (2nd)
		//		gp_Dir normalVec_2nd = new gp_Dir( normalVec_1st.XYZ() );

		//		// get tangent
		//		gp_Vec tangentVec = new gp_Vec();
		//		gp_Pnt _p = new gp_Pnt();
		//		oneGeomCurve.D1( U, ref _p, ref tangentVec );
		//		if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
		//			tangentVec.Reverse();
		//		}

		//		// apply the transformation
		//		point.Transform( transform );
		//		normalVec_1st.Transform( transform );
		//		normalVec_2nd.Transform( transform );
		//		tangentVec.Transform( transform );

		//		// build
		//		CADPoint cadPoint = new CADPoint( point, normalVec_1st, normalVec_2nd, new gp_Dir( tangentVec ) );
		//		result.Add( cadPoint );
		//	}
		//	return result;
		//}

		//void BuildCAMPointList()
		//{
		//	m_IsDirty = false;
		//	m_CAMPointList = new List<CAMPoint>();
		//	SetToolVec();
		//	SetStartPoint();
		//	SetOrientation();
		//}

		//void SetToolVec()
		//{
		//	for( int i = 0; i < CADPointList.Count; i++ ) {

		//		// calculate tool vector
		//		CADPoint cadPoint = CADPointList[ i ];
		//		CAMPoint camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st );
		//		m_CAMPointList.Add( camPoint );
		//	}
		//	ModifyToolVec();
		//}

		//void ModifyToolVec()
		//{
		//	if( m_ToolVecModifyMap.Count == 0 ) {
		//		return;
		//	}

		//	// sort the modify data by index
		//	List<int> indexInOrder = m_ToolVecModifyMap.Keys.ToList();
		//	indexInOrder.Sort();

		//	// modify the tool vector
		//	for( int i = 0; i < indexInOrder.Count; i++ ) {

		//		// get start and end index
		//		int nStartIndex = indexInOrder[ i ];
		//		int nEndIndex = indexInOrder[ ( i + 1 ) % indexInOrder.Count ];
		//		int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + m_CAMPointList.Count : nEndIndex;

		//		// do slerp from start to end
		//		gp_Vec startVec = GetVecFromAB( m_CAMPointList[ nStartIndex ],
		//			m_ToolVecModifyMap[ nStartIndex ].Item1 * Math.PI / 180,
		//			m_ToolVecModifyMap[ nStartIndex ].Item2 * Math.PI / 180 );
		//		gp_Vec endVec = GetVecFromAB( m_CAMPointList[ nEndIndex ],
		//			m_ToolVecModifyMap[ nEndIndex ].Item1 * Math.PI / 180,
		//			m_ToolVecModifyMap[ nEndIndex ].Item2 * Math.PI / 180 );
		//		gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
		//		gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
		//		m_CAMPointList[ nStartIndex ] = new CAMPoint( m_CAMPointList[ nStartIndex ].CADPoint, new gp_Dir( startVec ) );
		//		for( int j = nStartIndex + 1; j < nEndIndexModify; j++ ) {
		//			double t = ( j - nStartIndex ) / (double)( nEndIndexModify - nStartIndex );
		//			gp_Quaternion q = new gp_Quaternion();
		//			slerp.Interpolate( t, ref q );
		//			gp_Trsf trsf = new gp_Trsf();
		//			trsf.SetRotation( q );
		//			gp_Dir toolVec = new gp_Dir( startVec.Transformed( trsf ) );
		//			m_CAMPointList[ j % m_CAMPointList.Count ] = new CAMPoint( m_CAMPointList[ j % m_CAMPointList.Count ].CADPoint, toolVec );
		//		}
		//	}
		//}

		//gp_Vec GetVecFromAB( CAMPoint camPoint, double dRA_rad, double dRB_rad )
		//{
		//	// TDOD: RA == 0 || RB == 0
		//	if( dRA_rad == 0 && dRB_rad == 0 ) {
		//		return new gp_Vec( camPoint.ToolVec );
		//	}

		//	// get the x, y, z direction
		//	gp_Dir x = camPoint.CADPoint.TangentVec;
		//	gp_Dir z = camPoint.CADPoint.NormalVec_1st;
		//	gp_Dir y = z.Crossed( x );

		//	// X:Y:Z = tanA:tanB:1
		//	double X = 0;
		//	double Y = 0;
		//	double Z = 0;
		//	if( dRA_rad == 0 ) {
		//		X = 0;
		//		Z = 1;
		//	}
		//	else {
		//		X = dRA_rad < 0 ? -1 : 1;
		//		Z = X / Math.Tan( dRA_rad );
		//	}
		//	Y = Z * Math.Tan( dRB_rad );
		//	gp_Dir dir1 = new gp_Dir( x.XYZ() * X + y.XYZ() * Y + z.XYZ() * Z );
		//	return new gp_Vec( dir1.XYZ() );
		//}

		//void SetStartPoint()
		//{
		//	// rearrange cam points to start from the strt index
		//	if( StartPoint != 0 ) {
		//		List<CAMPoint> newCAMPointList = new List<CAMPoint>();
		//		for( int i = 0; i < m_CAMPointList.Count; i++ ) {
		//			newCAMPointList.Add( m_CAMPointList[ ( i + StartPoint ) % m_CAMPointList.Count ] );
		//		}
		//		m_CAMPointList = newCAMPointList;
		//	}
		//}

		//void SetOrientation()
		//{
		//	// reverse the cad points if is reverse
		//	if( IsReverse ) {
		//		m_CAMPointList.Reverse();

		//		// modify index
		//		CAMPoint lastPoint = m_CAMPointList.Last();
		//		m_CAMPointList.Remove( lastPoint );
		//		m_CAMPointList.Insert( 0, lastPoint );
		//	}
		//}
	}
}
