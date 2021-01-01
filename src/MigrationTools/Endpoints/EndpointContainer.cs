using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Endpoints
{
    public class EndpointContainer
    {
        private bool _Configured;
        private IEndpointOptions _SourceOptions;
        private IEndpoint _Source;
        private IEndpointOptions _TargetOptions;
        private IEndpoint _Target;

        public EndpointContainer(IServiceProvider services, ITelemetryLogger telemetry, ILogger<EndpointContainer> logger)
        {
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        public IEndpoint Source { get { return _Source; } }
        public IEndpoint Target { get { return _Target; } }

        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<EndpointContainer> Log { get; }

        public void ConfigureEndpoints(IEndpointOptions source, IEndpointOptions target)
        {
            Log.LogDebug("EndpointContainer::ConfigureEndpoints");
            if (_Configured)
            {
                Log.LogError("EndpointContainer::ConfigureEndpoints: You cant configure Endpoints twice");
                throw new Exception("You cant configure Endpoints twice");
            }

            _SourceOptions = source ?? throw new ArgumentNullException(nameof(source));
            _TargetOptions = target ?? throw new ArgumentNullException(nameof(target));

            if (source is RefEndpointOptions)
            {
                // Must load from catalog
                throw new NotImplementedException("RefEndpointOptions has not yet been implemnted");
            }
            else
            {
                var sourceEp = (IEndpoint)Services.GetRequiredService(_SourceOptions.ToConfigure);
                sourceEp.Configure(_SourceOptions);
                _Source = sourceEp;
            }

            if (target is RefEndpointOptions)
            {
                // Must load from catalog
                throw new NotImplementedException("RefEndpointOptions has not yet been implemnted");
            }
            else
            {
                var targetEp = (IEndpoint)Services.GetRequiredService(_TargetOptions.ToConfigure);
                targetEp.Configure(_TargetOptions);
                _Target = targetEp;
            }
            _Configured = true;
        }
    }
}