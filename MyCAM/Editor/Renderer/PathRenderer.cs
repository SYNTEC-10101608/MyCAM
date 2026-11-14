using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MyCAM.Editor
{
	internal class PathRenderer : CAMRendererBase
	{
		readonly Dictionary<string, AIS_Shape> m_OriginalPathAISDict = new Dictionary<string, AIS_Shape>();

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
			}
			else {
				foreach( var kvp in m_OriginalPathAISDict ) {
					m_Viewer.GetAISContext().Display( kvp.Value, false );
					m_Viewer.GetAISContext().Deactivate( kvp.Value );
				}
			}
		}

		public override void Show( bool bUpdate = false )
		{
			if( m_IsPauseRefreshAndHide || m_IsPauseRefresh ) {
				return;
			}
			Remove();
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( IReadOnlyList<string> pathIDList, bool bUpdate = false )
		{
			List<string> detailIDs = m_DataManager.GetEditablePathIDList( pathIDList.ToList() );
			ShowSpecifyPath( detailIDs, bUpdate );
		}

		public override void Remove( bool bUpdate = false )
		{
			RemovePaths( m_OriginalPathAISDict.Keys.ToList() );
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void Remove( IReadOnlyList<string> pathIDList, bool bUpdate = false )
		{
			List<string> detailIDs = m_DataManager.GetEditablePathIDList( pathIDList.ToList() ).Union( pathIDList ).ToList();
			RemovePaths( detailIDs );
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void ShowTrans( gp_Trsf trsf, bool bUpdate = false )
		{
			List<string> detailIDs = m_DataManager.GetEditablePathIDList( m_DataManager.PathIDList.ToList() );
			ShowSpecifyPath( detailIDs, bUpdate, trsf );
		}

		void ShowSpecifyPath( IReadOnlyList<string> pathIDList, bool bUpdate, gp_Trsf trsf = null )
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

		void RemovePaths( IReadOnlyList<string> pathIDList )
		{
			foreach( string pathID in pathIDList ) {
				if( m_OriginalPathAISDict.TryGetValue( pathID, out AIS_Shape oriPathAIS ) ) {
					m_Viewer.GetAISContext().Remove( oriPathAIS, false );
					m_OriginalPathAISDict.Remove( pathID );
				}
			}
		}

		IReadOnlyList<gp_Pnt> GetPathOriginalCADPointList( string pathID )
		{
			IReadOnlyList<gp_Pnt> pathlist = new List<gp_Pnt>();
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return new List<gp_Pnt>();
			}
			if( !DataGettingHelper.GetPathObject( pathID, out IPathObject pathObject ) ) {
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
