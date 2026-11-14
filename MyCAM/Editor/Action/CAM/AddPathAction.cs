using System.Collections.Generic;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using OCC.gp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class AddPathAction : EditActionBase
	{
		public AddPathAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;
			m_LanguageManager = new LanguageManager.LanguageManager( "AddPathAction" );
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.AddPath;
			}
		}

		public override void Start()
		{
			base.Start();

			m_AddPathDlg = new AddPathDlg();
			m_AddPathDlg.AutoFindPathConfirm += OnAutoFindPathConfirm;
			m_AddPathDlg.StartSelectFace += OnStartSelectFace;
			m_AddPathDlg.FreezeSelectFace += OnFreezeSelectFace;
			m_AddPathDlg.AutoFromFaceConfirm += OnAutoFromFaceConfirm;
			m_AddPathDlg.StartWireSelect += OnStartWireSelect;
			m_AddPathDlg.WireConfirm += OnWireConfirm;
			m_AddPathDlg.Cancel += OnDlgCancel;
			m_AddPathDlg.CancelSubAction += EndManualSubAction;
			m_AddPathDlg.D1ThresholdChanged += OnD1ThresholdChanged;
			m_AddPathDlg.Show( MyApp.MainForm );
		}

		public override void End()
		{
			EndManualSubAction();

			if( m_AddPathDlg != null ) {
				m_AddPathDlg.AutoFindPathConfirm -= OnAutoFindPathConfirm;
				m_AddPathDlg.StartSelectFace -= OnStartSelectFace;
				m_AddPathDlg.FreezeSelectFace -= OnFreezeSelectFace;
				m_AddPathDlg.AutoFromFaceConfirm -= OnAutoFromFaceConfirm;
				m_AddPathDlg.StartWireSelect -= OnStartWireSelect;
				m_AddPathDlg.WireConfirm -= OnWireConfirm;
				m_AddPathDlg.Cancel -= OnDlgCancel;
				m_AddPathDlg.CancelSubAction -= EndManualSubAction;
				m_AddPathDlg.D1ThresholdChanged -= OnD1ThresholdChanged;

				// Cancel is already unsubscribed above, so closing here will not re-enter OnDlgCancel.
				if( !m_AddPathDlg.IsDisposed ) {
					m_AddPathDlg.Close();
				}
				m_AddPathDlg = null;
			}

			base.End();
		}

		// Hat/Tube confirmed: run the corresponding auto path generation.
		void OnAutoFindPathConfirm( EAddPathMode mode )
		{
			switch( mode ) {
				case EAddPathMode.Hat:
					AutoFindHatPath();
					break;
				case EAddPathMode.Tube:
					AutoFindTubePath();
					break;
				default:
					return;
			}
			End();
		}

		void OnStartSelectFace()
		{
			EndManualSubAction();
			m_SelectFaceAction = new SelectFaceAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager,
				m_AddPathDlg.CurrentD1ThresholdDegrees );
			m_SelectFaceAction.SelectionChanged += OnFaceSelectionChanged;
			m_SelectFaceAction.Start();
		}

		void OnFaceSelectionChanged()
		{
			m_AddPathDlg?.SetFaceSelected( m_SelectFaceAction?.HasSelection == true );
		}

		void OnD1ThresholdChanged( double d1ThresholdDegrees )
		{
			m_SelectFaceAction?.SetD1ThresholdDegrees( d1ThresholdDegrees );
		}

		void OnFreezeSelectFace()
		{
			if( m_SelectFaceAction != null ) {
				m_SelectedFaceGroupList = m_SelectFaceAction.GetResult();
				m_SelectFaceAction.Freeze();
			}
		}

		string OnAutoFromFaceConfirm()
		{
			if( m_SelectFaceAction == null || m_SelectedFaceGroupList == null || m_SelectedFaceGroupList.Count == 0 ) {
				string errorMeesage = m_LanguageManager.TryGetString( "PleaseSelectFace", PLEASE_SELECT_FACE );
				MyApp.Logger.ShowOnLogPanel( errorMeesage, MyApp.NoticeType.Hint );
				return errorMeesage;
			}

			FindWireOnOuterShellHelper.WireFindingResult result =
				FindWireOnOuterShellHelper.ExtractFreeBoundariesFromFaceGroups( m_SelectedFaceGroupList );
			if( !result.IsSuccess ) {
				return FindWireOnOuterShellHelper.GetWireFindingErrorMessage( result.Error );
			}

			m_SelectFaceAction.End();
			m_SelectFaceAction = null;
			m_SelectedFaceGroupList = null;
			AddPathToDataManager( result.Wires, result.EdgeFaceMap );
			End();
			return null;
		}

		string OnStartWireSelect()
		{
			if( m_SelectedFaceGroupList == null || m_SelectedFaceGroupList.Count == 0 ) {
				return m_LanguageManager.TryGetString( "PleaseSelectFace", PLEASE_SELECT_FACE );
			}

			if( m_SelectFaceAction != null ) {
				m_SelectFaceAction.End();
				m_SelectFaceAction = null;
			}

			m_SelectWireAction = new SelectWireAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, m_SelectedFaceGroupList );
			m_SelectWireAction.SelectionChanged += OnWireSelectionChanged;
			m_SelectWireAction.Start();
			return null;
		}

		void OnWireSelectionChanged()
		{
			m_AddPathDlg?.SetWireSelected( m_SelectWireAction?.HasSelection == true );
		}

		string OnWireConfirm()
		{
			if( m_SelectWireAction == null ) {
				return m_LanguageManager.TryGetString( "PleaseSelectFace", PLEASE_SELECT_FACE );
			}
			string errorMessage = m_SelectWireAction.SelectDone(
				m_AddPathDlg.AdvancedParams.MaxChordError, m_AddPathDlg.AdvancedParams.MaxLineLength,
				m_AddPathDlg.AdvancedParams.DiscreteMethod );
			if( string.IsNullOrEmpty( errorMessage ) ) {
				m_SelectWireAction = null;
				m_SelectedFaceGroupList = null;
				End();
			}
			return errorMessage;
		}

		// dialog closed without confirming (or mode switched mid-flow): end any in-progress sub-action.
		void OnDlgCancel()
		{
			EndManualSubAction();

			// dialog is closing itself; drop the reference before ending this action to avoid double-Close.
			m_AddPathDlg = null;
			End();
		}

		void EndManualSubAction()
		{
			if( m_SelectFaceAction != null ) {
				m_SelectFaceAction.End();
				m_SelectFaceAction = null;
			}
			if( m_SelectWireAction != null ) {
				m_SelectWireAction.End();
				m_SelectWireAction = null;
			}
			m_SelectedFaceGroupList = null;
		}

		// the geometry align axis the workpiece is placed along, follows the machine tube direction
		EAlignAxis GetTargetAlignAxis()
		{
			if( DataGettingHelper.GetMachineData( out FiveAxisMachineData machineData ) ) {
				return RotaryAxisHelper.ConvertToAlignAxis( machineData.TubeDir );
			}
			return EAlignAxis.Z;
		}

		// Auto find top face along the target axis, then process D1 continuous faces and free boundaries
		void AutoFindHatPath()
		{
			BoundingBox bbox = GetVisibleWorkpieceBBox();
			if( bbox == null ) {
				return;
			}

			gp_Pnt bboxCenter = new gp_Pnt( bbox.XCenter, bbox.YCenter, bbox.ZCenter );
			List<gp_Ax1> alienatedStrategies = GeometryTool.BuildRayAxesFromBBox( ShapeTool.GetAxisDir( GetTargetAlignAxis() ), bbox );
			AutoFindOutermostFaceAndAddPath( alienatedStrategies, bboxCenter );
		}

		// Auto find outermost face along the radial axes of the target axis, then process D1 continuous faces and free boundaries
		void AutoFindTubePath()
		{
			BoundingBox bbox = GetVisibleWorkpieceBBox();
			if( bbox == null ) {
				return;
			}

			bool isGetSuccess = FindWireOnOuterShellHelper.TubeStrategies( bbox, GetTargetAlignAxis(), out gp_Pnt bboxCenter, out List<gp_Ax1> stretchedStrategies );
			if( isGetSuccess == false ) {
				return;
			}
			AutoFindOutermostFaceAndAddPath( stretchedStrategies, bboxCenter );
		}

		// Shared: auto find outermost face by ray strategies, then find D1 faces and add free boundary paths
		void AutoFindOutermostFaceAndAddPath( List<gp_Ax1> strategies, gp_Pnt referenceCenter )
		{
			List<TopoDS_Shape> partShapes = GetVisiblePartShapes();
			if( partShapes.Count == 0 ) {
				return;
			}
			if( m_AddPathDlg == null ) {
				return;
			}

			// D1Threshold in the UI is entered in degrees; GeometryTool.IsD1Cont expects radians.
			double d1ToleranceRad = m_AddPathDlg.CurrentD1ThresholdDegrees * System.Math.PI / 180.0;
			FindWireOnOuterShellHelper.WireFindingResult result =
				FindWireOnOuterShellHelper.FindWiresOnOuterShell( partShapes, strategies, referenceCenter, d1ToleranceRad );

			if( !result.IsSuccess ) {
				string errorMessage = FindWireOnOuterShellHelper.GetWireFindingErrorMessage( result.Error );
				MyApp.Logger.ShowOnLogPanel( errorMessage, MyApp.NoticeType.Hint );
				return;
			}

			AddPathToDataManager( result.Wires, result.EdgeFaceMap );
		}

		// Shared helper to commit wires to DataManager using current advanced params
		void AddPathToDataManager( List<TopoDS_Wire> wires, OCC.TopTools.TopTools_IndexedDataMapOfShapeListOfShape edgeFaceMap )
		{
			if( m_AddPathDlg == null ) {
				return;
			}
			m_DataManager.AddPath( wires, edgeFaceMap, true,
				m_AddPathDlg.AdvancedParams.MaxChordError, m_AddPathDlg.AdvancedParams.MaxLineLength,
				m_AddPathDlg.AdvancedParams.DiscreteMethod );
		}

		// get visible part shapes for path generation
		List<TopoDS_Shape> GetVisiblePartShapes()
		{
			List<TopoDS_Shape> shapeList = new List<TopoDS_Shape>();
			foreach( string partID in m_DataManager.PartIDList ) {
				if( !m_ViewManager.ViewObjectMap.ContainsKey( partID ) ) {
					continue;
				}
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				if( DataGettingHelper.GetShapeObject( partID, out IShapeObject shapeObject ) == false ) {
					continue;
				}
				shapeList.Add( shapeObject.Shape );
			}
			return shapeList;
		}

		BoundingBox GetVisibleWorkpieceBBox()
		{
			List<TopoDS_Shape> shapeList = GetVisiblePartShapes();
			if( shapeList.Count == 0 ) {
				return null;
			}
			TopoDS_Shape compound = ShapeTool.MakeCompound( shapeList );
			return new BoundingBox( compound );
		}

		readonly Viewer m_Viewer;
		readonly TreeView m_TreeView;
		readonly ViewManager m_ViewManager;
		readonly LanguageManager.LanguageManager m_LanguageManager;

		AddPathDlg m_AddPathDlg;
		SelectFaceAction m_SelectFaceAction;
		SelectWireAction m_SelectWireAction;
		List<TopoDS_Shape> m_SelectedFaceGroupList;

		const string PLEASE_SELECT_FACE = "[操作提醒]請先選擇面";
	}
}
