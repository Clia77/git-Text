using Newtonsoft.Json;
using System;
using System.IO;


namespace Dll_Test {
    public partial class CConfig {
        /// <summary>
        /// NTC 파라미터
        /// </summary>
        public class RecipeNtcParameter : ICloneable {
            /// <summary>
            /// ROI 영역
            /// NTC, PAD, CELL, ALIGN
            /// </summary>
            public int[] iRoiRectangle { get; set; }
            /// <summary>
            /// 마스터 포지션 X, Y
            /// </summary>
            public int iMasterX { get; set; }
            public int iMasterY { get; set; }
            /// <summary>
            /// 측정 타입 
            /// Low, Average, Upper
            /// </summary>
            public enumMeasureType eMeasureType { get; set; }
            /// <summary>
            /// 검사 스펙
            /// </summary>
            public structureNtcInspectionParameter objNtcInspectionSpecParameter;
            /// <summary>
            /// 스캔 거리
            /// </summary>
            public double dScanLength;
            /// <summary>
            /// 노출 값
            /// </summary>
            public double dSensorExposreTime;
            /// <summary>
            /// 트리거당 엔코더 거리값.
            /// </summary>
            public double dTriggerEncoderSpacing;
            /// <summary>
            /// 레시피 이름.
            /// </summary>
            public string strRecipeName;

            public RecipeNtcParameter()
            {
                iRoiRectangle = new int[ 16 ];
                iMasterX = 0;
                iMasterY = 0;
                eMeasureType = enumMeasureType.TYPE_AVERAGE;
                objNtcInspectionSpecParameter = new structureNtcInspectionParameter();
                objNtcInspectionSpecParameter.Init();
                dScanLength = 0.0;
                dSensorExposreTime = 0.0;
                dTriggerEncoderSpacing = 0.0;
                strRecipeName = "";
            }

            public object Clone()
            {
                RecipeNtcParameter obj = new RecipeNtcParameter();
                obj.iRoiRectangle = iRoiRectangle.Clone() as int[];
                obj.iMasterX = iMasterX;
                obj.iMasterY = iMasterY;
                obj.eMeasureType = eMeasureType;
                obj.objNtcInspectionSpecParameter = ( structureNtcInspectionParameter )objNtcInspectionSpecParameter.Clone();
                obj.dScanLength = dScanLength;
                obj.dSensorExposreTime = dSensorExposreTime;
                obj.dTriggerEncoderSpacing = dTriggerEncoderSpacing;
                obj.strRecipeName = strRecipeName;
                return obj;
            }
        }
        /// <summary>
        /// NTC 검사 파라미터
        /// </summary>
        public struct structureNtcInspectionParameter : ICloneable {
            /// <summary>
            /// Cell 에서 NTC 까지 높이
            /// </summary>
            public structureSpecValue dCellToNtcHeight;
            /// <summary>
            /// Pad 에서 NTC 까지 높이
            /// </summary>
            public structureSpecValue dPadToNtcHeight;
            /// <summary>
            /// Pad 에서 Cell 까지 거리
            /// </summary>
            public structureSpecValue dPadToCellDistance;
            /// <summary>
            /// NTC 포지션 스펙값
            /// </summary>
            public structureSpecValue dNtcPosition;

            public void Init()
            {
                this.dCellToNtcHeight = new structureSpecValue();
                this.dCellToNtcHeight.Init();
                this.dPadToNtcHeight = new structureSpecValue();
                this.dPadToNtcHeight.Init();
                this.dPadToCellDistance = new structureSpecValue();
                this.dPadToCellDistance.Init();
                this.dNtcPosition = new structureSpecValue();
                this.dNtcPosition.Init();
            }

