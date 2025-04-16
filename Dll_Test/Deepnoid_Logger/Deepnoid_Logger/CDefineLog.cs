using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deepnoid_Logger {
    public class CDefineLog {
        /// <summary>
        /// 로그 라이브러리 타입
        /// </summary>
        public enum eLogDllType
        {
            LOG_SERILOG = 0,
            LOG_LOG4NET
        }
        /// <summary>
        /// 로그 레벨
        /// </summary>
        public enum eLogLevel {
            LOG_LEVEL_DEBUG = 0,
            LOG_LEVEL_INFORMATION,
            LOG_LEVEL_WARNING,
            LOG_LEVEL_ERROR,
            LOG_LEVEL_FATAL
        }
    }
}
