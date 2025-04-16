namespace Database
{
	public class CProcessDatabaseInformation
	{
		/// <summary>
		/// SQLite
		/// </summary>
		public CSQLite m_objSQLite;
		/// <summary>
		/// Information UI Text
		/// </summary>
		public CManagerTable m_objManagerTableInformationUIText;
		/// <summary>
		/// Information User Message
		/// </summary>
		public CManagerTable m_objManagerTableInformationUserMessage;

		/// <summary>
		/// 생성자 함수
		/// </summary>
		public CProcessDatabaseInformation()
		{

		}

		/// <summary>
		/// 초기화 함수
		/// </summary>
		/// <param name="objProcessDatabase"></param>
		/// <returns></returns>
		public void Initialize( CDatabaseParameter objParameter )
		{
			// SQLite 초기화
			m_objSQLite = new CSQLite();
			m_objSQLite.Initialize( string.Format( @"{0}\{1}.db3", objParameter.strDatabasePath, objParameter.strDatabaseInformationName ) );
			// SQLite Connect
			m_objSQLite.Connect();
			// Information UI Text 초기화
			m_objManagerTableInformationUIText = new CManagerTable();
			{
				string strTablePath = $"{objParameter.strDatabaseTablePath}\\{objParameter.strTableInformationUIText}.txt";
				string strRecordPath = $"{objParameter.strDatabaseRecordPath}\\{objParameter.strRecordInformationUIText}.txt";
				m_objManagerTableInformationUIText.Initialize( m_objSQLite, strTablePath, strRecordPath );
			}
			// Information User Message 초기화
			m_objManagerTableInformationUserMessage = new CManagerTable();
			{
				string strTablePath = $"{objParameter.strDatabaseTablePath}\\{objParameter.strTableInformationUserMessage}.txt";
				string strRecordPath = $"{objParameter.strDatabaseRecordPath}\\{objParameter.strRecordInformationUserMessage}.txt";
				m_objManagerTableInformationUserMessage.Initialize( m_objSQLite, strTablePath, strRecordPath );
			}
			// 테이블 전체 로드
			m_objManagerTableInformationUIText.SetDataTableUpdate();
			m_objManagerTableInformationUserMessage.SetDataTableUpdate();
		}

		/// <summary>
		/// 해제 함수
		/// </summary>
		public void Deinitialize()
		{
			// Information UI Text
			m_objManagerTableInformationUIText?.Deinitialize();
			// Information User Message
			m_objManagerTableInformationUserMessage?.Deinitialize();

			// SQLite Disconnect
			m_objSQLite?.Disconnect();
			// SQLite 해제
			m_objSQLite?.DeInitialize();
		}
	}
}