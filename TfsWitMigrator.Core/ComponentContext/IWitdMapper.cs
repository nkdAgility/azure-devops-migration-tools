using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsWitMigrator.Core
{
    public interface IWitdMapper
    {
        string Map(WorkItem sourceWI);
    }
}