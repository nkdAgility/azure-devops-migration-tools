using System.Collections.Generic;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public interface IFieldMapOptions : IOptions
    {
        [JsonProperty(Order = -1)]
        List<string> ApplyTo { get; set; }

    }
}