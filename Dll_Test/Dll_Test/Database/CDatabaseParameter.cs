using System;

namespace Database
{
	public abstract class CDatabaseParameter : ICloneable
	{
		/// <summary>
		/// Database base path
		/// </summary>
		public string strDatabasePath;
		/// <summary>
		/// Database History
		/// </summary>
		public string strDatabaseHistoryName;
		/// <summary>
		/// Database Information
		/// </summary>
		public string strDatabaseInformationName;
		/// <summary>
		/// Database table path
		/// </summary>
		public string strDatabaseTablePath;
		/// <summary>
		/// Database record path
		/// </summary>
		public string strDatabaseRecordPath;
		/// <summary>
		/// Table Information UI Text
		/// </summary>
		public string strTableInformationUIText;
		/// <summary>
		/// Table Information User Message
		/// </summary>
		public string strTableInformationUserMessage;
		/// <summary>
		/// Record Information UI Text
		/// </summary>
		public string strRecordInformationUIText;
		/// <summary>
		/// Record Information User Message
		/// </summary>
		public string strRecordInformationUserMessage;
		/// <summary>
		/// Delete
		/// Delete Period
		/// </summary>
		public int iDatabaseDeletePeriod;

		public bool bDatabaseDelete;

		public CDatabaseParameter()
		{
			strDatabasePath = "";
			strDatabaseHistoryName = "";
			strDatabaseInformationName = "";
			strDatabaseTablePath = "";
			strDatabaseRecordPath = "";
			strTableInformationUIText = "";
			strTableInformationUserMessage = "";
			strRecordInformationUIText = "";
			strRecordInformationUserMessage = "";
			iDatabaseDeletePeriod = 0;
			bDatabaseDelete = false;
		}

		public abstract object Clone();
	}
}
