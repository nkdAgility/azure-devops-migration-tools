using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MigrationTools
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguredService(this IServiceCollection collection, IConfiguration config)
        {
            //if (collection == null) throw new ArgumentNullException(nameof(collection));
            //if (config == null) throw new ArgumentNullException(nameof(config));
            //collection.Configure
            //collection.Configure<GreetingServiceOptions>(config);
            //return collection.AddTransient<IGreetingService, GreetingService>();
            return collection;
        }

        
    }
}
//https://www.youtube.com/watch?v=kLl2Mt3eYxU
//https://csharp.christiannagel.com/2016/08/16/diwithconfiguration/
