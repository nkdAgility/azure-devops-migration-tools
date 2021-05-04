using System;
using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public class RefEndpointOptions : IEndpointOptions
    {
        public List<IEndpointEnricherOptions> EndpointEnrichers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RefName { get; set; }

        public Type ToConfigure => throw new NotImplementedException();

        public void SetDefaults()
        {
            throw new NotImplementedException();
        }
    }
}