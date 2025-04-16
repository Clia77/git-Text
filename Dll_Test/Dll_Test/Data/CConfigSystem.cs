using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Data
{
	public partial class CConfig
	{
		/// <summary>
		/// 시스템 파라미터
		/// </summary>
		public class SystemParameter
		{
			/// <summary>
			/// 설비 타입
			/// </summary>
			public enumEquipmentType eEquipmentType { get; set; }
			/// <summary>
			/// 언어
			/// </summary>
			public enumLanguage eLanguage { get; set; }
			/// <summary>
			/// 시뮬레이션 모드 
			/// </summary>
			public enumSimulationMode eSimulationMode { get; set; }
			/// <summary>
			/// Item 폴더 경로
			/// </summary>
			public string strItemPath { get; set; }
			/// <summary>
			/// 레시피 폴더 경로
			/// </summary>
			public string strRecipePath { get; set; }
			/// <summary>
			/// 레시피 ID
			/// </summary>
			public string strCurrentRecipeID { get; set; }
			/// <summary>
			/// 레시피 이름
			/// </summary>
			public string strCurrentRecipeName { get; set; }
			/// <summary>
			/// 이미지 저장
			/// </summary>
			public bool bImageSave { get; set; }
			/// <summary>
			/// 이미지 저장 기간
			/// </summary>
			public int iImagePeriod { get; set; }
			/// <summary>
			/// 이미지 저장 경로
			/// </summary>
			public string strImagePath { get; set; }
			/// <summary>
			/// Alive 신호 주기
			/// </summary>
			public int iAliveTimePeriod { get; set; }
			/// <summary>
			/// 시뮬레이션 이미지 폴더 경로
			/// </summary>
			public string strSimulationImagePath { get; set; }
			/// <summary>
			/// 바이패스 모드
			/// </summary>
			public enumBypassMode eBypassMode { get; set; }
			/// <summary>
			/// 데이터베이스 파라미터
			/// </summary>
			public DatabaseParameter objDatabaseParameter { get; set; }

			public int iObjectID { get; set; }
		}

		/// <summary>
		/// 시스템 파라미터 불러오기
		/// </summary>
		/// <returns></returns>
		public bool LoadSystemParameter()
		{
			try {
				string strPath = Environment.CurrentDirectory + @"\Config\Config.Json";

				if( File.Exists( strPath ) ) {
					string json = File.ReadAllText( strPath );
					m_objSystemParameter = JsonConvert.DeserializeObject<SystemParameter>( json );
					return true;
				} else {
					// 파일이 없는 경우 기본값으로 RootParameter 객체 생성 후 반환
					SystemParameter systemParameter = new SystemParameter();
					DefaultValue( out systemParameter );
					SaveSystemParameter( systemParameter );
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
		/// 시스템 파라미터 저장
		/// </summary>
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public bool SaveSystemParameter( SystemParameter objParameter )
		{
			bool bResult = false;
			try {
				m_objSystemParameter = objParameter;
				string strPath = Environment.CurrentDirectory + @"\Config\Config.Json";
				string json = JsonConvert.SerializeObject( m_objSystemParameter, Formatting.Indented );
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
		/// 저장된 설정파일이 없을 경우 기본값으로 대체.
		/// </summary>
		/// <param name="systemParameter"></param>
		private void DefaultValue( out SystemParameter systemParameter )
		{
			systemParameter = new SystemParameter();
			systemParameter.eLanguage = enumLanguage.LANGUAGE_ENGLISH;
			systemParameter.eEquipmentType = enumEquipmentType.TYPE_FLATNESS;
			systemParameter.eBypassMode = enumBypassMode.MODE_BYPASS_OFF;
			systemParameter.eSimulationMode = enumSimulationMode.MODE_SIMULATION_ON;
			systemParameter.bImageSave = true;
			systemParameter.strImagePath = @"D:\IMAGE\";
			systemParameter.strSimulationImagePath = @"D:\DeepnoidInspection\Item\SimulationImages\";
			systemParameter.strCurrentRecipeID = "1";
			systemParameter.strCurrentRecipeName = "Default";
			systemParameter.strRecipePath = @"D:\DeepnoidInspection_Recipe\";
			systemParameter.strItemPath = @"D:\DeepnoidInspection\Item\";
			systemParameter.iAliveTimePeriod = 1000;
			systemParameter.iImagePeriod = 1095;
		}

		/// <summary>
		/// 시스템 파라미터 객체
		/// </summary>
		/// <returns></returns>
		public SystemParameter GetSystemParameter()
		{
			return m_objSystemParameter;
		}
	}
}
