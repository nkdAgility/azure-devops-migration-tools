using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MigrationTools.Options
{

    public interface IOptions
    {
        [JsonIgnore]
        public ConfigurationMetadata ConfigurationMetadata { get; }

        /// <summary>
        /// Will be used if enabled
        /// </summary>
        [JsonProperty(Order = -200)]
        bool Enabled { get; set; }

        //public void SetExampleConfigSimple();
        //public void SetExampleConfigFull();

    }

}