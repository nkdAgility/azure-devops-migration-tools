using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Endpoints
{
    public interface IEndpointFactory
    {
        IEndpoint CreateEndpoint(string name);
    }
}
