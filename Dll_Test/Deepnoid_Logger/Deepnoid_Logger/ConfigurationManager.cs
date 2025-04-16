using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Newtonsoft.Json;

namespace Deepnoid_Logger {
    public  class ConfigurationManager {
        /// <summary>
        /// Json파일로 되어 있는 설정파일을 읽어와 적용.
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <returns></returns>
        public static LoggingConfiguration LoadConfiguration( string filePath )
        {
            try {
                if ( File.Exists( filePath ) ) {
                    string json = File.ReadAllText( filePath );
                    LoggingConfiguration obj = new LoggingConfiguration();
                    obj = JsonConvert.DeserializeObject<LoggingConfiguration>( json );
                    return obj.Clone() as LoggingConfiguration;
                }
                else {
                    // 파일이 없는 경우 기본 설정 반환 또는 예외 발생
                    LoggingConfiguration obj = new LoggingConfiguration();
                    obj.iLogDllType = 0;
                    obj.iLogLevel = 0;
                    obj.iLogTypeCount = 12;
                    obj.strLogPath = "D:/Logs/";
                    obj.objListLogName = new List<string>();
                    obj.objListLogName.Add( "SYSTEM" );
                    obj.objListLogName.Add( "PROCESS_STAGE_INSPECTION" );
                    obj.objListLogName.Add( "RESULT" );
                    obj.objListLogName.Add( "BUTTON_OPERATION" );
                    obj.objListLogName.Add( "CONFIG_DATA" );
                    obj.objListLogName.Add( "CAMERA" );
                    obj.objListLogName.Add( "TACT_TIME" );
                    obj.objListLogName.Add( "ETC" );
                    obj.objListLogName.Add( "EXCEPTION" );
                    obj.objListLogName.Add( "PROCESS_SIMULATION" );
                    obj.objListLogName.Add( "VISION_RESULT" );
                    obj.objListLogName.Add( "RESOURCE_INFOMATION" );
                    obj.iRollingInterval = 3;
                    obj.iFileCountLimit = 365;
                    obj.iFileSizeLimitBytes = 10_000_000;
                    obj.bRollOnFileSizeLimit = true;
                    obj.strOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}";
                    obj.bShared = true;
                    obj.iFlushToDiskInterval = 1;
                    // 없을 경우 Defult 값으로 저장시키자.
                    SaveConfiguration(filePath, obj);

                    return obj.Clone() as LoggingConfiguration; // 기본 설정
                                                                                                                                               // throw new FileNotFoundException("Configuration file not found.");
                }
            }
            catch ( JsonException ex ) {
                // JSON 파싱 오류 처리
                Console.WriteLine( $"Error parsing configuration file: {ex.Message}" );
                return null; // 또는 예외 발생
            }
        }
        /// <summary>
        /// 설정을 Json 파일로 저장.
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="config">설정 객체</param>
        public static void SaveConfiguration( string filePath, LoggingConfiguration config )
        {
            string json = JsonConvert.SerializeObject( config, Newtonsoft.Json.Formatting.Indented );
            File.WriteAllText( filePath, json );
        }
    }
}
