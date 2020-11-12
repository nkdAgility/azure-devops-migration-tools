using MigrationTools.Options;

namespace MigrationTools.Enrichers
{
    public interface IEnricherOptions : IOptions
    {
        /// <summary>
        /// Active the enricher if it true.
        /// </summary>
        bool Enabled { get; set; }
    }
}