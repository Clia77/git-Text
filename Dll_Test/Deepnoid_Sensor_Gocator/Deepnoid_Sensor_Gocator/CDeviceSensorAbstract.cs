using System;

namespace Deepnoid_Sensor_Gocator
{
	public abstract class CDeviceSensorAbstract
	{
		/// <summary>
		/// 초기화 파라미터 클래스
		/// </summary>
		public class CInitializeParameterGocator : ICloneable
		{
			/// <summary>
			/// 3D센서용 카메라 IP
			/// </summary>
			public string strSensorIP;
			/// <summary>
			/// 가상 이미지 경로
			/// </summary>
			public string strImagePath;

			public CInitializeParameterGocator()
			{
				strSensorIP = "";
				strImagePath = "";
			}

			public object Clone()
			{
				CInitializeParameterGocator objInitializeParameter = new CInitializeParameterGocator();
				objInitializeParameter.strSensorIP = strSensorIP;
				objInitializeParameter.strImagePath = strImagePath;
				return objInitializeParameter;
			}
		}

		/// <summary>
		/// 센서 데이터
		/// </summary>
		public class CScanDataGocator
		{
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
			public short[] objHeightDataOrigin;
			public double[] objHeightDataDoubleOrigin;
			// 센서에서 받은 밝기 데이터
			public byte[] objIntensityDataOrigin;
		}

		public class CScanData : ICloneable
		{
			public bool bGrabComplete;
			public int iWidth;
			public int iHeight;
			public CScanDataGocator objSensorDataGocator;

			public CScanData()
			{
				bGrabComplete = false;
				iWidth = 0;
				iHeight = 0;
				objSensorDataGocator = new CScanDataGocator();
			}

			public object Clone()
			{
				CScanData objImageData = new CScanData();
				objImageData.bGrabComplete = bGrabComplete;
				objImageData.iWidth = iWidth;
				objImageData.iHeight = iHeight;
				objImageData.objSensorDataGocator = objSensorDataGocator;
				return objImageData;
			}
		}

		/// <summary>
		/// 델리게이트 선언
		/// </summary>
		/// <param name="objImageData"></param>
		public delegate void CallBackScanData( CScanData objImageData );
		protected CallBackScanData _callBackScanData;
		public void SetCallbackScanData( CallBackScanData callBack )
		{
			_callBackScanData = callBack;
		}
		/// <summary>
		/// 메시지 콜백
		/// </summary>
		/// <param name="strMessage"></param>
		public delegate void CallBackMessage( string strMessage );
		protected CallBackMessage _callBackMessage;
		public void SetCallbackMessage( CallBackMessage callBack )
		{
			_callBackMessage = callBack;
		}
		/// <summary>
		/// Exception 콜백
		/// </summary>
		/// <param name="strException"></param>
		public delegate void CallBackExceptionMessage( string strException );
		protected CallBackExceptionMessage _callBackExceptionMessage;
		public void SetCallbackExceptionMessage( CallBackExceptionMessage callBack )
		{
			_callBackExceptionMessage = callBack;
		}
		/// <summary>
		/// 초기화 추상화
		/// </summary>
		/// <param name="objInitializeParameter"></param>
		/// <returns></returns>
		public abstract bool Initialize( CInitializeParameterGocator objInitializeParameter );
		/// <summary>
		/// 해제 추상화
		/// </summary>
		public abstract void DeInitialize();
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