using System;

namespace MigrationTools.DataContracts
{
    public class ApiPathAttribute : Attribute
    {
        public ApiPathAttribute(string Path)
        {
            this.Path = Path;
        }

        public string Path { get; }
    }

    public class ApiNameAttribute : Attribute
    {
        public ApiNameAttribute(string Name)
        {
            this.Name = Name;
        }

        public string Name { get; }
    }
}