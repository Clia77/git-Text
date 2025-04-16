using System;

namespace Database
{
	/// <summary>
	/// 검사 결과에 사용되는 구조체만 묶어서 관리
	/// </summary>
	public abstract class CDatabaseReport : ICloneable
	{
		/// <summary>
		/// 제품 아이디
		/// </summary>
		public string strMaterialID;
		/// <summary>
		/// 작성한 데이터 시간
		/// </summary>
		public DateTime objDateTime;

		public CDatabaseReport()
		{
			strMaterialID = "";
			objDateTime = DateTime.Now;
		}

		public abstract object Clone();
	}
}