using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using MigrationTools.CommandLine;

namespace MigrationTools.Core.Configuration
{
    public class EngineConfigurationWrapper : EngineConfiguration
    {
        private readonly EngineConfiguration _engineConfiguration;

        public override string Version { get { return _engineConfiguration.Version; } set { _engineConfiguration.Version = value; } }
        public override bool TelemetryEnableTrace { get { return _engineConfiguration.TelemetryEnableTrace; } set { _engineConfiguration.TelemetryEnableTrace = value; } }
        public override bool workaroundForQuerySOAPBugEnabled { get { return _engineConfiguration.workaroundForQuerySOAPBugEnabled; } set { _engineConfiguration.workaroundForQuerySOAPBugEnabled = value; } }
        public override string ChangeSetMappingFile { get { return _engineConfiguration.ChangeSetMappingFile; } set { _engineConfiguration.ChangeSetMappingFile = value; } }
        public override TeamProjectConfig Source { get { return _engineConfiguration.Source; } set { _engineConfiguration.Source = value; } }
        public override TeamProjectConfig Target { get { return _engineConfiguration.Target; } set { _engineConfiguration.Target = value; } }
        public override List<IFieldMapConfig> FieldMaps { get { return _engineConfiguration.FieldMaps; } set { _engineConfiguration.FieldMaps = value; } }
        public override Dictionary<string, string> WorkItemTypeDefinition { get { return _engineConfiguration.WorkItemTypeDefinition; } set { _engineConfiguration.WorkItemTypeDefinition = value; } }
        public override Dictionary<string, string> GitRepoMapping { get { return _engineConfiguration.GitRepoMapping; } set { _engineConfiguration.GitRepoMapping = value; } }
        public override List<ITfsProcessingConfig> Processors { get { return _engineConfiguration.Processors; } set { _engineConfiguration.Processors = value; } }

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
                logger.LogInformation("The config file {ConfigFile} does not exist, nor does the default 'configuration.json'. Use '{ExecutableName}.exe init' to create a configuration file first", opts.ConfigFile, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                throw new ArgumentException("missing configfile");
            }
            logger.LogInformation("Config Found, creating engine host");
            _engineConfiguration = engineConfigurationBuilder.BuildFromFile(opts.ConfigFile);
        }
    }
    public class EngineConfiguration
    {
        public virtual string Version { get; set; }
        public virtual bool TelemetryEnableTrace { get; set; }
        public virtual bool workaroundForQuerySOAPBugEnabled { get; set; }
        public virtual string ChangeSetMappingFile { get; set; }
        public virtual TeamProjectConfig Source { get; set; }
        public virtual TeamProjectConfig Target { get; set; }
        public virtual List<IFieldMapConfig> FieldMaps { get; set; }
        public virtual Dictionary<string, string> WorkItemTypeDefinition { get; set; }
        public virtual Dictionary<string, string> GitRepoMapping { get; set; }
        public virtual List<ITfsProcessingConfig> Processors { get; set; }

    }
}
