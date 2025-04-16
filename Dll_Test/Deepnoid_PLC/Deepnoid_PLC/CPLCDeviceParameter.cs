using System;
using System.Collections.Generic;
using static Deepnoid_PLC.CPLCDefine;

namespace Deepnoid_PLC
{
	public class CPLCDeviceParameter : ICloneable
	{
		/// <summary>
		/// PLC 인터페이스에 필요한 초기화 데이터
		/// </summary>
		public CPLCInterfaceMelsecParameter objParameter;
		/// <summary>
		/// 연결할 맵 데이터 전체 경로
		/// </summary>
		public string strMapDataPath;

		public CPLCDeviceParameter()
		{
			// 추상 클래스라 바깥에서 설정해줘야 함
			objParameter = null;
			strMapDataPath = "";
		}

		public object Clone()
		{
			CPLCDeviceParameter obj = new CPLCDeviceParameter();

			obj.objParameter = ( CPLCInterfaceMelsecParameter )this.objParameter.Clone();
			obj.strMapDataPath = this.strMapDataPath;

			return obj;
		}
	}

	public class CPLCDeviceMonitoringParameter : ICloneable
	{
		/// <summary>
		/// 모니터링 리스트
		/// </summary>
		public List<CPLCDeviceMonitoringParameterList> objParameterList;
		/// <summary>
		/// 스레드 기간
		/// </summary>
		public int iThreadPeriod;

		public CPLCDeviceMonitoringParameter()
		{
			objParameterList = new List<CPLCDeviceMonitoringParameterList>();
			iThreadPeriod = 50;
		}

		public object Clone()
		{
			CPLCDeviceMonitoringParameter obj = new CPLCDeviceMonitoringParameter();

			foreach( var item in objParameterList ) {
				obj.objParameterList.Add( ( CPLCDeviceMonitoringParameterList )item.Clone() );
			}
			obj.iThreadPeriod = this.iThreadPeriod;

			return obj;
		}
	}

	public class CPLCDeviceMonitoringParameterList : ICloneable
	{
		/// <summary>
		/// read or write type
		/// </summary>
		public enumPLCDeviceRWType eRWType;
		/// <summary>
		/// 모니터링 할 start address 이름
		/// </summary>
		public string strName;
		/// <summary>
		/// 모니터링 할 사이즈
		/// </summary>
		public int iCount;

		public CPLCDeviceMonitoringParameterList()
		{
			eRWType = enumPLCDeviceRWType.READ_WORD;
			strName = "";
			iCount = 0;
		}

		public object Clone()
		{
			CPLCDeviceMonitoringParameterList obj = new CPLCDeviceMonitoringParameterList();

			obj.eRWType = this.eRWType;
			obj.strName = this.strName;
			obj.iCount = this.iCount;

			return obj;
		}
	}
}