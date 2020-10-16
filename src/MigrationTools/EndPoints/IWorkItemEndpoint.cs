using MigrationTools.Clients;

namespace MigrationTools.EndPoints
{
    public interface IWorkItemEndpoint : IEndpoint
    {
        void Configure(IWorkItemQuery query);
    }
}