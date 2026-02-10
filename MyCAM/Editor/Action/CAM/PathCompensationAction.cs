using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class PathCompensationAction : EditCAMActionBase
	{
		public PathCompensationAction( DataManager dataManager, List<string> pathIDList )
			: base( dataManager, pathIDList )
		{
			BackupCompensatedDist();
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

		public override void Start()
		{
			base.Start();
			PathCompensateDlg compensateDlg = new PathCompensateDlg();
			compensateDlg.Preview += PreviewCompensatedDist;
			compensateDlg.Confirm += ConfirmCompensatedDist;
			compensateDlg.Cancel += CancelCompensatedDist;
			compensateDlg.Reset += RestoreCompensatedDist;
			compensateDlg.Show( MyApp.MainForm );
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void PreviewCompensatedDist( double compensatedDist )
		{
			CheckGeomConstraint( compensatedDist, out bool exceedsLimit );
			if( exceedsLimit ) {
				return;
			}

			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance = isResetClicked ? 0 : m_BackupCompensatedDistList[ i ];
			}
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance += compensatedDist;
			}
			PropertyChanged?.Invoke( m_PathIDList );
			isResetClicked = false;
		}

		void ConfirmCompensatedDist( double compensatedDist )
		{
			CheckGeomConstraint( compensatedDist, out bool exceedsLimit );
			if( exceedsLimit ) {
				return;
			}

			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance = isResetClicked ? 0 : m_BackupCompensatedDistList[ i ];
			}
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance += compensatedDist;
			}
			PropertyChanged?.Invoke( m_PathIDList );

			// because Confirm will end the action, so no need to set isResetClicked to false
			End();
		}

		void CancelCompensatedDist()
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance = m_BackupCompensatedDistList[ i ];
			}
			PropertyChanged?.Invoke( m_PathIDList );

			// because Cancel will end the action, so no need to set isResetClicked to false
			End();
		}

		void RestoreCompensatedDist( double compensatedDist )
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].CompensatedDistance = 0;
			}
			PropertyChanged?.Invoke( m_PathIDList );
			isResetClicked = true;
		}

		void CheckGeomConstraint( double compensatedDist, out bool exceedsLimit )
		{
			exceedsLimit = false;
			GetPathGeomDataList( out List<IStdPatternGeomData> pathGeomDataList );
			for( int i = 0; i < pathGeomDataList.Count; i++ ) {
				double constraint = GetGeomDataMinCompensation( pathGeomDataList[ i ] );
				if( constraint > m_CraftDataList[ i ].CompensatedDistance + compensatedDist ) {
					exceedsLimit = true;
					MyApp.Logger.ShowOnLogPanel( "路徑超出補償範圍，請調整補償數值", MyApp.NoticeType.Warning, true );
				}
			}
		}

		void GetPathGeomDataList( out List<IStdPatternGeomData> pathGeomDataList )
		{
			pathGeomDataList = new List<IStdPatternGeomData>();
			foreach( var pathID in m_PathIDList ) {
				if( !DataGettingHelper.GetGeomDataByID( pathID, out IGeomData geomData ) ) {
					continue;
				}
				if( DataGettingHelper.IsStdPattern( geomData.PathType ) ) {
					pathGeomDataList.Add( (IStdPatternGeomData)geomData );
				}
				else {
					continue;
				}
			}
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

		List<double> m_BackupCompensatedDistList = new List<double>();
		bool isResetClicked = false;
	}
}
