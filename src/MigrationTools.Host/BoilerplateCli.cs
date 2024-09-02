using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using MigrationTools.Host.Commands;
using MigrationTools.Options;
using Spectre.Console;

namespace MigrationTools.Host
{
    internal static class BoilerplateCli
    {

        internal static void ConfigIsNotValidMessage(IConfiguration configuration ,Serilog.ILogger logger)
        {
            var version = VersionOptions.ConfigureOptions.GetMigrationConfigVersion(configuration);
            AsciiLogo("unknown", logger);
            logger.Fatal("Config is Invalid");
            
            string exeName = Path.GetFileName(Assembly.GetEntryAssembly().Location);
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Markup($"[red]!!ACTION REQUIRED!![/] we no longer support the [yellow]{version.schema.ToString()} config schema[/]...  "));
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("You are using a deprecated version of the configuration, please update to the latest version.");
            AnsiConsole.Write(new Markup($"You can use [bold yellow]{exeName} upgrade -c myconfig.json[/] to atempt to update it."));
            AnsiConsole.WriteLine("This is best effort and may not bring across all of your property values as it will only create and map valid ones with the same name.");
            AnsiConsole.WriteLine("We have made lots of architectural changes, refactors, and renames. Not all of them are mapped...");
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Markup($"[red]Exiting...[/]"));
            AnsiConsole.WriteLine();
        }

        internal static void AsciiLogo(string thisVersion, Serilog.ILogger logger)
        {
            AnsiConsole.Write(new FigletText("Azure DevOps").LeftJustified().Color(Color.Purple));
            AnsiConsole.Write(new FigletText("Migration Tools").LeftJustified().Color(Color.Purple));
            var productName = ((AssemblyProductAttribute)Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyProductAttribute), true)[0]).Product;
            logger.Information("{productName} ", productName);
            logger.Information("{thisVersion}", thisVersion);
            var companyName = ((AssemblyCompanyAttribute)Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyCompanyAttribute), true)[0]).Company;
            logger.Information("{companyName} ", companyName);
            logger.Information("===============================================================================");
        }

        internal static void TelemetryNote<TSettings>(TSettings settings, Serilog.ILogger logger) where TSettings : CommandSettingsBase
        {
            logger.Information("--------------------------------------");
            logger.Information("Telemetry Note:");
            if (settings.DisableTelemetry)
            {
                logger.Information("   Telemetry is disabled by the user.");
            }
            else
            {
                logger.Information("   We use Application Insights to collect usage and error information in order to improve the quality of the tools.");
                logger.Information("   Currently we collect the following anonymous data:");
                logger.Information("     -Event data: application version, client city/country, hosting type, item count, error count, warning count, elapsed time.");
                logger.Information("     -Exceptions: application errors and warnings.");
                logger.Information("     -Dependencies: REST/ObjectModel calls to Azure DevOps to help us understand performance issues.");
                logger.Information("   This data is tied to a session ID that is generated on each run of the application and shown in the logs. This can help with debugging. If you want to disable telemetry you can run the tool with '--disableTelemetry' on the command prompt.");
                logger.Information("   Note: Exception data cannot be 100% guaranteed to not leak production data");
            }

            logger.Information("--------------------------------------");
        }

    }
}
