namespace Database
{
	public abstract class CProcessDatabaseHistory
	{
		/// <summary>
		/// SQLite
		/// </summary>
		public CSQLite m_objSQLite;

		/// <summary>
		/// 생성자 함수
		/// </summary>
		public CProcessDatabaseHistory()
		{

		}

		/// <summary>
		/// 초기화 함수
		/// </summary>
		/// <param name="objParameter"></param>
		public abstract void Initialize( CDatabaseParameter objParameter );

		/// <summary>
		/// 해제 함수
		/// </summary>
		public abstract void Deinitialize();

		public abstract CManagerTable GetManagerTable();

		public abstract bool IsCheckDatabaseTable();
	}
}