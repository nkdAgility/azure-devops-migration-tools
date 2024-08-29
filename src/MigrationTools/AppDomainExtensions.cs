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
                        .SelectMany(assembly => assembly.GetTypes());
        }
    }
}
