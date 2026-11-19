using System.Linq;
using System.Net.Http.Headers;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools.Services;

namespace MigrationTools.Clients
{
    /// <summary>
    /// The object model has two transports, each with its own user agent: the REST clients copy
    /// VssClientHttpRequestSettings.Default when a connection is created, and the SOAP services (work item store,
    /// attachments, version control) send TfsConnection.ApplicationName. Both are set here, before the first
    /// connection is opened.
    /// The SOAP value replaces the library's "Team Foundation (devopsmigration.exe, ...)" rather than going in front
    /// of it: the organisation's Usage page names SOAP traffic after that part when it is present, so keeping it
    /// would hide the tool's name there.
    /// </summary>
    public static class TfsUserAgent
    {
        private static readonly object Sync = new object();

        public static void Install()
        {
            lock (Sync)
            {
                var rest = VssClientHttpRequestSettings.Default.UserAgent;
                rest.RemoveAll(IsOurs);
                rest.InsertRange(0, MigrationUserAgent.ProductValues);

                TfsConnection.ApplicationName = MigrationUserAgent.Value;
            }
        }

        public static string Rest => string.Join(" ", VssClientHttpRequestSettings.Default.UserAgent.Select(p => p.ToString()));

        public static string Soap => TfsConnection.ApplicationName;

        private static bool IsOurs(ProductInfoHeaderValue p) =>
            p.Product?.Name == MigrationUserAgent.ProductName || (p.Comment != null && p.Comment.Contains(MigrationUserAgent.InfoUrl));
    }
}
