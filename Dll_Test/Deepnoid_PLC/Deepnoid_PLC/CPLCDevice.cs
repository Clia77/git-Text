using Deepnoid_Communication;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using static Deepnoid_PLC.CPLCDefine;

namespace Deepnoid_PLC
{
	public class CPLCDevice
	{
		private CPLCDeviceParameter m_objDeviceParameter;
		private CPLCDeviceMonitoringParameter m_objDeviceMonitoringParameter;
		private CPLCDeviceAbstract m_objAbstract;
		private bool m_bStartMonitoring;
		private bool m_bThreadExit;
		private Thread m_threadMonitoring;

		/// <summary>
		/// 맵 데이터 변경 콜백 처리
		/// </summary>
		/// <param name="objReceiveData"></param>
		public delegate void CallBackMapDataChanged( CPLCMapDataChanged[] obj );
		protected CallBackMapDataChanged _callBackMapDataChanged;
		public void SetCallBackMapDataChanged( CallBackMapDataChanged callBack )
		{
			_callBackMapDataChanged = callBack;
		}

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
		private CallBackErrorMessage _callBackErrorMessage;
		public void SetCallBackErrorMessage( CallBackErrorMessage callBack )
		{
			_callBackErrorMessage = callBack;
		}

		public CPLCDevice( CPLCDeviceAbstract objAbstract )
		{
			m_objAbstract = objAbstract;
		}

		/// <summary>
		/// 맵 데이터 변경 콜백 처리
		/// </summary>
		/// <param name="obj"></param>
		private void MapDataChanged( CPLCMapDataChanged[] obj )
		{
			_callBackMapDataChanged?.Invoke( obj );
		}

