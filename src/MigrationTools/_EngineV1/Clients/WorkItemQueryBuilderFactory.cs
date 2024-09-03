using System;
using Microsoft.Extensions.DependencyInjection;

namespace MigrationTools.Clients
{
    public class WorkItemQueryBuilderFactory : IWorkItemQueryBuilderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WorkItemQueryBuilderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IWorkItemQueryBuilder Create()
        {
            return _serviceProvider.GetService<IWorkItemQueryBuilder>();
        }
    }
}