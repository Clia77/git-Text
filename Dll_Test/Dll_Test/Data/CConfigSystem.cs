using Newtonsoft.Json;
using System;
using System.IO;

namespace Dll_Test {
    public partial class CConfig {
        /// <summary>
        /// 시스템 파라미터
        /// </summary>
        public class SystemParameter {
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
        /// 데이터베이스 파라미터
        /// </summary>
        public class DatabaseParameter {
            /// <summary>
            /// 데이터베이스 경로
            /// </summary>
            public string strDatabasePath { get; set; }
            /// <summary>
            /// 데이터 베이스 히스토리 이름
            /// </summary>
            public string strDatabaseHistoryName { get; set; }
            /// <summary>
            /// 데이터 베이스 인포메이션 이름
            /// </summary>
            public string strDatabaseInformationName { get; set; }
            /// <summary>
            /// 데이터 베이스 테이블 폴더 경로
            /// </summary>
            public string strDatabaseTablePath { get; set; }
            /// <summary>
            /// 데이터 베이스 레코드 폴더 경로
            /// </summary>
            public string strDatabaseRecordPath { get; set; }
            /// <summary>
            /// 데이터 베이스 인포메이션 테이블 UI text 이름
            /// </summary>
            public string strTableInformationUIText { get; set; }
            /// <summary>
            /// 데이터 베이스 인포메이션 테이블 유저메시지 text 이름
            /// </summary>
            public string strTableInformationUserMessage { get; set; }
            /// <summary>
            /// 데이터 베이스 인포메이션 레코드 UI text 이름
            /// </summary>
            public string strRecordInformationUIText { get; set; }
            /// <summary>
            /// 데이터 베이스 인포메이션 레코드 유저메시지 text 이름
            /// </summary>
            public string strRecordInformationUserMessage { get; set; }
            /// <summary>
            /// 데이터 베이스 저장 기간
            /// </summary>
            public int iDatabaseDeletePeriod { get; set; }
            /// <summary>
            /// 데이터 베이스 삭제 사용 유무
            /// </summary>
            public bool bDatabaseDelete { get; set; }
        }
        /// <summary>
        /// 시스템 파라미터 불러오기
        /// </summary>
        /// <returns></returns>
        public bool LoadSystemParameter()
        {
            try {
                string strPath = Environment.CurrentDirectory + @"\Config\Config.Json";

                if ( File.Exists( strPath ) ) {
                    string json = File.ReadAllText( strPath );
                    m_objSystemParameter = JsonConvert.DeserializeObject<SystemParameter>( json );
                    return true;
                }
                else {
                    // 파일이 없는 경우 기본값으로 RootParameter 객체 생성 후 반환
                    SystemParameter systemParameter = new SystemParameter();
                    DefaultValue( out systemParameter );
                    SaveSystemParameter( systemParameter );
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
        /// 시스템 파라미터 저장
        /// </summary>
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
        private void DefaultValue( out SystemParameter systemParameter )
        {
            systemParameter = new SystemParameter();
            // 데이터베이스
            systemParameter.objDatabaseParameter = new DatabaseParameter();
            systemParameter.objDatabaseParameter.strDatabasePath = "D:\\DeepnoidInspection\\Item\\Database\\";
            systemParameter.objDatabaseParameter.strDatabaseTablePath = "\\DatabaseTable";
            systemParameter.objDatabaseParameter.strDatabaseRecordPath = "\\DatabaseRecord";
            systemParameter.objDatabaseParameter.strDatabaseInformationName = "DATABASE_INFORMATION";
            systemParameter.objDatabaseParameter.strDatabaseHistoryName = "DATABASE_HISTORY";
            systemParameter.objDatabaseParameter.strRecordInformationUserMessage = "RECORD_INFORMATION_USER_MESSAGE";
            systemParameter.objDatabaseParameter.strTableInformationUserMessage = "TABLE_INFORMATION_USER_MESSAGE";
            systemParameter.objDatabaseParameter.strRecordInformationUIText = "RECORD_INFORMATION_UI_TEXT";
            systemParameter.objDatabaseParameter.strTableInformationUIText = "TABLE_INFORMATION_UI_TEXT";
            systemParameter.objDatabaseParameter.bDatabaseDelete = true;
            systemParameter.objDatabaseParameter.iDatabaseDeletePeriod = 1095;
            // 일반
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
