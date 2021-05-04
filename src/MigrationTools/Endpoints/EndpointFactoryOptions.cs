using System;
using System.Collections.Generic;

namespace MigrationTools.Endpoints
{
    public class EndpointFactoryOptions
    {
        public IList<Func<IServiceProvider, IEndpoint>> EndpointFuncs { get; } = new List<Func<IServiceProvider, IEndpoint>>();
    }
}
