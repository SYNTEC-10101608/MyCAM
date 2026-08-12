using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using OCC.AIS;
using OCC.Bnd;
using OCC.BRepBndLib;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class DesignSupportBracketAction : EditActionBase
	{
		public DesignSupportBracketAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;
		}

		public override EditActionType ActionType => EditActionType.DesignSupportBracket;

		public override void Start()
		{
			base.Start();

			m_Dialog = new DesignSupportBracketDlg();
			m_Dialog.NextStep += OnNextStep;
			m_Dialog.BackStep += OnBackStep;
			m_Dialog.Generate += OnGenerate;
			m_Dialog.ParameterChanged += OnParameterChanged;
			m_Dialog.Cancel += OnDialogCancel;
			m_Dialog.SaveDxf += OnSaveDxf;
			m_Dialog.SkipSave += OnSkipSaveAndFinish;
			m_Dialog.Show( MyApp.MainForm );

			m_Dialog.BeginInvoke( new MethodInvoker( delegate
			{
				if( m_Dialog != null && !m_Dialog.IsDisposed ) {
					GoToStep( EStep.SelectFace );
				}
			} ) );
		}

		public override void End()
		{
			EndSubAction();
			RemovePreviewVisualization();

			if( m_Dialog != null ) {
				m_Dialog.NextStep -= OnNextStep;
				m_Dialog.BackStep -= OnBackStep;
				m_Dialog.Generate -= OnGenerate;
				m_Dialog.ParameterChanged -= OnParameterChanged;
				m_Dialog.Cancel -= OnDialogCancel;  // unsubscribe before Close() so base.RaiseCancel won't re-enter
				m_Dialog.SaveDxf -= OnSaveDxf;
				m_Dialog.SkipSave -= OnSkipSaveAndFinish;

				if( !m_Dialog.IsDisposed ) {
					m_Dialog.Close();
				}
				m_Dialog = null;
			}

			base.End();
		}

		void GoToStep( EStep step )
		{
			m_CurrentStep = step;
			switch( step ) {
				case EStep.SelectFace:
					m_Dialog?.ShowSelectFacePanel();
					StartSelectFaceSubAction();
					break;
				case EStep.InputParameters:
					m_Dialog?.ShowInputParametersPanel();
					m_Dialog?.TriggerInitialParameterChanged();
					break;
				default:
					break;
			}
		}

		void StartSelectFaceSubAction()
		{
			EndSubAction();
			m_FaceSelectionAction = new SelectFaceAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			m_FaceSelectionAction.SelectionChanged += OnFaceSelectionChanged;
			m_FaceSelectionAction.Start();
		}

		void EndSubAction()
		{
			if( m_FaceSelectionAction != null ) {
				m_FaceSelectionAction.SelectionChanged -= OnFaceSelectionChanged;
				m_FaceSelectionAction.End();
				m_FaceSelectionAction = null;
			}
		}

		void OnFaceSelectionChanged()
		{
			m_Dialog?.SetFaceSelected( m_FaceSelectionAction?.HasSelection ?? false );
		}

		void OnNextStep()
		{
			try {
				if( m_FaceSelectionAction != null ) {
					m_InputShell = GetFirstShell( m_FaceSelectionAction.GetResult() );
				}
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel(
					string.Format( m_LanguageManager.TryGetString( "Error_BuildShellException", DefaultError_BuildShellException ), ex.Message ),
					MyApp.NoticeType.Error, true );
				return;
			}

			if( m_InputShell == null || m_InputShell.IsNull() ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_BuildShellFailed", DefaultError_BuildShellFailed ), MyApp.NoticeType.Error, true );
				return;
			}

			EndSubAction();

			double[] xDefaults = ComputeQuarterPositions( m_InputShell, isX: true );
			double[] yDefaults = ComputeQuarterPositions( m_InputShell, isX: false );
			m_Dialog?.ResetPositionListsFromBBox( xDefaults, yDefaults );

			GoToStep( EStep.InputParameters );
		}

		double[] ComputeQuarterPositions( TopoDS_Shell shell, bool isX )
		{
			try {
				Bnd_Box bbox = new Bnd_Box();
				BRepBndLib.AddOptimal( shell, ref bbox );
				double xMin = 0, yMin = 0, zMin = 0, xMax = 0, yMax = 0, zMax = 0;
				bbox.Get( ref xMin, ref yMin, ref zMin, ref xMax, ref yMax, ref zMax );
				double axisMin = isX ? xMin : yMin;
				double axisMax = isX ? xMax : yMax;
				return new double[]
				{
					Math.Round( axisMin + ( axisMax - axisMin ) * 0.25, 1 ),
					Math.Round( axisMin + ( axisMax - axisMin ) * 0.50, 1 ),
					Math.Round( axisMin + ( axisMax - axisMin ) * 0.75, 1 )
				};
			}
			catch {
				// BBox computation failed; log and fall back to hardcoded defaults so the dialog still opens.
				MyApp.Logger.ShowOnLogPanel(
					m_LanguageManager.TryGetString( "Error_ComputeBBoxFailed", DefaultError_ComputeBBoxFailed ),
					MyApp.NoticeType.Warning );
				return new double[] { -50.0, 0.0, 50.0 };
			}
		}

		void OnBackStep()
		{
			m_InputShell = null;
			RemovePreviewVisualization();
			GoToStep( EStep.SelectFace );
			m_Dialog?.SetFaceSelected( false );
		}

		void OnParameterChanged( SupportBracketParams parameters )
		{
			if( m_CurrentStep != EStep.InputParameters ) {
				return;
			}

			if( m_InputShell == null || m_InputShell.IsNull() ) {
				return;
			}

			if( double.IsNaN( parameters.XExtension ) || double.IsNaN( parameters.YExtension ) ||
				double.IsNaN( parameters.ZExtension ) || double.IsNaN( parameters.RibHeight ) ) {
				return;
			}

			double[] xPositions = parameters.XPositions ?? new double[ 0 ];
			double[] yPositions = parameters.YPositions ?? new double[ 0 ];
			if( xPositions.Length == 0 || yPositions.Length == 0 ) {
				return;
			}

			if( m_InputShell == null || m_InputShell.IsNull() ) {
				return;
			}

			DrawPreviewVisualization( m_InputShell, xPositions, yPositions,
				parameters.XExtension, parameters.YExtension, parameters.ZExtension, parameters.RibHeight );
		}

		void OnGenerate( SupportBracketParams parameters )
		{
			// Validate parameters and prepare for support generation
			ValidatedGenerateParams validated;
			if( !TryValidateGenerateParams( parameters, out validated ) ) {
				return;
			}

			TopoDS_Shape outputCompound;
			if( !TryBuildSupportCompound( validated, out outputCompound ) ) {
				return;
			}

			// Remove any existing preview visualization before generating the final support structure
			RemovePreviewVisualization();

			m_DataManager.AddAdditionalPart( outputCompound );

			// If support generation was successful, prompt the user to save the DXF files; otherwise, end the action
			if( m_LastSupportResult != null ) {
				string defaultOutputDir = Path.Combine(
					System.AppDomain.CurrentDomain.BaseDirectory,
					CUSTOM_ROOT_FOLDER_NAME, SUPPORT_BRACKET_FOLDER_NAME );
				m_Dialog?.ShowSaveDxfPanel( defaultOutputDir );
			}
			else {
				End();
			}
		}

		bool TryValidateGenerateParams( SupportBracketParams parameters, out ValidatedGenerateParams validated )
		{
			validated = default( ValidatedGenerateParams );

			if( m_InputShell == null || m_InputShell.IsNull() ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_NoFaceSelected", DefaultError_NoFaceSelected ), MyApp.NoticeType.Error, true );
				return false;
			}

			double xExtension, yExtension, baseThickness;
			double engagementDepth, wallThickness, ribHeight, zExtension, pinLength;
			int xPinCount, yPinCount;

			// Validate all parameters and report errors if any are invalid
			if( !ValidateDouble( parameters.XExtension, m_Dialog.XExtensionName, out xExtension ) ) {
				return false;
			}
			if( !ValidateDouble( parameters.YExtension, m_Dialog.YExtensionName, out yExtension ) ) {
				return false;
			}
			if( !ValidateDouble( parameters.BaseThick, m_Dialog.BaseThicknessName, out baseThickness ) ) {
				return false;
			}

			if( !ValidateDouble( parameters.CrossGap, m_Dialog.EngagementDepthName, out engagementDepth ) ) {
				return false;
			}
			if( !ValidatePositiveInt( parameters.XPinCount, m_Dialog.XPinCountName, out xPinCount ) ) {
				return false;
			}
			if( !ValidatePositiveInt( parameters.YPinCount, m_Dialog.YPinCountName, out yPinCount ) ) {
				return false;
			}
			if( !ValidateDouble( parameters.WallThick, m_Dialog.WallThicknessName, out wallThickness ) ) {
				return false;
			}
			if( !ValidateDouble( parameters.RibHeight, m_Dialog.RibHeightName, out ribHeight ) ) {
				return false;
			}
			if( !ValidateDouble( parameters.ZExtension, m_Dialog.ZExtensionName, out zExtension ) ) {
				return false;
			}
			if( !ValidateDouble( parameters.PinLength, m_Dialog.PinLengthName, out pinLength ) ) {
				return false;
			}

			double[] xPositions = parameters.XPositions ?? new double[ 0 ];
			double[] yPositions = parameters.YPositions ?? new double[ 0 ];

			// Ensure that there is at least one position in each axis; otherwise, report an error
			if( xPositions.Length == 0 || yPositions.Length == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_PositionCount", DefaultError_PositionCount ), MyApp.NoticeType.Error, true );
				return false;
			}

			// Validate the input shell and ensure it is not null or invalid; otherwise, report an error
			TopoDS_Shell inputShell = m_InputShell;
			if( inputShell == null || inputShell.IsNull() ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_BuildShellFailed", DefaultError_BuildShellFailed ), MyApp.NoticeType.Error, true );
				return false;
			}

			Array.Sort( xPositions );
			Array.Sort( yPositions );

			if( !ValidatePositions( inputShell, xPositions, yPositions ) ) {
				return false;
			}

			validated = new ValidatedGenerateParams(
				inputShell, xPositions, yPositions,
				xExtension, yExtension, zExtension,
				baseThickness, engagementDepth, wallThickness,
				ribHeight, pinLength, xPinCount, yPinCount );
			return true;
		}

		bool TryBuildSupportCompound( ValidatedGenerateParams p, out TopoDS_Shape outputCompound )
		{
			outputCompound = null;
			m_LastSupportResult = null;

			// Fill holes in the input shell to ensure a watertight geometry for support generation
			TopoDS_Shape filledShape;
			if( !SupportDesignHelper.FillShellHoles( p.InputShell, out filledShape ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_FillShellFailed", DefaultError_FillShellFailed ), MyApp.NoticeType.Error, true );
				return false;
			}

			// Sanity check: FillShellHoles occasionally produces oversized planar faces (e.g. unbounded planes
			// used to close side openings) that inflate the bounding box far beyond the original workpiece.
			// When that happens the downstream support geometry uses the wrong extents and results are unusable.
			if( !IsFilledBBoxWithinTolerance( p.InputShell, filledShape ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_FillShellBBoxDiverged", DefaultError_FillShellBBoxDiverged ), MyApp.NoticeType.Error, true );
				return false;
			}

			// Extract a shell from the filled shape for further processing
			TopoDS_Shell filledShell = GetShellFromShape( filledShape );
			if( filledShell == null || filledShell.IsNull() ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_ExtractShellFailed", DefaultError_ExtractShellFailed ), MyApp.NoticeType.Error, true );
				return false;
			}

			SupportStructureResult result;
			SupportBuildError buildError;
			if( !SupportDesignHelper.ComputeShellWithSectionWires(
				filledShell, p.XPositions, p.YPositions,
				p.ZExtension, p.XExtension, p.YExtension,
				p.WallThickness, p.EngagementDepth, p.RibHeight,
				FILLET_RADIUS, p.BaseThickness, p.PinLength,
				p.XPinCount, p.YPinCount,
				out result, out buildError ) ) {
				string msg = ( buildError == SupportBuildError.XZWallsFailed
						|| buildError == SupportBuildError.YZWallsFailed
						|| buildError == SupportBuildError.BasePlateFailed )
					? m_LanguageManager.TryGetString( "Error_SupportBuildFailed", DefaultError_SupportBuildFailed )
					: m_LanguageManager.TryGetString( "Error_SectionCalculationFailed", DefaultError_SectionCalculationFailed );
				MyApp.Logger.ShowOnLogPanel( msg, MyApp.NoticeType.Error, true );
				return false;
			}

			outputCompound = result.AllSupportCompound;
			m_LastSupportResult = result;
			return true;
		}

		void OnSaveDxf( string outputDir )
		{
			if( m_LastSupportResult == null ) {
				End();
				return;
			}

			if( string.IsNullOrWhiteSpace( outputDir ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_NoSavePath", DefaultError_NoSavePath ), MyApp.NoticeType.Error, true );
				return;
			}

			if( !SupportDesignHelper.ExportSupportsToDxf( m_LastSupportResult, outputDir ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_DxfExportFailed", DefaultError_DxfExportFailed ), MyApp.NoticeType.Error, true );
				return;
			}

			MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "Log_DxfSaved", DefaultLog_DxfSaved ), outputDir ), MyApp.NoticeType.Hint );
			End();
		}

		void OnSkipSaveAndFinish()
		{
			End();
		}

		void OnDialogCancel()
		{
			End();
		}

		static TopoDS_Shell GetFirstShell( List<TopoDS_Shape> shapes )
		{
			if( shapes == null ) {
				return null;
			}

			foreach( TopoDS_Shape shape in shapes ) {
				if( shape != null && !shape.IsNull()
					&& shape.ShapeType() == TopAbs_ShapeEnum.TopAbs_SHELL ) {
					return TopoDS.ToShell( shape );
				}
			}
			return null;
		}

		static TopoDS_Shell GetShellFromShape( TopoDS_Shape shape )
		{
			if( shape == null || shape.IsNull() ) {
				return null;
			}

			// if the shape is already a shell, return it directly
			if( shape.ShapeType() != TopAbs_ShapeEnum.TopAbs_SHELL ) {
				TopExp_Explorer exp = new TopExp_Explorer( shape, TopAbs_ShapeEnum.TopAbs_SHELL );
				if( !exp.More() ) {
					return null;
				}
				return TopoDS.ToShell( exp.Current() );
			}

			return TopoDS.ToShell( shape );
		}

		static bool IsFilledBBoxWithinTolerance( TopoDS_Shell originalShell, TopoDS_Shape filledShape )
		{
			try {
				if( originalShell == null || originalShell.IsNull() ||
					filledShape == null || filledShape.IsNull() ) {
					return false;
				}

				// Compute bounding boxes for both the original shell and the filled shape, to ensure they are within acceptable tolerances.
				double originalXMin = 0, originalYMin = 0, originalZMin = 0;
				double originalXMax = 0, originalYMax = 0, originalZMax = 0;
				Bnd_Box originalBBox = new Bnd_Box();
				BRepBndLib.AddOptimal( originalShell, ref originalBBox );
				originalBBox.Get( ref originalXMin, ref originalYMin, ref originalZMin,
					ref originalXMax, ref originalYMax, ref originalZMax );

				// Compute bounding box for the filled shape
				double filledXMin = 0, filledYMin = 0, filledZMin = 0;
				double filledXMax = 0, filledYMax = 0, filledZMax = 0;
				Bnd_Box filledBBox = new Bnd_Box();
				BRepBndLib.AddOptimal( filledShape, ref filledBBox );
				filledBBox.Get( ref filledXMin, ref filledYMin, ref filledZMin,
					ref filledXMax, ref filledYMax, ref filledZMax );

				// Check if the filled bounding box dimensions are within the allowed expansion ratio of the original bounding box dimensions.
				return IsAxisWithinTolerance( originalXMax - originalXMin, filledXMax - filledXMin )
					&& IsAxisWithinTolerance( originalYMax - originalYMin, filledYMax - filledYMin )
					&& IsAxisWithinTolerance( originalZMax - originalZMin, filledZMax - filledZMin );
			}
			catch {
				// If bbox computation itself throws, treat as diverged so caller aborts safely.
				return false;
			}
		}

		static bool IsAxisWithinTolerance( double originalSpan, double filledSpan )
		{
			// Guard against degenerate original span (nearly zero); treat any measurable filled span as divergence.
			if( originalSpan <= GEOMETRY_TOLERANCE ) {
				return filledSpan <= GEOMETRY_TOLERANCE;
			}
			double maxAllowed = originalSpan * ( 1.0 + BBOX_MAX_EXPAND_RATIO );
			return filledSpan <= maxAllowed;
		}

		bool ValidatePositions( TopoDS_Shell inputShell, double[] xPositions, double[] yPositions )
		{
			double xMin = 0, xMax = 0, yMin = 0, yMax = 0, zMin = 0, zMax = 0;
			try {
				Bnd_Box bbox = new Bnd_Box();
				BRepBndLib.AddOptimal( inputShell, ref bbox );
				bbox.Get( ref xMin, ref yMin, ref zMin, ref xMax, ref yMax, ref zMax );
			}
			catch {
				// BBox computation failed (e.g. degenerate geometry); log and treat positions as unvalidatable.
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "Error_ComputeBBoxFailed", DefaultError_ComputeBBoxFailed ), MyApp.NoticeType.Error, true );
				return false;
			}

			bool xOutOfRange = HasOutOfRangePosition( xPositions, xMin, xMax );
			bool yOutOfRange = HasOutOfRangePosition( yPositions, yMin, yMax );
			bool xDuplicate = HasDuplicatePosition( xPositions );
			bool yDuplicate = HasDuplicatePosition( yPositions );

			// If there are no errors or duplicates, return true
			if( !xOutOfRange && !yOutOfRange && !xDuplicate && !yDuplicate ) {
				return true;
			}

			// Construct error messages for all detected issues
			var errorMessages = new List<string>();
			if( xDuplicate ) {
				errorMessages.Add( m_LanguageManager.TryGetString( "Error_XPositionDuplicate", DefaultError_XPositionDuplicate ) );
			}
			if( yDuplicate ) {
				errorMessages.Add( m_LanguageManager.TryGetString( "Error_YPositionDuplicate", DefaultError_YPositionDuplicate ) );
			}
			if( xOutOfRange ) {
				errorMessages.Add( m_LanguageManager.TryGetString( "Error_XPositionOutOfRange", DefaultError_XPositionOutOfRange ) );
			}
			if( yOutOfRange ) {
				errorMessages.Add( m_LanguageManager.TryGetString( "Error_YPositionOutOfRange", DefaultError_YPositionOutOfRange ) );
			}
			string errorMessage = string.Join( System.Environment.NewLine, errorMessages.ToArray() )
				+ m_LanguageManager.TryGetString( "Error_PositionSuffix", DefaultError_PositionSuffix );
			MyApp.Logger.ShowOnLogPanel( errorMessage, MyApp.NoticeType.Error, true );
			return false;
		}

		bool HasOutOfRangePosition( double[] positions, double axisMin, double axisMax )
		{
			for( int i = 0; i < positions.Length; i++ ) {
				double v = positions[ i ];
				if( v < axisMin - POSITION_DUPLICATE_TOL || v > axisMax + POSITION_DUPLICATE_TOL ) {
					return true;
				}
			}
			return false;
		}

		bool HasDuplicatePosition( double[] positions )
		{
			// Already sort, only need to check next index is duplicate or not
			for( int i = 0; i < positions.Length - 1; i++ ) {
				if( System.Math.Abs( positions[ i ] - positions[ i + 1 ] ) <= POSITION_DUPLICATE_TOL ) {
					return true;
				}
			}
			return false;
		}

		// Reports an error and returns false when value is NaN
		bool ValidateDouble( double value, string fieldName, out double validated )
		{
			if( double.IsNaN( value ) ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "Error_InvalidParamFormat", DefaultError_InvalidParamFormat ), fieldName ), MyApp.NoticeType.Error, true );
				validated = 0;
				return false;
			}
			validated = value;
			return true;
		}

		// Reports an error and returns false when value is 0
		bool ValidatePositiveInt( int value, string fieldName, out int validated )
		{
			if( value <= 0 ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "Error_InvalidIntParamFormat", DefaultError_InvalidIntParamFormat ), fieldName ), MyApp.NoticeType.Error, true );
				validated = 0;
				return false;
			}
			validated = value;
			return true;
		}

		#region Preview Visualization

		void RemovePreviewVisualization( bool updateView = true )
		{
			if( m_PreviewAISList == null || m_PreviewAISList.Count == 0 ) {
				return;
			}

			foreach( AIS_Shape ais in m_PreviewAISList ) {
				m_Viewer.GetAISContext().Remove( ais, false );
			}
			m_PreviewAISList.Clear();
			if( updateView ) {
				m_Viewer.UpdateView();
			}
		}

		void DrawPreviewVisualization( TopoDS_Shell inputShell, double[] xPositions, double[] yPositions, double xExtension, double yExtension, double zExtension, double ribHeight )
		{
			RemovePreviewVisualization( updateView: false );

			if( !SupportDesignHelper.ComputePreviewPlanes(
			  inputShell, xPositions, yPositions, xExtension, yExtension, zExtension, ribHeight,
				out List<PreviewPlaneData> xzWallPlanes,
				out List<PreviewPlaneData> yzWallPlanes,
				out PreviewPlaneData basePlanePreview ) ) {
				return;
			}

			if( m_PreviewAISList == null ) {
				m_PreviewAISList = new List<AIS_Shape>();
			}

			foreach( PreviewPlaneData plane in xzWallPlanes ) {
				AIS_Shape planeAIS = DrawHelper.CreatePlaneAISOnXZPlane( plane.UMin, plane.UMax, plane.VMin, plane.VMax, plane.FixedCoord, Quantity_NameOfColor.Quantity_NOC_ROYALBLUE1 );
				DisplayPreviewAIS( planeAIS );
			}

			foreach( PreviewPlaneData plane in yzWallPlanes ) {
				AIS_Shape planeAIS = DrawHelper.CreatePlaneAISOnYZPlane( plane.UMin, plane.UMax, plane.VMin, plane.VMax, plane.FixedCoord, Quantity_NameOfColor.Quantity_NOC_LIMEGREEN );
				DisplayPreviewAIS( planeAIS );
			}

			if( basePlanePreview != null ) {
				AIS_Shape planeAIS = DrawHelper.CreatePlaneAIS( basePlanePreview.UMin, basePlanePreview.UMax, basePlanePreview.VMin, basePlanePreview.VMax, basePlanePreview.FixedCoord );
				DisplayPreviewAIS( planeAIS );
			}

			m_Viewer.UpdateView();
		}

		void DisplayPreviewAIS( AIS_Shape planeAIS )
		{
			if( planeAIS == null ) {
				return;
			}
			m_Viewer.GetAISContext().Display( planeAIS, false );
			m_Viewer.GetAISContext().Deactivate( planeAIS );
			m_PreviewAISList.Add( planeAIS );
		}

		#endregion

		enum EStep
		{
			SelectFace,
			InputParameters,
			SaveDxf
		}

		struct ValidatedGenerateParams
		{
			public TopoDS_Shell InputShell;
			public double[] XPositions;
			public double[] YPositions;
			public double XExtension;
			public double YExtension;
			public double ZExtension;
			public double BaseThickness;
			public double EngagementDepth;
			public double WallThickness;
			public double RibHeight;
			public double PinLength;
			public int XPinCount;
			public int YPinCount;

			public ValidatedGenerateParams(
				TopoDS_Shell inputShell,
				double[] xPositions, double[] yPositions,
				double xExtension, double yExtension, double zExtension,
				double baseThickness, double engagementDepth, double wallThickness,
				double ribHeight, double pinLength, int xPinCount, int yPinCount )
			{
				InputShell = inputShell;
				XPositions = xPositions;
				YPositions = yPositions;
				XExtension = xExtension;
				YExtension = yExtension;
				ZExtension = zExtension;
				BaseThickness = baseThickness;
				EngagementDepth = engagementDepth;
				WallThickness = wallThickness;
				RibHeight = ribHeight;
				PinLength = pinLength;
				XPinCount = xPinCount;
				YPinCount = yPinCount;
			}
		}

		EStep m_CurrentStep;
		SupportStructureResult m_LastSupportResult;

		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;

		DesignSupportBracketDlg m_Dialog;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "DesignSupportBracketAction" );
		SelectFaceAction m_FaceSelectionAction;
		TopoDS_Shell m_InputShell;
		List<AIS_Shape> m_PreviewAISList;

		const double FILLET_RADIUS = 1.0;
		const double POSITION_DUPLICATE_TOL = 1e-3;
		const double GEOMETRY_TOLERANCE = 1e-3;
		const double BBOX_MAX_EXPAND_RATIO = 0.10;  // allow up to 10% bbox growth per axis after hole filling

		const string CUSTOM_ROOT_FOLDER_NAME = "Custom";
		const string SUPPORT_BRACKET_FOLDER_NAME = "SupportBracket";

		const string DefaultError_NoFaceSelected = "請先選擇面";
		const string DefaultError_PositionCount = "支架數量不能為0";
		const string DefaultError_BuildShellFailed = "無法建立支撐面，請確認已選取足夠且相連的面";
		const string DefaultError_FillShellFailed = "填補孔洞失敗，此工件暫不支援此功能";
		const string DefaultError_FillShellBBoxDiverged = "填補孔洞後幾何範圍落差過大，此工件暫不支援此功能";
		const string DefaultError_ExtractShellFailed = "填補孔洞後無法取得曲面，此工件暫不支援此功能";
		const string DefaultError_SectionCalculationFailed = "截面計算失敗，請調整 X／Y 支架座標，避開孔洞或凹陷處";
		const string DefaultError_SupportBuildFailed = "目前曲面的選擇無法生成支架，請更換所選的面";
		const string DefaultError_NoSavePath = "請指定儲存路徑";
		const string DefaultError_DxfExportFailed = "DXF 匯出失敗";
		const string DefaultLog_DxfSaved = "DXF 已儲存至：{0}";
		const string DefaultError_ComputeBBoxFailed = "無法計算工件範圍";
		const string DefaultError_XPositionDuplicate = "X方向支架座標有重複";
		const string DefaultError_YPositionDuplicate = "Y方向支架座標有重複";
		const string DefaultError_XPositionOutOfRange = "X方向支架座標超出工件範圍";
		const string DefaultError_YPositionOutOfRange = "Y方向支架座標超出工件範圍";
		const string DefaultError_PositionSuffix = "，請修正後再試。";
		const string DefaultError_InvalidParamFormat = "參數格式錯誤：{0}";
		const string DefaultError_InvalidIntParamFormat = "參數格式錯誤：{0}（需為正整數）";
		const string DefaultError_BuildShellException = "面縫合時發生例外錯誤：{0}";
	}

	internal class SupportBracketParams
	{
		// --- Bottom section ---
		public double XExtension
		{
			get; set;
		} = 50.0;

		public double YExtension
		{
			get; set;
		} = 50.0;

		public double BaseThick
		{
			get; set;
		} = 5.0;

		// --- Bracket positions ---
		public double[] XPositions
		{
			get; set;
		} = new double[ 0 ];

		public double[] YPositions
		{
			get; set;
		} = new double[ 0 ];

		// --- Bracket section ---
		public double CrossGap
		{
			get; set;
		} = 5.0;

		public int XPinCount
		{
			get; set;
		} = 8;

		public int YPinCount
		{
			get; set;
		} = 2;

		public double WallThick
		{
			get; set;
		} = 3.0;

		public double RibHeight
		{
			get; set;
		} = 50.0;

		public double ZExtension
		{
			get; set;
		} = 100.0;

		public double PinLength
		{
			get; set;
		} = 20.0;
	}
}
