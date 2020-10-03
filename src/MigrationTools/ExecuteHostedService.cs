using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.CommandLine;

namespace MigrationTools
{
    public class ExecuteHostedService : IHostedService
    {
        private readonly ExecuteOptions _exceuteOptions;
        private readonly IMigrationEngine _migrationEngine;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        private int? _exitCode;
        public ExecuteHostedService(
            ExecuteOptions exceuteOptions,
            IMigrationEngine migrationEngine,
            ILogger<ExecuteHostedService> logger,
            IHostApplicationLifetime appLifetime)
        {
            _exceuteOptions = exceuteOptions;
            _migrationEngine = migrationEngine;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting with arguments: {string.Join(" ", Environment.GetCommandLineArgs())}");
            if (_exceuteOptions == null)
            {
                return Task.CompletedTask;
            }
            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        _migrationEngine.Run();
                        _exitCode = 0;
                    }
                    catch (Exception ex)
                    {
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

        //protected override Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    if (_exceuteOptions == null)
        //    {
        //        return Task.CompletedTask;
        //    }
        //    return Task.Run(() =>
        //    {
        //        _migrationEngine.Run();
        //    });
        //}
    }
}
