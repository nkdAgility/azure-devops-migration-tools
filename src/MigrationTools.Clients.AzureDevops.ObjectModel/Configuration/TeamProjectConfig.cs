using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Configuration
{
    public class TeamProjectConfig : IMigrationClientConfig
    {
        public Uri Collection { get; set; }
        public string Project { get; set; }
        public string ReflectedWorkItemIDFieldName { get; set; }
        public bool AllowCrossProjectLinking { get; set; }
        public string PersonalAccessToken { get; set; }
        public LanguageMaps LanguageMaps { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MigrationClientClientDirection Direction { get; set; }

        Type IMigrationClientConfig.MigrationClient => throw new NotImplementedException();
    }

    public class LanguageMaps
    {
        public string AreaPath { get; set; }
        public string IterationPath { get; set; }
    }
}