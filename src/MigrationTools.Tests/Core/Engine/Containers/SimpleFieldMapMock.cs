using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Engine.Containers;

namespace MigrationTools.Core.Engine.Containers.Tests
{
   public class SimpleFieldMapMock : IFieldMap
    {
        protected IFieldMapConfig _Config;

        public virtual void Configure(IFieldMapConfig config)
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
