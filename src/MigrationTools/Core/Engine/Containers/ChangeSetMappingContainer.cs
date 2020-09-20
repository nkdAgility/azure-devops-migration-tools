using Microsoft.Extensions.Hosting;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MigrationTools.Core.Engine.Containers
{
    public class ChangeSetMappingContainer : EngineContainer<ReadOnlyDictionary<int, string>>
    {
        public readonly Dictionary<int, string> _ChangeSetMappings = new Dictionary<int, string>();
        public override ReadOnlyDictionary<int,string> Items { get { return new ReadOnlyDictionary<int, string>(_ChangeSetMappings); } }
        public int Count { get { return _ChangeSetMappings.Count; } }

        public ChangeSetMappingContainer(IServiceProvider services, EngineConfiguration config) : base(services, config)
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
