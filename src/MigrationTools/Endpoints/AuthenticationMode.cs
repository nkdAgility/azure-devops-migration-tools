namespace MigrationTools.Endpoints
{
    public enum AuthenticationMode
    {
        AccessToken = 0,
        Windows = 1,
        Prompt = 2,
        /// <summary>
        /// Microsoft Entra ID token from the ambient identity (Azure CLI login, managed identity,
        /// workload identity federation, environment service principal). No secret is stored in config.
        /// </summary>
        EntraId = 3
    }
}