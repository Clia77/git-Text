using Deepnoid_Communication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using static Deepnoid_PLC.CPLCDefine;

namespace Deepnoid_PLC
{
	public class CPLCMapDataChanged
	{
		public string strAddress;
		public object objOriginValue;
		public object objValue;

		public CPLCMapDataChanged( string address, object originValue, object value )
		{
			strAddress = address;
			objOriginValue = originValue;
			objValue = value;
		}
	}

	public class CPLCMapDataParameterBit : ICloneable
	{
		public string strName;
		public bool bBit;

		public CPLCMapDataParameterBit()
		{
			strName = "";
			bBit = false;
		}

		public object Clone()
		{
			CPLCMapDataParameterBit obj = new CPLCMapDataParameterBit();

			obj.strName = this.strName;
			obj.bBit = this.bBit;

			return obj;
		}
	}

	public class CPLCMapDataParameterWordToBit : ICloneable
	{
		/// <summary>
		/// DataTable 만들 때 해당 영역 word 가 bit 로 쪼개서 쓰기 위한 건지 확인하기 위해 생성
		/// </summary>
		public bool bUseWordToBit;
		public CPLCMapDataParameterBit[] objBit;

		public CPLCMapDataParameterWordToBit()
		{
			bUseWordToBit = false;
			objBit = new CPLCMapDataParameterBit[ DEF_WORD_TO_BIT ];
			for( int iLoopBit = 0; iLoopBit < objBit.Length; iLoopBit++ ) {
				objBit[ iLoopBit ] = new CPLCMapDataParameterBit();
			}
		}

		// 인덱서 추가
		public CPLCMapDataParameterBit this[ string strName ]
		{
			get
			{
				return objBit.FirstOrDefault( bit => bit.strName == strName );
			}
		}

		public int GetIndex( string strName )
		{
			return objBit.ToList().FindIndex( bit => bit.strName == strName );
		}

		public object Clone()
		{
			CPLCMapDataParameterWordToBit obj = new CPLCMapDataParameterWordToBit();

			obj.bUseWordToBit = this.bUseWordToBit;
			obj.objBit = new CPLCMapDataParameterBit[ this.objBit.Length ];
			for( int iLoopBit = 0; iLoopBit < obj.objBit.Length; iLoopBit++ ) {
				obj.objBit[ iLoopBit ] = this.objBit[ iLoopBit ].Clone() as CPLCMapDataParameterBit;
			}

			return obj;
		}
	}

	/// <summary>
	/// 디바이스 dat 에 정의된 한 열 데이터 정의
	/// </summary>
	public class CPLCMapDataParameter : ICloneable
	{
		/// <summary>
		/// 어드레스
		/// </summary>
		public string strAddress;
		/// <summary>
		/// 접근 이름
		/// </summary>
		public string strName;
		/// <summary>
		/// plc 범위 타입
		/// </summary>
		public enumPLCDeviceCommunicationType ePLCCommunicationType;
		/// <summary>
		/// 표현하는 자리수에 따라서 곱하고 나누기 위함 ( short or double )
		/// </summary>
		public double dMultiple;
		/// <summary>
		/// 저장 값 ( 원본 )
		/// </summary>
		public object objValue;
		/// <summary>
		/// 타입이 WORD 인 경우에 해당 WORD를 bit 로 쪼개서 운영할 경우 필요함
		/// </summary>
		public CPLCMapDataParameterWordToBit objWordToBit;

		public CPLCMapDataParameter()
		{
			strAddress = "";
			strName = "";
			ePLCCommunicationType = enumPLCDeviceCommunicationType.BIT_IN;
			dMultiple = 1.0;
			objValue = null;
			objWordToBit = new CPLCMapDataParameterWordToBit();
		}

		public object Clone()
		{
			CPLCMapDataParameter obj = new CPLCMapDataParameter();

			obj.strAddress = this.strAddress;
			obj.strName = this.strName;
			obj.ePLCCommunicationType = this.ePLCCommunicationType;
			obj.dMultiple = this.dMultiple;
			obj.objValue = this.objValue;
			obj.objWordToBit = ( CPLCMapDataParameterWordToBit )this.objWordToBit.Clone();

			return obj;
		}
	}

	public class CPLCMapData
	{
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
		/// 디바이스 dat 정의
		/// </summary>
		private ConcurrentDictionary<int, CPLCMapDataParameter> objDataParameter;

