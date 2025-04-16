using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Deepnoid_Communication
{
	public class CCommunicationSocketClient : CCommunicationAbstract
	{
		private CCommunicationParameterSocket m_objParameterSocket;
		private bool m_bSocketClose;
		private Socket m_objSocket;

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public override bool Initialize( CCommunicationParameter objParameter )
		{
			bool bReturn = false;

			do {
				m_bConnected = false;
				m_bSocketClose = false;
				m_bThreadExit = false;
				m_objLock = new object();
				m_byteReceivedData = new byte[ CCommunicationDefine.DEF_BYTE_LENGTH ];
				m_objSocket = null;

				if( typeof( CCommunicationParameterSocket ) != objParameter.GetParameter().GetType() ) {
					string strError = $"Fail to initialize socket type unmatch - Type : {objParameter.GetParameter().GetType().ToString()}";
					_callBackErrorMessage?.Invoke( strError );
					break;
				}
				m_objParameterSocket = ( CCommunicationParameterSocket )objParameter.GetParameter().Clone();

				m_threadConnect = new Thread( ThreadConnect );
				m_threadConnect.Start( this );

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 해제
		/// </summary>
		public override void DeInitialize()
		{
			m_bThreadExit = true;
			m_threadConnect?.Join();
			Disconnect();
		}

		/// <summary>
		/// 연결 상태 확인
		/// </summary>
		/// <returns></returns>
		public override bool IsConnected()
		{
			return m_bConnected;
		}

		/// <summary>
		/// 내부 소켓 연결 상태 확인
		/// </summary>
		private void CheckConnect()
		{
			try {
				if( null == m_objSocket ) {
					m_bConnected = false;
					return;
				}
				IPGlobalProperties objproperties = IPGlobalProperties.GetIPGlobalProperties();
				// 현재 연결된 클라이언트 및 서버 IP 매칭
				TcpConnectionInformation[] objTcpConnection =
					objproperties.GetActiveTcpConnections().Where( ip => ip.LocalEndPoint.Equals( m_objSocket.LocalEndPoint ) && ip.RemoteEndPoint.Equals( m_objSocket.RemoteEndPoint ) ).ToArray();

				// 현재 연결 상태 확인
				if( null != objTcpConnection && 0 < objTcpConnection.Length ) {
					TcpState stateConnection = objTcpConnection.First().State;
					if( TcpState.Established == stateConnection ) {
						m_bConnected = true;
					} else {
						m_bConnected = false;
					}
				} else {
					m_bConnected = false;
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );

				// 연결 끊어버림
				Disconnect();
			}
		}

		/// <summary>
		/// 소켓 연결
		/// </summary>
		public override void Connect()
		{
			try {
				if( null == m_objParameterSocket ) {
					return;
				}
				IPAddress ipServer = IPAddress.Parse( m_objParameterSocket.strSocketIPAddress );
				m_objSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
				m_objSocket.NoDelay = true;
				m_objSocket.Connect( new IPEndPoint( ipServer, m_objParameterSocket.iSocketPortNumber ) );
				m_objSocket.BeginReceive( m_byteReceivedData, 0, m_byteReceivedData.Length, SocketFlags.None, new AsyncCallback( OnReceived ), null );
				m_bSocketClose = false;
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
		}

		/// <summary>
		/// 소켓 해제
		/// </summary>
		public override void Disconnect()
		{
			try {
				m_bSocketClose = true;
				m_objSocket?.Close();
				m_objSocket = null;
				m_bConnected = false;
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
		}

		/// <summary>
		/// 데이터 전송
		/// </summary>
		/// <param name="strData"></param>
		/// <returns></returns>
		public override bool Send( string strData )
		{
			bool bReturn = false;

			do {
				if( null == m_objSocket ) {
					break;
				}
				if( false == m_objSocket.Connected ) {
					break;
				}
				if( false == m_bConnected ) {
					break;
				}

				lock( m_objLock ) {
					try {
						byte[] byteData = Encoding.Default.GetBytes( strData );

						// 데이터 인코딩 타입에 따른 전송
						Encoding enc;
						switch( m_objParameterSocket.eDataEncoding ) {
							case CCommunicationDefine.enumDataEncoding.ENCODING_NONE:
								enc = Encoding.Default;
								break;
							case CCommunicationDefine.enumDataEncoding.ENCODING_DEFAULT:
								enc = Encoding.Default;
								break;
							case CCommunicationDefine.enumDataEncoding.ENCODING_UCS2:
								enc = Encoding.GetEncoding( "ucs-2" );
								break;
							default:
								enc = Encoding.Default;
								break;
						}

						byte[] byteSendData = Encoding.Convert( Encoding.Default, enc, byteData );
						m_objSocket.BeginSend( byteSendData, 0, byteSendData.Length, SocketFlags.None, new AsyncCallback( OnSend ), byteSendData );
					}
					catch( Exception ex ) {
						string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
						string strMethodName = MethodBase.GetCurrentMethod()?.Name;
						string strException = $"{strClassName} {strMethodName} : {ex.Message}";
						_callBackErrorMessage?.Invoke( strException );
						break;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 데이터 전송
		/// </summary>
		/// <param name="byteData"></param>
		/// <returns></returns>
		public override bool Send( byte[] byteData )
		{
			bool bReturn = false;

			do {
				if( null == m_objSocket ) {
					break;
				}
				if( false == m_objSocket.Connected ) {
					break;
				}
				if( false == m_bConnected ) {
					break;
				}

				lock( m_objLock ) {
					try {
						m_objSocket.BeginSend( byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback( OnSend ), byteData );
					}
					catch( Exception ex ) {
						string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
						string strMethodName = MethodBase.GetCurrentMethod()?.Name;
						string strException = $"{strClassName} {strMethodName} : {ex.Message}";
						_callBackErrorMessage?.Invoke( strException );
						break;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 재접속
		/// </summary>
		/// <param name="obj"></param>
		private static void ThreadConnect( object obj )
		{
			CCommunicationSocketClient pThis = ( CCommunicationSocketClient )obj;
			TimeSpan objTimeSpan = new TimeSpan( 0, 0, 1 );
			double dMilliseconds = objTimeSpan.TotalMilliseconds;
			int iThreadPeriod = 100;

			while( false == pThis.m_bThreadExit ) {
				if( 0.0 >= dMilliseconds ) {
					if( null != pThis.m_objSocket ) {
						// 소켓 연결 상태 확인
						pThis.CheckConnect();
						// 끊어 졌을 경우 다시 연결
						if( false == pThis.m_bConnected ) {
							// 기존 socket 소멸
							pThis.Disconnect();
						}
					} else {
						// socket 생성 및 재연결
						pThis.Connect();
					}
					// 초기화
					dMilliseconds = objTimeSpan.TotalMilliseconds;
				}
				dMilliseconds -= ( double )iThreadPeriod;
				Thread.Sleep( iThreadPeriod );
			}
		}

		/// <summary>
		/// Send 결과 대리자
		/// </summary>
		/// <param name="obj"></param>
		private void OnSend( IAsyncResult obj )
		{
			byte[] byteSendData = ( byte[] )obj.AsyncState;

			try {
				if( null == m_objSocket ) {
					return;
				}
				int iLength = m_objSocket.EndSend( obj );
				if( 0 == iLength ) {
					Disconnect();
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );

				Disconnect();
			}
		}

		/// <summary>
		/// Receive 결과 대리자
		/// </summary>
		/// <param name="obj"></param>
		private void OnReceived( IAsyncResult obj )
		{
			bool bReceiveData = true;

			try {
				// ObjectDisposedException 예외 처리
				if( false == m_bSocketClose && true == m_objSocket?.Connected ) {
					int iReceivedByteCount = m_objSocket.EndReceive( obj );
					if( 0 >= iReceivedByteCount ) {
						Disconnect();
						return;
					}

					string strReceivedData = "";
					byte[] byteReceiveData;
					Encoding enc;
					switch( m_objParameterSocket.eDataEncoding ) {
						case CCommunicationDefine.enumDataEncoding.ENCODING_NONE:
							enc = Encoding.Default;
							byteReceiveData = m_byteReceivedData.ToArray();
							break;
						case CCommunicationDefine.enumDataEncoding.ENCODING_DEFAULT:
							enc = Encoding.Default;
							byteReceiveData = Encoding.Convert( enc, Encoding.Default, m_byteReceivedData, 0, iReceivedByteCount );
							break;
						case CCommunicationDefine.enumDataEncoding.ENCODING_UCS2:
							enc = Encoding.GetEncoding( "ucs-2" );
							byteReceiveData = Encoding.Convert( enc, Encoding.Default, m_byteReceivedData, 0, iReceivedByteCount );
							break;
						default:
							enc = Encoding.Default;
							byteReceiveData = m_byteReceivedData.ToArray();
							break;
					}

					try {
						strReceivedData = Encoding.Default.GetString( byteReceiveData, 0, iReceivedByteCount );
					}
					catch( DecoderFallbackException ) {
						strReceivedData = Encoding.UTF8.GetString( byteReceiveData, 0, iReceivedByteCount );
					}
					catch( Exception ) {
						bReceiveData = false;
					}

					if( true == bReceiveData ) {
						CReceiveData objData = new CReceiveData();
						objData.strData = strReceivedData;
						objData.byteReceiveData = byteReceiveData.ToArray();
						objData.iByteLength = iReceivedByteCount;

						_callBackReceiveData?.Invoke( objData );
					}
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );

			} finally {

				try {
					Array.Clear( m_byteReceivedData, 0, m_byteReceivedData.Length );
					// ObjectDisposedException 예외 처리
					if( false == m_bSocketClose && true == m_objSocket?.Connected ) {
						m_objSocket.BeginReceive( m_byteReceivedData, 0, m_byteReceivedData.Length, SocketFlags.None, new AsyncCallback( OnReceived ), null );
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );

					Disconnect();
				}

			}
		}
	}
}