using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.Enrichers;
using MigrationTools.Tools.Infra;

namespace MigrationTools.Tools
{
    public class TfsGitRepositoryToolOptions : ToolOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsGitRepositoryTool";
     
    }
}