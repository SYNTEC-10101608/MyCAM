using System.Collections.Generic;
using MyCAM.App;
using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Helper
{
	// build per-frame component transforms for a specific machine product
	public interface IPerFrameTransformBuilder
	{
		// simulated component order; index in this list is the buffer index order
		IReadOnlyList<MachineComponentType> ComponentTypes
		{
			get;
		}

		// component type -> buffer index; returns -1 when not in the simulated list ( e.g. Base )
		int IndexOf( MachineComponentType type );

		// fill each component local transform of a single frame into outBuf
		void BuildFrameTransforms( PostPointBase framePoint, gp_Vec g54,
			HashSet<MachineComponentType> workPiecesChainSet, gp_Trsf[] outBuf );
	}

	// shared component order lookup, driven by machineData.ComponentTypes
	public abstract class PerFrameTransformBuilderBase : IPerFrameTransformBuilder
	{
		protected PerFrameTransformBuilderBase( IMachineDataBase machineData )
		{
			m_MachineDataBase = machineData;
		}

		public IReadOnlyList<MachineComponentType> ComponentTypes
		{
			get
			{
				return m_MachineDataBase.ComponentTypes;
			}
		}

		// component type -> buffer index; returns -1 when not in the simulated list ( e.g. Base )
		public int IndexOf( MachineComponentType type )
		{
			IReadOnlyList<MachineComponentType> list = m_MachineDataBase.ComponentTypes;
			for( int i = 0; i < list.Count; i++ ) {
				if( list[ i ] == type ) {
					return i;
				}
			}
			return -1;
		}

		public abstract void BuildFrameTransforms( PostPointBase framePoint, gp_Vec g54,
			HashSet<MachineComponentType> workPiecesChainSet, gp_Trsf[] outBuf );

		// translation transform along axes, optionally inverted for workpiece-carrying chain
		protected static gp_Trsf CreateAxisTranslation( double x, double y, double z, bool bNeedInvert )
		{
			gp_Trsf transform = new gp_Trsf();
			transform.SetTranslation( new gp_Vec( x, y, z ) );
			if( bNeedInvert ) {
				transform.Invert();
			}
			return transform;
		}

		// rotation transform about an axis, optionally inverted for workpiece-carrying chain
		protected static gp_Trsf CreateAxisRotation( gp_Pnt RotateCenterPnt, gp_Dir dir, double dAngle, bool bNeedInvert )
		{
			gp_Trsf transform = new gp_Trsf();
			gp_Ax1 rotateAxis = new gp_Ax1( RotateCenterPnt, dir );
			transform.SetRotation( rotateAxis, dAngle );
			if( bNeedInvert ) {
				transform.Invert();
			}
			return transform;
		}

		readonly IMachineDataBase m_MachineDataBase;
		protected static readonly LanguageManager.LanguageManager m_LangMgr = new LanguageManager.LanguageManager( "PerFrameTransformBuilder" );
		protected const string FRAME_CALC_FAILED = "模擬計算失敗";
	}

	// five-axis per-frame transform builder ( logic moved from SimulationHelper )
	public class FiveAxisPerFrameTransformBuilder : PerFrameTransformBuilderBase
	{
		public FiveAxisPerFrameTransformBuilder( FiveAxisMachineData machineData )
			: base( machineData )
		{
			m_MachineData = machineData;
		}

		public override void BuildFrameTransforms( PostPointBase framePoint, gp_Vec g54,
			HashSet<MachineComponentType> workPiecesChainSet, gp_Trsf[] outBuf )
		{
			if( !( framePoint is PostPoint fiveAxisPoint ) || m_MachineData == null ) {
				MyApp.Logger.ShowOnLogPanel( m_LangMgr.TryGetString( "FrameCalcFailed", FRAME_CALC_FAILED ), MyApp.NoticeType.Warning );
				for( int i = 0; i < outBuf.Length; i++ ) {
					outBuf[ i ] = new gp_Trsf();
				}
				return;
			}
			if( g54 == null ) {
				g54 = new gp_Vec( 0, 0, 0 );
			}

			// XYZ axis
			SetBuf( ref outBuf, MachineComponentType.XAxis, CreateAxisTranslation( fiveAxisPoint.X + g54.X(), 0, 0, workPiecesChainSet.Contains( MachineComponentType.XAxis ) ) );
			SetBuf( ref outBuf, MachineComponentType.YAxis, CreateAxisTranslation( 0, fiveAxisPoint.Y + g54.Y(), 0, workPiecesChainSet.Contains( MachineComponentType.YAxis ) ) );
			SetBuf( ref outBuf, MachineComponentType.ZAxis, CreateAxisTranslation( 0, 0, fiveAxisPoint.Z + g54.Z(), workPiecesChainSet.Contains( MachineComponentType.ZAxis ) ) );

			// rotational axis
			SetBuf( ref outBuf, MachineComponentType.Master, CreateAxisRotation( m_MachineData.MasterAxis.RotateCenter, m_MachineData.MasterAxis.RotateDir, fiveAxisPoint.Master, workPiecesChainSet.Contains( MachineComponentType.Master ) ) );
			SetBuf( ref outBuf, MachineComponentType.Slave, CreateAxisRotation( m_MachineData.SlaveAxis.RotateCenter, m_MachineData.SlaveAxis.RotateDir, fiveAxisPoint.Slave, workPiecesChainSet.Contains( MachineComponentType.Slave ) ) );

			// laser / workpiece
			SetBuf( ref outBuf, MachineComponentType.Laser, new gp_Trsf() );
			gp_Trsf trsfWorkPiece = new gp_Trsf();
			trsfWorkPiece.SetTranslation( g54 );
			SetBuf( ref outBuf, MachineComponentType.WorkPiece, trsfWorkPiece );
		}

		// set outBuf safely; skip when the component is not in the simulated list ( IndexOf returns -1 )
		void SetBuf( ref gp_Trsf[] outBuf, MachineComponentType type, gp_Trsf trsf )
		{
			int idx = IndexOf( type );
			if( idx < 0 || idx >= outBuf.Length ) {
				return;
			}
			outBuf[ idx ] = trsf;
		}

		readonly FiveAxisMachineData m_MachineData;
	}

	// articulated robot per-frame transform builder ( skeleton, FK filled in later step )
	public class ArticRobotPerFrameTransformBuilder : PerFrameTransformBuilderBase
	{
		public ArticRobotPerFrameTransformBuilder( ArticRobotMachineData machineData )
			: base( machineData )
		{
			m_MachineData = machineData;
		}

		public override void BuildFrameTransforms( PostPointBase framePoint, gp_Vec g54,
			HashSet<MachineComponentType> workPiecesChainSet, gp_Trsf[] outBuf )
		{
			if( !( framePoint is ArticRobotPostPoint armPoint ) || m_MachineData == null ) {
				MyApp.Logger.ShowOnLogPanel( m_LangMgr.TryGetString( "FrameCalcFailed", FRAME_CALC_FAILED ), MyApp.NoticeType.Warning );
				for( int i = 0; i < outBuf.Length; i++ ) {
					outBuf[ i ] = new gp_Trsf();
				}
				return;
			}
			if( g54 == null ) {
				g54 = new gp_Vec( 0, 0, 0 );
			}

			// each joint local rotation about its own axis; chain accumulation handled by caller
			IReadOnlyList<ArticRobotComponentInfo> joints = m_MachineData.Joints;
			for( int i = 0; i < joints.Count; i++ ) {
				int idx = IndexOf( joints[ i ].ComponentType );
				if( idx < 0 ) {
					continue;
				}
				RotateAxisDef axisDef = joints[ i ].AxisDef as RotateAxisDef;
				if( axisDef == null ) {
					outBuf[ idx ] = new gp_Trsf();
					continue;
				}
				double jointAngle = GetJointAngle( armPoint, i );
				outBuf[ idx ] = CreateAxisRotation( axisDef.RotateCenter, axisDef.RotateDir, jointAngle, workPiecesChainSet.Contains( joints[ i ].ComponentType ) );
			}

			// workpiece
			int workPieceIdx = IndexOf( MachineComponentType.WorkPiece );
			if( workPieceIdx >= 0 ) {
				gp_Trsf trsfWorkPiece = new gp_Trsf();
				trsfWorkPiece.SetTranslation( g54 );
				outBuf[ workPieceIdx ] = trsfWorkPiece;
			}
		}

		// joint angle ( rad ) of the i-th joint from the post point ( C1 ~ C6 )
		static double GetJointAngle( ArticRobotPostPoint point, int jointIndex )
		{
			switch( jointIndex ) {
				case 0:
					return point.C1;
				case 1:
					return point.C2;
				case 2:
					return point.C3;
				case 3:
					return point.C4;
				case 4:
					return point.C5;
				case 5:
					return point.C6;
				default:
					return 0;
			}
		}

		readonly ArticRobotMachineData m_MachineData;
	}

	// create the per-frame transform builder based on machine product
	public static class PerFrameTransformBuilderFactory
	{
		public static IPerFrameTransformBuilder Create( IMachineDataBase machineData )
		{
			if( machineData is FiveAxisMachineData fiveAxis ) {
				return new FiveAxisPerFrameTransformBuilder( fiveAxis );
			}
			if( machineData is ArticRobotMachineData articRobot ) {
				return new ArticRobotPerFrameTransformBuilder( articRobot );
			}
			return null;
		}
	}
}
