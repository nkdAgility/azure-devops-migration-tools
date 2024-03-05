using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Commerce;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;

namespace MigrationTools.ProcessorEnrichers
{
    public class TfsUserMappingEnricher : WorkItemProcessorEnricher
    {

        private readonly IMigrationEngine Engine;

        public TfsUserMappingEnricher(IServiceProvider services, ILogger<TfsUserMappingEnricher> logger) : base(services, logger)
        {
            Engine = services.GetRequiredService<IMigrationEngine>();

        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            throw new NotImplementedException();
        }

        protected override void EntryForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }

        protected override void RefreshForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new NotImplementedException();
        }

        //private List<string> findIdentityFieldsToCheck()
        //{

        //}

        private List<string> extractUserList(List<WorkItemData> workitems, List<string> identityFieldsToCheck)
        {
            List<string> foundUsers = new List<string>();
            foreach (var wItem in workitems)
            {
                foreach (var rItem in wItem.Revisions.Values)
                {
                    foreach (var fItem in rItem.Fields.Values)
                    {
                        if (identityFieldsToCheck.Contains(fItem.ReferenceName, new CaseInsensativeStringComparer()))
                        {
                            if (!foundUsers.Contains(fItem.Value))
                            {
                                foundUsers.Add(fItem.Value.ToString());
                            }
                        }
                    }
                }
            }
            return foundUsers;
        }

        public Dictionary<string, string> findUsersToMap(List<WorkItemData> sourceWorkItems, List<string> identityFieldsToCheck)
        {
            List<string> sourceUsers = extractUserList(sourceWorkItems, identityFieldsToCheck);
            return sourceUsers.ToDictionary(item => item, item => "");
        }
    }

    public class CaseInsensativeStringComparer : IEqualityComparer<string>
    {

        public bool Equals(string x, string y)
        {
            return x?.IndexOf(y, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}
