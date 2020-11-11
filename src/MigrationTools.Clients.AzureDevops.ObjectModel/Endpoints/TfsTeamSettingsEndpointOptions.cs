using System;

namespace MigrationTools.Endpoints
{
    public class TfsTeamSettingsEndpointOptions : TfsEndPointOptions
    {
        public override Type ToConfigure => typeof(TfsTeamSettingsEndpoint);

        public override void SetDefaults()
        {
            Direction = EndpointDirection.NotSet;
        }
    }
}