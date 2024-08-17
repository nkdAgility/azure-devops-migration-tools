using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.Enrichers;

namespace MigrationTools.Tools
{
    public class TfsGitRepositoryToolOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsGitRepositoryTool";
        public override Type ToConfigure => typeof(TfsGitRepositoryTool);

        public override void SetDefaults()
        {
            Enabled = true;
        }

        static public TfsGitRepositoryToolOptions GetDefaults()
        {
            var result = new TfsGitRepositoryToolOptions();
            result.SetDefaults();
            return result;
        }
    }
}