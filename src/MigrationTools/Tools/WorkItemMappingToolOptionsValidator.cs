using Microsoft.Extensions.Options;

namespace MigrationTools.Tools
{
    internal class WorkItemMappingToolOptionsValidator : IValidateOptions<WorkItemMappingToolOptions>
    {
        public ValidateOptionsResult Validate(string name, WorkItemMappingToolOptions options)
        {
            if (options.Enabled && string.IsNullOrWhiteSpace(options.TargetFile))
            {
                const string msg = $"'{nameof(options.TargetFile)}' is not set, so work item mappings cannot be saved.";
                return ValidateOptionsResult.Fail(msg);
            }
            return ValidateOptionsResult.Success;
        }
    }
}
