using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Options;
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
                if (string.IsNullOrEmpty(endpointType))
                {
                    Log.Warning("Endpoint '{EndpointName}' does not have a type configured. Skipping.", endpointName);
                    continue;
                }
                AddEndPointSingleton(services, configuration, endpointConfig, endpointName, endpointType);
            }
        }

        private static void AddEndPointSingleton(IServiceCollection services, IConfiguration configuration, IConfigurationSection endpointConfig, string endpointName, string endpointType)
        {
            var endpointOptionsType = GetEndpointOptionsType(endpointType);
            var endpointImplementationType = GetEndpointImplementationType(endpointType);

            if (endpointOptionsType != null && endpointImplementationType != null)
            {
                services.AddKeyedSingleton(typeof(IEndpoint), endpointName, (sp, key) =>
                {
                    // Create the options instance and bind the configuration
                    IEndpointOptions endpointOptionsInstance = (IEndpointOptions)Activator.CreateInstance(endpointOptionsType);
                    endpointOptionsInstance.Name = endpointName;
                    // Get and bind the defaults
                    var endpointsDefaultsSection = configuration.GetSection(endpointOptionsInstance.ConfigurationMetadata.PathToDefault);
                    endpointsDefaultsSection.Bind(endpointOptionsInstance);

                    // Bind the configuration to the options instance
                    endpointConfig.Bind(endpointOptionsInstance);

                    // Dynamically find and invoke the validator for the options type
                    var validatorType = GetValidatorTypeForOptions(endpointOptionsType);
                    if (validatorType != null)
                    {
                        var validator = Activator.CreateInstance(validatorType);
                        var validationResult = InvokeValidator(validator, endpointOptionsInstance);
                        if (!validationResult.IsValid)
                        {
                            throw new InvalidOperationException($"Validation failed for endpoint '{endpointName}': {validationResult.Message}");
                        }
                    }

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
                throw new InvalidOperationException($"Failed to create '{endpointName}' endpoint. Type not found for either main object or options: {endpointType}");
            }
        }

        private static Type GetValidatorTypeForOptions(Type optionsType)
        {
            // Loop through the inheritance chain to find the closest matching validator
            Type currentType = optionsType;

            while (currentType != null && currentType != typeof(object))
            {
                var validatorInterface = typeof(IValidateOptions<>).MakeGenericType(currentType);
                var validatorType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => validatorInterface.IsAssignableFrom(t));

                if (validatorType != null)
                {
                    return validatorType; // Return the validator for the most specific class
                }

                // Move up the inheritance chain
                currentType = currentType.BaseType;
            }

            return null; // No matching validator found
        }


        private static (bool IsValid, string Message) InvokeValidator(object validator, IEndpointOptions optionsInstance)
        {
            var validateMethod = validator.GetType().GetMethod("Validate");
            var validationResult = validateMethod?.Invoke(validator, new object[] { null, optionsInstance });

            if (validationResult is Microsoft.Extensions.Options.ValidateOptionsResult result)
            {
                if (result.Failed)
                {
                    return (false, result.FailureMessage);
                }
            }

            return (true, string.Empty);
        }


        private static Type GetEndpointOptionsType(string endpointType)
        {
            return AppDomain.CurrentDomain.GetMigrationToolsTypes()
                .WithInterface<IEndpointOptions>()
                .FirstOrDefault(t => t.Name.Equals($"{endpointType}Options", StringComparison.OrdinalIgnoreCase));
        }

        private static Type GetEndpointImplementationType(string endpointType)
        {
            return AppDomain.CurrentDomain.GetMigrationToolsTypes()
                .WithInterface<IEndpoint>()
                .FirstOrDefault(t => t.Name.Equals(endpointType, StringComparison.OrdinalIgnoreCase));
        }

    }
}