		/// <summary>
		/// 수신 데이터 콜백 처리
		/// </summary>
		/// <param name="obj"></param>
		private void ReceiveData( CReceiveData obj )
		{
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
		public bool Initialize( CPLCDeviceParameter objParameter )
		{
			m_bThreadExit = false;
			m_bStartMonitoring = false;
			m_objDeviceParameter = objParameter.Clone() as CPLCDeviceParameter;
			m_objDeviceMonitoringParameter = null;

			m_objAbstract?.SetCallBackMapDataChanged( MapDataChanged );
			m_objAbstract?.SetCallBackReceiveData( ReceiveData );
			m_objAbstract?.SetCallBackErrorMessage( ErrorMessage );
			if( false == m_objAbstract?.Initialize( objParameter ) ) {
				return false;
			}
			m_threadMonitoring = new Thread( ThreadMonitoring );
			m_threadMonitoring.Start( this );

			return true;
		}

		/// <summary>
		/// 해제
		/// </summary>
		public void DeInitialize()
		{
			m_bThreadExit = true;
			m_threadMonitoring?.Join();
			m_objAbstract?.DeInitialize();
		}

		public void StartMonitoring( CPLCDeviceMonitoringParameter objParameter )
		{
			m_objDeviceMonitoringParameter = objParameter.Clone() as CPLCDeviceMonitoringParameter;
			m_bStartMonitoring = true;
		}

		public void StopMonitoring()
		{
			m_bStartMonitoring = false;
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
		/// 읽어온 맵 데이터를 DataTable 형식에 맞춰서 생성해서 리턴
		/// </summary>
		/// <returns></returns>
		public DataTable GetDataTable()
		{
			return m_objAbstract?.GetDataTable();
		}

		/// <summary>
		/// 맵 데이터 내에 타입에 해당하는 수량 반환
		/// </summary>
		/// <param name="eCommunicationType"></param>
		/// <returns></returns>
		public int GetMapDataCount( enumPLCDeviceCommunicationType eCommunicationType )
		{
			return ( m_objAbstract?.GetMapDataCount( eCommunicationType ) ).GetValueOrDefault();
		}

		/// <summary>
		/// short 데이터 -> ascii 문자열로 변경
		/// </summary>
		/// <param name="iData"></param>
		/// <returns></returns>
		public string GetWordToAscii( int iData )
		{
			string strReturn = "";

			do {
				try {
					if( 0 == iData ) {
						break;
					}
					byte[] objBytes = BitConverter.GetBytes( iData );
					string strData = Encoding.UTF8.GetString( objBytes );
					strData = strData.Trim( '\0' );
					strReturn = strData;
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
				}
			} while( false );

			return strReturn;
		}

		public string GetWordToAscii( int[] iData )
		{
			string strReturn = "";

			foreach( var item in iData ) {
				strReturn += GetWordToAscii( item );
			}

			return strReturn;
		}

		/// <summary>
		/// double 데이터 -> ascii 문자열로 변경
		/// </summary>
		/// <param name="dData"></param>
		/// <returns></returns>
		public string GetWordToAscii( double dData )
		{
			string strReturn = "";

			do {
				try {
					if( 0 == dData ) {
						break;
					}
					byte[] objBytes = BitConverter.GetBytes( dData );
					string strData = Encoding.UTF8.GetString( objBytes );
					strData = strData.Trim( '\0' );
					strReturn = strData;
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
				}
			} while( false );

			return strReturn;
		}

		public string GetWordToAscii( double[] dData )
		{
			string strReturn = "";

			foreach( var item in dData ) {
				strReturn += GetWordToAscii( item );
			}

			return strReturn;
		}

		/// <summary>
		/// 맵 데이터에서 이름에 해당하는 value 값 get
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool GetValueName( string strName, ref bool objValue )
		{
			bool bReturn = false;

			do {
				// get map data [ bool type ]
				if( false == m_objAbstract?.GetValueName( strName, ref objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueName( string strName, ref bool[] objValue )
		{
			bool bReturn = false;

			do {
				// get map data [ bool type ]
				if( false == m_objAbstract?.GetValueName( strName, ref objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueName( string strName, ref int objValue )
		{
			bool bReturn = false;

			do {
				// devide 자리수
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithName( strName, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}
				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// get map data [ int type ]
					if( false == m_objAbstract?.GetValueName( strName, ref objValue ) ) {
						break;
					}
					objValue /= ( int )objParameter.dMultiple;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueName( string strName, ref int[] objValue )
		{
			bool bReturn = false;

			do {
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithName( strName, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}

				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// get map data [ int type ]
					if( false == m_objAbstract?.GetValueName( strName, ref objValue ) ) {
						break;
					}
				}

				// devide 자리수
				for( int iLoopCount = 0; iLoopCount < objValue.Length; iLoopCount++ ) {
					if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
						objValue[ iLoopCount ] /= ( int )objParameter.dMultiple;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueName( string strName, ref double objValue )
		{
			bool bReturn = false;

			do {
				// devide 자리수
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithName( strName, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}

				// Word 단위에 double 형 데이터가 들어갈 수 있다.
				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
					double dTemp = 0.0;
					// get map data [ double type ]
					if( false == m_objAbstract?.GetValueName( strName, ref dTemp ) ) {
						break;
					}
					dTemp /= objParameter.dMultiple;
					objValue = dTemp;
				} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					int iTemp = 0;
					// get map data [ int type ]
					if( false == m_objAbstract?.GetValueName( strName, ref iTemp ) ) {
						break;
					}
					iTemp /= ( int )objParameter.dMultiple;
					objValue = iTemp;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueName( string strName, ref double[] objValue )
		{
			bool bReturn = false;

			do {
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithName( strName, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}

				double[] dListTemp = new double[ objValue.Length ];
				int[] iListTemp = new int[ objValue.Length ];
				// Word 단위에 double 형 데이터가 들어갈 수 있다.
				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
					// get map data [ double type ]
					if( false == m_objAbstract?.GetValueName( strName, ref dListTemp ) ) {
						break;
					}
				} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// get map data [ int type ]
					if( false == m_objAbstract?.GetValueName( strName, ref iListTemp ) ) {
						break;
					}
				}

				// devide 자리수
				for( int iLoopCount = 0; iLoopCount < objValue.Length; iLoopCount++ ) {
					if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
						objValue[ iLoopCount ] = dListTemp[ iLoopCount ] / objParameter.dMultiple;
					} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
						objValue[ iLoopCount ] = iListTemp[ iLoopCount ] / objParameter.dMultiple;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueName( string strName, ref CPLCMapDataParameterWordToBit objValue )
		{
			bool bReturn = false;

			do {
				// get map data [ CPLCMapDataParameterWordToBit type ]
				if( false == m_objAbstract?.GetValueName( strName, ref objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueName( string strName, ref CPLCMapDataParameterWordToBit[] objValue )
		{
			bool bReturn = false;

			do {
				// get map data [ CPLCMapDataParameterWordToBit type ]
				if( false == m_objAbstract?.GetValueName( strName, ref objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 맵 데이터에서 어드레스에 해당하는 value 값 get
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool GetValueAddress( string strAddress, ref bool objValue )
		{
			bool bReturn = false;

			do {
				// get map data [ bool type ]
				if( false == m_objAbstract?.GetValueAddress( strAddress, ref objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueAddress( string strAddress, ref bool[] objValue )
		{
			bool bReturn = false;

			do {
				// get map data [ bool type ]
				if( false == m_objAbstract?.GetValueAddress( strAddress, ref objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueAddress( string strAddress, ref int objValue )
		{
			bool bReturn = false;

			do {
				// devide 자리수
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithAddress( strAddress, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}
				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// get map data [ int type ]
					if( false == m_objAbstract?.GetValueAddress( strAddress, ref objValue ) ) {
						break;
					}
					objValue /= ( int )objParameter.dMultiple;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueAddress( string strAddress, ref int[] objValue )
		{
			bool bReturn = false;

			do {
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithAddress( strAddress, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}

				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// get map data [ int type ]
					if( false == m_objAbstract?.GetValueAddress( strAddress, ref objValue ) ) {
						break;
					}
				}

				// devide 자리수
				for( int iLoopCount = 0; iLoopCount < objValue.Length; iLoopCount++ ) {
					if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
						objValue[ iLoopCount ] /= ( int )objParameter.dMultiple;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueAddress( string strAddress, ref double objValue )
		{
			bool bReturn = false;

			do {
				// devide 자리수
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithAddress( strAddress, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}
				// Word 단위에 double 형 데이터가 들어갈 수 있다.
				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
					double dTemp = 0.0;
					// get map data [ double type ]
					if( false == m_objAbstract?.GetValueAddress( strAddress, ref dTemp ) ) {
						break;
					}
					dTemp /= objParameter.dMultiple;
					objValue = dTemp;
				} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// get map data [ int type ]
					int iTemp = 0;
					if( false == m_objAbstract?.GetValueAddress( strAddress, ref iTemp ) ) {
						break;
					}
					iTemp /= ( int )objParameter.dMultiple;
					objValue = iTemp;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueAddress( string strAddress, ref double[] objValue )
		{
			bool bReturn = false;

			do {
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithAddress( strAddress, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}

				double[] dListTemp = new double[ objValue.Length ];
				int[] iListTemp = new int[ objValue.Length ];
				// Word 단위에 double 형 데이터가 들어갈 수 있다.
				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
					// get map data [ double type ]
					if( false == m_objAbstract?.GetValueAddress( strAddress, ref dListTemp ) ) {
						break;
					}
				} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// get map data [ int type ]
					if( false == m_objAbstract?.GetValueAddress( strAddress, ref iListTemp ) ) {
						break;
					}
				}

				// devide 자리수
				for( int iLoopCount = 0; iLoopCount < objValue.Length; iLoopCount++ ) {
					if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
						objValue[ iLoopCount ] = dListTemp[ iLoopCount ] / objParameter.dMultiple;
					} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
						objValue[ iLoopCount ] = iListTemp[ iLoopCount ] / objParameter.dMultiple;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueAddress( string strAddress, ref CPLCMapDataParameterWordToBit objValue )
		{
			bool bReturn = false;

			do {
				// get map data [ CPLCMapDataParameterWordToBit type ]
				if( false == m_objAbstract?.GetValueAddress( strAddress, ref objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool GetValueAddress( string strAddress, ref CPLCMapDataParameterWordToBit[] objValue )
		{
			bool bReturn = false;

			do {
				// get map data [ CPLCMapDataParameterWordToBit type ]
				if( false == m_objAbstract?.GetValueAddress( strAddress, ref objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 맵 데이터에서 이름에 해당하는 value 값 set
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool SetValueName( string strName, bool objValue )
		{
			bool bReturn = false;

			do {
				// set map data [ bool type ]
				if( false == m_objAbstract?.SetValueName( strName, objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueName( string strName, bool[] objValue )
		{
			bool bReturn = false;

			do {
				// set map data [ bool type ]
				if( false == m_objAbstract?.SetValueName( strName, objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueName( string strName, int objValue )
		{
			bool bReturn = false;

			do {
				// multiple 자리수
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithName( strName, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}
				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// set map data [ int type ]
					objValue *= ( int )objParameter.dMultiple;
					// check overflow
					// word 를 bit 로 나누는 경우 ushort 형으로
					if( true == objParameter.objWordToBit.bUseWordToBit ) {
						// ushort 범위로 제한
						if( objValue < ushort.MinValue ) {
							objValue = ushort.MinValue;
						} else if( objValue > ushort.MaxValue ) {
							objValue = ushort.MaxValue;
						}
					} else {
						// short 범위로 제한
						if( objValue < short.MinValue ) {
							objValue = short.MinValue;
						} else if( objValue > short.MaxValue ) {
							objValue = short.MaxValue;
						}
					}

					if( false == m_objAbstract?.SetValueName( strName, objValue ) ) {
						break;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueName( string strName, int[] objValue )
		{
			bool bReturn = false;

			do {
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithName( strName, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}

				// multiple 자리수
				for( int iLoopCount = 0; iLoopCount < objValue.Length; iLoopCount++ ) {
					// Word or DWord 인 경우 Multiple 인자값 반영
					if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
						objValue[ iLoopCount ] *= ( int )objParameter.dMultiple;
						// check overflow
						// word 를 bit 로 나누는 경우 ushort 형으로
						if( true == objParameter.objWordToBit.bUseWordToBit ) {
							// ushort 범위로 제한
							if( objValue[ iLoopCount ] < ushort.MinValue ) {
								objValue[ iLoopCount ] = ushort.MinValue;
							} else if( objValue[ iLoopCount ] > ushort.MaxValue ) {
								objValue[ iLoopCount ] = ushort.MaxValue;
							}
						} else {
							// short 범위로 제한
							if( objValue[ iLoopCount ] < short.MinValue ) {
								objValue[ iLoopCount ] = short.MinValue;
							} else if( objValue[ iLoopCount ] > short.MaxValue ) {
								objValue[ iLoopCount ] = short.MaxValue;
							}
						}
					}
				}

				if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// set map data [ int type ]
					if( false == m_objAbstract?.SetValueName( strName, objValue ) ) {
						break;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueName( string strName, double objValue )
		{
			bool bReturn = false;

			do {
				// multiple 자리수
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithName( strName, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}

				// Word 단위에 double 형 데이터가 들어갈 수 있다.
				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
					double dTemp = objValue * objParameter.dMultiple;
					// set map data [ double type ]
					if( false == m_objAbstract?.SetValueName( strName, dTemp ) ) {
						break;
					}
				} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// set map data [ int type ]
					int iTemp = ( int )( objValue * objParameter.dMultiple );
					// check overflow
					// word 를 bit 로 나누는 경우 ushort 형으로
					if( true == objParameter.objWordToBit.bUseWordToBit ) {
						// ushort 범위로 제한
						if( iTemp < ushort.MinValue ) {
							iTemp = ushort.MinValue;
						} else if( iTemp > ushort.MaxValue ) {
							iTemp = ushort.MaxValue;
						}
					} else {
						// short 범위로 제한
						if( iTemp < short.MinValue ) {
							iTemp = short.MinValue;
						} else if( iTemp > short.MaxValue ) {
							iTemp = short.MaxValue;
						}
					}

					if( false == m_objAbstract?.SetValueName( strName, iTemp ) ) {
						break;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueName( string strName, double[] objValue )
		{
			bool bReturn = false;

			do {
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithName( strName, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}

				double[] dListTemp = new double[ objValue.Length ];
				int[] iListTemp = new int[ objValue.Length ];
				// multiple 자리수
				for( int iLoopCount = 0; iLoopCount < objValue.Length; iLoopCount++ ) {
					// Word 단위에 double 형 데이터가 들어갈 수 있다.
					// Word or DWord 인 경우 Multiple 인자값 반영
					if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
						dListTemp[ iLoopCount ] = objValue[ iLoopCount ] * objParameter.dMultiple;
					} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
						iListTemp[ iLoopCount ] = ( int )( objValue[ iLoopCount ] * objParameter.dMultiple );
						// check overflow
						// word 를 bit 로 나누는 경우 ushort 형으로
						if( true == objParameter.objWordToBit.bUseWordToBit ) {
							// ushort 범위로 제한
							if( iListTemp[ iLoopCount ] < ushort.MinValue ) {
								iListTemp[ iLoopCount ] = ushort.MinValue;
							} else if( iListTemp[ iLoopCount ] > ushort.MaxValue ) {
								iListTemp[ iLoopCount ] = ushort.MaxValue;
							}
						} else {
							// short 범위로 제한
							if( iListTemp[ iLoopCount ] < short.MinValue ) {
								iListTemp[ iLoopCount ] = short.MinValue;
							} else if( iListTemp[ iLoopCount ] > short.MaxValue ) {
								iListTemp[ iLoopCount ] = short.MaxValue;
							}
						}
					}
				}

				if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
					// set map data [ double type ]
					if( false == m_objAbstract?.SetValueName( strName, dListTemp ) ) {
						break;
					}
				} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// set map data [ int type ]
					if( false == m_objAbstract?.SetValueName( strName, iListTemp ) ) {
						break;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueName( string strName, CPLCMapDataParameterWordToBit objValue )
		{
			bool bReturn = false;

			do {
				// set map data [ CPLCMapDataParameterWordToBit type ]
				if( false == m_objAbstract?.SetValueName( strName, objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueName( string strName, CPLCMapDataParameterWordToBit[] objValue )
		{
			bool bReturn = false;

			do {
				// set map data [ CPLCMapDataParameterWordToBit type ]
				if( false == m_objAbstract?.SetValueName( strName, objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 맵 데이터에서 어드레스에 해당하는 value 값 set
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool SetValueAddress( string strAddress, bool objValue )
		{
			bool bReturn = false;

			do {
				// set map data [ bool type ]
				if( false == m_objAbstract?.SetValueAddress( strAddress, objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueAddress( string strAddress, bool[] objValue )
		{
			bool bReturn = false;

			do {
				// set map data [ bool type ]
				if( false == m_objAbstract?.SetValueAddress( strAddress, objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueAddress( string strAddress, int objValue )
		{
			bool bReturn = false;

			do {
				// multiple 자리수
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithAddress( strAddress, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}
				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// set map data [ int type ]
					objValue *= ( int )objParameter.dMultiple;
					// check overflow
					// word 를 bit 로 나누는 경우 ushort 형으로
					if( true == objParameter.objWordToBit.bUseWordToBit ) {
						// ushort 범위로 제한
						if( objValue < ushort.MinValue ) {
							objValue = ushort.MinValue;
						} else if( objValue > ushort.MaxValue ) {
							objValue = ushort.MaxValue;
						}
					} else {
						// short 범위로 제한
						if( objValue < short.MinValue ) {
							objValue = short.MinValue;
						} else if( objValue > short.MaxValue ) {
							objValue = short.MaxValue;
						}
					}

					if( false == m_objAbstract?.SetValueAddress( strAddress, objValue ) ) {
						break;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueAddress( string strAddress, int[] objValue )
		{
			bool bReturn = false;

			do {
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithAddress( strAddress, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}

				// multiple 자리수
				for( int iLoopCount = 0; iLoopCount < objValue.Length; iLoopCount++ ) {
					// Word or DWord 인 경우 Multiple 인자값 반영
					if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
						objValue[ iLoopCount ] *= ( int )objParameter.dMultiple;
						// check overflow
						// word 를 bit 로 나누는 경우 ushort 형으로
						if( true == objParameter.objWordToBit.bUseWordToBit ) {
							// ushort 범위로 제한
							if( objValue[ iLoopCount ] < ushort.MinValue ) {
								objValue[ iLoopCount ] = ushort.MinValue;
							} else if( objValue[ iLoopCount ] > ushort.MaxValue ) {
								objValue[ iLoopCount ] = ushort.MaxValue;
							}
						} else {
							// short 범위로 제한
							if( objValue[ iLoopCount ] < short.MinValue ) {
								objValue[ iLoopCount ] = short.MinValue;
							} else if( objValue[ iLoopCount ] > short.MaxValue ) {
								objValue[ iLoopCount ] = short.MaxValue;
							}
						}
					}
				}

				if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// set map data [ int type ]
					if( false == m_objAbstract?.SetValueAddress( strAddress, objValue ) ) {
						break;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueAddress( string strAddress, double objValue )
		{
			bool bReturn = false;

			do {
				// multiple 자리수
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithAddress( strAddress, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}
				// Word 단위에 double 형 데이터가 들어갈 수 있다.
				// Word or DWord 인 경우 Multiple 인자값 반영
				if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
					double dTemp = objValue * objParameter.dMultiple;
					// set map data [ double type ]
					if( false == m_objAbstract?.SetValueAddress( strAddress, dTemp ) ) {
						break;
					}
				} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// set map data [ int type ]
					int iTemp = ( int )( objValue * objParameter.dMultiple );
					// check overflow
					// word 를 bit 로 나누는 경우 ushort 형으로
					if( true == objParameter.objWordToBit.bUseWordToBit ) {
						// ushort 범위로 제한
						if( iTemp < ushort.MinValue ) {
							iTemp = ushort.MinValue;
						} else if( iTemp > ushort.MaxValue ) {
							iTemp = ushort.MaxValue;
						}
					} else {
						// short 범위로 제한
						if( iTemp < short.MinValue ) {
							iTemp = short.MinValue;
						} else if( iTemp > short.MaxValue ) {
							iTemp = short.MaxValue;
						}
					}

					if( false == m_objAbstract?.SetValueAddress( strAddress, iTemp ) ) {
						break;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueAddress( string strAddress, double[] objValue )
		{
			bool bReturn = false;

			do {
				int iFindIndex = 0;
				if( false == m_objAbstract?.GetFindIndexWithAddress( strAddress, ref iFindIndex ) ) {
					break;
				}
				CPLCMapDataParameter objParameter = m_objAbstract?.GetParameterWithIndex( iFindIndex );
				if( null == objParameter ) {
					break;
				}

				double[] dListTemp = new double[ objValue.Length ];
				int[] iListTemp = new int[ objValue.Length ];
				// multiple 자리수
				for( int iLoopCount = 0; iLoopCount < objValue.Length; iLoopCount++ ) {
					// Word 단위에 double 형 데이터가 들어갈 수 있다.
					// Word or DWord 인 경우 Multiple 인자값 반영
					if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
						dListTemp[ iLoopCount ] = objValue[ iLoopCount ] * objParameter.dMultiple;
					} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
						iListTemp[ iLoopCount ] = ( int )( objValue[ iLoopCount ] * objParameter.dMultiple );
						// check overflow
						// word 를 bit 로 나누는 경우 ushort 형으로
						if( true == objParameter.objWordToBit.bUseWordToBit ) {
							// ushort 범위로 제한
							if( iListTemp[ iLoopCount ] < ushort.MinValue ) {
								iListTemp[ iLoopCount ] = ushort.MinValue;
							} else if( iListTemp[ iLoopCount ] > ushort.MaxValue ) {
								iListTemp[ iLoopCount ] = ushort.MaxValue;
							}
						} else {
							// short 범위로 제한
							if( iListTemp[ iLoopCount ] < short.MinValue ) {
								iListTemp[ iLoopCount ] = short.MinValue;
							} else if( iListTemp[ iLoopCount ] > short.MaxValue ) {
								iListTemp[ iLoopCount ] = short.MaxValue;
							}
						}
					}
				}

				if( enumPLCDeviceCommunicationType.DWORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objParameter.ePLCCommunicationType ) {
					// set map data [ double type ]
					if( false == m_objAbstract?.SetValueAddress( strAddress, dListTemp ) ) {
						break;
					}
				} else if( enumPLCDeviceCommunicationType.WORD_IN == objParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objParameter.ePLCCommunicationType ) {
					// set map data [ int type ]
					if( false == m_objAbstract?.SetValueAddress( strAddress, iListTemp ) ) {
						break;
					}
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueAddress( string strAddress, CPLCMapDataParameterWordToBit objValue )
		{
			bool bReturn = false;

			do {
				// set map data [ CPLCMapDataParameterWordToBit type ]
				if( false == m_objAbstract?.SetValueAddress( strAddress, objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		public bool SetValueAddress( string strAddress, CPLCMapDataParameterWordToBit[] objValue )
		{
			bool bReturn = false;

			do {
				// set map data [ CPLCMapDataParameterWordToBit type ]
				if( false == m_objAbstract?.SetValueAddress( strAddress, objValue ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			return bReturn;
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		private void ReadBit( string strName, int iCount )
		{
			m_objAbstract?.ReadBit( strName, iCount );
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		private void WriteBit( string strName, int iCount )
		{
			m_objAbstract?.WriteBit( strName, iCount );
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		private void ReadWord( string strName, int iCount )
		{
			m_objAbstract?.ReadWord( strName, iCount );
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		private void WriteWord( string strName, int iCount )
		{
			m_objAbstract?.WriteWord( strName, iCount );
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double word 데이터를 read plc -> set map data
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		private void ReadDoubleWord( string strName, int iCount )
		{
			m_objAbstract?.ReadDoubleWord( strName, iCount );
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double word 데이터를 get map data -> write plc
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iCount"></param>
		private void WriteDoubleWord( string strName, int iCount )
		{
			m_objAbstract?.WriteDoubleWord( strName, iCount );
		}

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		private bool Read( string strName, ref bool bData )
		{
			return ( m_objAbstract?.Read( strName, ref bData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		private bool Read( string strName, ref bool[] bData )
		{
			return ( m_objAbstract?.Read( strName, ref bData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 1개의 bit 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		private bool Write( string strName, bool bData )
		{
			return ( m_objAbstract?.Write( strName, bData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 bit 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="bData"></param>
		/// <returns></returns>
		private bool Write( string strName, bool[] bData )
		{
			return ( m_objAbstract?.Write( strName, bData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		private bool Read( string strName, ref int iData )
		{
			return ( m_objAbstract?.Read( strName, ref iData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		private bool Read( string strName, ref int[] iData )
		{
			return ( m_objAbstract?.Read( strName, ref iData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 1개의 word 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		private bool Write( string strName, int iData )
		{
			return ( m_objAbstract?.Write( strName, iData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 word 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iData"></param>
		/// <returns></returns>
		private bool Write( string strName, int[] iData )
		{
			return ( m_objAbstract?.Write( strName, iData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		private bool Read( string strName, ref double dData )
		{
			return ( m_objAbstract?.Read( strName, ref dData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 읽기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		private bool Read( string strName, ref double[] dData )
		{
			return ( m_objAbstract?.Read( strName, ref dData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 1개의 double 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		private bool Write( string strName, double dData )
		{
			return ( m_objAbstract?.Write( strName, dData ) ).GetValueOrDefault();
		}

		/// <summary>
		/// 시작 주소에서 설정된 사이즈만큼 double 데이터 쓰기
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="dData"></param>
		/// <returns></returns>
		private bool Write( string strName, double[] dData )
		{
			return ( m_objAbstract?.Write( strName, dData ) ).GetValueOrDefault();
		}

		private static void ThreadMonitoring( object obj )
		{
			CPLCDevice pThis = ( CPLCDevice )obj;
			int iThreadPeriod = 100;
			bool bPreviousStartMonitoring = false;
			CPLCDeviceMonitoringParameter objParameter = null;
			int iListIndex = 0;

			while( false == pThis.m_bThreadExit ) {
				bool bStartMonitoring = pThis.m_bStartMonitoring;

				if( bPreviousStartMonitoring != bStartMonitoring ) {
					// 모니터링 시작된 시점
					if( true == bStartMonitoring ) {
						objParameter = pThis.m_objDeviceMonitoringParameter.Clone() as CPLCDeviceMonitoringParameter;
						iThreadPeriod = objParameter.iThreadPeriod;
						iListIndex = 0;
					}
					bPreviousStartMonitoring = bStartMonitoring;
				}

				if( true == bStartMonitoring ) {
					if( true == pThis.IsConnected() ) {
						int iIndex = iListIndex++ % objParameter.objParameterList.Count;
						enumPLCDeviceRWType eRWType = objParameter.objParameterList[ iIndex ].eRWType;
						string strName = objParameter.objParameterList[ iIndex ].strName;
						int iCount = objParameter.objParameterList[ iIndex ].iCount;

						switch( eRWType ) {
							case enumPLCDeviceRWType.READ_BIT:
								pThis.ReadBit( strName, iCount );
								break;
							case enumPLCDeviceRWType.READ_WORD:
								pThis.ReadWord( strName, iCount );
								break;
							case enumPLCDeviceRWType.READ_DWORD:
								pThis.ReadDoubleWord( strName, iCount );
								break;
							case enumPLCDeviceRWType.WRITE_BIT:
								pThis.WriteBit( strName, iCount );
								break;
							case enumPLCDeviceRWType.WRITE_WORD:
								pThis.WriteWord( strName, iCount );
								break;
							case enumPLCDeviceRWType.WRITE_DWORD:
								pThis.WriteDoubleWord( strName, iCount );
								break;
						}
					}
				}

				Thread.Sleep( iThreadPeriod );
			}
		}
	}
}