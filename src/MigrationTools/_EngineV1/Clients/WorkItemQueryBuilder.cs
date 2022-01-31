using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;

namespace MigrationTools._EngineV1.Clients
{
    public class WorkItemQueryBuilder : IWorkItemQueryBuilder
    {
        internal Dictionary<string, string> _parameters;
        internal string _Query;
        private readonly IServiceProvider _Services;

        public string Query { get => _Query; set => _Query = value; }

        public WorkItemQueryBuilder(IServiceProvider services)
        {
            _parameters = new Dictionary<string, string>();
            _Services = services;
        }

        public void AddParameter(string name, string value)
        {
            if (_parameters.ContainsKey(name))
            {
                throw new ArgumentException("The parameter key you are trying to add to the query already exists.");
            }
            _parameters.Add(name, value);
        }

        public IWorkItemQuery BuildWIQLQuery(IMigrationClient migrationClient)
        {
            if (string.IsNullOrEmpty(Query))
            {
                throw new Exception("You must specify a Query");
            }
            Query = WorkAroundForSOAPError(Query); // TODO: Remove this once bug fixed... https://dev.azure.com/nkdagility/migration-tools/_workitems/edit/5066

            IWorkItemQuery wiq = _Services.GetRequiredService<IWorkItemQuery>();
            wiq.Configure(migrationClient, _Query, _parameters);
            return wiq;
        }

        // Fix for Query SOAP error when passing parameters
        [Obsolete("Temporary work aorund for SOAP issue https://dev.azure.com/nkdagility/migration-tools/_workitems/edit/5066")]
        private string WorkAroundForSOAPError(string query)
        {
            foreach (var parameter in _parameters)
            {
                if (!int.TryParse(parameter.Value, out _))
                {
                    // only replace with pattern when not part of a larger string like an area path which is already quoted.
                    query = Regex.Replace(query, $@"(?<=[=\s])@{parameter.Key}(?=$|\s)", $"'{parameter.Value}'");
                }

                // replace the other occurences of this key
                query = query.Replace(string.Format($"@{parameter.Key}"), parameter.Value);
            }
            return query;
        }
    }
}