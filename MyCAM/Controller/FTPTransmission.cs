using System;
using System.IO;
using System.Net;

namespace MyCAM
{
	internal static class FTPTransmission
	{
		public static bool FileTransmit( string szLocalFilePath, string szFTPServerIP, out string szErrorMessage, string szFileName )
		{
			szErrorMessage = string.Empty;

			// Validate parameters
			if( string.IsNullOrWhiteSpace( szLocalFilePath ) || !File.Exists( szLocalFilePath ) ) {
				szErrorMessage = "本地檔案不存在或路徑無效";
				return false;
			}

			if( string.IsNullOrWhiteSpace( szFTPServerIP ) ) {
				szErrorMessage = "FTP 伺服器 IP 位址無效";
				return false;
			}

			try {
				// Build FTP address
				string szFtpAddress = $"ftp://{szFTPServerIP}/NcFiles/";

				// Check FTP connection
				FtpWebRequest testRequest = (FtpWebRequest)WebRequest.Create( szFtpAddress );
				testRequest.Method = WebRequestMethods.Ftp.ListDirectory;
				testRequest.Timeout = 1000;

				try {
					using( FtpWebResponse testResponse = (FtpWebResponse)testRequest.GetResponse() ) {
						// Connection successful
					}
				}
				catch {
					szErrorMessage = "連線失敗";
					return false;
				}

				// Use local file name if not specified
				if( string.IsNullOrWhiteSpace( szFileName ) ) {
					szFileName = Path.GetFileName( szLocalFilePath );
				}

				// Create upload request
				string szUploadFilePath = szFtpAddress + szFileName;
				FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create( szUploadFilePath );
				uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
				uploadRequest.Timeout = 2000;

				// Read and upload file
				byte[] fileContents = File.ReadAllBytes( szLocalFilePath );
				uploadRequest.ContentLength = fileContents.Length;

				using( Stream requestStream = uploadRequest.GetRequestStream() ) {
					requestStream.Write( fileContents, 0, fileContents.Length );
				}

				// Get response
				using( FtpWebResponse response = (FtpWebResponse)uploadRequest.GetResponse() ) {
					if( response.StatusCode == FtpStatusCode.ClosingData ||
						response.StatusCode == FtpStatusCode.FileActionOK ) {
						return true;
					}
					else {
						szErrorMessage = $"上傳失敗，狀態碼：{response.StatusCode}";
						return false;
					}
				}
			}
			catch( WebException webEx ) {
				if( webEx.Response is FtpWebResponse ftpResponse ) {
					szErrorMessage = $"{ftpResponse.StatusDescription}";
				}
				else {
					szErrorMessage = $"網路錯誤：{webEx.Message}";
				}
				return false;
			}
			catch( Exception ex ) {
				szErrorMessage = $"上傳異常：{ex.Message}";
				return false;
			}
		}
	}
}
