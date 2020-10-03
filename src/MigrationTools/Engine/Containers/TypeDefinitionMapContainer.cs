using Microsoft.Extensions.Hosting;
using MigrationTools.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MigrationTools.Engine.Containers
{
   public class TypeDefinitionMapContainer : EngineContainer<ReadOnlyDictionary<string, IWitdMapper>>
    {

        private Dictionary<string, IWitdMapper> _TypeDefinitions = new Dictionary<string, IWitdMapper>();

        public override ReadOnlyDictionary<string, IWitdMapper> Items { get { return new ReadOnlyDictionary<string, IWitdMapper>(_TypeDefinitions); } }

        public TypeDefinitionMapContainer(IServiceProvider services, EngineConfiguration config) : base(services, config)
        {
        }

        protected override void Configure()
        {
            if (Config.WorkItemTypeDefinition != null)
            {
                foreach (string key in Config.WorkItemTypeDefinition.Keys)
                {
                    Log.Information("{Context}: Adding Work Item Type {WorkItemType}", key, "MigrationEngine");
                    AddWorkItemTypeDefinition(key, new WitMapper(Config.WorkItemTypeDefinition[key]));
                }
            }
        }

        public void AddWorkItemTypeDefinition(string workItemTypeName, IWitdMapper workItemTypeDefinitionMap = null)
        {
            if (!_TypeDefinitions.ContainsKey(workItemTypeName))
            {
                _TypeDefinitions.Add(workItemTypeName, workItemTypeDefinitionMap);
            }
        }

    }
}
