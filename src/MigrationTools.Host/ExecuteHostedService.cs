﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Host
{
    public class ExecuteHostedService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ITelemetryLogger Telemetery;

        private int? _exitCode;

        public ExecuteHostedService(
            IServiceProvider services,
            ILogger<ExecuteHostedService> logger,
            IHostApplicationLifetime appLifetime, ITelemetryLogger telemetryLogger)
        {
            Telemetery = telemetryLogger;
            _services = services;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(() =>
                {
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
                });
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Exiting with return code: {_exitCode}");

            if (_exitCode.HasValue)
            {
                Environment.ExitCode = _exitCode.Value;
            }
            return Task.CompletedTask;
        }
    }
}