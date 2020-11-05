using System;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients
{
    public abstract class ReflectedWorkItemId
    {
        private string _WorkItemId;

        public ReflectedWorkItemId(WorkItemData workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _WorkItemId = workItem.Id;
        }

        public ReflectedWorkItemId(string ReflectedWorkItemId)
        {
            if (ReflectedWorkItemId is null)
            {
                throw new ArgumentNullException(nameof(ReflectedWorkItemId));
            }
            _WorkItemId = ReflectedWorkItemId;
        }

        public override string ToString()
        {
            return WorkItemId;
        }

        public string WorkItemId
        {
            get
            {
                return _WorkItemId;
            }
        }
    }
}