using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Serilog;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Configuration options for mapping language-specific field names between source and target TFS systems. Used to translate area path and iteration path field names when migrating between TFS instances with different language configurations.
    /// </summary>
    public class TfsLanguageMapOptions : IValidateOptions<TfsLanguageMapOptions>
    {
        /// <summary>
        /// Gets or sets the field name for the area path in the TFS system language (e.g., "Area" for English, "Zone" for French).
        /// </summary>
        /// <default>Area</default>
        public string AreaPath { get; set; }
        
        /// <summary>
        /// Gets or sets the field name for the iteration path in the TFS system language (e.g., "Iteration" for English, "Itération" for French).
        /// </summary>
        /// <default>Iteration</default>
        public string IterationPath { get; set; }

        public ValidateOptionsResult Validate(string name, TfsLanguageMapOptions options)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(options.AreaPath))
            {
                errors.Add("The AreaPath property must not be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(options.IterationPath))
            {
                errors.Add("The IterationPath property must not be null or empty.");
            }
            if (errors.Any())
            {
                Log.Debug("TfsLanguageMapOptions::Validate::Fail");
                ValidateOptionsResult.Fail(errors);
            }
            Log.Debug("TfsLanguageMapOptions::Validate::Success");
            return ValidateOptionsResult.Success;
        }
    }
}