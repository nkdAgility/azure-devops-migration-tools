namespace MigrationTools.Enrichers
{
    public abstract class EndpointEnricherOptions : IEndpointEnricherOptions
    {
        public bool Enabled { get; set; }

        public abstract void SetDefaults();
    }
}