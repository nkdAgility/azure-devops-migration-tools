using System;

namespace MigrationTools._Enginev1.DataContracts
{
    public class RevisionItem
    {
        public int Index { get; set; }
        public int Number { get; set; }
        public DateTime ChangedDate { get; set; }
    }
}