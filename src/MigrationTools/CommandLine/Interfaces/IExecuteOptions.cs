using CommandLine;

namespace MigrationTools.CommandLine.Interfaces
{
    public interface IExecuteOptions
    {

        string ConfigFile { get; set; }
        string SourceDomain { get; set; }
        string SourceUserName { get; set; }
        string SourcePassword { get; set; }
        string TargetDomain { get; set; }
        string TargetUserName { get; set; }
        string TargetPassword { get; set; }
    }
}