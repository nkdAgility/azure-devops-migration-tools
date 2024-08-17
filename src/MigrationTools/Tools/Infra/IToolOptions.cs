using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Options;

namespace MigrationTools.Tools.Infra
{
    public interface IToolOptions : IOptions
    {
        bool Enabled { get; set; }
    }
}
