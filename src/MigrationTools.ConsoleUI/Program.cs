using MigrationTools.CommandLine;
using MigrationTools.CustomDiagnostics;
using MigrationTools.Services;
using CommandLine;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using MigrationTools.Core.Configuration;

namespace MigrationTools.ConsoleUI
{
    class Program
    {
        static DateTime _startTime = DateTime.Now;
        static Stopwatch _mainTimer = new Stopwatch();
        static IHost host;

        static int Main(string[] args)
        {
            _mainTimer.Start();
            var InstrumentationKey = "4b9bb17b-c7ee-43e5-b220-ec6db2c33373";
            var sessionID = Guid.NewGuid();
            var ApplicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            var builder = new ConfigurationBuilder();
            BuildAppConfig(builder);

            var aiServiceOptions = new ApplicationInsightsServiceOptions();
            aiServiceOptions.InstrumentationKey = InstrumentationKey;
            aiServiceOptions.ApplicationVersion = ApplicationVersion.ToString();

            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = InstrumentationKey;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(telemetryConfiguration, new CustomConverter())
                .WriteTo.File("log/migrate-core.log")
                .CreateLogger();

            LogContext.PushProperty("SessionID", sessionID);
            Log.Information("Application Starting");
            AsciiLogo(ApplicationVersion);
            Log.Information("Telemetry Note: We use Application Insights to collect telemetry on performance & feature usage for the tools to help our developers target features. This data is tied to a session ID that is generated and shown in the logs. This can help with debugging.");
            Log.Information("Start Time: {StartTime}", _startTime.ToUniversalTime().ToLocalTime());
            Log.Information("Running with args: {@Args}", args);
            Log.Information("OSVersion: {OSVersion}", Environment.OSVersion.ToString());
            Log.Information("Version: {CurrentVersion}", ApplicationVersion);
            Log.Information("userID: {UserId}", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            ///////////////////////////////////////////////////////
            /// Setup Host
            host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IDetectOnlineService, DetectOnlineService>();
                    services.AddSingleton<IDetectVersionService, DetectVersionService>();
                    services.AddApplicationInsightsTelemetryWorkerService(aiServiceOptions);
                    services.AddTransient<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
                    services.AddSingleton<MigrationHost>();
                })
                .UseSerilog()
                .Build();
            TelemetryClient telemetryClient = SetupTelemetry();
            var chk = CheckVersion(ApplicationVersion, host);
            if (chk != 0)
            {
                return chk;
            }
            //////////////////////////////////////////////////
            /// Setup Command Line
            int result = (int)Parser.Default.ParseArguments<InitOptions, ExecuteOptions>(args).MapResult(
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (ExecuteOptions opts) => RunExecuteAndReturnExitCode(opts),
                errs => 1);
            ///////////////////////////////////////////////////////
            Log.Information("Application Ending");
            _mainTimer.Stop();
            telemetryClient.TrackEvent("ApplicationEnd", null,
                new Dictionary<string, double> {
                        { "Application_Elapsed", _mainTimer.ElapsedMilliseconds }
                });
            if (telemetryClient != null)
            {
                telemetryClient.Flush();
            }
            Log.Information("The application ran in {Application_Elapsed} and finished at {Application_EndTime}", _mainTimer.Elapsed.ToString("c"), DateTime.Now.ToUniversalTime().ToLocalTime());
            Log.CloseAndFlush();
            System.Threading.Thread.Sleep(1000);
            return result;
        }

        private static int RunExecuteAndReturnExitCode(ExecuteOptions opts)
        {
            var migration = host.Services.GetRequiredService<MigrationHost>();
            return 0;
        }

        private static int RunInitAndReturnExitCode(InitOptions opts)
        {
            throw new NotImplementedException();
        }

        private static TelemetryClient SetupTelemetry()
        {
            var telemetryClient = host.Services.GetRequiredService<TelemetryClient>();
            telemetryClient.Context.User.Id = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            telemetryClient.Context.Component.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return telemetryClient;
        }

