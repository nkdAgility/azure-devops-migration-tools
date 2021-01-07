namespace MigrationTools.Endpoints
{
    public class AzureDevOpsEndpointOptions : EndpointOptions
    {
        public AuthenticationMode AuthenticationMode { get; set; }

        public string AccessToken { get; set; }

        public string Organisation { get; set; }

        public string Project { get; set; }

        public string ReflectedWorkItemIdField { get; set; }


        //public override void SetDefaults()
        //{
        //    base.SetDefaults();
        //    AccessToken = MigrationTools.Tests.TestingConstants.AccessToken;
        //    Organisation = "https://dev.azure.com/nkdagility-preview/";
        //    Project = "NeedToSetThis";
        //    ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId";
        //}
    }
}