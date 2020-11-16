using System;

namespace MigrationTools.Endpoints
{
    public class TfsTeamSettingsEndpointOptions : TfsEndpointOptions, ITfsTeamSettingsEndpointOptions
    {
        public override Type ToConfigure => typeof(TfsTeamSettingsEndpoint);
    }

    public interface ITfsTeamSettingsEndpointOptions
    {
    }
}