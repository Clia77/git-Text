using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deepnoid_Logger {
    public interface ILogger {

        /// <summary>
        /// 디버깅 목적으로 사용, 개발환경에서 런타임 결과를 감시하는데 유용.
        /// </summary>
        /// <param name="strMessage"></param>
        void Debug( string strMessage );
        /// <summary>
        /// 디버깅 목적으로 사용, 개발환경에서 런타임 결과를 감시하는데 유용.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="strMessage"></param>
        void Debug( Exception exception, string strMessage );
        /// <summary>
        /// 애플리케이션 모니터링 및 요청과 응답의 세부 사랑 또는 특정 작업 결과를 추적하는데 사용
        /// </summary>
        /// <param name="strMessage"></param>
        void Information( string strMessage );
        /// <summary>
        /// 애플리케이션 모니터링 및 요청과 응답의 세부 사랑 또는 특정 작업 결과를 추적하는데 사용
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="strMessage"></param>
        void Information( Exception exception, string strMessage );
        /// <summary>
        /// 비 치명적이지만 주의가 필요한 잠재적 문제를 기록
        /// </summary>
        /// <param name="strMessage"></param>
        void Warning( string strMessage );
        /// <summary>
        /// 비 치명적이지만 주의가 필요한 잠재적 문제를 기록
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="strMessage"></param>
        void Warning( Exception exception, string strMessage );
        /// <summary>
        /// 오류를 상세히 추척하며, 애플리케이션에서 발생한 문제를 기록
        /// </summary>
        /// <param name="strMessage"></param>
        void Error( string strMessage );
        /// <summary>
        /// 오류를 상세히 추척하며, 애플리케이션에서 발생한 문제를 기록
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="strMessage"></param>
        void Error( Exception exception, string strMessage );
        /// <summary>
        /// 가장 중요한 로그 레벨로 긴급한 주의가 필요한 치명적인 시스템 오류를 기록
        /// </summary>
        /// <param name="strMessage"></param>
        void Fatal( string strMessage );
        /// <summary>
        /// 가장 중요한 로그 레벨로 긴급한 주의가 필요한 치명적인 시스템 오류를 기록
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="strMessage"></param>
        void Fatal( Exception exception, string strMessage );
    }
}
