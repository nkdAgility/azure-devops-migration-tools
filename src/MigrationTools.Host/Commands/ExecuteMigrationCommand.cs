using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.Host.Commands;
using MigrationTools.Host.Services;
using MigrationTools.Options;
using MigrationTools.Services;
using OpenTelemetry.Trace;
using Serilog;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class ExecuteMigrationCommand : CommandBase<ExecuteMigrationCommandSettings>
    {
        private readonly IServiceProvider _services;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;


        private readonly ITelemetryLogger Telemetery;

        public ExecuteMigrationCommand(
            IHostApplicationLifetime appLifetime,
            IServiceProvider services,
            IDetectOnlineService detectOnlineService,
            IDetectVersionService2 detectVersionService,
            ILogger<CommandBase<ExecuteMigrationCommandSettings>> logger,
            ITelemetryLogger telemetryLogger,
            IMigrationToolVersion migrationToolVersion,
            IConfiguration configuration,
            ActivitySource activitySource) : base(appLifetime, services, detectOnlineService, detectVersionService, logger, telemetryLogger, migrationToolVersion, configuration, activitySource)
        {
            Telemetery = telemetryLogger;
            _services = services;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        internal override async Task<int> ExecuteInternalAsync(CommandContext context, ExecuteMigrationCommandSettings settings)
        {
            int _exitCode;
            try
            {
                    // KILL if the config is not valid
                    if (!VersionOptions.ConfigureOptions.IsConfigValid(Configuration))
                    {
                        CommandActivity.AddEvent(new ActivityEvent("ConfigIsNotValid"));
                        BoilerplateCli.ConfigIsNotValidMessage(Configuration, Log.Logger);
                        return -1;
                    }
                var migrationEngine = _services.GetRequiredService<IMigrationEngine>();
                migrationEngine.Run();
                _exitCode = 0;
            }
            catch (Exception ex)
            {
                CommandActivity.RecordException(ex);
                Telemetery.TrackException(ex, null);
                _logger.LogError(ex, "Unhandled exception!");

                _exitCode = 1;
            }
            finally
            {
                // Stop the application once the work is done
                _appLifetime.StopApplication();
            }
            return _exitCode;
        }
    }
}

