namespace MyCAM.Data
{
	// A snapshot of one progress report, independent of the manager instance.
	public struct LoadingBarInfo
	{
		public LoadingStep Step { get; }

		// Counts are available when Total is greater than zero.
		public int Completed { get; }

		public int Total { get; }

		public LoadingBarInfo( LoadingStep step, int completed = 0, int total = 0 )
		{
			Step = step;
			Completed = completed;
			Total = total;
		}
	}

	public enum LoadingWorkflow
	{
		ModelImport,
		RingImport,
		TubeImport,
		DxfImport,
		ProjectImport,
		Simulation
	}

	public enum LoadingStep
	{
		Preparing,
		PrepareReader,
		ReadFile,
		TransferFile,
		CombineShape,
		ValidateShape,
		SewShape,
		AddPart,
		PrepareWorkpiece,
		InspectWorkpiece,
		FindWorkpieceOutline,
		CompareWorkpieceOutlines,
		DetermineWorkpieceDirection,
		FindWorkpieceCenter,
		AlignWorkpieceAxis,
		OrientWorkpiece,
		SetWorkpieceBase,
		AlignWorkpieceOutline,
		ApplyPlacement,
		PreparePaths,
		FindOuterFace,
		FindConnectedFaces,
		SewOuterFaces,
		ExtractContours,
		CalculatePaths,
		ValidatePath,
		DiscretizeCurves,
		CalculatePathPoints,
		BuildPathCache,
		SetPathStart,
		UpdateDisplay,
		ClearDisplay,
		DisplayParts,
		DisplayPaths,
		UpdateDisplayTree,
		PathsComplete,
		PrepareSimulation,
		CalculateSimulation,
		PrepareCollision,
		CheckCollision,
		CollectCollisionResults,
		ApplySimulation,
		SummarizeCollisions,
		DisplaySimulation,
		SimulationComplete,
		ConvertDxf,
		ConnectDxfContours,
		BuildTargetSurface,
		ProjectContours,
		BuildEdgeMap,
		ReadProject,
		RestoreObjects,
		RestoreSettings,
		ApplyProject,
		RestoreSupportBracket,
		ImportComplete,
		ProjectContourEdges
	}
}
