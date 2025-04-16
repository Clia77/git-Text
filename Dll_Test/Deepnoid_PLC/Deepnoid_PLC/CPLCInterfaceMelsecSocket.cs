using Deepnoid_Communication;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using static Deepnoid_PLC.CPLCDefine;

namespace Deepnoid_PLC
{
	public class CPLCInterfaceMelsecSocket : CPLCInterfaceMelsecAbstract
	{
		private CReceiveData m_objReceiveData;
		private EventWaitHandle[] m_objWaitHandle;
		private CPLCInterfaceMelsecParameterSocket m_objParameter;
		private CCommunication m_objCommunication;

		/// <summary>
		/// 수신 데이터 콜백 처리
		/// </summary>
		/// <param name="obj"></param>
		private void ReceiveData( CReceiveData obj )
		{
			m_objReceiveData = ( CReceiveData )obj.Clone();
			SetEvent();
			_callBackReceiveData?.Invoke( obj );
		}

		/// <summary>
		/// 에러 메세지 콜백 처리
		/// </summary>
		/// <param name="strErrorMessage"></param>
		private void ErrorMessage( string strErrorMessage )
		{
			_callBackErrorMessage?.Invoke( strErrorMessage );
		}

		/// <summary>
		/// 초기화
		/// </summary>
		/// <param name="objParameter"></param>
		/// <returns></returns>
		public override bool Initialize( CPLCInterfaceMelsecParameter objParameter )
		{
			bool bReturn = false;

			do {
				m_objLock = new object();
				m_objReceiveData = new CReceiveData();
				m_objWaitHandle = new EventWaitHandle[ 1 ];
				m_objWaitHandle[ 0 ] = new EventWaitHandle( false, EventResetMode.ManualReset );

				if( typeof( CPLCInterfaceMelsecParameterSocket ) != objParameter.GetParameter().GetType() ) {
					string strError = $"Fail to initialize socket type unmatch - Type : {objParameter.GetParameter().GetType().ToString()}";
					_callBackErrorMessage?.Invoke( strError );
					break;
				}
				m_objParameter = ( CPLCInterfaceMelsecParameterSocket )objParameter.GetParameter().Clone();

				switch( m_objParameter.eSocketType ) {
					case enumSocketType.SOCKET_TYPE_CLIENT:
						m_objCommunication = new CCommunication( new CCommunicationSocketClient() );
						break;
					case enumSocketType.SOCKET_TYPE_SERVER:
						m_objCommunication = new CCommunication( new CCommunicationSocketServer() );
						break;
				}
				m_objCommunication.SetCallBackReceiveData( ReceiveData );
				m_objCommunication.SetCallBackErrorMessage( ErrorMessage );

				CCommunicationParameter objCommunicationParameter = new CCommunicationParameter( m_objParameter.objParameter.GetParameter() );
				if( false == m_objCommunication.Initialize( m_objParameter.objParameter ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 해제
		/// </summary>
		public override void DeInitialize()
		{
			m_objCommunication?.DeInitialize();
		}

		/// <summary>
		/// 접속 유무
		/// </summary>
		/// <returns></returns>
		public override bool IsConnected()
		{
			return ( m_objCommunication?.IsConnected() ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 n개의 bit 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		private bool Read( string strAddress, ref bool[] bData )
		{
			bool bReturn = false;

			do {
				if( false == IsConnected() ) {
					break;
				}
				int iCount = bData.Length;

				CReceiveData objReceiveData = new CReceiveData();

				try {
					int rsize = 0;
					byte[] byteBuffer;
					// PLC에 전송할 데이터
					String strSendData = ReadCommand( strAddress, iCount, enumDataFormat.DATA_FORMAT_BIT );
					byteBuffer = new byte[ ( strSendData.Length / 2 ) ];

					int[] ihex2dec;
					string strtemp;
					int iLenth = 0;
					ihex2dec = new int[ ( strSendData.Length / 2 ) ];

					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY: {
								for( int iLoopCount = 0; iLoopCount < ( strSendData.Length / 2 ); iLoopCount++ ) {
									strtemp = strSendData.Substring( iLenth, 2 );
									ihex2dec[ iLoopCount ] = Convert.ToInt16( strtemp, 16 );
									byteBuffer[ iLoopCount ] = Convert.ToByte( ihex2dec[ iLoopCount ] );
									iLenth = iLenth + 2;
									if( iLoopCount == ( strSendData.Length / 2 ) ) {
										break;
									}
								}
							}
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII: {
								byteBuffer = new byte[ ( strSendData.Length ) ];
								Array.Copy( Encoding.ASCII.GetBytes( strSendData ), 0, byteBuffer, 0, strSendData.Length );
							}
							break;
					}

					lock( m_objLock ) {
						// 데이터 보내기전 이벤트 리셋
						ResetEvent();
						// 데이터 SEND
						m_objCommunication?.Send( byteBuffer );
						// WAIT 100ms
						if( false == WaitHandle.WaitAll( m_objWaitHandle, 200 ) ) {
							rsize = 0;
						} else {
							objReceiveData = GetReceiveData();
							int iCheckSize = ( int )( ( double )iCount / 2.0 + 0.5 ) + 11;
							if( iCheckSize == objReceiveData.iByteLength ) {
								rsize = objReceiveData.iByteLength;
							} else {
								rsize = 0;
							}
						}
					}

					string strData = "";
					if( rsize > 0 ) {
						//인자 data -> rdata로 변경
						byte[] rdata = new byte[ rsize ];
						if( enumProtocolType.PROTOCOL_TYPE_BINARY == m_objParameter.eProtocolType ) {
							string strtempCode = null;
							strtemp = string.Empty;

							Array.Copy( objReceiveData.byteReceiveData, 0, rdata, 0, rsize );

							for( int iLoopCount = 0; iLoopCount < rdata.Length; iLoopCount++ ) {
								strtempCode = strtempCode + string.Format( "{0:X2}", rdata[ iLoopCount ] );
							}

							strData = strtempCode;
						}

						ConvertWordBit( strData, ref bData );
					} else {
						break;
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool ReadBit( string strAddress, ref bool bData )
		{
			bool bReturn = false;

			do {
				int iCount = 1;
				bool[] bTemp = new bool[ iCount ];
				if( false == Read( strAddress, ref bTemp ) ) {
					break;
				}
				bData = bTemp[ 0 ];

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool ReadBit( string strAddress, ref bool[] bData )
		{
			bool bReturn = false;

			do {
				if( false == Read( strAddress, ref bData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 n개의 bit 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		private bool Write( string strAddress, bool[] bData )
		{
			bool bReturn = false;

			do {
				if( false == IsConnected() ) {
					break;
				}
				int iCount = bData.Length;

				bool bSendComplete = false;
				CReceiveData objReceiveData = new CReceiveData();

				try {
					byte[] byteBuffer = new byte[ 1 ];
					String strSendData = WriteCommand( strAddress, iCount, enumDataFormat.DATA_FORMAT_BIT );
					int[] iRelayOn = new int[ iCount ];

					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY: {
								for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
									string strVal, strValH, strValL;
									if( bData[ iLoopCount ] ) {
										iRelayOn[ iLoopCount ] = 1;
									} else {
										iRelayOn[ iLoopCount ] = 0;
									}
									strVal = string.Format( "{0:D2}", iRelayOn[ iLoopCount ] );
									strValL = strVal.Substring( 1, 1 );
									strValH = strVal.Substring( 0, 1 );
									strVal = strValL + strValH;
									strSendData = strSendData + strVal;
								}
								byteBuffer = new byte[ ( strSendData.Length / 2 ) ];
								int[] ihex2dec;
								string strtemp;
								int iLenth = 0;
								ihex2dec = new int[ ( strSendData.Length / 2 ) ];

								for( int iLoopCount = 0; iLoopCount < ( strSendData.Length / 2 ); iLoopCount++ ) {
									strtemp = strSendData.Substring( iLenth, 2 );
									ihex2dec[ iLoopCount ] = Convert.ToInt16( strtemp, 16 );
									byteBuffer[ iLoopCount ] = Convert.ToByte( ihex2dec[ iLoopCount ] );

									iLenth = iLenth + 2;

									if( iLoopCount == ( strSendData.Length / 2 ) ) {
										break;
									}
								}
							}
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII: {
								for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
									if( bData[ iLoopCount ] ) {
										iRelayOn[ iLoopCount ] = 1;
									} else {
										iRelayOn[ iLoopCount ] = 0;
									}
									strSendData = strSendData + string.Format( "{0:D1}", iRelayOn[ iLoopCount ] );
								}
								byteBuffer = new byte[ ( strSendData.Length ) ];
								Array.Copy( Encoding.ASCII.GetBytes( strSendData ), 0, byteBuffer, 0, strSendData.Length );
							}
							break;
					}

					lock( m_objLock ) {
						// 데이터 보내기전 이벤트 리셋
						ResetEvent();
						// 데이터 SEND
						m_objCommunication?.Send( byteBuffer );
						// WAIT 100ms
						if( false == WaitHandle.WaitAll( m_objWaitHandle, 200 ) ) {
							bSendComplete = false;
						} else {
							// 수신 데이터 체크
							objReceiveData = GetReceiveData();

							// jht 테스트 진행할 때 11로 들어오는데.. Q,R 타입 및 바이너리 아스키 등 조건 바뀌면 바뀌기 때문에 수신 데이터가 들어오면
							// 문제가 없는 것으로 간주
							bSendComplete = true;
							//int iCheckSize = ( iCount * 2 * 2 ) + 11;
							//if( iCheckSize != objReceiveData.iByteLength && 11 != objReceiveData.iByteLength ) {
							//	bSendComplete = false;
							//} else {
							//	bSendComplete = true;
							//}
							
						}
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
				}

				if( true == bSendComplete ) {
					bReturn = true;
				}
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool WriteBit( string strAddress, bool bData )
		{
			bool bReturn = false;
			
			do {
				int iCount = 1;
				bool[] bTemp = new bool[ iCount ];
				bTemp[ 0 ] = bData;
				if( false == Write( strAddress, bTemp ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 쓰기 
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		public override bool WriteBit( string strAddress, bool[] bData )
		{
			bool bReturn = false;
			
			do {
				if( false == Write( strAddress, bData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 n개의 word 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		private bool Read( string strAddress, ref int[] iData )
		{
			bool bReturn = false;

			do {
				if( false == IsConnected() ) {
					break;
				}
				int iCount = iData.Length;

				CReceiveData objReceiveData = new CReceiveData();

				try {
					int rsize = 0;
					byte[] byteBuffer;

					String strSendData = ReadCommand( strAddress, iCount, enumDataFormat.DATA_FORMAT_WORD );
					byteBuffer = new byte[ ( strSendData.Length / 2 ) ];

					int[] ihex2dec;
					string strtemp;
					int iLenth = 0;
					ihex2dec = new int[ ( strSendData.Length / 2 ) ];

					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY: {
								for( int iLoopCount = 0; iLoopCount < ( strSendData.Length / 2 ); iLoopCount++ ) {
									strtemp = strSendData.Substring( iLenth, 2 );
									ihex2dec[ iLoopCount ] = Convert.ToInt16( strtemp, 16 );
									byteBuffer[ iLoopCount ] = Convert.ToByte( ihex2dec[ iLoopCount ] );
									iLenth = iLenth + 2;
									if( iLoopCount == ( strSendData.Length / 2 ) ) {
										break;
									}
								}
							}
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII: {
								byteBuffer = new byte[ ( strSendData.Length ) ];
								Array.Copy( Encoding.ASCII.GetBytes( strSendData ), 0, byteBuffer, 0, strSendData.Length );
							}
							break;
					}

					lock( m_objLock ) {
						// 데이터 보내기전 이벤트 리셋
						ResetEvent();
						// 데이터 SEND
						m_objCommunication?.Send( byteBuffer );
						// WAIT 100ms
						if( false == WaitHandle.WaitAll( m_objWaitHandle, 200 ) ) {
							rsize = 0;
						} else {
							objReceiveData = GetReceiveData();
							int iCheckSize = ( iCount * 2 ) + 11;
							if( iCheckSize == objReceiveData.iByteLength ) {
								rsize = objReceiveData.iByteLength;
							} else {
								rsize = 0;
							}
						}
					}

					string strData = "";
					if( rsize > 0 ) {
						//인자 data -> rdata로 변경
						byte[] rdata = new byte[ rsize ];
						if( enumProtocolType.PROTOCOL_TYPE_BINARY == m_objParameter.eProtocolType ) {
							string strtempCode = null;
							strtemp = string.Empty;

							Array.Copy( objReceiveData.byteReceiveData, 0, rdata, 0, rsize );

							for( int iLoopCount = 0; iLoopCount < rdata.Length; iLoopCount++ ) {
								strtempCode = strtempCode + string.Format( "{0:X2}", rdata[ iLoopCount ] );
							}

							strData = strtempCode;
						}

						ConvertWord( strData, ref iData );
					} else {
						break;
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool ReadWord( string strAddress, ref int iData )
		{
			bool bReturn = false;

			do {
				int iCount = 1;
				int[] iTemp = new int[ iCount ];
				if( false == Read( strAddress, ref iTemp ) ) {
					break;
				}
				iData = iTemp[ 0 ];

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool ReadWord( string strAddress, ref int[] iData )
		{
			bool bReturn = false;

			do {
				if( false == Read( strAddress, ref iData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 n개의 word 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		private bool Write( string strAddress, int[] iData )
		{
			bool bReturn = false;
			
			do {
				if( false == IsConnected() ) {
					break;
				}
				int iCount = iData.Length;

				bool bSendComplete = false;
				CReceiveData objReceiveData = new CReceiveData();

				try {
					byte[] byteBuffer = new byte[ 1 ];
					String strSendData = WriteCommand( strAddress, iCount, enumDataFormat.DATA_FORMAT_WORD );
					string strVal, strVal_H, strval_L;

					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY: {
								for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
									int iVal = iData[ iLoopCount ];
									strVal = string.Format( "{0:X4}", iVal );
									strval_L = strVal.Substring( 2, 2 );
									strVal_H = strVal.Substring( 0, 2 );
									strSendData = strSendData + strval_L + strVal_H;
								}
								byteBuffer = new byte[ ( strSendData.Length / 2 ) ];
								int[] ihex2dec;
								string strtemp;
								int iLenth = 0;
								ihex2dec = new int[ ( strSendData.Length / 2 ) ];

								for( int iLoopCount = 0; iLoopCount < ( strSendData.Length / 2 ); iLoopCount++ ) {
									strtemp = strSendData.Substring( iLenth, 2 );
									ihex2dec[ iLoopCount ] = Convert.ToInt16( strtemp, 16 );
									byteBuffer[ iLoopCount ] = Convert.ToByte( ihex2dec[ iLoopCount ] );

									iLenth = iLenth + 2;

									if( iLoopCount == ( strSendData.Length / 2 ) ) {
										break;
									}
								}
							}
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII: {
								for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
									int iVal = iData[ iLoopCount ];
									strVal = string.Format( "{0:X4}", iVal );
									strSendData = strSendData + strVal;
								}
								byteBuffer = new byte[ ( strSendData.Length ) ];
								Array.Copy( Encoding.ASCII.GetBytes( strSendData ), 0, byteBuffer, 0, strSendData.Length );
							}
							break;
					}

					lock( m_objLock ) {
						// 데이터 보내기전 이벤트 리셋
						ResetEvent();
						// 데이터 SEND
						m_objCommunication?.Send( byteBuffer );
						// WAIT 100ms
						if( false == WaitHandle.WaitAll( m_objWaitHandle, 200 ) ) {
							bSendComplete = false;
						} else {
							// 수신 데이터 체크
							objReceiveData = GetReceiveData();
							// jht 테스트 진행할 때 11로 들어오는데.. Q,R 타입 및 바이너리 아스키 등 조건 바뀌면 바뀌기 때문에 수신 데이터가 들어오면
							// 문제가 없는 것으로 간주
							bSendComplete = true;
							//int iCheckSize = ( iCount * 2 * 2 ) + 11;
							//if( iCheckSize != objReceiveData.iByteLength && 11 != objReceiveData.iByteLength ) {
							//	bSendComplete = false;
							//} else {
							//	bSendComplete = true;
							//}
						}
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
				}

				if( true == bSendComplete ) {
					bReturn = true;
				}
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool WriteWord( string strAddress, int iData )
		{
			bool bReturn = false;

			do {
				int iCount = 1;
				int[] iTemp = new int[ iCount ];
				iTemp[ 0 ] = iData;
				if( false == Write( strAddress, iTemp ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		public override bool WriteWord( string strAddress, int[] iData )
		{
			bool bReturn = false;

			do {
				if( false == Write( strAddress, iData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 n개의 double 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		private bool Read( string strAddress, ref double[] dData )
		{
			bool bReturn = false;

			do {
				if( false == IsConnected() ) {
					break;
				}
				int iCount = dData.Length;

				CReceiveData objReceiveData = new CReceiveData();

				try {
					int rsize = 0;
					byte[] byteBuffer;

					String strSendData = ReadCommand( strAddress, iCount * 2, enumDataFormat.DATA_FORMAT_WORD );
					byteBuffer = new byte[ ( strSendData.Length / 2 ) ];

					int[] ihex2dec;
					string strtemp;
					int iLenth = 0;
					ihex2dec = new int[ ( strSendData.Length / 2 ) ];

					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY: {
								for( int iLoopCount = 0; iLoopCount < ( strSendData.Length / 2 ); iLoopCount++ ) {
									strtemp = strSendData.Substring( iLenth, 2 );
									ihex2dec[ iLoopCount ] = Convert.ToInt16( strtemp, 16 );
									byteBuffer[ iLoopCount ] = Convert.ToByte( ihex2dec[ iLoopCount ] );
									iLenth = iLenth + 2;
									if( iLoopCount == ( strSendData.Length / 2 ) ) {
										break;
									}
								}
							}
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII: {
								byteBuffer = new byte[ ( strSendData.Length ) ];
								Array.Copy( Encoding.ASCII.GetBytes( strSendData ), 0, byteBuffer, 0, strSendData.Length );
							}
							break;
					}

					lock( m_objLock ) {
						// 데이터 보내기전 이벤트 리셋
						ResetEvent();
						// 데이터 SEND
						m_objCommunication?.Send( byteBuffer );
						// WAIT 100ms
						if( false == WaitHandle.WaitAll( m_objWaitHandle, 200 ) ) {
							rsize = 0;
						} else {
							objReceiveData = GetReceiveData();
							int iCheckSize = ( iCount * 2 * 2 ) + 11;
							if( iCheckSize == objReceiveData.iByteLength ) {
								rsize = objReceiveData.iByteLength;
							} else {
								rsize = 0;
							}
						}
					}

					string strData = "";
					if( rsize > 0 ) {
						byte[] rdata = new byte[ rsize ];
						if( enumProtocolType.PROTOCOL_TYPE_BINARY == m_objParameter.eProtocolType ) {
							string strtempCode = null;
							strtemp = string.Empty;

							Array.Copy( objReceiveData.byteReceiveData, 0, rdata, 0, rsize );

							for( int iLoopCount = 0; iLoopCount < rdata.Length; iLoopCount++ ) {
								strtempCode = strtempCode + string.Format( "{0:X2}", rdata[ iLoopCount ] );
							}

							strData = strtempCode;
						}

						ConvertDoubleWord( strData, ref dData );
					} else {
						break;
					}

				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool ReadDoubleWord( string strAddress, ref double dData )
		{
			bool bReturn = false;

			do {
				int iCount = 1;
				double[] dTemp = new double[ iCount ];
				if( false == Read( strAddress, ref dTemp ) ) {
					break;
				}
				dData = dTemp[ 0 ];

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 읽기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool ReadDoubleWord( string strAddress, ref double[] dData )
		{
			bool bReturn = false;

			do {
				if( false == Read( strAddress, ref dData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 n개의 double 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		private bool Write( string strAddress, double[] dData )
		{
			bool bReturn = false;
			
			do {
				if( false == IsConnected() ) {
					break;
				}
				int iCount = dData.Length;

				bool bSendComplete = false;
				CReceiveData objReceiveData = new CReceiveData();
				short[] pData;

				try {
					byte[] byteBuffer = new byte[ 1 ];
					String strSendData = WriteCommand( strAddress, iCount * 2, enumDataFormat.DATA_FORMAT_WORD );
					string strVal, strVal_H, strval_L;

					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY: {
								for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
									int dVal = ( int )( dData[ iLoopCount ] );
									// 더블워드 변환.. 일단 자료가 없어서 현재 양식에 맞춤
									pData = new short[ 2 ];
									pData[ 0 ] = ( short )dVal;
									pData[ 1 ] = ( short )( dVal >> 16 );

									for( int iLoopConvert = 0; iLoopConvert < pData.Length; iLoopConvert++ ) {
										strVal = string.Format( "{0:X4}", pData[ iLoopConvert ] );
										strval_L = strVal.Substring( 2, 2 );
										strVal_H = strVal.Substring( 0, 2 );
										strSendData = strSendData + strval_L + strVal_H;
									}
								}
								byteBuffer = new byte[ ( strSendData.Length / 2 ) ];
								int[] ihex2dec;
								string strtemp;
								int iLenth = 0;
								ihex2dec = new int[ ( strSendData.Length / 2 ) ];

								for( int iLoopCount = 0; iLoopCount < ( strSendData.Length / 2 ); iLoopCount++ ) {
									strtemp = strSendData.Substring( iLenth, 2 );
									ihex2dec[ iLoopCount ] = Convert.ToInt16( strtemp, 16 );
									byteBuffer[ iLoopCount ] = Convert.ToByte( ihex2dec[ iLoopCount ] );

									iLenth = iLenth + 2;

									if( iLoopCount == ( strSendData.Length / 2 ) ) {
										break;
									}
								}
							}
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII: {
								for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
									int dVal = ( int )( dData[ iLoopCount ] );
									// 더블워드 변환.. 일단 자료가 없어서 현재 양식에 맞춤
									pData = new short[ 2 ];
									pData[ 0 ] = ( short )dVal;
									pData[ 1 ] = ( short )( dVal >> 16 );

									for( int iLoopConvert = 0; iLoopConvert < pData.Length; iLoopConvert++ ) {
										strVal = string.Format( "{0:X4}", pData[ iLoopConvert ] );
										strSendData = strSendData + strVal;
									}
								}
								byteBuffer = new byte[ ( strSendData.Length ) ];
								Array.Copy( Encoding.ASCII.GetBytes( strSendData ), 0, byteBuffer, 0, strSendData.Length );
							}
							break;
					}

					lock( m_objLock ) {
						// 데이터 보내기전 이벤트 리셋
						ResetEvent();
						// 데이터 SEND
						m_objCommunication?.Send( byteBuffer );
						// WAIT 100ms
						if( false == WaitHandle.WaitAll( m_objWaitHandle, 200 ) ) {
							bSendComplete = false;
						} else {
							// 수신 데이터 체크
							objReceiveData = GetReceiveData();
							// jht 테스트 진행할 때 11로 들어오는데.. Q,R 타입 및 바이너리 아스키 등 조건 바뀌면 바뀌기 때문에 수신 데이터가 들어오면
							// 문제가 없는 것으로 간주
							bSendComplete = true;
							//int iCheckSize = ( iCount * 2 * 2 ) + 11;
							//if( iCheckSize != objReceiveData.iByteLength && 11 != objReceiveData.iByteLength ) {
							//	bSendComplete = false;
							//} else {
							//	bSendComplete = true;
							//}
						}
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
				}

				if( true == bSendComplete ) {
					bReturn = true;
				}
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool WriteDoubleWord( string strAddress, double dData )
		{
			bool bReturn = false;

			do {
				int iCount = 1;
				double[] dTemp = new double[ iCount ];
				dTemp[ 0 ] = dData;
				if( false == Write( strAddress, dTemp ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 쓰기
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		public override bool WriteDoubleWord( string strAddress, double[] dData )
		{
			bool bReturn = false;

			do {
				if( false == Write( strAddress, dData ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// PLC read 명령어 생성
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iCount"></param>
		/// <param name="eDataFormat"></param>
		/// <returns></returns>
		private string ReadCommand( string strAddress, int iCount, enumDataFormat eDataFormat )
		{
			string strBitWord;
			string strReadComand;
			strReadComand = "";
			string strAddressDM = strAddress.Substring( 0, 1 );
			string strAdddressNo = strAddress.Remove( 0, 1 );
			string strAdddressNo_H, strAdddressNo_M, strAdddressNo_M2, strAdddressNo_L;
			string strDeviceCount = string.Format( "{0:X4}", iCount );

			string strDeviceCode = "";
			// PLC영역별 주소코드 변환
			ParseAddress( strAddressDM, ref strDeviceCode );

			// 바이너리타입과 아스키 타입에 따라 생성되는 명령어가 다름
			switch( m_objParameter.eProtocolType ) {
				case enumProtocolType.PROTOCOL_TYPE_BINARY: {

						// 시리즈 타입
						switch( m_objParameter.eSeriseType ) {
							case enumSeriseType.SERISE_TYPE_Q: {
									strAdddressNo = String.Format( "{0:X6}", Convert.ToInt32( strAdddressNo ) );
									strAdddressNo_H = strAdddressNo.Substring( 4, 2 );
									strAdddressNo_M = strAdddressNo.Substring( 2, 2 );
									strAdddressNo_L = strAdddressNo.Substring( 0, 2 ); //Hex Adress H + L

									strAdddressNo = strAdddressNo_H + strAdddressNo_M + strAdddressNo_L;
									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0100";
									} else {
										strBitWord = "0000";
									}
								}
								break;
							case enumSeriseType.SERISE_TYPE_R: {
									strAdddressNo = String.Format( "{0:X8}", Convert.ToInt32( strAdddressNo ) );
									strAdddressNo_H = strAdddressNo.Substring( 6, 2 );
									strAdddressNo_M = strAdddressNo.Substring( 4, 2 );
									strAdddressNo_M2 = strAdddressNo.Substring( 2, 2 );
									strAdddressNo_L = strAdddressNo.Substring( 0, 2 );

									strAdddressNo = strAdddressNo_H + strAdddressNo_M + strAdddressNo_M2 + strAdddressNo_L;

									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0300";
									} else {
										strBitWord = "0200";
									}
								}
								break;
							default: {
									strAdddressNo = String.Format( "{0:X6}", Convert.ToInt32( strAdddressNo ) );
									strAdddressNo_H = strAdddressNo.Substring( 4, 2 );
									strAdddressNo_M = strAdddressNo.Substring( 2, 2 );
									strAdddressNo_L = strAdddressNo.Substring( 0, 2 ); //Hex Adress H + L
									strAdddressNo = strAdddressNo_H + strAdddressNo_M + strAdddressNo_L;

									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0100";
									} else {
										strBitWord = "0000";
									}
								}
								break;
						}

						strDeviceCount = strDeviceCount.Substring( 2, 2 ) + strDeviceCount.Substring( 0, 2 );

						strReadComand = strReadComand + "5000"; // 서브헤더
						strReadComand = strReadComand + "00"; // Network No
						strReadComand = strReadComand + "FF"; // PLC No
						strReadComand = strReadComand + "FF03"; // 요구 상대 모듈 I/O No
						strReadComand = strReadComand + "00"; // 요구 상대 국번호
						strReadComand = strReadComand + "0C00"; // 요구 데이터 길이 0C18[3098 LEN], 0018[요구 데이터 길이 뒤 부터 24BYTE :09~21 12*2]
						strReadComand = strReadComand + "1000"; // CPU 감시 타이머
						strReadComand = strReadComand + "0104"; // 0401[READ], 1401[WRITE]
						strReadComand = strReadComand + strBitWord; // Q시리즈0000[WORD], 0001[BIT] R시리즈0002[WORD], 0003[BIT]
						strReadComand = strReadComand + strAdddressNo; // Device Address Hexcode L88 H13 자릿수00
						strReadComand = strReadComand + strDeviceCode; // DEVICE CODE  //Binary Mode D*:A8 M*:90
						strReadComand = strReadComand + strDeviceCount; // DEVICE 점수 L05 H00
					}
					break;
				case enumProtocolType.PROTOCOL_TYPE_ASCII: {

						strReadComand = strReadComand + "5000"; // 서브헤더
						strReadComand = strReadComand + "00"; // Network No
						strReadComand = strReadComand + "FF"; // PLC No
						strReadComand = strReadComand + "03FF"; // 요구 상대 모듈 I/O No
						strReadComand = strReadComand + "00"; // 요구 상대 국번호
						strReadComand = strReadComand + "0018"; // 요구 데이터 길이 0C18[3098 LEN], 0018[요구 데이터 길이 뒤 부터 24BYTE :09~21 12*2]
						strReadComand = strReadComand + "0010"; // CPU 감시 타이머
						strReadComand = strReadComand + "0401"; // 0401[READ], 1401[WRITE]

						string strDeviceAddress;
						switch( m_objParameter.eSeriseType ) {
							case enumSeriseType.SERISE_TYPE_Q: {
									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0001";
									} else {
										strBitWord = "0000";
									}
									strDeviceAddress = String.Format( "{0:D6}", strAdddressNo );
								}
								break;
							case enumSeriseType.SERISE_TYPE_R: {
									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0003";
									} else {
										strBitWord = "0002";
									}
									strDeviceAddress = String.Format( "{0:D8}", strAdddressNo );
								}
								break;
							default: {
									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0001";
									} else {
										strBitWord = "0000";
									}
									strDeviceAddress = String.Format( "{0:D6}", strAdddressNo );
								}
								break;
						}

						strReadComand = strReadComand + strBitWord; // 0000[WORD], 0001[BIT]
						strReadComand = strReadComand + strDeviceCode; // DEVICE CODE  D*:A8 M*:90
						strReadComand = strReadComand + strDeviceAddress; // DEVICE ADDRESS
						strReadComand = strReadComand + String.Format( "{0:D4}", iCount ); // DEVICE 점수
					}
					break;
			}

			return strReadComand;
		}

		/// <summary>
		/// PLC write 명령어 생성
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iCount"></param>
		/// <param name="eDataFormat"></param>
		/// <returns></returns>
		private string WriteCommand( string strAddress, int iCount, enumDataFormat eDataFormat )
		{
			int iLenth;
			string strBitWord;
			string strReadComand;
			strReadComand = "";
			string strAddressDM = strAddress.Substring( 0, 1 );
			string strAdddressNo = strAddress.Remove( 0, 1 );
			string strAdddressNo_H, strAdddressNo_M, strAdddressNo_M2, strAdddressNo_L;
			string strHexLenth, strHexLenth_HL;
			string strDeviceCount, strDeviceCount_HL;

			string strDeviceCode = "";
			// PLC영역별 주소코드 변환
			ParseAddress( strAddressDM, ref strDeviceCode );

			// 바이너리타입과 아스키 타입에 따라 생성되는 명령어가 다름
			switch( m_objParameter.eProtocolType ) {
				case enumProtocolType.PROTOCOL_TYPE_BINARY: {

						// 시리즈 타입
						switch( m_objParameter.eSeriseType ) {
							case enumSeriseType.SERISE_TYPE_Q: {
									strAdddressNo = String.Format( "{0:X6}", Convert.ToInt32( strAdddressNo ) );
									strAdddressNo_H = strAdddressNo.Substring( 4, 2 );
									strAdddressNo_M = strAdddressNo.Substring( 2, 2 );
									strAdddressNo_L = strAdddressNo.Substring( 0, 2 ); //Hex Adress H + L
									strAdddressNo = strAdddressNo_H + strAdddressNo_M + strAdddressNo_L;

									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0100";
									} else {
										strBitWord = "0000";
									}
								}
								break;
							case enumSeriseType.SERISE_TYPE_R: {
									strAdddressNo = String.Format( "{0:X8}", Convert.ToInt32( strAdddressNo ) );
									strAdddressNo_H = strAdddressNo.Substring( 6, 2 );
									strAdddressNo_M = strAdddressNo.Substring( 4, 2 );
									strAdddressNo_M2 = strAdddressNo.Substring( 2, 2 );
									strAdddressNo_L = strAdddressNo.Substring( 0, 2 ); //Hex Adress H + L
									strAdddressNo = strAdddressNo_H + strAdddressNo_M + strAdddressNo_M2 + strAdddressNo_L;
									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0300";
									} else {
										strBitWord = "0200";
									}
								}
								break;
							default: {
									strAdddressNo = String.Format( "{0:X6}", Convert.ToInt32( strAdddressNo ) );
									strAdddressNo_H = strAdddressNo.Substring( 4, 2 );
									strAdddressNo_M = strAdddressNo.Substring( 2, 2 );
									strAdddressNo_L = strAdddressNo.Substring( 0, 2 ); //Hex Adress H + L
									strAdddressNo = strAdddressNo_H + strAdddressNo_M + strAdddressNo_L;
									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0100";
									} else {
										strBitWord = "0000";
									}
								}
								break;
						}

						if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
							iLenth = 12 + iCount;
							strHexLenth = string.Format( "{0:X4}", iLenth );
							strHexLenth_HL = strHexLenth.Substring( 2, 2 ) + strHexLenth.Substring( 0, 2 );
							strDeviceCount = string.Format( "{0:D4}", iCount );
						} else {
							iLenth = 12 + ( iCount * 2 );
							strHexLenth = string.Format( "{0:X4}", iLenth );
							strHexLenth_HL = strHexLenth.Substring( 2, 2 ) + strHexLenth.Substring( 0, 2 );
							strDeviceCount = String.Format( "{0:X4}", ( iCount * 1 ) );
						}

						strDeviceCount_HL = strDeviceCount.Substring( 2, 2 ) + strDeviceCount.Substring( 0, 2 );
						strReadComand = strReadComand + "5000"; // 서브헤더
						strReadComand = strReadComand + "00"; // Network No
						strReadComand = strReadComand + "FF"; // PLC No
						strReadComand = strReadComand + "FF03"; // 요구 상대 모듈 I/O No
						strReadComand = strReadComand + "00"; // 요구 상대 국번호
						strReadComand = strReadComand + strHexLenth_HL; // 요구 데이터 길이 0C18[3098 LEN], 0018[요구 데이터 길이 뒤 부터 24BYTE :09~21 12*2]
						strReadComand = strReadComand + "1000"; // CPU 감시 타이머
						strReadComand = strReadComand + "0114"; // 0401[READ], 1401[WRITE]
						strReadComand = strReadComand + strBitWord; // 0000[WORD], 0001[BIT]
						strReadComand = strReadComand + strAdddressNo; // Device Address Hexcode L88 H13 자릿수00
						strReadComand = strReadComand + strDeviceCode; // DEVICE CODE  //Binary Mode D*:A8 M*:90
						strReadComand = strReadComand + strDeviceCount_HL; // DEVICE 점수 L05 H00 Binary Bit 경우 짝수 word 상관없음
					}
					break;
				case enumProtocolType.PROTOCOL_TYPE_ASCII: {

						iLenth = 24 + iCount;
						strReadComand = strReadComand + "5000"; // 서브헤더
						strReadComand = strReadComand + "00"; // Network No
						strReadComand = strReadComand + "FF"; // PLC No
						strReadComand = strReadComand + "03FF"; // 요구 상대 모듈 I/O No
						strReadComand = strReadComand + "00"; // 요구 상대 국번호
						strReadComand = strReadComand + string.Format( "{0:X4}", iLenth ); // 요구 데이터 길이 0C18[3098 LEN], 0018[요구 데이터 길이 뒤 부터 24BYTE :09~21 12*2]
						strReadComand = strReadComand + "0010"; // CPU 감시 타이머
						strReadComand = strReadComand + "1401"; // 0401[READ], 1401[WRITE]

						string strDeviceAddress;
						switch( m_objParameter.eSeriseType ) {
							case enumSeriseType.SERISE_TYPE_Q: {
									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0001";
									} else {
										strBitWord = "0000";
									}
									strDeviceAddress = String.Format( "{0:D6}", strAdddressNo );
								}
								break;
							case enumSeriseType.SERISE_TYPE_R: {
									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0003";
									} else {
										strBitWord = "0002";
									}
									strDeviceAddress = String.Format( "{0:D8}", strAdddressNo );
								}
								break;
							default: {
									if( enumDataFormat.DATA_FORMAT_BIT == eDataFormat ) {
										strBitWord = "0001";
									} else {
										strBitWord = "0000";
									}
									strDeviceAddress = String.Format( "{0:D6}", strAdddressNo );
								}
								break;
						}

						strReadComand = strReadComand + strBitWord; // 0000[WORD], 0001[BIT]
						strReadComand = strReadComand + strDeviceCode; // DEVICE CODE //Binary Mode D*:A8 M*:90
						strReadComand = strReadComand + strDeviceAddress; // DEVICE ADDRESS
						strReadComand = strReadComand + String.Format( "{0:D4}", iCount ); // DEVICE 점수 
					}
					break;
			}

			return strReadComand;
		}

		/// <summary>
		/// 영역별 주소 코드 변환
		/// </summary>
		/// <param name="strAddressType"></param>
		/// <param name="strAddress"></param>
		/// <returns></returns>
		private bool ParseAddress( string strAddressType, ref string strAddress )
		{
			/*
            Address 예:			R901
            DEVICE CODE:		D, W, R, X, Y, M, B, F, L, S
            구현 DeviceCode:	D, W, R, X, Y, M, B, F, L, S
            WORD / BIT 구분:    W, W, W, B, B, B, B, B, B, B
            HEX / DEC 구분:     D, H, D, H, H, D, H, D, D, D
            */

			//string strType = "";
			//switch( m_objParameter.eSeriseType ) {
			//	case enumSeriseType.SERISE_TYPE_Q: {
			//			switch( m_objParameter.eProtocolType ) {
			//				case enumProtocolType.PROTOCOL_TYPE_BINARY:
			//					strType = "00";
			//					break;
			//				case enumProtocolType.PROTOCOL_TYPE_ASCII:
			//					strType = "**";
			//					break;
			//			}
			//		}
			//		break;
			//	default:
			//		strType = "";
			//		break;
			//}

			bool bReturn = false;

			do {
				strAddress = "";
				if( "D" == strAddressType ) {
					// 데이터 레지스터
					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY:
							strAddress = "A8";
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII:
							strAddress = "D*";
							break;
					}
				} else if( "W" == strAddressType ) {
					// 링크 레지스터
					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY:
							strAddress = "B4";
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII:
							strAddress = "W*";
							break;
					}
				} else if( "R" == strAddressType ) {
					// 화일 레지스터
					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY:
							strAddress = "AF";
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII:
							strAddress = "R*";
							break;
					}
				} else if( "X" == strAddressType ) {
					// 입력
					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY:
							strAddress = "9C";
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII:
							strAddress = "X*";
							break;
					}
				} else if( "Y" == strAddressType ) {
					// 출력
					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY:
							strAddress = "9D";
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII:
							strAddress = "Y*";
							break;
					}
				} else if( "M" == strAddressType ) {
					// 내부 릴레이
					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY:
							strAddress = "90";
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII:
							strAddress = "M*";
							break;
					}
				} else if( "B" == strAddressType ) {
					// 링크 릴레이
					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY:
							strAddress = "A0";
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII:
							strAddress = "B*";
							break;
					}
				} else if( "F" == strAddressType ) {
					// 어넌시에이터
					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY:
							strAddress = "93";
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII:
							strAddress = "F*";
							break;
					}
				} else if( "L" == strAddressType ) {
					// 래치 릴레이
					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY:
							strAddress = "92";
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII:
							strAddress = "L*";
							break;
					}
				} else if( "S" == strAddressType ) {
					// 링크 릴레이
					switch( m_objParameter.eProtocolType ) {
						case enumProtocolType.PROTOCOL_TYPE_BINARY:
							strAddress = "98";
							break;
						case enumProtocolType.PROTOCOL_TYPE_ASCII:
							strAddress = "S*";
							break;
					}
				}

				bReturn = true;
			} while( false );

			//strAddress = strAddress + strType;

			return bReturn;
		}

		/// <summary>
		/// string -> bit 변환
		/// </summary>
		/// <param name="strData"></param>
		/// <param name="bData"></param>
		private void ConvertWordBit( string strData, ref bool bData )
		{
			try {
				int iReciveData = strData.Length;
				int istrReciveData = strData.Length;
				string strBit = strData.Remove( 0, 22 );

				string strval_HL = strBit.Substring( 0, 1 );
				if( "1" == strval_HL ) {
					bData = true;
				} else {
					bData = false;
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
		}

		/// <summary>
		/// string -> bit 변환
		/// </summary>
		/// <param name="strData"></param>
		/// <param name="bData"></param>
		private void ConvertWordBit( string strData, ref bool[] bData )
		{
			try {
				int iReciveData = strData.Length;
				int istrReciveData = strData.Length;
				string strBit = strData.Remove( 0, 22 );

				for( int iLoopCount = 0; iLoopCount < strBit.Length; iLoopCount++ ) {
					string strval_HL = strBit.Substring( iLoopCount, 1 );
					if( "1" == strval_HL ) {
						bData[ iLoopCount ] = true;
					} else {
						bData[ iLoopCount ] = false;
					}
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
		}

		/// <summary>
		/// string -> word 변환
		/// </summary>
		/// <param name="strData"></param>
		/// <param name="iData"></param>
		private void ConvertWord( string strData, ref int iData )
		{
			try {
				int iReciveData = strData.Length;
				int istrReciveData = strData.Length;
				string strBit = strData.Remove( 0, 22 );
				int iDec;

				string strval_HL = strBit.Substring( 0, 4 );
				string strval_L = strval_HL.Substring( 0, 2 );
				string strval_H = strval_HL.Substring( 2, 2 );
				strval_HL = strval_H + strval_L;
				iDec = int.Parse( strval_HL, System.Globalization.NumberStyles.AllowHexSpecifier );
				iData = iDec;
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
		}

		/// <summary>
		/// string -> word 변환
		/// </summary>
		/// <param name="strData"></param>
		/// <param name="iData"></param>
		private void ConvertWord( string strData, ref int[] iData )
		{
			try {
				int iReciveData = strData.Length;
				int istrReciveData = strData.Length;
				string strBit = strData.Remove( 0, 22 );
				int iDec;

				for( int iLoopCount = 0; iLoopCount < strBit.Length / 4; iLoopCount++ ) {
					string strval_HL = strBit.Substring( iLoopCount * 4, 4 );
					string strval_L = strval_HL.Substring( 0, 2 );
					string strval_H = strval_HL.Substring( 2, 2 );
					strval_HL = strval_H + strval_L;
					iDec = int.Parse( strval_HL, System.Globalization.NumberStyles.AllowHexSpecifier );
					iData[ iLoopCount ] = iDec;
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
		}

		/// <summary>
		/// string -> double 변환
		/// </summary>
		/// <param name="strData"></param>
		/// <param name="dData"></param>
		private void ConvertDoubleWord( string strData, ref double dData )
		{
			try {
				string strWord;
				int iDec;
				string strval_HL, strval_H, strval_L, strval_H1, strval_L1;

				strWord = strData.Remove( 0, 22 );

				strval_HL = strWord.Substring( 0, 8 );
				strval_H = strval_HL.Substring( 0, 2 );
				strval_L = strval_HL.Substring( 2, 2 );
				strval_H1 = strval_HL.Substring( 4, 2 );
				strval_L1 = strval_HL.Substring( 6, 2 );
				strval_HL = strval_L1 + strval_H1 + strval_L + strval_H;
				iDec = int.Parse( strval_HL, System.Globalization.NumberStyles.AllowHexSpecifier );

				dData = ( Convert.ToDouble( iDec ) );
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
		}

		/// <summary>
		/// string -> double 변환
		/// </summary>
		/// <param name="strData"></param>
		/// <param name="dData"></param>
		private void ConvertDoubleWord( string strData, ref double[] dData )
		{
			try {
				string strWord;
				int iDec;
				string strval_HL, strval_H, strval_L, strval_H1, strval_L1;

				strWord = strData.Remove( 0, 22 );

				for( int iLoopCount = 0; iLoopCount < strWord.Length / 8; iLoopCount++ ) {
					strval_HL = strWord.Substring( iLoopCount * 8, 8 );
					strval_H = strval_HL.Substring( 0, 2 );
					strval_L = strval_HL.Substring( 2, 2 );
					strval_H1 = strval_HL.Substring( 4, 2 );
					strval_L1 = strval_HL.Substring( 6, 2 );
					strval_HL = strval_L1 + strval_H1 + strval_L + strval_H;
					iDec = int.Parse( strval_HL, System.Globalization.NumberStyles.AllowHexSpecifier );

					dData[ iLoopCount ] = ( Convert.ToDouble( iDec ) );
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}
		}

		/// <summary>
		/// binary -> ascii 문자열로 변환
		/// </summary>
		/// <param name="strData"></param>
		/// <returns></returns>
		private string ConvertAscii( string strData )
		{
			string strWord;
			string strval, strval_HL;
			int iDecValL;
			int iDecValH;

			string strResult = "";
			StringBuilder sBuffer = new StringBuilder();

			try {
				if( enumProtocolType.PROTOCOL_TYPE_BINARY == m_objParameter.eProtocolType ) {
					strWord = strData.Remove( 0, 22 );
					strval = string.Empty;
					for( int iLoopCount = 0; iLoopCount < strWord.Length / 4; iLoopCount++ ) {
						strval_HL = strWord.Substring( iLoopCount * 4, 4 );
						if( "0000" == strval_HL ) break;
						iDecValH = int.Parse( strval_HL.Substring( 0, 2 ), System.Globalization.NumberStyles.AllowHexSpecifier );
						iDecValL = int.Parse( strval_HL.Substring( 2, 2 ), System.Globalization.NumberStyles.AllowHexSpecifier );
						strval = sBuffer.Append( ( char )iDecValH ).ToString();
						if( 0 == ( char )iDecValL ) break;
						strval = sBuffer.Append( ( char )iDecValL ).ToString();
					}
					strResult = strval;
				} else {
					strResult = strData;
				}
			}
			catch( Exception ex ) {
				string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
				string strMethodName = MethodBase.GetCurrentMethod()?.Name;
				string strException = $"{strClassName} {strMethodName} : {ex.Message}";
				_callBackErrorMessage?.Invoke( strException );
			}

			return strResult;
		}

		/// <summary>
		/// 이벤트 핸들 set
		/// </summary>
		private void SetEvent()
		{
			m_objWaitHandle[ 0 ].Set();
		}

		/// <summary>
		/// 이벤트 핸들 reset
		/// </summary>
		private void ResetEvent()
		{
			m_objWaitHandle[ 0 ].Reset();
		}

		private CReceiveData GetReceiveData()
		{
			return ( CReceiveData )m_objReceiveData.Clone();
		}
	}
}