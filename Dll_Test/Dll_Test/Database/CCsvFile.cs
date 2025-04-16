using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Database
{
	public static class CCsvFile
	{
		/// <summary>
		/// Csv -> DataTable
		/// </summary>
		/// <param name="strFilePath"></param>
		/// <returns></returns>
		/// <exception cref="ApplicationException"></exception>
		public static DataTable ConvertCSVtoDataTable( string strFilePath )
		{
			try {
				DataTable objDataTable = new DataTable();
				StreamReader sr = new StreamReader( strFilePath );
				string[] headers = sr.ReadLine().Split( ',' );
				
				foreach( string header in headers ) {
					objDataTable.Columns.Add( header );
				}
				while( !sr.EndOfStream ) {
					string[] rows = Regex.Split( sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)" );
					DataRow dr = objDataTable.NewRow();
					for( int i = 0; i < headers.Length; i++ ) {
						dr[ i ] = rows[ i ];
					}
					objDataTable.Rows.Add( dr );
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
		/// DataTable -> Csv
		/// </summary>
		/// <param name="strPath"></param>
		/// <param name="objDataTable"></param>
		/// <param name="eMode"></param>
		/// <exception cref="ApplicationException"></exception>
		public static void SetDataTableToCsv( string strPath, DataTable objDataTable, FileMode eMode = FileMode.Create )
		{
			try {
				FileStream objFileStream = new FileStream( strPath, eMode, FileAccess.Write );
				StreamWriter objStreamWriter = new StreamWriter( objFileStream, Encoding.UTF8 );
				// 컬럼 구분자 , 로 나눔
				string strLine = string.Join( ",", objDataTable.Columns.Cast<DataColumn>().Select( x => x.ColumnName ) );
				objStreamWriter.WriteLine( strLine );
				// row 구분자 , 로 나눔
				for( int iLoopRow = 0; iLoopRow < objDataTable.Rows.Count; iLoopRow++ ) {
					strLine = string.Join( ",", objDataTable.Rows[ iLoopRow ].ItemArray.Cast<object>() );
					// 개행 문자 제거 mcr에 개행 문자 붙음..
					strLine = strLine.Replace( "\r", "" );
					strLine = strLine.Replace( "\n", "" );
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