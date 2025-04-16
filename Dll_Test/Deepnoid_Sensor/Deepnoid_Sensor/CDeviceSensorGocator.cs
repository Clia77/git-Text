using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Threading;
using System.Diagnostics;

using Lmi3d;
using Lmi3d.Zen;
using Lmi3d.GoSdk;
using Lmi3d.Zen.Io;
using Lmi3d.GoSdk.Messages;
using System.Runtime.InteropServices;
using System.IO;
using Lmi3d.GoSdk.Tools;
using Lmi3d.GoSdk.Outputs;

namespace Deepnoid_Sensor {
    public class CDeviceSensorGocator : CDeviceSensorAbstract {
        /// <summary>
        /// 카메라 인덱스
        /// </summary>
        private int m_iSensorIndex;
        /// <summary>
        /// 초기화 파라매터
        /// </summary>
        private CInitializeParameter m_objInitializeParameter;
        /// <summary>
        /// 센서 연결 체크 스레드
        /// </summary>
        private bool m_bThreadExit;
        private Thread m_ThreadConnect;
        /// <summary>
        /// 그랩 콜백
        /// </summary>
        private CallBackScanData m_objCallbackScanData;
        /// <summary>
        /// 메시지 콜백
        /// </summary>
        private CallBackMessage m_objCallbackMessage;
        /// <summary>
        /// 예외 메시지 콜백
        /// </summary>
        private CallBackExceptionMessage m_objCallbackExceptionMessage;
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
            get {
                if ( null == _objSensorSystem ) {
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
            m_objCallbackScanData = null;
            m_objCallbackMessage = null;
            m_objCallbackExceptionMessage = null;
            m_objInitializeParameter = new CInitializeParameter();
        }
        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="objInitializeParameter"></param>
        /// <returns></returns>
        public override bool Initialize( CInitializeParameter objInitializeParameter )
        {
            bool bReturn = false;

            do {
                try {
                    // 초기화 파라매터 복사
                    m_objInitializeParameter = ( CInitializeParameter )objInitializeParameter.Clone();
                    // 센서 인덱스
                    m_iSensorIndex = m_objInitializeParameter.iSensorIndex;
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
                catch {
                }
            } while ( false );

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

            for ( UInt32 i = 0; i < dataSet.Count; i++ ) {
                GoDataMsg dataObj = ( GoDataMsg )dataSet.Get( i );

                switch ( dataObj.MessageType ) {
                    case GoDataMessageType.Stamp: {
                            GoStampMsg stampMsg = ( GoStampMsg )dataObj;
                            for ( UInt32 j = 0; j < stampMsg.Count; j++ ) {
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
                                    if ( val != short.MinValue ) {
                                        //칼리브레이션
                                        double Zval = val * ( surfaceMsg.ZResolution / 1000000.0 ) + ( surfaceMsg.ZOffset / 1000.0 );
                                        objScanData.objHeightDataDoubleOrigin[ y * lWidth + x ] = Zval;

                                        //정규화를 위해 최소,최대값 저장
                                        if ( Zval > Zmax ) Zmax = Zval;
                                        if ( Zval < Zmin ) Zmin = Zval;
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

            if ( null != m_objCallbackScanData ) {
                CScanData objData = new CScanData();
                objData.bGrabComplete = true;
                objData.objSensorDataGocator = objScanData;
                m_objCallbackScanData( objData );
            }
        }
        /// <summary>
        /// 해제
        /// </summary>
        public override void DeInitialize()
        {
            // 접속 스레드 종료
            m_bThreadExit = true;

            if ( m_ThreadConnect != null )
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
            } while ( false );

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
            } while ( false );
            return bReturn;
        }
        /// <summary>
        /// 데이터 콜백 연결
        /// </summary>
        /// <param name="objCallback"></param>
        public override void SetCallbackScanData( CallBackScanData objCallback )
        {
            m_objCallbackScanData = objCallback;
        }
        /// <summary>
        /// 메시지 콜백 연결
        /// </summary>
        /// <param name="objCallback"></param>
        public override void SetCallbackMessage( CallBackMessage objCallback )
        {
            m_objCallbackMessage = objCallback;
        }
        /// <summary>
        /// 메시지 콜백 연결
        /// </summary>
        /// <param name="objCallback"></param>
        public override void SetCallbackExceptionMessage( CallBackExceptionMessage objCallback )
        {
            m_objCallbackExceptionMessage = objCallback;
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
                lock ( m_objSensorLock ) {
                    try {
                        double min = m_objSensor.Setup.GetExposureLimitMin( GoRole.Main );
                        double max = m_objSensor.Setup.GetExposureLimitMax( GoRole.Main );

                        if ( ( dExposureTime >= min ) && ( dExposureTime <= max ) ) {
                            m_objSensor.Setup.SetExposure( GoRole.Main, dExposureTime );
                        }
                        else {
                            break;
                        }
                    }
                    catch ( Exception ex ) {
                        m_objCallbackExceptionMessage( "SetExposureTime - " + ex.Message );
                        break;
                    }
                }

                bReturn = true;

            } while ( false );

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
            lock ( m_objSensorLock ) {
                try {
                    dExposureTime = m_objSensor.Setup.GetExposure( GoRole.Main );
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "GetExposureTime - " + ex.Message );
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

                    if ( m_objSensor.State == GoState.Running )
                        m_objSensor.Stop();

                    m_objSensor.EnableData( true );

                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "SetSensorConnect - " + ex.Message );
                }
            } while ( false );

            return bReturn;
        }

        /// <summary>
        /// 카메라 연결 해제
        /// </summary>
        private void SetSensorDisconnect()
        {
            try {
                if( null != m_objSensor ) {
                    if ( m_objSensor.State == GoState.Running )
                        m_objSensor.Stop();

                    m_objSensor.Disconnect();
                    m_objSensor.Dispose();
                }                
            }
            catch ( Exception ex ) {
                m_objCallbackExceptionMessage( "SetSensorDisconnect - " + ex.Message );
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
            } while ( false );

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
            } while ( false );

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
            } while ( false );

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
            } while ( false );

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
                    lock ( m_objSensorLock ) {
                        // 센서 연결 상태 확인.
                        if ( false == m_objSensor.IsConnected() ) {
                            m_objCallbackMessage( "Camera IsConnected False" );
                            return false;
                        }
                        // 센서 상태 확인.
                        if ( unresponsive == true ) {
                            m_objCallbackMessage( "Camera Unresponsive" );

                            return false;
                        }
                        int iTimeout = 3000;
                        //m_objCameraSystem.Refresh();
                        Action<object> objAction = ( object obj ) => {
                            while ( ( obj as GoSensor ).State != GoState.Ready ) {
                                if ( iTimeout < 0 ) {
                                    m_objCallbackMessage( "Start - Gostate is not Ready" );
                                    break;
                                }
                                else {
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
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "Start - " + ex.Message );
                    // 예외 발생시 계속 예외발생됨... 다시 연결하자.
                    ReConnect();
                    // 어짜피 시작 함수이니 시작 상태로 변경해주자.
                    m_objSensor.Start();
                }
                bReturn = true;
            } while ( false );

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
                    lock ( m_objSensorLock ) {
                        // 연결 상태 확인.
                        if ( false == m_objSensor.IsConnected() ) {
                            m_objCallbackMessage( "Camera IsConnected False" );
                            break;
                        }
                        m_objSensor.Stop();
                    }
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "Stop - " + ex.Message );
                    break;
                }

