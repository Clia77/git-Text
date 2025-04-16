using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

namespace Deepnoid_Communication
{
	public class CCommunicationSerialPort : CCommunicationAbstract
	{
		private CCommunicationParameterSerialPort m_objParameterSerialPort;
		private SerialPort m_objSerialPort;

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
				m_bThreadExit = false;
				m_objLock = new object();
				m_byteReceivedData = new byte[ CCommunicationDefine.DEF_BYTE_LENGTH ];
				m_objSerialPort = null;

				if( typeof( CCommunicationParameterSerialPort ) != objParameter.GetParameter().GetType() ) {
					string strError = $"Fail to initialize serialport type unmatch - Type : {objParameter.GetParameter().GetType().ToString()}";
					_callBackErrorMessage?.Invoke( strError );
					break;
				}
				m_objParameterSerialPort = ( CCommunicationParameterSerialPort )objParameter.GetParameter().Clone();

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
		/// 시리얼 포트 연결
		/// </summary>
		public override void Connect()
		{
			try {
				if( null == m_objParameterSerialPort ) {
					return;
				}
				m_objSerialPort = new SerialPort();
				m_objSerialPort.PortName = m_objParameterSerialPort.strSerialPortName;
				m_objSerialPort.BaudRate = m_objParameterSerialPort.iSerialPortBaudrate;
				m_objSerialPort.Parity = ( Parity )m_objParameterSerialPort.eParity;
				m_objSerialPort.DataBits = m_objParameterSerialPort.iSerialPortDataBits;
				m_objSerialPort.StopBits = ( StopBits )m_objParameterSerialPort.eStopBits;
				m_objSerialPort.Open();
				m_objSerialPort.DataReceived -= DataReceived;
				m_objSerialPort.DataReceived += DataReceived;

				m_bConnected = true;
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );

				m_bConnected = false;
			}
		}

		/// <summary>
		/// 시리얼 포트 연결 해제
		/// </summary>
		public override void Disconnect()
		{
			try {
				m_objSerialPort?.Close();
				m_objSerialPort = null;
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
			m_bConnected = false;
		}

		private void DataReceived( object sender, SerialDataReceivedEventArgs e )
		{
			int iReceivedByteCount = ( m_objSerialPort?.Read( m_byteReceivedData, 0, m_byteReceivedData.Length ) ).GetValueOrDefault();

			if( 0 >= iReceivedByteCount ) {
				Disconnect();
				return;
			}

			string strReceivedData = Encoding.Default.GetString( m_byteReceivedData ).Substring( 0, iReceivedByteCount );

			CReceiveData objData = new CReceiveData();
			objData.strData = strReceivedData;
			objData.byteReceiveData = m_byteReceivedData.ToArray();
			objData.iByteLength = iReceivedByteCount;

			_callBackReceiveData?.Invoke( objData );

			Array.Clear( m_byteReceivedData, 0, m_byteReceivedData.Length );
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
				if( null == m_objSerialPort ) {
					break;
				}

				lock( m_objLock ) {
					try {
						m_objSerialPort.DiscardInBuffer();
						m_objSerialPort.DiscardOutBuffer();
						m_objSerialPort.Write( strData );
						m_objSerialPort.BaseStream.Flush();
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
				if( null == m_objSerialPort ) {
					break;
				}

				lock( m_objLock ) {
					try {
						m_objSerialPort.DiscardInBuffer();
						m_objSerialPort.DiscardOutBuffer();
						m_objSerialPort.Write( byteData, 0, byteData.Length );
						m_objSerialPort.BaseStream.Flush();
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
			CCommunicationSerialPort pThis = ( CCommunicationSerialPort )obj;
			TimeSpan objTimeSpan = new TimeSpan( 0, 0, 1 );
			double dMilliseconds = objTimeSpan.TotalMilliseconds;
			int iThreadPeriod = 100;

			while( false == pThis.m_bThreadExit ) {
				if( 0.0 >= dMilliseconds ) {
					if( null != pThis.m_objSerialPort ) {
						// 소켓 연결 상태 확인
						pThis.Connect();
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
	}
}