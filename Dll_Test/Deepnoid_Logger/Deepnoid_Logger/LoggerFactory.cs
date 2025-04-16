using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;
using System.Threading;

namespace Deepnoid_Logger {
    public class LoggerFactory {
        /// <summary>
        /// 로그 객체 생성
        /// </summary>
        /// <param name="iIndex">로그 타입</param>
        /// <param name="objLogParameter">로그 설정 파라미터</param>
        /// <returns>생성된 로그 객체</returns>
        /// <exception cref="ArgumentException"></exception>
        public static ILogger Create( int iIndex, LoggingConfiguration objLogParameter )
        {
            do {
                switch ( ( CDefineLog.eLogDllType )objLogParameter.iLogDllType ) {
                    case CDefineLog.eLogDllType.LOG_SERILOG:
                        // 로그파일의 롤링 간격을 설정값에 따라 로그 파일 이름에 날짜 붙일지 말지 결정.
                        string strFileName = ( 0 == objLogParameter.iRollingInterval ) ? string.Format( "{0}.log", objLogParameter.objListLogName[ iIndex ] ) : string.Format( "{0}_.log", objLogParameter.objListLogName[ iIndex ] );
                        // Serilog 설정 로드
                        var serilogLogger = new LoggerConfiguration()
                            // Serilog 설정 추가 (예: 파일 출력, 콘솔 출력 등)
                            .WriteTo.Async( a =>
                                a.Map( m =>
                                    new DateTime( m.Timestamp.Year, m.Timestamp.Month, m.Timestamp.Day ), ( Date, wt ) =>
                                    wt.File(
                                        // 경로 지정. 
                                        path: objLogParameter.strLogPath + $"{Date: yyyy}/{Date: MM}/{Date: dd}/" + strFileName,
                                        rollingInterval: ( RollingInterval )objLogParameter.iRollingInterval,
                                        // 파일 삭제, 파일크기, 등등 설정 파일 우선 막자... 
                                        //retainedFileCountLimit: objLogParameter.iFileCountLimit,
                                        restrictedToMinimumLevel: ( Serilog.Events.LogEventLevel )objLogParameter.iLogLevel,
                                        //fileSizeLimitBytes: objLogParameter.iFileSizeLimitBytes,
                                        //rollOnFileSizeLimit: objLogParameter.bRollOnFileSizeLimit,
                                        outputTemplate: objLogParameter.strOutputTemplate,
                                        shared: objLogParameter.bShared,
                                        flushToDiskInterval: TimeSpan.FromSeconds( objLogParameter.iFlushToDiskInterval )
                                     )
                                )
                            )
                            .CreateLogger();
                        return new SerilogLogger( serilogLogger );
                    default:
                        throw new ArgumentException( "Invalid logger type." );
                }
            } while ( false );
        }
    }
}
