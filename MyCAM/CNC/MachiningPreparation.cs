using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CncManager.Definition;
using MyCAM.Editor;

namespace MyCAM
{
	internal sealed class MachiningPreparation
	{
		public MachiningPreparation( Func<CAMEditor> getEditor )
		{
			m_GetEditor = getEditor;
		}

		public async Task<CncResult<string>> PrepareAsync( string ip, CancellationToken cancellationToken )
		{
			cancellationToken.ThrowIfCancellationRequested();
			CAMEditor editor = m_GetEditor();
			if( editor == null ) {
				return CncResult<string>.Failure( CncResultStatus.Failed, "CAM 模組尚未就緒。" );
			}
			string filePath = Path.Combine( Application.StartupPath, PROGRAM_NAME );
			if( !editor.ExportNC( filePath, out string error ) ) {
				return CncResult<string>.Failure( CncResultStatus.Failed, "NC 產生失敗：" + error );
			}
			cancellationToken.ThrowIfCancellationRequested();
			return await Task.Run( () =>
			{
				if( !FTPTransmission.FileTransmit( filePath, ip, PROGRAM_NAME, out string uploadError ) ) {
					return CncResult<string>.Failure( CncResultStatus.Failed, "NC 傳輸失敗：" + uploadError );
				}
				cancellationToken.ThrowIfCancellationRequested();
				return CncResult<string>.Success( PROGRAM_NAME );
			}, cancellationToken );
		}

		const string PROGRAM_NAME = "FiveAxisProgram.nc";
		readonly Func<CAMEditor> m_GetEditor;
	}
}
