using MyCAM.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace MyCAM.Helper.LoadingBar
{
	internal enum LoadingText
	{
		Preparing, ImportTitle, PlacementTopic, FileTopicFormat, SimulationTitle, SimulationTopic,
		ReadStage, TransferStage, SewStage, PlacementStage, PathStage, CollisionStage,
		PrepareReader, ReadFile, TransferFile, CombineShape, ValidateShape, SewShape, AddPart,
		AnalyzeWorkpiece, PrepareWorkpiece, InspectWorkpiece, FindWorkpieceOutline, CompareWorkpieceOutlines,
		DetermineWorkpieceDirection, FindWorkpieceCenter, AdjustPlacement, ApplyPlacement,
		PreparePaths, AnalyzePathWorkpiece, ProcessPathWorkpiece, IdentifyPaths, CalculatePaths, UpdateDisplay, PathsComplete,
		PrepareSimulation, CalculateSimulation, CheckCollision, ApplySimulation, SummarizeCollisions, DisplaySimulation, SimulationComplete,
		StageFormat, AnalysisFormat, AnalysisPercentFormat, PathCountFormat, CollisionPercentFormat,
		SubmitStage, DisplayStage, DxfStage, SurfaceStage, ProjectionStage, ProjectStage,
		ConvertDxf, ConnectDxfContours, BuildTargetSurface, ProjectContours, BuildEdgeMap,
		ReadProject, RestoreObjects, RestoreSettings, ApplyProject, RestoreSupportBracket,
		ImportComplete, ItemCountFormat, ProjectTitle, TubeTopic, ProjectContourEdges, DisplayParts, DisplayPaths
	}

	internal sealed class LoadingProgressDisplay
	{
		public string Key { get; set; }
		public string Text { get; set; }
		public int Percent { get; set; }
		public bool IsComplete { get; set; }
	}

	internal static class LoadingHelper
	{
		// Display rules for one operation event, not an executable task.
		sealed class Definition
		{
			// Resource keys for the main stage and its visible step.
			public LoadingText Stage;
			public LoadingText Detail;

			// Zero-based visible step index and visible step count.
			public int Index;
			public int Count;

			// Overall progress range (0..100) assigned to the main stage.
			public int Start;
			public int End;

			// Nested analysis index; -1 means no nested analysis step.
			public int AnalysisIndex = -1;

			// Show detail only, without stage numbering.
			public bool IsSingle;

			public LoadingText? Counter;
		}

		// Ordered analysis events and their localized detail keys.
		sealed class AnalysisDefinition
		{
			public LoadingStep Step { get; }
			public LoadingText Detail { get; }

			public AnalysisDefinition( LoadingStep step, LoadingText detail )
			{
				Step = step;
				Detail = detail;
			}
		}

		static readonly LanguageManager.LanguageManager m_LanguageManager = CreateLanguageManager();

		// Fallback text is used only when the localized resource is missing.
		static readonly Dictionary<LoadingText, string> m_DefaultText = new Dictionary<LoadingText, string>() {
			{ LoadingText.Preparing, PREPARING },
			{ LoadingText.ImportTitle, IMPORT_TITLE },
			{ LoadingText.PlacementTopic, PLACEMENT_TOPIC },
			{ LoadingText.FileTopicFormat, FILE_TOPIC_FORMAT },
			{ LoadingText.SimulationTitle, SIMULATION_TITLE },
			{ LoadingText.SimulationTopic, SIMULATION_TOPIC },
			{ LoadingText.ReadStage, READ_STAGE },
			{ LoadingText.TransferStage, TRANSFER_STAGE },
			{ LoadingText.SewStage, SEW_STAGE },
			{ LoadingText.PlacementStage, PLACEMENT_STAGE },
			{ LoadingText.PathStage, PATH_STAGE },
			{ LoadingText.CollisionStage, COLLISION_STAGE },
			{ LoadingText.PrepareReader, PREPARE_READER },
			{ LoadingText.ReadFile, READ_FILE },
			{ LoadingText.TransferFile, TRANSFER_FILE },
			{ LoadingText.CombineShape, COMBINE_SHAPE },
			{ LoadingText.ValidateShape, VALIDATE_SHAPE },
			{ LoadingText.SewShape, SEW_SHAPE },
			{ LoadingText.AddPart, ADD_PART },
			{ LoadingText.AnalyzeWorkpiece, ANALYZE_WORKPIECE },
			{ LoadingText.PrepareWorkpiece, PREPARE_WORKPIECE },
			{ LoadingText.InspectWorkpiece, INSPECT_WORKPIECE },
			{ LoadingText.FindWorkpieceOutline, FIND_WORKPIECE_OUTLINE },
			{ LoadingText.CompareWorkpieceOutlines, COMPARE_WORKPIECE_OUTLINES },
			{ LoadingText.DetermineWorkpieceDirection, DETERMINE_WORKPIECE_DIRECTION },
			{ LoadingText.FindWorkpieceCenter, FIND_WORKPIECE_CENTER },
			{ LoadingText.AdjustPlacement, ADJUST_PLACEMENT },
			{ LoadingText.ApplyPlacement, APPLY_PLACEMENT },
			{ LoadingText.PreparePaths, PREPARE_PATHS },
			{ LoadingText.AnalyzePathWorkpiece, ANALYZE_PATH_WORKPIECE },
			{ LoadingText.ProcessPathWorkpiece, PROCESS_PATH_WORKPIECE },
			{ LoadingText.IdentifyPaths, IDENTIFY_PATHS },
			{ LoadingText.CalculatePaths, CALCULATE_PATHS },
			{ LoadingText.UpdateDisplay, UPDATE_DISPLAY },
			{ LoadingText.PathsComplete, PATHS_COMPLETE },
			{ LoadingText.PrepareSimulation, PREPARE_SIMULATION },
			{ LoadingText.CalculateSimulation, CALCULATE_SIMULATION },
			{ LoadingText.CheckCollision, CHECK_COLLISION },
			{ LoadingText.ApplySimulation, APPLY_SIMULATION },
			{ LoadingText.SummarizeCollisions, SUMMARIZE_COLLISIONS },
			{ LoadingText.DisplaySimulation, DISPLAY_SIMULATION },
			{ LoadingText.SimulationComplete, SIMULATION_COMPLETE },
			{ LoadingText.StageFormat, STAGE_FORMAT },
			{ LoadingText.AnalysisFormat, ANALYSIS_FORMAT },
			{ LoadingText.AnalysisPercentFormat, ANALYSIS_PERCENT_FORMAT },
			{ LoadingText.PathCountFormat, PATH_COUNT_FORMAT },
			{ LoadingText.CollisionPercentFormat, COLLISION_PERCENT_FORMAT },
			{ LoadingText.SubmitStage, SUBMIT_STAGE },
			{ LoadingText.DisplayStage, DISPLAY_STAGE },
			{ LoadingText.DxfStage, DXF_STAGE },
			{ LoadingText.SurfaceStage, SURFACE_STAGE },
			{ LoadingText.ProjectionStage, PROJECTION_STAGE },
			{ LoadingText.ProjectStage, PROJECT_STAGE },
			{ LoadingText.ConvertDxf, CONVERT_DXF },
			{ LoadingText.ConnectDxfContours, CONNECT_DXF_CONTOURS },
			{ LoadingText.BuildTargetSurface, BUILD_TARGET_SURFACE },
			{ LoadingText.ProjectContours, PROJECT_CONTOURS },
			{ LoadingText.BuildEdgeMap, BUILD_EDGE_MAP },
			{ LoadingText.ReadProject, READ_PROJECT },
			{ LoadingText.RestoreObjects, RESTORE_OBJECTS },
			{ LoadingText.RestoreSettings, RESTORE_SETTINGS },
			{ LoadingText.ApplyProject, APPLY_PROJECT },
			{ LoadingText.RestoreSupportBracket, RESTORE_SUPPORT_BRACKET },
			{ LoadingText.ImportComplete, IMPORT_COMPLETE },
			{ LoadingText.ItemCountFormat, ITEM_COUNT_FORMAT },
			{ LoadingText.ProjectTitle, PROJECT_TITLE },
			{ LoadingText.TubeTopic, TUBE_TOPIC },
			{ LoadingText.ProjectContourEdges, PROJECT_CONTOUR_EDGES },
			{ LoadingText.DisplayParts, DISPLAY_PARTS },
			{ LoadingText.DisplayPaths, DISPLAY_PATHS }
		};

		static readonly AnalysisDefinition[] m_AnalysisSteps = {
			new AnalysisDefinition( LoadingStep.PrepareWorkpiece, LoadingText.PrepareWorkpiece ),
			new AnalysisDefinition( LoadingStep.InspectWorkpiece, LoadingText.InspectWorkpiece ),
			new AnalysisDefinition( LoadingStep.FindWorkpieceOutline, LoadingText.FindWorkpieceOutline ),
			new AnalysisDefinition( LoadingStep.CompareWorkpieceOutlines, LoadingText.CompareWorkpieceOutlines ),
			new AnalysisDefinition( LoadingStep.DetermineWorkpieceDirection, LoadingText.DetermineWorkpieceDirection ),
			new AnalysisDefinition( LoadingStep.FindWorkpieceCenter, LoadingText.FindWorkpieceCenter )
		};
		static readonly Dictionary<LoadingWorkflow, Dictionary<LoadingStep, Definition>> m_Definitions = BuildDefinitions();

		public static string Get( LoadingText key )
		{
			try {
				string defaultText;
				if( !m_DefaultText.TryGetValue( key, out defaultText ) ) {
					throw new ArgumentOutOfRangeException( nameof( key ) );
				}
				return m_LanguageManager == null ? defaultText : m_LanguageManager.TryGetString( key.ToString(), defaultText );
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( Get ), key, ex );
				return PREPARING;
			}
		}

		public static string Format( LoadingText key, params object[] values )
		{
			try {
				return string.Format( CultureInfo.GetCultureInfo( LanguageManager.LanguageManager.CurrentUILanguage ), Get( key ), values );
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( Format ), key, ex );
			}

			// Retry with the default template, not the broken translation.
			try {
				return string.Format( CultureInfo.InvariantCulture, m_DefaultText[ key ], values );
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( Format ), key, ex );
				return Get( LoadingText.Preparing );
			}
		}

		// Convert a calculation event into localized text and overall progress.
		public static LoadingProgressDisplay Create( LoadingBarInfo progress, LoadingWorkflow workflow = LoadingWorkflow.RingImport )
		{
			try {
				return CreateDisplay( progress, workflow );
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( Create ), progress.Step, ex );

				// A display failure must not report successful completion.
				return new LoadingProgressDisplay()
				{
					Key = nameof( LoadingStep.Preparing ),
					Text = Get( LoadingText.Preparing ),
					Percent = 0,
					IsComplete = false
				};
			}
		}

		static LoadingProgressDisplay CreateDisplay( LoadingBarInfo progress, LoadingWorkflow workflow )
		{
			Dictionary<LoadingStep, Definition> definitions;
			Definition definition;
			if( !m_Definitions.TryGetValue( workflow, out definitions ) || !definitions.TryGetValue( progress.Step, out definition ) ) {
				throw new ArgumentOutOfRangeException( nameof( progress ) );
			}

			// Derive local progress from item counts when the total is known.
			double fraction = progress.Total > 0 ? Clamp( (double)progress.Completed / progress.Total ) : 0.0;

			// Completion can bypass UI throttling; it does not mean the whole operation is done.
			bool isComplete = progress.Total > 0 && progress.Completed >= progress.Total;
			string detail = Get( definition.Detail );
			string counter = null;
			if( definition.AnalysisIndex >= 0 ) {
				// Show nested analysis numbering and optional local percent.
				if( progress.Total > 0 ) {
					counter = Format( LoadingText.AnalysisPercentFormat, detail, (int)( fraction * 100 ) );
				}
				detail = Format( LoadingText.AnalysisFormat, Get( LoadingText.AnalyzeWorkpiece ), definition.AnalysisIndex + 1, m_AnalysisSteps.Length, detail );

				// All analysis substeps share one visible placement step.
				fraction = ( definition.AnalysisIndex + fraction ) / m_AnalysisSteps.Length;
			}
			else if( definition.Detail == LoadingText.CalculatePaths ) {
				// Keep path text stable and show processed paths, not internal geometry work.
				if( progress.Total > 0 ) {
					counter = Format( LoadingText.PathCountFormat, progress.Completed, progress.Total );
				}
			}
			else if( definition.Stage == LoadingText.CollisionStage ) {
				// Preparation and collection bracket the check; their counts use different units.
				fraction = progress.Step == LoadingStep.PrepareCollision ? 0.0
					: progress.Step == LoadingStep.CollectCollisionResults ? 1.0 : fraction;

				// Expose only a percent, not frame or component counts.
				counter = Format( LoadingText.CollisionPercentFormat, (int)( fraction * 100 ) );
				isComplete = fraction >= 1.0;
			}
			else if( definition.Counter.HasValue ) {
				if( progress.Total > 0 ) {
					counter = Format( definition.Counter.Value, progress.Completed, progress.Total );
				}
			}
			else {
				// Other events mark step boundaries without displaying local progress.
				fraction = 0.0;
				isComplete = false;
			}

			// Split the stage range evenly across visible steps, then apply local progress.
			int percent = definition.Start + (int)( ( definition.End - definition.Start ) * ( definition.Index + fraction ) / definition.Count );

			// The last workflow event explicitly reaches 100%.
			if( progress.Step == LoadingStep.PathsComplete || progress.Step == LoadingStep.SimulationComplete || progress.Step == LoadingStep.ImportComplete ) {
				percent = 100;
				isComplete = true;
			}

			// Single events omit numbering; optional counters occupy the next line.
			string text = definition.IsSingle ? detail
				: Format( LoadingText.StageFormat, Get( definition.Stage ), definition.Index + 1, definition.Count, detail );
			if( counter != null ) {
				text += Environment.NewLine + counter;
			}
			return new LoadingProgressDisplay()
			{
				// Shared aliases keep one UI key; counts are throttled, language changes are not.
				Key = workflow + ":" + definition.Stage + ":" + definition.Index + ":" + definition.AnalysisIndex + ":" + LanguageManager.LanguageManager.CurrentUILanguage,
				Text = text,
				Percent = percent,
				IsComplete = isComplete
			};
		}

		static double Clamp( double value )
		{
			return Math.Max( 0.0, Math.Min( 1.0, value ) );
		}

		// Catch configuration errors before they fail static type initialization.
		static Dictionary<LoadingWorkflow, Dictionary<LoadingStep, Definition>> BuildDefinitions()
		{
			try {
				var workflows = new Dictionary<LoadingWorkflow, Dictionary<LoadingStep, Definition>>();
				foreach( LoadingWorkflow workflow in Enum.GetValues( typeof( LoadingWorkflow ) ) ) {
					workflows.Add( workflow, CreateWorkflow( workflow ) );
				}
				return workflows;
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( BuildDefinitions ), LANGUAGE_MAP_NAME, ex );

				// Discard partial mappings and let Create use its safe fallback.
				return new Dictionary<LoadingWorkflow, Dictionary<LoadingStep, Definition>>();
			}
		}

		static Dictionary<LoadingStep, Definition> CreateWorkflow( LoadingWorkflow workflow )
		{
			Dictionary<LoadingStep, Definition> result;
			if( workflow == LoadingWorkflow.DxfImport || workflow == LoadingWorkflow.ProjectImport ) {
				result = new Dictionary<LoadingStep, Definition>();
				if( workflow == LoadingWorkflow.DxfImport ) {
					AddGroup( result, LoadingText.ReadStage, 0, 15, new[] { new[] { LoadingStep.ReadFile } }, new[] { LoadingText.ReadFile } );
					AddGroup( result, LoadingText.DxfStage, 15, 35, new[] { new[] { LoadingStep.ConvertDxf }, new[] { LoadingStep.ConnectDxfContours } }, new[] { LoadingText.ConvertDxf, LoadingText.ConnectDxfContours } );
					AddGroup( result, LoadingText.SurfaceStage, 35, 40, new[] { new[] { LoadingStep.BuildTargetSurface } }, new[] { LoadingText.BuildTargetSurface } );
					AddGroup( result, LoadingText.ProjectionStage, 40, 65, new[] { new[] { LoadingStep.ProjectContours }, new[] { LoadingStep.ProjectContourEdges }, new[] { LoadingStep.BuildEdgeMap } }, new[] { LoadingText.ProjectContours, LoadingText.ProjectContourEdges, LoadingText.BuildEdgeMap } );
					AddGroup( result, LoadingText.PathStage, 65, 90, new[] {
						new[] { LoadingStep.AddPart },
						new[] { LoadingStep.CalculatePaths, LoadingStep.ValidatePath, LoadingStep.DiscretizeCurves, LoadingStep.CalculatePathPoints, LoadingStep.BuildPathCache, LoadingStep.SetPathStart }
					}, new[] { LoadingText.AddPart, LoadingText.CalculatePaths } );
					result[ LoadingStep.ConvertDxf ].Counter = LoadingText.ItemCountFormat;
					result[ LoadingStep.ProjectContours ].Counter = LoadingText.ItemCountFormat;
					result[ LoadingStep.ProjectContourEdges ].Counter = LoadingText.ItemCountFormat;
					result[ LoadingStep.BuildEdgeMap ].Counter = LoadingText.ItemCountFormat;
				}
				else {
					AddGroup( result, LoadingText.ReadStage, 0, 20, new[] { new[] { LoadingStep.ReadProject } }, new[] { LoadingText.ReadProject } );
					AddGroup( result, LoadingText.ProjectStage, 20, 85, new[] {
						new[] { LoadingStep.RestoreObjects }, new[] { LoadingStep.RestoreSettings }, new[] { LoadingStep.ApplyProject }, new[] { LoadingStep.RestoreSupportBracket }
					}, new[] { LoadingText.RestoreObjects, LoadingText.RestoreSettings, LoadingText.ApplyProject, LoadingText.RestoreSupportBracket } );
					result[ LoadingStep.RestoreObjects ].Counter = LoadingText.ItemCountFormat;
				}
				AddDisplayGroup( result, workflow == LoadingWorkflow.DxfImport ? 90 : 85 );
				AddSingle( result, LoadingStep.Preparing, LoadingText.Preparing, 0 );
			}
			else {
				result = CreateDefinitions();
				if( workflow == LoadingWorkflow.ModelImport ) {
					SetRange( result, LoadingText.SewStage, 75, 85 );
					result[ LoadingStep.SewShape ].Count = 1;
					result.Remove( LoadingStep.AddPart );
					AddGroup( result, LoadingText.SubmitStage, 85, 90, new[] { new[] { LoadingStep.AddPart } }, new[] { LoadingText.AddPart } );
					RemoveDisplayGroup( result );
					AddDisplayGroup( result, 90 );
				}
				else if( workflow == LoadingWorkflow.TubeImport ) {
					SetRange( result, LoadingText.SewStage, 0, 15 );
					SetRange( result, LoadingText.PlacementStage, 15, 50 );
					SetRange( result, LoadingText.PathStage, 50, 99 );
				}
			}
			AddSingle( result, LoadingStep.ImportComplete, LoadingText.ImportComplete, 100 );
			return result;
		}

		static void SetRange( Dictionary<LoadingStep, Definition> result, LoadingText stage, int start, int end )
		{
			foreach( Definition definition in result.Values ) {
				if( definition.Stage == stage ) {
					definition.Start = start;
					definition.End = end;
				}
			}
		}

		static void RemoveDisplayGroup( Dictionary<LoadingStep, Definition> result )
		{
			foreach( LoadingStep step in new[] { LoadingStep.UpdateDisplay, LoadingStep.ClearDisplay, LoadingStep.DisplayParts, LoadingStep.DisplayPaths, LoadingStep.UpdateDisplayTree, LoadingStep.PathsComplete } ) {
				result.Remove( step );
			}
		}

		static void AddDisplayGroup( Dictionary<LoadingStep, Definition> result, int start )
		{
			AddGroup( result, LoadingText.DisplayStage, start, 99, new[] {
				new[] { LoadingStep.UpdateDisplay, LoadingStep.ClearDisplay }, new[] { LoadingStep.DisplayParts }, new[] { LoadingStep.DisplayPaths }, new[] { LoadingStep.UpdateDisplayTree }
			}, new[] { LoadingText.UpdateDisplay, LoadingText.DisplayParts, LoadingText.DisplayPaths, LoadingText.UpdateDisplay } );
			result[ LoadingStep.DisplayParts ].Counter = LoadingText.ItemCountFormat;
			result[ LoadingStep.DisplayPaths ].Counter = LoadingText.PathCountFormat;
		}

		// Build event-to-display mappings; enum order does not control UI order.
		// Each inner array is one visible step; details[i] labels groups[i].
		static Dictionary<LoadingStep, Definition> CreateDefinitions()
		{
			var result = new Dictionary<LoadingStep, Definition>();

			// Import workflow ranges are display weights, not measured execution times.
			// Read: 2 visible steps, 10..40%.
			AddGroup( result, LoadingText.ReadStage, 10, 40,
				new[] { new[] { LoadingStep.PrepareReader }, new[] { LoadingStep.ReadFile } },
				new[] { LoadingText.PrepareReader, LoadingText.ReadFile } );

			// Transfer: 3 visible steps, with no invented native substeps.
			AddGroup( result, LoadingText.TransferStage, 40, 75,
				new[] { new[] { LoadingStep.TransferFile }, new[] { LoadingStep.CombineShape }, new[] { LoadingStep.ValidateShape } },
				new[] { LoadingText.TransferFile, LoadingText.CombineShape, LoadingText.ValidateShape } );

			// Sewing and adding the imported part are separate visible steps.
			AddGroup( result, LoadingText.SewStage, 75, 80,
				new[] { new[] { LoadingStep.SewShape }, new[] { LoadingStep.AddPart } },
				new[] { LoadingText.SewShape, LoadingText.AddPart } );

			// Placement has 3 visible steps: analyze, adjust, apply.
			LoadingStep[] analysisSteps = Array.ConvertAll( m_AnalysisSteps, definition => definition.Step );
			AddGroup( result, LoadingText.PlacementStage, 80, 90,
				new[] { analysisSteps, new[] { LoadingStep.AlignWorkpieceAxis, LoadingStep.OrientWorkpiece, LoadingStep.SetWorkpieceBase, LoadingStep.AlignWorkpieceOutline }, new[] { LoadingStep.ApplyPlacement } },
				new[] { LoadingText.AnalyzeWorkpiece, LoadingText.AdjustPlacement, LoadingText.ApplyPlacement } );

			// Expand only the first placement step into ordered nested analysis details.
			for( int i = 0; i < m_AnalysisSteps.Length; i++ ) {
				Definition definition = result[ m_AnalysisSteps[ i ].Step ];
				definition.AnalysisIndex = i;
				definition.Detail = m_AnalysisSteps[ i ].Detail;
			}

			// Paths: 7 visible steps; internal calculation/display events share steps 6/7 and 7/7.
			AddGroup( result, LoadingText.PathStage, 90, 99,
				new[] {
					new[] { LoadingStep.PreparePaths }, new[] { LoadingStep.FindOuterFace }, new[] { LoadingStep.FindConnectedFaces },
					new[] { LoadingStep.SewOuterFaces }, new[] { LoadingStep.ExtractContours },
					new[] { LoadingStep.CalculatePaths, LoadingStep.ValidatePath, LoadingStep.DiscretizeCurves, LoadingStep.CalculatePathPoints, LoadingStep.BuildPathCache, LoadingStep.SetPathStart },
					new[] { LoadingStep.UpdateDisplay, LoadingStep.ClearDisplay, LoadingStep.DisplayParts, LoadingStep.DisplayPaths, LoadingStep.UpdateDisplayTree, LoadingStep.PathsComplete }
				},
				new[] { LoadingText.PreparePaths, LoadingText.AnalyzePathWorkpiece, LoadingText.AnalyzePathWorkpiece, LoadingText.ProcessPathWorkpiece, LoadingText.IdentifyPaths, LoadingText.CalculatePaths, LoadingText.UpdateDisplay } );

			// Change only the final event's detail, not the other display aliases.
			result[ LoadingStep.PathsComplete ].Detail = LoadingText.PathsComplete;

			// Simulation uses its own range; collision events share one visible check step.
			AddGroup( result, LoadingText.CollisionStage, 65, 90,
				new[] { new[] { LoadingStep.PrepareCollision, LoadingStep.CheckCollision, LoadingStep.CollectCollisionResults } },
				new[] { LoadingText.CheckCollision } );

			// Standalone messages use fixed overall percentages and no step numbering.
			AddSingle( result, LoadingStep.Preparing, LoadingText.Preparing, 0 );
			AddSingle( result, LoadingStep.PrepareSimulation, LoadingText.PrepareSimulation, 10 );
			AddSingle( result, LoadingStep.CalculateSimulation, LoadingText.CalculateSimulation, 25 );
			AddSingle( result, LoadingStep.ApplySimulation, LoadingText.ApplySimulation, 90 );
			AddSingle( result, LoadingStep.SummarizeCollisions, LoadingText.SummarizeCollisions, 93 );
			AddSingle( result, LoadingStep.DisplaySimulation, LoadingText.DisplaySimulation, 96 );
			AddSingle( result, LoadingStep.SimulationComplete, LoadingText.SimulationComplete, 100 );
			return result;
		}

		// groups and details must have matching lengths and order; each event must be unique.
		// start/end define the main stage's overall percent range.
		static void AddGroup( Dictionary<LoadingStep, Definition> result, LoadingText stage, int start, int end,
			LoadingStep[][] groups, LoadingText[] details )
		{
			for( int i = 0; i < groups.Length; i++ ) {
				// Events in the same inner array are aliases for one visible step.
				foreach( LoadingStep step in groups[ i ] ) {
					// Separate instances allow later overrides without changing other aliases.
					result.Add( step, new Definition() { Stage = stage, Detail = details[ i ], Index = i, Count = groups.Length, Start = start, End = end } );
				}
			}
		}

		// Register an unnumbered message at a fixed overall percentage.
		static void AddSingle( Dictionary<LoadingStep, Definition> result, LoadingStep step, LoadingText detail, int percent )
		{
			result.Add( step, new Definition() { Stage = detail, Detail = detail, Count = 1, Start = percent, End = percent, IsSingle = true } );
		}

		// Keep language loading failures out of static type initialization.
		static LanguageManager.LanguageManager CreateLanguageManager()
		{
			try {
				return new LanguageManager.LanguageManager( LANGUAGE_MAP_NAME );
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( CreateLanguageManager ), LANGUAGE_MAP_NAME, ex );
				return null;
			}
		}

		const string ERROR_FORMAT = "Loading progress {0} failed ({1}): {2}";
		const string LANGUAGE_MAP_NAME = "LoadingProgress";
		const string PREPARING = "準備中";
		const string IMPORT_TITLE = "導入圖檔";
		const string PLACEMENT_TOPIC = "工件擺放中";
		const string FILE_TOPIC_FORMAT = "檔案名稱：{0}";
		const string SIMULATION_TITLE = "模擬計算";
		const string SIMULATION_TOPIC = "計算模擬結果";
		const string READ_STAGE = "讀取檔案";
		const string TRANSFER_STAGE = "轉換圖形";
		const string SEW_STAGE = "縫合曲面";
		const string PLACEMENT_STAGE = "擺放工件";
		const string PATH_STAGE = "建立路徑";
		const string COLLISION_STAGE = "計算碰撞結果";
		const string PREPARE_READER = "準備讀取檔案";
		const string READ_FILE = "正在讀取檔案";
		const string TRANSFER_FILE = "轉換檔案內容";
		const string COMBINE_SHAPE = "整合工件圖形";
		const string VALIDATE_SHAPE = "確認轉換結果";
		const string SEW_SHAPE = "處理工件圖形";
		const string ADD_PART = "完成工件匯入";
		const string ANALYZE_WORKPIECE = "分析工件";
		const string PREPARE_WORKPIECE = "整理工件資料";
		const string INSPECT_WORKPIECE = "檢查工件外形";
		const string FIND_WORKPIECE_OUTLINE = "辨識工件外形";
		const string COMPARE_WORKPIECE_OUTLINES = "比對工件外形";
		const string DETERMINE_WORKPIECE_DIRECTION = "判斷擺放方向";
		const string FIND_WORKPIECE_CENTER = "計算工件中心";
		const string ADJUST_PLACEMENT = "調整工件位置";
		const string APPLY_PLACEMENT = "完成擺放";
		const string PREPARE_PATHS = "準備建立路徑";
		const string ANALYZE_PATH_WORKPIECE = "分析工件";
		const string PROCESS_PATH_WORKPIECE = "處理工件";
		const string IDENTIFY_PATHS = "辨識路徑";
		const string CALCULATE_PATHS = "計算路徑";
		const string UPDATE_DISPLAY = "更新顯示";
		const string PATHS_COMPLETE = "路徑顯示完成";
		const string PREPARE_SIMULATION = "準備模擬資料";
		const string CALCULATE_SIMULATION = "計算模擬結果";
		const string CHECK_COLLISION = "檢查碰撞";
		const string APPLY_SIMULATION = "套用模擬結果";
		const string SUMMARIZE_COLLISIONS = "整理碰撞結果";
		const string DISPLAY_SIMULATION = "更新模擬顯示";
		const string SIMULATION_COMPLETE = "模擬資料整理完成";
		const string STAGE_FORMAT = "{0}（{1}/{2}）：{3}";
		const string ANALYSIS_FORMAT = "{0}（{1}/{2}）：{3}";
		const string ANALYSIS_PERCENT_FORMAT = "本步進度（{0}）：{1}%";
		const string PATH_COUNT_FORMAT = "已處理路徑 {0:N0}／{1:N0}";
		const string COLLISION_PERCENT_FORMAT = "檢查進度：{0}%";
		const string SUBMIT_STAGE = "加入模型";
		const string DISPLAY_STAGE = "更新顯示";
		const string DXF_STAGE = "轉換圖形";
		const string SURFACE_STAGE = "建立目標曲面";
		const string PROJECTION_STAGE = "投影輪廓";
		const string PROJECT_STAGE = "還原專案";
		const string CONVERT_DXF = "轉換圖元";
		const string CONNECT_DXF_CONTOURS = "連接輪廓";
		const string BUILD_TARGET_SURFACE = "建立目標曲面";
		const string PROJECT_CONTOURS = "投影輪廓";
		const string BUILD_EDGE_MAP = "建立輪廓關聯";
		const string READ_PROJECT = "讀取專案檔案";
		const string RESTORE_OBJECTS = "還原模型與路徑";
		const string RESTORE_SETTINGS = "還原加工與校正設定";
		const string APPLY_PROJECT = "套用專案資料";
		const string RESTORE_SUPPORT_BRACKET = "還原支撐架資料";
		const string IMPORT_COMPLETE = "導入完成";
		const string ITEM_COUNT_FORMAT = "已處理項目 {0:N0}／{1:N0}";
		const string PROJECT_TITLE = "匯入專案";
		const string TUBE_TOPIC = "導入設計管件";
		const string PROJECT_CONTOUR_EDGES = "逐邊投影輪廓";
		const string DISPLAY_PARTS = "顯示模型";
		const string DISPLAY_PATHS = "顯示路徑";
	}
}
