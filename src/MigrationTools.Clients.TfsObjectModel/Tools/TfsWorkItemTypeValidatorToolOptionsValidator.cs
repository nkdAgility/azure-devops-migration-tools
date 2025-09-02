using Microsoft.Extensions.Options;

namespace MigrationTools.Tools
{
    public class TfsWorkItemTypeValidatorToolOptionsValidator : IValidateOptions<TfsWorkItemTypeValidatorToolOptions>
    {
        public ValidateOptionsResult Validate(string name, TfsWorkItemTypeValidatorToolOptions options)
        {
            int includedCount = options.IncludeWorkItemtypes?.Count ?? 0;
            int excludedCount = options.ExcludeWorkItemtypes?.Count ?? 0;

            if ((includedCount > 0) && (excludedCount > 0))
            {
                const string msg = $"'{nameof(options.IncludeWorkItemtypes)}' and '{nameof(options.ExcludeWorkItemtypes)}'"
                    + $" cannot be set both at the same time.";
                return ValidateOptionsResult.Fail(msg);
            }
            return ValidateOptionsResult.Success;
        }
    }
}
