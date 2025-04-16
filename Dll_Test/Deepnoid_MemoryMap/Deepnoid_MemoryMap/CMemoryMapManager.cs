using System;
using System.IO.MemoryMappedFiles;

/// <summary>
/// 메모리맵 페이지 관리 매니져들이 있는 네임스페이스 입니다.
/// </summary>
namespace Deepnoid_MemoryMap
{
	// [메모리맵 관리 매니져 클래스]
	//
	// 1. 메모리 맵 관리 매니져 클래스는 싱글톤 패턴을 사용하여 작성합니다.
	// 2. 파일 하나당 하나의 관리 클래스를 만들어 사용합니다. 
	// 3. 하나의 메모리 맵에는 다수의 페이지가 존재 할 수 있습니다.
	// 4. 맵 파일을 생성 할 때 기존 값이 존재하지 않으면 초기값을 써줍니다.
	//

	/// <summary>
	/// 싱글톤 패턴의 메모리 맵 데이터 매니져 클래스 입니다.
	/// </summary>
	public sealed class CMemoryMapManager
	{
		public enum enumPage
		{
			LOADING_PROCESS,
			WAIT_MESSAGE,
		};

		/// <summary>
		/// 메모리 맵 파일
		/// </summary>
		private MemoryMappedFile _memMap;
		/// <summary>
		/// 셀 데이터 페이지
		/// </summary>
		private CMemoryMapData_PageBase[] _dataPages;
		/// <summary>
		/// 싱글톤 패턴 인스턴스
		/// </summary>
		private static CMemoryMapManager _instance = null;
		/// <summary>
		/// 멀티 쓰레드 환경에서 인스턴스를 생성 할 때 중복 생성 되지 않도록 락을 걸어준다.
		/// </summary>
		private static object _instanceCreateLock = new object();

		/// <summary>
		/// 셀 데이터 매니져 생성자
		/// </summary>
		private CMemoryMapManager()
		{
			// 기존에 파일이 존재하는지 확인 한다.
			var memMapFileName = "MemoryMapData/MemoryMapData.bin";
			bool firstCreateFile = false;
			if( false == System.IO.File.Exists( memMapFileName ) ) {
				firstCreateFile = true;
			}

			int iPagesLength = Enum.GetNames( typeof( enumPage ) ).Length;
			// 메모리 맵 클래스를 생성합니다.
			_memMap = CMemoryMapData_PageBase.CreateMemoryMap( memMapFileName, memMapFileName, ( uint )iPagesLength );
			// 메모리 블럭의 데이터 뷰를 생성합니다.
			_dataPages = new CMemoryMapPages[ iPagesLength ];
			for( int iLoopCount = 0; iLoopCount < _dataPages.Length; iLoopCount++ ) {
				_dataPages[ iLoopCount ] = new CMemoryMapPages( _memMap, ( uint )iLoopCount, ( uint )iPagesLength );
			}

			// 파일이 존재 하지 않으면 기본값을 써 넣는다.
			if( true == firstCreateFile ) {
				DefaultValue();
			}
		}

		/// <summary>
		/// 싱글톤 패턴 인스턴스를 불러온다.
		/// </summary>
		public static CMemoryMapManager Instance
		{
			get
			{
				if( null == _instance ) {
					// 만약 인스턴스를 새로 생성해야 하는 상황이면 중복 생성을 막기 위해 락을 걸고 다시 확인한 뒤 생성한다.
					lock( _instanceCreateLock ) {
						if( null == _instance ) {
							_instance = new CMemoryMapManager();
						}
					}
				}
				return _instance;
			}
		}

		/// <summary>
		/// 파일을 처음 만들었을 때 기본값을 써준다.
		/// </summary>
		private void DefaultValue()
		{
			foreach( CMemoryMapData_PageBase item in _dataPages ) {
				( item as CMemoryMapPages ).DefaultValue();
			}
		}

		/// <summary>
		/// 페이지 접근 인덱서
		/// </summary>
		/// <param name="iIndex">페이지 번호</param>
		/// <returns></returns>
		public CMemoryMapPages this[ int iIndex ]
		{
			get { return _dataPages[ iIndex ] as CMemoryMapPages; }
		}

		/// <summary>
		/// 페이지 접근 인덱서
		/// </summary>
		/// <param name="ePage">enumPage</param>
		/// <returns></returns>
		public CMemoryMapPages this[ enumPage ePage ]
		{
			get { return _dataPages[ ( int )ePage ] as CMemoryMapPages; }
		}
	}
}
