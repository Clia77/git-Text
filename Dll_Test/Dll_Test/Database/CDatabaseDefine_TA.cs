namespace Database
{
	public partial class CDatabaseDefine
	{
		/// <summary>
		/// Table History Ta Schema
		/// </summary>
		public enum enumHistoryTA
		{
			/// <summary>
			/// 자재 아이디 인덱스별 세 번 스캔하고 프로파일별 두 번 검사하니 6개의 데이터가 같은 아이디
			/// </summary>
			MODULE_ID = 0,
			DATE,
			BARCODE,
			/// <summary>
			/// 검사 타입 master jig or production
			/// </summary>
			TYPE,
			/// <summary>
			/// 자재 스캔 위치 1 : left / 2 : right
			/// </summary>
			SEQUENCE_INDEX,
			/// <summary>
			/// 검사 프로파일 인덱스 ( 마스터지그만 해당 )
			/// </summary>
			SEQUENCE_PROFILE,
			/// <summary>
			/// 양불 - 종합 양불 아니고 [index][profile] - 양불
			/// </summary>
			RESULT,
			/// <summary>
			/// 케이스 측정 높이
			/// </summary>
			CASE_HEIGHT,
			/// <summary>
			/// 케이스 높이 - 셀 높이 스펙 양불
			/// </summary>
			CASE_RESULT,
			/// <summary>
			/// NG 리스트
			/// </summary>
			NG_LIST,
			/// <summary>
			/// 이미지 경로
			/// </summary>
			IMAGE_PATH,
			/// <summary>
			/// 셀 숫자 받아서 cell data 파싱할거
			/// </summary>
			CELL_COUNT,
			/// <summary>
			/// 36개면 36개 데이터 ',' 로 이어붙일 거임 ex ) 0.123,0.234,0.345 ...
			/// </summary>
			CELL_DATA,
		}
	}
}