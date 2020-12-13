using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationTools.Host.CommandLine;

namespace MigrationTools.Host
{
    public static class HostExtensions
    {
        public static IStartupService InitializeMigrationSetup(this IHost host, string[] args)
        {
            var startup = host.Services.GetRequiredService<IStartupService>();
            startup.RunStartupLogic(args);
            return startup;
        }
    }
}