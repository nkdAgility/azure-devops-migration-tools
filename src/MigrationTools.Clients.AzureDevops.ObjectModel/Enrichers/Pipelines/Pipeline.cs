using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MigrationTools.Enrichers.Pipelines
{
    public class Pipeline
    {
        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("revision")]
        public long Revision { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("folder")]
        public string Folder { get; set; }

        [JsonProperty("configuration")]
        public ConfigurationType Configuration { get; set; }
    }

    public class ConfigurationType
    {
        [JsonProperty("designerHyphenJson")]
        public string DesignerHyphenJson { get; set; }

        [JsonProperty("designerJson")]
        public string DesignerJson { get; set; }

        [JsonProperty("justInTime")]
        public string JustInTime { get; set; }

        [JsonProperty("unknown")]
        public string Unknown { get; set; }

        [JsonProperty("yaml")]
        public string Yaml { get; set; }
    }
    public partial class Links
    {
        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("web")]
        public Self Web { get; set; }
    }

    public partial class Self
    {
        [JsonProperty("href")]
        public Uri Href { get; set; }
    }
}
