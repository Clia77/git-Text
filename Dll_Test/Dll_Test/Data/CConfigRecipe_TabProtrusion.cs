using Newtonsoft.Json;
using System;
using System.IO;


namespace Dll_Test {
    public partial class CConfig {
        /// <summary>
        /// 탭돌출 파라미터
        /// </summary>
        public class RecipeTabProtrusionParameter {
            /// <summary>
            /// 필터 사이즈
            /// </summary>
            public int iFilterSize;
            /// <summary>
            /// 마스터 지그 필터 사이즈
            /// </summary>
            public int iMasterJigFilterSize;
            /// <summary>
            /// 셀 타입
            /// </summary>
            public enumCellType eCellType;
            /// <summary>
            /// 
            /// </summary>
            public enumTotalReultCount eTotalResultCount;
            /// <summary>
            /// 측정 타입 
            /// Low, Average, Upper
            /// </summary>
            public enumMeasureType eMeasureType;
            /// <summary>
            /// 검사 위치
            /// </summary>
            public enumInspectionMode eInspectionMode;
            /// <summary>
            /// 센터 탑 검사 시작 위치
            /// </summary>
            public int iCenterTopStartPoint;
            /// <summary>
            /// 센서 버텀 검사 시작 위치
            /// </summary>
            public int iCenterBottomStartPoint;
            /// <summary>
            ///  엔드 탑 검사 시작 위치
            /// </summary>
            public int iEndTopStartPoint;
            /// <summary>
            /// 엔드 버텀 검사 시작 위치
            /// </summary>
            public int iEndBottomStartPoint;
            /// <summary>
            /// 타임아웃 
            /// </summary>
            public int iTimeout;
            /// <summary>
            /// 검사 스펙
            /// </summary>
            public structureTabProtrusionInspectionParameter objTabProtrusionInspectionSpecParameter;
            public RecipeTabProtrusionParameter()
            {
                // 필터 사이즈
                iFilterSize = 21;
                // 마스터지그 필터 사이즈
                iMasterJigFilterSize = 23;
                // 측정 타입 
                eMeasureType = enumMeasureType.TYPE_AVERAGE;
                // 셀타입
                eCellType = enumCellType.TYPE_2P;
                // 토탈 검사 갯수
                eTotalResultCount = enumTotalReultCount.TOTAL_28;
                // 검사 모드
                eInspectionMode = enumInspectionMode.INSPECTION_MODE_AUTO;
                // Center Top 데이터 시작점.
                iCenterTopStartPoint = 0;
                // Center Bottom 데이터 시작점
                iCenterBottomStartPoint = 0;
                // End Top 데이터 시작점
                iEndTopStartPoint = 0;
                // End Bottom 데이터 시작점
                iEndBottomStartPoint = 0;
                // TimeOut
                iTimeout = 0;
                // 검사 스펙
                objTabProtrusionInspectionSpecParameter = new structureTabProtrusionInspectionParameter();
                objTabProtrusionInspectionSpecParameter.Init();
            }

            public object Clone()
            {
                RecipeTabProtrusionParameter obj = new RecipeTabProtrusionParameter();
                obj.iFilterSize = iFilterSize;
                obj.iMasterJigFilterSize = iMasterJigFilterSize;
                obj.eMeasureType = eMeasureType;
                obj.eCellType = eCellType;
                obj.eTotalResultCount = eTotalResultCount;
                obj.eInspectionMode = eInspectionMode;
                obj.iCenterTopStartPoint = iCenterTopStartPoint;
                obj.iCenterBottomStartPoint = iCenterBottomStartPoint;
                obj.iEndTopStartPoint = iEndTopStartPoint;
                obj.iEndBottomStartPoint = iEndBottomStartPoint;
                obj.iTimeout = iTimeout;
                obj.objTabProtrusionInspectionSpecParameter = ( structureTabProtrusionInspectionParameter )objTabProtrusionInspectionSpecParameter.Clone();

                return obj;
            }
        }
        /// <summary>
        /// 탭돌출 검사 파라미터
        /// </summary>
        public struct structureTabProtrusionInspectionParameter : ICloneable {

            // Center Upper Limit
            public structureSpecValue dCenterUpperLimit;
            // Cneter Low Limit
            public structureSpecValue dCenterLowerLimit;
            // End Upper Limit
            public structureSpecValue dEndUpperLimit;
            // End Low Limit
            public structureSpecValue dEndLowerLimit;
            public void Init()
            {
                this.dCenterUpperLimit = new structureSpecValue();
                this.dCenterUpperLimit.Init();
                this.dCenterLowerLimit = new structureSpecValue();
                this.dCenterLowerLimit.Init();
                this.dEndUpperLimit = new structureSpecValue();
                this.dEndUpperLimit.Init();
                this.dEndLowerLimit = new structureSpecValue();
                this.dEndLowerLimit.Init();
            }

