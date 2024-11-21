namespace MigrationTools.DataContracts
{
    public class IdentityItemData
    {
        public string Sid { get; set; }
        public string DisplayName { get; set; }
        public string Domain { get; set; }
        public string AccountName { get; set; }
        public string MailAddress { get; set; }
    }

    public class IdentityMapData
    {
        public IdentityItemData Source { get; set; }
        public IdentityItemData Target { get; set; }
    }
}
