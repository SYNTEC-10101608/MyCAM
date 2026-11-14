using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.PathCache;

namespace MyCAM.Editor
{
	internal class PathCompensationAction : EditCAMActionBase
	{
		public PathCompensationAction( DataManager dataManager, List<string> pathIDList )
			: base( dataManager, dataManager.GetEditablePathIDList( pathIDList ) )
		{
			m_SelectedPathIDList = new List<string>( pathIDList );
			BackupCompensatedDist();
			BackupGeomData();
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.PathCompensation;
			}
		}

		void BackupCompensatedDist()
		{
			foreach( var craftData in m_CraftDataList ) {
				if( craftData == null ) {
					throw new ArgumentNullException( "PathEditAction constructing argument craftDataList contains null craftData" );
				}
				m_BackupCompensatedDistList.Add( craftData.CompensatedDistance );
			}
		}

		void BackupGeomData()
		{
			m_BackupGeomDataList.Clear();
			foreach( var pathID in m_PathIDList ) {
				if( !DataGettingHelper.GetPathCacheByID( pathID, out IPathCache pathCache ) ) {
					continue;
				}
				if( pathCache is IStdPatternCache stdPatternCache ) {
					m_BackupGeomDataList.Add( stdPatternCache.ComputeGeomData );
				}
				else {
					continue;
				}
			}
		}

		public override void Start()
		{
			base.Start();
			m_Dlg = new PathCompensateDlg();
			m_Dlg.Preview = PreviewCompensatedDist;
			m_Dlg.Confirm = ConfirmCompensatedDist;
			m_Dlg.Cancel += CancelCompensatedDist;
			m_Dlg.Reset = RestoreCompensatedDist;
			m_Dlg.IsGeomConstraintExceedsLimit = IsGeomConstraintExceedsLimit;
			m_Dlg.Show( MyApp.MainForm );
			PropertyChanged?.Invoke( m_SelectedPathIDList );

			UpdateDialog();
		}

		public override void End()
		{
			if( !m_IsConfirmed ) {
				RestoreBackupData();
			}
			if( m_Dlg != null ) {
				m_Dlg.Preview = null;
				m_Dlg.Confirm = null;
				m_Dlg.Cancel = null;
				m_Dlg.Reset = null;
				m_Dlg.IsGeomConstraintExceedsLimit = null;
				m_Dlg.Close();
				m_Dlg = null;
			}
			base.End();
		}

		void PreviewCompensatedDist( double compensatedDist )
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance = isResetClicked ? 0 : m_BackupCompensatedDistList[ i ];
			}
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance += compensatedDist;
			}
			PropertyChanged?.Invoke( m_SelectedPathIDList );
			isResetClicked = false;

			UpdateDialog();
		}

		void ConfirmCompensatedDist( double compensatedDist )
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance = isResetClicked ? 0 : m_BackupCompensatedDistList[ i ];
			}
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance += compensatedDist;
			}
			PropertyChanged?.Invoke( m_SelectedPathIDList );
			m_IsConfirmed = true;
			End();
		}

		void CancelCompensatedDist()
		{
			End();
		}

		void RestoreBackupData()
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance = m_BackupCompensatedDistList[ i ];
			}
			PropertyChanged?.Invoke( m_SelectedPathIDList );

			UpdateDialog();
		}

		void RestoreCompensatedDist( double compensatedDist )
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance = 0;
			}
			PropertyChanged?.Invoke( m_SelectedPathIDList );
			isResetClicked = true;

			UpdateDialog();
		}

		bool IsGeomConstraintExceedsLimit( double compensatedDist )
		{
			if( isResetClicked ) {
				return false;
			}
			for( int i = 0; i < m_BackupGeomDataList.Count; i++ ) {
				double constraint = GetGeomDataMinCompensation( m_BackupGeomDataList[ i ] );
				if( constraint > compensatedDist ) {
					MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CompensationExceedsLimit", COMPENSATION_EXCEEDS_LIMIT ), MyApp.NoticeType.Warning, false );
					return true;
				}
			}
			return false;
		}

		void UpdateDialog()
		{
			double reference = m_CraftDataList.First().CompensatedDistance;
			bool isAllSame = m_CraftDataList.All( craftData => Math.Abs( craftData.CompensatedDistance - reference ) <= GEOMETRY_TOLERANCE );
			m_Dlg.SetTotalCompsensation( isAllSame ? m_CraftDataList.First().CompensatedDistance : (double?)null );
		}

		double GetGeomDataMinCompensation( IStdPatternGeomData geomData )
		{
			if( geomData is CircleGeomData circleGeomData ) {
				return -circleGeomData.Diameter / 2.0;
			}
			else if( geomData is RectangleGeomData rectangleGeomData ) {
				double minDimension = Math.Min( rectangleGeomData.Width, rectangleGeomData.Length );
				return -minDimension / 2.0;
			}
			else if( geomData is PolygonGeomData polygonGeomData ) {
				double angle = Math.PI / polygonGeomData.Sides;
				double apothem = polygonGeomData.SideLength / ( 2.0 * Math.Tan( angle ) );
				return -apothem;
			}
			else if( geomData is RunwayGeomData runwayGeomData ) {
				double minDimension = Math.Min( runwayGeomData.Length, runwayGeomData.Width );
				return -minDimension / 2.0;
			}

			return 0;
		}

		List<string> m_SelectedPathIDList;
		List<double> m_BackupCompensatedDistList = new List<double>();
		List<IStdPatternGeomData> m_BackupGeomDataList = new List<IStdPatternGeomData>();
		bool isResetClicked = false;
		bool m_IsConfirmed = false;
		PathCompensateDlg m_Dlg;
		const string COMPENSATION_EXCEEDS_LIMIT = "路徑超出補償範圍，請調整補償數值";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "PathCompensationAction" );

		const double GEOMETRY_TOLERANCE = 1e-3;
	}
}
