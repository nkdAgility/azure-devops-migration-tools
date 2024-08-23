using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.Options;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Endpoints.Infrastructure
{
    public  class EndpointContainer
    {
        private IServiceProvider _services;
        private ILogger<EndpointContainer> _logger;
        private EndpointContainerOptions _Options;

        private readonly Lazy<List<IEndpoint>> _sourcesLazy;
        private readonly Lazy<List<IEndpoint>> _TargetsLazy;

        protected ReadOnlyCollection<IEndpoint> Sources
        {
            get
            {
                return new ReadOnlyCollection<IEndpoint>(_sourcesLazy.Value);
            }
        }

        public TEndpoint GetSource<TEndpoint>() where TEndpoint : IEndpoint
        {
            return Sources.OfType<TEndpoint>().FirstOrDefault();
        }

        public IEndpoint GetSource(string endpointName) 
        {
            return GetTypeByFriendlyName(Sources, endpointName);
        }

        protected ReadOnlyCollection<IEndpoint> Targets
        {
            get
            {
                return new ReadOnlyCollection<IEndpoint>(_TargetsLazy.Value);
            }
        }

        public TEndpoint GetTarget<TEndpoint>() where TEndpoint : IEndpoint
        {
            return Targets.OfType<TEndpoint>().FirstOrDefault();
        }

        public IEndpoint GetTarget(string endpointName)
        {
            return GetTypeByFriendlyName(Targets,endpointName);
        }

        public static IEndpoint GetTypeByFriendlyName(IEnumerable<IEndpoint> endpoints, string friendlyName)
        {
            return endpoints.FirstOrDefault(t => t.GetType().Name.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));
        }


        public EndpointContainer(IOptions<EndpointContainerOptions> options)
        {
            _Options = options.Value;
            // Initialize the lazy processor list
            _sourcesLazy = new Lazy<List<IEndpoint>>(() => LoadEndpointsfromOptions(_Options.Source));
            // Initialize the lazy processor list
            _TargetsLazy = new Lazy<List<IEndpoint>>(() => LoadEndpointsfromOptions(_Options.Target));
        }

        private List<IEndpoint> LoadEndpointsfromOptions(List<IEndpointOptions> eOptions)
        {
            var endpoints = new List<IEndpoint>();
            if (eOptions != null)
            {
                _logger.LogInformation("EndpointContainer: Loading Endpoints {EndPointCount} ", eOptions.Count);
                var allTypes = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IEndpoint>().ToList();

                foreach (IEndpointOptions eOption in eOptions)
                {
                   
                        _logger.LogInformation("EndpointContainer: Adding Enpoint {ProcessorName}", eOption.ConfigurationOptionFor);
                        Type type = allTypes
                              .FirstOrDefault(t => t.Name.Equals(eOption.ConfigurationOptionFor));

                        if (type == null)
                        {
                            _logger.LogError("Type " + eOption.ConfigurationOptionFor + " not found.", eOption.ConfigurationOptionFor);
                            throw new Exception("Type " + eOption.ConfigurationOptionFor + " not found.");
                        }
                    IEndpoint pc = (IEndpoint)ActivatorUtilities.CreateInstance(_services, type, eOption);
                    endpoints.Add(pc);

                }
            }
            return endpoints;
        }
    }
}
