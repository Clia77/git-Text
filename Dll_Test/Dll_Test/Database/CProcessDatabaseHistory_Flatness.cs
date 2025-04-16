using System.Reflection;
using System;

namespace Database
{
	public class CProcessDatabaseHistory_Flatness : CProcessDatabaseHistory
	{
		/// <summary>
		/// History Flatness
		/// </summary>
		public CManagerTable m_objManagerTableHistoryFlatness;
		/// <summary>
		/// History Delete
		/// </summary>
		public CProcessDatabaseHistoryDelete m_objProcessDatabaseHistoryDelete;

		/// <summary>
		/// 생성자 함수
		/// </summary>
		public CProcessDatabaseHistory_Flatness()
		{

		}

		/// <summary>
		/// 초기화 함수
		/// </summary>
		/// <param name="objParameter"></param>
		public override void Initialize( CDatabaseParameter objParameter )
		{
			// SQLite 초기화
			m_objSQLite = new CSQLite();
			m_objSQLite.Initialize( string.Format( @"{0}\{1}.db3", objParameter.strDatabasePath, objParameter.strDatabaseHistoryName ) );
			// SQLite Connect
			m_objSQLite.Connect();
			// History Flatness 초기화
			m_objManagerTableHistoryFlatness = new CManagerTable();
			CDatabaseParameter_Flatness objParameterType = objParameter as CDatabaseParameter_Flatness;
			string strTablePath = $"{objParameterType.strDatabaseTablePath}\\{objParameterType.strTableHistoryFlatness}.txt";
			m_objManagerTableHistoryFlatness.Initialize( m_objSQLite, strTablePath, "" );
			// Process History Delete 초기화
			m_objProcessDatabaseHistoryDelete = new CProcessDatabaseHistoryDelete();
			m_objProcessDatabaseHistoryDelete.Initialize(
				m_objSQLite,
				m_objManagerTableHistoryFlatness,
				( int )CDatabaseDefine.enumHistoryFlatness.DATE,
				objParameter.bDatabaseDelete,
				objParameter.iDatabaseDeletePeriod );
		}

		/// <summary>
		/// 해제 함수
		/// </summary>
		public override void Deinitialize()
		{
			// Process History Delete 해제
			m_objProcessDatabaseHistoryDelete?.DeInitialize();
			// History Flatness
			m_objManagerTableHistoryFlatness?.Deinitialize();

			// SQLite Disconnect
			m_objSQLite?.Disconnect();
			// SQLite 해제
			m_objSQLite?.DeInitialize();
		}

		public override CManagerTable GetManagerTable()
		{
			return m_objManagerTableHistoryFlatness;
		}

		/// <summary>
		/// 실제 DB 테이블 구조랑 정의한 Table 엑셀이랑 일치하는지 비교
		/// </summary>
		/// <returns></returns>
		public override bool IsCheckDatabaseTable()
		{
			bool bReturn = false;

			do {
				// History Flatness
				if( false == m_objManagerTableHistoryFlatness?.IsCheckDatabaseTable() ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// Flatness 히스토리 삽입
		/// </summary>
		/// <param name="objData"></param>
		public void SetInsertHistoryFlatness( object objData )
		{
			CDatabaseSendMessage.CHistoryReport obj = objData as CDatabaseSendMessage.CHistoryReport;
			CDatabaseReport_Flatness objReportData = obj.m_objData as CDatabaseReport_Flatness;

			try {
				string strQuery = null;
				// 트랜잭션 시작 - SendMessage queue 에서 관리
				//var objTransaction = m_objDatabaseHistory.m_objSQLite.HLBeginTransaction();
				CManagerTable objManagerTable = m_objManagerTableHistoryFlatness;
				strQuery = string.Format( "insert into {0} values (", objManagerTable.GetTableName() );
				strQuery += string.Format( "'{0}',", objReportData.strMaterialID.ToUpper() ); // material id
				string strDate = objReportData.objDateTime.ToString( CDatabaseDefine.DEF_DATE_TIME_FORMAT );
				strQuery += string.Format( "'{0}',", strDate ); // date
				strQuery += string.Format( "'{0}',", objReportData.strBarcode ); // barcode
				strQuery += string.Format( "'{0}',", objReportData.strType ); // type
				strQuery += string.Format( "'{0:D}',", objReportData.iIndex ); // 자재 스캔 위치 1 : left / 2 : middle / 3 : right
				strQuery += string.Format( "'{0}',", objReportData.strProfile ); // 자재 검사 위치 1 : left / 2 : right
				strQuery += string.Format( "'{0}',", objReportData.strResult ); // 양불 - 종합 양불 아니고 [index][profile] - 양불
				strQuery += string.Format( "'{0}',", objReportData.strCaseHeight ); // 케이스 측정 높이
				strQuery += string.Format( "'{0}',", objReportData.strCaseResult ); // 케이스 높이 - 셀 높이 스펙 양불
				strQuery += string.Format( "'{0}',", objReportData.strNgList ); // NG 리스트
				strQuery += string.Format( "'{0}',", objReportData.strImagePath ); // 이미지 경로 - 프로파일1,2에 대해서는 동일하겠네?
				strQuery += string.Format( "'{0:D}',", objReportData.iCellCount ); // 셀 숫자 받아서 cell data 파싱할거
				strQuery += string.Format( "'{0}')", objReportData.strCellData + "," + objReportData.strCaseData ); // 36개면 36개 데이터 ',' 로 이어붙일 거임 ex ) 0.123,0.234,0.345 ...

				lock( m_objSQLite ) {
					m_objSQLite.Execute( strQuery );
				}
				// 커밋 - SendMessage queue 에서 관리
				//m_objSQLite.HLCommit( objTransaction );
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