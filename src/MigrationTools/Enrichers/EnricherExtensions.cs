using Microsoft.Extensions.DependencyInjection;

namespace MigrationTools.Enrichers
{
    public static class EnricherExtensions
    {
        public static void AddMigrationToolEndpointEnrichers(this IServiceCollection context)
        {
            context.AddTransient<AppendMigrationToolSignatureFooter>();
            context.AddTransient<FilterWorkItemsThatAlreadyExistInTarget>();
            context.AddTransient<SkipToFinalRevisedWorkItemType>();
            //Following are Abstract
            context.AddTransient<WorkItemAttachmentEnricher>();
            context.AddTransient<WorkItemCreatedEnricher>();
            context.AddTransient<WorkItemEmbedEnricher>();
            context.AddTransient<WorkItemFieldTableEnricher>();
            context.AddTransient<WorkItemLinkEnricher>();
        }

        public static void AddMigrationToolProcessorEnrichers(this IServiceCollection context)
        {
            context.AddTransient<WorkItemAttachmentEnricher>();
            context.AddTransient<PauseAfterEachItem>();
        }
    }
}