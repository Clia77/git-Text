using System;

namespace Database
{
	public class CDatabaseParameter_Flatness : CDatabaseParameter
	{
		/// <summary>
		/// Table History Flatness
		/// </summary>
		public string strTableHistoryFlatness;

		public CDatabaseParameter_Flatness() : base()
		{
			strTableHistoryFlatness = "";
		}

		public override object Clone()
		{
			CDatabaseParameter_Flatness obj = new CDatabaseParameter_Flatness();

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
			obj.strTableHistoryFlatness = this.strTableHistoryFlatness;

			return obj;
		}
	}
}
