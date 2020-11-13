using System;
using MigrationTools.Options;

namespace MigrationTools.EndpointEnrichers
{
    public class EndpointEnricherOptionsJsonConvertor : OptionsJsonConvertor
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IEndpointEnricherOptions).IsAssignableFrom(objectType);
        }
    }
}