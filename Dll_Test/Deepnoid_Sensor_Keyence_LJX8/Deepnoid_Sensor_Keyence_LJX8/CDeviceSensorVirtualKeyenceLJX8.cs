using System.Collections.Generic;

namespace Deepnoid_Sensor_Keyence_LJX8
{
	public class CDeviceSensorVirtualKeyenceLJX8 : CDeviceSensorAbstract
	{
		/// <summary>
		/// 이미지 데이터
		/// </summary>
		private List<CScanData> m_lstImageData;
		/// <summary>
		/// 이미지 인덱스
		/// </summary>
		private int m_iImageIndex = 0;
		/// <summary>
		/// 가상 이미지 경로
		/// </summary>
		private string m_strImagePath;

		/// <summary>
		/// 생성자
		/// </summary>
		public CDeviceSensorVirtualKeyenceLJX8()
		{
			_callBackScanData = null;
			_callBackMessage = null;
			_callBackExceptionMessage = null;
			m_lstImageData = new List<CScanData>();
		}

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objInitializeParameter"></param>
		/// <returns></returns>
		public override bool Initialize( CInitializeParameterLJX8 objInitializeParameter )
		{
			bool bReturn = false;

			do {
				// 변수 초기화
				m_strImagePath = objInitializeParameter.strImagePath;
				InitializeImage();
				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 해제
		/// </summary>
		public override void DeInitialize()
		{
			if( null != m_lstImageData ) m_lstImageData.Clear();
		}

		/// <summary>
		/// 특정 폴더 이미지를 로딩하여 가상 모드에서 사용
		/// </summary>
		/// <returns></returns>
		private bool InitializeImage()
		{
			bool bReturn = false;

			do {

				try {

				}
				catch { }

			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 연결 상태
		/// </summary>
		/// <returns></returns>
		public override bool IsConnected()
		{
			return true;
		}

		/// <summary>
		/// 고게이터 상태 중 Connected 상태 확인.
		/// </summary>
		/// <returns></returns>
		public bool IsGogatorStatusConnected()
		{
			bool bReturn = false;
			do {

			} while( false );
			return bReturn;
		}

		/// <summary>
		/// 스캔 시작
		/// </summary>
		/// <returns></returns>
		public override bool ScanStart()
		{
			bool bReturn = false;
			do {
				if( 0 == m_lstImageData.Count ) break;
				CScanData objImageData = m_lstImageData[ m_iImageIndex ].Clone() as CScanData;
				_callBackScanData?.Invoke( objImageData.Clone() as CScanData );
				m_iImageIndex++;

				if( m_iImageIndex >= m_lstImageData.Count )
					m_iImageIndex = 0;

				bReturn = true;
			} while( false );
			return bReturn;
		}

		/// <summary>
		/// 스캔 정지
		/// </summary>
		/// <returns></returns>
		public override bool ScanStop()
		{
			bool bReturn = false;

			do {
				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 센서 리셋
		/// </summary>
		/// <param name="bWait"></param>
		/// <returns></returns>
		public override bool SensorReset( bool bWait )
		{
			bool bReturn = false;

			do {
				bReturn = true;
			} while( false );

			return bReturn;
		}
	}
}
