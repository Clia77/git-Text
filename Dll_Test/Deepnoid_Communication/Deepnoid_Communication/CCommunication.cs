namespace Deepnoid_Communication
{
	public class CCommunication
	{
		private CCommunicationAbstract m_objAbstract;
		
		/// <summary>
		/// 수신 데이터 콜백 처리
		/// </summary>
		/// <param name="objReceiveData"></param>
		public delegate void CallBackReceiveData( CReceiveData obj );
		private CallBackReceiveData _callBackReceiveData;
		public void SetCallBackReceiveData( CallBackReceiveData callBack )
		{
			_callBackReceiveData = callBack;
		}

		/// <summary>
		/// Error Message 콜백 처리
		/// </summary>
		/// <param name="strErrorMessage"></param>
		public delegate void CallBackErrorMessage( string strErrorMessage );
		private CallBackErrorMessage _callBackErrorMessage;
		public void SetCallBackErrorMessage( CallBackErrorMessage callBack )
		{
			_callBackErrorMessage = callBack;
		}

		public CCommunication( CCommunicationAbstract objAbstract )
		{
			m_objAbstract = objAbstract;
		}

		private void ReceiveData( CReceiveData obj )
		{
			_callBackReceiveData?.Invoke( obj );
		}

		private void ErrorMessage( string strErrorMessage )
		{
			_callBackErrorMessage?.Invoke( strErrorMessage );
		}

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public bool Initialize( CCommunicationParameter objParameter )
		{
			m_objAbstract?.SetCallBackReceiveData( ReceiveData );
			m_objAbstract?.SetCallBackErrorMessage( ErrorMessage );
			if( false == m_objAbstract?.Initialize( objParameter ) ) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// 해제
		/// </summary>
		public void DeInitialize()
		{
			m_objAbstract?.DeInitialize();
		}

		/// <summary>
		/// 접속 유무
		/// </summary>
		/// <returns></returns>
		public bool IsConnected()
		{
			return ( m_objAbstract?.IsConnected() ).GetValueOrDefault();
		}

		/// <summary>
		/// 접속
		/// </summary>
		public void Connect()
		{
			m_objAbstract?.Connect();
		}

		/// <summary>
		/// 해제
		/// </summary>
		public void Disconnect()
		{
			m_objAbstract?.Disconnect();
		}

		/// <summary>
		/// 데이터 send
		/// </summary>
		/// <param name="strData"></param>
		/// <returns></returns>
		public bool Send( string strData )
		{
			return ( m_objAbstract?.Send( strData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 데이터 send
		/// </summary>
		/// <param name="byteData"></param>
		/// <returns></returns>
		public bool Send( byte[] byteData )
		{
			return ( m_objAbstract?.Send( byteData ) ).GetValueOrDefault();
		}
	}
}