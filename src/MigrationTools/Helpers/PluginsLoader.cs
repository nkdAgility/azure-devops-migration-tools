using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.Processors;
using Newtonsoft.Json;

namespace MigrationTools.Helpers
{
    public interface IPluginProcessor : IProcessor { }
    public class PluginsConfig
    {
        [JsonProperty("Plugins")]
        public List<string> Plugins { get; set; }

    }
    public class PluginsLoader : MarshalByRefObject
    {
        public bool Loaded { get; set; }
        public List<Assembly> Assemblies { get; set; }

        private ILogger<PluginsLoader> Log;
        public PluginsLoader(ILogger<PluginsLoader> logger)
        {
            Log = logger;
            Assemblies = new List<Assembly>();
        }
        public void Load(string configFile)
        {
            if (!Loaded)
            {
                try
                {
                    var config = JsonConvert.DeserializeObject<PluginsConfig>(System.IO.File.ReadAllText(configFile));

                    if (config.Plugins != null && config.Plugins.Count > 0)
                    {
                        foreach (var plugin in config.Plugins)
                        {
                            try
                            {
                                LoadAssemblyFrom(System.IO.Path.GetFullPath(plugin));
                            }
                            catch (Exception ex)
                            {
                                Log.LogWarning($"Plugin at path [{plugin}] failed to load. {ex.Message}");
                            }
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to load plugins, further processing may fail. {ex.Message}");
                }
            }
        }
        private void LoadAssembly(string path)
        {
            ValidatePath(path);

            Assemblies.Add(Assembly.Load(path));
        }

        private void LoadAssemblyFrom(string path)
        {
            ValidatePath(path);

            Assemblies.Add(Assembly.LoadFrom(path));
        }

        private void ValidatePath(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (!System.IO.File.Exists(path))
                throw new ArgumentException(String.Format("path \"{0}\" does not exist", path));
        }
    }
    public class PluginConstructorAttribute : Attribute { }
}
