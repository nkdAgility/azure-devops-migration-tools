using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Core.DataContracts
{
    public class RevisionItem
    {
        public int Index { get; set; }
        public int Number { get; set; }
        public DateTime ChangedDate { get; set; }
    }
}
