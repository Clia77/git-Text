using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Database
{
	public class CManagerTable
	{
		/// <summary>
		/// 스키마 정보
		/// </summary>
		public enum enumSchemaInfo
		{
			SCHEMA_INFO_INDEX = 0,
			SCHEMA_INFO_NAME = 1,
			SCHEMA_INFO_TYPE = 2,
			SCHEMA_INFO_NOT_NULL = 3,
			SCHEMA_INFO_DEFAULT = 4,
			SCHEMA_INFO_PK = 5
		};
		/// <summary>
		/// SQLite
		/// </summary>
		private CSQLite m_objSQLite;
		/// <summary>
		/// 파일 이름 ( 확장자를 포함 )
		/// </summary>
		private string m_strTableExtendName;
		/// <summary>
		/// 확장자를 제거한 파일 이름
		/// </summary>
		private string m_strTablePathOnly;
		/// <summary>
		/// 테이블 이름
		/// </summary>
		private string m_strTableName;
		/// <summary>
		/// 테이블 스키마 이름 문자열 배열
		/// </summary>
		private string[] m_strTableSchemaName;
		/// <summary>
		/// 테이블 스키마 타입 문자열 배열
		/// </summary>
		private string[] m_strTableSchemaType;
		/// <summary>
		/// 레코드 파일 전체 경로
		/// </summary>
		private string m_strRecordFullPath;
		/// <summary>
		/// pk 인덱스
		/// </summary>
		private int m_iPkIndex = 0;
		/// <summary>
		/// 테이블에 전체 데이터 테이블
		/// </summary>
		private DataTable m_objDataTable;

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objSQLite"></param>
		/// <param name="strTableFullPath"></param>
		/// <param name="strRecordFullPath"></param>
		/// <exception cref="ApplicationException"></exception>
		public void Initialize( CSQLite objSQLite, string strTableFullPath, string strRecordFullPath )
		{
			try {
				// SQLite 이어줌
				m_objSQLite = objSQLite;
				// 이름 제외한 폴더 경로
				string strTablePathOnly = Path.GetDirectoryName( strTableFullPath );
				m_strTablePathOnly = strTablePathOnly;
				// 파일 이름 (확장자를 포함)
				string strTableExtendName = Path.GetFileName( strTableFullPath );
				m_strTableExtendName = strTableExtendName;
				// 확장자를 제거한 파일 이름
				m_strTableName = Path.GetFileNameWithoutExtension( strTableFullPath );
				// 확장자
				string strTableExtendOnly = Path.GetExtension( strTableFullPath );
				// 레코드 파일 경로 ( 나중에 DB 셋업에서 레코드 파일 수정하면 해당 경로 txt를 갱신해줘야 함 )
				m_strRecordFullPath = strRecordFullPath;
				// 테이블이 이미 생성되어 있으면 생성 쿼리문 건너뜀
				bool bExistence = new bool();
				GetTableExistence( m_strTableName, ref bExistence );

				if( false == bExistence ) {
					// 테이블 생성
					DataTable objDataTable = new DataTable();
					// .csv 파일은 읽지 않는 걸로 수정
					if( ".txt" == strTableExtendOnly.ToLower() ) {
						objDataTable = CTxtFile.GetDataTableFromTxt( string.Format( @"{0}\{1}", strTablePathOnly, strTableExtendName ), true );
					} else {
						string strThrowLog = string.Format( "CManagerTable HLInitialize There is no {0} file.", strTableExtendName );
						throw new ArgumentException( strThrowLog );
					}
					SetTableCreate( m_strTableName, objDataTable );
				}
				// 테이블 스키마 정보 얻음
				DataTable objSchemaInfo = new DataTable();
				GetTableInformation( m_strTableName, ref objSchemaInfo );

				// 테이블 스키마 에트리뷰트 이름, 타입 가져옴
				DataRow[] objSchemaInfoRow = objSchemaInfo.Select();
				m_strTableSchemaName = new string[ objSchemaInfoRow.Length ];
				m_strTableSchemaType = new string[ objSchemaInfoRow.Length ];
				for( int iLoopRow = 0; iLoopRow < objSchemaInfoRow.Length; iLoopRow++ ) {
					m_strTableSchemaName[ iLoopRow ] = objSchemaInfoRow[ iLoopRow ].ItemArray[ ( int )enumSchemaInfo.SCHEMA_INFO_NAME ].ToString();
					m_strTableSchemaType[ iLoopRow ] = objSchemaInfoRow[ iLoopRow ].ItemArray[ ( int )enumSchemaInfo.SCHEMA_INFO_TYPE ].ToString();
					// pk에 해당하는 row값 저장
					if( "1" == objSchemaInfoRow[ iLoopRow ].ItemArray[ ( int )enumSchemaInfo.SCHEMA_INFO_PK ].ToString() ) {
						m_iPkIndex = iLoopRow;
					}
				}
				// 레코드 파일이 있으면 레코드 INSERT 쿼리문 실행
				if( null != strRecordFullPath && "" != strRecordFullPath ) {
					// 이름 제외한 폴더 경로
					string strRecordPathOnly = Path.GetDirectoryName( strRecordFullPath );
					// 파일 이름 (확장자를 포함)
					string strRecordExtendName = Path.GetFileName( strRecordFullPath );
					// 확장자를 제거한 파일 이름
					string strRecordName = Path.GetFileNameWithoutExtension( strRecordFullPath );
					// 확장자
					string strRecordExtendOnly = Path.GetExtension( strRecordFullPath );
					// 레코드 데이터 밀어넣기 전에 테이블에 데이터 삭제
					SetTableDataDelete( m_strTableName );

					// 테이블에 레코드 삽입
					DataTable objDataTable = new DataTable();
					// .csv 파일은 읽지 않는 걸로 수정
					if( ".txt" == strRecordExtendOnly.ToLower() ) {
						objDataTable = CTxtFile.GetDataTableFromTxt( string.Format( @"{0}\{1}", strRecordPathOnly, strRecordExtendName ), true );
					} else {
						string strThrowLog = string.Format( "CManagerTable HLInitialize There is no {0} file.", strTableExtendName );
						throw new ArgumentException( strThrowLog );
					}
					SetTableDataInsert( m_strTableName, objDataTable );
				}
				// 해당 테이블에 전체 Select 결과를 저장
				// 히스토리쪽 데이터 많아지면 프로그램 로딩 시, 느려지는 현상 있음
				//string strQuery = string.Format( "select * from {0}", m_strTableName );
				m_objDataTable = new DataTable();
				//m_objSQLite.HLReload( strQuery, ref m_objDataTable );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// 해제
		/// </summary>
		public void Deinitialize()
		{

		}

		/// <summary>
		/// 파일에 테이블 생성 쿼리를 읽어서 실행
		/// </summary>
		/// <param name="strTableName"></param>
		/// <param name="objDataTable"></param>
		/// <returns></returns>
		private void SetTableCreate( string strTableName, DataTable objDataTable )
		{
			// 데이터베이스 테이블 생성
			CSchemaInformation[] objSchemaInformation = GetSchemaInformation( objDataTable );
			SetTableCreate( strTableName, ref objSchemaInformation );
		}

		/// <summary>
		/// 테이블 데이터 삭제
		/// </summary>
		/// <param name="strTableName"></param>
		private void SetTableDataDelete( string strTableName )
		{
			string strQuery = string.Format( "delete from {0}", strTableName );
			m_objSQLite.Execute( strQuery );
		}

		/// <summary>
		/// 테이블 데이터 삽입
		/// </summary>
		/// <param name="strTableName"></param>
		/// <param name="objDataTable"></param>
		/// <exception cref="ApplicationException"></exception>
		private void SetTableDataInsert( string strTableName, DataTable objDataTable )
		{
			try {
				string strQuery = "";
				// 트랜잭션 시작
				SQLiteTransaction objTransaction = m_objSQLite.BeginTransaction();
				// INSERT 해야 하는 데이터 테이블 레코드 수만큼
				for( int iLoopRow = 0; iLoopRow < objDataTable.Rows.Count; iLoopRow++ ) {
					string strValues = "";
					DataRow objDataRow = objDataTable.Rows[ iLoopRow ];
					strQuery = string.Format( "insert into {0} values", strTableName );
					// 테이블에 정의된 스키마 타입 개수만큼
					for( int iLoopSchema = 0; iLoopSchema < m_strTableSchemaType.Length; iLoopSchema++ ) {
						// 타입 검사
						if( "INTEGER" == m_strTableSchemaType[ iLoopSchema ] ) {
							strValues += string.Format( "{0}", Convert.ToInt32( objDataRow[ iLoopSchema ] ) );
						} else if( true == m_strTableSchemaType[ iLoopSchema ].Contains( "VARCHAR" ) ) {
							strValues += string.Format( "'{0}'", objDataRow[ iLoopSchema ].ToString() );
						} else if( "REAL" == m_strTableSchemaType[ iLoopSchema ] || "DOUBLE" == m_strTableSchemaType[ iLoopSchema ] ) {
							strValues += string.Format( "{0}", Convert.ToDouble( objDataRow[ iLoopSchema ] ) );
						}

						if( iLoopSchema != m_strTableSchemaType.Length - 1 ) {
							strValues += ",";
						}
					}
					// INSERT 쿼리문
					strQuery = string.Format( "{0} ({1})", strQuery, strValues );
					// 쿼리문 수행
					m_objSQLite.Execute( strQuery );
				}
				// 문제 없으면 트랜잭션 커밋
				m_objSQLite.Commit( objTransaction );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// 테이블 스키마 정보 받음
		/// </summary>
		/// <param name="strTableName"></param>
		/// <param name="objDataTable"></param>
		private void GetTableInformation( string strTableName, ref DataTable objDataTable )
		{
			string strQuery = string.Format( "pragma table_info({0})", strTableName );
			m_objSQLite.Reload( strQuery, ref objDataTable );
		}

		/// <summary>
		/// 테이블 존재 유무 확인
		/// </summary>
		/// <param name="strTableName"></param>
		/// <param name="bExistence"></param>
		private void GetTableExistence( string strTableName, ref bool bExistence )
		{
			string strQuery = string.Format( "select count(*) from sqlite_master where name = '{0}'", strTableName );
			DataTable objDataTable = new DataTable();
			m_objSQLite.Reload( strQuery, ref objDataTable );
			// 테이블 존재 유무 결과 받음
			bExistence = Convert.ToBoolean( objDataTable.Rows[ 0 ][ 0 ] );
		}

		/// <summary>
		/// 테이블 생성
		/// </summary>
		/// <param name="strTableName"></param>
		/// <param name="strCreate"></param>
		private void SetTableCreate( string strTableName, string strCreate )
		{
			// 데이터베이스에서 delete 명령으로 값을 삭제할 때 실제 항목을 삭제하기 위해 테이블 생성 전에 해당 쿼리를 날려줌
			string strQuery = "pragma auto_vacuum = 1";
			m_objSQLite.Execute( strQuery );
			strQuery = string.Format( "create table if not exists {0} ( {1} )", strTableName, strCreate );
			m_objSQLite.Execute( strQuery );
		}

		/// <summary>
		/// 테이블 생성 ( 데이터를 받아서 쿼리문 생성 )
		/// </summary>
		/// <param name="strTableName"></param>
		/// <param name="objSchemaInformation"></param>
		private void SetTableCreate( string strTableName, ref CSchemaInformation[] objSchemaInformation )
		{
			string strCreate = null;
			string strPK = null;
			string strAutoIncrement = null;
			string strNotNull = null;

			for( int iLoopSchema = 0; iLoopSchema < objSchemaInformation.Length; iLoopSchema++ ) {
				// PK 키 설정 유무
				if( true == objSchemaInformation[ iLoopSchema ].m_bPk ) {
					strPK = "PRIMARY KEY ";
				} else {
					strPK = "";
				}
				// AutoIncrement 설정 유무
				if( true == objSchemaInformation[ iLoopSchema ].m_bAutoIncrement ) {
					strAutoIncrement = "AUTOINCREMENT ";
				} else {
					strAutoIncrement = "";
				}
				// Not Null 설정 유무
				if( true == objSchemaInformation[ iLoopSchema ].m_bNotNull ) {
					strNotNull = "NOT NULL ";
				} else {
					strNotNull = "";
				}

				strCreate += string.Format( "{0} {1} {2}{3}{4}",
					objSchemaInformation[ iLoopSchema ].m_strColumnName,
					objSchemaInformation[ iLoopSchema ].m_strDataType,
					strPK,
					strAutoIncrement,
					strNotNull );

				if( iLoopSchema != objSchemaInformation.Length - 1 ) {
					strCreate += ", ";
				}
			}
			// 생성
			SetTableCreate( strTableName, strCreate );
		}

		/// <summary>
		/// 초기화할 때 현재 Txt 테이블 스키마와 기존 DB 테이블 스키마 비교해서 구성 다를 시 DB 테이블 스키마를 Txt파일 테이블 스키마로 변경 후 기존 데이터 입력 => 이 후 기존체크부분 그대로 진행
		/// </summary>
		/// <param name="strTableName"></param>
		/// <param name="strFilteredSchemaName"></param>
		/// <exception cref="ApplicationException"></exception>
		private void SetTableChange( string strTableName, IEnumerable<string> strFilteredSchemaName )
		{
			try {
				// 트랜잭션 시작
				SQLiteTransaction objTransaction = m_objSQLite.BeginTransaction();
				// 임시 테이블에 레코드 복사
				string strQuery = "";
				strQuery = string.Format( "create table {0} as select * from {1}", strTableName + "_TEMP", strTableName );
				m_objSQLite.Execute( strQuery );
				// 원본 테이블 삭제
				strQuery = string.Format( "drop table {0}", strTableName );
				m_objSQLite.Execute( strQuery );
				// 테이블 새로 생성
				DataTable objDataTable = new DataTable();
				objDataTable = CTxtFile.GetDataTableFromTxt( string.Format( @"{0}\{1}", m_strTablePathOnly, m_strTableExtendName ), true );
				SetTableCreate( strTableName, objDataTable );
				// 임시 테이블 데이터 새 테이블에 삽입
				strQuery = string.Format( "insert into {0}", strTableName );
				string strColumns = "";
				string[] strSchemaName = strFilteredSchemaName.ToArray();
				for( int iLoopColumn = 0; iLoopColumn < strSchemaName.Length; iLoopColumn++ ) {
					strColumns += string.Format( "{0}", strSchemaName[ iLoopColumn ] );
					if( iLoopColumn != strSchemaName.Length - 1 ) {
						strColumns += ",";
					}
				}
				strQuery += string.Format( "({0}) select {1} from {2}", strColumns, strColumns, strTableName + "_TEMP" );
				m_objSQLite.Execute( strQuery );
				// 임시 테이블 삭제
				strQuery = string.Format( "drop table {0}", strTableName + "_TEMP" );
				m_objSQLite.Execute( strQuery );
				// 문제 없으면 트랜잭션 커밋
				m_objSQLite.Commit( objTransaction );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// 파일에 접근해서 스키마 정보를 얻어옴
		/// </summary>
		/// <param name="objDataTable"></param>
		/// <returns></returns>
		private CSchemaInformation[] GetSchemaInformation( DataTable objDataTable )
		{
			try {
				CSchemaInformation[] objSchemaInformation = null;
				objSchemaInformation = new CSchemaInformation[ objDataTable.Rows.Count ];
				// Row 값 Schema Information 자료에 넣는다.
				for( int iLoopRow = 0; iLoopRow < objDataTable.Rows.Count; iLoopRow++ ) {
					DataRow objDataRow = objDataTable.Rows[ iLoopRow ];
					objSchemaInformation[ iLoopRow ] = new CSchemaInformation(
						objDataRow.ItemArray[ ( int )CSchemaInformation.enumSchemaInformation.SCHEMA_INFORMATION_COLUMN_NAME ].ToString(),
						objDataRow.ItemArray[ ( int )CSchemaInformation.enumSchemaInformation.SCHEMA_INFORMATION_DATA_TYPE ].ToString(),
						Convert.ToBoolean( Convert.ToInt32( objDataRow.ItemArray[ ( int )CSchemaInformation.enumSchemaInformation.SCHEMA_INFORMATION_PK ] ) ),
						Convert.ToBoolean( Convert.ToInt32( objDataRow.ItemArray[ ( int )CSchemaInformation.enumSchemaInformation.SCHEMA_INFORMATION_AUTOINCREMENT ] ) ),
						Convert.ToBoolean( Convert.ToInt32( objDataRow.ItemArray[ ( int )CSchemaInformation.enumSchemaInformation.SCHEMA_INFORMATION_NOT_NULL ] ) ) );
				}
				return objSchemaInformation;
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				throw new ApplicationException( strException, ex );
			}
		}

		/// <summary>
		/// 외부에서 해당 객체 테이블 이름을 얻어옴
		/// </summary>
		/// <returns></returns>
		public string GetTableName()
		{
			return m_strTableName;
		}

		/// <summary>
		/// 외부에서 해당 객체 테이블 스키마 칼럼's 이름을 얻어옴
		/// </summary>
		/// <returns></returns>
		public string[] GetTableSchemaName()
		{
			return m_strTableSchemaName;
		}

		/// <summary>
		/// 외부에서 해당 객체 테이블 스키마 칼럼's 타입을 얻어옴
		/// </summary>
		/// <returns></returns>
		public string[] GetTableSchemaType()
		{
			return m_strTableSchemaType;
		}

		/// <summary>
		/// 외부에서 해당 객체 데이터 테이블을 얻어옴
		/// </summary>
		/// <returns></returns>
		public DataTable GetDataTable()
		{
			return m_objDataTable;
		}

		/// <summary>
		/// 외부에서 해당 객체 데이터 테이블을 설정함
		/// </summary>
		/// <param name="objDataTable"></param>
		public void SetDataTable( DataTable objDataTable )
		{
			m_objDataTable = objDataTable;
		}

		/// <summary>
		/// 외부에서 해당 객체 데이터 테이블에 pk를 재설정
		/// </summary>
		/// <returns></returns>
		public int GetPrimaryKey()
		{
			return m_iPkIndex;
		}

		/// <summary>
		/// 데이터 테이블을 select * from 으로 갱신함
		/// </summary>
		public void SetDataTableUpdate()
		{
			string strQuery = string.Format( "select * from {0}", m_strTableName );
			m_objSQLite.Reload( strQuery, ref m_objDataTable );
		}

		/// <summary>
		/// 실제 DB 테이블 구조랑 정의한 Table 엑셀이랑 일치하는지 비교
		/// </summary>
		/// <returns></returns>
		public bool IsCheckDatabaseTable()
		{
			bool bReturn = false;

			do {
				// DB에 있는 테이블 스키마에서 속성 이름을 얻어옴
				DataTable objSchemaInfo = new DataTable();
				GetTableInformation( m_strTableName, ref objSchemaInfo );
				DataRow[] objSchemaInfoRow = objSchemaInfo.Select();
				string[] strTableSchemaName = new string[ objSchemaInfoRow.Length ];
				for( int iLoopRow = 0; iLoopRow < objSchemaInfoRow.Length; iLoopRow++ ) {
					strTableSchemaName[ iLoopRow ] = objSchemaInfoRow[ iLoopRow ].ItemArray[ ( int )enumSchemaInfo.SCHEMA_INFO_NAME ].ToString();
				}
				// Txt 파일에 있는 테이블 스키마에서 속성 이름을 얻어옴
				DataTable objDataTable = new DataTable();
				objDataTable = CTxtFile.GetDataTableFromTxt( string.Format( @"{0}\{1}", m_strTablePathOnly, m_strTableExtendName ), true );
				string[] strTableExcelSchemaName = new string[ objDataTable.Rows.Count ];
				for( int iLoopRow = 0; iLoopRow < objDataTable.Rows.Count; iLoopRow++ ) {
					DataRow objDataRow = objDataTable.Rows[ iLoopRow ];
					strTableExcelSchemaName[ iLoopRow ] = objDataRow.ItemArray[ ( int )CSchemaInformation.enumSchemaInformation.SCHEMA_INFORMATION_COLUMN_NAME ].ToString();
				}
				// 비교해서 구성 다를 시 DB 테이블 스키마를 Txt파일 테이블 스키마로 수정 => 이 후 기존체크부분 그대로 진행
				{
					bool bIsEquivalentSchema = true;
					if( strTableSchemaName.Length != strTableExcelSchemaName.Length ) {
						bIsEquivalentSchema = false;
					}
					int iAttributeCount = 0;
					if( true == bIsEquivalentSchema ) {
						for( iAttributeCount = 0; iAttributeCount < strTableSchemaName.Length; iAttributeCount++ ) {
							if( strTableSchemaName[ iAttributeCount ] != strTableExcelSchemaName[ iAttributeCount ] ) {
								break;
							}
						}
					}
					if( iAttributeCount != strTableSchemaName.Length ) {
						bIsEquivalentSchema = false;
					}
					if( false == bIsEquivalentSchema ) {
						// New 스키마에 추가되었지만 Old 스키마엔 존재하지 않는 컬럼 필터링
						var strFilteredSchemaName = from strColumnName in strTableExcelSchemaName
													where true == strTableSchemaName.Contains( strColumnName )
													select strColumnName;
						SetTableChange( m_strTableName, strFilteredSchemaName );
						// 스키마 수정 성공시 DB에 있는 테이블 스키마에서 속성 이름을 다시 얻어옴
						objSchemaInfo = new DataTable();
						GetTableInformation( m_strTableName, ref objSchemaInfo );
						objSchemaInfoRow = objSchemaInfo.Select();
						m_strTableSchemaName = new string[ objSchemaInfoRow.Length ];
						m_strTableSchemaType = new string[ objSchemaInfoRow.Length ];
						for( int iLoopRow = 0; iLoopRow < objSchemaInfoRow.Length; iLoopRow++ ) {
							m_strTableSchemaName[ iLoopRow ] = objSchemaInfoRow[ iLoopRow ].ItemArray[ ( int )enumSchemaInfo.SCHEMA_INFO_NAME ].ToString();
							m_strTableSchemaType[ iLoopRow ] = objSchemaInfoRow[ iLoopRow ].ItemArray[ ( int )enumSchemaInfo.SCHEMA_INFO_TYPE ].ToString();
							// pk에 해당하는 row값 저장
							if( "1" == objSchemaInfoRow[ iLoopRow ].ItemArray[ ( int )enumSchemaInfo.SCHEMA_INFO_PK ].ToString() ) {
								m_iPkIndex = iLoopRow;
							}
						}
						// 비교해서 구성 다를 시 break - 기존체크부분
						if( m_strTableSchemaName.Length != strTableExcelSchemaName.Length ) {
							break;
						}
						int iLoopCount = 0;
						for( iLoopCount = 0; iLoopCount < m_strTableSchemaName.Length; iLoopCount++ ) {
							if( m_strTableSchemaName[ iLoopCount ] != strTableExcelSchemaName[ iLoopCount ] ) {
								break;
							}
						}
						if( iLoopCount != m_strTableSchemaName.Length ) {
							break;
						}
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 현재 데이터베이스 내 갱신된 파일을 프로그램 로딩 시 참조하는 레코드 파일에 갱신시킴
		/// </summary>
		public void UpdateRecordFile()
		{
			// DataTable 받아서 Csv로 저장
			// 참조하는 레코드 파일 경로에 테이블 이름.txt 로 파일 덮어씀
			CTxtFile.SetDataTableToTxt( m_strRecordFullPath, m_objDataTable );
		}
	}
}