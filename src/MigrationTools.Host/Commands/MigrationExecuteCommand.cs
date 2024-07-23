using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.Host.Commands;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class MigrationExecuteCommand : AsyncCommand<MigrationExecuteCommandSettings>
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ITelemetryLogger Telemetery;

        public MigrationExecuteCommand(IServiceProvider services,
            ILogger<MigrationExecuteCommand> logger,
            IHostApplicationLifetime appLifetime, ITelemetryLogger telemetryLogger)
        {
            Telemetery = telemetryLogger;
            _services = services;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, MigrationExecuteCommandSettings settings)
        {
             int _exitCode;
            try
            {
                var migrationEngine = _services.GetRequiredService<IMigrationEngine>();
                migrationEngine.Run();
                _exitCode = 0;
            }
            catch (Exception ex)
            {
                Telemetery.TrackException(ex, null, null);
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

