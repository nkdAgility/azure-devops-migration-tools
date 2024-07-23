using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Spectre.Console.Cli;

namespace MigrationTools.Host
{
    internal class MigrationService : BackgroundService
    {
        private ICommandApp AppCommand { get; }
        private IHostApplicationLifetime AppLifetime { get; }
        private ILogger Logger { get; }

        public MigrationService(ICommandApp appCommand, IHostApplicationLifetime appLifetime, ILogger<MigrationService> logger)
        {
            AppCommand = appCommand;
            AppLifetime = appLifetime;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await AppCommand.RunAsync(Environment.GetCommandLineArgs());
            AppLifetime.StopApplication();
        }
    }
}
