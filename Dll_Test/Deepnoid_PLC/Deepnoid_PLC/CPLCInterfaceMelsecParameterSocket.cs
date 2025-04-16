using Deepnoid_Communication;
using static Deepnoid_PLC.CPLCDefine;

namespace Deepnoid_PLC
{
	public class CPLCInterfaceMelsecParameterSocket : CPLCInterfaceMelsecParameterAbstract
	{
		/// <summary>
		/// 소켓 통신에 필요한 초기화 데이터
		/// </summary>
		public CCommunicationParameter objParameter;
		/// <summary>
		/// plc 시리즈 타입
		/// </summary>
		public enumSeriseType eSeriseType;
		/// <summary>
		/// plc 프로토콜 타입
		/// </summary>
		public enumProtocolType eProtocolType;
		/// <summary>
		/// 소켓 타입
		/// </summary>
		public enumSocketType eSocketType;

		public CPLCInterfaceMelsecParameterSocket() : base()
		{
			objParameter = new CCommunicationParameter( new CCommunicationParameterSocket() );
			eSeriseType = enumSeriseType.SERISE_TYPE_Q;
			eProtocolType = enumProtocolType.PROTOCOL_TYPE_BINARY;
			eSocketType = enumSocketType.SOCKET_TYPE_CLIENT;
		}

		public override object Clone()
		{
			CPLCInterfaceMelsecParameterSocket obj = new CPLCInterfaceMelsecParameterSocket();

			obj.objParameter = ( CCommunicationParameter )this.objParameter.Clone();
			obj.eSeriseType = this.eSeriseType;
			obj.eProtocolType = this.eProtocolType;
			obj.eSocketType = this.eSocketType;

			return obj;
		}
	}
}