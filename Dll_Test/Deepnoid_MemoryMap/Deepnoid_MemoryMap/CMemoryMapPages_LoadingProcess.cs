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
		public enum enumIntIndexLoadingProcess
		{
			INT_PROGRESS_INDEX = 0,
			INT_MESSAGE_TYPE,
		}

		public enum enumStringIndexLoadingProcess
		{
			STRING_PROGRAM_NAME = 0,
			STRING_PROGRAM_VERSION,
			STRING_PROGRESS_MESSAGE,
		}

		/// <summary>
		/// 진행률
		/// </summary>
		public int iProgressIndex
		{
			get { return DataInt[ ( int )enumIntIndexLoadingProcess.INT_PROGRESS_INDEX ]; }
			set { DataInt[ ( int )enumIntIndexLoadingProcess.INT_PROGRESS_INDEX ] = value; }
		}

		/// <summary>
		/// 메세지 타입
		/// </summary>
		public int iMessageType
		{
			get { return DataInt[ ( int )enumIntIndexLoadingProcess.INT_MESSAGE_TYPE ]; }
			set { DataInt[ ( int )enumIntIndexLoadingProcess.INT_MESSAGE_TYPE ] = value; }
		}

		/// <summary>
		/// 프로그램 이름
		/// </summary>
		public string strProgramName 
		{
			get { return DataString[ ( int )enumStringIndexLoadingProcess.STRING_PROGRAM_NAME ]; }
			set { DataString[ ( int )enumStringIndexLoadingProcess.STRING_PROGRAM_NAME ] = value; }
		}

		/// <summary>
		/// 프로그램 버전
		/// </summary>
		public string strProgramVersion
		{
			get { return DataString[ ( int )enumStringIndexLoadingProcess.STRING_PROGRAM_VERSION ]; }
			set { DataString[ ( int )enumStringIndexLoadingProcess.STRING_PROGRAM_VERSION ] = value; }
		}

		/// <summary>
		/// 메시지
		/// </summary>
		public string strProgressMessage
		{
			get { return DataString[ ( int )enumStringIndexLoadingProcess.STRING_PROGRESS_MESSAGE ]; }
			set { DataString[ ( int )enumStringIndexLoadingProcess.STRING_PROGRESS_MESSAGE ] = value; }
		}
	}
}
