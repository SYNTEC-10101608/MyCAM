using MyCAM.Data;
using OCC.gp;
using OCC.TopoDS;
using System;

namespace MyCAM.StandardPatternFactory
{
	public class PatternFactory
	{
		public PatternFactory( ICenterAvgDir standardPatternInfo, IStdPatternGeomData standardPatternGeomData )
		{
			if( standardPatternInfo == null ) {
				throw new ArgumentNullException( "PatternFactory constructing argument null" );
			}
			m_CenterPoint = standardPatternInfo.CenterPnt;
			m_NormalDir = standardPatternInfo.AverageNormalDir;
			m_StandardPatternGeomData = standardPatternGeomData;

			// get rotation angle from IRotatable interface if the geometry supports rotation
			double rotatedAngleInDegrees = 0.0;
			if( standardPatternGeomData is IRotatable rotatableGeom ) {
				rotatedAngleInDegrees = rotatableGeom.RotatedAngle_deg;
			}
			GetLocalCoordination( rotatedAngleInDegrees, out m_Coordination );

			if( CreateStandardPatternWire( out TopoDS_Wire wire ) ) {
				m_ShapeWire = wire;
			}
		}

		public TopoDS_Shape GetShape()
		{
			return m_ShapeWire;
		}

		public gp_Ax3 GetCoordinateInfo()
		{
			return m_Coordination;
		}

		void GetLocalCoordination( double rotationAngleInDegrees, out gp_Ax3 coordination )
		{
			coordination = null;
			gp_Dir xDir = null;
			gp_Dir yDir = null;

			// build local coordination system ( reference DOC:https://syntecclub.atlassian.net/wiki/spaces/AUTO/pages/89458700 )
			if( m_NormalDir.IsParallel( new gp_Dir( 0, 0, 1 ), TOLERANCE ) && !m_NormalDir.IsOpposite( new gp_Dir( 0, 0, 1 ), TOLERANCE ) ) {
				xDir = new gp_Dir( 1, 0, 0 );
			}
			else if( m_NormalDir.IsParallel( new gp_Dir( 0, 0, 1 ), TOLERANCE ) && m_NormalDir.IsOpposite( new gp_Dir( 0, 0, 1 ), TOLERANCE ) ) {
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 1, 0 ) ), 180 );
				gp_Dir refDir = new gp_Dir( 1, 0, 0 );
				xDir = refDir.Transformed( trsf );
			}
			else {

				// general case
				gp_Dir refDir = new gp_Dir( 0, 0, 1 );
				xDir = refDir.Crossed( m_NormalDir );
			}
			yDir = m_NormalDir.Crossed( xDir );

			// create coordination system considering rotation
			gp_Ax3 coordSystem = new gp_Ax3( m_CenterPoint, m_NormalDir, xDir );
			if( Math.Abs( rotationAngleInDegrees ) > 1e-9 ) {
				double rotationAngleInRadians = rotationAngleInDegrees * Math.PI / 180.0;
				gp_Ax1 rotationAxis = new gp_Ax1( m_CenterPoint, m_NormalDir );
				coordSystem.Rotate( rotationAxis, rotationAngleInRadians );
			}
			xDir = coordSystem.XDirection();
			yDir = coordSystem.YDirection();
			coordination = new gp_Ax3( m_CenterPoint, m_NormalDir, xDir );
		}

		bool CreateStandardPatternWire( out TopoDS_Wire wire )
		{
			gp_Ax3 ax3 = m_Coordination;
			gp_Pln plane = new gp_Pln( ax3 );
			wire = null;

			// get appropriate strategy for the geometry type
			IWireCreationStrategy strategy = WireCreationStrategyFactory.GetStrategy( m_StandardPatternGeomData.PathType );
			if( strategy == null ) {

				// pathType not supported by strategies (e.g., Contour)
				return false;
			}

			// use strategy to create wire
			return strategy.CreateWire( m_CenterPoint, plane, m_StandardPatternGeomData, out wire );
		}

		IStdPatternGeomData m_StandardPatternGeomData;
		TopoDS_Wire m_ShapeWire;
		gp_Pnt m_CenterPoint;
		gp_Dir m_NormalDir;
		gp_Ax3 m_Coordination;
		const double TOLERANCE = 0.001;
	}
}
