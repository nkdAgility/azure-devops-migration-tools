using System;
using System.Collections.Generic;
using MigrationTools.DataContracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using MigrationTools;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools._EngineV1.Enrichers;
using MigrationTools._EngineV1.Processors;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;
using Serilog.Context;
using Serilog.Events;
using ILogger = Serilog.ILogger;
namespace VstsSyncMigrator.Engine
{
    internal class MigrateInlineLinks
    {
        internal static void MigrateFor(WorkItemData[] workItemDatas)
        {
            foreach(var wi in workItemDatas)
            {
                wi.Title = wi.Title + "1";
                wi.SaveToAzureDevOps();
                wi.ToWorkItem().Close();
            }

        }
    }
}