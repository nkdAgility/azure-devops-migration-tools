using MigrationTools.DataContracts;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndPoint : WorkItemEndpoint
    {
        public TfsWorkItemEndPoint(TfsWorkItemEndPointOptions endpointOptions) : base(endpointOptions)
        {
        }

        public override void PersistWorkItem(WorkItemData2 source)
        {
            throw new System.NotImplementedException();
        }
    }
}