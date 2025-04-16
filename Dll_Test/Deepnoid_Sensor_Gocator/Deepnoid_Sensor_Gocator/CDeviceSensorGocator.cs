using Lmi3d.GoSdk;
using Lmi3d.GoSdk.Messages;
using Lmi3d.Zen;
using Lmi3d.Zen.Io;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Deepnoid_Sensor_Gocator
{
	public class CDeviceSensorGocator : CDeviceSensorAbstract
	{
		/// <summary>
		/// 초기화 파라매터
		/// </summary>
		private CInitializeParameterGocator m_objInitializeParameter;
		/// <summary>
		/// 센서 연결 체크 스레드
		/// </summary>
		private bool m_bThreadExit;
		private Thread m_ThreadConnect;
		/// <summary>
		/// 센서 객체
		/// </summary>
		private GoSensor _objSensor;
		public GoSensor m_objSensor
		{
			get { return _objSensor; }
			set { _objSensor = value; }
		}
		/// <summary>
		/// 시스템 객체
		/// </summary>
		private static GoSystem _objSensorSystem;
		public static GoSystem m_objSensorSystem
		{
			get
			{
				if( null == _objSensorSystem ) {
					_objSensorSystem = new GoSystem();
				}
				return _objSensorSystem;
			}
		}
		/// <summary>
		/// 연결 여부
		/// </summary>
		bool m_bConnect = false;
		/// <summary>
		/// Gocator 와의 연결 상태를 지속적으로 체크
		/// </summary>
		bool unresponsive = false;
		/// <summary>
		/// Gocator 상태가 Connected 상태 체크 
		/// </summary>
		bool m_bGogatorStautConnected = false;
		/// <summary>
		/// Sensor Lock 체크 객체
		/// </summary>
		object m_objSensorLock;

		/// <summary>
		/// 생성자
		/// </summary>
		public CDeviceSensorGocator()
		{
			m_bThreadExit = false;
			_callBackScanData = null;
			_callBackMessage = null;
			_callBackExceptionMessage = null;
			m_objInitializeParameter = new CInitializeParameterGocator();
		}
		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objInitializeParameter"></param>
		/// <returns></returns>
		public override bool Initialize( CInitializeParameterGocator objInitializeParameter )
		{
			bool bReturn = false;

			do {
				try {
					// 초기화 파라매터 복사
					m_objInitializeParameter = ( CInitializeParameterGocator )objInitializeParameter.Clone();
					// 센서 인덱스
					// Lock 객체 생성
					m_objSensorLock = new object();
					// Gocator 라이브러리 연결
					KApiLib.Construct();
					GoSdkLib.Construct();
					// Gocator 센서 찾기
					KIpAddress ipAddress = KIpAddress.Parse( m_objInitializeParameter.strSensorIP );
					m_objSensor = m_objSensorSystem.FindSensorByIpAddress( ipAddress );
					// Gocator 시스템 갱신
					m_objSensorSystem.Refresh();
					// Gocator 연결
					m_objSensor.Connect();
					// Gocator 시스템 정지
					m_objSensorSystem.Stop();
					// Gocator 데이터 사용 및 콜백 연결.
					m_objSensor.EnableData( true );
					m_objSensor.SetDataHandler( ReceiveData );
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
		/// 콜백 데이터 
		/// </summary>
		/// <param name="data"></param>
		public void ReceiveData( KObject data )
		{
			CScanDataGocator objScanData = new CScanDataGocator();

			// 일단 해상도를 임의로 지정하자
			GoDataSet dataSet = ( GoDataSet )data;

			for( UInt32 i = 0; i < dataSet.Count; i++ ) {
				GoDataMsg dataObj = ( GoDataMsg )dataSet.Get( i );

				switch( dataObj.MessageType ) {
					case GoDataMessageType.Stamp: {
							GoStampMsg stampMsg = ( GoStampMsg )dataObj;
							for( UInt32 j = 0; j < stampMsg.Count; j++ ) {
								GoStamp stamp = stampMsg.Get( j );
								Console.WriteLine( "Frame Index = {0}", stamp.FrameIndex );
								Console.WriteLine( "Time Stamp = {0}", stamp.Timestamp );
								Console.WriteLine( "Encoder Value = {0}", stamp.Encoder );
							}
						}
						break;

					case GoDataMessageType.UniformSurface: {
							// 형식 또는 멤버는 사용되지 않습니다.
#pragma warning disable 0618
							GoSurfaceMsg surfaceMsg = ( GoSurfaceMsg )dataObj;
#pragma warning restore 0618

							objScanData.iOffsetZ = surfaceMsg.ZOffset;
							objScanData.iResolutionX = surfaceMsg.XResolution;// * DEF_3D_DATA_MULTIPLE;
							objScanData.iResolutionY = surfaceMsg.YResolution;
							objScanData.iResolutionZ = surfaceMsg.ZResolution;
							int lWidth = objScanData.iWidth = ( int )surfaceMsg.Width;
							int lHeight = objScanData.iHeight = ( int )surfaceMsg.Length;
							int bufferSize = lWidth * lHeight;
							IntPtr bufferPointer = surfaceMsg.Data;

							Console.WriteLine( "Whole Part Height Map received:" );
							Console.WriteLine( " Buffer width: {0}", lWidth );
							Console.WriteLine( " Buffer Height: {0}", lHeight );

							objScanData.objHeightDataOrigin = new short[ bufferSize ];
							objScanData.objHeightDataDoubleOrigin = new double[ bufferSize ];

							Marshal.Copy( bufferPointer, objScanData.objHeightDataOrigin, 0, objScanData.objHeightDataOrigin.Length );

							//칼리브레이션한 surface 데이터 저장
							double[] surfaceBuffer = new double[ bufferSize ];
							double Zmax = double.MinValue;
							double Zmin = double.MaxValue;
							Parallel.For( 0, lHeight, y => {
								Parallel.For( 0, lWidth, x => {
									short val = objScanData.objHeightDataOrigin[ y * lWidth + x ];
									//surface 데이터가 유효값
									if( val != short.MinValue ) {
										//칼리브레이션
										double Zval = val * ( surfaceMsg.ZResolution / 1000000.0 ) + ( surfaceMsg.ZOffset / 1000.0 );
										objScanData.objHeightDataDoubleOrigin[ y * lWidth + x ] = Zval;

										//정규화를 위해 최소,최대값 저장
										if( Zval > Zmax ) Zmax = Zval;
										if( Zval < Zmin ) Zmin = Zval;
									}
									//surface 데이터가 무효값
									else {
										objScanData.objHeightDataDoubleOrigin[ y * lWidth + x ] = double.MinValue;
									}
								} );
							} );
						}
						break;

					case GoDataMessageType.SurfaceIntensity: {
							GoSurfaceIntensityMsg surfaceMsg = ( GoSurfaceIntensityMsg )dataObj;
							long width = surfaceMsg.Width;
							long length = surfaceMsg.Length;
							long bufferSize = width * length;
							IntPtr bufferPointeri = surfaceMsg.Data;

							Console.WriteLine( "Whole Part Intensity Image received:" );
							Console.WriteLine( " Buffer width: {0}", width );
							Console.WriteLine( " Buffer length: {0}", length );
							objScanData.objIntensityDataOrigin = new byte[ bufferSize ];
							Marshal.Copy( bufferPointeri, objScanData.objIntensityDataOrigin, 0, objScanData.objIntensityDataOrigin.Length );
						}
						break;

					default:
						break;
				}
			}

			CScanData objData = new CScanData();
			objData.bGrabComplete = true;
			objData.objSensorDataGocator = objScanData;
			_callBackScanData?.Invoke( objData );
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

			// 카메라 접속 해제
			SetSensorDisconnect();
		}
		/// <summary>
		/// 연결 상태
		/// </summary>
		/// <returns></returns>
		public override bool IsConnected()
		{
			bool bReturn = false;

			do {
				bReturn = !unresponsive;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// Gocator Connected 상태 체크 
		/// </summary>
		/// <returns></returns>
		public bool IsGogatorStatusConnected()
		{
			bool bReturn = false;
			do {
				bReturn = m_bGogatorStautConnected;
			} while( false );
			return bReturn;
		}
		/// <summary>
		/// 노출값 설정.
		/// </summary>
		/// <param name="dExposureTime"></param>
		/// <returns></returns>
		public bool SetExposureTime( double dExposureTime )
		{
			bool bReturn = false;

			do {
				lock( m_objSensorLock ) {
					try {
						double min = m_objSensor.Setup.GetExposureLimitMin( GoRole.Main );
						double max = m_objSensor.Setup.GetExposureLimitMax( GoRole.Main );

						if( ( dExposureTime >= min ) && ( dExposureTime <= max ) ) {
							m_objSensor.Setup.SetExposure( GoRole.Main, dExposureTime );
						} else {
							break;
						}
					}
					catch( Exception ex ) {
						string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
						string strMethodName = MethodBase.GetCurrentMethod()?.Name;
						string strException = $"{strClassName} {strMethodName} : {ex.Message}";
						_callBackExceptionMessage?.Invoke( strException );
						break;
					}
				}

				bReturn = true;

			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 노출값 설정.
		/// </summary>
		/// <param name="dExposureTime"></param>
		/// <returns></returns>
		public double GetExposureTime()
		{
			double dExposureTime = 0;
			lock( m_objSensorLock ) {
				try {
					dExposureTime = m_objSensor.Setup.GetExposure( GoRole.Main );
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
				}
			}
			return dExposureTime;
		}


		/// <summary>
		/// 센서 연결 
		/// </summary>
		/// <returns></returns>
		private bool SetSensorConnect()
		{
			bool bReturn = false;

			do {
				try {
					KIpAddress ipAddress = KIpAddress.Parse( m_objInitializeParameter.strSensorIP );
					GoDataSet dataSet = new GoDataSet();
					m_objSensor = m_objSensorSystem.FindSensorByIpAddress( ipAddress );
					m_objSensorSystem.Refresh();
					m_objSensor.Connect();

					if( m_objSensor.State == GoState.Running )
						m_objSensor.Stop();

					m_objSensor.EnableData( true );

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
		/// 카메라 연결 해제
		/// </summary>
		private void SetSensorDisconnect()
		{
			try {
				if( null != m_objSensor ) {
					if( m_objSensor.State == GoState.Running )
						m_objSensor.Stop();

					m_objSensor.Disconnect();
					m_objSensor.Dispose();
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			}
		}
		/// <summary>
		/// 고게이터 얼라인
		/// </summary>
		/// <returns></returns>
		public bool Align()
		{
			bool bReturn = false;
			do {
				m_objSensor.Align();
				bReturn = true;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 고게이터 설정값 백업
		/// </summary>
		/// <param name="strDestPath"></param>
		/// <returns></returns>
		public bool Backup( string strDestPath )
		{
			bool bReturn = false;
			do {
				m_objSensor.Backup( strDestPath );
				bReturn = true;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 고게이터 스캔 취소
		/// </summary>
		/// <returns></returns>
		public bool Cancel()
		{
			bool bReturn = false;
			do {
				m_objSensor.Cancel();
				bReturn = true;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 고게이터 스캔 시작 ?
		/// </summary>
		/// <returns></returns>
		public bool CanStart()
		{
			bool bReturn = false;
			do {
				bReturn = m_objSensor.CanStart();
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 고게이터 스캔 시작
		/// </summary>
		/// <returns></returns>
		public override bool ScanStart()
		{
			bool bReturn = false;
			do {
				try {
					lock( m_objSensorLock ) {
						// 센서 연결 상태 확인.
						if( false == m_objSensor.IsConnected() ) {
							string strError = "Camera IsConnected False";
							_callBackMessage?.Invoke( strError );
							return false;
						}
						// 센서 상태 확인.
						if( unresponsive == true ) {
							string strError = "Camera Unresponsive";
							_callBackMessage?.Invoke( strError );

							return false;
						}
						int iTimeout = 3000;
						//m_objCameraSystem.Refresh();
						Action<object> objAction = ( object obj ) => {
							while( ( obj as GoSensor ).State != GoState.Ready ) {
								if( iTimeout < 0 ) {
									string strError = "Start - Gostate is not Ready";
									_callBackMessage?.Invoke( strError );
									break;
								} else {
									iTimeout -= 10;
								}
								Thread.Sleep( 10 );
							}
						};
						// 센서 Ready 상태 확인.
						Task objTask = Task.Factory.StartNew( objAction, m_objSensor );
						Task.WaitAll( objTask );
						// 센서 스캔 시작
						m_objSensor.Start();
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					// 예외 발생시 계속 예외발생됨... 다시 연결하자.
					ReConnect();
					// 어짜피 시작 함수이니 시작 상태로 변경해주자.
					m_objSensor.Start();
				}
				bReturn = true;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 고게이터 스캔 정지.
		/// </summary>
		/// <returns></returns>
		public override bool ScanStop()
		{
			bool bReturn = false;
			do {
				try {
					lock( m_objSensorLock ) {
						// 연결 상태 확인.
						if( false == m_objSensor.IsConnected() ) {
							string strError = "Camera IsConnected False";
							_callBackMessage?.Invoke( strError );
							break;
						}
						m_objSensor.Stop();
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 고게이터 센서 리셋 ( 20 ~ 30 초 정도 꺼졌다 켜짐 )
		/// </summary>
		/// <param name="bWait"></param>
		/// <returns></returns>
		public override bool SensorReset( bool bWait )
		{
			bool bReturn = false;
			do {
				try {
					m_objSensor.Reset( bWait );
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}
				bReturn = true;
			} while( false );
			return bReturn;
		}

		/// <summary>
		/// 고게이터 엔코더 리셋
		/// </summary>
		/// <returns></returns>
		public bool ResetEncoder()
		{
			bool bReturn = false;

			do {
				try {
					// 연결 상태 확인.
					if( false == m_objSensor.IsConnected() ) {
						string strError = "Camera IsConnected False";
						_callBackMessage?.Invoke( strError );
						break;
					}
					m_objSensor.ResetEncoder();
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}
				bReturn = true;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 고게이터 엔코더 값 가져오기
		/// </summary>
		/// <returns></returns>
		public long GetEncoder()
		{
			long nEncoder = 0;

			try {
				if( m_objSensor.IsConnected() == true )
					nEncoder = m_objSensor.Encoder();
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			}

			return nEncoder;
		}
		/// <summary>
		/// 고게이터 스케쥴 스캔 시작
		/// </summary>
		/// <param name="lValue"></param>
		/// <returns></returns>
		public bool ScheduledStart( long lValue )
		{
			bool bReturn = false;
			do {
				try {
					// 연결 상태 확인.
					if( false == m_objSensor.IsConnected() ) {
						string strError = "Camera IsConnected False";
						_callBackMessage?.Invoke( strError );
						break;
					}
					m_objSensor.ScheduledStart( lValue );
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}
				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 고게이터 스냅샷
		/// </summary>
		/// <returns></returns>
		public bool Snapshot()
		{
			bool bReturn = false;
			do {
				try {
					// 연결 상태 확인.
					if( false == m_objSensor.IsConnected() ) {
						string strError = "Camera IsConnected False";
						_callBackMessage?.Invoke( strError );
						break;
					}
					m_objSensor.Snapshot();
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}
				bReturn = true;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 고게이터 비트맵 저장.
		/// </summary>
		/// <param name="strDestPath"></param>
		/// <returns></returns>
		public bool SaveBitmap( string strDestPath )
		{
			bool bReturn = false;
			do {
				try {
					// 연결 상태 확인.
					if( false == m_objSensor.IsConnected() ) {
						string strError = "Camera IsConnected False";
						_callBackMessage?.Invoke( strError );
						break;
					}
					m_objSensor.ExportBitmap( GoReplayExportSourceType.Intensity, GoDataSource.Top, strDestPath );
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}
				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 고게이터 데이터 CSV 파일로 저장
		/// </summary>
		/// <param name="strDestPath"></param>
		/// <returns></returns>
		public bool ExportCsv( string strDestPath )
		{
			bool bReturn = false;
			do {
				try {
					// 연결 상태 확인.
					if( false == m_objSensor.IsConnected() ) {
						string strError = "Camera IsConnected False";
						_callBackMessage?.Invoke( strError );
						break;
					}
					m_objSensor.ExportCsv( strDestPath );
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}
				bReturn = true;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 고게이터 트리거 
		/// </summary>
		/// <returns></returns>
		public bool Trigger()
		{
			bool bReturn = false;
			do {
				try {
					// 연결 상태 확인.
					if( false == m_objSensor.IsConnected() ) {
						string strError = "Camera IsConnected False";
						_callBackMessage?.Invoke( strError );
						break;
					}
					m_objSensor.Trigger();
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}
				bReturn = true;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 고게이터 센서 짝 대기 
		/// 센서 2개 일 경우 모두 끝날때까지 대기
		/// </summary>
		/// <param name="uTimeOut"></param>
		/// <returns></returns>
		public bool WaitForBuddies( ulong uTimeOut )
		{
			bool bReturn = false;
			do {
				try {
					// 연결 상태 확인.
					if( false == m_objSensor.IsConnected() ) {
						string strError = "Camera IsConnected False";
						_callBackMessage?.Invoke( strError );
						break;
					}
					m_objSensor.WaitForBuddies( uTimeOut );
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}
				bReturn = true;
			} while( false );

			return bReturn;
		}
		/// <summary>
		/// 고게이터 스캔 거리 설정
		/// </summary>
		/// <param name="iScanLength"></param>
		/// <returns></returns>
		public bool SetScanLength( double iScanLength )
		{
			bool bReturn = false;

			do {
				lock( m_objSensorLock ) {
					try {
						// 연결 상태 확인.
						if( false == m_objSensor.IsConnected() ) {
							string strError = "Camera IsConnected False";
							_callBackMessage?.Invoke( strError );
							break;
						}
						m_objSensor.Setup.ScanMode = GoMode.Surface;
						m_objSensor.Setup.GetSurfaceGeneration().GenerationType = GoSurfaceGenerationType.FixedLength;
						m_objSensor.Setup.GetSurfaceGeneration().FixedLengthLength = iScanLength;
						m_objSensor.Setup.GetSurfaceGeneration().FixedLengthStartTrigger = GoSurfaceGenerationStartTrigger.Sequential;
					}
					catch( Exception ex ) {
						string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
						string strMethodName = MethodBase.GetCurrentMethod()?.Name;
						string strException = $"{strClassName} {strMethodName} : {ex.Message}";
						_callBackExceptionMessage?.Invoke( strException );
						break;
					}
				}
				bReturn = true;

			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 고게이터 트리거 엔코더 간격
		/// </summary>
		/// <param name="dTriggerEncoderSpacing"></param>
		/// <returns></returns>
		public bool SetTriggerEncoderSpacing( double dTriggerEncoderSpacing )
		{
			bool bReturn = false;

			do {
				lock( m_objSensorLock ) {
					try {
						// 연결 상태 확인.
						if( false == m_objSensor.IsConnected() ) {
							string strError = "Camera IsConnected False";
							_callBackMessage?.Invoke( strError );
							break;
						}
						double min = m_objSensor.Setup.EncoderSpacingLimitMin;
						double max = m_objSensor.Setup.EncoderSpacingLimitMax;
						if( ( dTriggerEncoderSpacing >= min ) && ( dTriggerEncoderSpacing <= max ) ) {
							m_objSensor.Setup.EncoderSpacing = dTriggerEncoderSpacing;
						} else {
							break;
						}
					}
					catch( Exception ex ) {
						string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
						string strMethodName = MethodBase.GetCurrentMethod()?.Name;
						string strException = $"{strClassName} {strMethodName} : {ex.Message}";
						_callBackExceptionMessage?.Invoke( strException );
						break;
					}
				}

				bReturn = true;

			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 고게이터 스캔 폭 설정.
		/// </summary>
		/// <param name="dWidth"></param>
		/// <returns></returns>
		public bool SetScanWidth( double dWidth )
		{
			bool bReturn = false;
			do {
				try {
					// 연결 상태 확인.
					if( false == m_objSensor.IsConnected() ) {
						string strError = "Camera IsConnected False";
						_callBackMessage?.Invoke( strError );
						break;
					}
					// 센서 넓이를 최대로 설정한다
					m_objSensor.Setup.SetActiveAreaWidth( GoRole.Main, dWidth );
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}

				bReturn = true;

			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 고게이터 스캔 X 설정
		/// </summary>
		/// <param name="dX"></param>
		/// <returns></returns>
		public bool SetScanX( double dX )
		{
			bool bReturn = false;
			do {
				try {
					// 연결 상태 확인.
					if( false == m_objSensor.IsConnected() ) {
						string strError = "Camera IsConnected False";
						_callBackMessage?.Invoke( strError );
						break;
					}
					m_objSensor.Setup.SetActiveAreaX( GoRole.Main, dX );
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}

				bReturn = true;

			} while( false );
			return bReturn;
		}
		/// <summary>
		/// 고게이터 스캔 최대 폭 설정.
		/// </summary>
		/// <returns></returns>
		public bool SetScanMaxWidth()
		{
			bool bReturn = false;
			do {
				try {
					// 연결 상태 확인.
					if( false == m_objSensor.IsConnected() ) {
						string strError = "Camera IsConnected False";
						_callBackMessage?.Invoke( strError );
						break;
					}

					// 최대영역을 구하기 위해 센서 넓이와 크기를 0으로설정하고
					m_objSensor.Setup.SetActiveAreaWidth( GoRole.Main, 0 );
					m_objSensor.Setup.SetActiveAreaX( GoRole.Main, 0 );

					// 최대 센서의 넓이를 가져온다
					double dWidth = m_objSensor.Setup.GetActiveAreaWidthLimitMax( GoRole.Main );

					// 센서 시작위치를 제일 왼쪽으로 보내고 
					m_objSensor.Setup.SetActiveAreaX( GoRole.Main, ( dWidth / 2 ) * -1 );

					// 센서 넓이를 최대로 설정한다
					m_objSensor.Setup.SetActiveAreaWidth( GoRole.Main, dWidth );
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
					break;
				}

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
			CDeviceSensorGocator pThis = ( CDeviceSensorGocator )state;
			while( false == pThis.m_bThreadExit ) {
				pThis.GocatorStateCheck();
				Thread.Sleep( 500 );
			}
		}

		private void GocatorStateCheck()
		{
			if( m_objSensor == null )
				return;

			try {
				Lmi3d.GoSdk.GoState Gostate;
				lock( m_objSensorLock ) {
					Gostate = m_objSensor.State;
				}
				switch( Gostate ) {
					case Lmi3d.GoSdk.GoState.Online:
						// 센서가 감지 되었지만, 연결되진 않음
						{
							_callBackMessage?.Invoke( "GoState.Online" );
							// 센서가 끊어졌다가 다시 붙은 최초 시점
							// 이때 센서 연결수행
							//Thread.Sleep(1000);
							Connect();
							//unresponsive = true;
						}
						break;

					case Lmi3d.GoSdk.GoState.Offline:
						// 센서가 감지되지 않고 연결도 안됨
						_callBackMessage?.Invoke( "GoState.Offline" );

						// 만약 연결되어있던 중 강제로 전원이 나가면, 기존 버튼 초기화
						if( m_bConnect == true ) {
							m_bConnect = false;
							//  m_bScan = false;
						}
						break;

					case Lmi3d.GoSdk.GoState.Resetting:
						// 센서 연결이 끊어지고 재설정중
						_callBackMessage?.Invoke( "GoState.Resetting" );
						break;

					case Lmi3d.GoSdk.GoState.Incompatible:
						// 센서가 연결되었지만 클라이언트와 프로토콜이 호환되지 않음(펌웨어 업그레이드 필요)
						_callBackMessage?.Invoke( "GoState.Incompatible" );
						break;

					case Lmi3d.GoSdk.GoState.Inconsistent:
						// 센서가 연결되었지만 원격 상태가 변경됨
						_callBackMessage?.Invoke( "GoState.Inconsistent" );
						break;

					case Lmi3d.GoSdk.GoState.Unresponsive:
						// 센서가 연결되었지만, 더이상 감지가 되지 않고 있음
						if( false == unresponsive )
							_callBackMessage?.Invoke( "GoState.Unresponsive" );
						unresponsive = true;
						break;

					case Lmi3d.GoSdk.GoState.Cancelled:
						_callBackMessage?.Invoke( "GoState.Cancelled" );
						// 센서가 연결되었지만 GoSensor_Cancel 기능으로 인해 통신이 중단됨
						break;

					case Lmi3d.GoSdk.GoState.Incomplete:
						_callBackMessage?.Invoke( "GoState.Incomplete" );
						// 센서가 연결되었지만 필요한 버디 센서가 없음(버디 연결 대기 또는 제거 해야 함)
						break;

					case Lmi3d.GoSdk.GoState.Busy:
						_callBackMessage?.Invoke( "GoState.Busy" );
						// 센서가 연결되었지만, 다른 센서가 현재 제어중(버디로 연결되었을 경우 해당)
						break;

					case Lmi3d.GoSdk.GoState.Ready:
						// 센서가 연결됨
						// 센서가 연결되어 현재 실행중 
						if( unresponsive == true ) {
							// 센서가 끊어졌다가 다시 붙은 최초 시점
							// 이때 센서 연결수행
							_callBackMessage?.Invoke( "GoState.Ready-unresponsive true" );

							//Thread.Sleep( 1000 );
							SetSensorDisconnect();
							ReConnect();
							unresponsive = false;
						}
						break;

					case Lmi3d.GoSdk.GoState.Running:
						// 센서가 연결되어 현재 실행중 
						if( unresponsive == true ) {
							_callBackMessage?.Invoke( "GoState.Running-unresponsive true" );
							// 센서가 끊어졌다가 다시 붙은 최초 시점
							// 이때 센서 연결수행
							//Thread.Sleep( 1000 );
							SetSensorDisconnect();
							ReConnect();
							unresponsive = false;
						}
						break;

					case Lmi3d.GoSdk.GoState.Upgrading:
						// 현재 센서가 업그레이드 중
						break;

					case Lmi3d.GoSdk.GoState.Connected:
						_callBackMessage?.Invoke( "GoState.Connected" );
						m_bGogatorStautConnected = true;
						SetSensorDisconnect();
						ReConnect();
						break;
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			}
		}

		public void Connect()
		{
			try {
				// Gocator 초기화 및 접속 (GoSDK 예제를 참고)
				KIpAddress ipAddress;
				GoSensor objCamera;

				// GoSystem 재 생성.
				_objSensorSystem = new GoSystem();
				ipAddress = KIpAddress.Parse( m_objInitializeParameter.strSensorIP );

				// 센서 정보 가져오기.
				objCamera = m_objSensorSystem.FindSensorByIpAddress( ipAddress );

				// 시스템 리프레쉬
				m_objSensorSystem.Refresh();

				// 센서 연결.
				m_objSensor = objCamera;
				m_objSensor.Connect();

				// 센서 정지.
				m_objSensor.Stop();

				// 데이터 관련 
				m_objSensor.EnableData( true );
				m_objSensor.SetDataHandler( ReceiveData );
				m_bConnect = true;
				m_bGogatorStautConnected = false;
				Thread.Sleep( 300 );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			}
		}

		public void ReConnect()
		{
			try {
				GoSensor objCamera;
				// Gocator 초기화 및 접속 (GoSDK 예제를 참고)
				KIpAddress ipAddress;
				// GoSystem 재 생성.
				_objSensorSystem = new GoSystem();
				ipAddress = KIpAddress.Parse( m_objInitializeParameter.strSensorIP );
				// 센서 정보 가져오기.
				objCamera = m_objSensorSystem.FindSensorByIpAddress( ipAddress );
				// 시스템 리프레쉬
				m_objSensorSystem.Refresh();
				// 센서 연결.
				m_objSensor = objCamera;
				m_objSensor.Connect();
				// 센서 정지.
				m_objSensor.Stop();
				// 데이터 관련 
				m_objSensor.EnableData( true );
				m_objSensor.SetDataHandler( ReceiveData );
				m_bConnect = true;
				m_bGogatorStautConnected = false;
				Thread.Sleep( 300 );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackExceptionMessage?.Invoke( strException );
			}
		}
	}
}