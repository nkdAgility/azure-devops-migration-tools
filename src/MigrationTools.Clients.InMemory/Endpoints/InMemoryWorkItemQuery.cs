//using System;
//using System.Collections.Generic;
//using MigrationTools.Clients;
//using MigrationTools.DataContracts;

//namespace MigrationTools.Endpoints
//{
//    public class InMemoryWorkItemQuery : IWorkItemQuery
//    {
//        private int _query;

//        public string Query => _query.ToString();

//        public void Configure(IMigrationClient migrationClient, string query, Dictionary<string, string> parameters)
//        {
//            if (string.IsNullOrEmpty(query))
//            {
//                throw new ArgumentException($"'{nameof(query)}' cannot be null or empty", nameof(query));
//            }
//            if (!int.TryParse(query, out _query))
//            {
//                throw new ArgumentException($"'{nameof(query)}' must be an integer", nameof(query));
//            }
//        }

//        public List<WorkItemData> GetWorkItems()
//        {
//            throw new InvalidOperationException();
//        }

//        public List<WorkItemData2> GetWorkItems2()
//        {
//        }
//    }
//}