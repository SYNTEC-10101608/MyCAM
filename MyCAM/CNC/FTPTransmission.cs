using System;
using System.IO;
using System.Net;

namespace MyCAM
{
	internal static class FTPTransmission
	{
		static readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "FTPTransmission" );

		const string ERROR_LOCAL_FILE_NOT_FOUND = "本機檔案不存在或路徑無效";
		const string ERROR_FTP_SERVER_IP_INVALID = "FTP 伺服器 IP 位址無效";
		const string ERROR_CONNECTION_FAILED = "連線失敗";
		const string ERROR_UPLOAD_FAILED = "上傳失敗，狀態碼：{0}";
		const string ERROR_NETWORK_ERROR = "網路錯誤：{0}";
		const string ERROR_UPLOAD_EXCEPTION = "上傳異常：{0}";

		public static bool FileTransmit( string szLocalFilePath, string szFTPServerIP, string szFileName, out string szErrorMessage )
		{
			szErrorMessage = string.Empty;

			// Validate parameters
			if( string.IsNullOrWhiteSpace( szLocalFilePath ) || !File.Exists( szLocalFilePath ) ) {

				szErrorMessage = m_LanguageManager.TryGetString( "Error_LocalFileNotFound", ERROR_LOCAL_FILE_NOT_FOUND );
				return false;
			}

			if( string.IsNullOrWhiteSpace( szFTPServerIP ) ) {

				szErrorMessage = m_LanguageManager.TryGetString( "Error_FTPServerIPInvalid", ERROR_FTP_SERVER_IP_INVALID );
				return false;
			}

			try {

				// Use local file name if not specified
				if( string.IsNullOrWhiteSpace( szFileName ) ) {
					szFileName = Path.GetFileName( szLocalFilePath );
				}

				// Create upload request
				string szFtpAddress = $"ftp://{szFTPServerIP}/NcFiles/";
				string szUploadFilePath = szFtpAddress + szFileName;
				FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create( szUploadFilePath );
				uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
				uploadRequest.Timeout = 2000;

				FtpWebResponse response = null;
				try {
					response = (FtpWebResponse)uploadRequest.GetResponse();
				}
				catch {

							szErrorMessage = m_LanguageManager.TryGetString( "Error_ConnectionFailed", ERROR_CONNECTION_FAILED );
							if( response != null ) {
								response.Close();
							}
							return false;
						}

						byte[] fileContents = File.ReadAllBytes( szLocalFilePath );
						uploadRequest.ContentLength = fileContents.Length;

						using( Stream requestStream = uploadRequest.GetRequestStream() ) {
							requestStream.Write( fileContents, 0, fileContents.Length );
						}

						// Get response
						using( response = (FtpWebResponse)uploadRequest.GetResponse() ) {
							if( response.StatusCode == FtpStatusCode.ClosingData ||
								response.StatusCode == FtpStatusCode.FileActionOK ) {
								return true;
							}
							else {
								szErrorMessage = string.Format( m_LanguageManager.TryGetString( "Error_UploadFailed", ERROR_UPLOAD_FAILED ), response.StatusCode );
								return false;
							}
						}
					}
					catch( WebException webEx ) {
						if( webEx.Response is FtpWebResponse ftpResponse ) {
							szErrorMessage = ftpResponse.StatusDescription;
						}
						else {
							szErrorMessage = string.Format( m_LanguageManager.TryGetString( "Error_NetworkError", ERROR_NETWORK_ERROR ), webEx.Message );
						}
						return false;
					}
					catch( Exception ex ) {
						szErrorMessage = string.Format( m_LanguageManager.TryGetString( "Error_UploadException", ERROR_UPLOAD_EXCEPTION ), ex.Message );
						return false;
					}
		}
	}
}
