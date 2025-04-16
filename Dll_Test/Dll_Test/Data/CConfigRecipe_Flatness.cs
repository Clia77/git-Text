using Newtonsoft.Json;
using System;
using System.IO;

namespace Dll_Test {
    public partial class CConfig {
        /// <summary>
        /// 하부평탄 파라미터
        /// </summary>
        public class RecipeFlatnessParameter {
            /// <summary>
            /// 검사 스펙
            /// </summary>
            public structureFlatnessInspectionParameter objFlatnessInspectionSpecParameter;
            public RecipeFlatnessParameter()
            {
                objFlatnessInspectionSpecParameter = new structureFlatnessInspectionParameter();
                objFlatnessInspectionSpecParameter.Init();
            }

            public object Clone()
            {
                RecipeFlatnessParameter obj = new RecipeFlatnessParameter();
                obj.objFlatnessInspectionSpecParameter = ( structureFlatnessInspectionParameter )objFlatnessInspectionSpecParameter.Clone();
                return obj;
            }
        }
        /// <summary>
        /// Flatness 검사 파라미터
        /// </summary>
        public struct structureFlatnessInspectionParameter : ICloneable {


            public void Init()
            {

            }

            public object Clone()
            {
                structureNtcInspectionParameter obj = new structureNtcInspectionParameter();

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

                if ( File.Exists( strPath ) ) {
                    string json = File.ReadAllText( strPath );
                    m_objFlatnessRecipeParameter = JsonConvert.DeserializeObject<RecipeFlatnessParameter>( json );
                    return true;
                }
                else {
                    strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\";
                    DirectoryInfo directoryInfo = new DirectoryInfo( strPath );
                    if ( false == directoryInfo.Exists ) {
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
            catch ( JsonException ex ) {
                // JSON 파싱 오류 처리
                Console.WriteLine( $"JSON 파싱 오류: {ex.Message}" );
                return false; // 또는 예외를 던지거나 기본값 객체를 반환
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"파일 로드 오류: {ex.Message}" );
                return false;
            }
        }
        /// <summary>
        /// 파라미터 저장
        /// </summary>
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
            catch ( Exception ex ) {
                // 파일 저장 오류 처리
                Console.WriteLine( $"파일 저장 오류: {ex.Message}" );
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
