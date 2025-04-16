using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Database
{
	public class CProcessDatabase
	{
		/// <summary>
		/// 데이터베이스 이력 클래스
		/// </summary>
		public CProcessDatabaseHistory m_objProcessDatabaseHistory;
		/// <summary>
		/// 데이터베이스 정보 클래스
		/// </summary>
		public CProcessDatabaseInformation m_objProcessDatabaseInformation;
		/// <summary>
		/// 데이터베이스 쿼리 메세지
		/// </summary>
		public CDatabaseSendMessage m_objDatabaseSendMessage;

		/// <summary>
		/// Error Message 콜백 처리
		/// </summary>
		/// <param name="strErrorMessage"></param>
		public delegate void CallBackErrorMessage( string strErrorMessage );
		private CallBackErrorMessage _callBackErrorMessage;
		public void SetCallBackErrorMessage( CallBackErrorMessage callBack )
		{
			_callBackErrorMessage = callBack;
		}

		private void ReplyCallBack( string strErrorMessage )
		{
			_callBackErrorMessage?.Invoke( strErrorMessage );
		}

		/// <summary>
		/// 생성자 함수
		/// </summary>
		public CProcessDatabase()
		{

		}

		/// <summary>
		/// 초기화 함수
		/// </summary>
		/// <returns></returns>
		public bool Initialize( CDatabaseParameter objDatabaseParameter )
		{
			bool bReturn = false;
			
			do {
				// 데이터베이스 파라매터 타입으로 히스토리 생성
				Type objType = objDatabaseParameter.GetType();
				if( typeof( CDatabaseParameter_Flatness ) == objType ) {
					m_objProcessDatabaseHistory = new CProcessDatabaseHistory_Flatness();
				} else if( typeof( CDatabaseParameter_TA ) == objType ) {
					m_objProcessDatabaseHistory = new CProcessDatabaseHistory_TA();
				}
				// 데이터베이스 이력 초기화
				m_objProcessDatabaseHistory.Initialize( objDatabaseParameter );
				// 데이터베이스 정보 초기화
				m_objProcessDatabaseInformation = new CProcessDatabaseInformation();
				m_objProcessDatabaseInformation.Initialize( objDatabaseParameter );
				// 실제 DB 테이블 구조랑 정의한 Table 엑셀이랑 일치하는지 비교해서 불일치할 시,
				// 에러 메세지 break
				if( false == m_objProcessDatabaseHistory.IsCheckDatabaseTable() ) {
					string strDatabaseHistoryFile = string.Format( @"{0}\{1}.db3", objDatabaseParameter.strDatabasePath, objDatabaseParameter.strDatabaseHistoryName );
					string strBackupFilePath = Path.Combine( Path.GetDirectoryName( strDatabaseHistoryFile ), Path.GetFileNameWithoutExtension( strDatabaseHistoryFile ) + "_" + DateTime.Now.ToString( "yyyyMMddHHmmss" ) + ".db3" );
					m_objProcessDatabaseHistory.Deinitialize();
					FileInfo objFileInfo = new FileInfo( strDatabaseHistoryFile );
					Stopwatch objTimer = new Stopwatch();
					objTimer.Start();
					int iTimeout = 5000;
					bool bTimeout = false;
					// 파일 아직도 연결되었는지 확인
					while( true == IsFileLocked( objFileInfo ) ) {
						if( iTimeout <= objTimer.ElapsedMilliseconds ) {
							bTimeout = true;
							break;
						}
						Thread.Sleep( 100 );
					}
					// 시간 초과면 break
					if( true == bTimeout ) {
						MessageBox.Show( string.Format( "Check Database.db3 Table Tuple. Delete {0} and Restart program.", strDatabaseHistoryFile ) );
						break;
					}
					File.Move( strDatabaseHistoryFile, strBackupFilePath );
					// 제거하고 재연결
					if( typeof( CDatabaseParameter_Flatness ) == objType ) {
						m_objProcessDatabaseHistory = new CProcessDatabaseHistory_Flatness();
					} else if( typeof( CDatabaseParameter_TA ) == objType ) {
						m_objProcessDatabaseHistory = new CProcessDatabaseHistory_TA();
					}
					m_objProcessDatabaseHistory.Initialize( objDatabaseParameter );
					string strMessage = string.Format( "Check Database.db3 Table Tuple. Backup {0} and Recreate Database.", strBackupFilePath );
					Task<DialogResult> t = Task.Factory.StartNew( () => MessageBox.Show( strMessage ) );
					//break;
				}
				// 데이터베이스 히스토리 메세지
				m_objDatabaseSendMessage = new CDatabaseSendMessage( m_objProcessDatabaseHistory );
				m_objDatabaseSendMessage.SetCallBackErrorMessage( ReplyCallBack );

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 해제
		/// </summary>
		public void DeInitialize()
		{
			// 데이터베이스 이력 해제
			if( null != m_objProcessDatabaseHistory ) {
				m_objProcessDatabaseHistory.Deinitialize();
			}
			// 데이터베이스 정보 해제
			if( null != m_objProcessDatabaseInformation ) {
				m_objProcessDatabaseInformation.Deinitialize();
			}
			// 데이터베이스 히스토리 메세지 해제
			if( null != m_objDatabaseSendMessage ) {
				m_objDatabaseSendMessage.DeInitialize();
			}
		}

		/// <summary>
		/// 파일 다른 프로세스가 사용 중인지 확인
		/// </summary>
		/// <param name="objFile"></param>
		/// <returns></returns>
		private bool IsFileLocked( FileInfo objFile )
		{
			try {
				using( FileStream objStream = objFile.Open( FileMode.Open, FileAccess.Read, FileShare.None ) ) {
					objStream.Close();
				}
			}
			catch( IOException e ) {
				var errorCode = System.Runtime.InteropServices.Marshal.GetHRForException( e ) & ( ( 1 << 16 ) - 1 );
				if( 32 == errorCode || 33 == errorCode ) {
					return true;
				} else {
					return false;
				}
			}
			return false;
		}
	}
}