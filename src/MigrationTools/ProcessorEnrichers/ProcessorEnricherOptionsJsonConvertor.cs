using System;
using MigrationTools.Options;

namespace MigrationTools.Enrichers
{
    public class ProcessorEnricherOptionsJsonConvertor : OptionsJsonConvertor
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnricherOptions).IsAssignableFrom(objectType);
        }
    }
}