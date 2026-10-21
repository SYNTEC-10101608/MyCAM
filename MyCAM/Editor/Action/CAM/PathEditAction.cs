using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.gp;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Editor
{
	internal class PathEditAction : EditCAMActionBase
	{
		public PathEditAction( DataManager dataManager, List<string> pathIDList, Viewer viewer )
			: base( dataManager, pathIDList )
		{
			m_Viewer = viewer;
			m_BackupTrsfMatrixList = new List<gp_Trsf>();
			BackupTransformMatrices();
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.PathEdit;
			}
		}

		public override void Start()
		{
			base.Start();
			m_Dlg = new PathEditDlg();
			m_Dlg.Confirm = ConfirmPathEditData;
			m_Dlg.Preview = PreviewPathEditData;
			m_Dlg.Cancel = CancelPathEditData;
			m_Dlg.Reset = ResetPathEditData;
			m_Dlg.Show( MyApp.MainForm );
			PropertyChanged?.Invoke( m_PathIDList );
		}

		public override void End()
		{
			if( !m_IsConfirmed ) {
				RestoreBackupData();
			}
			RemoveTrihedron();
			if( m_Dlg != null ) {
				m_Dlg.Preview = null;
				m_Dlg.Confirm = null;
				m_Dlg.Cancel = null;
				m_Dlg.Reset = null;
				m_Dlg.Close();
				m_Dlg = null;
			}
			base.End();
		}

		void RemoveTrihedron()
		{
			foreach( var trihedron in m_RefCoordTrihedronList ) {
				m_Viewer.GetAISContext().Remove( trihedron, false );
			}
			m_RefCoordTrihedronList.Clear();
		}

		void PreviewPathEditData( PathEditData data )
		{
			RemoveTrihedron();
			UpdatePathData( data );
		}

		void ConfirmPathEditData( PathEditData data )
		{
			UpdatePathData( data );
			m_IsConfirmed = true;
			End();
		}

		void CancelPathEditData()
		{
			End();
		}

		void RestoreBackupData()
		{
			for( int i = 0; i < m_BackupTrsfMatrixList.Count; i++ ) {
				m_CraftDataList[ i ].CumulativeTrsfMatrix = m_BackupTrsfMatrixList[ i ];
			}
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void ResetPathEditData( PathEditData data )
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CumulativeTrsfMatrix = new gp_Trsf();
			}
			RemoveTrihedron();
			DisplayRefCoordinates( new PathEditData( data.RefCoordType, 0, 0, 0 ) );
			PropertyChanged?.Invoke( m_PathIDList );
			m_IsResetClicked = true;
			UpdateDialog( data );
		}

		void UpdatePathData( PathEditData data )
		{
			RestoreBackupTrsfMatrices();
			ApplyTrsfToAllPaths( data );
			DisplayRefCoordinates( data );
			RefreshViewer();
			UpdateDialog( data );
		}

		void BackupTransformMatrices()
		{
			foreach( var craftData in m_CraftDataList ) {
				if( craftData == null ) {
					throw new ArgumentNullException( "PathEditAction constructing argument craftDataList contains null craftData" );
				}
				if( craftData.CumulativeTrsfMatrix == null ) {
					m_BackupTrsfMatrixList.Add( new gp_Trsf() );
				}
				else {
					m_BackupTrsfMatrixList.Add( craftData.CumulativeTrsfMatrix.MakeCopy() );
				}
			}
		}

		void RestoreBackupTrsfMatrices()
		{
			for( int i = 0; i < m_BackupTrsfMatrixList.Count; i++ ) {
				m_CraftDataList[ i ].CumulativeTrsfMatrix = m_IsResetClicked ? new gp_Trsf() : m_BackupTrsfMatrixList[ i ].MakeCopy();
			}
		}

		void ApplyTrsfToAllPaths( PathEditData data )
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CumulativeTrsfMatrix = ApplyPathEditData( data, m_CraftDataList[ i ].CumulativeTrsfMatrix, m_PathIDList[ i ] );
			}
		}

		void DisplayRefCoordinates( PathEditData data )
		{
			for( int i = 0; i < m_PathIDList.Count; i++ ) {
				string pathID = m_PathIDList[ i ];
				gp_Ax3 refCoord = GetRefCoordinate( pathID, data.RefCoordType );
				DisplayTrihedron( refCoord );
			}
		}

		gp_Ax3 GetRefCoordinate( string pathID, RefCoordinateType refCoordType )
		{
			if( !DataGettingHelper.GetPathCacheByID( pathID, out IPathCache pathCache ) ) {
				return new gp_Ax3();
			}

			if( refCoordType == RefCoordinateType.Local ) {
				return pathCache.RefCoord;
			}
			else {
				return new gp_Ax3( pathCache.RefCoord.Location(), new gp_Dir( 0, 0, 1 ) );
			}
		}

		void DisplayTrihedron( gp_Ax3 refCoord )
		{
			AIS_Trihedron trihedron = DrawHelper.GetTrihedronAIS( refCoord.Ax2(), 10.0, OCC.Quantity.Quantity_NameOfColor.Quantity_NOC_RED );
			m_RefCoordTrihedronList.Add( trihedron );
			m_Viewer.GetAISContext().Display( trihedron, false );
			m_Viewer.GetAISContext().Deactivate( trihedron );
		}

		void RefreshViewer()
		{
			m_Viewer.UpdateView();
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void UpdateDialog( PathEditData data )
		{
			List<gp_XYZ> totalOffsetList = new List<gp_XYZ>();
			for( int i = 0; i < m_PathIDList.Count; ++i ) {
				totalOffsetList.Add( GetTotalOffset( i, data.RefCoordType ) );
			}

			for( int i = 1; i < totalOffsetList.Count; ++i ) {
				if( !totalOffsetList[ i ].IsEqual( totalOffsetList.First(), GEOMETRY_TOLERANCE ) ) {
					m_Dlg.SetTotaloffset( null, null, null );
					return;
				}
			}

			m_Dlg.SetTotaloffset( totalOffsetList.First().X(), totalOffsetList.First().Y(), totalOffsetList.First().Z() );
		}

		gp_XYZ GetTotalOffset( int pathIndex, RefCoordinateType coordinateType )
		{
			gp_Vec displacement = new gp_Vec( m_CraftDataList[ pathIndex ].CumulativeTrsfMatrix.TranslationPart() );

			if( coordinateType == RefCoordinateType.World ) {
				return displacement.XYZ();
			}

			gp_Ax3 coordinate = GetPathRefCoord( m_PathIDList[ pathIndex ] );

			gp_Trsf worldToLocal = new gp_Trsf();
			worldToLocal.SetTransformation( coordinate );
			return displacement.Transformed( worldToLocal ).XYZ();
		}

		gp_Trsf ApplyPathEditData( PathEditData editData, gp_Trsf transform, string pathID )
		{
			if( editData == null ) {
				throw new ArgumentNullException( nameof( editData ) );
			}
			if( transform == null ) {
				throw new ArgumentNullException( nameof( transform ) );
			}

			if( IsZeroOffset( editData ) ) {
				return transform.MakeCopy();
			}

			gp_Vec translationVec = CalculateTranslationVec( editData, pathID );
			return ApplyTranslation( transform, translationVec );
		}

		bool IsZeroOffset( PathEditData editData )
		{
			return editData.XDirOffset == 0 && editData.YDirOffset == 0 && editData.ZDirOffset == 0;
		}

		gp_Vec CalculateTranslationVec( PathEditData editData, string pathID )
		{
			if( editData.RefCoordType == RefCoordinateType.Local ) {
				return CalculateLocalToWorldTranslationVec( editData, pathID );
			}
			else {
				return new gp_Vec( editData.XDirOffset, editData.YDirOffset, editData.ZDirOffset );
			}
		}

		gp_Vec CalculateLocalToWorldTranslationVec( PathEditData editData, string pathID )
		{
			gp_Ax3 refCoord = GetPathRefCoord( pathID );
			if( refCoord == null ) {
				return new gp_Vec( editData.XDirOffset, editData.YDirOffset, editData.ZDirOffset );
			}

			gp_Vec localVec = new gp_Vec( editData.XDirOffset, editData.YDirOffset, editData.ZDirOffset );
			gp_Vec worldVec = new gp_Vec(
				refCoord.XDirection().XYZ() * localVec.X() +
				refCoord.YDirection().XYZ() * localVec.Y() +
				refCoord.Direction().XYZ() * localVec.Z()
			);
			return worldVec;
		}

		gp_Trsf ApplyTranslation( gp_Trsf transform, gp_Vec translationVec )
		{
			gp_Trsf result = transform.MakeCopy();
			gp_Trsf translationTrsf = new gp_Trsf();
			translationTrsf.SetTranslation( translationVec );
			result = result.Multiplied( translationTrsf );
			return result;
		}

		gp_Ax3 GetPathRefCoord( string pathID )
		{
			if( DataGettingHelper.GetPathCacheByID( pathID, out IPathCache pathCache ) ) {
				return pathCache.RefCoord;
			}
			return null;
		}

		List<gp_Trsf> m_BackupTrsfMatrixList;
		List<AIS_Trihedron> m_RefCoordTrihedronList = new List<AIS_Trihedron>();
		Viewer m_Viewer;
		bool m_IsResetClicked = false;
		bool m_IsConfirmed = false;
		PathEditDlg m_Dlg;

		const double GEOMETRY_TOLERANCE = 1e-3;
	}

	public enum RefCoordinateType
	{
		World,
		Local,
	}

	public class PathEditData
	{
		public PathEditData( RefCoordinateType refCoordType, double xDirOffset, double yDirOffset, double zDirOffset )
		{
			RefCoordType = refCoordType;
			XDirOffset = xDirOffset;
			YDirOffset = yDirOffset;
			ZDirOffset = zDirOffset;
		}

		public PathEditData()
		{
			RefCoordType = RefCoordinateType.World;
			XDirOffset = 0;
			YDirOffset = 0;
			ZDirOffset = 0;
		}

		public RefCoordinateType RefCoordType
		{
			get; private set;
		}

		public double XDirOffset
		{
			get; private set;
		}

		public double YDirOffset
		{
			get; private set;
		}

		public double ZDirOffset
		{
			get; private set;
		}
	}
}
