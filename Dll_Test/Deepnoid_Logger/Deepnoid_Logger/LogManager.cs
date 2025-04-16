using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Deepnoid_Logger {
    public  class LogManager {
        /// <summary>
        /// 싱글톤 패턴 Lazy의 경우 호출 전까지 생성 지연시킴.
        /// </summary>
        private static readonly Lazy<LogManager> _instance = new Lazy<LogManager>(() => new LogManager());
        /// <summary>
        /// 싱글톤 패턴
        /// </summary>
        public static LogManager Instance => _instance.Value;
        /// <summary>
        /// 로그 객체
        /// </summary>
        private List< ILogger > _logger = null;
        /// <summary>
        /// 설정 객체
        /// </summary>
        private LoggingConfiguration m_objLoggingConfiguration;        
        /// <summary>
        /// 생성자
        /// </summary>
        public LogManager() { Initialize(); }
        /// <summary>
        /// 초기화
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        private bool Initialize()
        {
            bool bReturn = false;
            do {
                try {
                    // 로그 설정 파일 읽기. - 실행파일 폴더안의 Config 폴더안에 있음.
                    m_objLoggingConfiguration = ConfigurationManager.LoadConfiguration( "Config/LogConfig.Json" ).Clone() as LoggingConfiguration;
                    // 로그 생성.
                    _logger = new List<ILogger>();
                    for ( int iLoopCount = 0; iLoopCount < m_objLoggingConfiguration.objListLogName.Count; iLoopCount++ ) {
                        var logger = LoggerFactory.Create( iLoopCount, m_objLoggingConfiguration );
                        _logger.Add( logger );
                    }

                    bReturn = true;
                }
                catch ( Exception ex ) {
                    // 예외를 다시 던짐
                    throw new ApplicationException( "An error occurred in the Logger DLL.", ex );
                }                
            } while ( false );
            return bReturn;
        }
        /// <summary>
        /// 메모리 해제
        /// </summary>
        /// <returns></returns>
        public void DeInitiaize()
        {
            // 메모리 해제하고 모든 기록 작성 후 종료.
            if ( null != _logger ) {
                // 하나만 호출해도 나머지 로거 자동 종료 됨.
                if ( CDefineLog.eLogDllType.LOG_SERILOG == ( CDefineLog.eLogDllType )m_objLoggingConfiguration.iLogDllType ) {
                    ( _logger[ 0 ] as SerilogLogger )?.DeInitialize();
                }                    
            }
        }
        /// <summary>
        /// 로그 관련 설정값 저장.
        /// </summary>
        /// <returns></returns>
        public bool SaveConfiguration( LoggingConfiguration objLoggingConfiguration )
        {
            bool bReturn = false;
            do {
                try {
                    // 설정 적용.
                    ConfigurationManager.SaveConfiguration( "Config/LogConfig.Json", objLoggingConfiguration );
                    m_objLoggingConfiguration = objLoggingConfiguration.Clone() as LoggingConfiguration;
                    bReturn = true;
                }
                catch ( Exception ex ) {
                    // 예외를 다시 던짐
                    throw new ApplicationException( "An error occurred in the Logger DLL.", ex );
                }                
            } while ( false );
            return bReturn;
        }

        public void WriteLog( int iLogType, CDefineLog.eLogLevel eLogLevel, string strMessage )
        {
            try {
                    switch ( eLogLevel ) {
                        case CDefineLog.eLogLevel.LOG_LEVEL_DEBUG:
                            _logger[ ( int )iLogType ].Debug( strMessage );
                            break;
                        case CDefineLog.eLogLevel.LOG_LEVEL_INFORMATION:
                            _logger[ ( int )iLogType ].Information( strMessage );
                            break;
                        case CDefineLog.eLogLevel.LOG_LEVEL_WARNING:
                            _logger[ ( int )iLogType ].Warning( strMessage );
                            break;
                        case CDefineLog.eLogLevel.LOG_LEVEL_ERROR:
                            _logger[ ( int )iLogType ].Error( strMessage );
                            break;
                        case CDefineLog.eLogLevel.LOG_LEVEL_FATAL:
                            _logger[ ( int )iLogType ].Fatal( strMessage );
                            break;
                        default:
                            break;
                    }
            }
            catch ( Exception ex  ) {
                // 예외를 다시 던짐
                throw new ApplicationException( "An error occurred in the Logger DLL.", ex );
            }                        
        }
    }
}
