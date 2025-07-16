using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace MigrationTools.Processors
{
    public class TfsWorkItemTypesValidationProcessorOptionsValidator
        : IValidateOptions<TfsWorkItemTypesValidationProcessorOptions>
    {
        public ValidateOptionsResult Validate(string name, TfsWorkItemTypesValidationProcessorOptions options)
        {
            if (HasItems(options.IncludeWorkItemtypes) && HasItems(options.ExcludeWorkItemtypes))
            {
                const string message = "Both 'IncludeWorkItemtypes' and 'ExcludeWorkItemtypes' cannot be set at the same time"
                    + $" in options for '{nameof(TfsWorkItemTypesValidationProcessor)}'.";
                return ValidateOptionsResult.Fail(message);
            }
            return ValidateOptionsResult.Success;
        }

        private bool HasItems(List<string> list) => (list is not null) && (list.Count > 0);
    }
}
