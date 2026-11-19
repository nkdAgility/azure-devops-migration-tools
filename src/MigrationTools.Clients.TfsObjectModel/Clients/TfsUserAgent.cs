using System.Linq;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools.Services;

namespace MigrationTools.Clients
{
    /// <summary>
    /// Every TfsTeamProjectCollection and VssConnection copies VssClientHttpRequestSettings.Default when it is
    /// created, so this has to run before the first connection is opened.
    /// </summary>
    public static class TfsUserAgent
    {
        private static readonly object Sync = new object();

        public static void Install()
        {
            lock (Sync)
            {
                var userAgent = VssClientHttpRequestSettings.Default.UserAgent;
                userAgent.RemoveAll(p => p.Product?.Name == MigrationUserAgent.ProductName
                    || (p.Comment != null && p.Comment.Contains(MigrationUserAgent.InfoUrl)));
                userAgent.InsertRange(0, MigrationUserAgent.ProductValues);
            }
        }

        public static string Current => string.Join(" ", VssClientHttpRequestSettings.Default.UserAgent.Select(p => p.ToString()));
    }
}
