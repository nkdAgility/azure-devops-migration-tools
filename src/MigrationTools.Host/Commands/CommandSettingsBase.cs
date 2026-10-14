using System.ComponentModel;
using Newtonsoft.Json;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;
using System.CommandLine;

namespace MigrationTools.Host.Commands
{
    internal class CommandSettingsBase : CommandSettings
    {
        [Description("Pre configure paramiters using this config file. Run `Init` to create it.")]
        [CommandOption("--config|--configFile|-c")]
        [DefaultValue("configuration.json")]
        [JsonIgnore, YamlIgnore]
        public string ConfigFile { get; set; }

        [Description("Add this paramiter to turn Telemetry off")]
        [CommandOption("--disableTelemetry")]
        public bool DisableTelemetry { get; set; }

        [Description("Enabled additional trace")]
        [CommandOption("--debugTrace")]
        public bool DebugTrace { get; set; }

        [Description("Add this paramiter to turn version check off")]
        [CommandOption("--skipVersionCheck")]
        public bool skipVersionCheck { get; set; }

        [Description("Folder to write the logs to. A `logs/{timestamp}` folder is created inside it. Defaults to the current working directory.")]
        [CommandOption("--logPath")]
        [JsonIgnore, YamlIgnore]
        public string LogPath { get; set; }

        public static string ForceGetLogPath(string[] args)
        {
            var logPathOption = new Option<string?>("--logPath");

            var rootCommand = new RootCommand();
            rootCommand.AddOption(logPathOption);

            return rootCommand.Parse(args).GetValueForOption<string>(logPathOption);
        }

        public static string ForceGetConfigFile(string[] args)
        {
            var fileOption = new Option<string?>("--config");
            fileOption.AddAlias("-c");
            fileOption.AddAlias("--configFile");

            var rootCommand = new RootCommand();
            rootCommand.AddOption(fileOption);

            var file = rootCommand.Parse(args);
            return file.GetValueForOption<string>(fileOption);

        }
    }

}
