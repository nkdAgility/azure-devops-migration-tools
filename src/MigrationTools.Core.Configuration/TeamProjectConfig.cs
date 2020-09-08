using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsMigrationTools.Core.Configuration
{
    public class TeamProjectConfig
    {
        public Uri Collection { get; set; }
        public string Project { get; set; }
        public string ReflectedWorkItemIDFieldName { get; set; }
        public bool AllowCrossProjectLinking { get; set; }
        public string PersonalAccessToken { get; set; }
        public LanguageMaps LanguageMaps { get; set; }
    }

    public class LanguageMaps
    {
        public string AreaPath { get; set; }
        public string IterationPath { get; set; }
    }
}
