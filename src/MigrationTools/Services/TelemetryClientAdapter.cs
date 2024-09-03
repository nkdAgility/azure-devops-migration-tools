using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Elmah.Io.Client;
using MigrationTools.Services;

namespace MigrationTools
{
    public class TelemetryClientAdapter : ITelemetryLogger
    {
        private static IElmahioAPI elmahIoClient;
        private static IMigrationToolVersion _MigrationToolVersion;

        public TelemetryClientAdapter(IMigrationToolVersion migrationToolVersion)
        {
            _MigrationToolVersion = migrationToolVersion;
            elmahIoClient = ElmahioAPI.Create("7589821e832a4ae1a1170f8201def634", new ElmahIoOptions
            {
                Timeout = TimeSpan.FromSeconds(30),
                UserAgent = "Azure-DevOps-Migration-Tools",
            });
            elmahIoClient.Messages.OnMessage += (sender, args) => args.Message.Version = migrationToolVersion.GetRunningVersion().versionString;

        }

        private static string _sessionid = Guid.NewGuid().ToString();

        public string SessionId => _sessionid;

        public void TrackException(Exception ex, IDictionary<string, string> properties)
        {
            var baseException = ex.GetBaseException();
            var createMessage = new CreateMessage
            {
                DateTime = DateTime.UtcNow,
                Detail = ex.ToString(),
                Type = baseException.GetType().FullName,
                Title = baseException.Message ?? "An error occurred",
                Severity = "Error",
                Data = new List<Item>(),
                Source = baseException.Source,
                User = Environment.UserName,
                Hostname = System.Environment.GetEnvironmentVariable("COMPUTERNAME"),
                Application = "Azure-DevOps-Migration-Tools",
                ServerVariables = new List<Item>
                    {
                        new Item("User-Agent", $"X-ELMAHIO-APPLICATION; OS={Environment.OSVersion.Platform}; OSVERSION={Environment.OSVersion.Version}; ENGINEVERSION={_MigrationToolVersion.GetRunningVersion().versionString}; ENGINE=Azure-DevOps-Migration-Tools"),
                    }
            };
            createMessage.Data.Add(new Item("SessionId", SessionId));
            createMessage.Data.Add(new Item("Version", _MigrationToolVersion.GetRunningVersion().versionString));

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    createMessage.Data.Add(new Item(property.Key, property.Value));
                }

            }
           var result = elmahIoClient.Messages.CreateAndNotify(new Guid("24086b6d-4f58-47f4-8ac7-68d8bc05ca9e"), createMessage);
            Console.WriteLine($"Error logged to Elmah.io");
        }

        public void TrackException(Exception ex, IEnumerable<KeyValuePair<string, string>> properties = null)
        {
            if (properties == null)
            {
                TrackException(ex, null);
                return;
            }
            TrackException(ex, properties.ToDictionary(k => k.Key, v => v.Value));
        }

    }
}