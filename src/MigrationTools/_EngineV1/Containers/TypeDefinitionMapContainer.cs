using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools._EngineV1.Containers
{
    public class TypeDefinitionMapContainer : EngineContainer<ReadOnlyDictionary<string, IWitdMapper>>
    {
        private Dictionary<string, IWitdMapper> _TypeDefinitions = new Dictionary<string, IWitdMapper>();
        private readonly ILogger<TypeDefinitionMapContainer> _logger;

        public override ReadOnlyDictionary<string, IWitdMapper> Items { get { return new ReadOnlyDictionary<string, IWitdMapper>(_TypeDefinitions); } }

        public TypeDefinitionMapContainer(IServiceProvider services, IOptions<EngineConfiguration> config, ILogger<TypeDefinitionMapContainer> logger) : base(services, config)
        {
            _logger = logger;
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
                _logger.LogDebug("TypeDefinitionMapContainer: Adding Work Item Type {WorkItemType}", workItemTypeName);
                _TypeDefinitions.Add(workItemTypeName, workItemTypeDefinitionMap);
            }
        }
    }
}