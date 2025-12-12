using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.StandardPatternFactory;
using OCC.AIS;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;

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

			foreach( string pathID in szPathIDList ) {
				if( !DataGettingHelper.GetGeomDataByID( pathID, out IGeomData geomData ) ) {
					continue;
				}
				if( m_GeomData == null ) {
					m_GeomData = geomData;
					m_BackFirstPathType = geomData.PathType;
				}
				m_BackUpPathObjectList.Add( pathID, dataManager.ObjectMap[ pathID ] as PathObject );
			}
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
			IStandardPatternGeomData standardPatternGeomData = ( m_GeomData is IStandardPatternGeomData ) ? (IStandardPatternGeomData)m_GeomData.Clone() : null;
			PatternSettingDlg patternFrom = new PatternSettingDlg( standardPatternGeomData );
			patternFrom.Confirm += ConfirmPatternSetting;
			patternFrom.Preview += PreviewPatternSetting;
			patternFrom.Cancel += CancelPatternSetting;
			patternFrom.Show( MyApp.MainForm );
		}

		void PreviewPatternSetting( IStandardPatternGeomData standardPatternGeomData )
		{
			PatternCreate( standardPatternGeomData );
			PathType pathType = ( standardPatternGeomData == null ) ? PathType.Contour : standardPatternGeomData.PathType;
			PropertyChanged?.Invoke( pathType, m_szPathIDList );
		}

		void ConfirmPatternSetting( IStandardPatternGeomData standardPatternGeomData )
		{
			PatternCreate( standardPatternGeomData );
			PathType pathType = ( standardPatternGeomData == null ) ? PathType.Contour : standardPatternGeomData.PathType;
			PropertyChanged?.Invoke( pathType, m_szPathIDList );
			End();
		}

		void CancelPatternSetting()
		{
			PatternRestore();
			PropertyChanged?.Invoke( m_BackFirstPathType, m_szPathIDList );
			End();
		}

		void PatternCreate( IStandardPatternGeomData standardPatternGeomData )
		{
			TopoDS_Shape shape = null;
			foreach( var szID in m_szPathIDList ) {
				if( !DataGettingHelper.TryGetPathObject( szID, out PathObject pathObject ) ) {
					continue;
				}
				if( !DataGettingHelper.GetContourPathObject( pathObject, out ContourPathObject contourPathObject ) ) {
					continue;
				}

				if( standardPatternGeomData == null ) {
					shape = contourPathObject.Shape;
				}
				else {
					PatternFactory standardPatternFactory = new PatternFactory( contourPathObject.ContourGeomData, standardPatternGeomData );
					shape = standardPatternFactory.GetShape();
				}

				m_DataManager.ObjectMap[ szID ] = CreatePathObject( szID, shape, standardPatternGeomData, contourPathObject, pathObject );
				UpdateCanvasPattern( szID, shape );
			}
			m_Viewer.UpdateView();
		}

		void PatternRestore()
		{
			TopoDS_Shape shape = null;
			foreach( var szID in m_szPathIDList ) {
				if( !m_BackUpPathObjectList.ContainsKey( szID ) || m_BackUpPathObjectList[ szID ] == null ) {
					continue;
				}
				shape = m_BackUpPathObjectList[ szID ].Shape;
				m_DataManager.ObjectMap[ szID ] = m_BackUpPathObjectList[ szID ];
				UpdateCanvasPattern( szID, shape );
			}
			m_Viewer.UpdateView();
		}

		PathObject CreatePathObject( string szID, TopoDS_Shape shape, IStandardPatternGeomData standardPatternGeomData, ContourPathObject contourPathObject, PathObject originalPathObject )
		{
			PathType pathType = ( standardPatternGeomData == null ) ? PathType.Contour : standardPatternGeomData.PathType;
			switch( pathType ) {
				case PathType.Circle:
					return new CirclePathObject( szID, shape, standardPatternGeomData as CircleGeomData, originalPathObject.CraftData, contourPathObject );
				case PathType.Rectangle:
					return new RectanglePathObject( szID, shape, standardPatternGeomData as RectangleGeomData, originalPathObject.CraftData, contourPathObject );
				case PathType.Runway:
					return new RunwayPathObject( szID, shape, standardPatternGeomData as RunwayGeomData, originalPathObject.CraftData, contourPathObject );
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					return new PolygonPathObject( szID, shape, standardPatternGeomData as PolygonGeomData, originalPathObject.CraftData, contourPathObject );
				case PathType.Contour:
				default:
					return contourPathObject;
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
		IGeomData m_GeomData;
		PathType m_BackFirstPathType;
		Dictionary<string, PathObject> m_BackUpPathObjectList = new Dictionary<string, PathObject>();
	}
}
