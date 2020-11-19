using CommandLine;
namespace MigrationTools.CommandLine {
    public class CommonOptions : Interfaces.ICommonOptions {
[Option("skipUpdateCheck", Required = false, HelpText = "Skip Update Check", Default = (bool)false)]
        public bool SkipUpdateCheck {get; set;}
    }
}