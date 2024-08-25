using System;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TfsUrlParser;

namespace MigrationTools.Endpoints
{
    public class TfsTeamProjectEndpointOptions : EndpointOptions
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

        public NetworkCredentials NetworkCredentials { get; set; }

        public string CollectionName { get { return GetCollectionName(); } }

        public string GetCollectionName()
        {
            //var repositoryDescription =  new RepositoryDescription(Collection);
            //return repositoryDescription.CollectionName;
            // Pending fix from https://github.com/bbtsoftware/TfsUrlParser
            return Collection != null ? Collection.ToString() : "https://dev.azure.com/sampleAccount";
        }

    }
}