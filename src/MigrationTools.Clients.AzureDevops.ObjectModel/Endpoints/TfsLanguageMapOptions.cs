using System;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public class TfsLanguageMapOptions : IOptions
    {
        public string AreaPath { get; set; }
        public string IterationPath { get; set; }
        public string RefName { get; set; }

        public Type ToConfigure => null;

        public void SetDefaults()
        {
            AreaPath = "Area";
            IterationPath = "Iteration";
        }
    }
}