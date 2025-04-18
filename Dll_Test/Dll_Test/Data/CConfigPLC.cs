﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Data
{
	public partial class CConfig
	{
		/// <summary>
		/// PLC 연결 파라미터
		/// </summary>
		public class PLCParameter
		{
			/// <summary>
			/// PLC 접속 IP
			/// </summary>
			public string strIpAddress { get; set; }
			/// <summary>
			/// PLC 접속 포트 번호
			/// </summary>
			public string strPortNumber { get; set; }
		}

		/// <summary>
		/// 센서 파라미터 불러오기
		/// </summary>
		/// <returns></returns>
		public bool LoadPLCParameter()
		{
			try {
				string strPath = $@"{m_objSystemParameter.strItemPath}PLC.Json";

				if( File.Exists( strPath ) ) {
					string json = File.ReadAllText( strPath );
					m_objPLCParameter = JsonConvert.DeserializeObject<PLCParameter>( json );
					return true;
				} else {
					strPath = $@"{m_objSystemParameter.strItemPath}";
					DirectoryInfo directoryInfo = new DirectoryInfo( strPath );
					if( false == directoryInfo.Exists ) {
						Directory.CreateDirectory( strPath );
					}
					strPath += "PLC.Json";
					// 파일이 없는 경우 기본값으로 RootParameter 객체 생성 후 반환
					PLCParameter sensorData = new PLCParameter();
					DefaultValue( out sensorData );
					SavePLCParameter( sensorData );
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
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public bool SavePLCParameter( PLCParameter objParameter )
		{
			bool bResult = false;
			try {
				m_objPLCParameter = objParameter;
				string strPath = $@"{m_objSystemParameter.strItemPath}\PLC.Json";
				string json = JsonConvert.SerializeObject( m_objPLCParameter, Formatting.Indented );
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
		private void DefaultValue( out PLCParameter plcParameter )
		{
			plcParameter = new PLCParameter();
			plcParameter.strIpAddress = "127.0.0.1";
			plcParameter.strPortNumber = "7000";
		}

		/// <summary>
		/// 시스템 파라미터 객체
		/// </summary>
		/// <returns></returns>
		public PLCParameter GetPLCParameter()
		{
			return m_objPLCParameter;
		}
	}
}
