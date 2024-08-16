using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace MigrationTools.Options
{

    public interface IWritableOptions<out T> : IOptionsSnapshot<T> where T : class, new()
    {
        void Update(Action<T> applyChanges);
    }

    public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
    {
        private readonly IOptionsMonitor<T> _options;
        private readonly string _section;
        private readonly string _filePath;

        public WritableOptions(
            IOptionsMonitor<T> options,
            string section,
            string filePath = "configuration.json")
        {
            _options = options;
            _section = section;
            _filePath = filePath;
        }

        public T Value => _options.CurrentValue;
        public T Get(string name) => _options.Get(name);

        public void Update(Action<T> applyChanges)
        {
            var jObject = JObject.Parse(File.ReadAllText(_filePath));

            var sectionToken = jObject.SelectToken(_section.Replace(":", "."));
            T sectionObject;
            if (sectionToken != null)
            {
                sectionObject = sectionToken.ToObject<T>();
            }
            else
            {
                // If the section doesn't exist, create a new instance of T
                sectionObject = new T();
            }


            applyChanges(sectionObject);

            jObject[_section] = JObject.FromObject(sectionObject);
            File.WriteAllText(_filePath, jObject.ToString());
        }
    }
}