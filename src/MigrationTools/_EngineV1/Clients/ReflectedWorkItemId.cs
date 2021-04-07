using System;
using MigrationTools.DataContracts;

namespace MigrationTools._EngineV1.Clients
{
    public abstract class ReflectedWorkItemId
    {
        public ReflectedWorkItemId(WorkItemData workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            WorkItemId = workItem.Id;
        }

        public ReflectedWorkItemId(string ReflectedWorkItemId)
        {
            if (ReflectedWorkItemId is null)
            {
                throw new ArgumentNullException(nameof(ReflectedWorkItemId));
            }
            WorkItemId = ReflectedWorkItemId;
        }

        public ReflectedWorkItemId(int ReflectedWorkItemId)
        {
            if (ReflectedWorkItemId == 0)
            {
                throw new ArgumentNullException(nameof(ReflectedWorkItemId));
            }
            WorkItemId = ReflectedWorkItemId.ToString();
        }

        public override string ToString()
        {
            return WorkItemId;
        }

        public string WorkItemId
        {
            get; private set;
        }
    }
}