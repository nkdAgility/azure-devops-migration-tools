using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools.Shadows
{
    public class MockWorkItemTypeMappingTool : IWorkItemTypeMappingTool
    {
        public Dictionary<string, string> Mappings => throw new NotImplementedException();
    }
}
