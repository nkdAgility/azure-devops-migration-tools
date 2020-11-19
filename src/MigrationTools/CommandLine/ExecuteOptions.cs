using CommandLine;

namespace MigrationTools.CommandLine
{
    [Verb("execute", HelpText = "Record changes to the repository.")]
    public class ExecuteOptions : CommonOptions, Interfaces.IExecuteOptions
    {
        [Option('c', "config", Required = true, HelpText = "Configuration file to be processed.")]
        public string ConfigFile { get; set; }

        [Option("sourceDomain", Required = false, HelpText = "Domain used to connect to the source TFS instance.")]
        public string SourceDomain { get; set; }

        [Option("sourceUserName", Required = false, HelpText = "User Name used to connect to the source TFS instance.")]
        public string SourceUserName { get; set; }

        [Option("sourcePassword", Required = false, HelpText = "Password used to connect to source TFS instance.")]
        public string SourcePassword { get; set; }

        [Option("targetDomain", Required = false, HelpText = "Domain used to connect to the target TFS instance.")]
        public string TargetDomain { get; set; }

        [Option("targetUserName", Required = false, HelpText = "User Name used to connect to the target TFS instance.")]
        public string TargetUserName { get; set; }

        [Option("targetPassword", Required = false, HelpText = "Password used to connect to target TFS instance.")]
        public string TargetPassword { get; set; }
        
    }
}