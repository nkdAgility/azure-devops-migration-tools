using Microsoft.Extensions.Options;

namespace MigrationTools.Tools
{
    internal class ExportWorkItemMappingToolOptionsValidator : IValidateOptions<ExportWorkItemMappingToolOptions>
    {
        public ValidateOptionsResult Validate(string name, ExportWorkItemMappingToolOptions options)
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
