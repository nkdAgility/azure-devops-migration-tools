using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MigrationTools.Options;

namespace MigrationTools.Endpoints.Infrastructure
{
    public class EndpointContainerOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:Endpoints";

        public List<IEndpointOptions> Target { get; set; } = new List<IEndpointOptions>();
        public List<IEndpointOptions> Source { get; set; } = new List<IEndpointOptions>();


        public class ConfigureOptions : IConfigureOptions<EndpointContainerOptions>
        {
            private readonly IConfiguration _configuration;

            public ConfigureOptions(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void Configure(EndpointContainerOptions options)
            {
                switch (VersionOptions.ConfigureOptions.GetMigrationConfigVersion(_configuration).schema)
                {
                    case MigrationConfigSchema.v160:
                        _configuration.GetSection(ConfigurationSectionName).Bind(options);
                        options.Source = GetEndpoints(_configuration, "Source");
                        options.Target = GetEndpoints(_configuration, "Target");
                        break;
                    case MigrationConfigSchema.v1:
                        throw new Exception("Not implemented");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                        break;
                }
            }

            private List<IEndpointOptions> GetEndpoints(IConfiguration configuration, string group)
            {
                var endpoints = new List<IEndpointOptions>();
                var section = configuration.GetSection($"{ConfigurationSectionName}:{group}");

                foreach (var endpointConfig in section.GetChildren())
                {
                    var endpointType = endpointConfig.Key; // Use the parent node name as the EndpointType
                    var endpointInstance = CreateEndpointOptionsInstance(endpointType);

                    if (endpointInstance != null)
                    {
                        endpointConfig.Bind(endpointInstance);
                        endpoints.Add(endpointInstance);
                    }
                }

                return endpoints;
            }

            private IEndpointOptions CreateEndpointOptionsInstance(string endpointType)
            {
                // Get the assembly containing the endpoint option classes
               var enpointTypes = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IEndpointOptions>();

                // Find the type that matches the endpointType
                var endpointOptionsType = enpointTypes.Where(t => t.Name.StartsWith(endpointType)).FirstOrDefault();

                if (endpointOptionsType != null)
                {
                    return Activator.CreateInstance(endpointOptionsType) as IEndpointOptions;
                }

                throw new InvalidOperationException($"Endpoint type '{endpointType}' is not supported.");
            }

        }
    }
}
