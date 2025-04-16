using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using static Deepnoid_Sensor_Keyence_CL3.CDeviceSensorAbstract;

namespace Deepnoid_Sensor_Keyence_CL3 {
    public class CDeviceSensorVirtualKeyenceCL3 : CDeviceSensorAbstract {
        /// <summary>
        /// 카메라 인덱스
        /// </summary>
        private int m_iSensorIndex;
        /// <summary>
        /// 이미지 데이터
        /// </summary>
        private List<CScanData> m_lstImageData;
        /// <summary>
        /// 이미지 인덱스
        /// </summary>
        private int m_iImageIndex = 0;
        /// <summary>
        /// 가상 이미지 경로
        /// </summary>
        private string m_strImagePath;
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
        /// 생성자
        /// </summary>
        public CDeviceSensorVirtualKeyenceCL3()
        {
            m_objCallbackScanData = null;
            m_objCallbackMessage = null;
            m_objCallbackExceptionMessage = null;
            m_lstImageData = new List<CScanData>();
        }
        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="objInitializeParameter"></param>
        /// <returns></returns>
        public override bool Initialize( CInitializeParameterCL3 objInitializeParameter )
        {
            bool bReturn = false;

            do {
                // 변수 초기화
                m_iSensorIndex = objInitializeParameter.iSensorIndex;
                m_strImagePath = objInitializeParameter.strImagePath;
                InitializeImage();
                bReturn = true;
            } while ( false );

            return bReturn;
        }
        /// <summary>
        /// 해제
        /// </summary>
        public override void DeInitialize()
        {
            if ( null != m_lstImageData ) m_lstImageData.Clear();
        }
        /// <summary>
        /// 특정 폴더 이미지를 로딩하여 가상 모드에서 사용
        /// </summary>
        /// <returns></returns>
        private bool InitializeImage()
        {
            bool bReturn = false;

            do {

                try {
                    string strImagePath = m_strImagePath;

                    m_lstImageData.Clear();

                    // 폴더 없는 경우 나가기
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo( strImagePath );
                    if ( di.Exists == false ) break;

                    // 시뮬레이션 폴더 내 폴더 목록 불러오기
                    string[] arFileDirectories = System.IO.Directory.GetDirectories( strImagePath );

                    // 각 폴더안에 파일리스트 가져오기... 
                    for ( int iLoop = 0; iLoop < arFileDirectories.Length; iLoop++ ) {
                        string[] astrFiles = Directory.GetFiles( arFileDirectories[ iLoop ] );
                        for ( int jLoopFile = 0; jLoopFile < astrFiles.Length; jLoopFile++ ) {
                            string strFileFullPath = astrFiles[ jLoopFile ];
                            if ( -1 == strFileFullPath.IndexOf( ".csv" ) || -1 != strFileFullPath.IndexOf( "Result.csv" ) ) continue;
                            if ( 0 == m_iSensorIndex ) {
                                if ( -1 == strFileFullPath.IndexOf( "CENTER" ) ) continue;
                            }
                            else {
                                if ( -1 == strFileFullPath.IndexOf( "END" ) ) continue;
                            }
                            // 파일 읽기.
                            using ( StreamReader fileStream = new StreamReader( strFileFullPath ) ) {
                                string strData;
                                CScanData cScanData = new CScanData();
                                // 첫줄읽고 버리자.
                                fileStream.ReadLine();
                                // 두번째 줄 부터 실 데이터임.
                                while ( ( strData = fileStream.ReadLine() ) != null ) {
                                    CKeyenceCL3000SingleData cKeyenceCL3000SingleData = new CKeyenceCL3000SingleData();
                                    cKeyenceCL3000SingleData.iPulseCount = int.Parse( strData.Substring( 0, strData.IndexOf( "," ) ) );
                                    string strMeasureData = strData.Substring( strData.IndexOf( "," ) + 1 );
                                    if ( true == strMeasureData.Contains( "," ) ) {
                                        cKeyenceCL3000SingleData.dMeasureData = double.Parse( strMeasureData.Substring( 0, strMeasureData.IndexOf( "," ) ) ) * 1;
                                    }
                                    else {
                                        cKeyenceCL3000SingleData.dMeasureData = double.Parse( strMeasureData ) * 1;
                                    }
                                    // 데이터 리스트에 넣자.
                                    cScanData.objSensorDataKeyenceCL3000.objListSingleData.Add( cKeyenceCL3000SingleData );
                                }
                                m_lstImageData.Add( cScanData );
                            }
                        }
                    }
                }
                catch { }

            } while ( false );

            return bReturn;
        }

        /// <summary>
        /// 연결 상태
        /// </summary>
        /// <returns></returns>
        public override bool IsConnected()
        {
            return true;
        }
        /// <summary>
        /// 고게이터 상태 중 Connected 상태 확인.
        /// </summary>
        /// <returns></returns>
        public bool IsGogatorStatusConnected()
        {
            bool bReturn = false;
            do {

            } while ( false );
            return bReturn;
        }
        /// <summary>
        /// 데이터 콜백
        /// </summary>
        /// <param name="objCallback"></param>
        public override void SetCallbackScanData( CallBackScanData objCallback )
        {
            m_objCallbackScanData = objCallback;
        }
        /// <summary>
        /// 메시지 콜백
        /// </summary>
        /// <param name="objCallback"></param>
        public override void SetCallbackMessage( CallBackMessage objCallback )
        {
            m_objCallbackMessage = objCallback;
        }
        /// <summary>
        /// 예외 콜백
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
                if ( null != m_objCallbackScanData ) {
                    if ( 0 == m_lstImageData.Count ) break;
                    CScanData objImageData = m_lstImageData[ m_iImageIndex ].Clone() as CScanData;

                    m_objCallbackScanData( objImageData.Clone() as CScanData );
                }
                m_iImageIndex++;

                if ( m_iImageIndex >= m_lstImageData.Count )
                    m_iImageIndex = 0;

                bReturn = true;
            } while ( false );
            return bReturn;
        }
        /// <summary>
        /// 스캔 정지
        /// </summary>
        /// <returns></returns>
        public override bool ScanStop()
        {
            bool bReturn = false;

            do {
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
    }
}
