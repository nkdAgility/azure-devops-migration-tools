using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.Endpoints;

namespace MigrationTools.EndPoints
{
    public class EndPointContainer : List<IEndpoint>
    {
        public EndPointContainer(IServiceProvider services, ITelemetryLogger telemetry, ILogger<EndPointContainer> logger)
        {
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<EndPointContainer> Log { get; }

        public void ConfigureEndpoint(IEndpointOptions endpointOptions)
        {
            var ep = (IWorkItemEndPoint)Services.GetRequiredService(endpointOptions.ToConfigure);
            ep.Configure(endpointOptions);
            Add(ep);
            Log.LogInformation("ConfigureEndpoint: {EndPointName} {Direction} : Enrichers{EnrichersCount}: {EnrichersList} ", ep.GetType().Name, endpointOptions.Direction, endpointOptions.Enrichers.Count, endpointOptions.Enrichers.Select(x => x.GetType().Name));
        }

        public void ConfigureEndpoints(List<IEndpointOptions> endpoints, bool sourceRequired = true, bool targetRequired = false)
        {
            if (endpoints is null)
            {
                Log.LogWarning("ConfigureEndpoints: No Endpoints have been Configured");
            }
            else
            {
                ValidateDirection(endpoints, EndpointDirection.Target, targetRequired);
                ValidateDirection(endpoints, EndpointDirection.Source, sourceRequired);
                foreach (IEndpointOptions item in endpoints)
                {
                    ConfigureEndpoint(item);
                }
            }
        }

        protected void ValidateDirection(List<IEndpointOptions> endpoints, EndpointDirection direction, bool required)
        {
            if (endpoints.Where(e => e.Direction == direction).SingleOrDefault() == null)
            {
                if (required)
                {
                    Log.LogCritical("ValidateDirection: No {Direction} configured - It is requiored for this processor", direction.ToString());
                    throw new InvalidOperationException();
                }
                else
                {
                    Log.LogWarning("ValidateDirection: No {Direction} configured: This may lead to problems, but it was not indecated as required for this processor", direction.ToString());
                }
            }
        }
    }
}