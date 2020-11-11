using System;

namespace MigrationTools.Endpoints
{
    public class TfsTeamSettingsEndpointOptions : TfsEndpointOptions, ITfsTeamSettingsEndpointOptions
    {
        public override Type ToConfigure => typeof(TfsTeamSettingsEndpoint);

        public override void SetDefaults()
        {
            Direction = EndpointDirection.NotSet;
        }
    }

    public interface ITfsTeamSettingsEndpointOptions
    {
    }
}