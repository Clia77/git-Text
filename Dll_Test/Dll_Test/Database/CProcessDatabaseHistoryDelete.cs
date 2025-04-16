using System;
using System.Threading;

namespace Database
{
	public class CProcessDatabaseHistoryDelete
	{
		/// <summary>
		/// SQLite
		/// </summary>
		private CSQLite m_objSQLite;
		/// <summary>
		/// Manager table
		/// </summary>
		private CManagerTable m_objManagerTable;
		/// <summary>
		/// 지우는 기준이 되는 날짜 인덱스
		/// </summary>
		private int m_iDateIndex;
		/// <summary>
		/// 지울꺼냐?
		/// </summary>
		private bool m_bDelete;
		/// <summary>
		/// 지우는 기간
		/// </summary>
		private int m_iDeletePeriod;
		/// <summary>
		/// 시스템 타이머
		/// </summary>
		private Timer m_objSystemTimer;

		/// <summary>
		/// 생성자 함수
		/// </summary>
		public CProcessDatabaseHistoryDelete()
		{

		}

		/// <summary>
		/// 초기화 함수
		/// </summary>
		/// <param name="objProcessDatabase"></param>
		/// <param name="objSQLite"></param>
		/// <returns></returns>
		public void Initialize( CSQLite objSQLite, CManagerTable objManagerTable, int iDateIndex, bool bDelete, int iDeletePeriod )
		{
			// SQLite 이어줌
			m_objSQLite = objSQLite;
			m_objManagerTable = objManagerTable;
			m_iDateIndex = iDateIndex;
			m_bDelete = bDelete;
			m_iDeletePeriod = iDeletePeriod;
			if( null == m_objSystemTimer ) {
				// 타이머에 대한 콜백 메서드 정의
				m_objSystemTimer = new Timer( SetDeleteHistory );
				// 지연시간, 기간 (ms) 한 시간마다 주기로 호출되게 설정
				m_objSystemTimer.Change( 1000, ( int )new TimeSpan( 1, 0, 0 ).TotalMilliseconds );
			}
		}

		/// <summary>
		/// 해제
		/// </summary>
		public void DeInitialize()
		{
			if( null != m_objSystemTimer ) {
				m_objSystemTimer.Change( Timeout.Infinite, Timeout.Infinite );
				m_objSystemTimer.Dispose();
				m_objSystemTimer = null;
			}
		}

		/// <summary>
		/// 지우는 기준이 되는 날짜 인덱스 갱신할 시 호출
		/// </summary>
		/// <param name="iDateIndex"></param>
		public void SetDateIndex( int iDateIndex )
		{
			m_iDateIndex = iDateIndex;
		}

		/// <summary>
		/// 지울지 말지 갱신할 시 호출
		/// </summary>
		/// <param name="bDelete"></param>
		public void SetDelete( bool bDelete )
		{
			m_bDelete = bDelete;
		}

		/// <summary>
		/// 지우는 기간 갱신할 시 호출
		/// </summary>
		/// <param name="iDeletePeriod"></param>
		public void SetDeletePeriod( int iDeletePeriod )
		{
			m_iDeletePeriod = iDeletePeriod;
		}
		
		/// <summary>
		/// 히스토리 삭제 쿼리 날려줌
		/// </summary>
		/// <param name="state"></param>
		private void SetDeleteHistory( Object state )
		{
			// 지우는 거 사용일 때만 진행
			if( false == m_bDelete ) {
				return;
			}
			CManagerTable objHistory = m_objManagerTable;

			// 트랜잭션 시작
			lock( m_objSQLite ) {
				var objTransaction = m_objSQLite.BeginTransaction();
				try {
					// 특정 기간 이전 히스토리를 삭제하는 쿼리
					SetDeleteHistory( objHistory, m_iDateIndex, m_iDeletePeriod );
				} finally {
					// 커밋
					m_objSQLite.Commit( objTransaction );
					objTransaction.Dispose();
				}
			}
		}

		/// <summary>
		/// 히스토리 삭제 쿼리 날려줌
		/// </summary>
		/// <param name="objManagerTable"></param>
		/// <param name="iIndex"></param>
		/// <param name="iDeletePeriod"></param>
		private void SetDeleteHistory( CManagerTable objManagerTable, int iIndex, int iDeletePeriod )
		{
			// 금일을 기준으로 특정일을 계산해야함
			string strQuery = string.Format( "DELETE FROM {0} WHERE {1} <= strftime('%J', 'now', 'localtime') - strftime('%J', {2});",
				objManagerTable.GetTableName(),
				iDeletePeriod,
				objManagerTable.GetTableSchemaName()[ iIndex ] );
			m_objSQLite.Execute( strQuery );
		}
	}
}