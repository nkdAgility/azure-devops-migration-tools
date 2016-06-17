using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace VSTS.DataBulkEditor.Engine
{
    public interface IWitdMapper
    {
        string Map(WorkItem sourceWI);
    }
}