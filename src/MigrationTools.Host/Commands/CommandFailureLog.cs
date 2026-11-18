using System;
using Microsoft.Extensions.Logging;
using MigrationTools.Services;

namespace MigrationTools.Host.Commands
{
    public static class CommandFailureLog
    {
        /// <summary>
        /// Logs a command failure as one line that says what failed, why and what to do; the full exception is logged at Debug only.
        /// </summary>
        public static void LogCommandFailure(this ILogger logger, string commandName, Exception ex)
        {
            logger.LogError(
                "{CommandName} failed: {Reason} Check the configuration, then run again with the Serilog MinimumLevel set to Debug to see the full error in the log at {LogPath}.",
                commandName,
                Describe(ex),
                LogLocationService.GetLogPath());
            logger.LogDebug(ex, "{CommandName} failed with this exception.", commandName);
        }

        /// <summary>
        /// The outermost message, which names what was being done, followed by the innermost one, which names the cause.
        /// </summary>
        public static string Describe(Exception ex)
        {
            var root = ex.GetBaseException();
            return ReferenceEquals(root, ex) ? ex.Message : $"{ex.Message} {root.Message}";
        }
    }
}
