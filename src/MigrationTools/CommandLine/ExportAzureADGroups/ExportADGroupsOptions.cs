using CommandLine;

namespace MigrationTools.CommandLine
{
    [Verb("exportADGroups", HelpText = "Iterates through a collection and lists all AD groups assigned but that have not been synced to AzureAD.")]
    public class ExportADGroupsOptions : OptionsBase
    {
        [Option('p', "teamproject", Required = false, HelpText = "Optional team project.")]
        public string TeamProject { get; set; }
    }
}
