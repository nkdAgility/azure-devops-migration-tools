using System;
using MigrationTools.Options;

namespace MigrationTools.Processors
{
    public class ProcessorOptionsJsonConvertor : OptionsJsonConvertor
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IProcessor).IsAssignableFrom(objectType);
        }
    }
}