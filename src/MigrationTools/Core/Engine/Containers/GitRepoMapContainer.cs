using Microsoft.Extensions.Hosting;
using MigrationTools.Core.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MigrationTools.Core.Engine.Containers
{
   public class GitRepoMapContainer : EngineContainer<ReadOnlyDictionary<string, string>>
    {

        public Dictionary<string, string> _GitRepoMaps { get; set; }

        public override ReadOnlyDictionary<string, string> Items { get { return new ReadOnlyDictionary<string, string>(_GitRepoMaps); } }

        public GitRepoMapContainer(IServiceProvider services, EngineConfiguration config) : base(services, config)
        {
        }

        protected override void Configure()
        {
            if (Config.GitRepoMapping != null)
            {
                _GitRepoMaps = Config.GitRepoMapping;
            }
        }


    }
}
