namespace Deepnoid_PLC
{
	public class CPLCDeviceVirtual : CPLCDeviceAbstract
	{
		private bool m_bConnected;

		/// <summary>
		/// 맵 데이터 변경 콜백 처리
		/// </summary>
		/// <param name="obj"></param>
		private void MapDataChanged( CPLCMapDataChanged[] obj )
		{
			_callBackMapDataChanged?.Invoke( obj );
		}

		public CPLCDeviceVirtual()
		{
			m_bConnected = false;
		}

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public override bool Initialize( CPLCDeviceParameter objParameter )
		{
			m_objMapData = new CPLCMapData( objParameter.strMapDataPath );
			m_objMapData.SetCallBackMapDataChanged( MapDataChanged );
			m_bConnected = true;
			return true;
		}

		/// <summary>
		/// 해제
		/// </summary>
		public override void DeInitialize()
		{
			m_bConnected = false;
		}

		/// <summary>
		/// 접속 유무
		/// </summary>
		/// <returns></returns>
		public override bool IsConnected()
		{
			return m_bConnected;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void ReadBit( string strName, int iCount )
		{
			// virtual 은 plc 읽는 과정 필요 없음
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void WriteBit( string strName, int iCount )
		{
			// virtual 은 plc 쓰는 과정 필요 없음
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void ReadWord( string strName, int iCount )
		{
			// virtual 은 plc 읽는 과정 필요 없음
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void WriteWord( string strName, int iCount )
		{
			// virtual 은 plc 쓰는 과정 필요 없음
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double word 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void ReadDoubleWord( string strName, int iCount )
		{
			// virtual 은 plc 읽는 과정 필요 없음
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double word 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void WriteDoubleWord( string strName, int iCount )
		{
			// virtual 은 plc 쓰는 과정 필요 없음
		}

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref bool bData )
		{
			return ( m_objMapData?.GetValueName( strName, ref bData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref bool[] bData )
		{
			return ( m_objMapData?.GetValueName( strName, ref bData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool Write( string strName, bool bData )
		{
			return ( m_objMapData?.SetValueName( strName, bData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool Write( string strName, bool[] bData )
		{
			return ( m_objMapData?.SetValueName( strName, bData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref int iData )
		{
			return ( m_objMapData?.GetValueName( strName, ref iData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref int[] iData )
		{
			return ( m_objMapData?.GetValueName( strName, ref iData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool Write( string strName, int iData )
		{
			return ( m_objMapData?.SetValueName( strName, iData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool Write( string strName, int[] iData )
		{
			return ( m_objMapData?.SetValueName( strName, iData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref double dData )
		{
			return ( m_objMapData?.GetValueName( strName, ref dData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref double[] dData )
		{
			return ( m_objMapData?.GetValueName( strName, ref dData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool Write( string strName, double dData )
		{
			return ( m_objMapData?.SetValueName( strName, dData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool Write( string strName, double[] dData )
		{
			return ( m_objMapData?.SetValueName( strName, dData ) ).GetValueOrDefault();
		}
	}
}