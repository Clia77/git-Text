using Database;
using Deepnoid_Logger;
using Deepnoid_MemoryMap;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Deepnoid_Logger.CDefineLog;
using Deepnoid_Sensor_Gocator;
using Deepnoid_Sensor_Keyence_CL3;
using Deepnoid_Sensor_Keyence_LJX8;

namespace Dll_Test
{
	public partial class Form1 : Form
	{
		public const string DEF_PROGRAM_WAIT = "Deepnoid_Wait";
		public const string DEF_PROGRAM_LOADING_PROCESS = "Deepnoid_LoadingProcess";

		private bool m_bWaitMessage = false;
		private bool m_bLoadingProcess = false;

		private CProcessDatabase m_objProcessDatabase;

        private enumType m_eType;

        CConfig m_objConfig;

		object[] m_objSensor;

        public enum enumType
		{
			TYPE_FLATNESS = 0,
			TYPE_TA,
			TYPE_TAB_PROTRUSION_1,
            TYPE_TAB_PROTRUSION_2,
            TYPE_TAB_PROTRUSION_3,
			TYPE_NTC
        }

		public Form1()
		{
			InitializeComponent();

			m_objConfig = CConfig.Instance;
			m_objConfig.Initialize();

            m_objConfig.LoadParameter();

            CLogin objLogin = new CLogin();
			objLogin.Initialize();
			objLogin.SetLogin( CConfig.enumUserAuthorityLevel.USER_AUTHORITY_LEVEL_MASTER, "1");

            CConfig.SystemParameter objSystemParameter = m_objConfig.m_objSystemParameter;


            LogManager.Instance?.WriteLog( 0, eLogLevel.LOG_LEVEL_INFORMATION, "Start Program" );

			m_eType = ( enumType )objSystemParameter.eEquipmentType;

			CDatabaseParameter objParameter = null;
			// 타입별 구성
			switch( m_eType ) {
				case enumType.TYPE_FLATNESS:
					objParameter = new CDatabaseParameter_Flatness();
					( objParameter as CDatabaseParameter_Flatness ).strTableHistoryFlatness = "TABLE_HISTORY_FLATNESS";
                    
                    break;
				case enumType.TYPE_TA:
					objParameter = new CDatabaseParameter_TA();
					( objParameter as CDatabaseParameter_TA ).strTableHistoryTA = "TABLE_HISTORY_TA";
                    
                    break;
                case enumType.TYPE_TAB_PROTRUSION_1:
                case enumType.TYPE_TAB_PROTRUSION_2:
                case enumType.TYPE_TAB_PROTRUSION_3:
                    objParameter = new CDatabaseParameter_Flatness();
                    ( objParameter as CDatabaseParameter_Flatness ).strTableHistoryFlatness = "TABLE_HISTORY_TAB_PROTRUSION";
                    m_objSensor = new object[ 2 ];
                    for ( int iLoopCount = 0; iLoopCount < m_objSensor.Length; iLoopCount++ ) {
                        m_objSensor[ iLoopCount ] = new CDeviceSensorKeyenceCL3();
                        CDeviceSensorKeyenceCL3.CInitializeParameterCL3 cInitializeParameterGocator = new CDeviceSensorKeyenceCL3.CInitializeParameterCL3();
                        cInitializeParameterGocator.iSensorIndex = iLoopCount;
                        cInitializeParameterGocator.strSensorIP = m_objConfig.GetSensorParameter().objSensors[ iLoopCount ].strIpAddress;
                        cInitializeParameterGocator.strSensorPort = m_objConfig.GetSensorParameter().objSensors[ iLoopCount ].strSerialNumber;
                        ( m_objSensor[ iLoopCount ] as CDeviceSensorKeyenceCL3 ).Initialize( cInitializeParameterGocator );
                    }
                    break;
                case enumType.TYPE_NTC:
                    objParameter = new CDatabaseParameter_Flatness();
                    ( objParameter as CDatabaseParameter_Flatness ).strTableHistoryFlatness = "TABLE_HISTORY_NTC";

					m_objSensor = new object[ 1 ];					
					for( int iLoopCount	= 0; iLoopCount< m_objSensor.Length; iLoopCount++ ) {
                        m_objSensor[ iLoopCount ] = new CDeviceSensorVirtualGocator();
                        CDeviceSensorGocator.CInitializeParameterGocator cInitializeParameterGocator = new CDeviceSensorGocator.CInitializeParameterGocator();
						cInitializeParameterGocator.strSensorIP = m_objConfig.GetSensorParameter().objSensors[ iLoopCount ].strSensorID;
                        cInitializeParameterGocator.strImagePath = m_objConfig.GetSystemParameter().strSimulationImagePath;
                        ( m_objSensor[ iLoopCount ] as CDeviceSensorGocator ).Initialize( cInitializeParameterGocator );
                    }

                    break;
            }
			objParameter.strDatabasePath = objSystemParameter.objDatabaseParameter.strDatabasePath;
			objParameter.strDatabaseHistoryName = objSystemParameter.objDatabaseParameter.strDatabaseHistoryName;
			objParameter.strDatabaseInformationName = objSystemParameter.objDatabaseParameter.strDatabaseInformationName;
			objParameter.strDatabaseTablePath = Application.StartupPath + objSystemParameter.objDatabaseParameter.strDatabaseTablePath;
			objParameter.strDatabaseRecordPath = Application.StartupPath + objSystemParameter.objDatabaseParameter.strDatabaseRecordPath;
			objParameter.strTableInformationUIText = objSystemParameter.objDatabaseParameter.strTableInformationUIText;
			objParameter.strTableInformationUserMessage = objSystemParameter.objDatabaseParameter.strTableInformationUserMessage;
			objParameter.strRecordInformationUIText = objSystemParameter.objDatabaseParameter.strRecordInformationUIText;
			objParameter.strRecordInformationUserMessage = objSystemParameter.objDatabaseParameter.strRecordInformationUserMessage;
			objParameter.iDatabaseDeletePeriod = objSystemParameter.objDatabaseParameter.iDatabaseDeletePeriod;
			objParameter.bDatabaseDelete = objSystemParameter.objDatabaseParameter.bDatabaseDelete;

			m_objProcessDatabase = new CProcessDatabase();
			if( false == m_objProcessDatabase.Initialize( objParameter ) ) {
				string strError = $"Fail to database initialize";
				Debug.WriteLine( strError );
			}
        }

