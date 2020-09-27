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
using MigrationTools.Core.Configuration.Processing;
using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Engine.Enrichers;
using MigrationTools.Core.Enrichers;

namespace MigrationTools.Sinks.AzureDevOps.Enrichers
{
    public class EmbededImagesRepairEnricher : EmbededImagesRepairEnricherBase
    {

        public override void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "")
        {
            throw new NotImplementedException();
        }

    }
}
