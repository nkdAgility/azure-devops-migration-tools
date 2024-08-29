﻿using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints.Infrastructure;

namespace MigrationTools.Endpoints
{
    public interface IEndpoint
    {
        //void Configure(IEndpointOptions options);

        EndpointEnricherContainer EndpointEnrichers { get; }
    }

    public interface ISourceEndPoint : IEndpoint
    {
        int Count { get; }
        IEnumerable<IEndpointSourceEnricher> SourceEnrichers { get; }
    }

    public interface ITargetEndPoint : IEndpoint
    {
        IEnumerable<IEndpointTargetEnricher> TargetEnrichers { get; }
    }
}