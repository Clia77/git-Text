namespace Deepnoid_PLC
{
	public class CPLCInterfaceMelsecParameterCCLink : CPLCInterfaceMelsecParameterAbstract
	{
		/// <summary>
		/// 설정 채널
		/// </summary>
		public string strChannel;
		/// <summary>
		/// 네트워크 번호
		/// </summary>
		public string strNetworkNumber;
		/// <summary>
		/// 스테이션 번호
		/// </summary>
		public string strStationNumber;

		public CPLCInterfaceMelsecParameterCCLink() : base()
		{
			strChannel = "";
			strNetworkNumber = "";
			strStationNumber = "";
		}

		public override object Clone()
		{
			CPLCInterfaceMelsecParameterCCLink obj = new CPLCInterfaceMelsecParameterCCLink();

			obj.strChannel = this.strChannel;
			obj.strNetworkNumber = this.strNetworkNumber;
			obj.strStationNumber = this.strStationNumber;

			return obj;
		}
	}
}
