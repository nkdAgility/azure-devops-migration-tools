namespace MigrationTools.Enrichers
{
    public interface IEnricherOptions
    {
        /// <summary>
        /// Active the enricher if it true.
        /// </summary>
        bool Enabled { get; set; }
    }
}