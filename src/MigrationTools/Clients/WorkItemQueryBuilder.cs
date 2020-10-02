using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace MigrationTools.Clients
{
    public class WorkItemQueryBuilder : IWorkItemQueryBuilder
    {

        internal Dictionary<string, string> _Parameters;
        internal string _Query;
        private readonly IServiceProvider _Services;

        public Dictionary<string, string> Parameters => _Parameters;


        public string Query { get => _Query; set => _Query = value; }

        public WorkItemQueryBuilder(IServiceProvider services)
        {
            _Parameters = new Dictionary<string, string>();
            _Services = services;
        }

        public void AddParameter(string name, string value)
        {
            if (!Parameters.ContainsKey(name))
            {
                Parameters.Add(name, value);
            } else
            {
                throw new Exception("You cant add the same key twice to the query builder properties.");
            }           
        }

        public IWorkItemQuery BuildWIQLQuery(IMigrationClient migrationClient)
        {
            if (string.IsNullOrEmpty(Query))
            {
                throw new Exception("You must specify a Query");
            }
            Query = WorkAroundForSOAPError(Query, Parameters); // TODO: Remove this once bug fixed... https://dev.azure.com/nkdagility/migration-tools/_workitems/edit/5066

            IWorkItemQuery wiq =_Services.GetRequiredService<IWorkItemQuery>();
            wiq.Configure(migrationClient, _Query, _Parameters);
            return wiq;
        }

        // Fix for Query SOAP error when passing parameters
        [Obsolete("Temporary work aorund for SOAP issue https://dev.azure.com/nkdagility/migration-tools/_workitems/edit/5066")]
        private string WorkAroundForSOAPError(string query, IDictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (!IsInteger(parameter.Value))
                {
                    // only replace with pattern when not part of a larger string like an area path which is already quoted.
                    query = Regex.Replace(query, $@"(?<=[=\s])@{parameter.Key}(?=$|\s)", $"'{parameter.Value}'");
                }

                // replace the other occurences of this key
                query = query.Replace(string.Format($"@{parameter.Key}"), parameter.Value);
            }
            return query;
        }

        private bool IsInteger(string maybeInt)
        {
            //Check whether 'first' is integer
            return int.TryParse(maybeInt, out _);
        }
    }
}
