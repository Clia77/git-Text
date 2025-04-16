using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Deepnoid_Logger {
    public  class SerilogLogger : ILogger {
        private readonly Serilog.ILogger _logger;

        public SerilogLogger( Serilog.ILogger logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        public void DeInitialize() => Log.CloseAndFlush();

        public void Debug( string strMessage ) => _logger?.Debug( strMessage );
        public void Debug( Exception exception, string strMessage ) => _logger?.Debug( exception, strMessage );
        public void Information( string strMessage ) => _logger?.Information( strMessage );
        public void Information( Exception exception, string strMessage ) => _logger?.Information( exception, strMessage );
        public void Warning( string strMessage ) => _logger?.Warning( strMessage );
        public void Warning( Exception exception, string strMessage ) => _logger?.Warning( exception, strMessage );
        public void Error( string strMessage ) => _logger?.Error( strMessage );
        public void Error( Exception exception, string strMessage ) => _logger?.Error( exception, strMessage );
        public void Fatal( string strMessage ) => _logger?.Fatal( strMessage );
        public void Fatal( Exception exception, string strMessage ) => _logger?.Fatal( exception, strMessage );
    }
}
