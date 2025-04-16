using Deepnoid_Communication;
using System.Data;
using static Deepnoid_PLC.CPLCDefine;

namespace Deepnoid_PLC
{
	public abstract class CPLCDeviceAbstract
	{
		protected object m_objLock;
		/// <summary>
		/// 정의한 어드레스 맵을 읽어들여서 데이터 동기화시키기 위함
		/// </summary>
		protected CPLCMapData m_objMapData;

		/// <summary>
		/// 맵 데이터 변경 콜백 처리
		/// </summary>
		/// <param name="objReceiveData"></param>
		public delegate void CallBackMapDataChanged( CPLCMapDataChanged[] obj );
		protected CallBackMapDataChanged _callBackMapDataChanged;
		public void SetCallBackMapDataChanged( CallBackMapDataChanged callBack )
		{
			_callBackMapDataChanged = callBack;
		}

		/// <summary>
		/// 수신 데이터 콜백 처리
		/// </summary>
		/// <param name="objReceiveData"></param>
		public delegate void CallBackReceiveData( CReceiveData obj );
		protected CallBackReceiveData _callBackReceiveData;
		public void SetCallBackReceiveData( CallBackReceiveData callBack )
		{
			_callBackReceiveData = callBack;
		}

		/// <summary>
		/// Error Message 콜백 처리
		/// </summary>
		/// <param name="strErrorMessage"></param>
		public delegate void CallBackErrorMessage( string strErrorMessage );
		protected CallBackErrorMessage _callBackErrorMessage;
		public void SetCallBackErrorMessage( CallBackErrorMessage callBack )
		{
			_callBackErrorMessage = callBack;
		}

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public abstract bool Initialize( CPLCDeviceParameter objParameter );

		/// <summary>
		/// 해제
		/// </summary>
		public abstract void DeInitialize();

		/// <summary>
		/// 접속 유무
		/// </summary>
		/// <returns></returns>
		public abstract bool IsConnected();

		/// <summary>
		/// 읽어온 맵 데이터를 DataTable 형식에 맞춰서 생성해서 리턴
		/// </summary>
		/// <returns></returns>
		public DataTable GetDataTable()
		{
			return m_objMapData?.GetDataTable();
		}

		/// <summary>
		/// 맵 데이터 내에 타입에 해당하는 수량 반환
		/// </summary>
		/// <param name="eCommunicationType"></param>
		/// <returns></returns>
		public int GetMapDataCount( enumPLCDeviceCommunicationType eCommunicationType )
		{
			return ( m_objMapData?.GetCount( eCommunicationType ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 맵 데이터에서 이름에 해당하는 value 값 get
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strName"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool GetValueName<T>( string strName, ref T objValue )
		{
			return ( m_objMapData?.GetValueName( strName, ref objValue ) ).GetValueOrDefault();
		}

		public bool GetValueName<T>( string strName, ref T[] objValue )
		{
			return ( m_objMapData?.GetValueName( strName, ref objValue ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 맵 데이터에서 어드레스에 해당하는 value 값 get
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strAddress"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool GetValueAddress<T>( string strAddress, ref T objValue )
		{
			return ( m_objMapData?.GetValueAddress( strAddress, ref objValue ) ).GetValueOrDefault();
		}

		public bool GetValueAddress<T>( string strAddress, ref T[] objValue )
		{
			return ( m_objMapData?.GetValueAddress( strAddress, ref objValue ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 맵 데이터에서 이름에 해당하는 value 값 set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strName"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool SetValueName<T>( string strName, T objValue )
		{
			return ( m_objMapData?.SetValueName( strName, objValue ) ).GetValueOrDefault();
		}

		public bool SetValueName<T>( string strName, T[] objValue )
		{
			return ( m_objMapData?.SetValueName( strName, objValue ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 맵 데이터에서 어드레스에 해당하는 value 값 set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strAddress"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool SetValueAddress<T>( string strAddress, T objValue )
		{
			return ( m_objMapData?.SetValueAddress( strAddress, objValue ) ).GetValueOrDefault();
		}

		public bool SetValueAddress<T>( string strAddress, T[] objValue )
		{
			return ( m_objMapData?.SetValueAddress( strAddress, objValue ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 리스트에서 이름에 해당하는 인덱스를 get
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iFindIndex"></param>
		/// <returns></returns>
		public bool GetFindIndexWithName( string strName, ref int iFindIndex )
		{
			return ( m_objMapData?.GetFindIndexWithName( strName, ref iFindIndex ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 리스트에서 어드레스에 해당하는 인덱스를 get
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iFindIndex"></param>
		/// <returns></returns>
		public bool GetFindIndexWithAddress( string strAddress, ref int iFindIndex )
		{
			return ( m_objMapData?.GetFindIndexWithAddress( strAddress, ref iFindIndex ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 리스트에서 이름에 해당하는 맵 데이터 객체를 get
		/// </summary>
		/// <param name="strName"></param>
		/// <returns></returns>
		public CPLCMapDataParameter GetParameterWithName( string strName )
		{
			return m_objMapData?.GetParameterWithName( strName );
		}

		/// <summary>
		/// 리스트에서 어드레스에 해당하는 맵 데이터 객체를 get
		/// </summary>
		/// <param name="strAddress"></param>
		/// <returns></returns>
		public CPLCMapDataParameter GetParameterWithAddress( string strAddress )
		{
			return m_objMapData?.GetParameterWithAddress( strAddress );
		}

		/// <summary>
		/// 리스트에서 인덱스에 해당하는 맵 데이터 객체를 get
		/// </summary>
		/// <param name="iIndex"></param>
		/// <returns></returns>
		public CPLCMapDataParameter GetParameterWithIndex( int iIndex )
		{
			return m_objMapData?.GetParameterWithIndex( iIndex );
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public abstract void ReadBit( string strName, int iCount );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public abstract void WriteBit( string strName, int iCount );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public abstract void ReadWord( string strName, int iCount );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public abstract void WriteWord( string strName, int iCount );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double word 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public abstract void ReadDoubleWord( string strName, int iCount );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double word 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public abstract void WriteDoubleWord( string strName, int iCount );

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public abstract bool Read( string strName, ref bool bData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public abstract bool Read( string strName, ref bool[] bData );

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public abstract bool Write( string strName, bool bData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public abstract bool Write( string strName, bool[] bData );

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public abstract bool Read( string strName, ref int iData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public abstract bool Read( string strName, ref int[] iData );

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public abstract bool Write( string strName, int iData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public abstract bool Write( string strName, int[] iData );

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public abstract bool Read( string strName, ref double dData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public abstract bool Read( string strName, ref double[] dData );

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public abstract bool Write( string strName, double dData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public abstract bool Write( string strName, double[] dData );
	}
}