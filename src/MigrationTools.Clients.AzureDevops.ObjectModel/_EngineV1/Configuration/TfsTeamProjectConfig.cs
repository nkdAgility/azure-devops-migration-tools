using System;
using MigrationTools.Endpoints;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools._EngineV1.Configuration
{
    public class TfsTeamProjectConfig : IMigrationClientConfig
    {
        public Uri Collection { get; set; }
        public string Project { get; set; }
        public string ReflectedWorkItemIDFieldName { get; set; }
        public bool AllowCrossProjectLinking { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationMode AuthenticationMode { get; set; }

        public string PersonalAccessToken { get; set; }
        public string PersonalAccessTokenVariableName { get; set; }
        public TfsLanguageMapOptions LanguageMaps { get; set; }

        public IMigrationClientConfig PopulateWithDefault()
        {
            Project = "myProjectName";
            AllowCrossProjectLinking = false;
            Collection = new Uri("https://dev.azure.com/nkdagility-preview/");
            ReflectedWorkItemIDFieldName = "Custom.ReflectedWorkItemId";
            PersonalAccessToken = "";
            PersonalAccessTokenVariableName = "";
            AuthenticationMode = AuthenticationMode.Prompt;
            LanguageMaps = new TfsLanguageMapOptions() { AreaPath = "Area", IterationPath = "Iteration" };
            return this;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Collection, Project);
        }
    }
}