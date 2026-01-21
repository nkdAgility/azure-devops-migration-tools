using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.Host.Services;
using MigrationTools.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class VersionCommand : CommandBase<VersionCommandSettings>
    {
        private readonly IMigrationToolVersion _migrationToolVersion;

        public VersionCommand(
            IHostApplicationLifetime appLifetime,
            IServiceProvider services,
            IDetectOnlineService detectOnlineService,
            IDetectVersionService2 detectVersionService,
            ILogger<CommandBase<VersionCommandSettings>> logger,
            ITelemetryLogger telemetryLogger,
            IMigrationToolVersion migrationToolVersion,
            IConfiguration configuration,
            ActivitySource activitySource)
            : base(appLifetime, services, detectOnlineService, detectVersionService, logger, telemetryLogger,
                migrationToolVersion, configuration, activitySource)
        {
            _migrationToolVersion = migrationToolVersion;
        }

        internal override Task<int> ExecuteInternalAsync(CommandContext context, VersionCommandSettings settings)
        {
            var versionInfo = _migrationToolVersion.GetRunningVersion();

            AnsiConsole.MarkupLine($"[bold cyan]Version:[/] {Markup.Escape(versionInfo.versionString)}");

            if (versionInfo.version.Major == 0)
            {
                AnsiConsole.MarkupLine($"[dim]Git Tag:[/] {Markup.Escape(ThisAssembly.Git.Tag)}");
                AnsiConsole.MarkupLine($"[dim]Git Branch:[/] {Markup.Escape(ThisAssembly.Git.Branch)}");
                AnsiConsole.MarkupLine($"[dim]Git Commits:[/] {Markup.Escape(ThisAssembly.Git.Commits)}");
            }

            return Task.FromResult(0);
        }
    }
}
