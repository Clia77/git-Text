using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Deepnoid_Sensor {
	public abstract class CDeviceSensorAbstract
	{
        #region PinnedObject
        /// <summary>
        /// Object pinning class
        /// </summary>
        public sealed class PinnedObject : IDisposable {
            #region Field

            private GCHandle _handle;      // Garbage collector handle

            #endregion

            #region Property

            /// <summary>
            /// Get the address.
            /// </summary>
            public IntPtr Pointer
            {
                // Get the leading address of the current object that is pinned.
                get { return _handle.AddrOfPinnedObject(); }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="target">Target to protect from the garbage collector</param>
            public PinnedObject( object target )
            {
                // Pin the target to protect it from the garbage collector.
                _handle = GCHandle.Alloc( target, GCHandleType.Pinned );
            }

            #endregion

            #region Interface
            /// <summary>
            /// Interface
            /// </summary>
            public void Dispose()
            {
                _handle.Free();
                _handle = new GCHandle();
            }

            #endregion
        }
        #endregion

        /// <summary>
        /// 초기화 파라미터 클래스
        /// </summary>
        public class CInitializeParameter : ICloneable
		{
            /// <summary>
            /// 센서 인덱스
            /// </summary>
            public int iSensorIndex;
			/// <summary>
            /// 카메라 픽셀 해상도
            /// </summary>
			public double dResolution;
            /// <summary>
            /// 3D센서용 카메라 IP
            /// </summary>
            public string strSensorIP;
            /// <summary>
            /// 3D 센서용 카메라 PORT
            /// </summary>
            public string strSensorPort;
            /// <summary>
            /// Keyence용 고속 데이터 Port
            /// </summary>
            public string strSensorHighSpeedDataPort;
			/// <summary>
            /// 가상 이미지 경로
            /// </summary>
			public string strImagePath;

			public object Clone()
			{
				CInitializeParameter objInitializeParameter = new CInitializeParameter();
                objInitializeParameter.iSensorIndex = iSensorIndex;
                objInitializeParameter.strSensorIP = strSensorIP;
                objInitializeParameter.strSensorPort = strSensorPort;
                objInitializeParameter.strSensorHighSpeedDataPort = strSensorHighSpeedDataPort;
                objInitializeParameter.dResolution = dResolution;
                objInitializeParameter.strImagePath = strImagePath;

				return objInitializeParameter;
			}
		}
        /// <summary>
        /// 센서 데이터
        /// </summary>
        public class CScanDataGocator {
            // 센서 높이 해상도
            public int iOffsetX;
            public int iOffsetY;
            public int iOffsetZ;
            public int iResolutionY;
            public int iResolutionX;
            public int iResolutionZ;
            public int iWidth;
            public int iHeight;

            // 센서에서 받은 높이 데이터
            public  short[] objHeightDataOrigin;
            public double[] objHeightDataDoubleOrigin;
            // 센서에서 받은 밝기 데이터
            public byte[] objIntensityDataOrigin;
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
        public class CScanDataKeyenceLJX8000A {
            public List<int[]> objListProfileData;
            public List<double[]> objListProfileDoubleData;
            public List<int[]> objListLuminanceData;

            public CScanDataKeyenceLJX8000A()
            {
                objListProfileData = new List<int[]>();
                objListProfileDoubleData = new List<double[]>();
                objListLuminanceData = new List<int[]>();
            }
        }
        public class CScanData : ICloneable
        {
            public bool bGrabComplete = false;
			public int iWidth = 0;
			public int iHeight = 0;
            public CScanDataGocator objSensorDataGocator = new CScanDataGocator();
            public CScanDataKeyenceCL3000 objSensorDataKeyenceCL3000 = new CScanDataKeyenceCL3000();
            public CScanDataKeyenceLJX8000A objSensorDataKeyenceLJX8000A = new CScanDataKeyenceLJX8000A();
            public object Clone()
            {
                CScanData objImageData = new CScanData();
                objImageData.bGrabComplete = bGrabComplete;
				objImageData.iWidth = iWidth;
                objImageData.iHeight = iHeight;
                objImageData.objSensorDataGocator = objSensorDataGocator;
                objImageData.objSensorDataKeyenceCL3000 = objSensorDataKeyenceCL3000;
                objImageData.objSensorDataKeyenceLJX8000A = objSensorDataKeyenceLJX8000A;

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
        public abstract bool Initialize( CInitializeParameter objInitializeParameter );
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