		public void SetSensorMessage( string strMessage )
		{
            LogManager.Instance?.WriteLog( 0, eLogLevel.LOG_LEVEL_INFORMATION, strMessage );
        }

        public void SetSensorExceptionMessage( string strMessage )
        {
            LogManager.Instance?.WriteLog( 0, eLogLevel.LOG_LEVEL_ERROR, strMessage );
        }

  //      public void SetSensorGrabImage( CScanData objImageData )
		//{
  //          m_objImageData = objImageData;
  //          /*****************************************************************************************
		//	 * NTC 검사
		//	 * 예제
		//	 * *****************************************************************************************/
		//	/*
  //          int[] iRoiRectangle = new int[ 16 ];
  //          iRoiRectangle[ 0 ] = 461;
  //          iRoiRectangle[ 1 ] = 340;
  //          iRoiRectangle[ 2 ] = 122;
  //          iRoiRectangle[ 3 ] = 161;
  //          iRoiRectangle[ 4 ] = 378;
  //          iRoiRectangle[ 5 ] = 394;
  //          iRoiRectangle[ 6 ] = 44;
  //          iRoiRectangle[ 7 ] = 53;
  //          iRoiRectangle[ 8 ] = 372;
  //          iRoiRectangle[ 9 ] = 518;
  //          iRoiRectangle[ 10 ] = 51;
  //          iRoiRectangle[ 11 ] = 20;
  //          iRoiRectangle[ 12 ] = 166;
  //          iRoiRectangle[ 13 ] = 475;
  //          iRoiRectangle[ 14 ] = 377;
  //          iRoiRectangle[ 15 ] = 273;

  //          int[] iAlign = new int[ 2 ];
  //          iAlign[ 0 ] = 438;
  //          iAlign[ 1 ] = 684;
  //          int[] iFindPoint = new int[ 2 ];
  //          double[] dResultHeight = new double[ 4 ];
  //          Algorithm.CAlgorithm.DeepNoid3DNTCInspect( m_objImageData.objSensorDataGocator.objHeightDataDoubleOrigin, m_objImageData.objSensorDataGocator.objIntensityDataOrigin, m_objImageData.iWidth, m_objImageData.iHeight, 4, iRoiRectangle, iAlign, iFindPoint, dResultHeight, 1 );
  //          LogManager.Instance?.WriteLog( 0, eLogLevel.LOG_LEVEL_INFORMATION, $"Inspection Completed" );
		//	*/

