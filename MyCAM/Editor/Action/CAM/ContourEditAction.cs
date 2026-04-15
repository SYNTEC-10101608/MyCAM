using MyCAM.App;
using MyCAM.Data;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.Aspect;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class ContourEditAction : IndexSelectAction
	{
		public ContourEditAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID )
			: base( dataManager, viewer, treeView, viewManager, pathID )
		{
			if( !DataGettingHelper.GetCraftDataByID( pathID, out m_CraftData ) ) {
				throw new ArgumentException( "Cannot get CraftData by pathID: " + pathID );
			}
			if( !DataGettingHelper.GetContourCacheByID( pathID, out m_ContourCache ) ) {
				throw new ArgumentException( "Cannot get ContourCache by pathID: " + pathID );
			}
			m_PathIDList = new List<string>() { pathID };
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ContourEdit;
			}
		}

		// Raised after each data change so CAMEditor can refresh the view
		public Action PropertyChanged;

		public override void Start()
		{
			base.Start();

			m_nSelectIndex = NULL_SELECT_INDEX;
			m_Param = null;

			m_Dlg = new ContourEditDlg();
			m_Dlg.AddEditIndex += OnAddEditIndex;
			m_Dlg.RemoveEditIndex += OnRemoveEditIndex;
			m_Dlg.DisplacementChanged += OnDisplacementChanged;
			m_Dlg.Cancel += End;

			m_Dlg.ResetParam( m_Param );
			m_Dlg.Show( MyApp.MainForm );
		}

		public override void End()
		{
			UnlockHighLight();
			base.End();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( IsPausedSelectMode ) {
				return;
			}
			if( e.Button != MouseButtons.Left ) {
				return;
			}

			int? hit = GetSelectIndex( out TopoDS_Shape selectedVertex );
			int nSelectIndex = hit ?? NULL_SELECT_INDEX;
			if( nSelectIndex == NULL_SELECT_INDEX || nSelectIndex == m_nSelectIndex ) {
				return;
			}

			UnlockHighLight();
			LockHighLight( selectedVertex );
			OnSelectedIndexChanged( nSelectIndex );
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( IsPausedSelectMode ) {
				return;
			}
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		// ---- selection ----

		void OnSelectedIndexChanged( int nSelectIndex )
		{
			if( m_nSelectIndex == nSelectIndex ) {
				return;
			}
			m_nSelectIndex = nSelectIndex;

			if( nSelectIndex == NULL_SELECT_INDEX ) {
				m_Param = null;
				m_Dlg.ResetParam( null );
				return;
			}

			bool isModified = m_CraftData.CADPointModifyMap.ContainsKey( nSelectIndex );
			double dx = 0, dy = 0, dz = 0;
			if( isModified ) {
				CADPointModifyData data = m_CraftData.CADPointModifyMap[ nSelectIndex ];
				dx = data.DX;
				dy = data.DY;
				dz = data.DZ;
			}
			m_Param = new ContourEditParam( dx, dy, dz, isModified );
			m_Dlg.ResetParam( m_Param );
		}

		// ---- dialog events ----

		void OnAddEditIndex()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}
			if( m_Param != null ) {
				m_Param.IsModified = true;
			}
			// commit to data with current (zero) displacement
			double dx = m_Param?.DX ?? 0;
			double dy = m_Param?.DY ?? 0;
			double dz = m_Param?.DZ ?? 0;
			m_CraftData.SetCADPointModify( m_nSelectIndex, dx, dy, dz );
			PropertyChanged?.Invoke();
		}

		void OnRemoveEditIndex()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}
			if( m_Param != null ) {
				m_Param.IsModified = false;
				m_Param.DX = 0;
				m_Param.DY = 0;
				m_Param.DZ = 0;
			}
			m_CraftData.RemoveCADPointModify( m_nSelectIndex );
			PropertyChanged?.Invoke();
		}

		void OnDisplacementChanged( double dx, double dy, double dz )
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX || m_Param == null ) {
				return;
			}
			m_Param.DX = dx;
			m_Param.DY = dy;
			m_Param.DZ = dz;
			m_CraftData.SetCADPointModify( m_nSelectIndex, dx, dy, dz );
			PropertyChanged?.Invoke();
		}

		// ---- highlight helpers (mirror ToolVectorAction) ----

		void LockHighLight( TopoDS_Shape selectedVertex )
		{
			if( selectedVertex == null || selectedVertex.IsNull() ) {
				return;
			}
			m_HighLightPoint = new AIS_Shape( selectedVertex );

			Prs3d_PointAspect pointAspect = new Prs3d_PointAspect(
				Aspect_TypeOfMarker.Aspect_TOM_BALL,
				new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ),
				3.0f
			);
			m_HighLightPoint.Attributes().SetPointAspect( pointAspect );
			m_Viewer.GetAISContext().Display( m_HighLightPoint, false );
		}

		void UnlockHighLight()
		{
			if( m_HighLightPoint != null ) {
				m_Viewer.GetAISContext().Remove( m_HighLightPoint, false );
				m_HighLightPoint = null;
			}
		}

		// ---- fields ----

		int m_nSelectIndex = NULL_SELECT_INDEX;
		ContourEditParam m_Param = null;

		readonly CraftData m_CraftData;
		readonly ContourCache m_ContourCache;
		readonly List<string> m_PathIDList;

		ContourEditDlg m_Dlg = null;
		AIS_Shape m_HighLightPoint = null;

		const int NULL_SELECT_INDEX = -999;
	}
}
