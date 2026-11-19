using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MigrationTools;
using MigrationTools.Host;
using MigrationTools.Host.Commands;
using MigrationTools.Services;

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                if (TryRunFromLongPath(args, out int longPathExitCode))
                {
                    return longPathExitCode;
                }
                await RunAsync(args);
                return Environment.ExitCode;
            }
            catch (Exception ex)
            {
                string line;
                try
                {
                    line = CommandFailureLog.ReportStartupFailure(ex);
                }
                catch (Exception reportFailure)
                {
                    line = $"devopsmigration could not start: {CommandFailureLog.Describe(ex)} The full error could not be logged: {CommandFailureLog.Describe(reportFailure)}";
                }
                Console.Error.WriteLine(line);
                return 1;
            }
        }

        /// <summary>
        /// Started through an 8.3 short path, the runtime looks for a configuration file named after the short path, which does not exist,
        /// so every binding redirect in devopsmigration.exe.config is lost. Run the tool again in a new application domain from its long path, with its real configuration file.
        /// Uses framework types only, so it runs before any dependency that needs a binding redirect is loaded.
        /// </summary>
        private static bool TryRunFromLongPath(string[] args, out int exitCode)
        {
            exitCode = 0;
            string exePath = Assembly.GetEntryAssembly()?.Location;
            if (string.IsNullOrEmpty(exePath) || File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
            {
                return false;
            }
            string longExePath = ToLongPath(exePath);
            string longConfigFile = longExePath + ".config";
            if (string.Equals(longExePath, exePath, StringComparison.OrdinalIgnoreCase) || !File.Exists(longConfigFile))
            {
                return false;
            }
            var setup = new AppDomainSetup { ApplicationBase = Path.GetDirectoryName(longExePath) + Path.DirectorySeparatorChar, ConfigurationFile = longConfigFile };
            var domain = AppDomain.CreateDomain(AppDomain.CurrentDomain.FriendlyName, null, setup);
            exitCode = domain.ExecuteAssembly(longExePath, args);
            return true;
        }

        private static string ToLongPath(string path)
        {
            var buffer = new System.Text.StringBuilder(32767);
            return GetLongPathName(path, buffer, buffer.Capacity) > 0 ? buffer.ToString() : path;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        private static extern int GetLongPathName(string shortPath, System.Text.StringBuilder longPath, int bufferLength);

        private static async Task RunAsync(string[] args)
        {
            string binDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] clientAssemblies = { "MigrationTools.Clients.TfsObjectModel.dll", "MigrationTools.Clients.FileSystem.dll", "MigrationTools.Clients.AzureDevops.Rest.dll" };
            foreach (var assemblyName in clientAssemblies)
            {
                string assemblyPath = Path.Combine(binDirectory, assemblyName);
                if (File.Exists(assemblyPath))
                {
                    // Load by identity, not by path: a path load from an 8.3 short path would load a second copy alongside the reference load.
                    var identity = AssemblyName.GetAssemblyName(assemblyPath);
                    if (!AppDomain.CurrentDomain.GetAssemblies().Any(loaded => AssemblyName.ReferenceMatchesDefinition(identity, loaded.GetName())))
                    {
                        Assembly.Load(identity);
                    }
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

                // Optional pause so an external terminal launched under the debugger stays open.
                try
                {
                    var pauseEnv = Environment.GetEnvironmentVariable("MIGRATIONTOOLS_PAUSE_ON_EXIT");
                    if (Debugger.IsAttached || (pauseEnv != null && (pauseEnv.Equals("1", StringComparison.OrdinalIgnoreCase) || pauseEnv.Equals("true", StringComparison.OrdinalIgnoreCase))))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Execution complete.");
                        Console.Write("Press Enter to close this window...");
                        Console.ReadLine();
                    }
                }
                catch { /* swallow any console IO exceptions (e.g., redirected output) */ }
            }
        }


    }
}
