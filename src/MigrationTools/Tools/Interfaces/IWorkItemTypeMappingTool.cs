using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Tools.Interfaces
{
    public interface IWorkItemTypeMappingTool
    {
        Dictionary<string, string> Mappings { get; }
    }
}
