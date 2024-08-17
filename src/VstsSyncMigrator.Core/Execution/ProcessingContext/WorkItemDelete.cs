using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;
using VstsSyncMigrator._EngineV1.Processors;
using VstsSyncMigrator.Core.Execution;
using Microsoft.Extensions.Options;

namespace VstsSyncMigrator.Engine
{
    /// <summary>
    /// The `WorkItemDelete` processor allows you to delete any amount of work items that meet the query.
    /// **DANGER:** This is not a recoverable action and should be use with extream caution.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>WorkItem</processingtarget>
    public class WorkItemDelete : TfsStaticProcessorBase
    {
        private WorkItemDeleteConfig _config;

        public WorkItemDelete(IOptions<WorkItemDeleteConfig> options, TfsStaticEnrichers tfsStaticEnrichers, StaticEnrichers staticEnrichers, IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<StaticProcessorBase> logger) : base(tfsStaticEnrichers, staticEnrichers, services, me, telemetry, logger)
        {
            _config = options.Value;
        }

        public override string Name
        {
            get
            {
                return "WorkItemDelete";
            }
        }


        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            var workItems = Engine.Target.WorkItems.GetWorkItemIds(_config.WIQLQuery);

            if (workItems.Count > 0)
            {
                Log.LogInformation("We are going to delete {sourceWorkItemsCount} work items?", workItems.Count);

                Console.WriteLine("Enter the number of work Items that we will be deleting! Then hit Enter e.g. 21");
                string result = Console.ReadLine();
                if (int.Parse(result) != workItems.Count)
                {
                    Log.LogWarning("USER ABORTED by selecting a number other than {sourceWorkItemsCount}", workItems.Count);
                    return;
                }

                //////////////////////////////////////////////////
                var counter = 0;
                var totalCount = workItems.Count;
                var lastProgressUpdate = DateTime.Now;
                var store = ((TfsWorkItemMigrationClient)Engine.Target.WorkItems).Store;
                var chunks = workItems.Chunk(10);
                foreach (var begone in chunks)
                {
                    if ((DateTime.Now - lastProgressUpdate).TotalSeconds > 30)
                    {
                        Log.LogInformation("Delete Progress {0}/{1} {2}", counter, totalCount, (1.0 * counter / totalCount).ToString("#0.##%", CultureInfo.InvariantCulture));
                        lastProgressUpdate = DateTime.Now;
                    }
                    var deleted = begone.ToList();
                    var err = store.DestroyWorkItems(begone);
                    if (err.Count > 0)
                    {
                        foreach (var e in err)
                        {
                            deleted.Remove(e.Id);
                            Log.LogWarning("Delete Failed: {0}", e.Exception.ToString());
                        }
                    }
                    var deletedIds = string.Join(", ", deleted);
                    Log.LogDebug("Deleted {0}", deletedIds);
                    counter += begone.Count();
                }
            }
            else
            {
                Log.LogInformation("Nothing to delete");
            }

            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }


    }

    public static class ListExtensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }
    }
}