using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSTS.DataBulkEditor.Engine.Configuration.Processing
{
    public interface ITfsProcessingConfig
    {
        bool Enabled { get; set; }
        [JsonIgnoreAttribute]
        Type Processor { get; }

    }
}
