using MigrationTools.Enrichers;

namespace MigrationTools.EndpointEnrichers
{
    //[JsonConverter(typeof(OptionsJsonConvertor<IEndpointEnricherOptions>))]
    public interface IEndpointEnricherOptions : IEnricherOptions
    {
    }
}