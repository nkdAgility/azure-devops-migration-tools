using System;
using MigrationTools._EngineV1.DataContracts;

namespace MigrationTools._EngineV1.Clients
{
    public abstract class ReflectedWorkItemId
    {
        protected string _WorkItemId;

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

        protected ReflectedWorkItemId()
        {
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