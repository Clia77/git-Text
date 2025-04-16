using System;
using System.Linq;
using System.Reflection;
using static Deepnoid_PLC.CPLCDefine;

namespace Deepnoid_PLC
{
	public class CPLCInterfaceMelsecCCLink : CPLCInterfaceMelsecAbstract
	{
		private int m_iMelsecPath;
		private bool m_bConnected;
		private CPLCInterfaceMelsecParameterCCLink m_objParameter;

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
				m_iMelsecPath = 0;
				m_bConnected = false;

				if( typeof( CPLCInterfaceMelsecParameterCCLink ) != objParameter.GetParameter().GetType() ) {
					string strError = $"Fail to initialize cc-link type unmatch - Type : {objParameter.GetParameter().GetType().ToString()}";
					_callBackErrorMessage?.Invoke( strError );
					break;
				}
				m_objParameter = ( CPLCInterfaceMelsecParameterCCLink )objParameter.GetParameter().Clone();

				if( false == Open( false ) ) {
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
			Close();
		}

		/// <summary>
		/// melsec board open
		/// </summary>
		/// <param name="bBoardReset"></param>
		/// <returns></returns>
		private bool Open( bool bBoardReset )
		{
			bool bReturn = false;

			do {
				if( true == m_bConnected ) {
					Close();
				}
				int iResult = CPLCInterfaceMelsecCCLinkDll.mdOpen( short.Parse( m_objParameter.strChannel ), -1, out m_iMelsecPath );
				// 0 성공, -31 Dll Load Error 무시하고 진행 시 정상동작
				if( 0 != iResult && -31 != iResult ) {
					break;
				}

				if( true == bBoardReset ) {
					if( 0 != CPLCInterfaceMelsecCCLinkDll.mdBdRst( m_iMelsecPath ) ) {
						break;
					}
				}

				bReturn = true;
			} while( false );

			if( true == bReturn ) {
				m_bConnected = true;
			}
			return bReturn;
		}

		/// <summary>
		/// melsec board close
		/// </summary>
		/// <returns></returns>
		private bool Close()
		{
			bool bReturn = false;

			do {
				if( false == m_bConnected ) {
					bReturn = true;
					break;
				}
				if( 0 != CPLCInterfaceMelsecCCLinkDll.mdClose( m_iMelsecPath ) ) {
					break;
				}

				bReturn = true;
			} while( false );

			if( true == bReturn ) {
				m_bConnected = false;
			}
			return bReturn;
		}

