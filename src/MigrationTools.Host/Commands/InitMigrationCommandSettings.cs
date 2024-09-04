using System.ComponentModel;
using MigrationTools.Options;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class InitMigrationCommandSettings : CommandSettingsBase
    {
        [Description("What type of config do you want to output? WorkItemTracking is the default.")]
        [CommandOption("--template|-t|--outputMode|--options")]
        [DefaultValue(OptionsConfigurationTemplate.WorkItemTracking)]
        public OptionsConfigurationTemplate Template { get; set; }

        [Description("Add to overwrite the existing file.")]
        [CommandOption("--overwrite|-o")]
        [DefaultValue(false)]
        public bool Overwrite { get; set; }

    }

}