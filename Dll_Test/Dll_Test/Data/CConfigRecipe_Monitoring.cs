using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Data
{
	public partial class CConfig
	{
		/// <summary>
		/// 모니터링 파라미터
		/// </summary>
		public class RecipeMonitoringParameter
		{
			/// <summary>
			/// 검사 스펙
			/// </summary>
			public structureMonitoringInspectionParameter objMonitoringInspectionSpecParameter;

			public RecipeMonitoringParameter()
			{
				objMonitoringInspectionSpecParameter = new structureMonitoringInspectionParameter();
				objMonitoringInspectionSpecParameter.Init();
			}

			public object Clone()
			{
				RecipeMonitoringParameter obj = new RecipeMonitoringParameter();
				obj.objMonitoringInspectionSpecParameter = ( structureMonitoringInspectionParameter )objMonitoringInspectionSpecParameter.Clone();
				return obj;
			}
		}

		/// <summary>
		/// Monitoring 검사 파라미터
		/// </summary>
		public struct structureMonitoringInspectionParameter : ICloneable
		{

			public void Init()
			{

			}

			public object Clone()
			{
				structureMonitoringInspectionParameter obj = new structureMonitoringInspectionParameter();

				return obj;
			}
		}

		/// <summary>
		/// 파라미터 불러오기
		/// </summary>
		/// <returns></returns>
		public bool LoadMonitoringRecipeParameter()
		{
			try {
				string strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\Recipe_Monitoring.Json";

				if( File.Exists( strPath ) ) {
					string json = File.ReadAllText( strPath );
					m_objMonitoringRecipeParameter = JsonConvert.DeserializeObject<RecipeMonitoringParameter>( json );
					return true;
				} else {
					strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\";
					DirectoryInfo directoryInfo = new DirectoryInfo( strPath );
					if( false == directoryInfo.Exists ) {
						Directory.CreateDirectory( strPath );
					}

					strPath += "Recipe_Monitoring.Json";
					// 파일이 없는 경우 기본값으로 RootParameter 객체 생성 후 반환
					RecipeMonitoringParameter parameter = new RecipeMonitoringParameter();
					DefaultValue( out parameter );
					SaveMonitoringRecipeParameter( parameter );
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
		/// 파라미터 저장
		/// </summary>
		/// <returns></returns>
		public bool SaveMonitoringRecipeParameter( RecipeMonitoringParameter objParameter )
		{
			bool bResult = false;
			try {
				m_objMonitoringRecipeParameter = objParameter;
				string strPath = m_objSystemParameter.strRecipePath + $@"\{m_objSystemParameter.strCurrentRecipeID}\Recipe_Monitoring.Json";
				string json = JsonConvert.SerializeObject( m_objMonitoringRecipeParameter, Formatting.Indented );
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
		private void DefaultValue( out RecipeMonitoringParameter parameter )
		{
			parameter = new RecipeMonitoringParameter();
		}

		/// <summary>
		/// 파라미터 객체
		/// </summary>
		/// <returns></returns>
		public RecipeMonitoringParameter GetMonitoringRecipearameter()
		{
			return m_objMonitoringRecipeParameter;
		}
	}
}