            public object Clone()
            {
                structureNtcInspectionParameter obj = new structureNtcInspectionParameter();
                obj.dCellToNtcHeight = ( structureSpecValue )this.dCenterUpperLimit.Clone();
                obj.dPadToNtcHeight = ( structureSpecValue )this.dCenterLowerLimit.Clone();
                obj.dPadToCellDistance = ( structureSpecValue )this.dEndUpperLimit.Clone();
                obj.dNtcPosition = ( structureSpecValue )this.dEndLowerLimit.Clone();
                return obj;
            }
        }
        /// <summary>
        /// 파라미터 불러오기
        /// </summary>
        /// <returns></returns>
        public bool LoadTabProtrusionRecipeParameter()
        {
            try {
                string strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\Recipe_TabProtrusion.Json";

                if ( File.Exists( strPath ) ) {
                    string json = File.ReadAllText( strPath );
                    m_objTabProtrusionRecipeParameter = JsonConvert.DeserializeObject<RecipeTabProtrusionParameter>( json );
                    return true;
                }
                else {
                    strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\";
                    DirectoryInfo directoryInfo = new DirectoryInfo( strPath );
                    if ( false == directoryInfo.Exists ) {
                        Directory.CreateDirectory( strPath );
                    }

                    strPath += "Recipe_TabProtrusion.Json";
                    // 파일이 없는 경우 기본값으로 객체 생성 후 반환
                    RecipeTabProtrusionParameter parameter = new RecipeTabProtrusionParameter();
                    DefaultValue( out parameter );
                    SaveTabProtrusionRecipeParameter( parameter );
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
        public bool SaveTabProtrusionRecipeParameter( RecipeTabProtrusionParameter objParameter )
        {
            bool bResult = false;
            try {
                m_objTabProtrusionRecipeParameter = objParameter;
                string strPath = m_objSystemParameter.strRecipePath + $@"\{m_objSystemParameter.strCurrentRecipeID}\Recipe_TabProtrusion.Json";
                string json = JsonConvert.SerializeObject( m_objTabProtrusionRecipeParameter, Formatting.Indented );
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
        private void DefaultValue( out RecipeTabProtrusionParameter parameter )
        {
            parameter = new RecipeTabProtrusionParameter();
            parameter.objTabProtrusionInspectionSpecParameter.Init();
            parameter.objTabProtrusionInspectionSpecParameter.dCenterUpperLimit.dSpecValue = 1.7;
            parameter.objTabProtrusionInspectionSpecParameter.dCenterUpperLimit.dSpecTolerancePlus = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dCenterUpperLimit.dSpecToleranceMinus = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dCenterUpperLimit.dSpecOffset = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dCenterLowerLimit.dSpecValue = 0.5;
            parameter.objTabProtrusionInspectionSpecParameter.dCenterLowerLimit.dSpecTolerancePlus = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dCenterLowerLimit.dSpecToleranceMinus = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dCenterLowerLimit.dSpecOffset = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dEndUpperLimit.dSpecValue = 1.7;
            parameter.objTabProtrusionInspectionSpecParameter.dEndUpperLimit.dSpecTolerancePlus = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dEndUpperLimit.dSpecToleranceMinus = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dEndUpperLimit.dSpecOffset = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dEndLowerLimit.dSpecValue = 0.5;
            parameter.objTabProtrusionInspectionSpecParameter.dEndLowerLimit.dSpecTolerancePlus = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dEndLowerLimit.dSpecToleranceMinus = 0.0;
            parameter.objTabProtrusionInspectionSpecParameter.dEndLowerLimit.dSpecOffset = 0.0;
            parameter.iEndTopStartPoint = 0;
            parameter.iCenterTopStartPoint = 0;
            parameter.iCenterBottomStartPoint = 0;
            parameter.iEndBottomStartPoint = 0;
            parameter.eCellType = enumCellType.TYPE_2P;
            parameter.eInspectionMode = enumInspectionMode.INSPECTION_MODE_AUTO;
            parameter.eMeasureType = enumMeasureType.TYPE_AVERAGE;
            parameter.eTotalResultCount = enumTotalReultCount.TOTAL_28;
            parameter.iFilterSize = 21;
            parameter.iMasterJigFilterSize = 23;
            parameter.iTimeout = 20000;
        }
        /// <summary>
        /// 파라미터 객체
        /// </summary>
        /// <returns></returns>
        public RecipeTabProtrusionParameter GetTabProtrusionRecipearameter()
        {
            return m_objTabProtrusionRecipeParameter;
        }
    }
}
