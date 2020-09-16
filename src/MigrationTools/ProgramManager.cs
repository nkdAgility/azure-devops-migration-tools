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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using MigrationTools.Core.Configuration;
using Newtonsoft.Json;
using MigrationTools.Core.Configuration.FieldMap;
using MigrationTools.Core.Sinks;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using System.Net;

namespace MigrationTools
{
   public class ProgramManager
    {
        protected static DateTime _startTime = DateTime.Now;
        protected static Stopwatch _mainTimer = new Stopwatch();

        public delegate IServiceCollection PlatformSpecificServices(IServiceCollection services);
        public delegate void EngineEntryPoint(IHost host, ExecuteOptions opts);

        protected static int RunExecuteAndReturnExitCode(ExecuteOptions opts, TelemetryClient tc, PlatformSpecificServices AddPlatformSpecificServices, EngineEntryPoint StartEngine)
        {
            tc.TrackEvent("ExecuteCommand");

            if (opts.ConfigFile == string.Empty)
            {
                opts.ConfigFile = "configuration.json";
            }

            if (!File.Exists(opts.ConfigFile))
            {
                Log.Information("The config file {ConfigFile} does not exist, nor does the default 'configuration.json'. Use '{ExecutableName}.exe init' to create a configuration file first", opts.ConfigFile, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                return 1;
            }
            Log.Information("Config Found, creating engine host");
            var config = new EngineConfigurationBuilder().BuildFromFile(opts.ConfigFile);
            Console.Title = $"Azure DevOps Migration Tools: {System.IO.Path.GetFileName(opts.ConfigFile)} - {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} - {config.Source.Project} - {config.Target.Project}";
            // Setup Host
            var host = Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("appsettings.json", optional: true);
                    configHost.AddEnvironmentVariables();
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddJsonFile(opts.ConfigFile, optional: false);
                    configApp.AddJsonFile(
                        $"{opts.ConfigFile}.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddOptions();
                    services.AddSingleton<IDetectOnlineService, DetectOnlineService>();
                    services.AddSingleton<IDetectVersionService, DetectVersionService>();
                    services.AddSingleton(tc);
                    services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
                    services.AddSingleton<EngineConfiguration>(config);
                    AddPlatformSpecificServices(services);
                })
                .UseConsoleLifetime()
                .UseSerilog()
                .Build();
            StartEngine(host, opts);
            Log.Information("Run complete...");
            return 0;
        }



        protected static object RunInitAndReturnExitCode(InitOptions opts, TelemetryClient telemetryClient)
        {
            Telemetry.Current.TrackEvent("InitCommand");

            string configFile = opts.ConfigFile;
            if (string.IsNullOrEmpty(configFile))
            {
                configFile = "configuration.json";
            }
            Log.Information("ConfigFile: {configFile}", configFile);
            if (File.Exists(configFile))
            {
                Log.Information("Deleting old configuration.json reference file");
                File.Delete(configFile);
            }
            if (!File.Exists(configFile))
            {
                Log.Information("Populating config with {Options}", opts.Options.ToString());
                IEngineConfigurationBuilder cbuilder = new EngineConfigurationBuilder();
                EngineConfiguration config;
                switch (opts.Options)
                {
                    case OptionsMode.Full:
                        config = cbuilder.BuildDefault();
                        break;
                    case OptionsMode.WorkItemTracking:
                        config = cbuilder.BuildWorkItemMigration();
                        break;
                    default:
                        config = cbuilder.BuildDefault();
                        break;
                }

                string json = JsonConvert.SerializeObject(config, Formatting.Indented,
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter());
                StreamWriter sw = new StreamWriter(configFile);
                sw.WriteLine(json);
                sw.Close();
                Log.Information("New configuration.json file has been created");
            }
            return 0;
        }

        protected static ILogger BuildLogger()
        {
            var builder = new ConfigurationBuilder();
            BuildAppConfig(builder);

            string logsPath = CreateLogsPath();
            var logPath = Path.Combine(logsPath, "migration.log");

            var logconf= new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(Telemetry.GetTelemiteryClient(), new CustomConverter())
                .WriteTo.File(logPath)
                .CreateLogger();
            Log.Information("Writing log to {logPath}", logPath);
            return logconf;
        }

        protected static void ApplicationStartup(string[] args)
        {
            _mainTimer.Start();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Log.Information("Application Starting");
            AsciiLogo(version);
            Log.Information("Telemetry Note: We use Application Insights to collect telemetry on performance & feature usage for the tools to help our developers target features. This data is tied to a session ID that is generated and shown in the logs. This can help with debugging.");
            Log.Information("Start Time: {StartTime}", _startTime.ToUniversalTime().ToLocalTime());
            Log.Information("Running with args: {@Args}", args);
            Log.Information("OSVersion: {OSVersion}", Environment.OSVersion.ToString());
            Log.Information("Version: {CurrentVersion}", version);
            Log.Information("userID: {UserId}", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
        }

        protected static void ApplicationShutdown()
        {
            Log.Information("Application Ending");
            _mainTimer.Stop();
            Telemetry.Current.TrackEvent("ApplicationEnd", null,
                new Dictionary<string, double> {
                        { "Application_Elapsed", _mainTimer.ElapsedMilliseconds }
                });
            if (Telemetry.Current != null)
            {
                Telemetry.Current.Flush();
            }
            Log.Information("The application ran in {Application_Elapsed} and finished at {Application_EndTime}", _mainTimer.Elapsed.ToString("c"), DateTime.Now.ToUniversalTime().ToLocalTime());
            Log.CloseAndFlush();
            System.Threading.Thread.Sleep(5000);
        }

        protected static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //ExceptionTelemetry excTelemetry = new ExceptionTelemetry((Exception)e.ExceptionObject);
            //excTelemetry.SeverityLevel = SeverityLevel.Critical;
            //excTelemetry.HandledAt = ExceptionHandledAt.Unhandled;
            Log.Error((Exception)e.ExceptionObject, "An Unhandled exception occured.");
            Log.CloseAndFlush();
            System.Threading.Thread.Sleep(5000);
        }

        protected static void BuildAppConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }

        protected static void AsciiLogo(Version thisVersion)
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

            var productName = ((AssemblyProductAttribute)Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyProductAttribute), true)[0]).Product;
            Log.Information("{productName} ", productName);
            Log.Information("v{thisVersion}", thisVersion);
            var companyName = ((AssemblyCompanyAttribute)Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyCompanyAttribute), true)[0]).Company;
            Log.Information("{companyName} ", companyName);
            Log.Information("===============================================================================");
        }



        protected static string CreateLogsPath()
        {
            string exportPath;
            string assPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exportPath = Path.Combine(Path.GetDirectoryName(assPath), "logs", DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            return exportPath;
        }
    }
}
