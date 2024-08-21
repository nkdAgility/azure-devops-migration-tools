using System.ComponentModel;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class InitMigrationCommandSettings : CommandSettingsBase
    {
        [Description("What type of config do you want to output? WorkItemTracking is the default.")]
        [CommandOption("--outputMode|--options")]
        [DefaultValue(OptionsMode.WorkItemTracking)]
        public OptionsMode Options { get; set; }

        [Description("Add to overwirte the existing file.")]
        [CommandOption("--overwrite")]
        [DefaultValue(false)]
        public bool Overwrite { get; set; }

    }

    public enum OptionsMode
    {
        Reference = 0,
        WorkItemTracking = 1,
        Basic = 4
    }
}