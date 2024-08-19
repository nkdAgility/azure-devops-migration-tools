using System;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.DataContracts;

namespace MigrationTools.Engine.Containers.Tests
{
    public class SimpleFieldMapMock : IFieldMap
    {
        protected IFieldMapOptions _Config;

        public virtual void Configure(IFieldMapOptions config)
        {
            _Config = config;
        }

        public string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public string MappingDisplayName => "SimpleFieldMapMock";

        public void Execute(WorkItemData source, WorkItemData target)
        {
            throw new NotImplementedException();
        }
    }
}