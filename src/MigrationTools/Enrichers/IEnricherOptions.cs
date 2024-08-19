using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Enrichers
{
    public interface IEnricherOptions : IOldOptions, IOptions
    {
        /// <summary>
        /// Active the enricher if it true.
        /// </summary>
        [JsonProperty(Order = -2)]
        bool Enabled { get; set; }
    }
}