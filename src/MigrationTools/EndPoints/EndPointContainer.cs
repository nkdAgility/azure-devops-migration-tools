using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Endpoints
{
    public class EndpointContainer : List<IEndpoint>
    {
        private bool _Configured;

        public EndpointContainer(IServiceProvider services, ITelemetryLogger telemetry, ILogger<EndpointContainer> logger)
        {
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        public IEndpoint Source { get { return GetDirection(EndpointDirection.Source); } }
        public IEndpoint Target { get { return GetDirection(EndpointDirection.Target); } }

        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<EndpointContainer> Log { get; }

        public void ConfigureEndpoint(IEndpointOptions endpointOptions)
        {
            var ep = (IEndpoint)Services.GetRequiredService(endpointOptions.ToConfigure);
            ep.Configure(endpointOptions);
            Add(ep);
            Log.LogInformation("ConfigureEndpoint: {EndPointName} {Direction} : Enrichers{EnrichersCount}: {EnrichersList} ", ep.GetType().Name, endpointOptions.Direction, endpointOptions.EndpointEnrichers?.Count, endpointOptions.EndpointEnrichers?.Select(x => x.GetType().Name));
        }

        public void ConfigureEndpoints(List<IEndpointOptions> endpoints, bool sourceRequired = true, bool targetRequired = true)
        {
            Log.LogDebug("EndpointContainer::ConfigureEndpoints");
            if (_Configured)
            {
                Log.LogError("EndpointContainer::ConfigureEndpoints: You cant configure Endpoints twice");
                throw new Exception("You cant configure Endpoints twice");
            }
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
            _Configured = true;
        }

        public IEndpoint GetDirection(EndpointDirection direction)
        {
            return this.Where(e => e.Direction == direction).SingleOrDefault();
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