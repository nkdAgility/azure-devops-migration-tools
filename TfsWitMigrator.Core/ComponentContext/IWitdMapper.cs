using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace VSTS.DataBulkEditor.Core
{
    public interface IWitdMapper
    {
        string Map(WorkItem sourceWI);
    }
}