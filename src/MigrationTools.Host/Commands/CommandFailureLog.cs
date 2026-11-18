using System;
using System.IO;
using Microsoft.Extensions.Logging;
using MigrationTools.Services;
using Serilog;
using Serilog.Events;

namespace MigrationTools.Host.Commands
{
    public static class CommandFailureLog
    {
        /// <summary>
        /// Logs a command failure as one line that says what failed, why and what to do; the full exception is logged at Debug only.
        /// </summary>
        public static void LogCommandFailure(this Microsoft.Extensions.Logging.ILogger logger, string commandName, Exception ex)
        {
            logger.LogError(
                "{CommandName} failed: {Reason} Check the configuration, then run again with the Serilog MinimumLevel set to Debug to see the full error in the log at {LogPath}.",
                commandName,
                Describe(ex),
                LogLocationService.GetLogPath());
            logger.LogDebug(ex, "{CommandName} failed with this exception.", commandName);
        }

        /// <summary>
        /// Records a failure that happened before or outside a command and returns the one line to show the user.
        /// When startup failed before the host set up logging, the full exception goes to a startup log file at Debug, so it is not lost.
        /// </summary>
        public static string ReportStartupFailure(Exception ex)
        {
            if (Log.Logger.IsEnabled(LogEventLevel.Fatal))
            {
                Log.Debug(ex, "devopsmigration could not start.");
                Log.CloseAndFlush();
                return $"devopsmigration could not start: {Describe(ex)} Fix the cause (for example the configuration file) and run again; with the Serilog MinimumLevel set to Debug the full error is in the log at {LogLocationService.GetLogPath()}.";
            }

            string startupLog = Path.Combine(LogLocationService.GetLogPath(), "startup-failure.log");
            using (var bootstrap = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(startupLog, shared: true).CreateLogger())
            {
                bootstrap.Debug(ex, "devopsmigration could not start.");
            }
            return $"devopsmigration could not start: {Describe(ex)} Fix the cause (for example the configuration file) and run again. The full error is in {startupLog}.";
        }

        /// <summary>
        /// The outermost message, which names what was being done, followed by the innermost one, which names the cause, on one line.
        /// </summary>
        public static string Describe(Exception ex)
        {
            var root = ex.GetBaseException();
            var text = ReferenceEquals(root, ex) ? ex.Message : $"{ex.Message} {root.Message}";
            return text.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
        }
    }
}
