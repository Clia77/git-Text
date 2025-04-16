using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Database
{
	public static class CTxtFile
	{
		/// <summary>
		/// Txt -> DataTable
		/// </summary>
		/// <param name="strPath"></param>
		/// <param name="isFirstRowHeader"></param>
		/// <returns></returns>
		/// <exception cref="ApplicationException"></exception>
		public static DataTable GetDataTableFromTxt( string strPath, bool isFirstRowHeader )
		{
			try {
				DataTable objDataTable = new DataTable();
				// 공백, 탭 문자 제거
				char[] chRemove = { ' ', '\t' };
				string[] strLines = File.ReadAllLines( strPath, Encoding.Unicode );
				string[] strCols = strLines[ 0 ].Split( ',' );
				// 헤더가 포함되어 있으면 칼럼을 삽입
				if( true == isFirstRowHeader ) {
					for( int iLoopColumn = 0; iLoopColumn < strCols.Length; iLoopColumn++ ) {
						objDataTable.Columns.Add( new DataColumn( strCols[ iLoopColumn ].Trim( chRemove ) ) );
					}
				} else {
					for( int iLoopColumn = 0; iLoopColumn < strCols.Length; iLoopColumn++ ) {
						objDataTable.Columns.Add( new DataColumn( "Column" + iLoopColumn.ToString() ) );
					}
				}
				// 레코드 파일을 삽입
				for( int iLoopLine = 1; iLoopLine < strLines.Length; iLoopLine++ ) {
					string[] strRecord = strLines[ iLoopLine ].Split( ',' );

					DataRow objDataRow = objDataTable.NewRow();
					for( int iLoopRow = 0; iLoopRow < strRecord.Length; iLoopRow++ ) {
						objDataRow[ iLoopRow ] = strRecord[ iLoopRow ].Trim( chRemove );
					}
					objDataTable.Rows.Add( objDataRow );
				}
				return objDataTable;
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// DataTable -> Txt
		/// </summary>
		/// <param name="strPath"></param>
		/// <param name="objDataTable"></param>
		/// <exception cref="ApplicationException"></exception>
		public static void SetDataTableToTxt( string strPath, DataTable objDataTable )
		{
			try {
				FileStream objFileStream = new FileStream( strPath, FileMode.Create, FileAccess.Write );
				StreamWriter objStreamWriter = new StreamWriter( objFileStream, Encoding.Unicode );
				// 컬럼 구분자 \t,\t 로 나눔
				string strLine = string.Join( "\t,\t", objDataTable.Columns.Cast<object>() );
				objStreamWriter.WriteLine( strLine );
				// row 구분자 \t,\t 로 나눔
				for( int iLoopRow = 0; iLoopRow < objDataTable.Rows.Count; iLoopRow++ ) {
					strLine = string.Join( "\t,\t", objDataTable.Rows[ iLoopRow ].ItemArray.Cast<object>() );
					objStreamWriter.WriteLine( strLine );
				}
				objStreamWriter.Close();
				objFileStream.Close();
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}
	}
}