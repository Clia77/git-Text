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
	// [메모리맵 페이지 베이스 클래스]
	//
	// 1. MMData_Base는 고정된 **1Page의 크기를 가진다.
	// 2. 각각의 데이터 형에 배열 처럼 접근하여 사용 할 수 있다.
	// 3. MMData_Base를 상속 받은 클래스에는 프로퍼티를 정의하여 배열 접근에 용이하게 한다.
	// 4. 메모리 맵 파일 클래스를 생성 할 때는 CreateMemMap() 함수를 이용하여 만든다.
	// 
	// **1Page: MMData_Base에 정의된 데이터 크기를 의미하며 고정된 크기를 가진다.
	//

	/// <summary>
	/// 메모리 맵 베이스 클래스 입니다.
	/// 이 클래스를 상속 받으면 고정된 크기의 메모리 블럭을 생성 합니다.
	/// </summary>
	public class CMemoryMapData_PageBase : IDisposable
	{
		/// <summary>
		/// 메모리 맵 파일 클래스 입니다.
		/// </summary>
		private MemoryMappedFile _memMap;
		/// <summary>
		/// 바이트형 데이터 클래스 입니다.
		/// </summary>
		private CMemoryMapData_Byte _dataByte;
		/// <summary>
		/// 불형 데이터 클래스 입니다.
		/// </summary>
		private CMemoryMapData_Bool _dataBool;
		/// <summary>
		/// 쇼트형 데이터 클래스 입니다.
		/// </summary>
		private CMemoryMapData_Short _dataShort;
		/// <summary>
		/// 인트형 데이터 클래스 입니다.
		/// </summary>
		private CMemoryMapData_Int _dataInt;
		/// <summary>
		/// 플롯형 데이터 클래스 입니다.
		/// </summary>
		private CMemoryMapData_Float _dataFloat;
		/// <summary>
		/// 더블형 데이터 클래스 입니다.
		/// </summary>
		private CMemoryMapData_Double _dataDouble;
		/// <summary>
		/// 스트링형 데이터 클래스 입니다.
		/// </summary>
		private CMemoryMapData_String _dataString;

		/// <summary>
		/// 바이트형 데이터의 크기 입니다.
		/// </summary>
		public static readonly long DataSizeByte = 1024 * 1024;        //=  15 MegaByte
		/// <summary>
		/// 불형 데이터의 크기 입니다.
		/// </summary>
		public static readonly long DataSizeBool = 2048 * 1;        //=  2,048 byte
		/// <summary>
		/// 쇼트형 데이터의 크기 입니다.
		/// </summary>
		public static readonly long DataSizeShort = 512 * 2;          //=  327,676 byte
		/// <summary>
		/// 인트형 데이터의 크기 입니다.
		/// </summary>
		public static readonly long DataSizeInt = 512 * 4;          //=  2,048 byte
		/// <summary>
		/// 플롯형 데이터의 크기 입니다.
		/// </summary>
		public static readonly long DataSizeFloat = 512 * 4;        //=  2,048 byte
		/// <summary>
		/// 더블형 데이터의 크기 입니다.
		/// </summary>
		public static readonly long DataSizeDouble = 512 * 8;        //=  4,096 byte
		/// <summary>
		/// 스트링형 데이터의 크기 입니다.
		/// </summary>
		public static readonly long DataSizeString = 128 * 512 * 1; //= 65,536 byte
		/// <summary>
		/// 메모리 블럭 하나의 크기 입니다.
		/// </summary>
		public static readonly long PageSize = ( 1024 * 1024 ) + ( 2048 * 1 ) + ( 512 * 2 ) + ( 512 * 4 ) + ( 512 * 4 ) + ( 512 * 8 ) + ( 128 * 512 * 1 ); // = ByteDataSize + IntDataSize + FloatDataSize + DoubleDataSize + StringDataSize

		/// <summary>
		/// 메모리 맵 베이스의 생성자 입니다.
		/// </summary>
		/// <param name="fileName">메모리맵 파일 이름. 기존에 같은 이름의 파일이 존재하지 않으면 만들고, 있으면 파일을 엽니다.</param>
		/// <param name="pageIndex">메모리 블럭 인덱스. 메모리 블럭의 인덱스 입니다. 인덱스가 메모리 블럭 보다 커지면 익셉션을 발생시킵니다.</param>
		/// <param name="pageCount">메모리 블럭 개수. 메모리 블럭 개수를 참고하여 메모리 맵 파일을 생성합니다. 메모리 블럭은 0보다 커야합니다.</param>
		protected CMemoryMapData_PageBase( MemoryMappedFile memMap, uint pageIndex, uint pageCount )
		{
			// 메모리 맵 파일을 엽니다.
			_memMap = memMap;

			// 메모리 시작 위치를 구합니다. (byte)
			long startPos;
			if( pageIndex >= pageCount ) {
				throw new IndexOutOfRangeException();
			} else {
				startPos = PageSize * pageIndex;
			}
			// 메모리 맵 뷰 클래스를 생성합니다.
			_dataByte = new CMemoryMapData_Byte( _memMap, startPos, DataSizeByte, MemoryMappedFileAccess.ReadWrite );
			startPos += DataSizeByte;
			_dataBool = new CMemoryMapData_Bool( _memMap, startPos, DataSizeBool, MemoryMappedFileAccess.ReadWrite );
			startPos += DataSizeBool;
			_dataShort = new CMemoryMapData_Short( _memMap, startPos, DataSizeShort, MemoryMappedFileAccess.ReadWrite );
			startPos += DataSizeShort;
			_dataInt = new CMemoryMapData_Int( _memMap, startPos, DataSizeInt, MemoryMappedFileAccess.ReadWrite );
			startPos += DataSizeInt;
			_dataFloat = new CMemoryMapData_Float( _memMap, startPos, DataSizeFloat, MemoryMappedFileAccess.ReadWrite );
			startPos += DataSizeFloat;
			_dataDouble = new CMemoryMapData_Double( _memMap, startPos, DataSizeDouble, MemoryMappedFileAccess.ReadWrite );
			startPos += DataSizeDouble;
			_dataString = new CMemoryMapData_String( _memMap, startPos, DataSizeString, MemoryMappedFileAccess.ReadWrite );
		}

		/// <summary>
		/// 지정한 크기의 메모리 맵 파일을 엽니다. 만약 파일이 존재 하지 않는다면 파일을 생성합니다.
		/// </summary>
		/// <param name="fileName">메모리 맵 파일 이름</param>
		/// <param name="mapName">메모리 맵 이름</param>
		/// <param name="pageCount">페이지 개수 (메모리 맵 파일의 총 크기를 결정 한다.)</param>
		/// <returns>메모리 맵 파일 클래스</returns>
		public static MemoryMappedFile CreateMemoryMap( string fileName, string mapName, uint pageCount )
		{
			// 메모리 맵 파일 크기를 구합니다.
			long capacity;
			if( pageCount < 1 ) {
				capacity = PageSize;
			} else {
				capacity = PageSize * pageCount;
			}
			// 디렉토리를 확인하고 없으면 생성합니다.
			var dirName = Path.GetDirectoryName( fileName );
			if( false == Directory.Exists( dirName ) ) {
				Directory.CreateDirectory( dirName );
			}

			// 메모리 맵 파일을 엽니다.
			try {
				using( MemoryMappedFile.OpenExisting( mapName ) ) {
				}
			}
			catch( System.IO.FileNotFoundException ) {
				return MemoryMappedFile.CreateFromFile( fileName, FileMode.OpenOrCreate, mapName, capacity, MemoryMappedFileAccess.ReadWriteExecute );
			}
			return MemoryMappedFile.OpenExisting( mapName, MemoryMappedFileRights.ReadWrite );
		}

		/// <summary>
		/// 바이트형 데이터에 접근합니다.
		/// </summary>
		public CMemoryMapData_Byte DataByte
		{
			get { return _dataByte; }
			set { _dataByte = value; }
		}

		/// <summary>
		/// 바이트형 데이터에 접근합니다.
		/// </summary>
		public CMemoryMapData_Bool DataBool
		{
			get { return _dataBool; }
			set { _dataBool = value; }
		}

		/// <summary>
		/// 쇼트형 데이터에 접근합니다.
		/// </summary>
		public CMemoryMapData_Short DataShort
		{
			get { return _dataShort; }
			set { _dataShort = value; }
		}

		/// <summary>
		/// 인트형 데이터에 접근합니다.
		/// </summary>
		public CMemoryMapData_Int DataInt
		{
			get { return _dataInt; }
			set { _dataInt = value; }
		}

		/// <summary>
		/// 플롯형 데이터에 접근합니다.
		/// </summary>
		public CMemoryMapData_Float DataFloat
		{
			get { return _dataFloat; }
			set { _dataFloat = value; }
		}

		/// <summary>
		/// 더블형 데이터에 접근합니다.
		/// </summary>
		public CMemoryMapData_Double DataDouble
		{
			get { return _dataDouble; }
			set { _dataDouble = value; }
		}

		/// <summary>
		/// 스트링형 데이터에 접근합니다.
		/// </summary>
		public CMemoryMapData_String DataString
		{
			get { return _dataString; }
			set { _dataString = value; }
		}

		/// <summary>
		/// 입력한 데이터를 내부 데이터 클래스에 복사한다.
		/// </summary>
		/// <param name="sourceData">복사 할 데이터</param>
		public void Copy( CMemoryMapData_PageBase sourceData )
		{
			_dataByte.Copy( sourceData.DataByte );
			_dataBool.Copy( sourceData.DataBool );
			_dataShort.Copy( sourceData.DataShort );
			_dataInt.Copy( sourceData.DataInt );
			_dataFloat.Copy( sourceData.DataFloat );
			_dataDouble.Copy( sourceData.DataDouble );
			_dataString.Copy( sourceData.DataString );
		}

		/// <summary>
		/// 내부 데이터를 클리어 합니다.
		/// </summary>
		public void Clear()
		{
			_dataByte.Clear();
			_dataBool.Clear();
			_dataShort.Clear();
			_dataInt.Clear();
			_dataFloat.Clear();
			_dataDouble.Clear();
			_dataString.Clear();
		}

		/// <summary>
		/// Dispose()를 호출하면 메모리를 해제합니다.
		/// </summary>
		public void Dispose()
		{
			_dataString.Dispose();
			_dataDouble.Dispose();
			_dataFloat.Dispose();
			_dataShort.Clear();
			_dataInt.Dispose();
			_dataBool.Dispose();
			_dataByte.Dispose();
			_memMap.Dispose();
		}
	}
}