using System;

namespace MigrationTools.Enrichers
{
    public class AppendMigrationToolSignatureFooterOptions : ProcessorEnricherOptions
    {
        public override Type ToConfigure => typeof(AppendMigrationToolSignatureFooter);

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}