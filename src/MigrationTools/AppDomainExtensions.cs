using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MigrationTools
{
    public static partial class AppDomainExtensions
    {
        public static IEnumerable<Type> GetMigrationToolsTypes(this AppDomain appDomain)
        {
            var assemblies = appDomain.GetAssemblies().Where(ass => (ass.FullName.StartsWith("MigrationTools") || ass.FullName.StartsWith("VstsSyncMigrator")));
            return assemblies
                        .SelectMany(GetLoadableTypes);
        }

        private static IEnumerable<Type> GetLoadableTypes(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                foreach (var loaderException in ex.LoaderExceptions.Where(e => e != null).Select(e => e.Message).Distinct())
                {
                    Serilog.Log.Warning("Could not load some types from {AssemblyName}: {LoaderException}", assembly.GetName().Name, loaderException);
                }
                return ex.Types.Where(t => t != null);
            }
        }
    }
}
