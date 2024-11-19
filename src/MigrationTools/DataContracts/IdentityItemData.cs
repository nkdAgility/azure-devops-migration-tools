namespace MigrationTools.DataContracts
{
    public class IdentityItemData
    {
        public string FriendlyName { get; set; }
        public string AccountName { get; set; }
    }

    public class IdentityMapData
    {
        public IdentityItemData Source { get; set; }
        public IdentityItemData Target { get; set; }
    }
}
