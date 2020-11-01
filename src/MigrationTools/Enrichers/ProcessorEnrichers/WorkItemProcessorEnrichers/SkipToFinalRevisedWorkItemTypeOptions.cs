namespace MigrationTools.Enrichers
{
    public class SkipToFinalRevisedWorkItemTypeOptions : ProcessorEnricherOptions
    {
        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}