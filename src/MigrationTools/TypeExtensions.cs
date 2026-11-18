using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MigrationTools.Options;

namespace MigrationTools
{
    public static partial class TypeExtensions
    {
        public static IEnumerable<Type> WithInterface<TInterface>(this IEnumerable<Type> types)
        {
            return types.Where(type => typeof(TInterface).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
        }

        public static IEnumerable<Type> WithInterface(this IEnumerable<Type> types, Type interfaceType)
        {
            return types.Where(t => t.GetInterfaces().Any(i => i == interfaceType));
        }

        public static IEnumerable<Type> WithInterface<TInterface, TInterface2>(this IEnumerable<Type> types)
        {
            return types.Where(type => (typeof(TInterface).IsAssignableFrom(type) || typeof(TInterface2).IsAssignableFrom(type)) && !type.IsInterface && !type.IsAbstract);
        }

        public static IEnumerable<Type> WithConfigurationSectionName(this IEnumerable<Type> types)
        {
            return types.Where(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Any(field => field.IsLiteral && !field.IsInitOnly && field.Name == "ConfigurationSectionName"));
        }

        public static Type WithNameString(this IEnumerable<Type> types, string search)
        {
            return SingleByName(types, search, StringComparison.InvariantCultureIgnoreCase);
        }

        public static Type FromOptions(this IEnumerable<Type> types, IOptions option)
        {
            return SingleByName(types, option.GetType().Name.Replace("Options", ""), StringComparison.Ordinal);
        }

        /// <summary>
        /// An exact name match wins over a prefix match. The same type loaded twice (for example from an 8.3 short path and its
        /// long path) is counted once, by assembly-qualified name. Different types that still match ambiguously throw.
        /// </summary>
        private static Type SingleByName(IEnumerable<Type> types, string name, StringComparison comparison)
        {
            var candidates = types.Where(type => type.Name.StartsWith(name, comparison))
                .GroupBy(type => type.AssemblyQualifiedName)
                .Select(group => group.First())
                .ToList();
            var exact = candidates.Where(type => string.Equals(type.Name, name, comparison)).ToList();
            return (exact.Count > 0 ? exact : candidates).SingleOrDefault();
        }


    }
}
