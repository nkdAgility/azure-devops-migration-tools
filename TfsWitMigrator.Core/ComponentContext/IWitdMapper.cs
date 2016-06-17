using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace _VSTS.DataBulkEditor.Engine
{
    public interface IWitdMapper
    {
        string Map(WorkItem sourceWI);
    }
}