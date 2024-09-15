using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Options;

namespace MigrationTools.Exceptions
{
    public class ConfigurationValidationException : Exception
    {
        public string ConfigrationSectionPath { get; }
        public IOptions OptionsInstance { get; }
        public ValidateOptionsResult ValidationResult { get; }

        public ConfigurationValidationException(IConfigurationSection configrationSection, IOptions optionsInstance, ValidateOptionsResult validationResult)
        {
            ConfigrationSectionPath = configrationSection.Path;
            OptionsInstance = optionsInstance;
            ValidationResult = validationResult;
        }

        public ConfigurationValidationException(IOptions optionsInstance, ValidateOptionsResult validationResult)
        {
            ConfigrationSectionPath = optionsInstance.ConfigurationMetadata.PathToInstance;
            OptionsInstance = optionsInstance;
            ValidationResult = validationResult;
        }

        public override string ToString()
        {
            return $"The configuration entry at '{ConfigrationSectionPath}' did not pass validation!\n Please check the following failures:\n -{string.Join("\n-", ValidationResult.Failures)}";
        }

    }
}
