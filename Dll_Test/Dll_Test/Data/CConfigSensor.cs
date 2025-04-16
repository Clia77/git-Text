using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Data
{
	public partial class CConfig
	{
		/// <summary>
		/// 센서 파라미터 모음 ( 센서 갯수에 따라 )
		/// </summary>
		public class SensorParameter
		{
			public List<SensorData> objSensors { get; set; }

			public SensorParameter()
			{
				objSensors = new List<SensorData>();
			}
		}

		/// <summary>
		/// 센서 연결 파라미터
		/// </summary>
		public class SensorData
		{
			/// <summary>
			/// 센서 아이디
			/// </summary>
			public string strSensorID { get; set; }
			/// <summary>
			/// 센서 접속 IP
			/// </summary>
			public string strIpAddress { get; set; }
			/// <summary>
			/// 센서 접속 포트 번호
			/// </summary>
			public string strPortNumber { get; set; }
			/// <summary>
			/// 센서 고속 데이터 통신 포트 번호 ( Keyence LJX8000A에서 사용함 )
			/// </summary>
			public string strHighSpeedDataPortNumber { get; set; }
			/// <summary>
			/// 시리얼 번호
			/// </summary>
			public string strSerialNumber { get; set; }
		}

		/// <summary>
		/// 센서 파라미터 불러오기
		/// </summary>
		/// <returns></returns>
		public bool LoadSensorParameter()
		{
			try {
				string strPath = $@"{m_objSystemParameter.strItemPath}Sensor.Json";

				if( File.Exists( strPath ) ) {
					string json = File.ReadAllText( strPath );
					m_objSensorParameter = JsonConvert.DeserializeObject<SensorParameter>( json );
					return true;
				} else {
					// 파일이 없는 경우 기본값으로 RootParameter 객체 생성 후 반환
					SensorParameter sensorData = new SensorParameter();
					DefaultValue( out sensorData );
					SaveSensorParameter( sensorData );
					return false;
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
				return false;
			}
		}

		/// <summary>
		/// 센서 파라미터 저장
		/// </summary>
		/// <returns></returns>
		public bool SaveSensorParameter( SensorParameter objParameter )
		{
			bool bResult = false;
			try {
				m_objSensorParameter = objParameter;
				string strPath = $@"{m_objSystemParameter.strItemPath}\Sensor.Json";
				string json = JsonConvert.SerializeObject( m_objSensorParameter, Formatting.Indented );
				File.WriteAllText( strPath, json );
				bResult = true;
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
			return bResult;
		}

		/// <summary>
		/// 저장된 설정파일이 없을 경우 기본값으로 대체. 센서의 경우 최대 2개 ( 탭돌출때문... )
		/// </summary>
		/// <param name="sensorData"></param>
		private void DefaultValue( out SensorParameter sensorData )
		{
			sensorData = new SensorParameter();
			sensorData.objSensors = new List<SensorData>();
			for( int iLoopCount = 0; iLoopCount < 2; iLoopCount++ ) {
				SensorData sensorParameter = new SensorData();
				sensorParameter.strSensorID = $"{iLoopCount}";
				sensorParameter.strIpAddress = "127.0.0.1";
				sensorParameter.strPortNumber = "5000";
				sensorParameter.strHighSpeedDataPortNumber = "5001";
				sensorParameter.strSerialNumber = "123456789";
				sensorData.objSensors.Add( sensorParameter );
			}
		}

		/// <summary>
		/// 시스템 파라미터 객체
		/// </summary>
		/// <returns></returns>
		public SensorParameter GetSensorParameter()
		{
			return m_objSensorParameter;
		}
	}
}
