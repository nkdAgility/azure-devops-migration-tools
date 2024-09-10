﻿using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors.Infrastructure;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace MigrationTools.Processors
{

    public class TfsWorkItemMigrationProcessorOptions : ProcessorOptions, IWorkItemProcessorConfig
    {


        /// <summary>
        /// If this is enabled the creation process on the target project will create the items with the original creation date.
        /// (Important: The item history is always pointed to the date of the migration, it's change only the data column CreateDate,
        /// not the internal create date)
        /// </summary>
        /// <default>true</default>
        public bool UpdateCreatedDate { get; set; }

        /// <summary>
        /// If this is enabled the creation process on the target project will create the items with the original creation date.
        /// (Important: The item history is always pointed to the date of the migration, it's change only the data column CreateDate,
        /// not the internal create date)
        /// </summary>
        /// <default>true</default>
        public bool UpdateCreatedBy { get; set; }


        /// <summary>
        /// A work item query based on WIQL to select only important work items. To migrate all leave this empty. See [WIQL Query Bits](#wiql-query-bits)
        /// </summary>
        /// <default>SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc</default>
        [Required]
        public string WIQLQuery { get; set; }

        /// <summary>
        /// **beta** If enabled this will fix any image attachments URL's, work item mention URL's or user mentions in the HTML
        /// fields as well as discussion comments. You must specify a PersonalAccessToken in the Source project for Azure DevOps;
        /// TFS should use integrated authentication.
        /// </summary>
        /// <default>?</default>
        public bool FixHtmlAttachmentLinks { get; set; }

        /// <summary>
        /// **beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count.
        /// This allows for periodic network glitches not to end the process.
        /// </summary>
        /// <default>5</default>
        public int WorkItemCreateRetryLimit { get; set; }

        /// <summary>
        /// This loads all of the work items already saved to the Target and removes them from the Source work item list prior to commencing the run.
        /// While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart.
        /// </summary>
        /// <default>true</default>
        public bool FilterWorkItemsThatAlreadyExistInTarget { get; set; }

        /// <summary>
        /// Pause after each work item is migrated
        /// </summary>
        /// <default>false</default>
        public bool PauseAfterEachWorkItem { get; set; }

        /// <summary>
        /// This will create a json file with the revision history and attach it to the work item. Best used with `MaxRevisions` or `ReplayRevisions`.
        /// </summary>
        /// <default>?</default>
        public bool AttachRevisionHistory { get; set; }

        /// <summary>
        /// If enabled, adds a comment recording the migration
        /// </summary>
        /// <default>false</default>
        public bool GenerateMigrationComment { get; set; }

        /// <summary>
        /// A list of work items to import
        /// </summary>
        /// <default>[]</default>
        public IList<int> WorkItemIDs { get; set; }

        /// <summary>
        /// The maximum number of failures to tolerate before the migration fails. When set above zero, a work item migration error is logged but the migration will
        /// continue until the number of failed items reaches the configured value, after which the migration fails.
        /// </summary>
        /// <default>0</default>
        public int MaxGracefulFailures { get; set; }

        /// <summary>
        /// This will skip a revision if the source iteration has not been migrated i.e. it was deleted
        /// </summary>
        public bool SkipRevisionWithInvalidIterationPath { get; set; }

        /// <summary>
        /// When set to true, this setting will skip a revision if the source area has not been migrated, has been deleted or is somehow invalid, etc.
        /// </summary>
        public bool SkipRevisionWithInvalidAreaPath { get; set; }

    }

    public class TfsWorkItemMigrationProcessorOptionsValidator : IValidateOptions<TfsWorkItemMigrationProcessorOptions>
    {

        public TfsWorkItemMigrationProcessorOptionsValidator()
        {
            
        }

        public ValidateOptionsResult Validate(string name, TfsWorkItemMigrationProcessorOptions options)
        {
            ValidateOptionsResult result = new ValidateOptionsResult();
            // Check if WIQLQuery is provided
            if (string.IsNullOrWhiteSpace(options.WIQLQuery))
            {
                return ValidateOptionsResult.Fail("The WIQLQuery must be provided.");
            }

            // Validate the presence of required elements in the WIQL query
            if (!ContainsTeamProjectCondition(options.WIQLQuery))
            {
                return ValidateOptionsResult.Fail("The WIQLQuery must contain the condition '[System.TeamProject] = @TeamProject'.");
            }

            if (!UsesWorkItemsTable(options.WIQLQuery))
            {
                return ValidateOptionsResult.Fail("The WIQLQuery must use 'WorkItems' after the 'FROM' clause and not 'WorkItemLinks'.");
            }

            return ValidateOptionsResult.Success;
        }

        // Check if the WIQL query contains the '[System.TeamProject] = @TeamProject' condition
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