﻿//New COmment
using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools._EngineV1.Configuration.FieldMap;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class FieldSkipMap : FieldMapBase
    {
        private FieldSkipMapConfig Config { get { return (FieldSkipMapConfig)_Config; } }

        public override string MappingDisplayName => $"{Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
        }
    }
}