using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Core.Configuration;

namespace MigrationTools.Core.Engine.Containers.Tests
{
    public class SimpleFieldMapConfigMock : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string FieldMap
        {
            get
            {
                return "SimpleFieldMapMock";
            }
        }
    }
}
