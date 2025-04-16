using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deepnoid_Logger {
    public class LoggingConfiguration : ICloneable {
        /// <summary>
        /// 로그 타입 
        /// 0 : SeriLog, 1 : Log4Net
        /// </summary>
        public int iLogDllType { get; set; }
        /// <summary>
        /// 로그 타입 개수.
        /// 예 ) System, Process, Config, 등등
        /// </summary>
        public int iLogTypeCount { get; set; }
        /// <summary>
        /// 로그 이름
        /// </summary>
        public List<string> objListLogName { get; set; }
        /// <summary>
        /// 로그 경로
        /// </summary>
        public string strLogPath { get; set; }
         /// <summary>
        /// 로그 파일의 롤링 간격을 설정
        /// </summary>
        public int iRollingInterval { get; set; }
        /// <summary>
        /// 유지할 로그 파일의 최대 개수를 설정
        /// </summary>
        public int iFileCountLimit { get; set; }
        /// <summary>
        /// 저장할 로그 레벨
        /// 0 : Verbose, 1 : Debug, 2 : Information, 3 : Warning, 4 : Error, 5 : Fatal
        /// </summary>
        public int iLogLevel { get; set; }
        /// <summary>
        /// 로그 파일의 최대 크기를 설정합니다.
        /// </summary>
        public int iFileSizeLimitBytes { get; set; }
        /// <summary>
        /// 파일 크기 제한에 도달했을 때 파일을 롤링할지 여부를 설정
        /// </summary>
        public bool bRollOnFileSizeLimit { get; set; }
        /// <summary>
        /// 로그 메시지의 출력 템플릿을 설정
        /// </summary>
        public string strOutputTemplate { get; set; }
        /// <summary>
        /// 여러 프로세스에서 로그 파일을 공유할지 여부를 설정
        /// </summary>
        public bool bShared { get; set; }
        /// <summary>
        /// 로그를 디스크에 플러시하는 간격을 설정 ( 초단위 )
        /// </summary>
        public int iFlushToDiskInterval { get; set; }
        /// <summary>
        /// 생성자
        /// </summary>
        public LoggingConfiguration() {
            iLogDllType = 0;
            iLogTypeCount = 0;
            objListLogName = new List<string>();
            strLogPath = "";
            iRollingInterval = 0;
            iFileCountLimit = 0;
            iLogLevel = 0;
            iFileSizeLimitBytes = 10_000_000;
            bRollOnFileSizeLimit = false;
            strOutputTemplate = "";
            bShared = false;
            iFlushToDiskInterval = 0;
        }
        /// <summary>
        /// 복사
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            LoggingConfiguration obj = new LoggingConfiguration();
            obj.iLogDllType = iLogDllType;
            obj.iLogTypeCount = iLogTypeCount;
            obj.objListLogName.AddRange(objListLogName);
            obj.strLogPath = strLogPath;
            obj.iRollingInterval = iRollingInterval;
            obj.iFileCountLimit = iFileCountLimit;
            obj.iLogLevel = iLogLevel;
            obj.iFileSizeLimitBytes = iFileSizeLimitBytes;
            obj.bRollOnFileSizeLimit = bRollOnFileSizeLimit;
            obj.strOutputTemplate = strOutputTemplate;
            obj.bShared = bShared;
            obj.iFlushToDiskInterval = iFlushToDiskInterval;
            return obj;
        }
    }
}
