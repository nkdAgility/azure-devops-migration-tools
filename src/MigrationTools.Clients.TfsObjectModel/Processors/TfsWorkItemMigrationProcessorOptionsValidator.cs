using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Serilog;

namespace MigrationTools.Processors
{
    public class TfsWorkItemMigrationProcessorOptionsValidator : IValidateOptions<TfsWorkItemMigrationProcessorOptions>
    {
        public ValidateOptionsResult Validate(string name, TfsWorkItemMigrationProcessorOptions options)
        {
            
            var errors = new List<string>();
            ValidateOptionsResult baseResult = options.Validate(name, options);
            if (baseResult != ValidateOptionsResult.Success)
            {
                errors.AddRange(baseResult.Failures);
            }
            // Check if WIQLQuery is provided
            if (string.IsNullOrWhiteSpace(options.WIQLQuery))
            {
                errors.Add($"The WIQLQuery on {name} must be provided.");
            }
            else
            {
                // Validate the presence of required elements in the WIQL query
                if (!ContainsTeamProjectCondition(options.WIQLQuery))
                {
                    errors.Add("The WIQLQuery on {name} must contain the condition '[System.TeamProject] = @TeamProject'.");
                }

                if (!UsesWorkItemsTable(options.WIQLQuery))
                {
                    errors.Add("The WIQLQuery on {name} must use 'WorkItems' after the 'FROM' clause and not 'WorkItemLinks'.");
                }
            }
            if (errors.Count > 0)
            {
                Log.Debug("TfsWorkItemMigrationProcessorOptionsValidator::Validate::Fail");
                return ValidateOptionsResult.Fail(errors);
            }
            Log.Debug("TfsWorkItemMigrationProcessorOptionsValidator::Validate::Success");
            return ValidateOptionsResult.Success;
        }

        private bool ContainsTeamProjectCondition(string query)
        {
            // Regex to match '[System.TeamProject] = @TeamProject'
            string teamProjectPattern = @"\[System\.TeamProject\]\s*=\s*@TeamProject";

            return Regex.IsMatch(query, teamProjectPattern, RegexOptions.IgnoreCase);
        }

        // Check if the WIQL query is using 'WorkItems' and not 'WorkItemLinks'
        private bool UsesWorkItemsTable(string query)
        {
            // Regex to ensure that 'FROM WorkItems' exists and 'WorkItemLinks' does not follow the FROM clause
            string fromWorkItemsPattern = @"FROM\s+WorkItems\b";
            string fromWorkItemLinksPattern = @"FROM\s+WorkItemLinks\b";

            // Return true if "WorkItems" is used and "WorkItemLinks" is not
            return Regex.IsMatch(query, fromWorkItemsPattern, RegexOptions.IgnoreCase) &&
                   !Regex.IsMatch(query, fromWorkItemLinksPattern, RegexOptions.IgnoreCase);
        }
    }
}
