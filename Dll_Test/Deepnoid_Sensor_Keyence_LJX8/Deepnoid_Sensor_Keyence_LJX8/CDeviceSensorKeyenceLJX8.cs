using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Deepnoid_Sensor_Keyence_LJX8
{
	public class CDeviceSensorKeyenceLJX8 : CDeviceSensorAbstract
	{
		/// <summary>
		/// 카메라 인덱스
		/// </summary>
		private int m_iSensorIndex;
		/// <summary>
		/// 초기화 파라매터
		/// </summary>
		private CInitializeParameterLJX8 m_objInitializeParameter;
		/// <summary>
		/// 센서 연결 체크 스레드
		/// </summary>
		private bool m_bThreadExit;
		private Thread m_ThreadConnect;
		/// <summary>
		/// 연결 여부
		/// </summary>
		private bool m_bConnect = false;
		private bool m_bHighSpeedConnect = false;
		/// <summary>
		/// Sensor Lock 체크 객체
		/// </summary>
		private object m_objSensorLock;
		/// <summary>
		/// 고속 데이터 통신 콜백
		/// </summary>
		private HighSpeedDataCallBack m_objHighSpeedDataCallBack;
		/// <summary>
		/// 프로파일 정보 
		/// </summary>
		private LJX8IF_PROFILE_INFO m_objProfileInfo;

		public CDeviceSensorKeyenceLJX8()
		{
			m_bThreadExit = false;
			_callBackScanData = null;
			_callBackMessage = null;
			_callBackExceptionMessage = null;
			m_objHighSpeedDataCallBack = null;
			m_objInitializeParameter = new CInitializeParameterLJX8();
			m_objProfileInfo = new LJX8IF_PROFILE_INFO();
		}
		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objInitializeParameter"></param>
		/// <returns></returns>
		public override bool Initialize( CInitializeParameterLJX8 objInitializeParameter )
		{
			bool bReturn = false;

			do {
				try {
					// 초기화 파라매터 복사
					m_objInitializeParameter = ( CInitializeParameterLJX8 )objInitializeParameter.Clone();

					m_iSensorIndex = m_objInitializeParameter.iSensorIndex;
					// Lock 객체 생성
					m_objSensorLock = new object();
					// 센서 자체 데이터 콜백
					m_objHighSpeedDataCallBack = new HighSpeedDataCallBack( ReceiveHighSpeedData );
					// 연결 체크 스레드 시작
					m_ThreadConnect = new Thread( ThreadConnect );
					m_ThreadConnect.Start( this );
					bReturn = true;
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
				}
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 해제
		/// </summary>
		public override void DeInitialize()
		{
			// 접속 스레드 종료
			m_bThreadExit = true;
			if( m_ThreadConnect != null )
				m_ThreadConnect.Join();

			// 센서 연결 해제.
			DisconnectSensor();
		}
		/// <summary>
		/// 연결 상태
		/// </summary>
		/// <returns></returns>
		public override bool IsConnected()
		{
			bool bReturn = false;

			do {
				bReturn = m_bConnect;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 스캔 시작
		/// </summary>
		/// <returns></returns>
		public override bool ScanStart()
		{
			bool bReturn = false;
			try {
				do {
					// 컨트롤러의 메모리 초기화.
					ClearMemory();
					// 우선 프로파일 1개씩 받는 걸로 하고 나중에 밖으로 빼든가 해야겠다.
					// 고속 데이터 통신 연결
					if( false == InitializeHighSpeedDataCommunication( 1 ) ) {
						break;
					}
					// 고속 데이터 사전 준비
					if( false == PreStartHighSpeedDataCommunication( 2 ) ) {
						break;
					}
					// 고속 데이터 통신 시작
					if( false == StartHighSpeedDataCommunication() ) {
						break;
					}
					// 측정 시작.
					if( false == StartMeasure() ) {
						break;
					}

					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 스캔 정지
		/// </summary>
		/// <returns></returns>
		public override bool ScanStop()
		{
			bool bReturn = false;
			try {
				do {
					// 측정 정지.
					if( false == StopMeasure() ) {
						break;
					}
					// 고속 데이터 통신 정지.
					if( false == StopHighSpeedDataCommunication() ) {
						break;
					}
					// 고속 데이터 통신 종료.
					if( false == FinalizeHighSpeedDataCommunication() ) {
						break;
					}
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 센서 리셋
		/// </summary>
		/// <param name="bWait"></param>
		/// <returns></returns>
		public override bool SensorReset( bool bWait )
		{
			bool bReturn = false;
			try {
				do {
					int iReturnCode = NativeMethodsLJX8000A.LJX8IF_RebootController( m_iSensorIndex );
					if( iReturnCode != ( int )Rc.Ok ) {
						break;
					}
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 고속 데이터 통신 초기화
		/// </summary>
		/// <param name="iReceiveDataSize"></param>
		/// <returns></returns>
		public bool InitializeHighSpeedDataCommunication( int iReceiveDataSize )
		{
			bool bReturn = false;
			try {
				do {
					// 버퍼 클리어
					ThreadSafeBuffer.ClearBuffer( m_iSensorIndex );
					// 고속 통신 종료 확인.

					// 고속 데이터 통신 연결.
					LJX8IF_ETHERNET_CONFIG ethernetConfig = new LJX8IF_ETHERNET_CONFIG();
					string[] strIpAddress;
					strIpAddress = m_objInitializeParameter.strSensorIP.Split( '.' );
					ethernetConfig.abyIpAddress = new byte[] {
						Convert.ToByte( strIpAddress[ 0 ] ),
						Convert.ToByte( strIpAddress[ 1 ] ),
						Convert.ToByte( strIpAddress[ 2 ] ),
						Convert.ToByte( strIpAddress[ 3 ] ),
					};
					ethernetConfig.wPortNo = Convert.ToUInt16( m_objInitializeParameter.strSensorHighSpeedDataPort );
					int iReturnCode = NativeMethodsLJX8000A.LJX8IF_InitializeHighSpeedDataCommunication( m_iSensorIndex, ref ethernetConfig,
						ethernetConfig.wPortNo, m_objHighSpeedDataCallBack, ( uint )iReceiveDataSize, ( uint )m_iSensorIndex );

					// 추후에 고속 데이터 통신 연결에 문제가 생길 경우 여기서 재접속 후 진행하는 걸로 변경해보자.
					if( iReturnCode != ( int )Rc.Ok ) {
						break;
					}
					// 통신 연결 확인 
					m_bHighSpeedConnect = true;
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 고속 데이터 시작 사전 준비 요청 
		/// </summary>
		/// <param name="iSendPosition">
		/// 0 - 이전 송신 완료 위치 부터
		/// 1 - 가장 오랜된 데이터 부터
		/// 2 - 다음 데이터부터 
		/// 보통 2번으로 사용함.
		/// </param>
		/// <returns></returns>
		public bool PreStartHighSpeedDataCommunication( int iSendPosition )
		{
			bool bReturn = false;
			try {
				do {
					// 고속 데이터 통신 시작 준비 요청
					LJX8IF_HIGH_SPEED_PRE_START_REQUEST request = new LJX8IF_HIGH_SPEED_PRE_START_REQUEST();
					request.bySendPosition = Convert.ToByte( iSendPosition );
					LJX8IF_PROFILE_INFO profileInfo = new LJX8IF_PROFILE_INFO();
					int iReturnCode = NativeMethodsLJX8000A.LJX8IF_PreStartHighSpeedDataCommunication( m_iSensorIndex, ref request, ref profileInfo );
					if( iReturnCode != ( int )Rc.Ok ) {
						break;
					}
					m_objProfileInfo = profileInfo;
					// 프로파일 데이터 저장은 어떻게 밖에서 하나???

					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 고속 데이터 통신 시작
		/// </summary>
		/// <returns></returns>
		public bool StartHighSpeedDataCommunication()
		{
			bool bReturn = false;
			try {
				do {
					// 버퍼 클리어 
					ThreadSafeBuffer.ClearBuffer( m_iSensorIndex );
					// 고속 데이터 통신 시작
					int iReturnCode = NativeMethodsLJX8000A.LJX8IF_StartHighSpeedDataCommunication( m_iSensorIndex );
					if( iReturnCode != ( int )Rc.Ok ) {
						break;
					}
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 고속 데이터 통신 해제( 종료 )
		/// </summary>
		/// <returns></returns>
		public bool FinalizeHighSpeedDataCommunication()
		{
			bool bReturn = false;
			try {
				do {
					// 고속 데이터 통신 종료
					int iReturnCode = NativeMethodsLJX8000A.LJX8IF_FinalizeHighSpeedDataCommunication( m_iSensorIndex );
					if( iReturnCode != ( int )Rc.Ok ) {
						break;
					}
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {
				m_bHighSpeedConnect = false;
			}
			return bReturn;
		}
		/// <summary>
		/// 측정 시작
		/// </summary>
		/// <returns></returns>
		public bool StartMeasure()
		{
			bool bReturn = false;
			try {
				do {
					// 측정 시작
					int iReturnCode = NativeMethodsLJX8000A.LJX8IF_StartMeasure( m_iSensorIndex );
					if( iReturnCode != ( int )Rc.Ok ) {
						break;
					}
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {
				m_bHighSpeedConnect = false;
			}
			return bReturn;
		}
		/// <summary>
		/// 고속 데이터 통신 정지
		/// </summary>
		/// <returns></returns>
		public bool StopHighSpeedDataCommunication()
		{
			bool bReturn = false;
			try {
				do {
					// 고속 데이터 통신 정지
					int iReturnCode = NativeMethodsLJX8000A.LJX8IF_StopHighSpeedDataCommunication( m_iSensorIndex );
					if( iReturnCode != ( int )Rc.Ok ) {
						break;
					}
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 측정 정지
		/// </summary>
		/// <returns></returns>
		public bool StopMeasure()
		{
			bool bReturn = false;
			try {
				do {
					// 측정 정지
					int iReturnCode = NativeMethodsLJX8000A.LJX8IF_StopMeasure( m_iSensorIndex );
					if( iReturnCode != ( int )Rc.Ok ) {
						break;
					}
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {
				m_bHighSpeedConnect = false;
			}
			return bReturn;
		}
		/// <summary>
		/// 노출값 설정
		/// </summary>
		/// <param name="iExposureIndex"></param>
		/// <returns></returns>
		public bool SetExposureTime( int iExposureIndex )
		{
			bool bReturn = false;
			try {
				do {
					// 고속데이터 통신 연결이 되어 있을 경우 설정값을 변경할 수 없다.
					if( true == m_bHighSpeedConnect ) {
						string strError = $"Sensor {m_iSensorIndex} - Failed to change the exposure time due to interference from high-speed data communication.";
						_callBackMessage?.Invoke( strError );
						break;
					}
					LJX8IF_TARGET_SETTING targetSetting;
					// Exposure Time
					targetSetting.byType = 0x10;
					targetSetting.byCategory = 0x1;
					targetSetting.byItem = 0x6;
					targetSetting.reserve = 0x0;
					targetSetting.byTarget1 = 0x0;
					targetSetting.byTarget2 = 0x0;
					targetSetting.byTarget3 = 0x0;
					targetSetting.byTarget4 = 0x0;
					string trimStrings = iExposureIndex.ToString( "X" );

					byte[] data = null;
					if( 0 < trimStrings.Length ) {
						string[] parameterTexts = trimStrings.Split( ',' );
						if( 0 < parameterTexts.Length )
							data = Array.ConvertAll( parameterTexts, delegate ( string text ) { return Convert.ToByte( text, 16 ); } );

						Array.Resize( ref data, 1 );

						using( PinnedObject pin = new PinnedObject( data ) ) {
							uint error = 0;
							int iReturnCode = NativeMethodsLJX8000A.LJX8IF_SetSetting( m_iSensorIndex, ( byte )LJX8IF_SETTING_DEPTH.LJX8IF_SETTING_DEPTH_RUNNING, targetSetting, pin.Pointer, 1, ref error );

							if( iReturnCode == ( int )Rc.Ok ) {
								bReturn = true;
							} else {
								break;
							}
						}
					}
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 프로파일 데이터 가져오기
		/// </summary>
		/// <param name="iBatchPoint">배치 점수( 트리커 카운터 )</param>
		/// <returns></returns>
		public bool GetProfileData( uint iBatchPoint )
		{
			bool bReturn = false;
			try {
				do {
					uint iNotify;
					int iBatchNo;
					// 버퍼의 갯수를 BatchPoint 보다 작을 경우 아직 스캔완료 안되었다고 판단하자...
					uint iBufferCount = ThreadSafeBuffer.GetCount( m_iSensorIndex, out iNotify, out iBatchNo );
					if( iBatchPoint > iBufferCount ) break;

					// 완료 후 데이터 가져오기... 
					List<int[]> data = ThreadSafeBuffer.Get( m_iSensorIndex, out iNotify, out iBatchNo );
					if( data.Count == 0 && iNotify == 0 ) break;

					CScanData objImageData = new CScanData();
					objImageData.bGrabComplete = true;
					// 프로파일 데이터 객체 리스트 생성 (헤더/푸터 정보 포함)
					List<ProfileData> objProfileData = new List<ProfileData>();
					foreach( int[] profile in data ) {
						if( objProfileData.Count < Define.BufferFullCount ) { // Define.BufferFullCount:  최대 프로파일 데이터 개수 제한
							objProfileData.Add( new ProfileData( profile, m_objProfileInfo ) );
						}
					}
					// 실제 사용 데이터 (높이, 휘도) 분리 및 CImageData 에 저장
					ConvertProfileDataToImageData( objProfileData, objImageData );
					_callBackScanData?.Invoke( objImageData );
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 프로파일 데이터를 전송할 데이터로 변환
		/// </summary>
		/// <param name="objProfileData">획득한 프로파일 데이터</param>
		/// <param name="objImageData">획득한 프로파일 데이터를 가공하여 전송 데이터 </param>
		private void ConvertProfileDataToImageData( List<ProfileData> objProfileData, CScanData objImageData )
		{
			const double ProfileDataScaleFactor = 160.0; // 프로파일 데이터 스케일 팩터 (단위: 0.1um)
			const double UnitConversionFactor = 100000.0; // 단위 변환 계수 (um -> mm)

			foreach( var item in objProfileData ) {
				int halfProfileLength = ( int )( item.ProfData.Count() / 2 );
				int[] iProfileData = new int[ halfProfileLength ];
				int[] iLuminanceData = new int[ halfProfileLength ];
				Array.Copy( item.ProfData, 0, iProfileData, 0, iProfileData.Length );
				Array.Copy( item.ProfData, halfProfileLength, iLuminanceData, 0, halfProfileLength );
				objImageData.objSensorDataKeyenceLJX8.objListProfileData.Add( iProfileData );
				objImageData.objSensorDataKeyenceLJX8.objListLuminanceData.Add( iLuminanceData );
			}
			// double 형으로 변환하자.
			for( int iLoopCount = 0; iLoopCount < objProfileData.Count; iLoopCount++ ) {
				double[] dProfileData = new double[ objProfileData[ iLoopCount ].ProfData.Length ];
				Parallel.For( 0, objProfileData[ iLoopCount ].ProfData.Length, iLoopProfileCount => {
					// 높이 데이터:  배열 앞쪽 절반 (인덱스 0 ~ Length/2 - 1)
					if( iLoopProfileCount < objProfileData[ iLoopCount ].ProfData.Length / 2 ) {
						// Keyence LJX8000A 센서 높이 데이터 스케일링 및 단위 변환 (0.1um -> mm)
						dProfileData[ iLoopProfileCount ] = ( int )Math.Truncate( objProfileData[ iLoopCount ].ProfData[ iLoopProfileCount ] / ProfileDataScaleFactor );
						dProfileData[ iLoopProfileCount ] = dProfileData[ iLoopProfileCount ] * ProfileDataScaleFactor / UnitConversionFactor;
					}
					// 휘도 데이터: 배열 뒤쪽 절반 (인덱스 Length/2 ~ Length - 1)
					else {
						dProfileData[ iLoopProfileCount ] = objProfileData[ iLoopCount ].ProfData[ iLoopProfileCount ]; // 휘도 데이터는 별도 변환 없이 그대로 사용
					}
				} );
				objImageData.objSensorDataKeyenceLJX8.objListProfileDoubleData.Add( dProfileData );
			}
		}
		/// <summary>
		/// 프로파일 데이터 정보 
		/// </summary>
		/// <param name="objProfileInfo"></param>
		/// <returns></returns>
		public bool GetProfileInfo( ref LJX8IF_PROFILE_INFO objProfileInfo )
		{
			bool bReturn = false;
			try {
				do {
					objProfileInfo = m_objProfileInfo;
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 컨트롤러 내부 메모리 초기화
		/// </summary>
		public void ClearMemory()
		{
			NativeMethodsLJX8000A.LJX8IF_ClearMemory( m_iSensorIndex );
		}

		private bool ConnectSensor()
		{
			bool bReturn = false;
			try {
				do {
					LJX8IF_ETHERNET_CONFIG ethernetConfig = new LJX8IF_ETHERNET_CONFIG();
					string[] strIpAddress;
					strIpAddress = m_objInitializeParameter.strSensorIP.Split( '.' );
					ethernetConfig.abyIpAddress = new byte[] {
						Convert.ToByte( strIpAddress[ 0 ] ),
						Convert.ToByte( strIpAddress[ 1 ] ),
						Convert.ToByte( strIpAddress[ 2 ] ),
						Convert.ToByte( strIpAddress[ 3 ] ),
					};
					ethernetConfig.wPortNo = Convert.ToUInt16( m_objInitializeParameter.strSensorPort );

					// 카메라 연결. 
					int iReturn = NativeMethodsLJX8000A.LJX8IF_EthernetOpen( m_iSensorIndex, ref ethernetConfig );
					if( iReturn != ( int )Rc.Ok ) {
						string strError = $"Sensor {m_iSensorIndex} - Connect is Fail";
						_callBackMessage?.Invoke( strError );
						break;
					}
					bReturn = true;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
				bReturn = true;
			} finally {

			}
			return bReturn;
		}
		/// <summary>
		/// 센서 연결 해제
		/// </summary>
		private void DisconnectSensor()
		{
			try {
				do {
					// 카메라 접속 해제.
					int iReturn = NativeMethodsLJX8000A.LJX8IF_CommunicationClose( m_iSensorIndex );
					if( iReturn != ( int )Rc.Ok ) {
						string strError = $"Sensor {m_iSensorIndex} - Disconnect is Fail";
						_callBackMessage?.Invoke( strError );
						break;
					}
					m_bConnect = false;
				} while( false );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"Sensor {m_iSensorIndex} {strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			} finally {

			}
		}
		/// <summary>
		/// 센서 데이터 콜백 함수.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="size"></param>
		/// <param name="count"></param>
		/// <param name="notify"></param>
		/// <param name="user"></param>
		private void ReceiveHighSpeedData( IntPtr buffer, uint size, uint count, uint notify, uint user )
		{
			// @Point
			// Take care to only implement storing profile data in a thread save buffer in the callback function.
			// As the thread used to call the callback function is the same as the thread used to receive data,
			// the processing time of the callback function affects the speed at which data is received,
			// and may stop communication from being performed properly in some environments.
			uint profileSize = ( uint )( size / Marshal.SizeOf( typeof( int ) ) );
			List<int[]> receiveBuffer = new List<int[]>();
			int[] bufferArray = new int[ ( int )( profileSize * count ) ];
			Marshal.Copy( buffer, bufferArray, 0, ( int )( profileSize * count ) );

			// Profile data retention
			for( int i = 0; i < ( int )count; i++ ) {
				int[] oneProfile = new int[ ( int )profileSize ];
				Array.Copy( bufferArray, i * profileSize, oneProfile, 0, profileSize );
				receiveBuffer.Add( oneProfile );
			}
			if( ThreadSafeBuffer.GetBufferDataCount( ( int )user ) + receiveBuffer.Count < Define.BufferFullCount ) {
				ThreadSafeBuffer.Add( ( int )user, receiveBuffer, notify );
			} else {
				//_isBufferFull[ ( int )user ] = true;
				//m_objError.strMessage = $"Sensor {m_iSensorIndex} ReceiveHighSpeedData - Buffer Full Error.";
				//m_objError.strEventTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff");
				//m_objError.strFunctionName = "ReceiveHighSpeedData";
				//// 에러 메시지 호출.
				//m_objCallbackGrabError();
			}
		}
		/// <summary>
		/// 센서 핑 테스트 결과.
		/// </summary>
		/// <param name="iSensorCheck"></param>
		/// <returns></returns>
		private bool PingReplyFailSensor( ref int iSensorCheck )
		{
			bool bReturn = false;
			Ping pingSender = new Ping();

			do {
				// Ping 명령 전송
				PingReply pingReply = pingSender?.Send( m_objInitializeParameter.strSensorIP, 200 );
				if( pingReply.Status != IPStatus.Success ) {
					iSensorCheck++;
				} else {
					iSensorCheck = 0;
				}
				// 3보다 크거나 같을 경우 연결이 끊긴 걸로 간주한다.
				if( iSensorCheck >= 3 ) break;

				bReturn = true;
			} while( false );
			return bReturn;
		}

		/// <summary>
		/// 고게이터 접속 상태 체크 쓰레드
		/// </summary>
		/// <param name="state"></param>
		private static void ThreadConnect( Object state )
		{
			CDeviceSensorKeyenceLJX8 pThis = ( CDeviceSensorKeyenceLJX8 )state;
			int iSensorCheck = 0;
			while( false == pThis.m_bThreadExit ) {
				// 센서 연결 상태가 False일 경우 
				if( false == pThis.m_bConnect ) {
					pThis.m_bConnect = pThis.ConnectSensor();
				} else {
					// 센서 핑테스트 실패시 재 접속 하자.
					if( false == pThis.PingReplyFailSensor( ref iSensorCheck ) ) {
						pThis.m_bConnect = false;
					}
				}
				Thread.Sleep( 1000 );
			}
		}
	}
}
