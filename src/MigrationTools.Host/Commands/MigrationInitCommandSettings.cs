using System.ComponentModel;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class MigrationInitCommandSettings : CommandSettingsBase
    {
        [Description("What type of config do you want to output? WorkItemTracking is the default.")]
        [CommandOption("--outputMode|--options")]
        [DefaultValue(OptionsMode.WorkItemTracking)]
        public OptionsMode Options { get; set; }
    }

    public enum OptionsMode
    {
        Reference = 0,
        WorkItemTracking = 1,
        Fullv2 = 2,
        WorkItemTrackingv2 = 3,
        Basic = 4
    }
}