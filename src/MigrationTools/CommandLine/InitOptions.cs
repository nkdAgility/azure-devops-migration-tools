using CommandLine;

namespace MigrationTools.CommandLine
{
    [Verb("init", HelpText = "Creates initial config file")]
    public class InitOptions : CommonOptions, Interfaces.IInitOptions
    {
        [Option('c', "config", Required = false, HelpText = "Configuration file to be processed.")]
        public string ConfigFile { get; set; }

        [Option('o', "options", Required = false, Default = OptionsMode.WorkItemTracking, HelpText = "Configuration file to be processed.")]
        public OptionsMode Options { get; set; }

    }


}