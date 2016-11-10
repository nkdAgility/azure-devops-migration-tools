using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace VstsSyncMigrator.Engine
{
    public interface IWitdMapper
    {
        string Map(WorkItem sourceWI);
    }
}