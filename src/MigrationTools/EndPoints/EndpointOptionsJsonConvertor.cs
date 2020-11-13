using System;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public class EndpointOptionsJsonConvertor : OptionsJsonConvertor
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IEndpointOptions).IsAssignableFrom(objectType);
        }
    }
}