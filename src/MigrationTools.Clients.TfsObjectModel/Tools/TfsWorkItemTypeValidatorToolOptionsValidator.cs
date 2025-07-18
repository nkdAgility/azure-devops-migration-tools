using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace MigrationTools.Tools
{
    internal class TfsWorkItemTypeValidatorToolOptionsValidator
           : IValidateOptions<TfsWorkItemTypeValidatorToolOptions>
    {
        public ValidateOptionsResult Validate(string name, TfsWorkItemTypeValidatorToolOptions options)
        {
            if (HasItems(options.IncludeWorkItemtypes) && HasItems(options.ExcludeWorkItemtypes))
            {
                const string message =
                    $"Both '{nameof(options.IncludeWorkItemtypes)}' and '{nameof(options.ExcludeWorkItemtypes)}'"
                    + $" cannot be set at the same time in options for '{nameof(TfsWorkItemTypeValidatorTool)}'.";
                return ValidateOptionsResult.Fail(message);
            }
            return ValidateOptionsResult.Success;
        }

        private bool HasItems(List<string> list) => (list is not null) && (list.Count > 0);
    }
}
