using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.Clients;
using MigrationTools.DataContracts;
using Newtonsoft.Json;

namespace MigrationTools.Endpoints
{
    public class FileSystemWorkItemQuery : IWorkItemQuery
    {
        private string _query;

        public string Query { get => _query; }

        public void Configure(IMigrationClient migrationClient, string query, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentException($"'{nameof(query)}' cannot be null or empty", nameof(query));
            }
            _query = query;
            if (!System.IO.Directory.Exists(_query))
            {
                System.IO.Directory.CreateDirectory(_query);
            }
        }

        public List<int> GetWorkItemIds()
        {
            return GetWorkItems().Select(wi => int.Parse(wi.Id)).ToList();
        }

        public List<WorkItemData> GetWorkItems()
        {
            var workItems = new List<WorkItemData>();

            var workitemFiles = System.IO.Directory.GetFiles(_query);
            foreach (var item in workitemFiles)
            {
                var contents = System.IO.File.ReadAllText(item);
                var workItem = JsonConvert.DeserializeObject<WorkItemData>(contents);
                workItems.Add(workItem);
            }

            return workItems;
        }
    }
}