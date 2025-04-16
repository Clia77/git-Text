using System.Collections.Generic;

namespace Dll_Test {
    public partial class CConfig {

        /// <summary>
        /// System + Database 파라미터
        /// </summary>
        public SystemParameter m_objSystemParameter { get; set; }
        /// <summary>
        /// 센서 파라미터
        /// </summary>
        public SensorParameter m_objSensorParameter { get; set; }

        public RecipeNtcParameter m_objNtcRecipeParameter { get; set; }

        public RecipeTabProtrusionParameter m_objTabProtrusionRecipeParameter { get; set; }

        public RecipeTaParameter m_objTaRecipeParameter { get; set; }

        public RecipeFlatnessParameter m_objFlatnessRecipeParameter { get; set; }

        public RecipeMonitoringParameter m_objMonitoringRecipeParameter { get; set; }

        public PLCParameter m_objPLCParameter { get; set; }
        /// <summary>
        /// 생성자
        /// </summary>
        private CConfig()
        {

        }

        private static class Nested {
            internal static readonly CConfig _instance = new CConfig();
        }

        public static CConfig Instance
        {
            get {
                return Nested._instance;
            }
        }

       
        /// <summary>
        /// 초기화
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            // 환경 설정 파라미터
            m_objSystemParameter = new SystemParameter();
            m_objSystemParameter.objDatabaseParameter = new DatabaseParameter();
            m_objSensorParameter = new SensorParameter();
            m_objSensorParameter.objSensors = new List<SensorData>();
            m_objNtcRecipeParameter = new RecipeNtcParameter();
            m_objTabProtrusionRecipeParameter = new RecipeTabProtrusionParameter();
            m_objTaRecipeParameter = new RecipeTaParameter();
            m_objFlatnessRecipeParameter = new RecipeFlatnessParameter();
            m_objMonitoringRecipeParameter = new RecipeMonitoringParameter();
            m_objPLCParameter = new PLCParameter();
            return true;
        }
        /// <summary>
        /// 해제
        /// </summary>
        public void DeInitialize()
        {

        }
        /// <summary>
        /// 전체 파라미터 불러오기
        /// </summary>
        /// <returns></returns>
        public bool LoadParameter()
        {
            LoadSystemParameter();
            LoadSensorParameter();

            switch( m_objSystemParameter.eEquipmentType ) {
                case enumEquipmentType.TYPE_TA:
                    LoadTaRecipeParameter();
                    break;
                case enumEquipmentType.TYPE_FLATNESS:
                    LoadFlatnessRecipeParameter();
                    break;
                case enumEquipmentType.TYPE_NTC:
                    LoadNtcRecipeParameter();
                    break;
                case enumEquipmentType.TYPE_TAB_PROTRUSION_1:
                case enumEquipmentType.TYPE_TAB_PROTRUSION_2:
                case enumEquipmentType.TYPE_TAB_PROTRUSION_3:
                    LoadTabProtrusionRecipeParameter();
                    break;
                case enumEquipmentType.TYPE_MONITORING:
                    LoadMonitoringRecipeParameter();
                    break;
            }
            return true; 
        }
        public bool SaveParameter()
        {
            SaveSystemParameter( m_objSystemParameter );
            SaveSensorParameter( m_objSensorParameter );

            switch ( m_objSystemParameter.eEquipmentType ) {
                case enumEquipmentType.TYPE_TA:
                    SaveTaRecipeParameter( m_objTaRecipeParameter );
                    break;
                case enumEquipmentType.TYPE_FLATNESS:
                    SaveFlatnessRecipeParameter( m_objFlatnessRecipeParameter );
                    break;
                case enumEquipmentType.TYPE_NTC:
                    SaveNtcRecipeParameter( m_objNtcRecipeParameter );
                    break;
                case enumEquipmentType.TYPE_TAB_PROTRUSION_1:
                case enumEquipmentType.TYPE_TAB_PROTRUSION_2:
                case enumEquipmentType.TYPE_TAB_PROTRUSION_3:
                    SaveTabProtrusionRecipeParameter(m_objTabProtrusionRecipeParameter);
                    break;
                case enumEquipmentType.TYPE_MONITORING:
                    SaveMonitoringRecipeParameter( m_objMonitoringRecipeParameter);
                    break;
            }

            return true;
        }
    }
}