		public CPLCMapData( string strFullPath )
		{
			objDataParameter = new ConcurrentDictionary<int, CPLCMapDataParameter>();
			int iIndex = 0;

			try {
				string strDirectoryPath = Directory.GetParent( strFullPath )?.FullName;
				if( false == Directory.Exists( strDirectoryPath ) ) {
					throw new ApplicationException( "Fail to read Plc Device Map Data because directory is none" );
				}
				if( false == File.Exists( strFullPath ) ) {
					throw new ApplicationException( "Fail to read Plc Device Map Data because file is none" );
				}

				using( FileStream objStream = File.Open( strFullPath, FileMode.Open ) ) {
					using( StreamReader objReader = new StreamReader( objStream, Encoding.Default ) ) {
						while( false == objReader.EndOfStream ) {
							string strTemp = objReader.ReadLine();
							string strLine = strTemp.Trim().Replace( "\t", "" );
							string[] strSplitLine = strLine.Split( ',' );

							if( null == strSplitLine || Enum.GetNames( typeof( enumPLCDeviceIndex ) ).Length > strSplitLine.Length ) {
								throw new ApplicationException( $"Fail to read Plc Device Map Data because split line error : {strLine}" );
							}

							string strAddress = strSplitLine[ ( int )enumPLCDeviceIndex.ADDRESS ];
							string strName = strSplitLine[ ( int )enumPLCDeviceIndex.NAME ];
							string strCommunicationType = strSplitLine[ ( int )enumPLCDeviceIndex.COMMUNICATION_TYPE ];
							enumPLCDeviceCommunicationType eCommunicationType = ( enumPLCDeviceCommunicationType )Enum.Parse( typeof( enumPLCDeviceCommunicationType ), strCommunicationType );
							string strDigit = strSplitLine[ ( int )enumPLCDeviceIndex.DIGIT ];

							if( enumPLCDeviceCommunicationType.WORD_TO_BIT_NAME == eCommunicationType ) {
								// 기존에 있는 리스트에 파라매터에서 동일한 어드레스를 갖는 곳에 접근하여 
								// plc 번지 '.' 으로 구분
								string[] strWordToBitSplit = strAddress.Split( '.' );
								// 해당 번지 검색
								if( null != strWordToBitSplit && 2 == strWordToBitSplit.Length ) {
									string strSearchAddress = strWordToBitSplit[ 0 ];
									string strWordToBitIndex = strWordToBitSplit[ 1 ];

									CPLCMapDataParameter objSearchParameter = objDataParameter.Values.FirstOrDefault( obj => obj.strAddress == strSearchAddress );
									if( null != objSearchParameter ) {
										int iWordToBitIndex = Convert.ToInt32( strWordToBitIndex, 16 );
										objSearchParameter.objWordToBit.objBit[ iWordToBitIndex ].strName = strName;
										if( false == objSearchParameter.objWordToBit.bUseWordToBit ) {
											objSearchParameter.objWordToBit.bUseWordToBit = true;
										}
									}
								}
							} else {
								CPLCMapDataParameter objParameter = new CPLCMapDataParameter();
								objParameter.strAddress = strSplitLine[ ( int )enumPLCDeviceIndex.ADDRESS ];
								objParameter.strName = strSplitLine[ ( int )enumPLCDeviceIndex.NAME ];

								// 이름이 reserve 인 경우 중복 적용이 될 위험이 있으므로 어드레스로 대체
								if( 0 <= strName.IndexOf( "RESERVE" ) ) {
									objParameter.strName = objParameter.strAddress;
								}
								objParameter.ePLCCommunicationType = eCommunicationType;

								int iDigit = Convert.ToInt32( strDigit );
								objParameter.dMultiple = Math.Pow( 10, iDigit );

								switch( eCommunicationType ) {
									case enumPLCDeviceCommunicationType.BIT_IN:
									case enumPLCDeviceCommunicationType.BIT_OUT:
										objParameter.objValue = false;
										break;
									case enumPLCDeviceCommunicationType.WORD_IN:
									case enumPLCDeviceCommunicationType.WORD_OUT:
										objParameter.objValue = ( int )0;
										break;
									case enumPLCDeviceCommunicationType.DWORD_IN:
									case enumPLCDeviceCommunicationType.DWORD_OUT:
										objParameter.objValue = 0.0;
										break;
								}

								objDataParameter.TryAdd( iIndex++, objParameter );
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				throw new ApplicationException( "Fail to read Plc Device Map Data", ex );
			}
		}

		/// <summary>
		/// 읽어온 맵 데이터를 DataTable 형식에 맞춰서 생성해서 리턴
		/// </summary>
		/// <returns></returns>
		public DataTable GetDataTable()
		{
			DataTable objDataTable = new DataTable();
			// 컬럼 추가
			for( int iLoopColumn = 0; iLoopColumn < Enum.GetNames( typeof( enumPLCDeviceIndex ) ).Length; iLoopColumn++ ) {
				switch( ( enumPLCDeviceIndex )iLoopColumn ) {
					case enumPLCDeviceIndex.ADDRESS:
					case enumPLCDeviceIndex.NAME:
					case enumPLCDeviceIndex.COMMUNICATION_TYPE:
						objDataTable.Columns.Add( ( ( enumPLCDeviceIndex )iLoopColumn ).ToString(), typeof( string ) );
						break;
					case enumPLCDeviceIndex.DIGIT:
						objDataTable.Columns.Add( ( ( enumPLCDeviceIndex )iLoopColumn ).ToString(), typeof( int ) );
						break;
				}
			}
			objDataTable.Columns.Add( "ORI_VALUE", typeof( object ) );
			objDataTable.Columns.Add( "VALUE", typeof( object ) );

			// 데이터 추가
			foreach( var parameter in objDataParameter.Values ) {
				if( 1 == parameter.dMultiple ) {
					objDataTable.Rows.Add(
					parameter.strAddress,
					parameter.strName,
					parameter.ePLCCommunicationType.ToString(),
					( int )Math.Log10( parameter.dMultiple ),
					parameter.objValue,
					parameter.objValue );
				} else {
					double dTemp = Convert.ToDouble( parameter.objValue );
					dTemp /= parameter.dMultiple;
					objDataTable.Rows.Add(
					parameter.strAddress,
					parameter.strName,
					parameter.ePLCCommunicationType.ToString(),
					( int )Math.Log10( parameter.dMultiple ),
					parameter.objValue,
					dTemp );
				}
					
				// word to bit 인 경우
				if( true == parameter.objWordToBit.bUseWordToBit ) {
					for( int iLoopBit = 0; iLoopBit < parameter.objWordToBit.objBit.Length; iLoopBit++ ) {
						string strBitAddress = $"{parameter.strAddress}.{iLoopBit:X}";
						string strBitName = parameter.objWordToBit.objBit[ iLoopBit ].strName;
						objDataTable.Rows.Add(
							strBitAddress,
							strBitName,
							enumPLCDeviceCommunicationType.WORD_TO_BIT_NAME.ToString(),
							0,
							parameter.objWordToBit.objBit[ iLoopBit ].bBit,
							parameter.objWordToBit.objBit[ iLoopBit ].bBit );
					}
				}
			}
			return objDataTable;
		}

		/// <summary>
		/// 맵 데이터 내에 타입에 해당하는 수량 반환
		/// </summary>
		/// <param name="eCommunicationType"></param>
		/// <returns></returns>
		public int GetCount( enumPLCDeviceCommunicationType eCommunicationType )
		{
			return objDataParameter.Values.Count( param => param.ePLCCommunicationType == eCommunicationType );
		}

		/// <summary>
		/// 리스트 인덱스에 해당하는 맵 데이터를 get
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="iFindIndex"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		private bool GetValue<T>( int iFindIndex, ref T[] objValue )
		{
			for( int iLoopCount = 0; iLoopCount < objValue.Length; iLoopCount++ ) {
				if( iFindIndex + iLoopCount >= objDataParameter.Count ) {
					return false;
				}
				if( false == objDataParameter.TryGetValue( iFindIndex + iLoopCount, out CPLCMapDataParameter objSearchParameter ) ) {
					return false;
				}

				if( typeof( T ) == typeof( bool ) ) {
					// set value 가 bool 타입이고 해당 영역 설정도 BIT 인지 확인
					if( enumPLCDeviceCommunicationType.BIT_IN == objSearchParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.BIT_OUT == objSearchParameter.ePLCCommunicationType ) {
						if( typeof( bool ) == objSearchParameter.objValue.GetType() ) {
							objValue[ iLoopCount ] = ( T )( object )Convert.ToBoolean( objSearchParameter.objValue );
						}
					}
				} else if( typeof( T ) == typeof( int ) ) {
					// set value 가 int 타입이고 해당 영역 설정도 WORD 인지 확인
					if( enumPLCDeviceCommunicationType.WORD_IN == objSearchParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objSearchParameter.ePLCCommunicationType ) {
						if( typeof( int ) == objSearchParameter.objValue.GetType() ) {
							objValue[ iLoopCount ] = ( T )( object )Convert.ToInt32( objSearchParameter.objValue );
						}
					}
				} else if( typeof( T ) == typeof( double ) ) {
					// set value 가 double 타입이고 해당 영역 설정도 DWORD 인지 확인
					if( enumPLCDeviceCommunicationType.DWORD_IN == objSearchParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objSearchParameter.ePLCCommunicationType ) {
						if( typeof( double ) == objSearchParameter.objValue.GetType() ) {
							objValue[ iLoopCount ] = ( T )( object )Convert.ToDouble( objSearchParameter.objValue );
						}
					}
				} else if( typeof( T ) == typeof( CPLCMapDataParameterWordToBit ) ) {
					// set value 가 CPLCDeviceMapDataParameterWordToBit 타입이고 해당 영역 설정도 WORD 인지 확인
					if( enumPLCDeviceCommunicationType.WORD_IN == objSearchParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objSearchParameter.ePLCCommunicationType ) {
						objValue[ iLoopCount ] = ( T )objSearchParameter.objWordToBit.Clone();
					}
				}
			}
			return true;
		}

		/// <summary>
		/// 리스트에서 이름에 해당하는 영역에 맵 데이터를 get
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strName"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool GetValueName<T>( string strName, ref T objValue )
		{
			int iFindIndex = objDataParameter.Values.ToList().FindIndex( obj => obj.strName == strName );
			if( -1 == iFindIndex ) {
				return false;
			}
			T[] objTemp = new T[ 1 ];
			if( false == GetValue( iFindIndex, ref objTemp ) ) {
				return false;
			}
			objValue = objTemp[ 0 ];
			return true;
		}

		/// <summary>
		/// 리스트에서 이름에 해당하는 영역에 맵 데이터를 get
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strName"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool GetValueName<T>( string strName, ref T[] objValue )
		{
			int iFindIndex = objDataParameter.Values.ToList().FindIndex( obj => obj.strName == strName );
			if( -1 == iFindIndex ) {
				return false;
			}
			if( false == GetValue( iFindIndex, ref objValue ) ) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// 리스트에서 어드레스에 해당하는 영역에 맵 데이터를 get
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strAddress"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool GetValueAddress<T>( string strAddress, ref T objValue )
		{
			int iFindIndex = objDataParameter.Values.ToList().FindIndex( obj => obj.strAddress == strAddress );
			if( -1 == iFindIndex ) {
				return false;
			}
			T[] objTemp = new T[ 1 ];
			if( false == GetValue( iFindIndex, ref objTemp ) ) {
				return false;
			}
			objValue = objTemp[ 0 ];
			return true;
		}

		/// <summary>
		/// 리스트에서 어드레스에 해당하는 영역에 맵 데이터를 get
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strAddress"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool GetValueAddress<T>( string strAddress, ref T[] objValue )
		{
			int iFindIndex = objDataParameter.Values.ToList().FindIndex( obj => obj.strAddress == strAddress );
			if( -1 == iFindIndex ) {
				return false;
			}
			if( false == GetValue( iFindIndex, ref objValue ) ) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// 리스트 인덱스에 해당하는 맵 데이터를 set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="iFindIndex"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		private bool SetValue<T>( int iFindIndex, T[] objValue )
		{
			lock( objDataParameter ) {
				// 맵 데이터가 변경될 시 데이터 전달
				List<CPLCMapDataChanged> objMapDataChanged = new List<CPLCMapDataChanged>();

				for( int iLoopCount = 0; iLoopCount < objValue.Length; iLoopCount++ ) {
					int iKey = iFindIndex + iLoopCount;
					if( iKey >= objDataParameter.Count ) {
						return false;
					}
					if( false == objDataParameter.TryGetValue( iKey, out CPLCMapDataParameter objSearchParameter ) ) {
						return false;
					}

					if( typeof( T ) == typeof( bool ) ) {
						// set value 가 bool 타입이고 해당 영역 설정도 BIT 인지 확인
						if( enumPLCDeviceCommunicationType.BIT_IN == objSearchParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.BIT_OUT == objSearchParameter.ePLCCommunicationType ) {
							bool bValue = Convert.ToBoolean( objValue[ iLoopCount ] );
							if( ( bool )objSearchParameter.objValue != bValue ) {
								objSearchParameter.objValue = bValue;
								objMapDataChanged.Add( new CPLCMapDataChanged( objSearchParameter.strAddress, bValue, bValue ) );
							}
						}
					} else if( typeof( T ) == typeof( int ) ) {
						// set value 가 int 타입이고 해당 영역 설정도 WORD 인지 확인
						if( enumPLCDeviceCommunicationType.WORD_IN == objSearchParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objSearchParameter.ePLCCommunicationType ) {
							int iValue = Convert.ToInt32( objValue[ iLoopCount ] );
							if( ( int )objSearchParameter.objValue != iValue ) {
								objSearchParameter.objValue = iValue;
								
								if( 1 == objSearchParameter.dMultiple ) {
									objMapDataChanged.Add( new CPLCMapDataChanged( objSearchParameter.strAddress, iValue, iValue ) );
								} else {
									double dTemp = iValue;
									dTemp /= objSearchParameter.dMultiple;
									objMapDataChanged.Add( new CPLCMapDataChanged( objSearchParameter.strAddress, iValue, dTemp ) );
								}

								// WORD 를 비트 영역으로 쪼개서 갖고 있는다.
								for( int iLoopBit = 0; iLoopBit < objSearchParameter.objWordToBit.objBit.Length; iLoopBit++ ) {
									bool bTemp = ( ( iValue >> ( iLoopBit % 16 ) ) & 0x01 ) > 0 ? true : false;
									if( objSearchParameter.objWordToBit.objBit[ iLoopBit ].bBit != bTemp ) {
										objSearchParameter.objWordToBit.objBit[ iLoopBit ].bBit = bTemp;
										if( true == objSearchParameter.objWordToBit.bUseWordToBit ) {
											objMapDataChanged.Add( new CPLCMapDataChanged( $"{objSearchParameter.strAddress}.{iLoopBit:X}", bTemp, bTemp ) );
										}
									}
								}
							}
						}
					} else if( typeof( T ) == typeof( double ) ) {
						// set value 가 double 타입이고 해당 영역 설정도 DWORD 인지 확인
						if( enumPLCDeviceCommunicationType.DWORD_IN == objSearchParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.DWORD_OUT == objSearchParameter.ePLCCommunicationType ) {
							double dValue = Convert.ToDouble( objValue[ iLoopCount ] );
							if( ( double )objSearchParameter.objValue != dValue ) {
								objSearchParameter.objValue = dValue;
								objMapDataChanged.Add( new CPLCMapDataChanged( objSearchParameter.strAddress, dValue, dValue / objSearchParameter.dMultiple ) );
							}
						}
					} else if( typeof( T ) == typeof( CPLCMapDataParameterWordToBit ) ) {
						// set value 가 CPLCDeviceMapDataParameterWordToBit 타입이고 해당 영역 설정도 WORD 인지 확인
						if( enumPLCDeviceCommunicationType.WORD_IN == objSearchParameter.ePLCCommunicationType || enumPLCDeviceCommunicationType.WORD_OUT == objSearchParameter.ePLCCommunicationType ) {
							int iTemp = 0;
							for( int iLoopBit = 0; iLoopBit < objSearchParameter.objWordToBit.objBit.Length; iLoopBit++ ) {
								if( objSearchParameter.objWordToBit.objBit[ iLoopBit ].bBit != ( objValue[ iLoopCount ] as CPLCMapDataParameterWordToBit ).objBit[ iLoopBit ].bBit ) {
									objSearchParameter.objWordToBit.objBit[ iLoopBit ].bBit = ( objValue[ iLoopCount ] as CPLCMapDataParameterWordToBit ).objBit[ iLoopBit ].bBit;
									if( true == objSearchParameter.objWordToBit.bUseWordToBit ) {
										objMapDataChanged.Add( new CPLCMapDataChanged( $"{objSearchParameter.strAddress}.{iLoopBit:X}", objSearchParameter.objWordToBit.objBit[ iLoopBit ].bBit, objSearchParameter.objWordToBit.objBit[ iLoopBit ].bBit ) );
									}
								}
								if( true == objSearchParameter.objWordToBit.objBit[ iLoopBit ].bBit ) {
									iTemp |= 1 << iLoopBit;
								}
							}
							if( ( int )objSearchParameter.objValue != iTemp ) {
								objSearchParameter.objValue = iTemp;
								objMapDataChanged.Add( new CPLCMapDataChanged( objSearchParameter.strAddress, iTemp, iTemp ) );
							}
						}
					}
				}
				if( 0 != objMapDataChanged.Count ) {
					// fire map data changed event
					_callBackMapDataChanged?.Invoke( objMapDataChanged.ToArray() );
				}
			}
			return true;
		}

		/// <summary>
		/// 리스트에서 이름에 해당하는 영역에 맵 데이터를 set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strName"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool SetValueName<T>( string strName, T objValue )
		{
			int iFindIndex = objDataParameter.Values.ToList().FindIndex( obj => obj.strName == strName );
			if( -1 == iFindIndex ) {
				return false;
			}
			T[] objTemp = new T[ 1 ];
			objTemp[ 0 ] = objValue;
			if( false == SetValue( iFindIndex, objTemp ) ) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// 리스트에서 이름에 해당하는 영역에 맵 데이터를 set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strName"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool SetValueName<T>( string strName, T[] objValue )
		{
			int iFindIndex = objDataParameter.Values.ToList().FindIndex( obj => obj.strName == strName );
			if( -1 == iFindIndex ) {
				return false;
			}
			if( false == SetValue( iFindIndex, objValue ) ) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// 리스트에서 어드레스에 해당하는 영역에 맵 데이터를 set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strAddress"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool SetValueAddress<T>( string strAddress, T objValue )
		{
			int iFindIndex = objDataParameter.Values.ToList().FindIndex( obj => obj.strAddress == strAddress );
			if( -1 == iFindIndex ) {
				return false;
			}
			T[] objTemp = new T[ 1 ];
			objTemp[ 0 ] = objValue;
			if( false == SetValue( iFindIndex, objTemp ) ) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// 리스트에서 어드레스에 해당하는 영역에 맵 데이터를 set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strAddress"></param>
		/// <param name="objValue"></param>
		/// <returns></returns>
		public bool SetValueAddress<T>( string strAddress, T[] objValue )
		{
			int iFindIndex = objDataParameter.Values.ToList().FindIndex( obj => obj.strAddress == strAddress );
			if( -1 == iFindIndex ) {
				return false;
			}
			if( false == SetValue( iFindIndex, objValue ) ) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// 리스트에서 이름에 해당하는 인덱스를 get
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="iFindIndex"></param>
		/// <returns></returns>
		public bool GetFindIndexWithName( string strName, ref int iFindIndex )
		{
			iFindIndex = objDataParameter.Values.ToList().FindIndex( obj => obj.strName == strName );
			if( -1 == iFindIndex ) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// 리스트에서 어드레스에 해당하는 인덱스를 get
		/// </summary>
		/// <param name="strAddress"></param>
		/// <param name="iFindIndex"></param>
		/// <returns></returns>
		public bool GetFindIndexWithAddress( string strAddress, ref int iFindIndex )
		{
			iFindIndex = objDataParameter.Values.ToList().FindIndex( obj => obj.strAddress == strAddress );
			if( -1 == iFindIndex ) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// 리스트에서 이름에 해당하는 맵 데이터 객체를 get
		/// </summary>
		/// <param name="strName"></param>
		/// <returns></returns>
		public CPLCMapDataParameter GetParameterWithName( string strName )
		{
			return objDataParameter.Values.FirstOrDefault( obj => obj.strName == strName )?.Clone() as CPLCMapDataParameter;
		}

		/// <summary>
		/// 리스트에서 어드레스에 해당하는 맵 데이터 객체를 get
		/// </summary>
		/// <param name="strAddress"></param>
		/// <returns></returns>
		public CPLCMapDataParameter GetParameterWithAddress( string strAddress )
		{
			return objDataParameter.Values.FirstOrDefault( obj => obj.strAddress == strAddress )?.Clone() as CPLCMapDataParameter;
		}

		/// <summary>
		/// 리스트에서 인덱스에 해당하는 맵 데이터 객체를 get
		/// </summary>
		/// <param name="iIndex"></param>
		/// <returns></returns>
		public CPLCMapDataParameter GetParameterWithIndex( int iIndex )
		{
			return objDataParameter[ iIndex ].Clone() as CPLCMapDataParameter;
		}
	}
}