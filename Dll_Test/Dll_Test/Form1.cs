using Data;
using Database;
using Deepnoid_Communication;
using Deepnoid_Logger;
using Deepnoid_MemoryMap;
using Deepnoid_PLC;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Deepnoid_Logger.CDefineLog;
using static Deepnoid_PLC.CPLCDefine;
using Timer = System.Windows.Forms.Timer;

namespace Dll_Test
{
	public partial class Form1 : Form
	{
		/// <summary>
		/// plc 디바이스 컬럼
		/// </summary>
		public enum enumPLCDataTableColumn
		{
			ADDRESS = 0,
			NAME,
			COMMUNICATION_TYPE,
			DIGIT,
			ORI_VALUE,
			VALUE,
		}

		public const string DEF_PROGRAM_WAIT = "Deepnoid_Wait";
		public const string DEF_PROGRAM_LOADING_PROCESS = "Deepnoid_LoadingProcess";

		private bool m_bWaitMessage = false;
		private bool m_bLoadingProcess = false;

		private CProcessDatabase m_objProcessDatabase;

		private enumType m_eType;

		public enum enumType
		{
			TYPE_FLATNESS = 0,
			TYPE_TA,
		}

		private CPLCDevice m_objPLCDevice;
		private DataTable m_objPLCDataTable;
		private bool m_bPLCMapDataChanged;
		private bool m_bClickServer;
		private bool m_bClickClient;
		private bool m_bClickVirtual;
		private bool m_bConnect;
		private Timer m_objTimerConnect;
		private Timer m_objTimerPLCMapDataChanged;

		//private CConfig m_objConfig;

		public Form1()
		{
			InitializeComponent();

			LogManager.Instance?.WriteLog( 0, eLogLevel.LOG_LEVEL_INFORMATION, "Start Program" );

			//m_objConfig = CConfig.Instance;
			//m_objConfig.SetCallBackErrorMessage( ErrorMessageConfig );
			//m_objConfig.Initialize();
			//m_objConfig.LoadParameter();

			m_eType = enumType.TYPE_FLATNESS;

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
			}
			objParameter.strDatabasePath = "D:\\FLATNESS_DATA\\DATABASE";
			objParameter.strDatabaseHistoryName = "DATABASE_HISTORY";
			objParameter.strDatabaseInformationName = "DATABASE_INFORMATION";
			objParameter.strDatabaseTablePath = Application.StartupPath + "\\DatabaseTable";
			objParameter.strDatabaseRecordPath = Application.StartupPath + "\\DatabaseRecord";
			objParameter.strTableInformationUIText = "TABLE_INFORMATION_UI_TEXT";
			objParameter.strTableInformationUserMessage = "TABLE_INFORMATION_USER_MESSAGE";
			objParameter.strRecordInformationUIText = "RECORD_INFORMATION_UI_TEXT";
			objParameter.strRecordInformationUserMessage = "RECORD_INFORMATION_USER_MESSAGE";
			objParameter.iDatabaseDeletePeriod = 100;
			objParameter.bDatabaseDelete = false;

			m_objProcessDatabase = new CProcessDatabase();
			if( false == m_objProcessDatabase.Initialize( objParameter ) ) {
				string strError = $"Fail to database initialize";
				Debug.WriteLine( strError );
			}

			m_bClickServer = false;
			m_bClickClient = false;
			m_bClickVirtual = false;
			m_bConnect = false;
			m_bPLCMapDataChanged = false;
		}

		private void Form1_FormClosed( object sender, FormClosedEventArgs e )
		{
			m_objPLCDevice?.DeInitialize();
			m_objProcessDatabase?.DeInitialize();
			LogManager.Instance?.WriteLog( 0, eLogLevel.LOG_LEVEL_INFORMATION, "Exit Program" );
			LogManager.Instance?.DeInitiaize();
		}

