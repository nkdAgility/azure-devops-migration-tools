using System;
using System.ComponentModel.DataAnnotations;
using MigrationTools.Endpoints.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TfsUrlParser;

namespace MigrationTools.Endpoints
{
    public class TfsEndpointOptions : EndpointOptions
    {
        [JsonProperty(Order = -3)]
        [Required]
        public Uri Collection { get; set; }
        [JsonProperty(Order = -2)]
        [Required]
        public string Project { get; set; }

        [Required]
        public TfsAuthenticationOptions Authentication { get; set; }

        [JsonProperty(Order = -1)]
        [Required]
        public string ReflectedWorkItemIdField { get; set; }
        public bool AllowCrossProjectLinking { get; set; }

        [Required]
        public TfsLanguageMapOptions LanguageMaps { get; set; }

        //[JsonIgnore]
        //public string CollectionName { get { return GetCollectionName(); } }

        //public string GetCollectionName()
        //{
        //    //var repositoryDescription =  new RepositoryDescription(Collection);
        //    //return repositoryDescription.CollectionName;
        //    // Pending fix from https://github.com/bbtsoftware/TfsUrlParser
        //    return Collection != null ? Collection.ToString() : "https://dev.azure.com/sampleAccount";
        //}

    }
}