using static Deepnoid_Communication.CCommunicationDefine;

namespace Deepnoid_Communication
{
	public class CCommunicationParameterSocket : CCommunicationParameterAbstract
	{
		/// <summary>
		/// ip 주소
		/// </summary>
		public string strSocketIPAddress;
		/// <summary>
		/// 포트
		/// </summary>
		public int iSocketPortNumber;
		/// <summary>
		/// 인코딩 타입
		/// </summary>
		public enumDataEncoding eDataEncoding;

		public CCommunicationParameterSocket() : base()
		{
			strSocketIPAddress = "192.168.0.1";
			iSocketPortNumber = 5000;
			eDataEncoding = enumDataEncoding.ENCODING_UCS2;
		}

		public override object Clone()
		{
			CCommunicationParameterSocket obj = new CCommunicationParameterSocket();

			obj.strSocketIPAddress = this.strSocketIPAddress;
			obj.iSocketPortNumber = this.iSocketPortNumber;
			obj.eDataEncoding = this.eDataEncoding;

			return obj;
		}
	}
}