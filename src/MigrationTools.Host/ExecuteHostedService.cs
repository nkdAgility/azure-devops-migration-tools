using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.Host.CommandLine;

namespace MigrationTools.Host
{
    public class ExecuteHostedService : IHostedService
    {
        private readonly ExecuteOptions _exceuteOptions;
        
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        private int? _exitCode;

        public ExecuteHostedService(
            IServiceProvider services,
            ExecuteOptions exceuteOptions,
            ILogger<ExecuteHostedService> logger,
            IHostApplicationLifetime appLifetime)
        {
            _exceuteOptions = exceuteOptions;
            _services = services;
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
                        var migrationEngine = _services.GetRequiredService<IMigrationEngine>();
                        migrationEngine.Run();
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
    }
}