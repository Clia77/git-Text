using static Deepnoid_Communication.CCommunicationDefine;

namespace Deepnoid_Communication
{
	public class CCommunicationParameterSerialPort : CCommunicationParameterAbstract
	{
		/// <summary>
		/// 포트 이름
		/// </summary>
		public string strSerialPortName;
		/// <summary>
		/// 전송 속도
		/// </summary>
		public int iSerialPortBaudrate;
		/// <summary>
		/// 패리티 검사 프로토콜
		/// </summary>
		public enumSerialPortParity eParity;
		/// <summary>
		/// 데이터 비트 길이
		/// </summary>
		public int iSerialPortDataBits;
		/// <summary>
		/// 바이트 당 정지 비트
		/// </summary>
		public enumSerialPortStopBits eStopBits;

		public CCommunicationParameterSerialPort() : base()
		{
			strSerialPortName = "COM1";
			iSerialPortBaudrate = 19200;
			eParity = enumSerialPortParity.PARITY_NONE;
			iSerialPortDataBits = 8;
			eStopBits = enumSerialPortStopBits.STOP_BITS_ONE;
		}

		public override object Clone()
		{
			CCommunicationParameterSerialPort obj = new CCommunicationParameterSerialPort();

			obj.strSerialPortName = this.strSerialPortName;
			obj.iSerialPortBaudrate = this.iSerialPortBaudrate;
			obj.eParity = this.eParity;
			obj.iSerialPortDataBits = this.iSerialPortDataBits;
			obj.eStopBits = this.eStopBits;

			return obj;
		}
	}
}