		private void btnInsert_Click( object sender, EventArgs e )
		{
			// test git
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
				}
			}
		}

		private void ErrorMessageConfig( string strErrorMessage )
		{
			// ..
		}

		private void ErrorMessage( string strErrorMessage )
		{
			if( true == m_bClickServer ) {
				Trace.WriteLine( $"[Server] {strErrorMessage}" );
			} else {
				Trace.WriteLine( $"[Client] {strErrorMessage}" );
			}
		}

		private void ReceiveData( CReceiveData obj )
		{
			if( true == m_bClickServer ) {
				Trace.WriteLine( $"[Server] {obj.iByteLength}\r\n{obj.strData}" );
			} else {
				Trace.WriteLine( $"[Client] {obj.iByteLength}\r\n{obj.strData}" );
			}
		}

		private void MapDataChanged( CPLCMapDataChanged[] obj )
		{
			if( m_objPLCDataTable.Columns.Count <= ( int )enumPLCDataTableColumn.VALUE ) {
				return;
			}
			// DataTable value 에 해당하는 값 갱신시켜줌
			lock( m_objPLCDataTable ) {
				foreach( CPLCMapDataChanged item in obj ) {
					string strColumnName = m_objPLCDataTable.Columns[ ( int )enumPLCDataTableColumn.ADDRESS ].ColumnName;
					DataRow[] objDataRow = m_objPLCDataTable.Select( $"{strColumnName} = '{item.strAddress}'" );
					if( 0 < objDataRow.Length ) {
						// 일치하는 행이 있으면 VALUE 열 값을 갱신
						objDataRow[ 0 ][ ( int )enumPLCDataTableColumn.ORI_VALUE ] = item.objOriginValue;
						objDataRow[ 0 ][ ( int )enumPLCDataTableColumn.VALUE ] = item.objValue;
					}
				}
			}
			m_bPLCMapDataChanged = true;
		}

		private void Connect_Tick( object sender, EventArgs e )
		{
			if( true == m_bClickServer ) {
				if( true == m_objPLCDevice?.IsConnected() ) {
					this.btnConnectServer.BackColor = Color.LightGreen;
				} else {
					this.btnConnectServer.BackColor = Color.OrangeRed;
				}
			} else if( true == m_bClickClient ) {
				if( true == m_objPLCDevice?.IsConnected() ) {
					this.btnConnectClient.BackColor = Color.LightGreen;
				} else {
					this.btnConnectClient.BackColor = Color.OrangeRed;
				}
			} else if( true == m_bClickVirtual ) {
				if( true == m_objPLCDevice?.IsConnected() ) {
					this.btnConnectVirtual.BackColor = Color.LightGreen;
				} else {
					this.btnConnectVirtual.BackColor = Color.OrangeRed;
				}
			}
		}

		private void btnServer_Click( object sender, EventArgs e )
		{
			if( false == m_bClickServer ) {
				m_bClickServer = true;
				m_bClickClient = false;
				m_bClickVirtual = false;
				this.btnClient.Enabled = false;
				this.btnVirtual.Enabled = false;
				this.textBoxClient.Enabled = false;
				this.btnSendClient.Enabled = false;
			}
			if( true == m_bConnect ) {
				// 접속 off
				m_objTimerConnect?.Stop();
				m_objPLCDevice?.DeInitialize();
				this.btnServer.BackColor = SystemColors.ControlLightLight;
				this.btnConnectServer.BackColor = SystemColors.ControlLightLight;
			} else {
				// 접속 on
				m_objPLCDevice = new CPLCDevice( new CPLCDeviceMelsec( new CPLCInterfaceMelsecSocket() ) );
				m_objPLCDevice.SetCallBackErrorMessage( ErrorMessage );
				//m_objPLCDevice.SetCallBackReceiveData( ReceiveData );

				// 이야.. 파라매터 설정하기 좀 빡시네?
				CPLCDeviceParameter objParameter = new CPLCDeviceParameter();
				{
					CCommunicationParameterSocket objCommunicationParameterSocket = new CCommunicationParameterSocket();
					{
						objCommunicationParameterSocket.strSocketIPAddress = "127.0.0.1";
						objCommunicationParameterSocket.iSocketPortNumber = 9999;
						objCommunicationParameterSocket.eDataEncoding = CCommunicationDefine.enumDataEncoding.ENCODING_NONE;
					}
					CPLCInterfaceMelsecParameterSocket objPLCInterfaceMelsecParameterSocket = new CPLCInterfaceMelsecParameterSocket();
					{
						objPLCInterfaceMelsecParameterSocket.objParameter = new CCommunicationParameter( objCommunicationParameterSocket );
						objPLCInterfaceMelsecParameterSocket.eSocketType = CPLCDefine.enumSocketType.SOCKET_TYPE_SERVER;
						objPLCInterfaceMelsecParameterSocket.eSeriseType = CPLCDefine.enumSeriseType.SERISE_TYPE_Q;
						objPLCInterfaceMelsecParameterSocket.eProtocolType = CPLCDefine.enumProtocolType.PROTOCOL_TYPE_BINARY;
					}
					objParameter.objParameter = new CPLCInterfaceMelsecParameter( objPLCInterfaceMelsecParameterSocket );
					objParameter.strMapDataPath = Directory.GetCurrentDirectory() + "\\AddressMap\\PLCMap.txt";
				}

				bool bInitialize = m_objPLCDevice.Initialize( objParameter );
				if( true == bInitialize ) {
					this.btnServer.BackColor = SystemColors.ControlDark;
				} else {
					this.btnServer.BackColor = Color.OrangeRed;
				}

				CPLCDeviceMonitoringParameter objMonitoringParameter = new CPLCDeviceMonitoringParameter();
				objMonitoringParameter.iThreadPeriod = 1000;
				// read address plc
				{
					CPLCDeviceMonitoringParameterList objMonitoringParameterList = new CPLCDeviceMonitoringParameterList();
					objMonitoringParameterList.eRWType = CPLCDefine.enumPLCDeviceRWType.READ_WORD;
					objMonitoringParameterList.strName = "PLC_EVENT";
					objMonitoringParameterList.iCount = m_objPLCDevice.GetMapDataCount( CPLCDefine.enumPLCDeviceCommunicationType.WORD_IN );
					objMonitoringParameter.objParameterList.Add( objMonitoringParameterList );
				}
				// write address pc
				{
					CPLCDeviceMonitoringParameterList objMonitoringParameterList = new CPLCDeviceMonitoringParameterList();
					objMonitoringParameterList.eRWType = CPLCDefine.enumPLCDeviceRWType.WRITE_WORD;
					objMonitoringParameterList.strName = "PC_EVENT";
					objMonitoringParameterList.iCount = m_objPLCDevice.GetMapDataCount( CPLCDefine.enumPLCDeviceCommunicationType.WORD_OUT );
					objMonitoringParameter.objParameterList.Add( objMonitoringParameterList );
				}
				m_objPLCDevice.StartMonitoring( objMonitoringParameter );

				if( null == m_objTimerConnect ) {
					m_objTimerConnect = new Timer();
					m_objTimerConnect.Interval = 100;
					m_objTimerConnect.Tick += Connect_Tick;
				}
				m_objTimerConnect.Start();
			}
			m_bConnect = !m_bConnect;
		}

		private void btnClient_Click( object sender, EventArgs e )
		{
			if( false == m_bClickClient ) {
				m_bClickClient = true;
				m_bClickServer = false;
				m_bClickVirtual = false;
				this.btnServer.Enabled = false;
				this.btnVirtual.Enabled = false;
				this.textBoxServer.Enabled = false;
				this.btnSendServer.Enabled = false;
			}
			if( true == m_bConnect ) {
				// 접속 off
				m_objTimerConnect?.Stop();
				m_objPLCDevice?.DeInitialize();
				this.btnClient.BackColor = SystemColors.ControlLightLight;
				this.btnConnectClient.BackColor = SystemColors.ControlLightLight;
			} else {
				// 접속 on
				m_objPLCDevice = new CPLCDevice( new CPLCDeviceMelsec( new CPLCInterfaceMelsecSocket() ) );
				m_objPLCDevice.SetCallBackErrorMessage( ErrorMessage );
				m_objPLCDevice.SetCallBackMapDataChanged( MapDataChanged );
				//m_objPLCDevice.SetCallBackReceiveData( ReceiveData );

				CPLCDeviceParameter objParameter = new CPLCDeviceParameter();
				{
					CCommunicationParameterSocket objCommunicationParameterSocket = new CCommunicationParameterSocket();
					{
						objCommunicationParameterSocket.strSocketIPAddress = "127.0.0.1";
						objCommunicationParameterSocket.iSocketPortNumber = 9999;
						objCommunicationParameterSocket.eDataEncoding = CCommunicationDefine.enumDataEncoding.ENCODING_NONE;
					}
					CPLCInterfaceMelsecParameterSocket objPLCInterfaceMelsecParameterSocket = new CPLCInterfaceMelsecParameterSocket();
					{
						objPLCInterfaceMelsecParameterSocket.objParameter = new CCommunicationParameter( objCommunicationParameterSocket );
						objPLCInterfaceMelsecParameterSocket.eSocketType = CPLCDefine.enumSocketType.SOCKET_TYPE_CLIENT;
						objPLCInterfaceMelsecParameterSocket.eSeriseType = CPLCDefine.enumSeriseType.SERISE_TYPE_Q;
						objPLCInterfaceMelsecParameterSocket.eProtocolType = CPLCDefine.enumProtocolType.PROTOCOL_TYPE_BINARY;
					}
					objParameter.objParameter = new CPLCInterfaceMelsecParameter( objPLCInterfaceMelsecParameterSocket );
					objParameter.strMapDataPath = Directory.GetCurrentDirectory() + "\\AddressMap\\PLCMap.txt";
				}

				bool bInitialize = m_objPLCDevice.Initialize( objParameter );
				if( true == bInitialize ) {
					this.btnClient.BackColor = SystemColors.ControlDark;
				} else {
					this.btnClient.BackColor = Color.OrangeRed;
				}

				CPLCDeviceMonitoringParameter objMonitoringParameter = new CPLCDeviceMonitoringParameter();
				objMonitoringParameter.iThreadPeriod = 10;
				// read address plc
				{
					CPLCDeviceMonitoringParameterList objMonitoringParameterList = new CPLCDeviceMonitoringParameterList();
					objMonitoringParameterList.eRWType = CPLCDefine.enumPLCDeviceRWType.READ_WORD;
					objMonitoringParameterList.strName = "PLC_EVENT";
					objMonitoringParameterList.iCount = m_objPLCDevice.GetMapDataCount( CPLCDefine.enumPLCDeviceCommunicationType.WORD_IN );
					objMonitoringParameter.objParameterList.Add( objMonitoringParameterList );
				}
				// write address pc
				{
					CPLCDeviceMonitoringParameterList objMonitoringParameterList = new CPLCDeviceMonitoringParameterList();
					objMonitoringParameterList.eRWType = CPLCDefine.enumPLCDeviceRWType.WRITE_WORD;
					objMonitoringParameterList.strName = "PC_EVENT";
					objMonitoringParameterList.iCount = m_objPLCDevice.GetMapDataCount( CPLCDefine.enumPLCDeviceCommunicationType.WORD_OUT );
					objMonitoringParameter.objParameterList.Add( objMonitoringParameterList );
				}
				m_objPLCDevice.StartMonitoring( objMonitoringParameter );

				if( null == m_objTimerConnect ) {
					m_objTimerConnect = new Timer();
					m_objTimerConnect.Interval = 100;
					m_objTimerConnect.Tick += Connect_Tick;
				}
				m_objTimerConnect.Start();

				m_objPLCDataTable = m_objPLCDevice.GetDataTable();
				InitializeGridView( this.dataGridViewPLC, m_objPLCDataTable );

				if( null == m_objTimerPLCMapDataChanged ) {
					m_objTimerPLCMapDataChanged = new Timer();
					m_objTimerPLCMapDataChanged.Interval = 100;
					m_objTimerPLCMapDataChanged.Tick += M_PLCMapDataChanged_Tick;
				}
				m_objTimerPLCMapDataChanged.Start();
			}
			m_bConnect = !m_bConnect;
		}

		private void btnSendServer_Click( object sender, EventArgs e )
		{
			if( null == this.textBoxServer || "" == this.textBoxServer.Text ) {
				return;
			}
			if( false == m_objPLCDevice?.IsConnected() ) {
				return;
			}
			string strData = "testServer";
			byte[] byteData = Encoding.Default.GetBytes( strData );
			int[] iData = new int[ byteData.Length / 2 ];
			Buffer.BlockCopy( byteData, 0, iData, 0, byteData.Length );

			m_objPLCDevice?.SetValueName( "PLC_CELL_ID_1", iData );
		}

		private void btnSendClient_Click( object sender, EventArgs e )
		{
			//if( null == this.textBoxClient || "" == this.textBoxClient.Text ) {
			//	return;
			//}
			if( false == m_objPLCDevice?.IsConnected() ) {
				return;
			}

			m_objPLCDevice?.SetValueName( "PC_EVENT", 3 );
			//m_objPLCDevice?.WriteWord( "PC_EVENT", 1 );

			// write word
			{
				CPLCMapDataParameterWordToBit objWordToBit = new CPLCMapDataParameterWordToBit();
				m_objPLCDevice?.GetValueName( "PC_EVENT", ref objWordToBit );
				objWordToBit[ "PC_ALIVE" ].bBit = !objWordToBit[ "PC_ALIVE" ].bBit;
				objWordToBit[ "PC_READY" ].bBit = !objWordToBit[ "PC_READY" ].bBit;
				objWordToBit[ "PC_BUSY" ].bBit = !objWordToBit[ "PC_BUSY" ].bBit;
				m_objPLCDevice?.SetValueName( "PC_EVENT", objWordToBit );

				double[] dValue = new double[ 3 ];
				dValue[ 0 ] = 0.1;
				dValue[ 1 ] = 0.2;
				dValue[ 2 ] = 0.3;
				m_objPLCDevice?.SetValueName( "PC_RESULT_CELL_TO_NTC", dValue );

				//m_objPLCDevice?.WriteWord( "PC_EVENT", m_objPLCDevice.GetMapDataCount( enumPLCDeviceCommunicationType.WORD_OUT ) );
			}

			// write bit
			//{
			//	bool[] bTest = { true, false };
			//	m_objPLCDevice?.SetValueName( "BIT_OUT_TEST_1", bTest );
			//	m_objPLCDevice?.WriteBit( "BIT_OUT_TEST_1", bTest.Length );
			//}

			// write doubleword
			{
				double[] dTest = { -70.123, -75.789 };
				m_objPLCDevice?.SetValueName( "DWORD_OUT_TEST_1", dTest );
				//m_objPLCDevice?.WriteDoubleWord( "DWORD_OUT_TEST_1", dTest.Length );
			}
		}

		private void btnReceiveClient_Click( object sender, EventArgs e )
		{
			CPLCMapDataParameterWordToBit objWordToBit = new CPLCMapDataParameterWordToBit();
			m_objPLCDevice?.GetValueName( "PLC_EVENT", ref objWordToBit );
			Trace.WriteLine( $"PLC_TRIGGER bit : {objWordToBit[ "PLC_TRIGGER" ].bBit}" );
			//if( false == m_objPLCDevice?.IsConnected() ) {
			//	return;
			//}

			//// read word
			//{
			//	//m_objPLCDevice?.ReadWord( "PLC_EVENT", m_objPLCDevice.GetMapDataCount( enumPLCDeviceCommunicationType.WORD_IN ) );
			//	m_objPLCDevice?.ReadWord( "WORD_IN_TEST_1", 2 );
			//}

			//double[] dValue = new double[ 2 ];
			//m_objPLCDevice?.GetValueName( "WORD_IN_TEST_1", ref dValue );

			//dValue[ 0 ] *= 1000;
			//dValue[ 1 ] *= 1000;

			//short[] sValue = { ( short )dValue[ 0 ], ( short )dValue[ 1 ] };
			//int doubleWord = ( ( sValue[ 1 ] & 0xFFFF ) << 16 ) | ( sValue[ 0 ] & 0xFFFF );

			//// read bit
			////{
			////	m_objPLCDevice?.ReadBit( "BIT_IN_TEST_1", 2 );
			////}

			//// read doubleword
			//{
			//	m_objPLCDevice?.ReadDoubleWord( "DWORD_IN_TEST_1", 1 );
			//}
		}

		private void btnVirtual_Click( object sender, EventArgs e )
		{
			if( false == m_bClickVirtual ) {
				m_bClickServer = false;
				m_bClickClient = false;
				m_bClickVirtual = true;
				this.btnClient.Enabled = false;
				this.btnServer.Enabled = false;
				this.textBoxClient.Enabled = false;
				this.btnSendClient.Enabled = false;
				this.textBoxServer.Enabled = false;
				this.btnSendServer.Enabled = false;
			}
			if( true == m_bConnect ) {
				// 접속 off
				m_objTimerConnect?.Stop();
				m_objTimerPLCMapDataChanged?.Stop();
				m_objPLCDevice?.DeInitialize();
				this.btnVirtual.BackColor = SystemColors.ControlLightLight;
				this.btnConnectVirtual.BackColor = SystemColors.ControlLightLight;
			} else {
				// 접속 on
				m_objPLCDevice = new CPLCDevice( new CPLCDeviceVirtual() );
				m_objPLCDevice.SetCallBackErrorMessage( ErrorMessage );
				m_objPLCDevice.SetCallBackMapDataChanged( MapDataChanged );
				//m_objPLCDevice.SetCallBackReceiveData( ReceiveData );

				// 이야.. 파라매터 설정하기 좀 빡시네?
				CPLCDeviceParameter objParameter = new CPLCDeviceParameter();
				{
					CCommunicationParameterSocket objCommunicationParameterSocket = new CCommunicationParameterSocket();
					//{
					//	objCommunicationParameterSocket.strSocketIPAddress = "127.0.0.1";
					//	objCommunicationParameterSocket.iSocketPortNumber = 9999;
					//	objCommunicationParameterSocket.eDataEncoding = CCommunicationDefine.enumDataEncoding.ENCODING_NONE;
					//}
					CPLCInterfaceMelsecParameterSocket objPLCInterfaceMelsecParameterSocket = new CPLCInterfaceMelsecParameterSocket();
					//{
					//	objPLCInterfaceMelsecParameterSocket.objParameter = new CCommunicationParameter( objCommunicationParameterSocket );
					//	objPLCInterfaceMelsecParameterSocket.eSocketType = CPLCDefine.enumSocketType.SOCKET_TYPE_SERVER;
					//	objPLCInterfaceMelsecParameterSocket.eSeriseType = CPLCDefine.enumSeriseType.SERISE_TYPE_Q;
					//	objPLCInterfaceMelsecParameterSocket.eProtocolType = CPLCDefine.enumProtocolType.PROTOCOL_TYPE_BINARY;
					//}
					objParameter.objParameter = new CPLCInterfaceMelsecParameter( objPLCInterfaceMelsecParameterSocket );
					objParameter.strMapDataPath = Directory.GetCurrentDirectory() + "\\AddressMap\\PLCMap.txt";
				}

				bool bInitialize = m_objPLCDevice.Initialize( objParameter );
				if( true == bInitialize ) {
					this.btnVirtual.BackColor = SystemColors.ControlDark;
				} else {
					this.btnVirtual.BackColor = Color.OrangeRed;
				}

				CPLCDeviceMonitoringParameter objMonitoringParameter = new CPLCDeviceMonitoringParameter();
				objMonitoringParameter.iThreadPeriod = 10;
				// read address plc
				{
					CPLCDeviceMonitoringParameterList objMonitoringParameterList = new CPLCDeviceMonitoringParameterList();
					objMonitoringParameterList.eRWType = CPLCDefine.enumPLCDeviceRWType.READ_WORD;
					objMonitoringParameterList.strName = "PLC_EVENT";
					objMonitoringParameterList.iCount = m_objPLCDevice.GetMapDataCount( CPLCDefine.enumPLCDeviceCommunicationType.WORD_IN );
					objMonitoringParameter.objParameterList.Add( objMonitoringParameterList );
				}
				// write address pc
				{
					CPLCDeviceMonitoringParameterList objMonitoringParameterList = new CPLCDeviceMonitoringParameterList();
					objMonitoringParameterList.eRWType = CPLCDefine.enumPLCDeviceRWType.WRITE_WORD;
					objMonitoringParameterList.strName = "PC_EVENT";
					objMonitoringParameterList.iCount = m_objPLCDevice.GetMapDataCount( CPLCDefine.enumPLCDeviceCommunicationType.WORD_OUT );
					objMonitoringParameter.objParameterList.Add( objMonitoringParameterList );
				}
				m_objPLCDevice.StartMonitoring( objMonitoringParameter );

				if( null == m_objTimerConnect ) {
					m_objTimerConnect = new Timer();
					m_objTimerConnect.Interval = 100;
					m_objTimerConnect.Tick += Connect_Tick;
				}
				m_objTimerConnect.Start();

				m_objPLCDataTable = m_objPLCDevice.GetDataTable();
				InitializeGridView( this.dataGridViewPLC, m_objPLCDataTable );

				if( null == m_objTimerPLCMapDataChanged ) {
					m_objTimerPLCMapDataChanged = new Timer();
					m_objTimerPLCMapDataChanged.Interval = 100;
					m_objTimerPLCMapDataChanged.Tick += M_PLCMapDataChanged_Tick;
				}
				m_objTimerPLCMapDataChanged.Start();
			}
			m_bConnect = !m_bConnect;
		}

		private void M_PLCMapDataChanged_Tick( object sender, EventArgs e )
		{
			if( true == m_bPLCMapDataChanged ) {
				// dataGridViewPLC 갱신
				this.dataGridViewPLC.Invoke( ( MethodInvoker )delegate {
					this.dataGridViewPLC.Invalidate();
				} );
				m_bPLCMapDataChanged = false;
			}
		}

		private void InitializeGridView( DataGridView objGridView, DataTable objDataTable )
		{
			objGridView.Rows.Clear();
			if( 0 == objGridView.Columns.Count ) {
				// 더블 버퍼링으로 속성 변경
				Type objType = objGridView.GetType();
				PropertyInfo objPropertyInfo = objType.GetProperty( "DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic );
				objPropertyInfo.SetValue( objGridView, true, null );
				// 그리드 뷰 배경색
				objGridView.BackgroundColor = Color.White;
				// 그리드 뷰 행, 열 사이즈 유저 조정 막음
				objGridView.AllowUserToResizeRows = false;
				objGridView.AllowUserToResizeColumns = false;
				// 그리드 뷰 행 머리글 없앰
				objGridView.RowHeadersVisible = false;
				// 그리드 뷰 홀수행 색 변경
				objGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
				// 마지막 행 제거
				objGridView.AllowUserToAddRows = false;
				// 그리드 뷰 칼럼 사이즈 (성능상 문제가 있어서 사이즈는 고정으로 픽스)
				objGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
				// 그리드 뷰 ReadOnly
				objGridView.ReadOnly = true;
				// 그리드 뷰 다중 선택 o
				objGridView.MultiSelect = true;
				// 가상 모드로 사용해서 빠른 처리
				objGridView.VirtualMode = true;
				// 그리드 뷰 선택 모드 (행 전체 선택)
				objGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				// 그리드 뷰 크기 변경
				objGridView.Font = new Font( objGridView.Font.Name, ( float )9.0, objGridView.Font.Style );
				// 그리드 뷰 칼럼 추가
				for( int iLoopColumn = 0; iLoopColumn < objDataTable.Columns.Count; iLoopColumn++ ) {
					objGridView.Columns.Add( objDataTable.Columns[ iLoopColumn ].ToString(), objDataTable.Columns[ iLoopColumn ].ToString() );
				}
				// 그리드 뷰 칼럼 정렬 x
				for( int iLoopColumn = 0; iLoopColumn < objGridView.Columns.Count; iLoopColumn++ ) {
					objGridView.Columns[ iLoopColumn ].SortMode = DataGridViewColumnSortMode.NotSortable;
				}

				// 헤더 이름 및 넓이 조정
				SetHeaderTextWidth( objGridView, ( int )enumPLCDataTableColumn.ADDRESS, "Address", 100 );
				SetHeaderTextWidth( objGridView, ( int )enumPLCDataTableColumn.NAME, "Name", 200 );
				SetHeaderTextWidth( objGridView, ( int )enumPLCDataTableColumn.COMMUNICATION_TYPE, "Type", 200 );
				SetHeaderTextWidth( objGridView, ( int )enumPLCDataTableColumn.DIGIT, "Digit", 50 );
				SetHeaderTextWidth( objGridView, ( int )enumPLCDataTableColumn.ORI_VALUE, "Origin Value", 100 );
				SetHeaderTextWidth( objGridView, ( int )enumPLCDataTableColumn.VALUE, "Value", 100 );

				objGridView.CellValueNeeded -= GridView_CellValueNeeded;
				objGridView.CellValueNeeded += GridView_CellValueNeeded;
				objGridView.CellDoubleClick -= GridView_CellDoubleClick;
				objGridView.CellDoubleClick += GridView_CellDoubleClick;
			}
			// 첫 행 포커스 해제
			objGridView.ClearSelection();
			// row 수량 set
			objGridView.RowCount = objDataTable.Rows.Count;
		}

		private void GridView_CellValueNeeded( object sender, DataGridViewCellValueEventArgs e )
		{
			try {
				DataTable objDataTable = m_objPLCDataTable;
				if( null == objDataTable ) {
					return;
				}
				DataRow[] objDataRow = objDataTable.Select();
				if( 0 == objDataRow.Length ) {
					return;
				}
				if( objDataRow.Length <= e.RowIndex ) {
					return;
				}
				if( objDataRow[ e.RowIndex ].ItemArray.Length <= e.ColumnIndex ) {
					return;
				}
				e.Value = objDataRow[ e.RowIndex ].ItemArray[ e.ColumnIndex ];
			}
			catch( Exception ) {
				// ...
			}
		}

		private void GridView_CellDoubleClick( object sender, DataGridViewCellEventArgs e )
		{
			try {
				// 더블 클릭한 셀이 유효한지 확인
				if( 0 > e.RowIndex || 0 > e.ColumnIndex ) {
					return;
				}
				// DataGridView의 특정 셀 값 변경
				DataGridView objGridView = sender as DataGridView;
				if( null == objGridView ) {
					return;
				}
				if( ( int )enumPLCDataTableColumn.VALUE >= objGridView.Columns.Count ) {
					return;
				}
				string strAddress = ( string )objGridView.Rows[ e.RowIndex ].Cells[ ( int )enumPLCDataTableColumn.ADDRESS ].Value;
				string strPLCDeviceCommunicationType = ( string )objGridView.Rows[ e.RowIndex ].Cells[ ( int )enumPLCDataTableColumn.COMMUNICATION_TYPE ].Value;
				if( false == Enum.TryParse( strPLCDeviceCommunicationType, out enumPLCDeviceCommunicationType ePLCDeviceCommunicationType ) ) {
					return;
				}

				switch( ePLCDeviceCommunicationType ) {
					case enumPLCDeviceCommunicationType.BIT_IN:
					case enumPLCDeviceCommunicationType.BIT_OUT:
					case enumPLCDeviceCommunicationType.WORD_TO_BIT_NAME: {
							char cSplitToken = '.';
							// 선택한 bit 인덱스 값을 뒤집음
							if( true == strAddress.Contains( cSplitToken ) ) {
								// word to bit name 인 경우
								string[] strWordToBitSplit = strAddress.Split( cSplitToken );
								if( null != strWordToBitSplit && 2 == strWordToBitSplit.Length ) {
									string strWordToBitAddress = strWordToBitSplit[ 0 ];
									string strWordToBitIndex = strWordToBitSplit[ 1 ];
									int iWordToBitIndex = Convert.ToInt32( strWordToBitIndex, 16 );

									CPLCMapDataParameterWordToBit objWordToBit = new CPLCMapDataParameterWordToBit();
									m_objPLCDevice?.GetValueAddress( strWordToBitAddress, ref objWordToBit );

									objWordToBit.objBit[ iWordToBitIndex ].bBit = !objWordToBit.objBit[ iWordToBitIndex ].bBit;

									m_objPLCDevice?.SetValueAddress( strWordToBitAddress, objWordToBit );
								}
							} else {
								// 일반 bit 인 경우
								bool bValue = false;
								m_objPLCDevice?.GetValueAddress( strAddress, ref bValue );
								m_objPLCDevice?.SetValueAddress( strAddress, !bValue );
							}
						}
						break;
					case enumPLCDeviceCommunicationType.WORD_IN:
					case enumPLCDeviceCommunicationType.WORD_OUT:
					case enumPLCDeviceCommunicationType.DWORD_IN:
					case enumPLCDeviceCommunicationType.DWORD_OUT:
						// double 인자값을 입력받음
						string strValue = string.Empty;
						if( DialogResult.OK == CInputText.Show( "Input Value", "Enter new value:", ref strValue ) ) {
							if( double.TryParse( strValue, out double dValue ) ) {
								m_objPLCDevice?.SetValueAddress( strAddress, dValue );
							} else {
								MessageBox.Show( "Invalid input. Please enter a valid double value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
							}
						}
						break;
				}
			}
			catch( Exception ) {
				// ...
			}
		}

		private void SetHeaderTextWidth( DataGridView objGridView, int iColumnIndex, string strText, int iWidth, bool bVisible = true )
		{
			objGridView.Columns[ iColumnIndex ].HeaderText = strText;
			objGridView.Columns[ iColumnIndex ].Width = iWidth;
			objGridView.Columns[ iColumnIndex ].Visible = bVisible;
		}

		private int m_iVirtualCount = 0;
		private void btnSendVirtual_Click( object sender, EventArgs e )
		{
			if( false == m_objPLCDevice?.IsConnected() ) {
				return;
			}
			if( 10 == m_iVirtualCount ) {
				m_iVirtualCount = 0;
			}
			m_iVirtualCount++;

			// ex) Write WordToBit
			{
				// name PLC_EVENT
				{
					int PLC_TRIGGER = 1;
					int PLC_MOVE_COMPLETE = 2;
					int PLC_JOB_LOAD = 3;
					int PLC_SPEC_LOAD = 4;

					CPLCMapDataParameterWordToBit objWordToBit = new CPLCMapDataParameterWordToBit();
					m_objPLCDevice?.GetValueName( "PLC_EVENT", ref objWordToBit );

					bool bTemp = false;
					if( 0 == m_iVirtualCount % 2 ) {
						bTemp = true;
					}
					objWordToBit.objBit[ PLC_TRIGGER ].bBit = bTemp;
					objWordToBit.objBit[ PLC_MOVE_COMPLETE ].bBit = !bTemp;
					objWordToBit.objBit[ PLC_JOB_LOAD ].bBit = bTemp;
					objWordToBit.objBit[ PLC_SPEC_LOAD ].bBit = !bTemp;

					m_objPLCDevice?.SetValueName( "PLC_EVENT", objWordToBit );
				}
				// address D4202
				{
					CPLCMapDataParameterWordToBit objWordToBit = new CPLCMapDataParameterWordToBit();

					bool bTemp = false;
					if( 0 == m_iVirtualCount % 2 ) {
						bTemp = true;
					}
					for( int iLoopBit = 0; iLoopBit < objWordToBit.objBit.Length; iLoopBit++ ) {
						if( 0 == iLoopBit % 2 ) {
							objWordToBit.objBit[ iLoopBit ].bBit = bTemp;
						} else {
							objWordToBit.objBit[ iLoopBit ].bBit = !bTemp;
						}
					}

					m_objPLCDevice?.SetValueAddress( "D4202", objWordToBit );
				}
			}

			// ex) Write Word
			{
				// name PLC_JOB_NUMBER
				{
					DateTime objDate = DateTime.Now;
					int[] iData = new int[ 6 ];
					int iIndex = 0;
					iData[ iIndex++ ] = objDate.Year;
					iData[ iIndex++ ] = objDate.Month;
					iData[ iIndex++ ] = objDate.Day;
					iData[ iIndex++ ] = objDate.Hour;
					iData[ iIndex++ ] = objDate.Minute;
					iData[ iIndex++ ] = objDate.Second;
					m_objPLCDevice?.SetValueName( "PLC_DATE_YEAR", iData );
				}
				// address D4302
				{
					double[] dData = new double[ 2 ];
					dData[ 0 ] = m_iVirtualCount / 100.0;
					dData[ 1 ] = dData[ 0 ] + 1;
					m_objPLCDevice?.SetValueAddress( "D4302", dData );
				}
			}

			// ex) Write Bit
			{
				// ?
			}

			// ex) Write DWord
			{
				// ?
			}

			// ex) Read WordToBit
			{
				// name PLC_EVENT
				{
					CPLCMapDataParameterWordToBit objWordToBit = new CPLCMapDataParameterWordToBit();
					m_objPLCDevice?.GetValueName( "PLC_EVENT", ref objWordToBit );
					string strTrace = "";
					foreach( var item in objWordToBit.objBit ) {
						if( true == item.bBit ) {
							strTrace += "1";
						} else {
							strTrace += "0";
						}
					}

					int iData = 0;
					m_objPLCDevice?.GetValueName( "PLC_EVENT", ref iData );
				}
				// address D4202
				{
					CPLCMapDataParameterWordToBit objWordToBit = new CPLCMapDataParameterWordToBit();
					m_objPLCDevice?.GetValueAddress( "D4202", ref objWordToBit );

					string strTrace = "";
					foreach( var item in objWordToBit.objBit ) {
						if( true == item.bBit ) {
							strTrace += "1";
						} else {
							strTrace += "0";
						}
					}

					int iData = 0;
					m_objPLCDevice?.GetValueAddress( "D4202", ref iData );
				}
			}

			// ex) Read Word
			{
				// name PLC_JOB_NUMBER
				{
					int[] iData = new int[ 6 ];
					m_objPLCDevice?.GetValueName( "PLC_DATE_YEAR", ref iData );
				}
				// address D4302
				{
					double[] dData = new double[ 2 ];
					m_objPLCDevice?.GetValueAddress( "D4302", ref dData );
				}
			}
		}
	}
}
