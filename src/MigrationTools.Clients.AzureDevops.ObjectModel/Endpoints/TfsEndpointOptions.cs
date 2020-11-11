using System;

namespace MigrationTools.Endpoints
{
    public class TfsEndpointOptions : EndpointOptions, ITfsEndpointOptions
    {
        public string AccessToken { get; set; }
        public string Organisation { get; set; }
        public string Project { get; set; }

        public override Type ToConfigure => typeof(TfsEndpoint);

        public override void SetDefaults()
        {
            base.SetDefaults();
            AccessToken = "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq";
            Organisation = "https://dev.azure.com/nkdagility-preview/";
            Project = "NeedToSetThis";
        }
    }

    public interface ITfsEndpointOptions
    {
        public string AccessToken { get; }
        public string Organisation { get; }
        public string Project { get; }
    }
}