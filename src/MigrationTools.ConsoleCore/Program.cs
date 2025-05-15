using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MigrationTools.Host;

namespace MigrationTools.ConsoleCore
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = MigrationToolHost.CreateDefaultBuilder(args);
            if (hostBuilder is null)
            {
                return;
            }

            hostBuilder
                .ConfigureServices((context, services) =>
                {
                    // Field Mapps


                    // Processors

                    // Core
                    // services.AddTransient<IMigrationClient, MigrationRestClient>();
                });
            await hostBuilder.RunConsoleAsync();
        }
    }
}
