using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools.CommandLine
{
    public class OptionsBase
    {
        [Option('c', "collection", Required = true, HelpText = "Collection that you want to connect to.")]
        public Uri CollectionURL { get; set; }
    }
}
