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
			PathEditDlg pathEditFrom = new PathEditDlg();
			pathEditFrom.Confirm += ConfirmPathEditData;
			pathEditFrom.Preview += PreviewPathEditData;
			pathEditFrom.Cancel += CancelPathEditData;
			pathEditFrom.Reset += ResetPathEditData;
			pathEditFrom.Show( MyApp.MainForm );
			PropertyChanged?.Invoke( m_PathIDList );
		}

		public override void End()
		{
			RemoveTrihedron();
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
			End();
		}

		void CancelPathEditData()
		{
			for( int i = 0; i < m_BackupTrsfMatrixList.Count; i++ ) {
				m_CraftDataList[ i ].CumulativeTrsfMatrix = m_BackupTrsfMatrixList[ i ];
			}
			PropertyChanged?.Invoke( m_PathIDList );
			End();
		}

		void ResetPathEditData( PathEditData data )
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CumulativeTrsfMatrix = new gp_Trsf();
			}
			DisplayRefCoordinates( new PathEditData( data.RefCoordType, 0, 0, 0 ) );
			PropertyChanged?.Invoke( m_PathIDList );
			m_IsResetClicked = true;
		}

		void UpdatePathData( PathEditData data )
		{
			RestoreBackupTrsfMatrices();
			ApplyTrsfToAllPaths( data );
			DisplayRefCoordinates( data );
			RefreshViewer();
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
			AIS_Trihedron trihedron = DrawHelper.GetTrihedronAIS( refCoord.Ax2() );
			m_RefCoordTrihedronList.Add( trihedron );
			m_Viewer.GetAISContext().Display( trihedron, false );
			m_Viewer.GetAISContext().Deactivate( trihedron );
		}

		void RefreshViewer()
		{
			m_Viewer.UpdateView();
			PropertyChanged?.Invoke( m_PathIDList );
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
			m_RefCoordType = refCoordType;
			m_XDirOffset = xDirOffset;
			m_YDirOffset = yDirOffset;
			m_ZDirOffset = zDirOffset;
		}

		public PathEditData()
		{
			m_RefCoordType = RefCoordinateType.World;
			m_XDirOffset = 0;
			m_YDirOffset = 0;
			m_ZDirOffset = 0;
		}

		public RefCoordinateType RefCoordType
		{
			get
			{
				return m_RefCoordType;
			}
		}

		public double XDirOffset
		{
			get
			{
				return m_XDirOffset;
			}
		}

		public double YDirOffset
		{
			get
			{
				return m_YDirOffset;
			}
		}

		public double ZDirOffset
		{
			get
			{
				return m_ZDirOffset;
			}
		}

		RefCoordinateType m_RefCoordType;
		double m_XDirOffset;
		double m_YDirOffset;
		double m_ZDirOffset;
	}
}
