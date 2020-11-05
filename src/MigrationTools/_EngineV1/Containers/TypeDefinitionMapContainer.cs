using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MigrationTools._EngineV1.Configuration;
using Serilog;

namespace MigrationTools._EngineV1.Containers
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
                    AddWorkItemTypeDefinition(key, new WitMapper(Config.WorkItemTypeDefinition[key]));
                }
            }
        }

        public void AddWorkItemTypeDefinition(string workItemTypeName, IWitdMapper workItemTypeDefinitionMap = null)
        {
            if (!_TypeDefinitions.ContainsKey(workItemTypeName))
            {
                Log.Debug("TypeDefinitionMapContainer: Adding Work Item Type {WorkItemType}", workItemTypeName);
                _TypeDefinitions.Add(workItemTypeName, workItemTypeDefinitionMap);
            }
        }
    }
}