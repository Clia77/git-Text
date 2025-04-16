using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;

/// <summary>
/// 메모리맵 페이지의 정의가 있는 네임스페이스 입니다.
/// </summary>
namespace Deepnoid_MemoryMap
{
	/// <summary>
	/// 인트형 데이터 클래스 입니다.
	/// </summary>
	public sealed class CMemoryMapData_Int : IDisposable
	{
		/// <summary>
		/// 메모리 맵 파일 입니다.
		/// </summary>
		private MemoryMappedFile _memMap;
		/// <summary>
		/// 메모리 맵 뷰 입니다.
		/// </summary>
		private MemoryMappedViewAccessor _memView;
		/// <summary>
		/// 인트형의 크기 입니다.
		/// </summary>
		private readonly int _typeSize = 4; // sizeof(int) == 4
		/// <summary>
		/// 메모리 맵의 시작 위치 (바이트)
		/// </summary>
		private long _mapOffset;
		/// <summary>
		/// 메모리 맵 크기 (바이트)
		/// </summary>
		private long _mapSize;

		/// <summary>
		/// 인트형 데이터 클래스의 생성자 입니다.
		/// </summary>
		/// <param name="memMap">메모리 맵 파일 클래스</param>
		/// <param name="offset">메모리 맵 시작 위치 (바이트)</param>
		/// <param name="size">메모리 맵 크기 (바이트)</param>
		/// <param name="access">접근 권한</param>
		public CMemoryMapData_Int( MemoryMappedFile memMap, long offset, long size, MemoryMappedFileAccess access )
		{
			_memMap = memMap;
			_memView = _memMap.CreateViewAccessor( offset, size, access );
			_mapOffset = offset;
			_mapSize = size;
		}

		/// <summary>
		/// 인트형 데이터의 인덱서 입니다.
		/// </summary>
		/// <param name="idx">반환 할 데이터의 인덱스</param>
		/// <returns>인덱스에 위치한 값</returns>
		public int this[ int idx ]
		{
			get
			{
				var position = getPosition( idx );
				int readValue;
				_memView.Read( position, out readValue );
				return readValue;
			}
			set
			{
				var position = getPosition( idx );
				_memView.Write( position, value );
			}
		}

		/// <summary>
		/// 인덱스에 해당하는 메모리 위치(바이트)를 계산합니다.
		/// </summary>
		/// <param name="idx">데이터의 인덱스</param>
		/// <returns>메모리 위치(바이트)</returns>
		private int getPosition( int idx )
		{
			var targetPosition = _typeSize * idx;
			if( ( targetPosition + _typeSize ) >= _memView.Capacity ) {
				throw new IndexOutOfRangeException();
			} else {
				return targetPosition;
			}
		}

		/// <summary>
		/// 입력한 데이터를 복사합니다.
		/// </summary>
		/// <param name="sourceData">복사 할 데이터</param>
		public void Copy( CMemoryMapData_Int sourceData )
		{
			var copySourceData = sourceData.ToBytes();
			_memView.WriteArray( 0, copySourceData, 0, ( int )_mapSize );
		}

		/// <summary>
		/// 내부 데이터를 클리어 합니다.
		/// </summary>
		public void Clear()
		{
			var copySourceData = new byte[ _mapSize ];
			_memView.WriteArray( 0, copySourceData, 0, ( int )_mapSize );
		}

		/// <summary>
		/// 내부 전체 데이터의 바이트 배열을 읽어 옵니다.
		/// </summary>
		/// <returns>내부 전체 데이터의 배열</returns>
		public byte[] ToBytes()
		{
			var data = new byte[ _mapSize ];
			_memView.ReadArray( 0, data, 0, ( int )_mapSize );
			return data;
		}

		/// <summary>
		/// Dispose()를 호출하면 메모리를 해제합니다.
		/// </summary>
		public void Dispose()
		{
			_memView.Dispose();
		}
	}
}