using System;
using Newtonsoft.Json;

namespace MigrationTools.Enrichers
{
    public interface IEnricherOptions
    {
        /// <summary>
        /// Active the enricher if it true.
        /// </summary>
        bool Enabled { get; set; }

        [JsonIgnoreAttribute]
        public Type ToConfigure { get; }

        void SetDefaults();
    }
}