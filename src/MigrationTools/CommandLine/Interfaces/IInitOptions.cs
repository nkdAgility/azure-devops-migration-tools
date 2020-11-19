using CommandLine;

namespace MigrationTools.CommandLine.Interfaces
{
    public interface IInitOptions
    {
        public string ConfigFile { get; set; }
        public OptionsMode Options { get; set; }
    }
}