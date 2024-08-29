using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Options
{
    public class ConfigurationMetadata
    {
        public string OptionFor { get; set; }
        public string PathToInstance { get; set; }
        public bool IsCollection { get; set; } = false;
        public string PathToDefault { get; set; }
        public string PathToSample { get; set; }
        public string ObjectName { get; set; }
        public bool IsKeyed { get; set; } = false;
    }
}
