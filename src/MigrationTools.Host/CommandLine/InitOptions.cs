using CommandLine;

namespace MigrationTools.Host.CommandLine
{
    [Verb("init", HelpText = "Creates initial config file")]
    public class InitOptions
    {
        [Option('c', "config", Required = false, HelpText = "Configuration file to be processed.")]
        public string ConfigFile { get; set; }

        [Option('o', "options", Required = false, Default = OptionsMode.WorkItemTracking, HelpText = "Configuration file to be processed.")]
        public OptionsMode Options { get; set; }
    }
}