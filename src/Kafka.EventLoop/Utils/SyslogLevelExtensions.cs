using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Utils
{
    internal static class SyslogLevelExtensions
    {
        public static LogLevel ToLogLevel(this SyslogLevel syslogLevel)
        {
            return syslogLevel switch
            {
                SyslogLevel.Emergency => LogLevel.Critical,
                SyslogLevel.Alert => LogLevel.Critical,
                SyslogLevel.Critical => LogLevel.Critical,
                SyslogLevel.Error => LogLevel.Error,
                SyslogLevel.Warning => LogLevel.Warning,
                SyslogLevel.Notice => LogLevel.Warning,
                SyslogLevel.Info => LogLevel.Information,
                SyslogLevel.Debug => LogLevel.Debug,
                _ => LogLevel.Debug
            };
        }
    }
}
