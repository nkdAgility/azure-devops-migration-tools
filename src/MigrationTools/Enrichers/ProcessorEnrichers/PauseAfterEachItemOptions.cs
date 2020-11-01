namespace MigrationTools.Enrichers
{
    public class PauseAfterEachItemOptions : ProcessorEnricherOptions
    {
        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}