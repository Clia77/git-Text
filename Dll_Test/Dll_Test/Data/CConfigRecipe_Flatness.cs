using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Data
{
	public partial class CConfig
	{
		/// <summary>
		/// 하부평탄 파라미터
		/// </summary>
		public class RecipeFlatnessParameter
		{


			public RecipeFlatnessParameter()
			{

			}

			public object Clone()
			{
				RecipeFlatnessParameter obj = new RecipeFlatnessParameter();

				return obj;
			}
		}

		/// <summary>
		/// 파라미터 불러오기
		/// </summary>
		/// <returns></returns>
		public bool LoadFlatnessRecipeParameter()
		{
			try {
				string strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\Recipe_Flatness.Json";

				if( File.Exists( strPath ) ) {
					string json = File.ReadAllText( strPath );
					m_objFlatnessRecipeParameter = JsonConvert.DeserializeObject<RecipeFlatnessParameter>( json );
					return true;
				} else {
					strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\";
					DirectoryInfo directoryInfo = new DirectoryInfo( strPath );
					if( false == directoryInfo.Exists ) {
						Directory.CreateDirectory( strPath );
					}

					strPath += "Recipe_Flatness.Json";
					// 파일이 없는 경우 기본값으로 RootParameter 객체 생성 후 반환
					RecipeFlatnessParameter parameter = new RecipeFlatnessParameter();
					DefaultValue( out parameter );
					SaveFlatnessRecipeParameter( parameter );
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
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public bool SaveFlatnessRecipeParameter( RecipeFlatnessParameter objParameter )
		{
			bool bResult = false;
			try {
				m_objFlatnessRecipeParameter = objParameter;
				string strPath = m_objSystemParameter.strRecipePath + $@"\{m_objSystemParameter.strCurrentRecipeID}\Recipe_Flatness.Json";
				string json = JsonConvert.SerializeObject( m_objFlatnessRecipeParameter, Formatting.Indented );
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
		private void DefaultValue( out RecipeFlatnessParameter parameter )
		{
			parameter = new RecipeFlatnessParameter();
		}

		/// <summary>
		/// 파라미터 객체
		/// </summary>
		/// <returns></returns>
		public RecipeFlatnessParameter GetFlatnessRecipearameter()
		{
			return m_objFlatnessRecipeParameter;
		}
	}
}
