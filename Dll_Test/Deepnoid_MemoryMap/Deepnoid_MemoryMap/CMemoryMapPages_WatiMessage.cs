/// <summary>
/// 메모리맵 페이지들이 정의 되어 있는 네임스페이스 입니다.
/// </summary>
namespace Deepnoid_MemoryMap
{
	// [메모리맵 페이지 클래스]
	//
	// 1. CMemoryMapData_PageBase를 상속 받아 개별 페이지를 정의합니다.
	// 2. 프로퍼티를 정의하여 내부 데이터에 접근이 용이하게 합니다.
	//

	/// <summary>
	/// 개별 셀 데이터를 정의한 클래스
	/// </summary>
	public sealed partial class CMemoryMapPages : CMemoryMapData_PageBase
	{
		public enum enumBoolIndexWaitMessage
		{
			BOOL_WAIT_SHOW = 0,
		}

		public enum enumStringIndexWaitMessage
		{
			STRING_WAIT_MESSAGE = 0,
		}

		/// <summary>
		/// 대기 다이얼로그 Show Hide 유무
		/// </summary>
		public bool bWaitShow
		{
			get { return DataBool[ ( int )enumBoolIndexWaitMessage.BOOL_WAIT_SHOW ]; }
			set { DataBool[ ( int )enumBoolIndexWaitMessage.BOOL_WAIT_SHOW ] = value; }
		}

		/// <summary>
		/// 대기 다이얼로그 문자열
		/// </summary>
		public string strWaitMessage
		{
			get { return DataString[ ( int )enumStringIndexWaitMessage.STRING_WAIT_MESSAGE ]; }
			set { DataString[ ( int )enumStringIndexWaitMessage.STRING_WAIT_MESSAGE ] = value; }
		}
	}
}
