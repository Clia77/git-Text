using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Buffers;

namespace Deepnoid_Sensor {
    public class CDeviceSensorKeyenceCL3000 : CDeviceSensorAbstract {
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
        /// 연결 여부
        /// </summary>
        private bool m_bConnect = false;
        /// <summary>
        /// Sensor Lock 체크 객체
        /// </summary>
        private object m_objSensorLock;
        /// <summary>
        /// 최대 데이터 갯수
        /// </summary>
        private const int m_iMaxCount = 1000000;
        /// <summary>
        /// 응답 데이터 길이
        /// </summary>
        private const int m_iRequestDataLength = 512000;
        /// <summary>
        /// 최대 측정 갯수
        /// </summary>
        private const int m_iMaxMeasureCount = 60000;
        int m_iRetryCount = 0;
        uint m_iIndexNext = 0;
        uint m_iIndexStart = 0;
        CancellationTokenSource cts;
        /// <summary>
        /// 생성자
        /// </summary>
        public CDeviceSensorKeyenceCL3000()
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
                    // 연결 체크 스레드 시작
                    m_ThreadConnect = new Thread( ThreadConnect );
                    m_ThreadConnect.Start( this );
                    bReturn = true;
                }
                catch { }

            } while ( false );

            return bReturn;
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
        /// 스캔 시작
        /// </summary>
        /// <returns></returns>
        public override bool ScanStart()
        {
            bool bReturn = false;
            do {
                // 스캔 시작 명령은 따로 없고 PulseCount 초기화 및 TrendIndex 값 가져오기.
                if ( false == InitializeSensorAndGetStartIndex() ) {
                    break;
                }
                bReturn = true;
            } while ( false );
            
            return bReturn;
        }

        /// <summary>
        /// 센서 초기화 메서드...
        /// </summary>
        /// <param name="iIndexStart"></param>
        /// <returns></returns>
        private bool InitializeSensorAndGetStartIndex()
        {
            bool bReturn = false;
            do {
                try {
                    int iReturnCode;
                    iReturnCode = NativeMethods.CL3IF_ResetPulseCount( m_iSensorIndex );
                    m_objCallbackMessage( string.Format( "Sensor {0} Reset Pulse Return is {1}", m_iSensorIndex, iReturnCode ) );
                    iReturnCode = NativeMethods.CL3IF_GetTrendIndex( m_iSensorIndex, out m_iIndexStart );
                    m_objCallbackMessage( string.Format( "Sensor {0} Get Index Return is {1}", m_iSensorIndex, iReturnCode ) );
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( string.Format( "Sensor {0} InitializeSensorAndGetStartIndex - ", m_iSensorIndex, ex.Message ) );
                }                            
            } while ( false );
            return bReturn;
        }
        /// <summary>
        /// 데이터 가져옴.... MoveComplete 신호 오면 호출.
        /// </summary>
        /// <returns></returns>
        public bool GeTrendData()
        {
            byte[] abyBuffer = null;
            try {
                abyBuffer = ArrayPool<byte>.Shared.Rent( m_iRequestDataLength );
                using ( var pin = new PinnedObject( abyBuffer ) ) {
                    CL3IF_OUTNO eOutTarget = 0;
                    uint iGetDataCount = 0;

                    int iReturnCode = NativeMethods.CL3IF_GetTrendData( m_iSensorIndex, m_iIndexStart, m_iMaxMeasureCount,
                        out m_iIndexNext, out iGetDataCount, out eOutTarget, pin.Pointer );
                    if ( iReturnCode != NativeMethods.CL3IF_RC_OK ) {
                        return false; // 오류 발생 시 즉시 반환
                    }
                    m_iIndexStart = m_iIndexNext;
                    List<int> ltnTargetOut = ConvertOutTargetList( eOutTarget );
                    List<CKeyenceCL3000SingleData> objListSingleData = new List<CKeyenceCL3000SingleData>();
                    int iReadPosition = 0;

                    for ( int iLoopCount = 0; iLoopCount < iGetDataCount; iLoopCount++ ) {
                        CL3IF_MEASUREMENT_DATA cL3IF_MEASUREMENT_DATA = new CL3IF_MEASUREMENT_DATA();
                        cL3IF_MEASUREMENT_DATA.outMeasurementData = new CL3IF_OUTMEASUREMENT_DATA[ ltnTargetOut.Count ];
                        cL3IF_MEASUREMENT_DATA.addInfo = ( CL3IF_ADD_INFO )Marshal.PtrToStructure( pin.Pointer + iReadPosition, typeof( CL3IF_ADD_INFO ) );
                        iReadPosition += Marshal.SizeOf( typeof( CL3IF_ADD_INFO ) );

                        for ( int nSensorNo = 0; nSensorNo < ltnTargetOut.Count; nSensorNo++ ) {
                            cL3IF_MEASUREMENT_DATA.outMeasurementData[ nSensorNo ] =
                                ( CL3IF_OUTMEASUREMENT_DATA )Marshal.PtrToStructure( pin.Pointer + iReadPosition, typeof( CL3IF_OUTMEASUREMENT_DATA ) );
                            iReadPosition += Marshal.SizeOf( typeof( CL3IF_OUTMEASUREMENT_DATA ) );
                        }

                        CKeyenceCL3000SingleData sensorData = new CKeyenceCL3000SingleData();
                        sensorData.iPulseCount = cL3IF_MEASUREMENT_DATA.addInfo.pulseCount;
                        sensorData.dMeasureData = cL3IF_MEASUREMENT_DATA.outMeasurementData[ 0 ].measurementValue * 0.0001;
                        objListSingleData.Add( sensorData );
                    }

                    if ( m_objCallbackScanData != null ) {
                        CScanData objImageData = new CScanData();
                        objImageData.bGrabComplete = true;
                        objImageData.objSensorDataKeyenceCL3000.objListSingleData.AddRange( objListSingleData );
                        m_objCallbackScanData( objImageData );
                    }
                    return true; // 모든 작업이 성공적으로 완료되면 true 반환
                }
            } catch ( Exception ex ) {
                m_objCallbackExceptionMessage( string.Format( "Sensor {0} GeTrendData - ", m_iSensorIndex, ex.Message ) );
                return false;
            } finally {
                // ArrayPool 해제.
                if ( abyBuffer != null ) {
                    ArrayPool<byte>.Shared.Return( abyBuffer ); // finally 블록에서 ArrayPool 반환
                }
            }
        }
        /// <summary>
        /// 혹시 몰라 남겨둠... 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            try {
                bool result = await ScanStartAsync( token ); // await 키워드 사용

                if ( result ) {
                    Console.WriteLine( "Scan started successfully." );
                }
                else {
                    Console.WriteLine( "Scan failed to start." );
                }

                return result; // 결과 반환
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"ScanStart Error: {ex.Message}" );
                return false;
            }
        }
        /// <summary>
        /// 혹시 몰라 남겨둠... 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ScanStartAsync( CancellationToken cancellationToken )
        {
            bool bReturn = false;
            uint iIndexStart = 0;
            List<CKeyenceCL3000SingleData> objListSingleData = new List<CKeyenceCL3000SingleData>();
            CScanData objImageData = new CScanData();

            // abybuffer 선언 및 ArrayPool 사용
            byte[] abybuffer = ArrayPool<byte>.Shared.Rent( m_iRequestDataLength );

            try {
                if ( false == InitializeSensorAndGetStartIndex() ) {
                    return false;
                }

                uint iIndexCurrent = iIndexStart;
                m_iRetryCount = 0;

                while ( false == cancellationToken.IsCancellationRequested ) {
                    if ( false == await GetDataAndProcessAsync( abybuffer, iIndexCurrent, objListSingleData, cancellationToken ) ) {
                        if ( cancellationToken.IsCancellationRequested || m_iRetryCount > 200 ) {
                            break;
                        }
                    }

                    iIndexCurrent = m_iIndexNext;
                    await Task.Delay( 50, cancellationToken );
                }

                // 작업 완료 후 처리
                if ( cancellationToken.IsCancellationRequested || m_iRetryCount > 200 ) {
                    m_objCallbackMessage( string.Format( "Sensor {0} ScanStart Cancelled", m_iSensorIndex ) );
                    return bReturn;
                }
                else {
                    m_objCallbackMessage( string.Format( "Sensor {0} ScanStart Completed", m_iSensorIndex ) );

                    // 콜백 호출
                    if ( m_objCallbackScanData != null ) {
                        objImageData.bGrabComplete = true;
                        objImageData.objSensorDataKeyenceCL3000.objListSingleData = objListSingleData;
                        m_objCallbackScanData( objImageData );
                    }
                }

                bReturn = true;
            }
            catch ( Exception ex ) {
                m_objCallbackMessage( string.Format( "ScanStart Error: {0}", ex.Message ) );
                bReturn = false;
                // 필요한 추가 처리 (로깅 등)
            }
            finally {
                // ArrayPool 반환
                ArrayPool<byte>.Shared.Return( abybuffer );
            }

            return bReturn;
        }
        /// <summary>
        /// 혹시 몰라 남겨둠... 
        /// </summary>
        /// <returns></returns>
        private async Task<bool> GetDataAndProcessAsync( byte[] abybuffer, uint iIndexCurrent, List<CKeyenceCL3000SingleData> objListSingleData, CancellationToken cancellationToken )
        {
            int iReturnCode;
           
            uint iGetDataCount = 0;
            CL3IF_OUTNO eOutTarget = 0;
            int iReceiveDataCount = 0;
            try {
                // PinnedObject using 블록 내에서 처리
                using ( PinnedObject pin = new PinnedObject( abybuffer ) ) {
                    iReturnCode = NativeMethods.CL3IF_GetTrendData( m_iSensorIndex, iIndexCurrent, m_iMaxMeasureCount, out m_iIndexNext, out iGetDataCount, out eOutTarget, pin.Pointer );

                    if ( iReturnCode != NativeMethods.CL3IF_RC_OK ) {
                        // 에러 코드에 따른 처리
                        if ( iReceiveDataCount > 10000 ) {
                            m_objCallbackMessage( string.Format( "Sensor {0} Get Data END", m_iSensorIndex ) );
                            return true;
                        }
                        else if ( m_iRetryCount > 200 ) {
                            m_objCallbackMessage( string.Format( "Sensor {0} Get Data Return Code is => {1} [{2}]", m_iSensorIndex, iReturnCode, iReceiveDataCount ) );
                        }
                        else {
                            m_iRetryCount++;
                            //if ( m_iRetryCount % 100 == 0 ) 
                            {
                                m_objCallbackMessage( string.Format( "Sensor {0} Get Data Retry is => {1} [{2}]", m_iSensorIndex, m_iRetryCount, iReceiveDataCount ) );
                            }
                            await Task.Delay( 100, cancellationToken );
                            return false;
                        }
                        return false;
                    }

                    // 데이터 처리 로직 (기존 코드와 동일)
                    int iTrendDataCount = 0;
                    int iReadPosition = 0;
                    List<int> ltnTargetOut = ConvertOutTargetList( eOutTarget );

                    for ( int iLoopCount = 0; iLoopCount < iGetDataCount; iLoopCount++ ) {
                        if ( m_iMaxCount <= iLoopCount + iReceiveDataCount ) {
                            m_objCallbackMessage( string.Format( "Sensor {0} Get Data Count is over Max Count : {1}", m_iSensorIndex, iLoopCount + iReceiveDataCount ) );
                            throw new Exception( "Data count over max count" );
                        }

                        CL3IF_MEASUREMENT_DATA cL3IF_MEASUREMENT_DATA = new CL3IF_MEASUREMENT_DATA();
                        cL3IF_MEASUREMENT_DATA.outMeasurementData = new CL3IF_OUTMEASUREMENT_DATA[ ltnTargetOut.Count ];
                        cL3IF_MEASUREMENT_DATA.addInfo = ( CL3IF_ADD_INFO )Marshal.PtrToStructure( pin.Pointer + iReadPosition, typeof( CL3IF_ADD_INFO ) );
                        iReadPosition += Marshal.SizeOf( typeof( CL3IF_ADD_INFO ) );

                        for ( int nSensorNo = 0; nSensorNo < ltnTargetOut.Count; nSensorNo++ ) {
                            cL3IF_MEASUREMENT_DATA.outMeasurementData[ nSensorNo ] =
                                ( CL3IF_OUTMEASUREMENT_DATA )Marshal.PtrToStructure( pin.Pointer + iReadPosition, typeof( CL3IF_OUTMEASUREMENT_DATA ) );

                            iReadPosition += Marshal.SizeOf( typeof( CL3IF_OUTMEASUREMENT_DATA ) );
                        }

                        iTrendDataCount++;
                        CKeyenceCL3000SingleData sensorData = new CKeyenceCL3000SingleData();
                        sensorData.iPulseCount = cL3IF_MEASUREMENT_DATA.addInfo.pulseCount;
                        sensorData.dMeasureData = cL3IF_MEASUREMENT_DATA.outMeasurementData[ 0 ].measurementValue * 0.0001;
                        objListSingleData.Add( sensorData );
                    }

                    iReceiveDataCount += iTrendDataCount;
                }
            }
            catch ( Exception ex ) {
                m_objCallbackExceptionMessage( string.Format( "GetDataAndProcessAsync Error: {0}", ex.Message ) );
                return false;
            }

            return true;
        }
        /// <summary>
        /// 데이터 획득 프로세스
        /// </summary>
        /// <param name="abybuffer"></param>
        /// <param name="iIndexCurrent"></param>
        /// <param name="iIndexNext"></param>
        /// <param name="iGetDataCount"></param>
        /// <param name="eOutTarget"></param>
        /// <param name="iReceiveDataCount"></param>
        /// <param name="objListSingleData"></param>
        /// <param name="objImageData"></param>
        /// <param name="iRetryCount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool GetDataAndProcess( byte[] abybuffer, uint iIndexCurrent, out uint iIndexNext, out uint iGetDataCount, out CL3IF_OUTNO eOutTarget, ref int iReceiveDataCount, ref List<CKeyenceCL3000SingleData> objListSingleData, ref CScanData objImageData, ref uint iRetryCount, CancellationTokenSource cts )
        {
            int iReturnCode;

            using ( PinnedObject pin = new PinnedObject( abybuffer ) ) {
                iReturnCode = NativeMethods.CL3IF_GetTrendData( m_iSensorIndex, iIndexCurrent, m_iMaxMeasureCount, out iIndexNext, out iGetDataCount, out eOutTarget, pin.Pointer );
                if ( iReturnCode != NativeMethods.CL3IF_RC_OK ) {
                    if ( iReceiveDataCount > 10000 ) {
                        // 데이터 수신 완료
                        if ( m_objCallbackScanData != null ) {
                            objImageData.bGrabComplete = true;
                            objImageData.objSensorDataKeyenceCL3000.objListSingleData = objListSingleData; // 이미 리스트는 생성되어 있음
                            m_objCallbackScanData( objImageData );
                        }
                        m_objCallbackMessage( string.Format( "Sensor {0} Get Data END", m_iSensorIndex ) );
                    }
                    else if ( iRetryCount > 200 ) {
                        m_objCallbackMessage( string.Format( "Sensor {0} Get Data Return Code is => {1} [{2}]", m_iSensorIndex, iReturnCode, iReceiveDataCount ) );
                    }
                    else {
                        iRetryCount++;
                        if ( iRetryCount % 100 == 0 ) {
                            m_objCallbackMessage( string.Format( "Sensor {0} Get Data Retry is => {1} [{2}]", m_iSensorIndex, iRetryCount, iReceiveDataCount ) );
                        }
                        Thread.Sleep( 100 );
                        return false; // 재시도 필요
                    }
                    return false; // 에러 발생
                }

                // 데이터 처리 로직 (기존 코드와 동일)
                int iTrendDataCount = 0;
                int iReadPosition = 0;
                List<int> ltnTargetOut = ConvertOutTargetList( eOutTarget );

                for ( int iLoopCount = 0; iLoopCount < iGetDataCount; iLoopCount++ ) {
                    if ( m_iMaxCount <= iLoopCount + iReceiveDataCount ) {
                        m_objCallbackMessage( string.Format( "Sensor {0} Get Data Count is over Max Count : {1}", m_iSensorIndex, iLoopCount + iReceiveDataCount ) );
                        cts.Cancel(); // 최대 개수 초과 시 취소 요청
                        break;
                    }

                    CL3IF_MEASUREMENT_DATA cL3IF_MEASUREMENT_DATA = new CL3IF_MEASUREMENT_DATA();
                    cL3IF_MEASUREMENT_DATA.outMeasurementData = new CL3IF_OUTMEASUREMENT_DATA[ ltnTargetOut.Count ];
                    cL3IF_MEASUREMENT_DATA.addInfo = ( CL3IF_ADD_INFO )Marshal.PtrToStructure( pin.Pointer + iReadPosition, typeof( CL3IF_ADD_INFO ) );
                    iReadPosition += Marshal.SizeOf( typeof( CL3IF_ADD_INFO ) );

                    for ( int iSensorNo = 0; iSensorNo < ltnTargetOut.Count; iSensorNo++ ) {
                        cL3IF_MEASUREMENT_DATA.outMeasurementData[ iSensorNo ] =
                            ( CL3IF_OUTMEASUREMENT_DATA )Marshal.PtrToStructure( pin.Pointer + iReadPosition, typeof( CL3IF_OUTMEASUREMENT_DATA ) );

                        iReadPosition += Marshal.SizeOf( typeof( CL3IF_OUTMEASUREMENT_DATA ) );
                    }

                    iTrendDataCount++;
                    CKeyenceCL3000SingleData sensorData = new CKeyenceCL3000SingleData();
                    sensorData.iPulseCount = cL3IF_MEASUREMENT_DATA.addInfo.pulseCount;
                    sensorData.dMeasureData = cL3IF_MEASUREMENT_DATA.outMeasurementData[ 0 ].measurementValue * 0.0001;
                    objListSingleData.Add( sensorData );
                }

                iReceiveDataCount += iTrendDataCount;
            }
            return true; // 성공
        }
        /// <summary>
        /// 스캔 정지
        /// </summary>
        /// <returns></returns>
        public override bool ScanStop()
        {
            bool bReturn = false;
            do {
                //if( null != cts ) {
                //    cts.Cancel();
                //    cts.Dispose();
                //    cts = null;
                //}
                GeTrendData();

                bReturn = true;
            } while ( false );
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
            do {

                bReturn = true;
            } while ( false );
            return bReturn;
        }
        private List<int> ConvertOutTargetList( CL3IF_OUTNO outTarget )
        {
            byte mask = 1;
            List<int> outList = new List<int>();

            try {
                for ( int i = 0; i < NativeMethods.CL3IF_MAX_OUT_COUNT; i++ ) {
                    if ( ( ( ushort )outTarget & mask ) != 0 ) {
                        outList.Add( i + 1 );
                    }
                    mask = ( byte )( mask << 1 );
                }
            }
            catch ( Exception ex ) {
                m_objCallbackExceptionMessage( "ConvertOutTargetList - " + ex.Message );
            }

            return outList;
        }
        /// <summary>
        /// 센서 연결
        /// </summary>
        /// <returns></returns>
        private bool ConnectSensor()
        {
            bool bReturn = false;
            int iReturnCode = 0;
            
            try {
                do {
                    // IP 주소 변환
                    IPAddress address = IPAddress.Parse( m_objInitializeParameter.strSensorIP );
                    CL3IF_ETHERNET_SETTING cL3IF_ETHERNET_SETTING = new CL3IF_ETHERNET_SETTING();
                    cL3IF_ETHERNET_SETTING.ipAddress = address.GetAddressBytes();
                    if ( false == ushort.TryParse( m_objInitializeParameter.strSensorPort, out cL3IF_ETHERNET_SETTING.portNo ) ) {
                        m_objCallbackExceptionMessage( "ConnectSensor - " + string.Format( "m_objInitializeParameter.strSensorPort is Fail [ {0} ]", m_objInitializeParameter.strSensorPort ) );
                        break;
                    }
                    // 센서 연결 시도.
                    iReturnCode = NativeMethods.CL3IF_OpenEthernetCommunication( m_iSensorIndex, ref cL3IF_ETHERNET_SETTING, 10000 );

                    bReturn = ( iReturnCode == NativeMethods.CL3IF_RC_OK ) ? true : false;
                } while ( false );
            }
            catch ( Exception ex ){
                m_objCallbackExceptionMessage( "ConnectSensor - " + ex.Message );
            }
            return bReturn;
        }
        /// <summary>
        /// 센서 연결 해제
        /// </summary>
        /// <returns></returns>
        private bool DisconnectSensor()
        {
            bool bReturn = false;
            int iReturnCode = 0;

            try {
                do {
                    // 센서 해제 시도.
                    iReturnCode = NativeMethods.CL3IF_CloseCommunication( m_iSensorIndex );

                    bReturn = ( iReturnCode == NativeMethods.CL3IF_RC_OK ) ? true : false;
                } while ( false );
            }
            catch ( Exception ex ) {
                m_objCallbackExceptionMessage( "ConnectSensor - " + ex.Message );
            }
            return bReturn;
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
                if ( pingReply.Status != IPStatus.Success ) {
                    iSensorCheck++;
                }
                else {
                    iSensorCheck = 0;
                }
                // 3보다 크거나 같을 경우 연결이 끊긴 걸로 간주한다.
                if ( iSensorCheck >= 3 ) break;

                bReturn = true;
            } while ( false );
            return bReturn;
        }
        /// <summary>
        /// 키엔스 CL3000 명령어 - 엔코더 간격.... 
        /// </summary>
        /// <param name="iSkipping"></param>
        /// <returns></returns>
        public bool SetEncoderSkipping( int iSkipping )
        {
            bool bReturn = false;
            do {
                try {
                    // 접속되었을 때만 사용
                    if ( true == IsConnected() ) {
                        CL3IF_ENCODER_SETTING cL3IF_ENCODER_SETTING = new CL3IF_ENCODER_SETTING();
                        NativeMethods.CL3IF_GetEncoder( m_iSensorIndex, out cL3IF_ENCODER_SETTING );
                        cL3IF_ENCODER_SETTING.encoderOnOff = true;
                        cL3IF_ENCODER_SETTING.operatingMode = ( int )CL3IF_ENCODER_OPERATING_MODE.CL3IF_ENCODER_OPERATING_MODE_TRIGGER;
                        cL3IF_ENCODER_SETTING.enterMode = ( int )CL3IF_ENCODER_ENTER_MODE.CL3IF_ENCODER_ENTER_MODE_2_PHASE_4_MULTIPLIER;
                        cL3IF_ENCODER_SETTING.decimationPoint = ( short )iSkipping;
                        cL3IF_ENCODER_SETTING.detectionEdge = ( int )CL3IF_ENCODER_DETECTION_EDGE.CL3IF_ENCODER_DETECTION_EDGE_BOTH_EDGE;
                        cL3IF_ENCODER_SETTING.minInputTime = ( int )CL3IF_ENCODER_MIN_INPUT_TIME.CL3IF_ENCODER_MIN_INPUT_TIME_500ns;
                        cL3IF_ENCODER_SETTING.pulseCountOffsetDetectionLogic = ( int )CL3IF_ENCODER_PULSE_COUNT_OFFSET_DETECTION_LOGIC.CL3IF_ENCODER_PULSE_COUNT_OFFSET_DETECTION_LOGIC_POSITIVE;
                        cL3IF_ENCODER_SETTING.presetValue = 0;
                        NativeMethods.CL3IF_SetEncoder( m_iSensorIndex, ref cL3IF_ENCODER_SETTING );
                    }
                }
                catch ( Exception ex ) {
                    m_objCallbackExceptionMessage( "SetEncoderSkipping - " + ex.Message );
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
            CDeviceSensorKeyenceCL3000 pThis = ( CDeviceSensorKeyenceCL3000 )state;
            int iSensorCheck = 0;

            while ( false == pThis.m_bThreadExit ) {
                // 센서 연결 상태가 False일 경우 
                if( false == pThis.m_bConnect ) {
                    pThis.m_bConnect = pThis.ConnectSensor();
                } else {
                    // 센서 핑테스트 실패시 재 접속 하자.
                    if( false == pThis.PingReplyFailSensor( ref iSensorCheck) ) {
                        pThis.m_bConnect = false;
                    }
                }
                Thread.Sleep( 1000 );
            }
        }
    }
}