  //          /* *****************************************************************************************
		//	 * 탭돌출 검사
		//	 * 예제
		//	 * *****************************************************************************************
		//	int iCount = m_objImageData.objSensorDataKeyenceCL3000.objListSingleData.Count;

		//	double[] adData = new double[iCount];
		//	double[] adLowData = new double[ iCount ];
		//	int[] iTabPosition = new int[ 36 ];
		//	double[] adCellTopHeight = new double[ 36 ];
  //          double[] adEmbosHeight = new double[ 36 ];
  //          double[] adFullTopHeight = new double[ 36 ];
		//	double[] adEmbosProfileData = new double[ iCount ];
		//	int[] aiEmbosPosition = new int[ 36 ];
		//	Parallel.For( 0, iCount, iLoop => {
		//		adData[ iLoop ] = m_objImageData.objSensorDataKeyenceCL3000.objListSingleData[ iLoop ].dMeasureData;
		//	} );

		//	CAlgorithmTabProtrusion.DeepNoid3DCellTapCheckHeight_USA8um_3( adData, adData.Length, 36,
		//		adLowData, iTabPosition, adCellTopHeight, adEmbosHeight, adFullTopHeight, 1, adEmbosProfileData, 800, 21, 1, aiEmbosPosition );
		//	*/
  //      }

        private void Form1_FormClosed( object sender, FormClosedEventArgs e )
		{
			foreach ( var item in m_objSensor ) {
				if( item.GetType() == typeof( CDeviceSensorGocator ) ) {
					( item as CDeviceSensorGocator )?.DeInitialize();
				} else if ( item.GetType() == typeof( CDeviceSensorKeyenceCL3 ) ) {
                    ( item as CDeviceSensorKeyenceCL3 )?.DeInitialize();
                } else if ( item.GetType() == typeof( CDeviceSensorKeyenceLJX8 ) ) {
                    ( item as CDeviceSensorKeyenceLJX8 )?.DeInitialize();
                }
            }
			
            m_objProcessDatabase?.DeInitialize();
			LogManager.Instance?.WriteLog( 0, eLogLevel.LOG_LEVEL_INFORMATION, "Exit Program" );
			LogManager.Instance?.DeInitiaize();
		}

		private void btnInsert_Click( object sender, EventArgs e )
		{
			CDatabaseReport objReport = null;
			// 타입별 구성
			switch( m_eType ) {
				case enumType.TYPE_FLATNESS: {
						objReport = new CDatabaseReport_Flatness();
						( objReport as CDatabaseReport_Flatness ).strMaterialID = "strMaterialID";
						( objReport as CDatabaseReport_Flatness ).objDateTime = DateTime.Now;
						( objReport as CDatabaseReport_Flatness ).strBarcode = "strBarcode";
						( objReport as CDatabaseReport_Flatness ).strType = "Production";
						( objReport as CDatabaseReport_Flatness ).iIndex = 1;
						( objReport as CDatabaseReport_Flatness ).strProfile = "Avg";
						( objReport as CDatabaseReport_Flatness ).strResult = "OK";
						( objReport as CDatabaseReport_Flatness ).strCaseHeight = "0.000";
						( objReport as CDatabaseReport_Flatness ).strCaseResult = "OK";
						( objReport as CDatabaseReport_Flatness ).strNgList = "NONE";
						( objReport as CDatabaseReport_Flatness ).strImagePath = "strImagePath";
						( objReport as CDatabaseReport_Flatness ).iCellCount = 28;
						string strCellData = "";
						for( int iLoopCount = 0; iLoopCount < ( objReport as CDatabaseReport_Flatness ).iCellCount; iLoopCount++ ) {
							strCellData += $"{0.0:F3}";
							if( iLoopCount != ( objReport as CDatabaseReport_Flatness ).iCellCount - 1 ) {
								strCellData += ", ";
							}
						}
					( objReport as CDatabaseReport_Flatness ).strCellData = strCellData;
						( objReport as CDatabaseReport_Flatness ).strCaseData = strCellData;

						m_objProcessDatabase.m_objDatabaseSendMessage.SetInsertHistoryFlatness( objReport );
					}
					break;
				case enumType.TYPE_TA: {
						// ta 관련 정의 ~~
					}
					break;
			}
		}

