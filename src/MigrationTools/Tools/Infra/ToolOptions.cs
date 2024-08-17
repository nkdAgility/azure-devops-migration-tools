using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Tools.Infra
{
    public abstract class ToolOptions : IToolOptions
    {
        public bool Enabled { get; set; }
    }
}
