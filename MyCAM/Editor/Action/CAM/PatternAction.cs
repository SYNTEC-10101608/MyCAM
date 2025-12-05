using MyCAM.App;
using MyCAM.Data;
using MyCAM.Data.GeomDataFolder;
using MyCAM.Editor.Dialog;
using MyCAM.Editor.Factory;
using OCC.AIS;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Editor
{
	internal class PatternAction : EditActionBase
	{
		public PatternAction( DataManager dataManager, Viewer viewer, ViewManager viewManager, List<string> szPathIDList )
			: base( dataManager )
		{
			if( szPathIDList == null || szPathIDList.Count == 0 ) {
				throw new ArgumentException( "PatternAction constructing argument szPathIDList invalid" );
			}

			foreach( string pathID in szPathIDList ) {
				if( !( dataManager.ObjectMap.ContainsKey( pathID ) && dataManager.ObjectMap[ pathID ] is PathObject ) ) {
					throw new ArgumentException( "PatternAction constructing argument szPathIDList invalid pathID: " + pathID );
				}
			}

			m_PatternSettingInfoList = new List<PatternSettingInfo>();
			foreach( string pathID in szPathIDList ) {
				IGeomData geomData = DataGettingHelper.GetGeomDataByID( pathID );
				if( geomData == null ) {
					continue;
				}

				ContourPathObject contourPathObject = null;
				if( dataManager.ObjectMap[ pathID ] is PathObject pathObject ) {
					contourPathObject = GetContourPathObject( pathObject );
				}
				m_PatternSettingInfoList.Add( new PatternSettingInfo( geomData, contourPathObject ) );
			}

			m_BackUpPatternSettingInfoList = m_PatternSettingInfoList.Select( info => info.Clone() ).ToList();

			m_Viewer = viewer;
			m_ViewManager = viewManager;
			m_szPathIDList = szPathIDList;
		}

		public Action<PathType, List<string>> PropertyChanged;

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SetPattern;
			}
		}

		public override void Start()
		{
			base.Start();
			PatternSettingDlg patternFrom = new PatternSettingDlg( m_PatternSettingInfoList );
			patternFrom.Confirm += ConfirmPatternSetting;
			patternFrom.Preview += PreviewPatternSetting;
			patternFrom.Cancel += CancelPatternSetting;
			patternFrom.Show( MyApp.MainForm );
		}

		void PreviewPatternSetting( List<PatternSettingInfo> patternSettingInfoList )
		{
			PatternCreate( patternSettingInfoList );
			PropertyChanged?.Invoke( patternSettingInfoList.First().GeomData.PathType, m_szPathIDList );
		}

		void ConfirmPatternSetting( List<PatternSettingInfo> patternSettingInfoList )
		{
			PatternCreate( patternSettingInfoList );
			PropertyChanged?.Invoke( patternSettingInfoList.First().GeomData.PathType, m_szPathIDList );
			End();
		}

		void CancelPatternSetting()
		{
			PatternCreate( m_BackUpPatternSettingInfoList );
			PropertyChanged?.Invoke( m_BackUpPatternSettingInfoList.First().GeomData.PathType, m_szPathIDList );
			End();
		}

		void PatternCreate( List<PatternSettingInfo> patternSettingInfoList )
		{
			TopoDS_Shape shape = null;
			List<IGeomData> geomDataList = patternSettingInfoList.Select( info => info.GeomData ).ToList();
			Dictionary<string, PathObject> pathObjectDict = m_DataManager.GetPathObjectDictionary();
			int nCount = 0;
			foreach( var szID in m_szPathIDList ) {
				if( !pathObjectDict.ContainsKey( szID ) || pathObjectDict[ szID ] == null ) {
					nCount++;
					continue;
				}

				if( !ShapeCreate( geomDataList[ nCount ], patternSettingInfoList[ nCount ].ContourPathObject.ContourGeomData.CADPointList, out shape ) ) {
					nCount++;
					continue;
				}

				ContourPathObject contourPathObject = GetContourPathObject( pathObjectDict[ szID ] );
				m_DataManager.ObjectMap[ szID ] = CreatePathObject( szID, shape, geomDataList[ nCount ], contourPathObject, pathObjectDict[ szID ] );
				UpdateCanvasPattern( szID, shape );
				nCount++;
			}
			m_Viewer.UpdateView();
		}

		bool ShapeCreate( IGeomData geomData, List<CADPoint> originalCADPointList, out TopoDS_Shape shape )
		{
			PatternFactory standardPatternFactory = new PatternFactory( originalCADPointList, geomData );
			shape = standardPatternFactory.GetShape();
			if( shape == null || shape.IsNull() ) {
				return false;
			}
			return true;
		}

		PathObject CreatePathObject( string szID, TopoDS_Shape shape, IGeomData geomData, ContourPathObject contourPathObject, PathObject originalPathObject )
		{
			switch( geomData.PathType ) {
				case PathType.Circle:
					return new CirclePathObject( szID, shape, geomData as CircleGeomData, originalPathObject.CraftData, contourPathObject );
				case PathType.Rectangle:
					return new RectanglePathObject( szID, shape, geomData as RectangleGeomData, originalPathObject.CraftData, contourPathObject );
				case PathType.Contour:
				default:

					// For ContourPathObject, we need to use ContourGeomData
					ContourGeomData contourGeomData = geomData as ContourGeomData;
					if( contourGeomData == null ) {

						// If geomData is not ContourGeomData, create one from the original
						contourGeomData = ( originalPathObject as ContourPathObject ).ContourGeomData;
					}
					return new ContourPathObject( szID, shape, contourGeomData, originalPathObject.CraftData );
			}
		}

		ContourPathObject GetContourPathObject( PathObject pathObject )
		{
			switch( pathObject.PathType ) {
				case PathType.Circle:
					return ( pathObject as CirclePathObject ).ContourPathObject;
				case PathType.Rectangle:
					return ( pathObject as RectanglePathObject ).ContourPathObject;
				case PathType.Contour:
				default:
					return pathObject as ContourPathObject;
			}
		}

		void UpdateCanvasPattern( string szID, TopoDS_Shape shape )
		{
			AIS_Shape shapeAIS = AIS_Shape.DownCast( m_ViewManager.ViewObjectMap[ szID ].AISHandle );
			shapeAIS.SetShape( shape );
			m_Viewer.GetAISContext().Redisplay( shapeAIS, false );
		}


		ViewManager m_ViewManager;
		Viewer m_Viewer;
		List<string> m_szPathIDList = new List<string>();
		List<PatternSettingInfo> m_PatternSettingInfoList;
		List<PatternSettingInfo> m_BackUpPatternSettingInfoList;
	}
}
