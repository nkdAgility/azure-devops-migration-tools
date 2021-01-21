using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MigrationTools.Endpoints
{
    class EndpointFactory : IEndpointFactory
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;
        private readonly IOptionsMonitor<EndpointFactoryOptions> _optionsMonitor;

        public EndpointFactory(
            IServiceProvider services,
            ILogger<EndpointFactory> logger,
            IOptionsMonitor<EndpointFactoryOptions> optionsMonitor)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            _services = services;
            _optionsMonitor = optionsMonitor;

            _logger = logger;
        }

        public IEndpoint CreateEndpoint(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            _logger.LogInformation("Creating endpoint with name {EndpointName}", name);
            EndpointFactoryOptions options = _optionsMonitor.Get(name);
            if(options.EndpointFuncs.Count != 1)
            {
                throw new InvalidOperationException("There is no endpoint named that or duplicates of the same name");
            }
            return options.EndpointFuncs[0](_services);
        }
    }
}
