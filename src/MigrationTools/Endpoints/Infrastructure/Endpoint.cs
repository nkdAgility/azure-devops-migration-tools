using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public abstract class Endpoint<TOptions> : ISourceEndPoint, ITargetEndPoint
        where TOptions : class, IEndpointOptions
    {
        private List<IEndpointEnricher> _EndpointEnrichers;

        public Endpoint(
            IOptions<TOptions> options,
            EndpointEnricherContainer endpointEnrichers,
            IServiceProvider serviceProvider,
            ITelemetryLogger telemetry,
            ILogger<Endpoint<TOptions>> logger)
        {
            Options = options.Value;    
            EndpointEnrichers = endpointEnrichers;
            Telemetry = telemetry;
            Services = serviceProvider;
            Log = logger;
            _EndpointEnrichers = new List<IEndpointEnricher>();
        }

        public EndpointEnricherContainer EndpointEnrichers { get; }

        public IEnumerable<IEndpointSourceEnricher> SourceEnrichers => _EndpointEnrichers.Where(e => e.GetType().IsAssignableFrom(typeof(IEndpointSourceEnricher))).Select(e => (IEndpointSourceEnricher)e);
        public IEnumerable<IEndpointTargetEnricher> TargetEnrichers => _EndpointEnrichers.Where(e => e.GetType().IsAssignableFrom(typeof(IEndpointTargetEnricher))).Select(e => (IEndpointTargetEnricher)e);

        protected IServiceProvider Services { get; }

        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<Endpoint<TOptions>> Log { get; }

        public TOptions Options { get; private set; }

        [Obsolete("Dont know what this is for")]
        public abstract int Count { get; }

        //public virtual void Configure(TOptions options)
        //{
        //    Log.LogDebug("Endpoint::Configure");
        //    Options = options;
        //    EndpointEnrichers.ConfigureEnrichers(Options.EndpointEnrichers); TODO.. do we lazy load this?
        //}

    }
}