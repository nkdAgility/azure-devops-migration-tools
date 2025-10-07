using Microsoft.Extensions.Options;

namespace MigrationTools.Tools
{
    public class TfsWorkItemTypeValidatorToolOptionsValidator : IValidateOptions<TfsWorkItemTypeValidatorToolOptions>
    {
        public ValidateOptionsResult Validate(string name, TfsWorkItemTypeValidatorToolOptions options)
        {
            int includedCount = options.IncludeWorkItemTypes?.Count ?? 0;
            int excludedCount = options.ExcludeWorkItemTypes?.Count ?? 0;

            if ((includedCount > 0) && (excludedCount > 0))
            {
                const string msg = $"'{nameof(options.IncludeWorkItemTypes)}' and '{nameof(options.ExcludeWorkItemTypes)}'"
                    + $" cannot be set both at the same time."
                    + $" If '{nameof(options.IncludeWorkItemTypes)}' list is not empty,"
                    + $" '{nameof(options.ExcludeDefaultWorkItemTypes)}' must be set to 'false'.";
                return ValidateOptionsResult.Fail(msg);
            }
            return ValidateOptionsResult.Success;
        }
    }
}
