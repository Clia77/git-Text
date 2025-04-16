using System.IO.MemoryMappedFiles;

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
		/// <summary>
		/// 맵 데이터 생성자 입니다.
		/// </summary>
		/// <param name="memMap">메모리 맵 파일 클래스</param>
		/// <param name="pageIndex">할당 할 페이지 인덱스</param>
		/// <param name="pageCount">메모리 맵에 모든 페이지 개수</param>
		public CMemoryMapPages( MemoryMappedFile memMap, uint pageIndex, uint pageCount )
			: base( memMap, pageIndex, pageCount )
		{
		}

		/// <summary>
		/// 기본값을 씁니다.
		/// </summary>
		public void DefaultValue()
		{
			// 내부 데이터를 클리어 한다.
			DataByte.Clear();
			DataBool.Clear();
			DataInt.Clear();
			DataFloat.Clear();
			DataDouble.Clear();
			DataString.Clear();
		}
	}
}