		/// <summary>
		/// 접속 유무
		/// </summary>
		/// <returns></returns>
		public override bool IsConnected()
		{
			return m_bConnected;
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
				if( false == m_bConnected ) {
					break;
				}
				int iCount = bData.Length;

				try {
					short[] pBuf = new short[ iCount ];
					int iAddress = int.Parse( strAddress );

					short[] sDev = new short[ 4 ];
					sDev[ 0 ] = 1;
					sDev[ 1 ] = CPLCInterfaceMelsecCCLinkDll.DevB;
					sDev[ 2 ] = ( short )iAddress;
					sDev[ 3 ] = ( short )iCount;

					int iReturnCode = CPLCInterfaceMelsecCCLinkDll.mdRandR( m_iMelsecPath, DEF_MELSEC_STATION_NUMBER, ref sDev[ 0 ], out pBuf[ 0 ], ( short )iCount );
					if( 0 != iReturnCode ) {
						break;
					}
					for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
						bData[ iLoopCount ] = ( ( pBuf[ iLoopCount ] >> ( 0 % 16 ) ) & 0x01 ) > 0 ? true : false;
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
					break;
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
				if( false == m_bConnected ) {
					break;
				}
				int iCount = bData.Length;

				try {
					short[] pBuf = new short[ iCount ];
					int iAddress = int.Parse( strAddress );

					for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
						int iData = ( true == bData[ iLoopCount ] ) ? 1 : 0;
						pBuf[ iLoopCount ] |= ( short )( iData << ( 0 % 16 ) );
					}

					short[] sDev = new short[ 4 ];
					sDev[ 0 ] = 1;
					sDev[ 1 ] = CPLCInterfaceMelsecCCLinkDll.DevB;
					sDev[ 2 ] = ( short )iAddress;
					sDev[ 3 ] = ( short )iCount;

					int iReturnCode = CPLCInterfaceMelsecCCLinkDll.mdRandW( m_iMelsecPath, DEF_MELSEC_STATION_NUMBER, ref sDev[ 0 ], out pBuf[ 0 ], ( short )iCount );
					if( 0 != iReturnCode ) {
						break;
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
					break;
				}

				bReturn = true;
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
				if( false == WriteBit( strAddress, bData ) ) {
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
				if( false == m_bConnected ) {
					break;
				}
				int iCount = iData.Length;

				try {
					int iAddress = int.Parse( strAddress );
					short[] sTemp = new short[ iCount ];
					int iBufSize = iCount * sizeof( short );

					int iReturnCode = CPLCInterfaceMelsecCCLinkDll.mdReceiveEx( m_iMelsecPath, 0, DEF_MELSEC_STATION_NUMBER, CPLCInterfaceMelsecCCLinkDll.DevW, iAddress, ref iBufSize, ref sTemp[ 0 ] );
					if( 0 != iReturnCode ) {
						break;
					}
					for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
						iData[ iLoopCount ] = sTemp[ iLoopCount ];
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
					break;
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

		private bool Write( string strAddress, int[] iData )
		{
			bool bReturn = false;

			do {
				if( false == m_bConnected ) {
					break;
				}
				int iCount = iData.Length;

				try {
					int iAddress = int.Parse( strAddress );
					int iBufSize = iCount * sizeof( short );
					short[] sTemp = new short[ iCount ];
					for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
						sTemp[ iLoopCount ] = ( short )iData[ iLoopCount ];
					}

					int iReturnCode = CPLCInterfaceMelsecCCLinkDll.mdSendEx( m_iMelsecPath, 0, DEF_MELSEC_STATION_NUMBER, CPLCInterfaceMelsecCCLinkDll.DevW, iAddress, ref iBufSize, ref sTemp[ 0 ] );
					if( 0 != iReturnCode ) {
						break;
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
					break;
				}

				bReturn = true;
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
				if( false == m_bConnected ) {
					break;
				}
				int iCount = dData.Length;

				try {
					int[] iTemp = new int[ iCount * 2 ];
					if( false == Read( strAddress, ref iTemp ) ) {
						break;
					}

					int iData = 0;
					for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
						iData = ( ushort )iTemp[ ( iLoopCount * 2 ) + 1 ] << 16;
						iData += ( ushort )iTemp[ ( iLoopCount * 2 ) ];
						dData[ iLoopCount ] = iData;
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
					break;
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
				if( false == m_bConnected ) {
					break;
				}
				int iCount = dData.Length;

				try {
					int[] iTemp = new int[ iCount * 2 ];
					for( int iLoopCount = 0; iLoopCount < iCount; iLoopCount++ ) {
						int iValue = ( int )dData[ iLoopCount ];
						iTemp[ iLoopCount * 2 ] = ( short )iValue;
						iTemp[ ( iLoopCount * 2 ) + 1 ] = ( short )( iValue >> 16 );
					}

					if( false == Write( strAddress, iTemp ) ) {
						break;
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackErrorMessage?.Invoke( strException );
					break;
				}

				bReturn = true;
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
		/// short 형 데이터를 char 형 아스키 문자열로 캐스팅
		/// </summary>
		/// <param name="pszOut"></param>
		/// <param name="pnaBuf"></param>
		/// <param name="iSize"></param>
		private void AsciiToString( char[] pszOut, short[] pnaBuf, int iSize )
		{
			int i = 0;
			for( i = 0; i < iSize; i++ ) {
				pszOut[ ( i * 2 ) + 1 ] = ( char )( pnaBuf[ i ] / 0x0100 );
				pszOut[ ( i * 2 ) + 0 ] = ( char )( pnaBuf[ i ] - ( ( pnaBuf[ i ] / 0x0100 ) * 0x0100 ) );
			}
		}
	}
}