using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace MigrationTools.Endpoints
{
    public class TfsLanguageMapOptions : IValidateOptions<TfsLanguageMapOptions>
    {
        public string AreaPath { get; set; }
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
                ValidateOptionsResult.Fail(errors);
            }
            return ValidateOptionsResult.Success;
        }
    }
}