using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.Core;
using MigrationTools.Core.Configuration;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MigrationTools
{
    public class MigrationHost
    {
        private IHost host;
        private EngineConfiguration config;

        public MigrationHost(ILogger<MigrationHost> log, TelemetryClient telemetry, IEngineConfigurationBuilder engineConfigBuilder)
        {
            config = engineConfigBuilder.BuildFromFile();
            IHostBuilder hostbuilder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<EngineConfiguration>(config);
                    services.AddSingleton<TelemetryClient>(telemetry);

                })
                .UseSerilog();

            Log.Error("Running but no implementation :) ");
        }

        
    }
}
