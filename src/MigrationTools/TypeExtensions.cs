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
            return types.SingleOrDefault(type => type.Name.StartsWith(search, StringComparison.InvariantCultureIgnoreCase));
        }

        public static Type FromOptions(this IEnumerable<Type> types, IOptions option)
        {
            return types.SingleOrDefault(type => type.Name.StartsWith(option.GetType().Name.Replace("Options", "")));
        }


    }
}
