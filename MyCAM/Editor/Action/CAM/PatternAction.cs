using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.gp;
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
				m_BackUpGeomDataList.Add( pathID, geomData.Clone() );
			}
			m_Viewer = viewer;
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
			foreach( var szID in m_szPathIDList ) {
				ProcessSinglePath( szID, standardPatternGeomData );
			}
			m_Viewer.UpdateView();
		}

		void ProcessSinglePath( string szID, IStdPatternGeomData standardPatternGeomData )
		{
			// GeomData needs to be copied because it will be used in different paths.
			IStdPatternGeomData stdPatternGeomDataClone = ( standardPatternGeomData != null ) ? (IStdPatternGeomData)standardPatternGeomData.Clone() : null;

			// get path object
			if( !DataGettingHelper.GetPathObject( szID, out PathObject pathObject ) ) {
				return;
			}

			// get contour path object
			if( !GetContourPathObject( pathObject, out ContourPathObject contourPathObject ) ) {
				return;
			}

			// TODO：shape isn't necessary
			TopoDS_Shape shape = CreatePatternShape( szID, contourPathObject, stdPatternGeomDataClone );
			if( shape == null || shape.IsNull() ) {
				return;
			}

			if( !DataGettingHelper.GetGeomDataByID( szID, out IGeomData oldGeomData ) ) {
				return;
			}

			UpdateGeomDataForPath( szID, shape, oldGeomData, contourPathObject, stdPatternGeomDataClone );
		}

		TopoDS_Shape CreatePatternShape( string szID, ContourPathObject contourPathObject, IStdPatternGeomData standardPatternGeomData )
		{
			if( standardPatternGeomData == null ) {
				return contourPathObject.Shape;
			}

			if( contourPathObject?.GeomData == null ) {
				return null;
			}

			// create pattern shape
			TopoDS_Shape shape = StdPatternHelper.GetPathWire( contourPathObject.GeomData.RefCenterDir, standardPatternGeomData );
			if( shape == null || shape.IsNull() ) {
				return null;
			}

			return shape;
		}

		void UpdateGeomDataForPath( string szID, TopoDS_Shape shape, IGeomData oldGeomData, ContourPathObject contourPathObject, IStdPatternGeomData standardPatternGeomData )
		{
			// should convert to contour
			if( oldGeomData.PathType != PathType.Contour && standardPatternGeomData == null ) {
				m_DataManager.ObjectMap[ szID ] = contourPathObject;
			}

			// should check if update geom data
			else if( standardPatternGeomData != null && oldGeomData.PathType == standardPatternGeomData.PathType ) {
				ShowStdPatternTrihedron( szID, standardPatternGeomData, false );
				UpdateGeomData( oldGeomData, standardPatternGeomData );
			}
			else {
				if( standardPatternGeomData != null ) {
					if( contourPathObject?.GeomData != null ) {
						standardPatternGeomData.RefCenterDir = contourPathObject.GeomData.RefCenterDir;
					}
					ShowStdPatternTrihedron( szID, standardPatternGeomData, true );
				}

				// create new path object
				if( m_BackUpPathObjectList.ContainsKey( szID ) ) {
					PathObject newPathObject = CreatePathObject( szID, shape, standardPatternGeomData, contourPathObject, m_BackUpPathObjectList[ szID ] );
					if( newPathObject != null ) {
						m_DataManager.ObjectMap[ szID ] = newPathObject;
					}
				}
			}
		}

		void UpdateGeomData( IGeomData oldGeomData, IStdPatternGeomData standardPatternGeomData )
		{
			switch( oldGeomData.PathType ) {
				case PathType.Circle:
					UpdateCircleGeomData( oldGeomData as CircleGeomData, standardPatternGeomData as CircleGeomData );
					break;
				case PathType.Rectangle:
					UpdateRectangleGeomData( oldGeomData as RectangleGeomData, standardPatternGeomData as RectangleGeomData );
					break;
				case PathType.Runway:
					UpdateRunwayGeomData( oldGeomData as RunwayGeomData, standardPatternGeomData as RunwayGeomData );
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					UpdatePolygonGeomData( oldGeomData as PolygonGeomData, standardPatternGeomData as PolygonGeomData );
					break;
			}
		}

		void PatternRestore()
		{
			RemoveTrihedron();
			foreach( var szID in m_szPathIDList ) {
				if( !m_BackUpPathObjectList.ContainsKey( szID ) || m_BackUpPathObjectList[ szID ] == null ) {
					continue;
				}
				if( !m_DataManager.ObjectMap.ContainsKey( szID ) ) {
					continue;
				}
				PathObject currentPathObject = m_DataManager.ObjectMap[ szID ] as PathObject;
				if( currentPathObject == null ) {
					continue;
				}
				if( m_BackUpPathObjectList[ szID ].PathType != PathType.Contour && currentPathObject.PathType == m_BackUpPathObjectList[ szID ].PathType ) {

					// reset geom data to back up value, because the path object may be reused if only geom data is updated in PatternCreate
					if( !m_BackUpGeomDataList.ContainsKey( szID ) ) {
						continue;
					}
					switch( m_BackUpPathObjectList[ szID ].PathType ) {
						case PathType.Circle:
							UpdateCircleGeomData( ( m_BackUpPathObjectList[ szID ] as CirclePathObject ).GeomData as CircleGeomData, ( m_BackUpGeomDataList[ szID ] as CircleGeomData ) );
							break;
						case PathType.Rectangle:
							UpdateRectangleGeomData( ( m_BackUpPathObjectList[ szID ] as RectanglePathObject ).GeomData as RectangleGeomData, ( m_BackUpGeomDataList[ szID ] as RectangleGeomData ) );
							break;
						case PathType.Runway:
							UpdateRunwayGeomData( ( m_BackUpPathObjectList[ szID ] as RunwayPathObject ).GeomData as RunwayGeomData, ( m_BackUpGeomDataList[ szID ] as RunwayGeomData ) );
							break;
						case PathType.Triangle:
						case PathType.Square:
						case PathType.Pentagon:
						case PathType.Hexagon:
							UpdatePolygonGeomData( ( m_BackUpPathObjectList[ szID ] as PolygonPathObject ).GeomData as PolygonGeomData, ( m_BackUpGeomDataList[ szID ] as PolygonGeomData ) );
							break;
						default:
							break;
					}
				}
				m_DataManager.ObjectMap[ szID ] = m_BackUpPathObjectList[ szID ];
			}
			m_Viewer.UpdateView();
		}

		void ShowStdPatternTrihedron( string szID, IStdPatternGeomData standardPatternGeomData, bool isTypeChanged )
		{
			if( standardPatternGeomData == null ) {
				return;
			}

			gp_Ax1 refCenterDir = new gp_Ax1();
			if( isTypeChanged ) {
				refCenterDir = standardPatternGeomData.RefCenterDir;
			}
			else {
				if( DataGettingHelper.GetPathCacheByID( szID, out IPathCache pathCache ) ) {
					refCenterDir = pathCache.ComputeRefCenterDir;
				}
			}
			gp_Ax3 refCoord = StdPatternHelper.GetPatternRefCoord( refCenterDir, standardPatternGeomData.IsCoordinateReversed, standardPatternGeomData.RotatedAngle_deg );
			AIS_Trihedron trihedron = DrawHelper.GetTrihedronAIS( refCoord.Ax2() );
			if( trihedron == null ) {
				return;
			}

			m_Viewer.GetAISContext().Display( trihedron, false );
			m_Viewer.GetAISContext().Deactivate( trihedron );
			m_TrihedronList.Add( trihedron );
		}

		void RemoveTrihedron()
		{
			foreach( var trihedron in m_TrihedronList ) {
				if( trihedron != null ) {
					m_Viewer.GetAISContext().Remove( trihedron, false );
				}
			}
			m_TrihedronList.Clear();
		}

		bool GetContourPathObject( PathObject pathObject, out ContourPathObject contourPathObj )
		{
			contourPathObj = null;
			if( pathObject == null ) {
				return false;
			}

			// use unified ContourPathObject property from StandardPatternBasedPathObject
			if( pathObject is StdPatternObjectBase standardPatternPathObject ) {
				contourPathObj = standardPatternPathObject.ContourPathObject;
				return true;
			}
			else if( pathObject is ContourPathObject contourPathObject ) {
				contourPathObj = contourPathObject;
				return true;
			}
			return false;
		}

		void UpdateCircleGeomData( CircleGeomData oldGeomData, CircleGeomData newGeomData )
		{
			if( oldGeomData == null || newGeomData == null ) {
				return;
			}
			if( oldGeomData.Diameter != newGeomData.Diameter ) {
				oldGeomData.Diameter = newGeomData.Diameter;
			}
			if( oldGeomData.IsCoordinateReversed != newGeomData.IsCoordinateReversed ) {
				oldGeomData.IsCoordinateReversed = newGeomData.IsCoordinateReversed;
			}
			if( oldGeomData.RotatedAngle_deg != newGeomData.RotatedAngle_deg ) {
				oldGeomData.RotatedAngle_deg = newGeomData.RotatedAngle_deg;
			}
		}

		void UpdateRectangleGeomData( RectangleGeomData oldGeomData, RectangleGeomData newGeomData )
		{
			if( oldGeomData == null || newGeomData == null ) {
				return;
			}
			if( oldGeomData.Width != newGeomData.Width ) {
				oldGeomData.Width = newGeomData.Width;
			}
			if( oldGeomData.Length != newGeomData.Length ) {
				oldGeomData.Length = newGeomData.Length;
			}
			if( oldGeomData.CornerRadius != newGeomData.CornerRadius ) {
				oldGeomData.CornerRadius = newGeomData.CornerRadius;
			}
			if( oldGeomData.IsCoordinateReversed != newGeomData.IsCoordinateReversed ) {
				oldGeomData.IsCoordinateReversed = newGeomData.IsCoordinateReversed;
			}
			if( oldGeomData.RotatedAngle_deg != newGeomData.RotatedAngle_deg ) {
				oldGeomData.RotatedAngle_deg = newGeomData.RotatedAngle_deg;
			}
		}

		void UpdatePolygonGeomData( PolygonGeomData oldGeomData, PolygonGeomData newGeomData )
		{
			if( oldGeomData == null || newGeomData == null ) {
				return;
			}
			if( oldGeomData.SideLength != newGeomData.SideLength ) {
				oldGeomData.SideLength = newGeomData.SideLength;
			}
			if( oldGeomData.CornerRadius != newGeomData.CornerRadius ) {
				oldGeomData.CornerRadius = newGeomData.CornerRadius;
			}
			if( oldGeomData.IsCoordinateReversed != newGeomData.IsCoordinateReversed ) {
				oldGeomData.IsCoordinateReversed = newGeomData.IsCoordinateReversed;
			}
			if( oldGeomData.RotatedAngle_deg != newGeomData.RotatedAngle_deg ) {
				oldGeomData.RotatedAngle_deg = newGeomData.RotatedAngle_deg;
			}
		}

		void UpdateRunwayGeomData( RunwayGeomData oldGeomData, RunwayGeomData newGeomData )
		{
			if( oldGeomData == null || newGeomData == null ) {
				return;
			}
			if( oldGeomData.Width != newGeomData.Width ) {
				oldGeomData.Width = newGeomData.Width;
			}
			if( oldGeomData.Length != newGeomData.Length ) {
				oldGeomData.Length = newGeomData.Length;
			}
			if( oldGeomData.IsCoordinateReversed != newGeomData.IsCoordinateReversed ) {
				oldGeomData.IsCoordinateReversed = newGeomData.IsCoordinateReversed;
			}
			if( oldGeomData.RotatedAngle_deg != newGeomData.RotatedAngle_deg ) {
				oldGeomData.RotatedAngle_deg = newGeomData.RotatedAngle_deg;
			}
		}

		PathObject CreatePathObject( string szID, TopoDS_Shape shape, IStdPatternGeomData standardPatternGeomData, ContourPathObject contourPathObject, PathObject originalPathObject )
		{
			if( contourPathObject == null ) {
				return null;
			}

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

		Viewer m_Viewer;
		List<string> m_szPathIDList = new List<string>();
		List<AIS_Trihedron> m_TrihedronList = new List<AIS_Trihedron>();
		IGeomData m_GeomData;
		PathType m_BackFirstPathType;
		Dictionary<string, PathObject> m_BackUpPathObjectList = new Dictionary<string, PathObject>();
		Dictionary<string, IGeomData> m_BackUpGeomDataList = new Dictionary<string, IGeomData>();
	}
}
