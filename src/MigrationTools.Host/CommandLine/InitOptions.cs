using CommandLine;

namespace MigrationTools.Host.CommandLine
{
    [Verb("init", HelpText = "Creates initial config file")]
    public class InitOptions
    {
        [Option('c', "config", Required = false, HelpText = "Configuration file to be processed.")]
        public string ConfigFile { get; set; }

        [Option('o', "options", Required = false, Default = OptionsMode.Basic, HelpText = "Configuration file to be generated: Basic, Reference, WorkItemTracking, Fullv2, WorkItemTrackingv2")]
        public OptionsMode Options { get; set; }
    }
}