                bReturn = true;
            } while ( false );

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
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "Reset - " + ex.Message );
                    break;
                }
                bReturn = true;
            } while ( false );
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
                    if ( false == m_objSensor.IsConnected() ) {
                        m_objCallbackMessage( "Camera IsConnected False" );
                        break;
                    }
                    m_objSensor.ResetEncoder();
                }
                catch ( Exception ex ) {
                    //Trace.WriteLine(ex.ToString());
                    m_objCallbackExceptionMessage( "ResetEncoder - " + ex.Message );
                    break;
                }
                bReturn = true;
            } while ( false );

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
                if ( m_objSensor.IsConnected() == true )
                    nEncoder = m_objSensor.Encoder();
            }
            catch ( Exception ex ) {
                m_objCallbackExceptionMessage( "GetEncoder - " + ex.Message );
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
                    if ( false == m_objSensor.IsConnected() ) {
                        m_objCallbackMessage( "Camera IsConnected False" );
                        break;
                    }
                    m_objSensor.ScheduledStart( lValue );
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "ScheduledStart - " + ex.Message );
                    break;
                }                
                bReturn = true;
            } while ( false );

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
                    if ( false == m_objSensor.IsConnected() ) {
                        m_objCallbackMessage( "Camera IsConnected False" );
                        break;
                    }
                    m_objSensor.Snapshot();
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "Snapshot - " + ex.Message );
                    break;
                }
                bReturn = true;
            } while ( false );

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
                    if ( false == m_objSensor.IsConnected() ) {
                        m_objCallbackMessage( "Camera IsConnected False" );
                        break;
                    }
                    m_objSensor.ExportBitmap( GoReplayExportSourceType.Intensity, GoDataSource.Top, strDestPath );
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "SaveBitmap - " + ex.Message );
                    break;
                }                
                bReturn = true;
            } while ( false );

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
                    if ( false == m_objSensor.IsConnected() ) {
                        m_objCallbackMessage( "Camera IsConnected False" );
                        break;
                    }
                    m_objSensor.ExportCsv( strDestPath );
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "ExportCsv - " + ex.Message );
                    break;
                }
                bReturn = true;
            } while ( false );

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
                    if ( false == m_objSensor.IsConnected() ) {
                        m_objCallbackMessage( "Camera IsConnected False" );
                        break;
                    }
                    m_objSensor.Trigger();
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "Trigger - " + ex.Message );
                    break;
                }
                bReturn = true;
            } while ( false );

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
                    if ( false == m_objSensor.IsConnected() ) {
                        m_objCallbackMessage( "Camera IsConnected False" );
                        break;
                    }
                    m_objSensor.WaitForBuddies( uTimeOut );
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "WaitForBuddies - " + ex.Message );
                    break;
                }
                bReturn = true;
            } while ( false );

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
                lock ( m_objSensorLock ) {
                    try {
                        // 연결 상태 확인.
                        if ( false == m_objSensor.IsConnected() ) {
                            m_objCallbackMessage( "Camera IsConnected False" );
                            break;
                        }
                        m_objSensor.Setup.ScanMode = GoMode.Surface;
                        m_objSensor.Setup.GetSurfaceGeneration().GenerationType = GoSurfaceGenerationType.FixedLength;
                        m_objSensor.Setup.GetSurfaceGeneration().FixedLengthLength = iScanLength;
                        m_objSensor.Setup.GetSurfaceGeneration().FixedLengthStartTrigger = GoSurfaceGenerationStartTrigger.Sequential;
                    }
                    catch ( Exception ex ) {
                        m_objCallbackExceptionMessage( "SetScanLength - " + ex.Message );
                        break;
                    }
                }
                bReturn = true;

            } while ( false );

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
                lock ( m_objSensorLock ) {
                    try {
                        // 연결 상태 확인.
                        if ( false == m_objSensor.IsConnected() ) {
                            m_objCallbackMessage( "Camera IsConnected False" );
                            break;
                        }
                        double min = m_objSensor.Setup.EncoderSpacingLimitMin;
                        double max = m_objSensor.Setup.EncoderSpacingLimitMax;
                        if ( ( dTriggerEncoderSpacing >= min ) && ( dTriggerEncoderSpacing <= max ) ) {
                            m_objSensor.Setup.EncoderSpacing = dTriggerEncoderSpacing;
                        }
                        else {
                            break;
                        }
                    }
                    catch ( Exception ex ) {
                        m_objCallbackExceptionMessage( "HLSetTriggerEncoderSpacing - " + ex.Message );
                        break;
                    }
                }

                bReturn = true;

            } while ( false );

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
                    if ( false == m_objSensor.IsConnected() ) {
                        m_objCallbackMessage( "Camera IsConnected False" );
                        break;
                    }
                    // 센서 넓이를 최대로 설정한다
                    m_objSensor.Setup.SetActiveAreaWidth( GoRole.Main, dWidth );
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "SetScanWidth - " + ex.Message );
                    break;
                }

                bReturn = true;

            } while ( false );

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
                    if ( false == m_objSensor.IsConnected() ) {
                        m_objCallbackMessage( "Camera IsConnected False" );
                        break;
                    }
                    m_objSensor.Setup.SetActiveAreaX( GoRole.Main, dX );
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "SetScanX - " + ex.Message );
                    break;
                }

                bReturn = true;

            } while ( false );
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
                    if ( false == m_objSensor.IsConnected() ) {
                        m_objCallbackMessage( "Camera IsConnected False" );
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
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "SetScanMaxWidth - " + ex.Message );
                    break;
                }

                bReturn = true;

            } while ( false );

            return bReturn;
        }
        /// <summary>
        /// 고게이터 접속 상태 체크 쓰레드
        /// </summary>
        /// <param name="state"></param>
        private static void ThreadConnect( Object state )
        {
            CDeviceSensorGocator pThis = ( CDeviceSensorGocator )state;
            while ( false == pThis.m_bThreadExit ) {
                pThis.GocatorStateCheck();
                Thread.Sleep( 500 );
            }
        }

        private void GocatorStateCheck()
        {
            if ( m_objSensor == null )
                return;

            try {
                Lmi3d.GoSdk.GoState Gostate;
                lock ( m_objSensorLock ) {
                    Gostate = m_objSensor.State;
                }
                switch ( Gostate ) {
                    case Lmi3d.GoSdk.GoState.Online:
                        // 센서가 감지 되었지만, 연결되진 않음
                        {
                            m_objCallbackMessage( "GoState.Online" );
                            // 센서가 끊어졌다가 다시 붙은 최초 시점
                            // 이때 센서 연결수행
                            //Thread.Sleep(1000);
                            Connect();
                            //unresponsive = true;
                        }
                        break;

                    case Lmi3d.GoSdk.GoState.Offline:
                        // 센서가 감지되지 않고 연결도 안됨
                        m_objCallbackMessage( "GoState.Offline" );

                        // 만약 연결되어있던 중 강제로 전원이 나가면, 기존 버튼 초기화
                        if ( m_bConnect == true ) {
                            m_bConnect = false;
                            //  m_bScan = false;
                        }
                        break;

                    case Lmi3d.GoSdk.GoState.Resetting:
                        // 센서 연결이 끊어지고 재설정중
                        m_objCallbackMessage( "GoState.Resetting" );
                        break;

                    case Lmi3d.GoSdk.GoState.Incompatible:
                        // 센서가 연결되었지만 클라이언트와 프로토콜이 호환되지 않음(펌웨어 업그레이드 필요)
                        m_objCallbackMessage( "GoState.Incompatible" );
                        break;

                    case Lmi3d.GoSdk.GoState.Inconsistent:
                        // 센서가 연결되었지만 원격 상태가 변경됨
                        m_objCallbackMessage( "GoState.Inconsistent" );
                        break;

                    case Lmi3d.GoSdk.GoState.Unresponsive:
                        // 센서가 연결되었지만, 더이상 감지가 되지 않고 있음
                        if ( false == unresponsive )
                            m_objCallbackMessage( "GoState.Unresponsive" );
                        unresponsive = true;
                        break;

                    case Lmi3d.GoSdk.GoState.Cancelled:
                        m_objCallbackMessage( "GoState.Cancelled" );
                        // 센서가 연결되었지만 GoSensor_Cancel 기능으로 인해 통신이 중단됨
                        break;

                    case Lmi3d.GoSdk.GoState.Incomplete:
                        m_objCallbackMessage( "GoState.Incomplete" );
                        // 센서가 연결되었지만 필요한 버디 센서가 없음(버디 연결 대기 또는 제거 해야 함)
                        break;

                    case Lmi3d.GoSdk.GoState.Busy:
                        m_objCallbackMessage( "GoState.Busy" );
                        // 센서가 연결되었지만, 다른 센서가 현재 제어중(버디로 연결되었을 경우 해당)
                        break;

                    case Lmi3d.GoSdk.GoState.Ready:
                        // 센서가 연결됨
                        // 센서가 연결되어 현재 실행중 
                        if ( unresponsive == true ) {
                            // 센서가 끊어졌다가 다시 붙은 최초 시점
                            // 이때 센서 연결수행
                            m_objCallbackMessage( "GoState.Ready-unresponsive true" );

                            //Thread.Sleep( 1000 );
                            SetSensorDisconnect();
                            ReConnect();
                            unresponsive = false;
                        }
                        break;

                    case Lmi3d.GoSdk.GoState.Running:
                        // 센서가 연결되어 현재 실행중 
                        if ( unresponsive == true ) {
                            m_objCallbackMessage( "GoState.Running-unresponsive true" );
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
                        m_objCallbackMessage( "GoState.Connected" );
                        m_bGogatorStautConnected = true;
                        SetSensorDisconnect();
                        ReConnect();
                        break;
                }
            }
            catch ( Exception ex ) {
                m_objCallbackExceptionMessage( "GocatorStateCheck - " + ex.Message );
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
                m_objCallbackMessage( "Connect()" );
                Thread.Sleep( 300 );
            }
            catch ( Exception ex ) {
                m_objCallbackExceptionMessage( "Connect - " + ex.Message );
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
                m_objCallbackMessage( "ReConnect()" );
                Thread.Sleep( 300 );
            }
            catch ( Exception ex ) {
                m_objCallbackExceptionMessage( "ReConnect - " + ex.Message );
            }
        }
    }
}