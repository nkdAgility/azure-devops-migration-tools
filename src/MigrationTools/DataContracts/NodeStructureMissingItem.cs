using System;
using System.Collections.Generic;

namespace MigrationTools.DataContracts
{
    public class NodeStructureItem
    {
        public bool anchored { get; set; } = true;

        public string sourcePath { get; set; }
        public string targetPath { get; set; }
        public string systemPath { get; set; }
        public string nodeType { get; set; }

        public DateTime? startDate { get; set; }
        public DateTime? finishDate { get; set; }

        public List<int> workItems { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as NodeStructureItem;

            if (item == null)
            {
                return false;
            }

            return this.sourcePath.Equals(item.sourcePath);
        }

        public override int GetHashCode()
        {
            return this.sourcePath.GetHashCode();
        }

    }
}