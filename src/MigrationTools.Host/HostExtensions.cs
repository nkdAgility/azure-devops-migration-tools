using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationTools.CommandLine;

namespace MigrationTools.Host
{
    public static class HostExtensions
    {
        public static IStartupService InitializeMigrationSetup(this IHost host, string[] args)
        {
            var initOptions = host.Services.GetService<InitOptions>();
            var executeOptions = host.Services.GetService<ExecuteOptions>();
            if (initOptions == null && executeOptions == null)
            {
                return null;
            }
            var startup = host.Services.GetRequiredService<IStartupService>();
            startup.RunStartupLogic(args);
            return startup;
        }
    }
}