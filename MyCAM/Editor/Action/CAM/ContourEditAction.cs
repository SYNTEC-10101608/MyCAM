using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCC.Aspect;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System;
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
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ContourEdit;
			}
		}

		public Action PropertyChanged;

		public override void Start()
		{
			base.Start();

			// create dialog
			m_Dlg = new ContourEditDlg();
			m_Dlg.AddEditIndex += OnAddEditIndex;
			m_Dlg.RemoveEditIndex += OnRemoveEditIndex;
			m_Dlg.DisplacementChanged += OnDisplacementChanged;
			m_Dlg.Cancel += End;

			// default to first point
			OnSelectedIndexChanged( 0 );
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

		void OnSelectedIndexChanged( int nSelectIndex )
		{
			if( m_nSelectIndex == nSelectIndex ) {
				return;
			}
			m_nSelectIndex = nSelectIndex;

			// should not happen, but just in case
			if( nSelectIndex == NULL_SELECT_INDEX ) {
				m_Param = null;
				m_Dlg.ResetParam( null );
				return;
			}

			// refresh dialog
			GetParamFormIndex( nSelectIndex, out double dx, out double dy, out double dz, out bool isModified );
			m_Param = new ContourEditParam( dx, dy, dz, isModified );
			m_Dlg.ResetParam( m_Param );
		}

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

			// refresh dialog
			m_Dlg.ResetParam( m_Param );
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

			// refresh dialog
			m_Dlg.ResetParam( m_Param );
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

			// refresh dialog
			m_Dlg.ResetParam( m_Param );
		}

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

		void GetParamFormIndex( int nSelectIndex, out double dx, out double dy, out double dz, out bool isModified )
		{
			dx = 0;
			dy = 0;
			dz = 0;
			isModified = false;
			if( nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}
			isModified = m_CraftData.CADPointModifyMap.ContainsKey( nSelectIndex );
			if( isModified ) {
				CADPointModifyData data = m_CraftData.CADPointModifyMap[ nSelectIndex ];
				dx = data.DX;
				dy = data.DY;
				dz = data.DZ;
			}
			return;
		}

		// edit param
		int m_nSelectIndex = NULL_SELECT_INDEX;
		ContourEditParam m_Param = null;
		readonly CraftData m_CraftData;

		// dlg
		ContourEditDlg m_Dlg = null;

		// interactive highlight
		AIS_Shape m_HighLightPoint = null;

		// constants
		const int NULL_SELECT_INDEX = -999;
	}
}
