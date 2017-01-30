using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Incomm.Libraries.Logging;

namespace Swashbuckle.Adapters
{
    internal static class LogAdapter
    {
        private const string LogName = "Application";
        private const string ApplicationName = "Swashbuckle";
        //private const string ApplicationName = "OrderApi";

        
        private static readonly EventLog EventLog;

        internal static ILogger Logger;

        static LogAdapter()
        {
            EventLog = new EventLog { Source = ApplicationName };
            StartLogger();
        }

        internal static void StartLogger()
        {
            try
            {
                Logger = Incomm.Libraries.Logging.Logger.CreateLogger(ApplicationName);
            }
            catch (Exception ex)
            {
                LogEventError(ex.Message);
            }
        }

        public static void LogError(string message, Exception e) =>
            Logger?.Error(message, e);

        public static Task LogErrorAsync(string message, Exception e) =>
            Logger?.ErrorAsync(message, e) ?? Task.CompletedTask;

        public static Task LogErrorAsync(Exception e) =>
            LogErrorAsync(e.Message, e) ?? Task.CompletedTask;

        public static void LogInfo(string message) => Logger?.Info(message);

        public static Task LogInfoAsync(string message) =>
            Logger?.InfoAsync(message) ?? Task.CompletedTask;

        internal static void LogEventError(string message) =>
            LogEvent(message, EventLogEntryType.Error);

        internal static void LogEvent(string message, EventLogEntryType eventLogEntryType)
        {
            if (!EventLog.SourceExists(ApplicationName))
                EventLog.CreateEventSource(ApplicationName, LogName);

            EventLog.WriteEntry(message, eventLogEntryType);
        }
    }
}
