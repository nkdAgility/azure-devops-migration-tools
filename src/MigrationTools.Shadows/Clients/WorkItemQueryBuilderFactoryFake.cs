using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigrationTools.Clients;

namespace MigrationTools.Clients.Shadows
{
    public class WorkItemQueryBuilderFactoryFake : IWorkItemQueryBuilderFactory
    {
        public IWorkItemQueryBuilder Create()
        {
            throw new NotImplementedException();
        }
    }
}
