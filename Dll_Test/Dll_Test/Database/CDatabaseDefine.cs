namespace Database
{
	public partial class CDatabaseDefine
	{
		public const string DEF_RECORD_READ_LIMIT = "LIMIT 50000";
		/// <summary>
		/// 날짜 포멧
		/// </summary>
		public const string DEF_DATE_FORMAT = "yyyy-MM-dd";
		/// <summary>
		/// 날짜 시간 포멧
		/// </summary>
		public const string DEF_DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";
		public const string DEF_TACT_TIME_FORMAT = @"hh\:mm\:ss\.fff";
		/// <summary>
		/// 오름차순
		/// </summary>
		public const string DEF_ASC = "ASC";
		/// <summary>
		/// 내림차순
		/// </summary>
		public const string DEF_DESC = "DESC";

		/// <summary>
		/// Table Information UI Text Schema
		/// </summary>
		public enum enumInformationUIText
		{
			IDX,
			ID,
			FORM_NAME,
			TEXT_KOREA,
			TEXT_ENGLISH,
			TEXT_SPANISH,
		}

		/// <summary>
		/// Table Information User Message Schema
		/// </summary>
		public enum enumInformationUserMessage
		{
			ID,
			TEXT_KOREA,
			TEXT_ENGLISH,
			TEXT_SPANISH,
		}
	}
}