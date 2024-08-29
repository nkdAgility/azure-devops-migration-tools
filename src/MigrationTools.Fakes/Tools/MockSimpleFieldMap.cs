using System;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.DataContracts;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools.Shadows
{
    public class MockSimpleFieldMap : IFieldMap
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

        public string MappingDisplayName => "MockSimpleFieldMap";

        public void Execute(WorkItemData source, WorkItemData target)
        {
            throw new NotImplementedException();
        }
    }
}