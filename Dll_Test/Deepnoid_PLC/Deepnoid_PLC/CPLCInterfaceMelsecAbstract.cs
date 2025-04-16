using Deepnoid_Communication;

namespace Deepnoid_PLC
{
	public abstract class CPLCInterfaceMelsecAbstract
	{
		protected object m_objLock;

		/// <summary>
		/// 수신 데이터 콜백 처리
		/// </summary>
		/// <param name="objReceiveData"></param>
		public delegate void CallBackReceiveData( CReceiveData obj );
		protected CallBackReceiveData _callBackReceiveData;
		public void SetCallBackReceiveData( CallBackReceiveData callBack )
		{
			_callBackReceiveData = callBack;
		}

		/// <summary>
		/// Error Message 콜백 처리
		/// </summary>
		/// <param name="strErrorMessage"></param>
		public delegate void CallBackErrorMessage( string strErrorMessage );
		protected CallBackErrorMessage _callBackErrorMessage;
		public void SetCallBackErrorMessage( CallBackErrorMessage callBack )
		{
			_callBackErrorMessage = callBack;
		}

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public abstract bool Initialize( CPLCInterfaceMelsecParameter objParameter );

		/// <summary>
		/// 해제
		/// </summary>
		public abstract void DeInitialize();

		/// <summary>
		/// 접속 유무
		/// </summary>
		/// <returns></returns>
		public abstract bool IsConnected();

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public abstract bool ReadBit( string strAddress, ref bool bData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public abstract bool ReadBit( string strAddress, ref bool[] bData );

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public abstract bool WriteBit( string strAddress, bool bData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public abstract bool WriteBit( string strAddress, bool[] bData );

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public abstract bool ReadWord( string strAddress, ref int iData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public abstract bool ReadWord( string strAddress, ref int[] iData );

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public abstract bool WriteWord( string strAddress, int iData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public abstract bool WriteWord( string strAddress, int[] iData );

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public abstract bool ReadDoubleWord( string strAddress, ref double dData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public abstract bool ReadDoubleWord( string strAddress, ref double[] dData );

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public abstract bool WriteDoubleWord( string strAddress, double dData );

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public abstract bool WriteDoubleWord( string strAddress, double[] dData );
	}
}