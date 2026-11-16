using System.Collections.Generic;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public interface IFieldMapOptions : IOptions
    {
        /// <summary>
        /// A list of Work Item Types that this Field Map will apply to. Use "*" to apply it to all Work Item Types. A field map with an empty list applies to no Work Item Types and never runs.
        /// </summary>
        [JsonProperty(Order = -1)]
        List<string> ApplyTo { get; set; }

    }
}