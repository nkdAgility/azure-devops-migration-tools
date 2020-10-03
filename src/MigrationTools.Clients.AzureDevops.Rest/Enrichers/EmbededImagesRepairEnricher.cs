using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationTools.Configuration.Processing;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Clients.AzureDevops.Rest.Enrichers
{
    public class EmbededImagesRepairEnricher : EmbededImagesRepairEnricherBase
    {

        public EmbededImagesRepairEnricher(IMigrationEngine engine, ILogger<EmbededImagesRepairEnricher> logger) :base(engine, logger)
        {

        }
        public override void Configure(bool save = true, bool filter = true)
        {
            throw new NotImplementedException();
        }

        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new NotImplementedException();
        }

        protected override void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "")
        {
            throw new NotImplementedException();
        }
    }
}
