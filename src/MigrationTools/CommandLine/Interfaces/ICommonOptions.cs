using CommandLine;

namespace MigrationTools.CommandLine.Interfaces
{
    public interface ICommonOptions
    {
        bool SkipUpdateCheck { get; set; }
    }

}