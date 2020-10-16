using System;
using System.Collections.Generic;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients.InMemory.Endpoints
{
    public class WorkItemQuery : IWorkItemQuery
    {
        private int _query;

        public void Configure(IMigrationClient migrationClient, string query, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentException($"'{nameof(query)}' cannot be null or empty", nameof(query));
            }
            if (!int.TryParse(query, out _query))
            {
                throw new ArgumentException($"'{nameof(query)}' must be an integer", nameof(query));
            }
        }

        public List<WorkItemData> GetWorkItems()
        {
            var list = new List<WorkItemData>();
            for (int i = 0; i < _query; i++)
            {
                list.Add(new WorkItemData()
                {
                    Id = i.ToString(),
                    Title = string.Format("Title {0}", i),
                    Rev = 1,
                    Revision = 1,
                    Revisions = GetRevisions()
                });
            }

            List<RevisionItem> GetRevisions()
            {
                Random rand = new Random();
                int revCount = rand.Next(0, 5);
                List<RevisionItem> list = new List<RevisionItem>();
                for (int i = 0; i < revCount; i++)
                {
                    list.Add(new RevisionItem { Index = i, Number = i, ChangedDate = DateTime.Now.AddHours(-i) });
                }
                return list;
            }
            return list;
        }
    }
}