using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Host.Services;
using MigrationTools.Options;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Services;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class UpgradeConfigCommand : CommandBase<UpgradeConfigCommandSettings>
    {


        private readonly ILogger _logger;

       

        public UpgradeConfigCommand(IHostApplicationLifetime appLifetime, IServiceProvider services, IDetectOnlineService detectOnlineService, IDetectVersionService2 detectVersionService, ILogger<CommandBase<UpgradeConfigCommandSettings>> logger, ITelemetryLogger telemetryLogger, IMigrationToolVersion migrationToolVersion, IConfiguration configuration, ActivitySource activitySource) : base(appLifetime, services, detectOnlineService, detectVersionService, logger, telemetryLogger, migrationToolVersion, configuration, activitySource)
        {
            _logger = logger;
        }

        internal override async Task<int> ExecuteInternalAsync(CommandContext context, UpgradeConfigCommandSettings settings)
        {
            CommandActivity.SetTagsFromObject(settings);
            int _exitCode = 0;
            string configFile = settings.ConfigFile;
            if (string.IsNullOrEmpty(configFile))
            {
                configFile = "configuration.json";
            }
            _logger.LogInformation("ConfigFile: {configFile}", configFile);

            OptionsConfigurationBuilder optionsBuilder = Services.GetRequiredService<OptionsConfigurationBuilder>();
            OptionsConfigurationUpgrader optionsUpgrader = Services.GetRequiredService<OptionsConfigurationUpgrader>();

            optionsUpgrader.UpgradeConfiguration(optionsBuilder);

            string json = optionsBuilder.Build();
            configFile = AddSuffixToFileName(configFile, "-upgraded");
            File.WriteAllText(configFile, json);
            _logger.LogInformation("New {configFile} file has been created", configFile);
            Console.WriteLine(json);
            return _exitCode;
        }


        static string AddSuffixToFileName(string filePath, string suffix)
        {
            // Get the directory path
            string directory = Path.GetDirectoryName(filePath);

            // Get the file name without the extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            // Get the file extension
            string extension = Path.GetExtension(filePath);

            // Combine them to create the new file name
            string newFileName = $"{fileNameWithoutExtension}{suffix}{extension}";

            // Combine the directory with the new file name
            return Path.Combine(directory, newFileName);
        }



    }
}
