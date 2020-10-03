namespace VstsSyncMigrator
{
    internal class UserModeConstants
    {
        public const string ClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
        public const string AuthString = GlobalConstants.AuthString + "common/";
    }

    internal class GlobalConstants
    {
        public const string AuthString = "https://login.microsoftonline.com/";
        public const string ResourceUrl = "https://graph.windows.net";
        public const string GraphServiceObjectId = "00000002-0000-0000-c000-000000000000";
    }
}
