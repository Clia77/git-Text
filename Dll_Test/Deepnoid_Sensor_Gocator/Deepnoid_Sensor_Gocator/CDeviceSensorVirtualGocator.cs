using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Deepnoid_Sensor_Gocator
{
	public class CDeviceSensorVirtualGocator : CDeviceSensorAbstract
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
		public CDeviceSensorVirtualGocator()
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
		public override bool Initialize( CInitializeParameterGocator objInitializeParameter )
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
					string strImagePath = m_strImagePath;

					m_lstImageData.Clear();

					// 폴더 없는 경우 나가기
					System.IO.DirectoryInfo di = new System.IO.DirectoryInfo( strImagePath );
					if( di.Exists == false ) break;

					// 폴더 내 파일 목록 불러오기
					string[] arFileNames = System.IO.Directory.GetFiles( strImagePath );

					List<byte[]> objListDataIntensity = new List<byte[]>();
					List<short[]> objListDataHeight = new List<short[]>();
					List<double[]> objListDoubleDataHeight = new List<double[]>();
					List<int> objListSizeWidth = new List<int>();
					List<int> objListSizeHeight = new List<int>();

					List<int> objListSizeResolutionX = new List<int>();
					List<int> objListSizeResolutionY = new List<int>();
					List<int> objListSizeResolutionZ = new List<int>();
					List<int> objListSizeOffsetX = new List<int>();
					List<int> objListSizeOffsetY = new List<int>();
					List<int> objListSizeOffsetZ = new List<int>();

					// 초기값 설정
					{
						for( int i = 0; i < ( arFileNames.Length - 1 ) / 2; i++ ) {
							objListDataIntensity.Add( new byte[ 10 ] );
							objListDataHeight.Add( new short[ 10 ] );
							objListDoubleDataHeight.Add( new double[ 10 ] );
							objListSizeWidth.Add( 10 );
							objListSizeHeight.Add( 10 );
							objListSizeResolutionX.Add( 10 );
							objListSizeResolutionY.Add( 10 );
							objListSizeResolutionZ.Add( 10 );
							objListSizeOffsetX.Add( 10 );
							objListSizeOffsetY.Add( 10 );
							objListSizeOffsetZ.Add( 10 );
						}
					}

					int iPosition = 0;
					// 폴더 내의 파일 수만큼 루프
					for( int iLoopCount = 0; iLoopCount < arFileNames.Length; iLoopCount++ ) {
						string[] strData = arFileNames[ iLoopCount ].Split( '_' );
						// 5개 아니면 지정한 파일 아님
						if( 6 != strData.Length ) continue;

						try {
							if( -1 != arFileNames[ iLoopCount ].IndexOf( "Height.ini" ) ) {
								FileStream objFileStream = new FileStream( arFileNames[ iLoopCount ], FileMode.Open, FileAccess.Read );
								StreamReader objStreamReader = new StreamReader( objFileStream, Encoding.UTF8 );

								string strReadData = "";
								List<string> objListReadData = new List<string>();
								while( ( strReadData = objStreamReader.ReadLine() ) != null ) {
									objListReadData.Add( strReadData );
								}

								string[] strDataLine = objListReadData[ 0 ].Split( ':' );
								objListSizeResolutionX[ iPosition ] = ( int.Parse( strDataLine[ 1 ] ) );
								strDataLine = objListReadData[ 1 ].Split( ':' );
								objListSizeResolutionY[ iPosition ] = ( int.Parse( strDataLine[ 1 ] ) );
								strDataLine = objListReadData[ 2 ].Split( ':' );
								objListSizeResolutionZ[ iPosition ] = ( int.Parse( strDataLine[ 1 ] ) );
								strDataLine = objListReadData[ 3 ].Split( ':' );
								objListSizeOffsetX[ iPosition ] = ( int.Parse( strDataLine[ 1 ] ) );
								strDataLine = objListReadData[ 4 ].Split( ':' );
								objListSizeOffsetY[ iPosition ] = ( int.Parse( strDataLine[ 1 ] ) );
								strDataLine = objListReadData[ 5 ].Split( ':' );
								objListSizeOffsetZ[ iPosition++ ] = ( int.Parse( strDataLine[ 1 ] ) );
								break;
							} else {
								objListSizeResolutionX[ iPosition ] = ( 45000 );
								objListSizeResolutionY[ iPosition ] = ( 45000 );
								objListSizeResolutionZ[ iPosition ] = ( 300 );
								objListSizeOffsetX[ iPosition ] = ( 0 );
								objListSizeOffsetY[ iPosition ] = ( 0 );
								objListSizeOffsetZ[ iPosition ] = ( -500 );
							}
						}
						catch( Exception ex ) {
							string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
							string strMethodName = MethodBase.GetCurrentMethod()?.Name;
							string strException = $"{strClassName} {strMethodName} : {ex.Message}";
							_callBackExceptionMessage?.Invoke( strException );
						}
					}

					iPosition = 0;
					// 폴더 내의 파일 수만큼 루프
					for( int iLoopCount = 0; iLoopCount < arFileNames.Length; iLoopCount++ ) {
						string[] strData = arFileNames[ iLoopCount ].Split( '_' );
						// 5개 아니면 지정한 파일 아님
						if( 6 != strData.Length ) continue;

						// 높이 데이터 파일
						if( -1 != arFileNames[ iLoopCount ].IndexOf( "Height" ) ) {
							// Tif 확장자 확인.
							if( -1 != arFileNames[ iLoopCount ].IndexOf( ".tif" ) ) {
								// tif 데이터 파일 읽기.
								OpenCvSharp.Mat objTifRead = new OpenCvSharp.Mat();
								objTifRead = OpenCvSharp.Cv2.ImRead( arFileNames[ iLoopCount ], OpenCvSharp.ImreadModes.AnyDepth );
								objTifRead.ConvertTo( objTifRead, OpenCvSharp.MatType.CV_16SC1, 1, -32768 );

								// 데이터 1차원 배열로 넣기.
								objListDataHeight[ iPosition ] = new short[ objTifRead.Rows * objTifRead.Cols ];
								objTifRead.GetArray( 0, 0, objListDataHeight[ iPosition ] );

								//칼리브레이션한 surface 데이터 저장
								objListDoubleDataHeight[ iPosition ] = new double[ objTifRead.Rows * objTifRead.Cols ];
								double Zmax = double.MinValue;
								double Zmin = double.MaxValue;

								Parallel.For( 0, objTifRead.Height, iLoopHeight => {
									Parallel.For( 0, objTifRead.Width, iLoopWidth => {
										short val = objListDataHeight[ iPosition ][ iLoopHeight * objTifRead.Width + iLoopWidth ];
										//surface 데이터가 유효값
										if( val != short.MinValue ) {
											//칼리브레이션
											double Zval = val * ( objListSizeResolutionZ[ 0 ] / 1000000.0 ) + ( objListSizeOffsetZ[ 0 ] / 1000.0 );
											objListDoubleDataHeight[ iPosition ][ iLoopHeight * objTifRead.Width + iLoopWidth ] = Zval;

											//정규화를 위해 최소,최대값 저장
											if( Zval > Zmax ) Zmax = Zval;
											if( Zval < Zmin ) Zmin = Zval;
										}
										//surface 데이터가 무효값
										else {
											objListDoubleDataHeight[ iPosition ][ iLoopHeight * objTifRead.Width + iLoopWidth ] = -99.999;
										}
									} );
								} );

								//// 영상 크기 반대로....
								//// 영상 회전된 상태로 그대로 저장
								objListSizeWidth[ iPosition ] = objTifRead.Width;
								objListSizeHeight[ iPosition ] = objTifRead.Height;
							}
							// 기존 방식
							else if( -1 != arFileNames[ iLoopCount ].IndexOf( ".csv" ) ) {
								FileStream objFileStream = new FileStream( arFileNames[ iLoopCount ], FileMode.Open, FileAccess.Read );
								StreamReader objStreamReader = new StreamReader( objFileStream, Encoding.UTF8 );

								string strReadData = "";
								List<string> objListReadData = new List<string>();
								while( ( strReadData = objStreamReader.ReadLine() ) != null ) {
									objListReadData.Add( strReadData );
								}

								short[] objDataHeight = new short[ objListReadData.Count ];
								for( int iLoopData = 0; iLoopData < objListReadData.Count; iLoopData++ ) {
									objDataHeight[ iLoopData ] = short.Parse( objListReadData[ iLoopData ] );
								}

								objListDataHeight[ iPosition ] = objDataHeight;

								// 영상 크기
								objListSizeWidth[ iPosition ] = Int32.Parse( strData[ 3 ] );
								objListSizeHeight[ iPosition ] = Int32.Parse( strData[ 4 ] );

								objListDoubleDataHeight[ iPosition ] = new double[ objListSizeWidth[ iPosition ] * objListSizeHeight[ iPosition ] ];
								double Zmax = double.MinValue;
								double Zmin = double.MaxValue;
								for( int y = 0; y < objListSizeHeight[ iPosition ]; y++ ) //row
								{
									for( int x = 0; x < objListSizeWidth[ iPosition ]; x++ ) //column
									{
										short val = objListDataHeight[ iPosition ][ y * objListSizeWidth[ iPosition ] + x ];
										//surface 데이터가 유효값
										if( val != short.MinValue ) {
											//칼리브레이션
											double Zval = val * ( objListSizeResolutionZ[ 0 ] / 1000000.0 ) + ( objListSizeOffsetZ[ 0 ] / 1000.0 );
											objListDoubleDataHeight[ iPosition ][ y * objListSizeWidth[ iPosition ] + x ] = Zval;

											//정규화를 위해 최소,최대값 저장
											if( Zval > Zmax ) Zmax = Zval;
											if( Zval < Zmin ) Zmin = Zval;
										}
										//surface 데이터가 무효값
										else {
											objListDoubleDataHeight[ iPosition ][ y * objListSizeWidth[ iPosition ] + x ] = -99.999;
										}
									}
								}

								objStreamReader.Close();
								objFileStream.Close();
							}
						}
						// 밝기 데이터파일
						else if( -1 != arFileNames[ iLoopCount ].IndexOf( "Intesnisy" ) ) {
							// Tif 확장자 확인.
							if( -1 != arFileNames[ iLoopCount ].IndexOf( ".tif" ) ) {
								OpenCvSharp.Mat buf = new OpenCvSharp.Mat();
								buf = OpenCvSharp.Cv2.ImRead( arFileNames[ iLoopCount ], OpenCvSharp.ImreadModes.AnyDepth );
								buf.ConvertTo( buf, OpenCvSharp.MatType.CV_8UC1 );

								objListDataIntensity[ iPosition ] = new byte[ buf.Rows * buf.Cols ];
								buf.GetArray( 0, 0, objListDataIntensity[ iPosition++ ] );
							}
							// 기존 방식
							else if( -1 != arFileNames[ iLoopCount ].IndexOf( ".csv" ) ) {
								FileStream objFileStream = new FileStream( arFileNames[ iLoopCount ], FileMode.Open, FileAccess.Read );
								StreamReader objStreamReader = new StreamReader( objFileStream, Encoding.UTF8 );

								string strReadData = "";
								List<string> objListReadData = new List<string>();
								while( ( strReadData = objStreamReader.ReadLine() ) != null ) {
									objListReadData.Add( strReadData );
								}

								byte[] objDataIntensity = new byte[ objListReadData.Count ];
								for( int iLoopData = 0; iLoopData < objListReadData.Count; iLoopData++ ) {
									objDataIntensity[ iLoopData ] = byte.Parse( objListReadData[ iLoopData ] );
								}

								objListDataIntensity[ iPosition++ ] = objDataIntensity;

								objStreamReader.Close();
								objFileStream.Close();
							}
						}
					}

					// 데이터 없는 경우
					if( 0 >= objListDataIntensity.Count || 0 >= objListDataHeight.Count )
						break;

					for( int iLoopCount = 0; iLoopCount < objListDataHeight.Count; iLoopCount++ ) {
						CScanData objScanData = new CScanData();
						objScanData.iWidth = objListSizeWidth[ iLoopCount ];
						objScanData.iHeight = objListSizeHeight[ iLoopCount ];
						objScanData.objSensorDataGocator.iResolutionX = objListSizeResolutionX[ 0 ];
						objScanData.objSensorDataGocator.iResolutionY = objListSizeResolutionY[ 0 ];
						objScanData.objSensorDataGocator.iResolutionZ = objListSizeResolutionZ[ 0 ];
						objScanData.objSensorDataGocator.iOffsetX = objListSizeOffsetX[ 0 ];
						objScanData.objSensorDataGocator.iOffsetY = objListSizeOffsetY[ 0 ];
						objScanData.objSensorDataGocator.iOffsetZ = objListSizeOffsetZ[ 0 ];

						objScanData.objSensorDataGocator.iWidth = objListSizeWidth[ iLoopCount ];
						objScanData.objSensorDataGocator.iHeight = objListSizeHeight[ iLoopCount ];
						objScanData.objSensorDataGocator.objHeightDataOrigin = objListDataHeight[ iLoopCount ];
						objScanData.objSensorDataGocator.objHeightDataDoubleOrigin = objListDoubleDataHeight[ iLoopCount ];
						objScanData.objSensorDataGocator.objIntensityDataOrigin = objListDataIntensity[ iLoopCount ];
						objScanData.bGrabComplete = true;

						m_lstImageData.Add( objScanData );
					}
				}
				catch( Exception ex ) {
					string strClassName = MethodBase.GetCurrentMethod()?.DeclaringType?.Name;
					string strMethodName = MethodBase.GetCurrentMethod()?.Name;
					string strException = $"{strClassName} {strMethodName} : {ex.Message}";
					_callBackExceptionMessage?.Invoke( strException );
				}

				bReturn = true;
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
				CScanData objScanData = m_lstImageData[ m_iImageIndex ].Clone() as CScanData;
				_callBackScanData?.Invoke( objScanData.Clone() as CScanData );
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
		/// 노출값 
		/// </summary>
		/// <returns></returns>
		public double GetExposureTime()
		{
			return 100.0;
		}

		/// <summary>
		/// 노출값 설정
		/// </summary>
		/// <param name="dExposeTime"></param>
		/// <returns></returns>
		public bool SetExposureTime( double dExposeTime )
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