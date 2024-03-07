using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.DataContracts
{
    public class IdentityItemData
    {
        public string FriendlyName { get; set; }
        public string AccountName { get; set; }
    }

    public class IdentityMapData
    {
        public IdentityItemData Source { get; set; }
        public IdentityItemData target { get; set; }
    }
}
