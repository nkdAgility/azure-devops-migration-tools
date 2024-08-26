using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using Serilog;

namespace MigrationTools
{
    public static class EndpointRegistrationExtensions
    {

        public static void AddConfiguredEndpoints(this IServiceCollection services, IConfiguration configuration)
        {
            var endpointsSection = configuration.GetSection("MigrationTools:Endpoints");

            foreach (var endpointConfig in endpointsSection.GetChildren())
            {
                var endpointName = endpointConfig.Key;
                var endpointType = endpointConfig.GetValue<string>("EndpointType");

                var endpointOptionsType = GetEndpointOptionsType(endpointType);
                var endpointImplementationType = GetEndpointImplementationType(endpointType);

                if (endpointOptionsType != null && endpointImplementationType != null)
                {
                    services.AddKeyedSingleton(typeof(IEndpoint), endpointName, (sp, key) =>
                    {
                        // Create the options instance and bind the configuration
                        var endpointOptionsInstance = Activator.CreateInstance(endpointOptionsType);
                        endpointConfig.Bind(endpointOptionsInstance); // TODO: Find out why this does not load Environment Variables?
                        IEndpoint endpointInstance;
                        try
                        {
                            // Create the IEndpoint instance, passing the options instance to the constructor

                            var optionsWrapper = typeof(Microsoft.Extensions.Options.Options).GetMethod("Create")
                            .MakeGenericMethod(endpointOptionsInstance.GetType())
                            .Invoke(null, new object[] { endpointOptionsInstance });

                            var constructor = endpointImplementationType.GetConstructors().First();
                            var parameters = constructor.GetParameters()
                                 .Select(p => p.ParameterType.IsAssignableFrom(optionsWrapper.GetType()) ? optionsWrapper : sp.GetRequiredService(p.ParameterType))
                                 .ToArray();

                            endpointInstance = Activator.CreateInstance(endpointImplementationType, parameters) as IEndpoint;
                            if (endpointInstance == null)
                            {
                                throw new InvalidOperationException($"Failed to create an instance of '{endpointImplementationType.Name}'.");
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"Failed to create an instance of '{endpointImplementationType.Name}'.", ex);
                        }

                        return endpointInstance;
                    });
                }
                else
                {
                    // Cant log... and cant throw exception. This method should be called for each of the clients to enable the endpoints. TODO have some check after all the configruation to validate that each of the enpoints were imported.
                    //Log.Fatal("Failed to create '{endpointName}' endpoint. Type not found for either main object or options: {EndpointType}", endpointName, endpointType);
                }
            }
        }

        private static Type GetEndpointOptionsType(string endpointType)
        {
            return AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IEndpointOptions>().FirstOrDefault(t => t.Name.Equals($"{endpointType}Options", StringComparison.OrdinalIgnoreCase));
        }

        private static Type GetEndpointImplementationType(string endpointType)
        {
            return AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IEndpoint>().FirstOrDefault(t => t.Name.Equals(endpointType, StringComparison.OrdinalIgnoreCase));
        }

    }
}