            public object Clone()
            {
                structureNtcInspectionParameter obj = new structureNtcInspectionParameter();
                obj.dCellToNtcHeight = ( structureSpecValue )this.dCellToNtcHeight.Clone();
                obj.dPadToNtcHeight = ( structureSpecValue )this.dPadToNtcHeight.Clone();
                obj.dPadToCellDistance = ( structureSpecValue )this.dPadToCellDistance.Clone();
                obj.dNtcPosition = ( structureSpecValue )this.dNtcPosition.Clone();
                return obj;
            }
        }
        /// <summary>
        /// 파라미터 불러오기
        /// </summary>
        /// <returns></returns>
        public bool LoadNtcRecipeParameter()
        {
            try {
                string strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\Recipe_NTC.Json";

                if ( File.Exists( strPath ) ) {
                    string json = File.ReadAllText( strPath );
                    m_objNtcRecipeParameter = JsonConvert.DeserializeObject<RecipeNtcParameter>( json );
                    return true;
                }
                else {
                    strPath = m_objSystemParameter.strRecipePath + $@"{m_objSystemParameter.strCurrentRecipeID}\";
                    DirectoryInfo directoryInfo = new DirectoryInfo( strPath );
                    if ( false == directoryInfo.Exists ) {
                        Directory.CreateDirectory( strPath );
                    }

                    strPath += "Recipe_NTC.Json";
                    // 파일이 없는 경우 기본값으로 RootParameter 객체 생성 후 반환
                    RecipeNtcParameter parameter = new RecipeNtcParameter();
                    DefaultValue( out parameter );
                    SaveNtcRecipeParameter( parameter );
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
        public bool SaveNtcRecipeParameter( RecipeNtcParameter objParameter )
        {
            bool bResult = false;
            try {
                m_objNtcRecipeParameter = objParameter;
                string strPath = m_objSystemParameter.strRecipePath + $@"\{m_objSystemParameter.strCurrentRecipeID}\Recipe_NTC.Json";
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
        private void DefaultValue( out RecipeNtcParameter parameter )
        {
            parameter = new RecipeNtcParameter();
            parameter.dScanLength = 35.0;
            parameter.dSensorExposreTime = 70.0;
            parameter.dTriggerEncoderSpacing = 0.046;
            parameter.iMasterX = 100;
            parameter.iMasterY = 100;
            parameter.eMeasureType = enumMeasureType.TYPE_AVERAGE;
            parameter.objNtcInspectionSpecParameter.Init();
            parameter.objNtcInspectionSpecParameter.dPadToNtcHeight.dSpecValue = 2.5;
            parameter.objNtcInspectionSpecParameter.dPadToNtcHeight.dSpecTolerancePlus = 0.5;
            parameter.objNtcInspectionSpecParameter.dPadToNtcHeight.dSpecToleranceMinus = 0.5;
            parameter.objNtcInspectionSpecParameter.dPadToNtcHeight.dSpecOffset = 0.0;
            parameter.objNtcInspectionSpecParameter.dCellToNtcHeight.dSpecValue = 5.7;
            parameter.objNtcInspectionSpecParameter.dCellToNtcHeight.dSpecTolerancePlus = 0.0;
            parameter.objNtcInspectionSpecParameter.dCellToNtcHeight.dSpecToleranceMinus = 0.0;
            parameter.objNtcInspectionSpecParameter.dCellToNtcHeight.dSpecOffset = 0.0;
            parameter.objNtcInspectionSpecParameter.dNtcPosition.dSpecValue = -8.5;
            parameter.objNtcInspectionSpecParameter.dNtcPosition.dSpecTolerancePlus = 0.0;
            parameter.objNtcInspectionSpecParameter.dNtcPosition.dSpecToleranceMinus = 0.0;
            parameter.objNtcInspectionSpecParameter.dNtcPosition.dSpecOffset = 0.0;
            for ( int iLoopCount = 0; iLoopCount < 16; iLoopCount++ ) {
                parameter.iRoiRectangle[ iLoopCount ] = 100;
            }
        }
        /// <summary>
        /// 시스템 파라미터 객체
        /// </summary>
        /// <returns></returns>
        public RecipeNtcParameter GetNtcRecipearameter()
        {
            return m_objNtcRecipeParameter;
        }
    }
}
