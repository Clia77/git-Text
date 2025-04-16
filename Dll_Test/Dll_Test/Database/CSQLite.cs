using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Database
{
	/// <summary>
	/// SQLite.dll을 직접 연결하는 인터페이스 클래스
	/// </summary>
	public class CSQLite
	{
		/// <summary>
		/// SQLite 접속 객체
		/// </summary>
		private SQLiteConnection m_objSQLiteConnection;
		/// <summary>
		/// SQLite 쿼리 날리는 객체
		/// </summary>
		private SQLiteCommand m_objSQLiteCommand;
		/// <summary>
		/// sql 접속하기 위한 명령
		/// </summary>
		private string m_strConnection;
		/// <summary>
		/// sql 접속하려는 데이터베이스 경로
		/// </summary>
		private string _strDatabasePath;
		public string m_strDatabasePath
		{
			get
			{
				return _strDatabasePath;
			}
		}
		/// <summary>
		/// sql 접속하려는 데이터베이스 이름
		/// </summary>
		private string _strDatabaseName;
		public string m_strDatabaseName
		{
			get
			{
				return _strDatabaseName;
			}
		}

		/// <summary>
		/// Query 날라오면 콜백 처리
		/// </summary>
		/// <param name="strQuery">쿼리문</param>
		public delegate void CallBackQueryMessage( string strQuery );
		private CallBackQueryMessage _callBackQueryMessage;
		public void SetCallBackQueryMessage( CallBackQueryMessage callBack )
		{
			_callBackQueryMessage = callBack;
		}

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="strDatabaseFullPath"></param>
		public void Initialize( string strDatabaseFullPath )
		{
			// 이름 제외한 폴더 경로
			string strDatabasePathOnly = Path.GetDirectoryName( strDatabaseFullPath );
			// 파일 이름 (확장자를 포함)
			string strDatabaseExtendName = Path.GetFileName( strDatabaseFullPath );
			// 확장자를 제거한 파일 이름
			string strDatabaseName = Path.GetFileNameWithoutExtension( strDatabaseFullPath );
			// 데이터베이스 경로랑 이름만
			_strDatabasePath = strDatabasePathOnly;
			_strDatabaseName = strDatabaseName;
			// 데이퍼베이스 폴더 유무 체크
			if( false == Directory.Exists( strDatabasePathOnly ) ) {
				// 폴더가 없으면 생성
				Directory.CreateDirectory( strDatabasePathOnly );
			}
			// 데이터베이스 파일 유무 체크
			if( false == File.Exists( strDatabaseFullPath ) ) {
				// db3 생성
				SQLiteConnection.CreateFile( strDatabaseFullPath );
			}
			// 확장자 포함해서 연결
			// Journal Mode=WAL; 을 넣을 경우 성능향상에 도움 ( Read, Write를 따로 구분 )
			m_strConnection = string.Format( @"Data Source={0}\{1}; Journal Mode=WAL;", m_strDatabasePath, strDatabaseExtendName );
		}

		/// <summary>
		/// 해제
		/// </summary>
		public void DeInitialize()
		{
			m_objSQLiteCommand.Dispose();
			m_objSQLiteConnection.Dispose();
			// 재연결할 때 자원을 반환하고 바로 다시 붙기 위해...
			GC.Collect();
		}

		/// <summary>
		/// 연결
		/// </summary>
		/// <exception cref="ApplicationException"></exception>
		public void Connect()
		{
			try {
				m_objSQLiteConnection = new SQLiteConnection( m_strConnection );
				m_objSQLiteCommand = new SQLiteCommand();
				// 연결된 데이터베이스랑 커맨드 1:1 매칭
				m_objSQLiteCommand.Connection = m_objSQLiteConnection;
				// 이벤트 등록
				m_objSQLiteConnection.Commit += new SQLiteCommitHandler( OnEventCommit );
				m_objSQLiteConnection.RollBack += new EventHandler( OnEventRollback );
				// sql 통신 열어줌
				m_objSQLiteConnection.Open();
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// 연결 해제
		/// </summary>
		public void Disconnect()
		{
			// sql 통신 닫아줌
			m_objSQLiteConnection?.Close();
		}

		/// <summary>
		/// 트랜잭션 시작
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ApplicationException"></exception>
		public SQLiteTransaction BeginTransaction()
		{
			try {
				// 트랜잭션 시작
				return m_objSQLiteConnection?.BeginTransaction();
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// 커밋
		/// </summary>
		/// <param name="objSQLiteTransaction"></param>
		/// <exception cref="ApplicationException"></exception>
		public void Commit( SQLiteTransaction objSQLiteTransaction )
		{
			try {
				// 트랜잭션 COMMIT
				objSQLiteTransaction?.Commit();
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// 롤백
		/// </summary>
		/// <param name="objSQLiteTransaction"></param>
		/// <exception cref="ApplicationException"></exception>
		public void Rollback( SQLiteTransaction objSQLiteTransaction )
		{
			try {
				// 트랜잭션 ROLLBACK
				objSQLiteTransaction?.Rollback();
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// 실행( Insert, Update, Delete )
		/// </summary>
		/// <param name="strQuery"></param>
		/// <exception cref="ApplicationException"></exception>
		public void Execute( string strQuery )
		{
			try {
				// sql 쿼리문 넣어줌
				m_objSQLiteCommand.CommandText = strQuery;
				// 콜백 호출
				_callBackQueryMessage?.Invoke( strQuery );
				// 연결에 대한 Transact-SQL 문을 실행하고 영향을 받는 행의 수를 반환합니다.
				m_objSQLiteCommand?.ExecuteNonQuery();
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// 데이터베이스 데이터 불러오기( Select )
		/// </summary>
		/// <param name="strQuery"></param>
		/// <param name="objDataTable"></param>
		/// <exception cref="ApplicationException"></exception>
		public void Reload( string strQuery, ref DataTable objDataTable )
		{
			try {
				lock( this ) {
					SQLiteDataAdapter objSQLiteDataAdapter = new SQLiteDataAdapter( strQuery, m_strConnection );
					// 콜백 호출
					_callBackQueryMessage?.Invoke( strQuery );
					// 이름을 사용하여 지정된 범위에서 데이터 소스의 행과 일치하도록 행을 추가하거나 새로 고칩니다.
					objSQLiteDataAdapter?.Fill( objDataTable );
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// 트랜잭션 완료 이벤트
		/// </summary>
		/// <param name="objSender"></param>
		/// <param name="e"></param>
		private void OnEventCommit( object objSender, EventArgs e )
		{
		}

		/// <summary>
		/// 트랜잭션 회복 이벤트
		/// </summary>
		/// <param name="objSender"></param>
		/// <param name="e"></param>
		private void OnEventRollback( object objSender, EventArgs e )
		{
		}
	}
}