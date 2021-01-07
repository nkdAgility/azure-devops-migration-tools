namespace MigrationTools.Endpoints
{
    public class TfsEndpointOptions : EndpointOptions
    {
        public AuthenticationMode AuthenticationMode { get; set; }

        public string AccessToken { get; set; }

        public string Organisation { get; set; }

        public string Project { get; set; }

        public string ReflectedWorkItemIdField { get; set; }

        public TfsLanguageMapOptions LanguageMaps { get; set; }

        //public override void SetDefaults()
        //{
        //    base.SetDefaults();
        //    AccessToken = "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq";
        //    Organisation = "https://dev.azure.com/nkdagility-preview/";
        //    Project = "NeedToSetThis";
        //    ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId";
        //    LanguageMaps = new TfsLanguageMapOptions();
        //    LanguageMaps.SetDefaults();
        //}
    }
}