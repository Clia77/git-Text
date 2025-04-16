using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Data
{
	public partial class CConfig
	{
		/// <summary>
		/// 데이터베이스 파라미터
		/// </summary>
		public class DatabaseParameter
		{
			/// <summary>
			/// 데이터베이스 경로
			/// </summary>
			public string strDatabasePath { get; set; }
			/// <summary>
			/// 데이터 베이스 히스토리 이름
			/// </summary>
			public string strDatabaseHistoryName { get; set; }
			/// <summary>
			/// 데이터 베이스 인포메이션 이름
			/// </summary>
			public string strDatabaseInformationName { get; set; }
			/// <summary>
			/// 데이터 베이스 테이블 폴더 경로
			/// </summary>
			public string strDatabaseTablePath { get; set; }
			/// <summary>
			/// 데이터 베이스 레코드 폴더 경로
			/// </summary>
			public string strDatabaseRecordPath { get; set; }
			/// <summary>
			/// 데이터 베이스 인포메이션 테이블 UI text 이름
			/// </summary>
			public string strTableInformationUIText { get; set; }
			/// <summary>
			/// 데이터 베이스 인포메이션 테이블 유저메시지 text 이름
			/// </summary>
			public string strTableInformationUserMessage { get; set; }
			/// <summary>
			/// 데이터 베이스 인포메이션 레코드 UI text 이름
			/// </summary>
			public string strRecordInformationUIText { get; set; }
			/// <summary>
			/// 데이터 베이스 인포메이션 레코드 유저메시지 text 이름
			/// </summary>
			public string strRecordInformationUserMessage { get; set; }
			/// <summary>
			/// 데이터 베이스 저장 기간
			/// </summary>
			public int iDatabaseDeletePeriod { get; set; }
			/// <summary>
			/// 데이터 베이스 삭제 사용 유무
			/// </summary>
			public bool bDatabaseDelete { get; set; }
		}

		/// <summary>
		/// 데이터베이스 파라미터 불러오기
		/// </summary>
		/// <returns></returns>
		public bool LoadDatabaseParameter()
		{
			try {
				string strPath = Environment.CurrentDirectory + @"\Config\ConfigDatabase.Json";

				if( File.Exists( strPath ) ) {
					string json = File.ReadAllText( strPath );
					m_objSystemParameter = JsonConvert.DeserializeObject<SystemParameter>( json );
					return true;
				} else {
					// 파일이 없는 경우 기본값으로 RootParameter 객체 생성 후 반환
					DatabaseParameter databaseParameter = new DatabaseParameter();
					DefaultValue( out databaseParameter );
					SaveDatabaseParameter( databaseParameter );
					return false;
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
				return false;
			}
		}

		/// <summary>
		/// 데이터베이스 파라미터 저장
		/// </summary>
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public bool SaveDatabaseParameter( DatabaseParameter objParameter )
		{
			bool bResult = false;
			try {
				m_objDatabaseParameter = objParameter;
				string strPath = Environment.CurrentDirectory + @"\Config\ConfigDatabase.Json";
				string json = JsonConvert.SerializeObject( m_objDatabaseParameter, Formatting.Indented );
				File.WriteAllText( strPath, json );
				bResult = true;
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
			return bResult;
		}

		/// <summary>
		/// 저장된 설정파일이 없을 경우 기본값으로 대체.
		/// </summary>
		/// <param name="databaseParameter"></param>
		private void DefaultValue( out DatabaseParameter databaseParameter )
		{
			databaseParameter = new DatabaseParameter();
			databaseParameter.strDatabasePath = "D:\\DeepnoidInspection\\Item\\Database\\";
			databaseParameter.strDatabaseTablePath = "\\DatabaseTable";
			databaseParameter.strDatabaseRecordPath = "\\DatabaseRecord";
			databaseParameter.strDatabaseInformationName = "DATABASE_INFORMATION";
			databaseParameter.strDatabaseHistoryName = "DATABASE_HISTORY";
			databaseParameter.strRecordInformationUserMessage = "RECORD_INFORMATION_USER_MESSAGE";
			databaseParameter.strTableInformationUserMessage = "TABLE_INFORMATION_USER_MESSAGE";
			databaseParameter.strRecordInformationUIText = "RECORD_INFORMATION_UI_TEXT";
			databaseParameter.strTableInformationUIText = "TABLE_INFORMATION_UI_TEXT";
			databaseParameter.bDatabaseDelete = true;
			databaseParameter.iDatabaseDeletePeriod = 1095;
		}

		/// <summary>
		/// 데이터베이스 파라미터 객체
		/// </summary>
		/// <returns></returns>
		public DatabaseParameter GetDatabaseParameter()
		{
			return m_objDatabaseParameter;
		}
	}
}