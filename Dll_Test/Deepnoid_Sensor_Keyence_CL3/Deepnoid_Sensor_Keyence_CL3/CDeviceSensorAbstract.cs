using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Deepnoid_Sensor_Keyence_CL3 {
	public abstract class CDeviceSensorAbstract
	{
        /// <summary>
        /// 초기화 파라미터 클래스
        /// </summary>
        public class CInitializeParameterCL3 : ICloneable
		{
            /// <summary>
            /// 센서 인덱스
            /// </summary>
            public int iSensorIndex;
            /// <summary>
            /// 3D센서용 카메라 IP
            /// </summary>
            public string strSensorIP;
            /// <summary>
            /// 3D 센서용 카메라 PORT
            /// </summary>
            public string strSensorPort;
			/// <summary>
            /// 가상 이미지 경로
            /// </summary>
			public string strImagePath;

			public object Clone()
			{
                CInitializeParameterCL3 objInitializeParameter = new CInitializeParameterCL3();
                objInitializeParameter.iSensorIndex = iSensorIndex;
                objInitializeParameter.strSensorIP = strSensorIP;
                objInitializeParameter.strSensorPort = strSensorPort;
                objInitializeParameter.strImagePath = strImagePath;
				return objInitializeParameter;
			}
		}
        /// <summary>
        /// Keyence CL3000 센서 측정 데이터
        /// </summary>
        public class CKeyenceCL3000SingleData {
            public int iPulseCount { get; set; }
            public double dMeasureData { get; set; }
        }
        /// <summary>
        /// 카메라 데이터
        /// </summary>
        public class CScanDataKeyenceCL3000 {
            public List<CKeyenceCL3000SingleData> objListSingleData;

            public CScanDataKeyenceCL3000()
            {
                objListSingleData = new List<CKeyenceCL3000SingleData>();
            }
        }
        public class CScanData : ICloneable
        {
            public bool bGrabComplete = false;
            public CScanDataKeyenceCL3000 objSensorDataKeyenceCL3000 = new CScanDataKeyenceCL3000();
            public object Clone()
            {
                CScanData objImageData = new CScanData();
                objImageData.bGrabComplete = bGrabComplete;
                objImageData.objSensorDataKeyenceCL3000 = objSensorDataKeyenceCL3000;
                return objImageData;
            }
        }
		/// <summary>
        /// 델리게이트 선언
        /// </summary>
        /// <param name="objImageData"></param>
		public delegate void CallBackScanData( CScanData objImageData );
        /// <summary>
        /// 메시지 콜백
        /// </summary>
        /// <param name="strMessage"></param>
        public delegate void CallBackMessage( string strMessage );
        /// <summary>
        /// Exception 콜백
        /// </summary>
        /// <param name="strException"></param>
		public delegate void CallBackExceptionMessage( string strException );
        /// <summary>
        /// 초기화 추상화
        /// </summary>
        /// <param name="objInitializeParameter"></param>
        /// <returns></returns>
        public abstract bool Initialize( CInitializeParameterCL3 objInitializeParameter );
        /// <summary>
        /// 해제 추상화
        /// </summary>
        public abstract void DeInitialize();
        /// <summary>
        /// 데이터 콜백
        /// </summary>
        /// <param name="objCallback"></param>
        public abstract void SetCallbackScanData( CallBackScanData objCallback );
        /// <summary>
        /// 메시지 콜백
        /// </summary>
        /// <param name="objCallback"></param>
        public abstract void SetCallbackMessage( CallBackMessage objCallback );
        /// <summary>
        /// 메시지 콜백
        /// </summary>
        /// <param name="objCallback"></param>
        public abstract void SetCallbackExceptionMessage( CallBackExceptionMessage objCallback );
        /// <summary>
        /// 연결 상태
        /// </summary>
        /// <returns></returns>
        public abstract bool IsConnected();
        /// <summary>
        /// 센서 스캔 시작
        /// </summary>
        /// <returns></returns>
        public abstract bool ScanStart();
        /// <summary>
        /// 센서 스캔 정지
        /// </summary>
        /// <returns></returns>
        public abstract bool ScanStop();
        /// <summary>
        /// 센서 리셋
        /// </summary>
        /// <returns></returns>
        public abstract bool SensorReset( bool bWait );
	}
}