using System;
using System.Collections.Generic;
using MigrationTools.Clients;
using MigrationTools.DataContracts;

namespace MigrationTools.Endpoints
{
    public class InMemoryWorkItemQuery : IWorkItemQuery
    {
        private int _query;

        public string Query => _query.ToString();

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
            throw new InvalidOperationException();
        }

        public List<WorkItemData2> GetWorkItems2()
        {
            var list = new List<WorkItemData2>();
            for (int i = 0; i < _query; i++)
            {
                list.Add(new WorkItemData2()
                {
                    Id = i.ToString(),
                    Revisions = GetRevisions()
                });
            }

            List<RevisionItem2> GetRevisions()
            {
                Random rand = new Random();
                int revCount = rand.Next(0, 5);
                List<RevisionItem2> list = new List<RevisionItem2>();
                for (int i = 0; i < revCount; i++)
                {
                    list.Add(new RevisionItem2 { Index = i, Number = i, ChangedDate = DateTime.Now.AddHours(-i) });
                }
                return list;
            }
            return list;
        }
    }
}