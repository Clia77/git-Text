using System;

namespace Data
{
	public partial class CConfig
	{
		public enum enumUserAuthorityLevel
		{
			USER_AUTHORITY_LEVEL_OPERATOR = 0,
			USER_AUTHORITY_LEVEL_ENGINEER,
			USER_AUTHORITY_LEVEL_MASTER,
		}
		/// <summary>
		/// 설비 타입
		/// </summary>
		public enum enumEquipmentType
		{
			TYPE_FLATNESS = 0,
			TYPE_TA,
			TYPE_TAB_PROTRUSION_1,
			TYPE_TAB_PROTRUSION_2,
			TYPE_TAB_PROTRUSION_3,
			TYPE_NTC,
			TYPE_MONITORING,
		}
		/// <summary>
		/// 언어
		/// </summary>
		public enum enumLanguage
		{
			LANGUAGE_KOREA = 0,
			LANGUAGE_ENGLISH,
			LANGUAGE_SPAIN,
		}
		/// <summary>
		/// 시뮬레이션 모드
		/// </summary>
		public enum enumSimulationMode
		{
			MODE_SIMULATION_OFF = 0,
			MODE_SIMULATION_ON,
		}
		/// <summary>
		/// 바이패스 모드
		/// </summary>
		public enum enumBypassMode
		{
			MODE_BYPASS_OFF = 0,
			MODE_BYPASS_ON,
		}
		/// <summary>
		/// 측정 타입
		/// </summary>
		public enum enumMeasureType { TYPE_LOW = 0, TYPE_AVERAGE, TYPE_UPPER };
		/// <summary>
		/// 셀 타입
		/// </summary>
		public enum enumCellType { TYPE_2P = 0, TYPE_3P }
		/// <summary>
		/// 검사 위치
		/// </summary>
		public enum enumInspectPosition { POSITION_TOP = 0, POSITION_BOTTOM }
		/// <summary>
		/// 총 결과 갯수
		/// </summary>
		public enum enumTotalReultCount { TOTAL_28 = 28, TOTAL_36 = 36 };
		/// <summary>
		/// 검사 모드
		/// </summary>
		public enum enumInspectionMode { INSPECTION_MODE_JIG, INSPECTION_MODE_MANUAL, INSPECTION_MODE_AUTO }

		/// <summary>
		/// 스펙 값 구성 ( 값 / 허용값 / 옵셋 )
		/// </summary>
		public class CSpecValue : ICloneable
		{
			/// <summary>
			/// 검사 스펙
			/// </summary>
			public double dSpecValue;
			/// <summary>
			/// 검사 스펙 허용 범위 +
			/// </summary>
			public double dSpecTolerancePlus;
			/// <summary>
			/// 검사 스펙 허용 범위 -
			/// </summary>
			public double dSpecToleranceMinus;
			/// <summary>
			/// 검사 스펙 옵셋
			/// </summary>
			public double dSpecOffset;

			public CSpecValue()
			{
				this.dSpecValue = 0.0;
				this.dSpecTolerancePlus = 0.0;
				this.dSpecToleranceMinus = 0.0;
				this.dSpecOffset = 0.0;
			}

			public object Clone()
			{
				CSpecValue obj = new CSpecValue();
				obj.dSpecValue = this.dSpecValue;
				obj.dSpecTolerancePlus = this.dSpecTolerancePlus;
				obj.dSpecToleranceMinus = this.dSpecToleranceMinus;
				obj.dSpecOffset = this.dSpecOffset;
				return obj;
			}
		}
	}
}
