using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace VstsSyncMigrator.Engine
{
    public class DiscreteWitMapper : IWitdMapper
    {
        private string _MapTo;

        public DiscreteWitMapper(string mapTo)
        {
            this._MapTo = mapTo;
        }

        public string Map(WorkItem sourceWI)
        {
            return _MapTo;
        }
    }
}