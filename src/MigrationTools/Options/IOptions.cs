using System;
using Newtonsoft.Json;

namespace MigrationTools.Options
{
    public interface IOptions
    {
        /// <summary>
        /// If you set a `RefName` then this configration will be added to a Catalog of configurations that can be refernced using tha `RefName` so tha tyou dont have to keep adding the ame items with the same configuration.
        /// </summary>
        public string RefName { get; set; }

        [JsonIgnoreAttribute]
        Type ToConfigure { get; }

        void SetDefaults();
    }
}