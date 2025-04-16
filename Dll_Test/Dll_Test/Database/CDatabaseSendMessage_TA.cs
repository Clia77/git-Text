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
		/// TA 히스토리 삽입
		/// </summary>
		/// <param name="objData"></param>
		public void SetInsertHistoryTA( CDatabaseReport objData )
		{
			CHistoryReport obj = new CHistoryReport( objData );
			_queue.Enqueue( new EventItem( ( m_objDatabaseHistory as CProcessDatabaseHistory_TA ).SetInsertHistoryTA, obj ) );
		}
	}
}