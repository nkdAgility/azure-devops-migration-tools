using System.ComponentModel;
using Microsoft.VisualStudio.Services.Common.CommandLine;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class ExecuteMigrationCommandSettings : CommandSettingsBase
    {
        [Description("Domain used to connect to the source TFS instance.")]
        [CommandOption("--sourceDomain")]
        public string SourceDomain { get; set; }

        [Description("User Name used to connect to the source TFS instance.")]
        [CommandOption("--sourceUserName")]
        public string SourceUserName { get; set; }

        [Description("Password used to connect to source TFS instance.")]
        [CommandOption("--sourcePassword")]
        public string SourcePassword { get; set; }

        [Description("Domain used to connect to the target TFS instance.")]
        [CommandOption("--targetDomain")]
        public string TargetDomain { get; set; }

        [Description("User Name used to connect to the target TFS instance.")]
        [CommandOption("--targetUserName")]
        public string TargetUserName { get; set; }

        [Description("Password used to connect to target TFS instance.")]
        [CommandOption("--targetPassword")]
        public string TargetPassword { get; set; }
    }
}