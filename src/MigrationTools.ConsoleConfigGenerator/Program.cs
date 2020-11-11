using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MigrationTools.Options;
using MigrationTools.Tests;

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program
    {
        public static AppDomain domain = AppDomain.CreateDomain("MigrationTools");

        public static async Task Main(string[] args)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;

            domain.Load("MigrationTools");
            domain.Load("MigrationTools.Clients.AzureDevops.ObjectModel");
            domain.Load("MigrationTools.Clients.FileSystem");
            //AppDomain.CurrentDomain.Load("MigrationTools.Clients.AzureDevops.Rest");

            List<Type> types = domain.GetAssemblies()
                  .Where(a => !a.IsDynamic && a.FullName.StartsWith("MigrationTools"))
                  .SelectMany(a => a.GetTypes()).ToList();

            var endpointOptions = types.Where(t => typeof(IOptions).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToList();

            foreach (var item in endpointOptions)
            {
                var instance = (IOptions)Activator.CreateInstance(item);
                instance.SetDefaults();
                TestHelpers.SaveDocsObjectAsJSON(instance);
            }
        }
    }
}