        private static int CheckVersion(Version ApplicationVersion, IHost host)
        {
            var doService = ActivatorUtilities.GetServiceOrCreateInstance<IDetectOnlineService>(host.Services);
            if (doService.IsOnline())
            {
                var dvService = ActivatorUtilities.GetServiceOrCreateInstance<IDetectVersionService>(host.Services);
                Version latestVersion = dvService.GetLatestVersion();
                Log.Information("Latest version detected as {Version_Latest}", latestVersion);
                if (latestVersion > ApplicationVersion)
                {
                    Log.Warning("You are currently running version {Version_Current} and a newer version ({Version_Latest}) is available. You should upgrade now using Chocolatey command 'choco upgrade vsts-sync-migrator' from the command line.", ApplicationVersion, latestVersion);
#if !DEBUG
                    Console.WriteLine("Do you want to continue? (y/n)");
                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        Log.Warning("User aborted to update version");
                        return 2;
                    }
#endif
                }
            }
            return 0;
        }

        static void BuildAppConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional:false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }

        private static void AsciiLogo(Version thisVersion)
        {
            Log.Information("                                      &@&                                      ");
            Log.Information("                                   @@(((((@                                    ");
            Log.Information("                                  @(((((((((@                                  ");
            Log.Information("                                @(((((((((((((&                                ");
            Log.Information("                              ##((((((@ @((((((@@                              ");
            Log.Information("                             @((((((@     @((((((&                             ");
            Log.Information("                            @(((((#        @((((((@                            ");
            Log.Information("                           &(((((&           &(((((@                           ");
            Log.Information("                          @(((((&             &(((((@                          ");
            Log.Information("                          &(((((@#&@((.((&@@@(#(((((@                          ");
            Log.Information("                         #((((#..................#@((&                         ");
            Log.Information("                       &@(((((&......................(@                        ");
            Log.Information("                     @.(&((((&...&&        &@&..........&@                     ");
            Log.Information("                   @...@(((((@                   @#.......((                   ");
            Log.Information("                 &.....@(((((@                   @((@.......&                  ");
            Log.Information("                @......@(((((                    #((((&.......&                ");
            Log.Information("               #.....( &(((((         @@@        ((((((@@......@               ");
            Log.Information("              &.....@  @(((&@@#(((((((((((((((((#@(((((&  ......@              ");
            Log.Information("             @.....@  &@&((((((((((((((((((((((((@(((((@#  ......@             ");
            Log.Information("            @.....&@(((((((((((((((&&@@@@@(((((@((((#(((#@(....&               ");
            Log.Information("            @.....&((((((((&@@&                 @(((((@(((((((@...#            ");
            Log.Information("            &....((((((@@(((((@                &@(((((@&((((((((#&&            ");
            Log.Information("           @(....&((@    @(((((@               @(((((@    @(((((((##           ");
            Log.Information("         @(#(....&        &(((((@             @(((((&       &@(((((((&         ");
            Log.Information("       &@(((&.....        @((((((&           @(((((       &.(&((((((@          ");
            Log.Information("      @(((((@.....&        (((((@        &@(((((&         @....@((((((@        ");
            Log.Information("     @(((((#@.....(          &(((((@&     ##(((((&         @.....@@((((((@     ");
            Log.Information("   (&(((((@  &.....@&         @((((((@   @((((((@         @......   @(((((@    ");
            Log.Information("   &(((((@    @.....#&         @#((((((@((((((#          @......&    @(((((@   ");
            Log.Information("  @(((((@      &......&          @(((((((@#((@         &@......       @(((((@  ");
            Log.Information(" @(((((@        @......@&        @@@(((((((&@&        @......(         #(((((@ ");
            Log.Information(" #((((&           &.......@  &@&(((((@#((((((((@@& &@.......@          ((((&   ");
            Log.Information("&(((((@@           @(....&@#((((((((((@ @(((((((#@........@            &@(((((@");
            Log.Information("&(((((((((((((((((((((((((((((((((&@@@@@@@@@&...........@(((((((((((((((((((((@");
            Log.Information("@(((((((((((((((((((((((((((((&@(....................@#((((((((((((((((((((((#@");
            Log.Information("      @((((((((((((((&@&  &&...................@   @@#((((((((((((((#@@        ");
            Log.Information("                                                                               ");
            Log.Information("===============================================================================");
            Log.Information("===                       Azure DevOps Migration Tools                       ==");
            Log.Information($"===                                 v{thisVersion}                                ==");
            Log.Information("===============================================================================");
        }

    }
}
