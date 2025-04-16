using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Reflection;
using System.Threading;

namespace Database
{
	public partial class CDatabaseSendMessage
	{
		/// <summary>
		/// DB 추상 클래스
		/// </summary>
		//private CDatabaseImplementAbstract m_objDatabaseImplement;
		private CProcessDatabaseHistory m_objDatabaseHistory;
		/// <summary>
		/// 레포트 히스토리 클래스
		/// </summary>
		public class CHistoryReport
		{
			/// <summary>
			/// 레포트 데이터
			/// </summary>
			public CDatabaseReport m_objData;

			public CHistoryReport( CDatabaseReport objData )
			{
				m_objData = objData.Clone() as CDatabaseReport;
			}
		}
		private volatile bool _shouldStop = false;
		private Thread _threadCommand;
		private ConcurrentQueue<EventItem> _queue = new ConcurrentQueue<EventItem>();
		public class EventItem
		{
			public Action<object> Method;
			public object Parameter;
			public EventItem( Action<object> method, object parameter )
			{
				Method = method;
				Parameter = parameter;
			}
		}

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

		/// <summary>
		/// 생성자
		/// </summary>
		/// <param name="objDatabaseImplement"></param>
		public CDatabaseSendMessage( CProcessDatabaseHistory objDatabaseHistory )
		{
			m_objDatabaseHistory = objDatabaseHistory;
			_threadCommand = new Thread( new ThreadStart( RunCommand ) );
			_threadCommand.Start();
		}

		/// <summary>
		/// 해제 함수
		/// </summary>
		public void DeInitialize()
		{
			_shouldStop = true;
			_threadCommand.Join();
		}

		/// <summary>
		/// 받은 메세지를 큐에 쌓고 스레드에서 처리하도록
		/// </summary>
		private void RunCommand()
		{
			EventItem item;
			int commandCount = 0;
			SQLiteTransaction transaction = null;
			int iThreadSleep = 10;
			int iMaxCommandCount = 100;

			while( !_shouldStop ) {
				try {
					// 큐가 비었고 커맨드 처리한 게 있으면 commit 날림
					if( _queue.Count == 0 ) {
						if( commandCount > 0 ) {
							// sql commit
							lock( m_objDatabaseHistory.m_objSQLite ) {
								m_objDatabaseHistory.m_objSQLite.Commit( transaction );
							}
							commandCount = 0;
						}
						Thread.Sleep( iThreadSleep );
						continue;
					}
					// 큐에 데이터가 있음
					// 커맨드가 특정 갯수 이상 쌓이면 에러 처리
					if( commandCount > iMaxCommandCount ) {
						// EndTransaction;
						lock( m_objDatabaseHistory.m_objSQLite ) {
							m_objDatabaseHistory.m_objSQLite.Commit( transaction );
						}
						string strError = $"DatabaseSendMessage -> EndTransaction[{commandCount}]";
						_callBackErrorMessage?.Invoke( strError );

						commandCount = 0;
						Thread.Sleep( iThreadSleep );
						continue;
					}
					// 큐에 데이터가 있고 커맨드 처리 완료되었으면 다시 begin transaction 시작
					if( 0 == commandCount ) {
						// sql begin transaction
						lock( m_objDatabaseHistory.m_objSQLite ) {
							transaction = m_objDatabaseHistory.m_objSQLite.BeginTransaction();
						}
					}
					// 큐에서 데이터 빼내서 sql execute 날려주는 함수 호출
					bool result = _queue.TryDequeue( out item );
					if( result ) {
						item.Method( item.Parameter );
						commandCount++;
					} else {
						string strError = $"DatabaseSendMessage -> Dequeue Fail";
						_callBackErrorMessage?.Invoke( strError );
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
					continue;
				}
				Thread.Sleep( iThreadSleep );
			}
		}
	}
}