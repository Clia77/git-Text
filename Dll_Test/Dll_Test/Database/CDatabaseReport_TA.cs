using System;

namespace Database
{
	/// <summary>
	/// 검사 결과에 사용되는 구조체만 묶어서 관리
	/// </summary>
	public class CDatabaseReport_TA : CDatabaseReport
	{
		public string strBarcode;
		/// <summary>
		/// 검사 타입 master jig or production
		/// </summary>
		public string strType;
		/// <summary>
		/// 자재 스캔 위치 1 : left / 2 : right
		/// </summary>
		public int iIndex;
		/// <summary>
		/// 자재 검사 위치
		/// </summary>
		public string strProfile;
		/// <summary>
		/// 양불 - 종합 양불 아니고 [index][profile] - 양불
		/// </summary>
		public string strResult;
		/// <summary>
		/// 케이스 측정 높이
		/// </summary>
		public string strCaseHeight;
		/// <summary>
		/// 케이스 높이 - 셀 높이 스펙 양불
		/// </summary>
		public string strCaseResult;
		/// <summary>
		/// NG 리스트
		/// </summary>
		public string strNgList;
		/// <summary>
		/// 이미지 경로 - 프로파일1,2에 대해서는 동일하겠네?
		/// </summary>
		public string strImagePath;
		/// <summary>
		/// 셀 숫자 받아서 cell data 파싱할거
		/// </summary>
		public int iCellCount;
		/// <summary>
		/// 36개면 36개 데이터 ',' 로 이어붙일 거임 ex ) 0.123,0.234,0.345 ...
		/// </summary>
		public string strCellData;
		/// <summary>
		/// 36개면 36개 데이터 ',' 로 이어붙일 거임 ex ) 0.123,0.234,0.345 ... ( 여기는 케이스 높이 - 셀 높이 )
		/// </summary>
		public string strCaseData;

		public CDatabaseReport_TA() : base()
		{
			strBarcode = "";
			strType = "";
			iIndex = 0;
			strProfile = "";
			strResult = "";
			strCaseHeight = "";
			strCaseResult = "";
			strNgList = "";
			strImagePath = "";
			iCellCount = 0;
			strCellData = "";
			strCaseData = "";
		}

		public override object Clone()
		{
			CDatabaseReport_TA obj = new CDatabaseReport_TA();

			obj.strMaterialID = this.strMaterialID;
			obj.objDateTime = this.objDateTime;
			// 타입별로 구성
			obj.strBarcode = this.strBarcode;
			obj.strType = this.strType;
			obj.iIndex = this.iIndex;
			obj.strProfile = this.strProfile;
			obj.strResult = this.strResult;
			obj.strCaseHeight = this.strCaseHeight;
			obj.strCaseResult = this.strCaseResult;
			obj.strNgList = this.strNgList;
			obj.strImagePath = this.strImagePath;
			obj.iCellCount = this.iCellCount;
			obj.strCellData = this.strCellData;
			obj.strCaseData = this.strCaseData;

			return obj;
		}
	}
}