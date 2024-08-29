using System.Collections.Generic;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public interface IFieldMapOptions : IOptions
    {
        /// <summary>
        /// A list of Work Item Types that this Field Map will apply to. If the list is empty it will apply to all Work Item Types. You can use "*" to apply to all Work Item Types.
        /// </summary>
        [JsonProperty(Order = -1)]
        List<string> ApplyTo { get; set; }

    }
}