using System;
using System.Collections.Generic;

namespace Deepnoid_Sensor_Keyence_LJX8
{
	public abstract class CDeviceSensorAbstract
	{
		/// <summary>
		/// 초기화 파라미터 클래스
		/// </summary>
		public class CInitializeParameterLJX8 : ICloneable
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
			/// Keyence용 고속 데이터 Port
			/// </summary>
			public string strSensorHighSpeedDataPort;
			/// <summary>
			/// 가상 이미지 경로
			/// </summary>
			public string strImagePath;

			public CInitializeParameterLJX8()
			{
				iSensorIndex = 0;
				strSensorIP = "";
				strSensorPort = "";
				strSensorHighSpeedDataPort = "";
				strImagePath = "";
			}

			public object Clone()
			{
				CInitializeParameterLJX8 objInitializeParameter = new CInitializeParameterLJX8();
				objInitializeParameter.iSensorIndex = iSensorIndex;
				objInitializeParameter.strSensorIP = strSensorIP;
				objInitializeParameter.strSensorPort = strSensorPort;
				objInitializeParameter.strSensorHighSpeedDataPort = strSensorHighSpeedDataPort;
				objInitializeParameter.strImagePath = strImagePath;
				return objInitializeParameter;
			}
		}

		public class CScanDataKeyenceLJX8
		{
			public List<int[]> objListProfileData;
			public List<double[]> objListProfileDoubleData;
			public List<int[]> objListLuminanceData;

			public CScanDataKeyenceLJX8()
			{
				objListProfileData = new List<int[]>();
				objListProfileDoubleData = new List<double[]>();
				objListLuminanceData = new List<int[]>();
			}
		}

		public class CScanData : ICloneable
		{
			public bool bGrabComplete;
			public int iWidth;
			public int iHeight;
			public CScanDataKeyenceLJX8 objSensorDataKeyenceLJX8;

			public CScanData()
			{
				bGrabComplete = false;
				iWidth = 0;
				iHeight = 0;
				objSensorDataKeyenceLJX8 = new CScanDataKeyenceLJX8();
			}

			public object Clone()
			{
				CScanData objImageData = new CScanData();
				objImageData.bGrabComplete = bGrabComplete;
				objImageData.iWidth = iWidth;
				objImageData.iHeight = iHeight;
				objImageData.objSensorDataKeyenceLJX8 = objSensorDataKeyenceLJX8;
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
		public abstract bool Initialize( CInitializeParameterLJX8 objInitializeParameter );
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