		private void btnSelect_Click( object sender, EventArgs e )
		{
			dataGridView1.DataSource = null;

			// 타입별 구성
			switch( m_eType ) {
				case enumType.TYPE_FLATNESS: {
						CManagerTable objManager = m_objProcessDatabase.m_objProcessDatabaseHistory.GetManagerTable();
						string strQuery = null;
						strQuery = string.Format( "select * from {0} ", objManager.GetTableName() );

						DataTable objDataTable = new DataTable();
						m_objProcessDatabase.m_objProcessDatabaseHistory.m_objSQLite.Reload( strQuery, ref objDataTable );
						dataGridView1.DataSource = objDataTable;
					}
					break;
				case enumType.TYPE_TA: {
						// ta 관련 정의 ~~
					}
					break;
			}
		}

		private void btnWaitMessage_Click( object sender, EventArgs e )
		{
			m_bWaitMessage = !m_bWaitMessage;

			// show wait message
			var objMemoryMap = Deepnoid_MemoryMap.CMemoryMapManager.Instance;
			objMemoryMap[ CMemoryMapManager.enumPage.WAIT_MESSAGE ].bWaitShow = m_bWaitMessage;
			objMemoryMap[ CMemoryMapManager.enumPage.WAIT_MESSAGE ].strWaitMessage = textBoxWaitMessage.Text;

			Process[] ProcessList = Process.GetProcessesByName( DEF_PROGRAM_WAIT );

			// 프로그램 실행 중이면 죽임
			if( 0 < ProcessList.Length ) {
				// 실행 중이면 강제 종료
				for( int iLoopCount = 0; iLoopCount < ProcessList.Length; iLoopCount++ ) {
					ProcessList[ iLoopCount ].Kill();
				}
			}

			if( true == m_bWaitMessage ) {
				Process objProcess = new Process();
				objProcess.StartInfo.FileName = DEF_PROGRAM_WAIT + ".exe";
				objProcess.StartInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
				objProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
				objProcess.Start();
			}
		}

		private void btnLoadingScreen_Click( object sender, EventArgs e )
		{
			m_bLoadingProcess = !m_bLoadingProcess;

			// show loading process
			var objMemoryMap = Deepnoid_MemoryMap.CMemoryMapManager.Instance;
			objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].strProgramName = "TEST_PROGRAM";
			objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].strProgramVersion = "TEST_VERSION";
			objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].iProgressIndex = 0;
			objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].strProgressMessage = "WAITING...";
			objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].iMessageType = 1;

			Process[] ProcessList = Process.GetProcessesByName( DEF_PROGRAM_LOADING_PROCESS );

			// 프로그램 실행 중이면 죽임
			if( 0 < ProcessList.Length ) {
				// 실행 중이면 강제 종료
				for( int iLoopCount = 0; iLoopCount < ProcessList.Length; iLoopCount++ ) {
					ProcessList[ iLoopCount ].Kill();
				}
			}

			if( true == m_bLoadingProcess ) {
				Process objProcess = new Process();
				objProcess.StartInfo.FileName = DEF_PROGRAM_LOADING_PROCESS + ".exe";
				objProcess.StartInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
				objProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
				objProcess.Start();

				{

					System.Action action = () => {
						int iProcess = 0;
						while( 100 >= iProcess ) {
							if( false == m_bLoadingProcess ) {
								break;
							}
							objMemoryMap[ CMemoryMapManager.enumPage.LOADING_PROCESS ].iProgressIndex = iProcess;

							iProcess += 2;
							Thread.Sleep( 100 );
						}
                    };
					Task objTask = Task.Factory.StartNew( action );
                    Task.WaitAll( objTask );
					objProcess.Kill();
                }				
			}
		}

        private void BtnScanStart_Click( object sender, EventArgs e )
        {
			//( m_objSensor[ 0 ] as CDeviceSensorGocator )?.ScanStart();
        }
        private void BtnScanStop_Click( object sender, EventArgs e )
        {
            
        }

        private void BtnMeasure_Click( object sender, EventArgs e )
        {
           
        }
    }
}
