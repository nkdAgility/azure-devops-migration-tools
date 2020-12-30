using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools._EngineV1.Containers
{
    public class ChangeSetMappingContainer : EngineContainer<ReadOnlyDictionary<int, string>>
    {
        private Dictionary<int, string> _ChangeSetMappings = new Dictionary<int, string>();
        public override ReadOnlyDictionary<int, string> Items { get { return new ReadOnlyDictionary<int, string>(_ChangeSetMappings); } }
        public int Count { get { return _ChangeSetMappings.Count; } }

        public ChangeSetMappingContainer(IServiceProvider services, IOptions<EngineConfiguration> config) : base(services, config)
        {
        }

        protected override void Configure()
        {
            if (Config.ChangeSetMappingFile != null)
            {
                if (System.IO.File.Exists(Config.ChangeSetMappingFile))
                {
                    IChangeSetMappingProvider csmp = new ChangeSetMappingProvider(Config.ChangeSetMappingFile);
                    csmp.ImportMappings(_ChangeSetMappings);
                }
            }
        }
    }
}