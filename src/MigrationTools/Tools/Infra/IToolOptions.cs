using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infra
{
    public interface IToolOptions : IOptions
    {
        [JsonIgnore]
        bool Enabled { get; set; }
    }
}
