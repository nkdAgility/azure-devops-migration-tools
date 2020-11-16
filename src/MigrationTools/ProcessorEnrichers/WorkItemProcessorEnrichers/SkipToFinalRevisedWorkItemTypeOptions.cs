using System;

namespace MigrationTools.Enrichers
{
    public class SkipToFinalRevisedWorkItemTypeOptions : ProcessorEnricherOptions
    {
        public override Type ToConfigure => typeof(SkipToFinalRevisedWorkItemType);

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}