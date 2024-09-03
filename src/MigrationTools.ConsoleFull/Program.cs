using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using MigrationTools;
using MigrationTools.Host;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using MigrationTools.Services;

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using (var CommandActivity = ActivitySourceProvider.GetActivitySource().StartActivity("MigrationToolsCli"))
            {
                var hostBuilder = MigrationToolHost.CreateDefaultBuilder(args);

                if (hostBuilder is null)
                {
                    return;
                }

                hostBuilder
                    .ConfigureServices((context, services) =>
                    {
                        // New v2 Architecture fpr testing
                        services.AddMigrationToolServicesForClientFileSystem(context.Configuration);
                        services.AddMigrationToolServicesForClientAzureDevOpsObjectModel(context.Configuration);
                        services.AddMigrationToolServicesForClientAzureDevopsRest(context.Configuration);

                        // v1 Architecture (Legacy)
                        services.AddMigrationToolServicesForClientLegacyAzureDevOpsObjectModel();
                        services.AddMigrationToolServicesForClientLegacyCore();
                    });
                await hostBuilder.RunConsoleAsync();
            }
        }


    }
}