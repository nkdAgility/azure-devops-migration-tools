using System;
using MigrationTools.Enrichers;
using MigrationTools.Tools.Infra;

namespace MigrationTools.Tools
{
    public class TfsChangeSetMappingToolOptions : ToolOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsChangeSetMappingTool";

        public string ChangeSetMappingFile { get; set; }

    }
}