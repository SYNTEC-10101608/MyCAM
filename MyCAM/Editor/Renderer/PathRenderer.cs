using MyCAM.Data;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.gp;
using OCC.Quantity;
using OCC.TCollection;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Editor
{
	internal class PathRenderer : CAMRendererBase
	{
		readonly Dictionary<string, AIS_Shape> m_OriginalPathAISDict = new Dictionary<string, AIS_Shape>();
		readonly Dictionary<string, List<AIS_TextLabel>> m_MicroJointLabelsDict = new Dictionary<string, List<AIS_TextLabel>>();

		public PathRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void SetPauseRefreshAndHide( bool isPause )
		{
			if( m_IsPauseRefreshAndHide == isPause ) {
				return;
			}
			base.SetPauseRefreshAndHide( isPause );
			if( isPause ) {
				foreach( var kvp in m_OriginalPathAISDict ) {
					m_Viewer.GetAISContext().Erase( kvp.Value, false );
				}
				foreach( var kvp in m_MicroJointLabelsDict ) {
					foreach( var label in kvp.Value ) {
						m_Viewer.GetAISContext().Erase( label, false );
					}
				}
			}
			else {
				foreach( var kvp in m_OriginalPathAISDict ) {
					m_Viewer.GetAISContext().Display( kvp.Value, false );
					m_Viewer.GetAISContext().Deactivate( kvp.Value );
				}
				foreach( var kvp in m_MicroJointLabelsDict ) {
					foreach( var label in kvp.Value ) {
						m_Viewer.GetAISContext().Display( label, false );
						m_Viewer.GetAISContext().Deactivate( label );
					}
				}
			}
		}

		public override void Show( bool bUpdate = false )
		{
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( List<string> pathIDList, bool bUpdate = false )
		{
			ShowSpecifyPath( pathIDList, bUpdate );
		}

		public override void Remove( bool bUpdate = false )
		{
			Remove( m_DataManager.PathIDList, bUpdate );
		}

		public void Remove( List<string> pathIDList, bool bUpdate = false )
		{
			RemovePaths( pathIDList );
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void ShowTrans( gp_Trsf trsf, bool bUpdate = false )
		{
			ShowSpecifyPath( m_DataManager.PathIDList, bUpdate, trsf );
		}

		void ShowSpecifyPath( List<string> pathIDList, bool bUpdate, gp_Trsf trsf = null )
		{
			if( m_IsPauseRefreshAndHide || m_IsPauseRefresh ) {
				return;
			}

			Remove( pathIDList );
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			foreach( string pathID in pathIDList ) {
				ShowOriginalPath( pathID, trsf );
				ShowMicroJointMarkers( pathID, trsf );
			}

			if( bUpdate ) {
				UpdateView();
			}
		}

		public void Reset( bool bUpdate = false )
		{
			gp_Trsf theTrsf = new gp_Trsf();
			ShowTrans( theTrsf, bUpdate );
			if( bUpdate ) {
				UpdateView();
			}
		}

		void RemovePaths( List<string> pathIDList )
		{
			foreach( string pathID in pathIDList ) {
				if( m_OriginalPathAISDict.TryGetValue( pathID, out AIS_Shape oriPathAIS ) ) {
					m_Viewer.GetAISContext().Remove( oriPathAIS, false );
					m_OriginalPathAISDict.Remove( pathID );
				}
			}

			foreach( string pathID in pathIDList ) {
				if( m_MicroJointLabelsDict.TryGetValue( pathID, out List<AIS_TextLabel> labels ) ) {
					foreach( var label in labels ) {
						m_Viewer.GetAISContext().Remove( label, false );
					}
					m_MicroJointLabelsDict.Remove( pathID );
				}
			}
		}

		IReadOnlyList<gp_Pnt> GetPathOriginalCADPointList( string pathID )
		{
			IReadOnlyList<gp_Pnt> pathlist = new List<gp_Pnt>();
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return new List<gp_Pnt>();
			}
			if( !DataGettingHelper.GetPathObject( pathID, out PathObject pathObject ) ) {
				return new List<gp_Pnt>();
			}
			if( pathType == PathType.Contour ) {
				ContourPathObject contourPathObject = pathObject as ContourPathObject;
				pathlist = contourPathObject?.GeomData.CADPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}
			else if( DataGettingHelper.IsStdPattern( pathType ) ) {
				StdPatternObjectBase stdPatternPathObject = pathObject as StdPatternObjectBase;
				pathlist = stdPatternPathObject?.ContourPathObject?.GeomData?.CADPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}
			else {
				return new List<gp_Pnt>();
			}
			return pathlist;
		}

		void ShowOriginalPath( string pathID, gp_Trsf trsf = null )
		{
			trsf = trsf ?? new gp_Trsf();
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				return;
			}

			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return;
			}

			if( pathType == PathType.Contour ) {
				if( craftData.CompensatedDistance == 0 && IsIdentityTransform( craftData.CumulativeTrsfMatrix ) ) {
					return;
				}
			}

			IReadOnlyList<gp_Pnt> originalPointList = GetPathOriginalCADPointList( pathID );
			if( originalPointList == null || originalPointList.Count < 2 ) {
				return;
			}

			TopoDS_Wire pathOriWire = ToolVecAndPathVisibleHelper.CreatePolylineWire( originalPointList );
			if( pathOriWire == null || pathOriWire.IsNull() ) {
				return;
			}

			AIS_Shape oriPathAIS = new AIS_Shape( pathOriWire );
			oriPathAIS.SetLocalTransformation( trsf );
			oriPathAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_DEEPSKYBLUE1 ) );
			oriPathAIS.SetWidth( 2.0 );

			m_OriginalPathAISDict.Add( pathID, oriPathAIS );
			m_Viewer.GetAISContext().Display( oriPathAIS, false );
			m_Viewer.GetAISContext().Deactivate( oriPathAIS );
		}

		void ShowMicroJointMarkers( string pathID, gp_Trsf trsf = null )
		{
			// get CAMPoint list with MicroJoint information
			List<CAMPoint> camPointList = GetCAMPointList( pathID );
			if( camPointList == null || camPointList.Count == 0 ) {
				return;
			}
			List<AIS_TextLabel> microJointLabels = new List<AIS_TextLabel>();

			// create markers for MicroJoint start points
			for( int i = 0; i < camPointList.Count; i++ ) {
				CAMPoint camPoint = camPointList[ i ];
				if( camPoint.IsMicroJointStart ) {
					DrawMicroJointMarrk( camPoint.Point, "MJ¡¶", Quantity_NameOfColor.Quantity_NOC_GREEN,
						trsf, microJointLabels );
				}
				if( camPoint.IsMicroJointEnd ) {
					DrawMicroJointMarrk( camPoint.Point, "MJ¡¿", Quantity_NameOfColor.Quantity_NOC_RED,
						trsf, microJointLabels );
				}
			}
			if( microJointLabels.Count > 0 ) {
				m_MicroJointLabelsDict.Add( pathID, microJointLabels );
			}
		}

		void DrawMicroJointMarrk( gp_Pnt point, string labelText, Quantity_NameOfColor color,
			gp_Trsf trsf, List<AIS_TextLabel> labelsList )
		{
			if( point == null ) {
				return;
			}

			// create text label
			AIS_TextLabel label = new AIS_TextLabel();
			label.SetText( new TCollection_ExtendedString( labelText ) );
			label.SetColor( new Quantity_Color( color ) );
			label.SetPosition( point );
			label.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
			if( trsf != null ) {
				label.SetLocalTransformation( trsf );
			}

			// add to list and display
			labelsList.Add( label );
			m_Viewer.GetAISContext().Display( label, false );

			// label should not be selectable
			m_Viewer.GetAISContext().Deactivate( label );
		}

		List<CAMPoint> GetCAMPointList( string pathID )
		{
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return null;
			}

			if( pathType == PathType.Contour ) {
				if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache contourCache ) ) {
					return null;
				}
				return contourCache.MainPathPointList;
			}
			else if( DataGettingHelper.IsStdPattern( pathType ) ) {
				if( !DataGettingHelper.GetStdPatternCacheByID( pathID, out IStdPatternCache stdPatternCache ) ) {
					return null;
				}
				return stdPatternCache.KeyCAMPointList?.Cast<CAMPoint>().ToList();
			}

			return null;
		}

		static bool IsIdentityTransform( gp_Trsf trsf )
		{
			const double TOLERANCE = 1e-3;
			if( trsf == null ) {
				return true;
			}

			// check if translation part is zero
			gp_XYZ translation = trsf.TranslationPart();
			if( Math.Abs( translation.X() ) > TOLERANCE ||
				Math.Abs( translation.Y() ) > TOLERANCE ||
				Math.Abs( translation.Z() ) > TOLERANCE ) {
				return false;
			}

			// check if scale factor is 1
			if( Math.Abs( trsf.ScaleFactor() - 1.0 ) > TOLERANCE ) {
				return false;
			}

			// check if rotation part is identity matrix
			gp_Mat rotationMatrix = trsf.GetRotation().GetMatrix();
			for( int i = 1; i <= 3; i++ ) {
				for( int j = 1; j <= 3; j++ ) {
					double expectedValue = ( i == j ) ? 1.0 : 0.0;
					if( Math.Abs( rotationMatrix.Value( i, j ) - expectedValue ) > TOLERANCE ) {
						return false;
					}
				}
			}
			return true;
		}
	}
}
