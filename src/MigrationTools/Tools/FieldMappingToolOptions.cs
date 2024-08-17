﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Tools.Infra;

namespace MigrationTools.Tools
{
    public class FieldMappingToolOptions : ToolOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:FieldMappingTool";

        public List<IFieldMapConfig> FieldMaps { get; set; } = new List<IFieldMapConfig>();


        public class ConfigureOptions : IConfigureOptions<FieldMappingToolOptions>
        {
            private readonly IConfiguration _configuration;

            public ConfigureOptions(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void Configure(FieldMappingToolOptions options)
            {
                switch (VersionOptions.ConfigureOptions.GetMigrationConfigVersion(_configuration).schema)
                {
                    case MigrationConfigSchema.v160:
                        _configuration.GetSection(ConfigurationSectionName).Bind(options);
                        options.FieldMaps = _configuration.GetSection(ConfigurationSectionName + ":FieldMaps")?.ToMigrationToolsList(child => child.GetMigrationToolsOption<IFieldMapConfig>("FieldMapType"));
                        break;
                    case MigrationConfigSchema.v1:
                        options.Enabled = true;
                        options.FieldMaps = _configuration.GetSection("FieldMaps")?.ToMigrationToolsList(child => child.GetMigrationToolsOption<IFieldMapConfig>("$type"));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                        break;
                }
            }
        }

    }
}