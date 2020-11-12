using System;
using Newtonsoft.Json;

namespace MigrationTools.Options
{
    public interface IOptions
    {
        [JsonIgnoreAttribute]
        Type ToConfigure { get; }

        void SetDefaults();
    }
}