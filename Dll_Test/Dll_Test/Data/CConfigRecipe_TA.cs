using Newtonsoft.Json;
using System;
using System.IO;


namespace Dll_Test {
    public partial class CConfig {
        /// <summary>
        /// TA 파라미터
        /// </summary>
        public class RecipeTaParameter {
            structureTaInspectionParameter objTaInspectionParameter;
            public RecipeTaParameter()
            {
                objTaInspectionParameter = new structureTaInspectionParameter();
                objTaInspectionParameter.Init();
            }

            public object Clone()
            {
                RecipeTaParameter obj = new RecipeTaParameter();
                obj.objTaInspectionParameter = ( structureTaInspectionParameter )objTaInspectionParameter.Clone();
                return obj;
            }
        }
        /// <summary>
        /// TA 검사 파라미터
        /// </summary>
        public struct structureTaInspectionParameter : ICloneable {


            public void Init()
            {

            }

            public object Clone()
            {
                structureTaInspectionParameter obj = new structureTaInspectionParameter();

                return obj;
            }
        }

        /// <summary>
        /// 파라미터 불러오기
        /// </summary>
        /// <returns></returns>
        public bool LoadTaRecipeParameter()
        {
            try {
                string strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\Recipe_TA.Json";

                if ( File.Exists( strPath ) ) {
                    string json = File.ReadAllText( strPath );
                    m_objTaRecipeParameter = JsonConvert.DeserializeObject<RecipeTaParameter>( json );
                    return true;
                }
                else {
                    strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\";
                    DirectoryInfo directoryInfo = new DirectoryInfo( strPath );
                    if ( false == directoryInfo.Exists ) {
                        Directory.CreateDirectory( strPath );
                    }

                    strPath += "Recipe_TA.Json";
                    // 파일이 없는 경우 기본값으로 RootParameter 객체 생성 후 반환
                    RecipeTaParameter parameter = new RecipeTaParameter();
                    DefaultValue( out parameter );
                    SaveTaRecipeParameter( parameter );
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
        public bool SaveTaRecipeParameter( RecipeTaParameter objParameter )
        {
            bool bResult = false;
            try {
                m_objTaRecipeParameter = objParameter;
                string strPath = m_objSystemParameter.strRecipePath + $@"\{m_objSystemParameter.strCurrentRecipeID}\Recipe_TA.Json";
                string json = JsonConvert.SerializeObject( m_objNtcRecipeParameter, Formatting.Indented );
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
        private void DefaultValue( out RecipeTaParameter parameter )
        {
            parameter = new RecipeTaParameter();
        }
        /// <summary>
        /// 파라미터 객체
        /// </summary>
        /// <returns></returns>
        public RecipeTaParameter GetTaRecipearameter()
        {
            return m_objTaRecipeParameter;
        }
    }
}
