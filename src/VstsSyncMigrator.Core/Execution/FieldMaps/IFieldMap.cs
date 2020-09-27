using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine.ComponentContext
{
   public interface IFieldMap
    {
        string Name { get; }
        string MappingDisplayName { get; }

        void Configure(IFieldMapConfig config);
        void Execute(WorkItem source, WorkItem target);
    }
}
