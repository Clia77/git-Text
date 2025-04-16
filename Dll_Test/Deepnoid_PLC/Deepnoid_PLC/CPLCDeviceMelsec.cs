using Deepnoid_Communication;
using System.Xml.Linq;

namespace Deepnoid_PLC
{
	public class CPLCDeviceMelsec : CPLCDeviceAbstract
	{
		/// <summary>
		/// 실제 plc랑 인터페이스 하는 부분 현재 socket or cc-link 클래스
		/// </summary>
		private CPLCInterfaceMelsecAbstract m_objPLCInterface;

		/// <summary>
		/// 맵 데이터 변경 콜백 처리
		/// </summary>
		/// <param name="obj"></param>
		private void MapDataChanged( CPLCMapDataChanged[] obj )
		{
			_callBackMapDataChanged?.Invoke( obj );
		}

		/// <summary>
		/// 수신 데이터 콜백 처리
		/// </summary>
		/// <param name="obj"></param>
		private void ReceiveData( CReceiveData obj )
		{
			_callBackReceiveData?.Invoke( obj );
		}

		/// <summary>
		/// 에러 메세지 콜백 처리
		/// </summary>
		/// <param name="strErrorMessage"></param>
		private void ErrorMessage( string strErrorMessage )
		{
			_callBackErrorMessage?.Invoke( strErrorMessage );
		}

		public CPLCDeviceMelsec( CPLCInterfaceMelsecAbstract objPLCInterface )
		{
			m_objPLCInterface = objPLCInterface;
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

			m_objPLCInterface?.SetCallBackReceiveData( ReceiveData );
			m_objPLCInterface?.SetCallBackErrorMessage( ErrorMessage );
			if( false == m_objPLCInterface?.Initialize( objParameter.objParameter ) ) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// 해제
		/// </summary>
		public override void DeInitialize()
		{
			m_objPLCInterface?.DeInitialize();
		}

		/// <summary>
		/// 접속 유무
		/// </summary>
		/// <returns></returns>
		public override bool IsConnected()
		{
			return ( m_objPLCInterface?.IsConnected() ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void ReadBit( string strName, int iCount )
		{
			CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
			if( null == objParameter ) {
				return;
			}
			// plc read -> set map data
			bool[] bData = new bool[ iCount ];
			if( false == m_objPLCInterface?.ReadBit( objParameter.strAddress, ref bData ) ) {
				return;
			}
			if( false == m_objMapData?.SetValueName( strName, bData ) ) {
				return;
			}
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void WriteBit( string strName, int iCount )
		{
			CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
			if( null == objParameter ) {
				return;
			}
			// get map data -> plc write
			bool[] bData = new bool[ iCount ];
			if( false == m_objMapData?.GetValueName( strName, ref bData ) ) {
				return;
			}
			if( false == m_objPLCInterface?.WriteBit( objParameter.strAddress, bData ) ) {
				return;
			}
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void ReadWord( string strName, int iCount )
		{
			CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
			if( null == objParameter ) {
				return;
			}
			// plc read -> set map data
			int[] iData = new int[ iCount ];
			if( false == m_objPLCInterface?.ReadWord( objParameter.strAddress, ref iData ) ) {
				return;
			}
			if( false == m_objMapData?.SetValueName( strName, iData ) ) {
				return;
			}
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void WriteWord( string strName, int iCount )
		{
			CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
			if( null == objParameter ) {
				return;
			}
			// get map data -> plc write
			int[] iData = new int[ iCount ];
			if( false == m_objMapData?.GetValueName( strName, ref iData ) ) {
				return;
			}
			if( false == m_objPLCInterface?.WriteWord( objParameter.strAddress, iData ) ) {
				return;
			}
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double word 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void ReadDoubleWord( string strName, int iCount )
		{
			CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
			if( null == objParameter ) {
				return;
			}
			// plc read -> set map data
			double[] dData = new double[ iCount ];
			if( false == m_objPLCInterface?.ReadDoubleWord( objParameter.strAddress, ref dData ) ) {
				return;
			}
			if( false == m_objMapData?.SetValueName( strName, dData ) ) {
				return;
			}
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double word 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		public override void WriteDoubleWord( string strName, int iCount )
		{
			CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
			if( null == objParameter ) {
				return;
			}
			// get map data -> plc write
			double[] dData = new double[ iCount ];
			if( false == m_objMapData?.GetValueName( strName, ref dData ) ) {
				return;
			}
			if( false == m_objPLCInterface?.WriteDoubleWord( objParameter.strAddress, dData ) ) {
				return;
			}
		}

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref bool bData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.ReadBit( objParameter.strAddress, ref bData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, bData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref bool[] bData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.ReadBit( objParameter.strAddress, ref bData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, bData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool Write( string strName, bool bData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.WriteBit( objParameter.strAddress, bData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, bData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool Write( string strName, bool[] bData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.WriteBit( objParameter.strAddress, bData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, bData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref int iData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.ReadWord( objParameter.strAddress, ref iData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, iData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref int[] iData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.ReadWord( objParameter.strAddress, ref iData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, iData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool Write( string strName, int iData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.WriteWord( objParameter.strAddress, iData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, iData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool Write( string strName, int[] iData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.WriteWord( objParameter.strAddress, iData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, iData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref double dData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.ReadDoubleWord( objParameter.strAddress, ref dData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, dData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool Read( string strName, ref double[] dData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.ReadDoubleWord( objParameter.strAddress, ref dData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, dData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool Write( string strName, double dData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.WriteDoubleWord( objParameter.strAddress, dData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, dData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool Write( string strName, double[] dData )
		{
			bool bReturn = false;

			do {
				CPLCMapDataParameter objParameter = m_objMapData?.GetParameterWithName( strName );
				if( null == objParameter ) {
					break;
				}
				if( false == m_objPLCInterface?.WriteDoubleWord( objParameter.strAddress, dData ) ) {
					break;
				}
				if( false == m_objMapData?.SetValueName( strName, dData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}
	}
}