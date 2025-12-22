using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
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
			IStdPatternGeomData standardPatternGeomData = ( m_GeomData is IStdPatternGeomData ) ? (IStdPatternGeomData)m_GeomData.Clone() : null;
			PatternSettingDlg patternFrom = new PatternSettingDlg( standardPatternGeomData );
			patternFrom.Confirm += ConfirmPatternSetting;
			patternFrom.Preview += PreviewPatternSetting;
			patternFrom.Cancel += CancelPatternSetting;
			patternFrom.Show( MyApp.MainForm );
		}

		public override void End()
		{
			RemoveTrihedron();
			base.End();
		}

		void PreviewPatternSetting( IStdPatternGeomData standardPatternGeomData )
		{
			PatternCreate( standardPatternGeomData );
			PathType pathType = ( standardPatternGeomData == null ) ? PathType.Contour : standardPatternGeomData.PathType;
			PropertyChanged?.Invoke( pathType, m_szPathIDList );
		}

		void ConfirmPatternSetting( IStdPatternGeomData standardPatternGeomData )
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

		void PatternCreate( IStdPatternGeomData standardPatternGeomData )
		{
			RemoveTrihedron();
			TopoDS_Shape shape = null;
			foreach( var szID in m_szPathIDList ) {
				if( !DataGettingHelper.GetPathObject( szID, out PathObject pathObject ) ) {
					continue;
				}
				if( !DataGettingHelper.GetContourPathObject( pathObject, out ContourPathObject contourPathObject ) ) {
					continue;
				}

				if( standardPatternGeomData == null ) {
					shape = contourPathObject.Shape;
				}
				else {
					shape = StdPatternHelper.GetPathWire( contourPathObject.GeomData.RefCenterDir, standardPatternGeomData );
					if( shape == null || shape.IsNull() ) {
						continue;
					}

					gp_Ax3 refCoord = StdPatternHelper.GetPatternRefCoord( contourPathObject.GeomData.RefCenterDir, standardPatternGeomData.RotatedAngle_deg );
					ShowStdPatternTrihedron( refCoord );
				}

				m_DataManager.ObjectMap[ szID ] = CreatePathObject( szID, shape, standardPatternGeomData, contourPathObject, pathObject );
				UpdateCanvasPattern( szID, shape );
			}
			m_Viewer.UpdateView();
		}

		void PatternRestore()
		{
			RemoveTrihedron();
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

		PathObject CreatePathObject( string szID, TopoDS_Shape shape, IStdPatternGeomData standardPatternGeomData, ContourPathObject contourPathObject, PathObject originalPathObject )
		{
			CraftData craftData = new CraftData();
			PathType pathType = ( standardPatternGeomData == null ) ? PathType.Contour : standardPatternGeomData.PathType;
			switch( pathType ) {
				case PathType.Circle:
					return new CirclePathObject( szID, shape, standardPatternGeomData as CircleGeomData, craftData, contourPathObject );
				case PathType.Rectangle:
					return new RectanglePathObject( szID, shape, standardPatternGeomData as RectangleGeomData, craftData, contourPathObject );
				case PathType.Runway:
					return new RunwayPathObject( szID, shape, standardPatternGeomData as RunwayGeomData, craftData, contourPathObject );
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					return new PolygonPathObject( szID, shape, standardPatternGeomData as PolygonGeomData, craftData, contourPathObject );
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

		void ShowStdPatternTrihedron( gp_Ax3 refCoord )
		{
			gp_Ax2 ax2 = refCoord.Ax2();
			AIS_Trihedron trihedron = new AIS_Trihedron( new Geom_Axis2Placement( ax2 ) );
			trihedron.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			trihedron.SetSize( 10.0 );
			trihedron.SetAxisColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			trihedron.SetTextColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			trihedron.SetArrowColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			m_Viewer.GetAISContext().Display( trihedron, false );
			m_Viewer.GetAISContext().Deactivate( trihedron );
			m_TrihedronList.Add( trihedron );
		}

		void RemoveTrihedron()
		{
			foreach( var trihedron in m_TrihedronList ) {
				m_Viewer.GetAISContext().Remove( trihedron, false );
			}
			m_TrihedronList.Clear();
		}

		ViewManager m_ViewManager;
		Viewer m_Viewer;
		List<string> m_szPathIDList = new List<string>();
		List<AIS_Trihedron> m_TrihedronList = new List<AIS_Trihedron>();
		IGeomData m_GeomData;
		PathType m_BackFirstPathType;
		Dictionary<string, PathObject> m_BackUpPathObjectList = new Dictionary<string, PathObject>();
	}
}
