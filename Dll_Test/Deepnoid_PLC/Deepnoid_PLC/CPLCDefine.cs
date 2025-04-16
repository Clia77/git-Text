namespace Deepnoid_PLC
{
	public class CPLCDefine
	{
		public const int DEF_MELSEC_STATION_NUMBER = 255;

		public const int DEF_WORD_TO_BIT = 16;

		/// <summary>
		/// plc 시리즈 타입
		/// </summary>
		public enum enumSeriseType
		{
			SERISE_TYPE_Q = 0,
			SERISE_TYPE_R,
		}

		/// <summary>
		/// plc 프로토콜 타입
		/// </summary>
		public enum enumProtocolType
		{
			PROTOCOL_TYPE_BINARY = 0,
			PROTOCOL_TYPE_ASCII,
		}

		/// <summary>
		/// 소켓 타입
		/// </summary>
		public enum enumSocketType
		{
			SOCKET_TYPE_CLIENT = 0,
			SOCKET_TYPE_SERVER,
		}

		/// <summary>
		/// 데이터 포맷
		/// </summary>
		public enum enumDataFormat
		{
			DATA_FORMAT_BIT = 0,
			DATA_FORMAT_WORD,
		}

		/// <summary>
		/// plc 디바이스 인덱스 정의
		/// </summary>
		public enum enumPLCDeviceIndex
		{
			ADDRESS = 0,
			NAME,
			COMMUNICATION_TYPE,
			DIGIT,
		}

		/// <summary>
		/// plc 디바이스 범위 타입
		/// </summary>
		public enum enumPLCDeviceCommunicationType
		{
			BIT_IN = 0,
			WORD_IN,
			DWORD_IN,
			BIT_OUT,
			WORD_OUT,
			DWORD_OUT,
			WORD_TO_BIT_NAME,
		}

		/// <summary>
		/// read or wirte 구분
		/// </summary>
		public enum enumPLCDeviceRWType
		{
			READ_BIT = 0,
			READ_WORD,
			READ_DWORD,
			WRITE_BIT,
			WRITE_WORD,
			WRITE_DWORD,
		}
	}
}