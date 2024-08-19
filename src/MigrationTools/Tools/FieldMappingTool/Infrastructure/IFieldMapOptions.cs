using System.Collections.Generic;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public interface IFieldMapOptions : IOptions
    {
        List<string> ApplyTo { get; set; }

        void SetExampleConfigDefaults();

    }
}