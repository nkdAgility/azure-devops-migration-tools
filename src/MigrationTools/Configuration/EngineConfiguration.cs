using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using MigrationTools.CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;

namespace MigrationTools.Configuration
{
    public class EngineConfiguration
    {
        public virtual string ChangeSetMappingFile { get; set; }
        public virtual IMigrationClientConfig Source { get; set; }
        public virtual IMigrationClientConfig Target { get; set; }
        public virtual List<IFieldMapConfig> FieldMaps { get; set; }
        public virtual Dictionary<string, string> GitRepoMapping { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual LogEventLevel LogLevel { get; set; }

        public virtual List<IProcessorConfig> Processors { get; set; }
        public virtual string Version { get; set; }
        public virtual bool workaroundForQuerySOAPBugEnabled { get; set; }
        public virtual Dictionary<string, string> WorkItemTypeDefinition { get; set; }
    }

    public class EngineConfigurationWrapper : EngineConfiguration
    {
        private readonly EngineConfiguration _engineConfiguration;

        public EngineConfigurationWrapper(IEngineConfigurationBuilder engineConfigurationBuilder, ExecuteOptions opts, ILogger<EngineConfigurationWrapper> logger)
        {
            if (opts == null) //means that we are in init command and not execute
            {
                _engineConfiguration = engineConfigurationBuilder.BuildDefault();
                return;
            }
            if (opts.ConfigFile == string.Empty)
            {
                opts.ConfigFile = "configuration.json";
            }
            if (!File.Exists(opts.ConfigFile))
            {
                logger.LogInformation("The config file {ConfigFile} does not exist, nor does the default 'configuration.json'. Use '{ExecutableName}.exe init' to create a configuration file first", opts.ConfigFile, Assembly.GetEntryAssembly().GetName().Name);
                throw new ArgumentException("missing configfile");
            }
            logger.LogInformation("Config Found, creating engine host");
            _engineConfiguration = engineConfigurationBuilder.BuildFromFile(opts.ConfigFile);
        }

        public override string ChangeSetMappingFile { get { return _engineConfiguration.ChangeSetMappingFile; } set { _engineConfiguration.ChangeSetMappingFile = value; } }
        public override IMigrationClientConfig Source { get { return _engineConfiguration.Source; } set { _engineConfiguration.Source = value; } }
        public override IMigrationClientConfig Target { get { return _engineConfiguration.Target; } set { _engineConfiguration.Target = value; } }
        public override List<IFieldMapConfig> FieldMaps { get { return _engineConfiguration.FieldMaps; } set { _engineConfiguration.FieldMaps = value; } }
        public override Dictionary<string, string> GitRepoMapping { get { return _engineConfiguration.GitRepoMapping; } set { _engineConfiguration.GitRepoMapping = value; } }
        public override LogEventLevel LogLevel { get { return _engineConfiguration.LogLevel; } set { _engineConfiguration.LogLevel = value; } }
        public override List<IProcessorConfig> Processors { get { return _engineConfiguration.Processors; } set { _engineConfiguration.Processors = value; } }
        public override string Version { get { return _engineConfiguration.Version; } set { _engineConfiguration.Version = value; } }
        public override bool workaroundForQuerySOAPBugEnabled { get { return _engineConfiguration.workaroundForQuerySOAPBugEnabled; } set { _engineConfiguration.workaroundForQuerySOAPBugEnabled = value; } }
        public override Dictionary<string, string> WorkItemTypeDefinition { get { return _engineConfiguration.WorkItemTypeDefinition; } set { _engineConfiguration.WorkItemTypeDefinition = value; } }
    }
}