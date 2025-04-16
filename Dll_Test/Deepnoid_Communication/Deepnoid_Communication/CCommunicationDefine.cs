namespace Deepnoid_Communication
{
	public class CCommunicationDefine
	{
		public const int DEF_BYTE_LENGTH = 4096;
		/// <summary>
		/// 패리티 검사 프로토콜
		/// </summary>
		public enum enumSerialPortParity
		{
			PARITY_NONE = 0,
			PARITY_ODD,
			PARITY_EVEN,
			PARITY_MARK,
			PARITY_SPACE
		}
		/// <summary>
		/// 바이트 당 정지 비트
		/// </summary>
		public enum enumSerialPortStopBits
		{
			STOP_BITS_NONE = 0,
			STOP_BITS_ONE,
			STOP_BITS_TWO,
			STOP_BITS_ONE_POINT_FIVE
		}
		/// <summary>
		/// 인코딩 타입
		/// </summary>
		public enum enumDataEncoding
		{
			ENCODING_NONE = 0,
			ENCODING_DEFAULT,
			ENCODING_UCS2
		}
	}
}
