using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using Serilog;

namespace MigrationTools.Processors.Infrastructure
{
    public class ProcessorContainerOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:Processors";

        public List<IProcessorConfig> Processors { get; set; } = new List<IProcessorConfig>();

        public class ConfigureOptions : IConfigureOptions<ProcessorContainerOptions>
        {
            private readonly IConfiguration _configuration;
            private readonly IServiceProvider _serviceProvider;

            public ConfigureOptions(IConfiguration configuration, IServiceProvider serviceProvider)
            {
                _configuration = configuration;
                _serviceProvider = serviceProvider;
            }

            public void Configure(ProcessorContainerOptions options)
            {
                BindProcessorOptions(options, ConfigurationSectionName, "ProcessorType");
            }

            private void BindProcessorOptions(ProcessorContainerOptions options, string sectionName, string objectTypePropertyName)
            {
                _configuration.GetSection(sectionName).Bind(options);
                var validProcessors = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IProcessorOptions>();

                foreach (var processorSection in _configuration.GetSection(sectionName).GetChildren())
                {
                    var processorTypeString = processorSection.GetValue<string>(objectTypePropertyName);
                    if (processorTypeString == null)
                    {
                        Log.Fatal("Your processor at `{path}` in the config does not have a property called {objectTypePropertyName} that is required to sucessfully detect the type and load it.", processorSection.Path, objectTypePropertyName);
                        throw new InvalidProcessorException($"`{objectTypePropertyName}` missing");
                    }

                    var processorType = validProcessors.WithNameString(processorTypeString);
                    if (processorType == null)
                    {
                        Log.Fatal("The value of {objectTypePropertyName} for your processor at `{path}` may have an error as we were unable to find a type that matches {processorTypeString}! Please check the spelling, and that it's a processor listed in the documentation.", objectTypePropertyName, processorSection.Path, processorTypeString);
                        Log.Information("Valid options are @{validProcessors}", validProcessors.Select(type => type.Name).ToList());
                        throw new InvalidProcessorException($"`{processorTypeString}` is not valid");
                    }

                    IProcessorOptions processorOption = Activator.CreateInstance(processorType) as IProcessorOptions;
                    // get defaults and bind
                    _configuration.GetSection(processorOption.ConfigurationMetadata.PathToDefault).Bind(processorOption);
                    // Bind collection item
                    processorSection.Bind(processorOption);

                    // Bind enrichers for each processor
                    var enrichersSection = processorSection.GetSection("Enrichers");
                    var enrichers = enrichersSection?.ToMigrationToolsList(child => child.GetMigrationToolsOption<IProcessorEnricherOptions>("EnricherType"));
                    if (processorOption.Enrichers == null)
                    {
                        processorOption.Enrichers = new List<IProcessorEnricherOptions>();
                    }
                    processorOption.Enrichers.AddRange(enrichers);


                    // Validate the processor options
                    ValidateProcessorOptions(processorType, processorOption);
                    // Add the processor to the list of processors
                    options.Processors.Add(processorOption);
                }
            }

            private void ValidateProcessorOptions(Type processorType, IProcessorOptions processorOptions)
            {
                // Find a validator for the processor options type
                var validatorType = typeof(IValidateOptions<>).MakeGenericType(processorType);

                var validator = _serviceProvider.GetService(validatorType);

                if (validator != null)
                {
                    var validateMethod = validatorType.GetMethod("Validate");
                    if (validateMethod == null)
                    {
                        Log.Fatal("No Validate method found on validator type {TypeFound}", validatorType.FullName);
                        return;
                    }
                    // Assuming you have the processorOptions instance available
                    var result = validateMethod.Invoke(validator, new object[] { processorType.Name, processorOptions });
                    // Check if validation failed
                    var failedProperty = result.GetType().GetProperty("Failed");
                    if ((bool)failedProperty.GetValue(result))
                    {
                        var failuresProperty = result.GetType().GetProperty("Failures");
                        var failures = (IEnumerable<string>)failuresProperty.GetValue(result);
                        throw new OptionsValidationException(processorType.Name, processorType, failures.ToArray());
                    }
                }
                else
                {
                    Log.Information("No validator found for processor type {ProcessorType}", processorType.Name);
                }
            }

        }
    }
}
