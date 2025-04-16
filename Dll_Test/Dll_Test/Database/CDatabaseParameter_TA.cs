using System;

namespace Database
{
	public class CDatabaseParameter_TA : CDatabaseParameter
	{
		/// <summary>
		/// Table History TA
		/// </summary>
		public string strTableHistoryTA;

		public CDatabaseParameter_TA() : base()
		{
			strTableHistoryTA = "";
		}

		public override object Clone()
		{
			CDatabaseParameter_TA obj = new CDatabaseParameter_TA();

			obj.strDatabasePath = this.strDatabasePath;
			obj.strDatabaseHistoryName = this.strDatabaseHistoryName;
			obj.strDatabaseInformationName = this.strDatabaseInformationName;
			obj.strDatabaseTablePath = this.strDatabaseTablePath;
			obj.strDatabaseRecordPath = this.strDatabaseRecordPath;
			obj.strTableInformationUIText = this.strTableInformationUIText;
			obj.strTableInformationUserMessage = this.strTableInformationUserMessage;
			obj.strRecordInformationUIText = this.strRecordInformationUIText;
			obj.strRecordInformationUserMessage = this.strRecordInformationUserMessage;
			obj.iDatabaseDeletePeriod = this.iDatabaseDeletePeriod;
			obj.bDatabaseDelete = this.bDatabaseDelete;
			// 타입별로 구성
			obj.strTableHistoryTA = this.strTableHistoryTA;

			return obj;
		}
	}
}
