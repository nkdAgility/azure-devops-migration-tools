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
using System.Linq;
using System.Configuration.Assemblies;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string binDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] clientAssemblies = { "MigrationTools.Clients.TfsObjectModel.dll", "MigrationTools.Clients.FileSystem.dll", "MigrationTools.Clients.AzureDevops.Rest.dll" };
            foreach (var assemblyName in clientAssemblies)
            {
                string assemblyPath = Path.Combine(binDirectory, assemblyName);
                if (File.Exists(assemblyPath))
                {
                    Assembly.LoadFrom(assemblyPath);
                }
                else
                {
                    Console.WriteLine($"Assembly not found: {assemblyName}");
                }
            }
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(ass => (ass.FullName.StartsWith("MigrationTools") || ass.FullName.StartsWith("VstsSyncMigrator")));
            foreach (var assembly in assemblies)
            {
               // Console.WriteLine(assembly.FullName);
            }

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
                        services.AddMigrationToolServicesForClientTfs_Processors();
                    });
                await hostBuilder.RunConsoleAsync();
            }
        }


    }
}