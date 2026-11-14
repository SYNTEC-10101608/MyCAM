using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MyCAM.Data;
using OCC.IFSelect;
using OCC.IGESControl;
using OCC.Message;
using OCC.STEPControl;
using OCC.TopoDS;
using OCC.XSControl;
using OCCTool;

namespace MyCAM.Helper.FileIO
{
	internal enum FileFormat
	{
		BREP = 0,
		STEP = 1,
		IGES = 2,
		DXF = 3
	}

	internal static class CadFileReader
	{
		public static FileFormat GetFormat( string szFileName, bool allowDxf = false )
		{
			string extension = Path.GetExtension( szFileName ).ToLowerInvariant();
			if( extension == IGES_EXTENSION || extension == IGS_EXTENSION ) {
				return FileFormat.IGES;
			}
			if( allowDxf && extension == DXF_EXTENSION ) {
				return FileFormat.DXF;
			}

			// Preserve the existing STEP fallback for unrecognized extensions.
			return FileFormat.STEP;
		}

		// Return validated, sewn geometry; the caller owns data submission and errors.
		public static TopoDS_Shape Read( FileFormat format, string szFileName, Action<LoadingBarInfo> reportProgress = null )
		{
			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.PrepareReader ) );
			XSControl_Reader reader;
			switch( format ) {
				case FileFormat.BREP:
					reader = new XSControl_Reader();
					break;
				case FileFormat.STEP:
					reader = new STEPControl_Reader();
					break;
				case FileFormat.IGES:
					reader = new IGESControl_Reader();
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( format ) );
			}
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ReadFile ) );
			if( reader.ReadFile( szFileName ) != IFSelect_ReturnStatus.IFSelect_RetDone ) {
				return null;
			}

			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.TransferFile ) );
			using( TransferProgressIndicator indicator = new TransferProgressIndicator( reportProgress ) ) {
				using( Message_ProgressRange range = indicator.Start() ) {
					reader.TransferRoots( range );
				}
			}
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.CombineShape ) );
			if( reader.NbShapes() == 0 ) {
				return null;
			}
			TopoDS_Shape shape = reader.OneShape();
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.ValidateShape ) );
			if( shape == null || shape.IsNull() ) {
				return null;
			}
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.SewShape ) );
			return ShapeTool.SewShape( new List<TopoDS_Shape>() { shape } );
		}

		sealed class TransferProgressIndicator : Message_ProgressIndicator
		{
			readonly Action<LoadingBarInfo> m_ReportProgress;
			readonly int m_nThreadID = Thread.CurrentThread.ManagedThreadId;
			readonly Stopwatch m_RefreshWatch = Stopwatch.StartNew();

			public TransferProgressIndicator( Action<LoadingBarInfo> reportProgress )
			{
				m_ReportProgress = reportProgress;
			}

			protected override void Show( Message_ProgressScope scope, bool isForce )
			{
				if( Thread.CurrentThread.ManagedThreadId == m_nThreadID && m_RefreshWatch.ElapsedMilliseconds >= TRANSFER_REFRESH_INTERVAL_MS ) {
					m_RefreshWatch.Restart();

					// Report progress to the loading bar.
					m_ReportProgress?.Invoke( new LoadingBarInfo( LoadingStep.TransferFile ) );
				}
			}
		}

		const string IGES_EXTENSION = ".iges";
		const string IGS_EXTENSION = ".igs";
		const string DXF_EXTENSION = ".dxf";
		const int TRANSFER_REFRESH_INTERVAL_MS = 150;
	}
}
