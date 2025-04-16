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
		/// Flatness 히스토리 삽입
		/// </summary>
		/// <param name="objData"></param>
		public void SetInsertHistoryFlatness( CDatabaseReport objData )
		{
			CHistoryReport obj = new CHistoryReport( objData );
			_queue.Enqueue( new EventItem( ( m_objDatabaseHistory as CProcessDatabaseHistory_Flatness ).SetInsertHistoryFlatness, obj ) );
		}
	}
}