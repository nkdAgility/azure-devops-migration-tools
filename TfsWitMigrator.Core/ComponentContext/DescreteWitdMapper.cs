using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace VSTS.DataBulkEditor.Core
{
    public class DescreteWitdMapper : IWitdMapper
    {
        private string _MapTo;

        public DescreteWitdMapper(string mapTo)
        {
            this._MapTo = mapTo;
        }

        public string Map(WorkItem sourceWI)
        {
            return _MapTo;
        }
    }
}